//
//  Author:
//    Charles Yust charles.yust@frogdesign.com
//
//  Copyright (c) 2016, frog design
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of frog design nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerControllerDodge : MonoBehaviour {

	// Player Movement
	public Material matLevel2;
	public float speed;
	public float tiltSensitivity = 5.0f;
	public float altitude = -20.0f;
	public float dAltitude = 5.0f;
	public GameObject explosion;

	// Player Positioning and Rotation
	public float playerGuideRotation;
	public Vector3 lookAtTarget;
	public bool bIncDistance;

	// Audio
	public AudioClip impact;

	// Player Sky Dome
	public GameObject skyDome;

	public Text textFPS;

	private const float ROTATION_ADJUST = 30.0f;
	private float altitudeTemp;
	private float lastAlt;
	private float initAlt = 0.0f;
	private GameObject explosionGO;
	private Stalag explosionCode;
	private bool moving = false;
	private bool gameOver = false;
	private float gameOverVel = 0.0f;
	private float gameOverDecTime = 4.0f;
	private float gameOverTime; 
	private float rotVal;
	private Material matLevel1;
	
	// Player Positioning and Rotation
	private GameObject spaceShip;
	private Rigidbody rb;
	private RaycastHit hitHover;
	private bool readInitialAlt;
	private float distInc;
	private float totalDistance;
	private float distStart;
	private float distTotal;
	private int endOpenIndex;

	// Player Spaceship Cylindar Movement
	private GameObject cyl0, cyl1, cyl2;
	private Vector3 cylPos0, cylPos1, cylPos2;

	// Collision Variables
	private List<int> hitIDs;
	private bool paused;
	private bool bCollision;

	// Audio 
	private AudioSource impactSource;

	void Awake () {
		moving = false;
		endOpenIndex = 0;
		explosionGO = (GameObject) Instantiate (explosion);
		explosionGO.name = "StalagExplosion";
		explosionCode = explosionGO.GetComponent<Stalag> ();
		explosionCode.setShatters (true);
		explosionGO.SetActive (false);
		matLevel1 = explosionGO.GetComponent<MeshRenderer> ().material;
	}

	// Use this for initialization
	void Start () {
		rb = GetComponent <Rigidbody> ();
		lookAtTarget = new Vector3 ();
		spaceShip = GameObject.FindGameObjectWithTag ("ShipCircle");
		bIncDistance = true;
		playerGuideRotation = 180.0f;
		hitIDs = new List<int>();
		paused = false;
		bCollision = false;

		// Player Hover Calculation
		readInitialAlt = false;
		totalDistance = 0.0f;
		rotVal = 0f;
		moving = false;
		gameOver = false;
		gameOverTime = 0.0f;

		EventManager.StartListening ("startMoving", onStartMoving);
		EventManager.StartListening ("gameOver", onGameOver);
		EventManager.StartListening ("StalagCollision", onStalagCollision);
		EventManager.StartListening ("exitDodge", onExit);

		// Audio
		impactSource = GetComponent<AudioSource> ();
	}

	void onStartMoving() {
		moving = true;
	}

	void onGameOver() {
		gameOver = true;
		gameOverTime = Time.time;
		gameOverVel = rb.velocity.magnitude;
		bIncDistance = false;
	}

	void onExit() {
		moving = false;
		gameOver = true;
		EventManager.TriggerEvent ("exitDodgePlayer");
	}

	void Update() {
		if (moving) {
			// Calculating Distance Traveled
			if (bIncDistance) {
				totalDistance = (rb.position.z - (distStart * ((float) endOpenIndex + 1f))) * distInc;
				if (totalDistance < 0)
					totalDistance = 0;
			}
			// Explosion 
			if (explosionGO.activeInHierarchy) {
				explosionGO.transform.position = rb.transform.position;
				explosionGO.transform.localPosition = transform.forward * -1.0f * speed + rb.transform.localPosition; // new Vector3(rb.transform.localPosition.x, rb.transform.localPosition.y, rb.transform.localPosition.z + 80.0f);
			}
			skyDome.transform.position = new Vector3 (transform.position.x, 0.0f, transform.position.z);
		}

		if (moving) {
			rotVal = (PlayerInputSingleton.Instance.getZPerDeg () - .5f) * tiltSensitivity;
			if (GameGlobalVars.GlobalStrafe) {
				spaceShip.transform.rotation = Quaternion.AngleAxis (playerGuideRotation, Vector3.up);
			} else {
				spaceShip.transform.Rotate (Vector3.up, rotVal);
			}

			// textFPS.text = "getZPer0:" + PlayerInputSingleton.Instance.getZPer () + "\n" + "getZPer1:" + PlayerInputSingleton.Instance.getZPerDeg ();

			// STRAFE
			if (!gameOver && !GameGlobalVars.GlobalStrafe) {
				checkPlayerRotationLimit ();
			}
			rb.transform.rotation = spaceShip.transform.rotation;

			// Add velocity to rb
			if (!paused) {
				// STRAFE
				if (GameGlobalVars.GlobalStrafe) {
					rb.AddForce (transform.right * rotVal * -10.0f);
				}
				if (!gameOver) 
					rb.AddForce (transform.forward * speed * -1.0f); //, ForceMode.VelocityChange);
			}

			// Hover Test only
			LayerMask myLayerMask = 1 << LayerMask.NameToLayer ("caves");

			if (!readInitialAlt) {
				if (Physics.Raycast (rb.transform.position, -Vector3.up, out hitHover, 1000.0f, myLayerMask)) {
					readInitialAlt = true;
					initAlt = hitHover.distance;
					lastAlt = initAlt;
				}
			}

			altitudeTemp = altitude;
			// Hover code
			if (Physics.Raycast (rb.transform.position, -Vector3.up, out hitHover, 1000.0f, myLayerMask) && initAlt != 0.0f) {
				if (!paused)
					rb.AddForce (Vector3.up * altitudeTemp, ForceMode.VelocityChange);
				if (hitHover.distance < 500.0f) {
					float yVal = altitude;
					float dist = (hitHover.distance - initAlt);
					if (dist > dAltitude)
						yVal = altitude - dAltitude;
					else if (dist < dAltitude * -1.0)
						yVal = altitude + dAltitude;
					else
						yVal = altitude - dist;

					yVal = Mathf.Lerp (lastAlt, yVal, 0.1f);
					rb.transform.position = new Vector3 (rb.transform.position.x, yVal, rb.transform.position.z);
					lastAlt = yVal;
				}
			}

			// Speed Limit
			if (!gameOver) {
				if (rb.velocity.magnitude > speed) {
					rb.velocity = rb.velocity.normalized * speed;
				}
			} else {
				float perCovered = ((Time.time - gameOverTime) / (gameOverDecTime - 1.0f));
				if (perCovered >= 1.0f)
					perCovered = 1.0f; 
				gameOverVel = Mathf.Lerp (speed, 0.0f, perCovered);
				rb.velocity = rb.velocity.normalized * gameOverVel;
				rb.AddForce (transform.forward * gameOverVel * -1.0f);
			}

			// Transfer position to ship and spotlight
			spaceShip.transform.position = rb.transform.position;
		} else {
			//rb.velocity = new Vector3 (0.0f, 0.0f, 0.0f);
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero; 
		}
	}

	Vector3 getRelativePosition(Transform origin, Vector3 position) {
		Vector3 distance = position - origin.localPosition; // .position;
		Vector3 relativePosition = Vector3.zero;
		relativePosition.x = 0.0f;
		relativePosition.y = 0.0f;
		relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);
		return relativePosition;
	}

	void checkPlayerRotationLimit() {

		float upperLimit = playerGuideRotation + ROTATION_ADJUST;
		if (upperLimit > 360.0f) 
			upperLimit -= 360.0f;
		float lowerLimit = playerGuideRotation - ROTATION_ADJUST;
		if (lowerLimit <= 0.0f)
			lowerLimit += 360.0f;

		// Workarounds for dealing with 0/360.f rotation transition.
		if (playerGuideRotation < ROTATION_ADJUST || playerGuideRotation > 360.0f - ROTATION_ADJUST) {
			if (spaceShip.transform.rotation.eulerAngles.y > upperLimit && spaceShip.transform.rotation.eulerAngles.y <= ROTATION_ADJUST * 2) {
				spaceShip.transform.rotation = Quaternion.AngleAxis (upperLimit, Vector3.up);
			} else if (spaceShip.transform.rotation.eulerAngles.y < lowerLimit && spaceShip.transform.rotation.eulerAngles.y >= 360.0f - ROTATION_ADJUST * 2) {
				spaceShip.transform.rotation = Quaternion.AngleAxis (lowerLimit, Vector3.up);
			}
		} else { // Regular, Transfer rotation from ship/camera
			if (spaceShip.transform.rotation.eulerAngles.y > upperLimit) {
				spaceShip.transform.rotation = Quaternion.AngleAxis (upperLimit, Vector3.up);
			} else if (spaceShip.transform.rotation.eulerAngles.y < lowerLimit) {
				spaceShip.transform.rotation = Quaternion.AngleAxis (lowerLimit, Vector3.up);
			}
		}
	}

	void onStalagCollision() { 
		if (!gameOver && !bCollision) {
			EventManager.TriggerEvent ("collision");
			explosionGO.SetActive (true);
			explosionCode.shatter ();
			bCollision = true;
			impactSource.PlayOneShot (impact);
			StartCoroutine (PlayerDelays (1.5f));
		}
	}

	IEnumerator PlayerDelays(float timeDelay = 1.0f) {
		if (bCollision) {
			yield return new WaitForSeconds (timeDelay);
			onTimerCollisionComplete ();
		}
	}

	void onTimerCollisionComplete(){
		endCollision ();
	}

	void endCollision() {
		bCollision = false;
		explosionCode.resetStalag ();
		explosionGO.SetActive (false);
	}

	void onTimerPauseComplete() {
		paused = false;
	}

	// Don't hit the same stalagmite 
	bool checkIfHitAlready(int id) {
		bool found = false;
		for (int i = 0; i < hitIDs.Count; i++) {
			if (id == hitIDs[i]) {
				found = true;
				break;
			}
		}
		if (!found)
			hitIDs.Add (id);
		return found;
	}

	public Quaternion getPlayerRotation () {
		return spaceShip.transform.rotation;
	}

	public int getDistance () {
		return ((int)totalDistance);
	}

	public bool getCollisionPause() {
		return bCollision;
	}

	public void setGameOverTime(float time) {
		gameOverDecTime = time;
	}

	public void setDistanceInc(float dist, float distT = 1000) {
		distInc = dist;
		distTotal = distT;
	}

	public void setDistanceStart(float dStart) {
		distStart = dStart;
	}

	public void setDistanceSection (int tEndOpenIndex) {
		endOpenIndex = tEndOpenIndex;
		totalDistance = distTotal * (float) endOpenIndex;
	}

	public void setLevel(int levelNum) {
		if (levelNum == 1) {
			explosionGO.GetComponent<MeshRenderer> ().material = matLevel1;
		} else if (levelNum == 2) {
			explosionGO.GetComponent<MeshRenderer> ().material = matLevel2;
		}
	}
}