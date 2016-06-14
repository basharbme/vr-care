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
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class Boundry {
	public float xMin, xMax, zMin, zMax;	 
}
	
public class PlayerController : MonoBehaviour {

	public Text debugText; 
	public float speed;
	public float tilt;
	public Boundry boundry;
	public GameObject shot;
	public Transform shotSpawn;
	public float fireRate;
	public GameObject beam;
	public float yRotMax;
	public float xRotVar;
	public GameObject CarboardAdaptor;
	public float limit = 0.5f;
	public GameObject passengerBall;

	// Star Skydome
	public SkyDomeStar stars;
	public Planets planets;

	// Astroid
	public float accelerationForce = .0001f;
	public float rotationForce = .0003f;

	private const float RADIUS = 0.5f;
	private const float RADIUS_SPEED = 0.5f;
	private const float rotationSpeed = 80.0f;

	private Rigidbody rb;
	private PlayerBeam beamCode;
	private float nextFire;
	private Quaternion rot, rotTemp;
	// private double rotXLast, rotYLast;
	private List<GameObject> passengerBalls;
	private List<Vector3> passengerBallPos;
	private List<Vector3> axises;
	private int numPassengers;
	private Vector3 curPosDir;

	// Rotate Around 
	private float rotShip;
	// private float rotShipLast;
	private GameController gameController;

	void Awake() {
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent<GameController>();
		}
		if (gameController == null) {
			Debug.Log ("Cannot find 'GameController' script");
		}

		passengerBalls = new List<GameObject> ();
		passengerBallPos = new List<Vector3> ();
		axises = new List<Vector3> ();
		numPassengers = GameGlobalVars.GlobalFlowMaxPassengers;
		for (int i = 0; i < numPassengers; i++) {
			passengerBalls.Add (Instantiate (passengerBall) as GameObject);
			passengerBallPos.Add (new Vector3 (0.0f, 0.0f, 0.0f));
			passengerBalls [passengerBalls.Count - 1].transform.localScale = new Vector3 (2.0f, 2.0f, 2.0f);
			passengerBalls [passengerBalls.Count - 1].transform.SetParent (transform, false);
			passengerBalls [passengerBalls.Count - 1].transform.position = new Vector3 (0.0f, 0.0f, -1.0f * (float)(i + 1) * .2f);
			passengerBalls [passengerBalls.Count - 1].transform.position = (passengerBalls [passengerBalls.Count - 1].transform.position - transform.position).normalized * RADIUS + transform.position;
			passengerBalls [passengerBalls.Count - 1].SetActive (false);
		}

		axises.Add (new Vector3 (1, 1, 0));
		axises.Add (new Vector3 (1, 0, 1));
		axises.Add (new Vector3 (-1, 0, 1));
		axises.Add (new Vector3 (-1, 0, -1));
		axises.Add (new Vector3 (1, 0, -1));
		axises.Add (new Vector3 (Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0));
		axises.Add (new Vector3 (Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
		axises.Add (new Vector3 (Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
		axises.Add (new Vector3 (Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
		axises.Add (new Vector3 (Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
	}

	void Start() {
		rb = GetComponent<Rigidbody> ();
		rotShip = 0.0f;
		// rotShipLast = 0.0f;
		beamCode = beam.GetComponent<PlayerBeam> ();
	}

	void Update() {
		if (Time.time > nextFire) {
			nextFire = Time.time + fireRate;
		}
			
		if (gameController.GetPassengerOnBoard () > 0) {
			Quaternion currentQuat = rb.rotation;
			curPosDir = currentQuat * transform.InverseTransformDirection (transform.forward);
			for (int i = 0; i < gameController.GetPassengerOnBoard (); i++) {
				if (!passengerBalls [i].activeSelf) {
					passengerBalls [i].SetActive (true);
				}
				passengerBalls [i].transform.RotateAround (curPosDir, axises [i], (rotationSpeed + ((i + 1) * 20.0f)) * Time.deltaTime);
				var desiredPosition0 = (passengerBalls [i].transform.position - curPosDir).normalized * RADIUS + curPosDir;
				passengerBalls [i].transform.position = Vector3.MoveTowards (passengerBalls [i].transform.position, desiredPosition0, Time.deltaTime * RADIUS_SPEED);
			}
		}

		if (!gameController.getWaitingToStart()) {

			// Skybox Movement
			stars.rotXZ ();
			planets.rotXZ();

			// SINGLETON ROTATION
			rotShip = Mathf.Atan2((PlayerInputSingleton.Instance.getXPerDeg () - .5f), (PlayerInputSingleton.Instance.getZPerDeg () - .5f)) * Mathf.Rad2Deg; //  - 90.0f;
			float beamLast = rb.transform.rotation.eulerAngles.y;
			rb.transform.rotation = Quaternion.Lerp(rb.transform.rotation, Quaternion.AngleAxis(rotShip-90f, Vector3.up), Time.deltaTime * 6f);

			// BEAM
			float perEngine = Mathf.Abs(PlayerInputSingleton.Instance.getXPerDeg () - .5f);
			if (Mathf.Abs(PlayerInputSingleton.Instance.getZPerDeg () - .5f) >= perEngine)
				perEngine = Mathf.Abs(PlayerInputSingleton.Instance.getZPerDeg () - .5f);
			beam.transform.localScale = new Vector3 (1.0f, (1.0f - .1f) * perEngine * 2f + .1f, 1.0f);
			beamCode.setBend ((beamLast - rb.transform.rotation.eulerAngles.y) * 2f);
		}
	}

	void OnTriggerEnter(Collider other) {
		if (!gameController.getWaitingToStart ()) {
			if (other.tag == "Passenger") {
				gameController.AddPassenger ();
			} else if (other.transform.gameObject.layer == LayerMask.NameToLayer ("astroidsFlow")) {
				for (int i = 0; i < passengerBalls.Count; i++) {
					passengerBalls [i].SetActive (false);
				}
				gameController.EjectPassengers ();
			}
		}
	}

	public void resetPassengers() {
		for (int i = 0; i < passengerBalls.Count; i++) {
			passengerBalls [i].SetActive (false);
		}
	}

	public float resetXRot() {
		//rot = rotTemp = Cardboard.SDK.HeadPose.Orientation;
		float rotXNew = rot.x;
		return rotXNew;
	}

	public void reloadXRot (float xRotBase) {

	}

	public void setColor(Color col) {
		for (int i = 0; i < passengerBalls.Count; i++) {
			Color color = passengerBalls [i].GetComponent<MeshRenderer>().material.color;
			color.r = col.r;
			color.g = col.g;
			color.b = col.b;
			passengerBalls [i].GetComponent<MeshRenderer>().material.color = color;
		}
	}
}