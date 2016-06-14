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
using UnityEngine.SceneManagement;

public class latLongAreaData
{
	public float minLong, maxLong;
	public float minLat, maxLat;
}

public class GameController : MonoBehaviour {

	// FLOW WORLD OBJECTS
	public GameObject hazard;

	// Player
	public GameObject player;
	public GameObject rotatePlanetX; 
	public GameObject rotatePlanetZ; 

	// Planets
	public GameObject planet;
	public GameObject planetRotate;
	public GameObject planetHalo;
	public GameObject planetPassenger;
	public GameObject planetPassengerTorus;

	// Game Control
	public float startWait;
	public float restartWait;
	public float waveWait;

	// HUD
	public Image flash;
	public Text scoreText;
	public Text restartText;
	public Text gameOverText;
	public Text countdownText;
	public Image transition;
	public Text transitionText;

	private const float PLANET_RADIUS = 15f;

	// Planets
	private PlanetDataCont[] planetData;
	private struct PlanetDataCont {
		public int planetType;
		public float latitude, longitude, satelliteRadius;
		public bool isDestination;
		public bool hasPassenger;
		public string planetID;
		public Planet pC;
		public string color;
		public PlanetDataCont(int planetT, float la,  float lo, float sR, bool iDest, string pID, string col, bool pssngr = false, Planet pCode = null) {
			planetType = planetT;
			latitude = la;
			longitude = lo;
			satelliteRadius = sR;
			isDestination = iDest;
			hasPassenger = pssngr;
			planetID = pID;
			pC = pCode;
			color = col;
		}
	};
	private string[] planetColorList = {
		"#ff7e00", // "#F10D24",
		"#c341ff", // "#CCCCCC",
		"#e2d72a", // "#2A5885"
		"#00a2ff",
		"#00ffea"
	};
	private int curColorNum;

	// Audio
	private AudioSource[] aSources;
	private AudioSource aCheer;
	private AudioSource aEject;
	private AudioSource aLoad;

	// Game Control
	private bool gameOver;
	private bool bWaitingToStart;
	private PlayerController pC;

	// Countdown
	private float endTime;
	private bool countingDown;

	// Passengers
	private List<PassengerTorus> passengers;
	private List<latLongAreaData> latLongAreas;
	private int passengerOnBoard; // AboardShip
	private int passengerMax;
	private int passengerTotalLevel;
	private int passengerNumFloating;
	private int passengerNumHome;
	private Color curColor;

	// Levels
	private int levelCur;
	private int astroidNum;
	private int astroidInc;
	private List<Astroid> astroids;
	private List<Planet> levelPlanets;

	void Start () {
		curColor = new Color (1.0f, 1.0f, 1.0f);
		gameOver = false;
		bWaitingToStart = true;
		countingDown = false;
		pC = player.GetComponent<PlayerController> ();
		aSources = GetComponents<AudioSource>(); aCheer = aSources[1]; aEject = aSources[2]; aLoad = aSources[3];
		gameOverText.text = "";
		countdownText.text = "";

		if (!transition.gameObject.activeSelf)
			transition.gameObject.SetActive( true );
		transition.CrossFadeAlpha (0.0f, 0f, false);
		transitionText.CrossFadeAlpha (0.0f, 0f, false);

		if (!flash.gameObject.activeSelf)
			flash.gameObject.SetActive( true );
		flash.CrossFadeAlpha (0.0f, 0.0f, false);

		RenderPassengerCount ();
		passengerOnBoard = 0;
		passengerNumHome = 0;
		passengerMax = GameGlobalVars.GlobalFlowMaxPassengers;
		passengerNumFloating = passengerTotalLevel = 1;
		astroidInc = 10;
		setAstroidNum ();

		CreatePlanets ();
		CreatePassengers ();
		CreateAstroids ();
		SetColor (curColor);

		// Start Scene
		StartCoroutine(SpawnWaves());
		StartCountDown ();
		EjectPassengers ();
	}



