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
using System;
using System.Collections;
using System.Collections.Generic;

public class Planet : MonoBehaviour {

	public GameObject lookAtHolder;
	public GameObject planetHug;
	public GameObject planetLg;
	public GameObject planetMed;
	public GameObject planetSm;
	public GameObject arrowHUD;
	public int timeHold;
	public int segments = 40;

	// Passenger Orbit
	public GameObject passengerLoad;
	public GameObject passengerBall;

	private const float ROTATION_SPEED = 80.0f; 
	private const float ARROW_RADIUS = 11.0f; 
	private const float ARROW_RADIUS_MARGIN = 2.0f; 
	private const float NUM_DEGREES = 360f;
	private const float PASSENGER_CELEBRATE_SPEED = 160f;

	private GameObject planetBase;
	private GameController gameController;
	private bool hasPassenger;
	private GameObject cam;
	private Color color;
	private MeshRenderer planetRenderer;
	private bool isDestinaton;
	private Bounds planetBounds;
	private float lat, lon, planetRadius;
	private SphereCollider sC;

	// Passenger Orbit
	private float passengerLoadRadius;
	private List<GameObject> passengerBalls;
	private List<Vector3> passengerBallPos;
	private List<Vector3> axises;
	private int numPassengers, numPassengersTotal;
	private Vector3 curPosDir;
	private float passengerRadius;

	// Halo
	private PlanetHalo haloScript;
	private float startTime = 0f;
	private bool enteringPlanet;
	private int levelNum;

	// Glow
	private Light glow;
	private bool glowing;

	// HUD Arrow
	private GameObject arrowHUDContainer;
	private Color colorPyr;
	private SpriteRenderer renderSpr;
	private GameObject player;
	private PlayerController playerController;

	// Passenger Progress System
	private float xradius = 3.5f;
	private float yradius = 3.5f;
	private LineRenderer line;
	private bool bActive;
	private bool passengersLoaded;
	private bool bPlayerContact;

	// Celebration
	private bool bPassengerCelebrating;
	private float passengerCelebrateCurDeg;