	void CreatePlanets() {
		levelCur = 0;
		levelPlanets = new List<Planet> ();
		planetData = new PlanetDataCont[] { 
			new PlanetDataCont(3, 37.723763f, -122.415112f, 6.0f, false, "planetBig", planetColorList[0], true),	// SF
			new PlanetDataCont(0, 25.766628f, -80.193559f, 3.5f, false, "planetMedRed", planetColorList[1], true),	// Miami
			new PlanetDataCont(1, 6.514773f, 3.385816f, 2.5f, true, "planetSmRed", "#CCCCCC"),		// Lagos
			new PlanetDataCont(1, -33.842242f, 151.243743f, 4.0f, false, "planetMedPur", planetColorList[2], true),	// Sydney
			new PlanetDataCont(2, 60.173196f, 24.940596f, 4.0f, true, "planetMedPurLi", "#5EA191")	// Helsinki
		};
		curColorNum = 3;

		int levelInc = 1;
		for (int i = 0; i < planetData.Length; i++)
		{
			GameObject newGO = Instantiate (planet) as GameObject;
			newGO.transform.parent = planetRotate.transform;
			Planet planetScript = newGO.GetComponent<Planet> ();
			planetScript.setLatLong (planetData [i].latitude, planetData [i].longitude, PLANET_RADIUS);
			planetScript.SetPlanetType (planetData [i].planetType);
			planetScript.ID = planetData[i].planetID;
			planetScript.SetIsDestination(planetData[i].isDestination);
			// planetScript.SetRadius (planetData[i].satelliteRadius);
			planetScript.SetColor (planetData[i].color);
			planetData[i].pC = planetScript;

			if (planetData [i].isDestination) {
				GameObject halo = Instantiate (planetHalo) as GameObject;
				halo.transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);
				halo.transform.SetParent (newGO.transform, false);
				halo.GetComponent<PlanetHalo> ().setHolder(planetScript.lookAtHolder);
				planetScript.setHalo (halo.GetComponent<PlanetHalo> ());
				planetScript.setLevel (levelInc);
				levelInc++;
			} else if (planetData [i].hasPassenger) {
				planetScript.SetHasPassenger (passengerTotalLevel + levelPlanets.Count);
				ColorUtility.TryParseHtmlString(planetData [i].color, out curColor);
				levelPlanets.Add (planetScript);
			}
		}