	void Awake () {
		isDestinaton = false;
		hasPassenger = false;
		bActive = false;
		passengersLoaded = false;
		bPlayerContact = false;
		enteringPlanet = false;
		bPassengerCelebrating = false;
		levelNum = 1;

		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent<GameController>();
		}
		if (gameController == null) {
			Debug.Log ("Cannot find 'GameController' script");
		}
		glowing = false;
		cam = GameObject.FindGameObjectWithTag ("MainCamera");
	}

	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
		playerController = player.GetComponent<PlayerController> ();
	}

	// Positioning on the exterior of Space Sphere
	public void setLatLong(float la, float lo, float pRadius) {
		lat = la;
		lon = lo;
		planetRadius = pRadius;
	}

	// Create the Planet Mesh
	private void CreatePlanet(int size) {
		float scaleVal = 1f;
		if (size == 0) {
			planetBase = Instantiate (planetSm);
		} else if (size == 1) {
			planetBase = Instantiate (planetMed);
		} else if (size == 2) {
			planetBase = Instantiate (planetLg);
		} else if (size == 3) {
			planetBase = Instantiate (planetHug);
		}

		if (size < 2)
			scaleVal = 1.5f;

		planetBase.transform.localScale = new Vector3 (scaleVal, scaleVal, scaleVal);
		planetBase.transform.SetParent (transform, false);

		Mesh planetMesh = planetBase.GetComponent<MeshFilter> ().mesh;
		planetBounds = planetMesh.bounds;
		planetBounds.size *= scaleVal;
		xradius = yradius = planetBounds.size.x * .7f;

		glow = GetComponent<Light>();
		glow.range = planetBounds.size.x * 2f;
		glow.intensity = .5f;
		planetRenderer = planetBase.GetComponent<MeshRenderer> ();

		// positioning
		sC = planetBase.GetComponent<SphereCollider> ();
		transform.localPosition = Quaternion.AngleAxis (lon, -Vector3.up) * Quaternion.AngleAxis (lat, -Vector3.right) * new Vector3 (0, 0, planetRadius + xradius);
		sC.center = Quaternion.AngleAxis (lon * -1f, Vector3.up) * Quaternion.AngleAxis (lat * -1f, Vector3.right) * new Vector3 (0, 0, -xradius);
	}

	void Update () {

		if (glowing) {
			glow.intensity = glow.intensity - .05f;
			if (glow.intensity <= .5f) {
				glow.intensity = .5f;
				glowing = false;
			}
		}

		// rotate passenger around the planet
		if (isDestinaton || (hasPassenger && bActive)) {
			// HUD Arrow
			float dist = Vector3.Distance (transform.position, player.transform.position);
			if (dist > ARROW_RADIUS - ARROW_RADIUS_MARGIN) {
				float per = 1.0f;
				if (dist <= ARROW_RADIUS)
					per = (dist - (ARROW_RADIUS - ARROW_RADIUS_MARGIN)) / ARROW_RADIUS_MARGIN;
				colorPyr = renderSpr.material.color;
				colorPyr.a = per;
				renderSpr.material.color = colorPyr;
				arrowHUD.transform.LookAt (new Vector3 (transform.position.x, arrowHUD.transform.position.y, transform.position.z));
			}
		}

		if (isDestinaton) {
			if (bPlayerContact && !enteringPlanet) {
				float timerPer = (Time.time - startTime) / (float)timeHold;
				haloScript.setFill( timerPer );

				if (haloScript.getFill() >= 1f) {
					enteringPlanet = true;
					gameController.transitionToDodge (levelNum);
				}
			}
		}

		if (hasPassenger && bActive) {
			lookAtHolder.transform.LookAt (cam.transform);
			if (numPassengers > 0) {
				for (int i = 0; i < numPassengers; i++) {
					if (!passengerBalls [i].activeSelf) {
						passengerBalls [i].SetActive (true);
					}
					passengerBalls [i].transform.RotateAround (transform.position, axises [i], (ROTATION_SPEED + ((i + 1) * 20.0f)) * Time.deltaTime);
					float currentMoonDistance = Vector3.Distance (transform.position, passengerBalls [i].transform.position);
					Vector3 towardsTarget = passengerBalls [i].transform.position - transform.position;
					passengerBalls [i].transform.position += (passengerRadius - currentMoonDistance) * towardsTarget.normalized;
				}
			}
		}

		if (bPassengerCelebrating) {
			if (passengerCelebrateCurDeg > 90f || passengerCelebrateCurDeg < 90f - PASSENGER_CELEBRATE_SPEED * Time.deltaTime * 2f) {
				passengerCelebrateCurDeg = (passengerCelebrateCurDeg + (PASSENGER_CELEBRATE_SPEED * Time.deltaTime)) % 360f;
				passengerLoad.transform.localRotation = Quaternion.AngleAxis(passengerCelebrateCurDeg, new Vector3 (1f, 0f, 0f));
			} else {
				bPassengerCelebrating = false;
				passengerLoad.transform.localRotation = Quaternion.AngleAxis (90f, new Vector3 (1f, 0f, 0f));
			}
		}
	}

	void FixedUpdate () {}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") { 
			if (getIsDestination ()) {
				haloScript.setFill(0f);
				startTime = Time.time;
			} else if (GetHasPassenger ()) {
				loadPassengers (gameController.GetPassengerOnBoard ());
				playerController.resetPassengers ();
				gameController.LoadPassengers ();
			}

			if (!bPlayerContact) {
				bPlayerContact = true;
			}
		}
	}

	void OnTriggerExit (Collider other) {
		if (other.tag == "Player" && bPlayerContact && isDestinaton) {
			bPlayerContact = false;
			haloScript.setFill(0f);
		}
	}

	public void SetIsDestination(bool isD) {
		isDestinaton = isD;
		if (isDestinaton) {
			CreateArrowHUD ();
		}
	}

	public void setHalo(PlanetHalo pH) {
		haloScript = pH;
		haloScript.SetPlanetBounds (planetBounds);
		sC.radius = planetBounds.size.x * .5f;
	}

	public bool getIsDestination() {
		return isDestinaton;
	}

	public string ID {
		get; set;
	}

	public void SetPlanetType(int size) {
		CreatePlanet (size);
	}

	public void SetColor(string col) {
		ColorUtility.TryParseHtmlString(col, out color);

		// Planet Mesh
		planetRenderer.material.color = color;

		// Glow
		glow.color = color;
	}

	public void SetColorNew(string col) {
		ColorUtility.TryParseHtmlString(col, out color);

		// Planet Mesh
		planetRenderer.material.color = color;

		// Glow
		glow.color = color;

		// Passenger Load
		Color cC = passengerLoad.GetComponent<MeshRenderer>().material.color;
		cC.r = color.r;
		cC.g = color.g;
		cC.b = color.b;
		passengerLoad.GetComponent<MeshRenderer>().material.color = cC;

		// Passengers
		for (int i = 0; i < passengerBalls.Count; i++) {
			Color c = passengerBalls [i].GetComponent<MeshRenderer>().material.color;
			c.r = color.r;
			c.g = color.g;
			c.b = color.b;
			passengerBalls [i].GetComponent<MeshRenderer>().material.color = c;
		}

		// ArrowHUD Color
		Color cS = renderSpr.material.color;
		cS.r = color.r;
		cS.g = color.g;
		cS.b = color.b;
		renderSpr.material.color = cS;
	}

	public Color GetColor() {
		return color;
	}

	public void SetHasPassenger(int numPassengersTot) {
		hasPassenger = true;

		if (hasPassenger) {
			axises = new List<Vector3> ();
			passengerBalls = new List<GameObject> ();
			passengerBallPos = new List<Vector3> ();
			passengerRadius = planetBounds.size.x * .8f;
			numPassengers = 0;
			numPassengersTotal = numPassengersTot;
			for (int i = 0; i < GameGlobalVars.GlobalFlowMaxPassengers; i++) {
				float xA = UnityEngine.Random.Range (-1.0f, 1.0f);
				float zA = UnityEngine.Random.Range (-1.0f, 1.0f);
				axises.Add(new Vector3(xA, 0f, zA));
				passengerBalls.Add (Instantiate (passengerBall) as GameObject);
				passengerBallPos.Add (new Vector3 (0.0f, 0.0f, 0.0f));
				passengerBalls [passengerBalls.Count - 1].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				passengerBalls [passengerBalls.Count - 1].transform.SetParent (transform, false);
				passengerBalls [passengerBalls.Count - 1].transform.position = new Vector3 (0f, passengerRadius, 0f); // new Vector3 (0.0f, 0.0f, -1.0f * (float)(i + 1) * .2f);
				//  passengerBalls [passengerBalls.Count - 1].transform.position = (passengerBalls [passengerBalls.Count - 1].transform.position - transform.position).normalized * passengerRadius + transform.position;
				passengerBalls [passengerBalls.Count - 1].SetActive (false);
				Color c = passengerBalls [passengerBalls.Count - 1].GetComponent<MeshRenderer>().material.color;
				c.r = color.r;
				c.g = color.g;
				c.b = color.b;
				passengerBalls [passengerBalls.Count - 1].GetComponent<MeshRenderer>().material.color = c;
			}

			// Create the completeton line/circle
			GameObject passengerOrbit = new GameObject ();
			passengerOrbit.transform.SetParent (lookAtHolder.transform, false);
			line = passengerOrbit.gameObject.AddComponent<LineRenderer>();
			line.material = new Material(Shader.Find("Particles/Additive"));
			line.SetColors (new Color (color.r, color.g, color.b, .4f), new Color (color.r, color.g, color.b, .4f));
			line.SetWidth(0.05F, 0.05F);
			line.SetVertexCount (segments + 1);
			line.useWorldSpace = false;
			CreatePoints ();

			// Create the Progress Disk
			passengerLoad = Instantiate (passengerLoad) as GameObject;
			passengerLoad.transform.SetParent (lookAtHolder.transform, false);
			passengerLoad.transform.localRotation = Quaternion.AngleAxis (90f, new Vector3 (1f, 0f, 0f));
			setPassengerLoadRadius ();
			Color cC = passengerLoad.GetComponent<MeshRenderer>().material.color;
			cC.r = color.r;
			cC.g = color.g;
			cC.b = color.b;
			passengerLoad.GetComponent<MeshRenderer>().material.color = cC;

			CreateArrowHUD ();
			setPassengerActive (false);
		}
	}

	private void setPassengerLoadRadius() {
		passengerLoadRadius = xradius * 2f * .7f + xradius * 2f * .3f * ((float)numPassengers / (float)numPassengersTotal);
		passengerLoad.transform.localScale = new Vector3 (passengerLoadRadius, passengerLoad.transform.localScale.y, passengerLoadRadius);
	}

	private void CreatePoints () {
		float x;
		float y;
		float z = 0f;
		float angle = (360f - NUM_DEGREES) * .5f;
		for (int i = 0; i < (segments + 1); i++) {
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * xradius;
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * yradius;
			line.SetPosition (i, new Vector3(x,y,z));
			angle += (NUM_DEGREES / segments);
		}
	}

	void CreateArrowHUD() {
		arrowHUDContainer = GameObject.FindWithTag ("ArrowContain");
		arrowHUD = Instantiate (arrowHUD) as GameObject;
		arrowHUD.transform.SetParent (arrowHUDContainer.transform, false);
		foreach (Transform child in arrowHUD.transform){
			if (hasPassenger) {
				if (child.name == "ArrowHUDPlanet") {
					renderSpr = child.GetComponent<SpriteRenderer> ();
					if (hasPassenger) {
						Color cS = renderSpr.material.color;
						cS.r = color.r;
						cS.g = color.g;
						cS.b = color.b;
						renderSpr.material.color = cS;
					}
				} else if (child.name == "ArrowHUDPlanet2") {
					child.gameObject.SetActive (false);
				}
			} else {
				if (child.name == "ArrowHUDPlanet") {
					child.gameObject.SetActive (false);
				} else {
					renderSpr = child.GetComponent<SpriteRenderer> ();
				}
			}
		}
	} 
		
	public bool GetHasPassenger() {
		return hasPassenger && bActive && !passengersLoaded;
	}

	private void PassengersLoaded() {
		passengersLoaded = true;
		passengerCelebration ();
	}

	private void passengerCelebration() {
		if (arrowHUD.activeSelf)
			arrowHUD.SetActive (false);
		bPassengerCelebrating = true;
		passengerCelebrateCurDeg = (passengerLoad.transform.localRotation.eulerAngles.x + PASSENGER_CELEBRATE_SPEED * Time.deltaTime) % 360f;
	}

	public bool GetPassengersLoaded() {
		return passengersLoaded;
	}
		
	public void loadPassengers(int numPass) {
		if (numPass > 0 && !passengersLoaded) {
			glowing = true;
			glow.intensity = 1f;
			numPassengers += numPass;
			setPassengerLoadRadius ();

			if (numPassengers == numPassengersTotal) {
				PassengersLoaded ();
			}
		}
	}

	public void setPassengerTotal(int tot) {
		numPassengersTotal = tot;
	}

	public int getPassengerTotal() {
		return numPassengersTotal;
	}

	public void setPassengerActive(bool active) {

		if (bActive)
			resetPassengerSettings ();
		bActive = active;

		if (bActive) {
			if (!line.enabled)
				line.enabled = true;
			if (!passengerLoad.activeSelf)
				passengerLoad.SetActive (true);
			if (!arrowHUD.activeSelf)
				arrowHUD.SetActive (true);
		} else {
			if (line.enabled)
				line.enabled = false;
			if (passengerLoad.activeSelf)
				passengerLoad.SetActive (false);
			if (arrowHUD.activeSelf)
				arrowHUD.SetActive (false);
		}
	}

	private void resetPassengerSettings() {
		passengersLoaded = false;
		numPassengers = 0;
		setPassengerLoadRadius ();
		for (int i = 0; i < GameGlobalVars.GlobalFlowMaxPassengers; i++) {
			if (passengerBalls [i].activeSelf) {
				passengerBalls [i].SetActive (false);
			}
		}
	}

	public void setLevel(int lNum) {
		levelNum = lNum;
	}
}