		// Set Levels for planet
		levelPlanets[levelCur % levelPlanets.Count].setPassengerActive(true);
		curColor = levelPlanets [levelCur % levelPlanets.Count].GetColor ();
	}

	private void CreatePassengers() {
		populateLatLongAreas ();
		passengers = new List<PassengerTorus> ();
		Vector3 originPos = planetRotate.transform.InverseTransformPoint(player.transform.position);
		for (int i = 0; i < passengerMax; i++) {
			GameObject newGO = Instantiate (planetPassengerTorus) as GameObject;
			newGO.transform.SetParent (planetRotate.transform, false);
			newGO.transform.localPosition = originPos;
			PassengerTorus pT = newGO.GetComponent("PassengerTorus") as PassengerTorus;
			pT.MoveToSpace (newGO.transform.localPosition, getSphereRange(i), 2.0f);
			pT.setColor (curColor);
			passengers.Add (pT);
			if (i >= passengerTotalLevel) {
				pT.Disable ();
			}
		}
		RenderPassengerCount ();
	}

	void CreateAstroids() {
		astroids = new List<Astroid>();
		for (int k = 0; k < GameGlobalVars.GlobalFlowMaxAstroids; k++) {
			Vector3 onSphere = Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f) , -Vector3.up) * Quaternion.AngleAxis(Random.Range(-90.0f, 90.0f) , -Vector3.right) * new Vector3(0, 0, PLANET_RADIUS);
			GameObject newGO = Instantiate (hazard) as GameObject;
			newGO.transform.rotation = Random.rotation;
			newGO.transform.parent = planetRotate.transform;
			newGO.transform.localPosition = onSphere;
			astroids.Add (newGO.GetComponent<Astroid>());
			if (k >= astroidNum) {
				newGO.GetComponent<Astroid> ().disableAstroid ();
			}
		}
	}
		
	void Update() {
		if (countingDown) {
			float timeLeft = endTime - Time.time;
			if (timeLeft <= 0) {
				timeLeft = 0;
				countingDown = false;
			}
			countdownText.text = Mathf.Round(timeLeft).ToString ();
		} else {
			countdownText.text = "";
		}
	}

	private void StartCountDown () {
		endTime = Time.time + startWait;
		countdownText.text = startWait.ToString();
		countingDown = true;
	}
	
	IEnumerator SpawnWaves() {
		yield return new WaitForSeconds (startWait);
		bWaitingToStart = false;
		while (true) {
			yield return new WaitForSeconds(1.0f);
			if (gameOver) {
				restartText.text = "Press 'R' for Restart";
				// restart = true;
				break;
			}
		}
		yield return new WaitForSeconds (restartWait);
	}

	IEnumerator TransitionPause() {
		yield return new WaitForSeconds (2f);
		SceneManager.LoadScene ("Dodge");
	}

	public void EjectPassengers() {
		flash.CrossFadeAlpha (0.5f, 0.0f, false);
		flash.CrossFadeAlpha (0.0f, 0.3f, false);
		Vector3 originPos = planetRotate.transform.InverseTransformPoint(player.transform.position);
		if (!bWaitingToStart && passengerOnBoard > 0) {
			aEject.Play ();
			for (int i = 0; i < passengerTotalLevel; i++) {
				passengers[i].MoveToSpace (originPos, getSphereRange(i), 2.0f);
			}
		}
		passengerNumFloating += passengerOnBoard;
		passengerOnBoard = 0;
		RenderPassengerCount ();
	}

	private Vector3 getSphereRange(int index) {
		return Quaternion.AngleAxis (Random.Range (latLongAreas[index].minLong, latLongAreas[index].maxLong), -Vector3.up) * Quaternion.AngleAxis (Random.Range (latLongAreas[index].minLat, latLongAreas[index].maxLat), -Vector3.right) * new Vector3 (0, 0, PLANET_RADIUS);
	}

	private void setAstroidNum() {
		astroidNum = passengerTotalLevel * astroidInc;
	}

	// for DEBUGGING
	void drawLineToPassenger(Vector3 posStart, Vector3 posEnd) {
		GameObject lineHolder;
		lineHolder = new GameObject();
		lineHolder.transform.SetParent(planetRotate.transform, false);
		LineRenderer line;
		line = lineHolder.AddComponent<LineRenderer>();
		line.transform.SetParent (lineHolder.transform, false);
		line.SetColors (new Color (1.0f, 0.0f, 0.0f, 1.0f), new Color (1.0f, 0.0f, 0.0f, 1.0f));
		line.SetWidth(0.2F, 0.2F);
		line.useWorldSpace = false;
		line.SetPosition (0, posStart);
		line.SetPosition (1, posEnd);
	}
		
	public void AddPassenger() {
		if (!bWaitingToStart && !gameOver) {
			aCheer.Play ();
			passengerOnBoard += 1;
			passengerNumFloating -= 1;
			RenderPassengerCount ();
		}
	}

	public void LoadPassengers() {
		if (passengerOnBoard > 0) {
			for (int i = 0; i < passengerTotalLevel; i++) {
				passengers [i].setInOrbit ();
			}
			aLoad.Play ();
		}
		passengerNumHome += passengerOnBoard;
		passengerOnBoard = 0;
		RenderPassengerCount ();

		if (levelPlanets [levelCur % levelPlanets.Count].GetPassengersLoaded()) {
			nextLevel ();
		}
	}

	private void nextLevel() {
		levelCur += 1;

		// Change Color
		if (levelCur > 2) {
			curColorNum = levelCur % planetColorList.Length;
			levelPlanets [levelCur % levelPlanets.Count].SetColorNew (planetColorList [curColorNum]);
		}

		// Increase the number of passengers
		passengerTotalLevel += 1;
		if (passengerTotalLevel > passengerMax) {
			passengerTotalLevel = passengerMax;
		}
		passengerNumHome = 0;
		passengerNumFloating = passengerTotalLevel;
		curColor = levelPlanets [levelCur % levelPlanets.Count].GetColor ();
		SetColor (curColor);

		Vector3 originPos = planetRotate.transform.InverseTransformPoint(player.transform.position);
		for (int i = 0; i < passengerTotalLevel; i++) {
			passengers[i].Disable ();
			passengers[i].MoveToSpace (originPos, getSphereRange(i), 2.0f);
		}
		RenderPassengerCount ();
	
		// Enable next planet
		levelPlanets [levelCur % levelPlanets.Count].setPassengerTotal(passengerTotalLevel);
		levelPlanets [levelCur % levelPlanets.Count].setPassengerActive (true);

		// Increase the number of astroids
		setAstroidNum();
		for (int i = 0; i < astroidNum; i++) {
			astroids[i].enableAstroid ();
		}
	}

	public int GetPassengerOnBoard() {
		return passengerOnBoard;
	}

	public void GoToDodge(string planetID) {
		gameOverText.text = "Now entering " + planetID + " for Dodge";
		gameOver = true;
	}

	public void GameOver() {
		gameOverText.text = "Game Over - restarting...";
		gameOver = true;
	}
		
	void RenderPassengerCount() {
		scoreText.text = passengerNumHome + " / " + passengerTotalLevel;
	}

	public bool getWaitingToStart() {
		return bWaitingToStart;
	}

	private void SetColor(Color col) {
		// Set the floating passengers Color;
		for (int i = 0; i < passengerTotalLevel; i++) {
			passengers [i].setColor (curColor);
		}

		// Set the players passenger's colors
		pC.setColor (curColor);
	}

	public void transitionToDodge(int levelNum) {
		transition.CrossFadeAlpha (1.0f, .5f, false);
		transitionText.CrossFadeAlpha (1.0f, .5f, false);
		PlayerPrefs.SetInt ("dodgeLevel", levelNum);
		StartCoroutine(TransitionPause());
	}

	private void populateLatLongAreas() {
		latLongAreas = new List<latLongAreaData> ();
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = -92.621f;
		latLongAreas[latLongAreas.Count - 1].maxLong = -20.008f;
		latLongAreas[latLongAreas.Count - 1].minLat = 41.495f;
		latLongAreas[latLongAreas.Count - 1].maxLat = 69.954f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = -174.928f;
		latLongAreas[latLongAreas.Count - 1].maxLong = -115.490f;
		latLongAreas[latLongAreas.Count - 1].minLat = -10.259f;
		latLongAreas[latLongAreas.Count - 1].maxLat = 34.040f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = 43.079f;
		latLongAreas[latLongAreas.Count - 1].maxLong = 97.784f;
		latLongAreas[latLongAreas.Count - 1].minLat = -21.034f;
		latLongAreas[latLongAreas.Count - 1].maxLat = 4.472f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = -53.897f;
		latLongAreas[latLongAreas.Count - 1].maxLong = 4.135f;
		latLongAreas[latLongAreas.Count - 1].minLat = -36.280f;
		latLongAreas[latLongAreas.Count - 1].maxLat = -5.996f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = 130.738f;
		latLongAreas[latLongAreas.Count - 1].maxLong = 172.315f;
		latLongAreas[latLongAreas.Count - 1].minLat = 6.122f;
		latLongAreas[latLongAreas.Count - 1].maxLat = 33.888f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = -119f;
		latLongAreas[latLongAreas.Count - 1].maxLong = -81f;
		latLongAreas[latLongAreas.Count - 1].minLat = -55f;
		latLongAreas[latLongAreas.Count - 1].maxLat = -32f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = -179f;
		latLongAreas[latLongAreas.Count - 1].maxLong = -153f;
		latLongAreas[latLongAreas.Count - 1].minLat = -58f;
		latLongAreas[latLongAreas.Count - 1].maxLat = -34f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = 74f;
		latLongAreas[latLongAreas.Count - 1].maxLong = 111f;
		latLongAreas[latLongAreas.Count - 1].minLat = -53f;
		latLongAreas[latLongAreas.Count - 1].maxLat = -38f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = 74f;
		latLongAreas[latLongAreas.Count - 1].maxLong = 126f;
		latLongAreas[latLongAreas.Count - 1].minLat = 60f;
		latLongAreas[latLongAreas.Count - 1].maxLat = 73f;
		latLongAreas.Add (new latLongAreaData ());
		latLongAreas[latLongAreas.Count - 1].minLong = -62f;
		latLongAreas[latLongAreas.Count - 1].maxLong = -32f;
		latLongAreas[latLongAreas.Count - 1].minLat = 16f;
		latLongAreas[latLongAreas.Count - 1].maxLat = 29f;
	}

}