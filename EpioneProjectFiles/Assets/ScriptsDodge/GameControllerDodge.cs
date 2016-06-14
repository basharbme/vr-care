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
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

enum CaveSegType{Straight1, Straight2, Straight3, Right45, Left45, Right90, Left90, EndOpen, EndCliff};

public class fallingStalagData
{
	public int positionIndex;
	public Stalag codeRef;
	public bool fell = false;
	public int totalPerSegment = 0;
}

public class GameControllerDodge : MonoBehaviour {

	// Player Vars
	public GameObject player;

	// Game Level Variables
	public GameObject stalagS;
	public GameObject stalagM;
	public GameObject stalagL;
	public GameObject obstacleContainer;
	public GameObject caveSegmentStraight;
	public GameObject caveSegmentEndOpen;
	public float targDistToEnd;
	public float startWait;
	public float restartWait;
	public float gameOverWait;

	// FPS
	public  float updateInterval = 0.5F;
	public Text textFPS;

	// Scoring + UI
	public Canvas canvasHUD;
	public Canvas canvasHUDScore;

	// Game Level Variables
	private GameObject[] caveSegs;
	private CaveSeg[] caveSegScripts;

	// Player Vars
	private Rigidbody rb;
	private PlayerControllerDodge pC;

	// Distance
	private const float SEC_DISTANCE = 1000f;
	private float secDistPerUnit;
	private float secDistTotal;

	// Positioning
	private List<Vector3> pathPoints;
	private List<GameObject> spheres; // Debug Only;
	private List<int> endOpenIndexes;
	private List<fallingStalagData> fallingStalags;
	private int endOpenIndex;
	private int numStagesBeforeFalling;
	private bool bFallingStalags;
	private bool incEndOpenIndex;
	private int lastPointNear0;
	private int lastPointNear1;
	private float angleCorrect;
	private float angleCorrectOld;
	private float angleCorrectNew;
	private float angleCorrectDelta;

	public Image transition;
	public Text transitionText;

	// Tweening the angle value
	private float lerpTime = 1.5f;
	private float currentLerpTime;		
	private CaveSegType[] boards = {
		CaveSegType.EndOpen,
		CaveSegType.Straight2,
		CaveSegType.Straight3,
		CaveSegType.Straight1,
		CaveSegType.Straight2,
		CaveSegType.Straight2,
		CaveSegType.Straight3,
		CaveSegType.Straight1,
		CaveSegType.Straight2,
		CaveSegType.EndOpen,
		CaveSegType.Straight2,
		CaveSegType.Straight1,
		CaveSegType.Straight3,
		CaveSegType.Straight1,
		CaveSegType.Straight2,
		CaveSegType.Straight1,
		CaveSegType.Straight3,
		CaveSegType.Straight1,
		CaveSegType.EndOpen,
		CaveSegType.Straight3,
		CaveSegType.Straight2,
		CaveSegType.Straight1,
		CaveSegType.Straight2,
		CaveSegType.Straight3,
		CaveSegType.Straight2,
		CaveSegType.Straight1,
		CaveSegType.Straight2,
		CaveSegType.EndOpen
	};

	private Vector3 lastPosition;
	private RaycastHit hitStalag;

	// Game Management
	private bool restart = false;
	private bool gameOver = false;
	private bool starting = false;
	private bool bExit = false;
	private float endTime;
	private bool countingDown;

	// FPS
	private float accum   = 0; 	// FPS accumulated over the interval
	private int   frames  = 0; 	// Frames drawn over the interval
	private float timeleft; 	// Left time for current interval

	// Scoring + UI
	private int numCollisions;
	private int highScoreLevel0;
	private HUDDodge canvasHUDCode;
	private HUDDodgeScore canvasHUDScoreCode;
	private UnityAction collisionListener, exitListener;

	// Levels
	private int curLevel;

	// Use this for initialization
	void Awake () {

		// Player
		rb = player.GetComponent<Rigidbody> ();
		pC = player.GetComponent<PlayerControllerDodge> ();
		pC.setGameOverTime (gameOverWait);

		// Positioning 
		lastPointNear0 = -1;
		lastPointNear1 = 0;
		endOpenIndex = 0;
		incEndOpenIndex = false;
		bFallingStalags = false;
		currentLerpTime = 0f;
		numStagesBeforeFalling = 3;

		// FPS
		timeleft = updateInterval;
		
		// Set up the level according to the CaveSegType[] arrangement
		curLevel = PlayerPrefs.GetInt ("dodgeLevel");
		CreateCaves ();

		// Positioning
		angleCorrect = angleCorrectOld = angleCorrectNew = Mathf.Atan2 (pathPoints [1].z - pathPoints [0].z, pathPoints [1].x - pathPoints [0].x) * 180 / Mathf.PI;
		angleCorrectDelta = 0.0f;
		pC.playerGuideRotation = playerRotation (angleCorrect);
		if (GameGlobalVars.GlobalDebug) {
			colorSpheresDebug ();
		}

		secDistPerUnit = SEC_DISTANCE / secDistTotal;
		pC.setDistanceInc (secDistPerUnit, SEC_DISTANCE);
		pC.setLevel (curLevel);
		
		// Scoring
		collisionListener = new UnityAction (onCollision);
		EventManager.StartListening ("collision", collisionListener);

		exitListener = new UnityAction (onExit);
		EventManager.StartListening ("exitDodgePlayer", exitListener);

		transition.CrossFadeAlpha (0.0f, 0f, false);
		transitionText.CrossFadeAlpha (0.0f, 0f, false);

		gameOver = false;
		restart = false;
		starting = true;
		bExit = false;
		numCollisions = 0;

		// UI Reset Gameover
		canvasHUD.enabled = true;
		canvasHUDScore.enabled = false;
		canvasHUDCode = canvasHUD.GetComponent<HUDDodge> ();
		canvasHUDScoreCode = canvasHUDScore.GetComponent<HUDDodgeScore> ();

		highScoreLevel0 = PlayerPrefs.GetInt ("highScoreLevel" + curLevel, 0);
		// PlayerPrefs.SetInt ("highScoreLevel0", 0);

		UnityAction canvasHUDScoreCodeHide = new UnityAction (onExitScoreComplete);
		EventManager.StartListening (HUDDodgeScore.HIDE, canvasHUDScoreCodeHide);

		UnityAction canvasHUDScoreCodeEnd = new UnityAction (onTimerExitScoreHideComplete);
		EventManager.StartListening (HUDDodgeScore.END, canvasHUDScoreCodeEnd);

		// Start Game
		countingDown = false;
		StartCountDown ();
		StartCoroutine (GameDelays ());
	}

	void CreateCaves(){
		// Create Cave Segments
		lastPosition = new Vector3 (0, 0, 0);
	
		int numCaves = boards.Length;
		caveSegs = new GameObject[numCaves];
		caveSegScripts = new CaveSeg[numCaves];
		endOpenIndexes = new List<int> (); 

		if (GameGlobalVars.GlobalFalling) {
			fallingStalags = new List<fallingStalagData> ();
		}

		// Keep track of the path points through all segments to make sure player doesn't go backwards. 
		pathPoints = new List<Vector3> ();

		// Add an initial point behind the player position
		pathPoints.Add (new Vector3 (0.0f, 0.0f, -200.0f));
		if (GameGlobalVars.GlobalDebug) {
			spheres = new List<GameObject> (); 
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.position = pathPoints[0];
			sphere.transform.localScale = new Vector3(30.0f, 30.0f, 30.0f);
			spheres.Add(sphere);
		}

		// Cumulative turn rotation of all segments 
		float totRotation = 0.0f;

		// Density of Stalags
		int numStalagsPerSeg = 5;

		// Section Distance
		bool countingDistance = true;
		secDistTotal = 0f;

		for (int i = 0; i < caveSegs.Length; i++) {
			GameObject newGO1;
			CaveSeg myScriptReference;
			
			if (boards[i] == CaveSegType.Straight1) {
				newGO1 = Instantiate (caveSegmentStraight) as GameObject;
				myScriptReference = newGO1.GetComponent<CaveSegStraight> ();
				caveSegScripts[i] = myScriptReference;
				(myScriptReference as CaveSegStraight).setLevel (curLevel);
			} else if (boards[i] == CaveSegType.Straight2) {
				newGO1 = Instantiate (caveSegmentStraight) as GameObject;
				myScriptReference = newGO1.GetComponent<CaveSegStraight>();
				(myScriptReference as CaveSegStraight).setMesh (2);
				(myScriptReference as CaveSegStraight).setLevel (curLevel);
				caveSegScripts[i] = myScriptReference;
			} else if (boards[i] == CaveSegType.Straight3) {
				newGO1 = Instantiate (caveSegmentStraight) as GameObject;
				myScriptReference = newGO1.GetComponent<CaveSegStraight>();
				(myScriptReference as CaveSegStraight).setMesh (3);
				(myScriptReference as CaveSegStraight).setLevel (curLevel);
				caveSegScripts[i] = myScriptReference;
			} else {
				newGO1 = Instantiate(caveSegmentEndOpen) as GameObject;
				myScriptReference = newGO1.GetComponent<CaveSegEndOpen>();
				(myScriptReference as CaveSegEndOpen).setLevel (curLevel);
			}
			
			newGO1.transform.SetParent (obstacleContainer.transform, false);  // .transform.parent = obstacleContainer.transform;
			caveSegs[i] = newGO1;

			Vector3 lastPositionOld = lastPosition;
			
			// Position and rotate the segment
			lastPosition = myScriptReference.GetEndPosition(lastPosition, totRotation);
			
			if (boards [i] == CaveSegType.EndOpen) { 
				newGO1.transform.Rotate (Vector3.forward, totRotation);
				// Increase density of stalags per segment
				// numStalagsPerSeg += 5;
				if (i != 0) {
					countingDistance = false;
					endOpenIndexes.Add (pathPoints.Count - 1);
				} else {
					pC.setDistanceStart ((myScriptReference as CaveSegEndOpen).getCaveLength ());
					endOpenIndexes.Add (pathPoints.Count - 1);
					(myScriptReference as CaveSegEndOpen).setBegin ();
				}

				if (i == caveSegs.Length - 1) {
					(myScriptReference as CaveSegEndOpen).setEnd ();
				} else if (endOpenIndexes.Count == 1) {
					(myScriptReference as CaveSegEndOpen).walk ();
					// numStalagsPerSeg += 5;
				} else if (endOpenIndexes.Count == 2) {
					(myScriptReference as CaveSegEndOpen).wave ();
					numStalagsPerSeg = 15;
				}
			} else {
				numStalagsPerSeg += 1;
				newGO1.transform.Rotate (Vector3.up, totRotation);
			}

			// Place the stalagmites
			if (boards [i] != CaveSegType.EndOpen) {
				if (countingDistance) {
					secDistTotal += (myScriptReference as CaveSegStraight).getCaveLength ();
				}
				PlaceStalags (i, lastPositionOld, totRotation, numStalagsPerSeg);
			}

			totRotation += myScriptReference.GetRotation();
		}

		// Add the final point for the end of the board.
		pathPoints.Add (lastPosition);
	}
	
	void PlaceStalags (int index, Vector3 lastPosition, float totRotation, int numStalagsPerSeg) {	
		// Number of bezier line points.
		float numPoints = caveSegScripts [index].path.points.Count;
		
		// Variation in X and Z directions
		float xVar = 40f; // 30.0f;
		float zVar = 40f; // 30.0f;

		List<int> fallingStalagIndexForTotal = new List<int> ();
		int fallingStalagTotal = 0;

		// Instantiate each stalag along the BezierPath through the tunnel segment.
		for (int i = 0; i < numStalagsPerSeg; i++) {
			float calcVal = ((float)i / (float) numStalagsPerSeg) * (numPoints - 1);

			if (index == 0) {
				if (calcVal < .3f)
					calcVal = .3f;
			}

			float xPos = caveSegScripts[index].path.GetPositionByT(calcVal).x * -1.0f;
			float zPos = caveSegScripts[index].path.GetPositionByT(calcVal).z * -1.0f; // - caveSegScripts[index].bounds.size.y;
			
			Vector3 sPos = new Vector3(xPos + (xVar - (xVar * 2.0f * Random.value)), 10.0f, zPos + (zVar - (zVar * 2.0f * Random.value)));
			sPos = Quaternion.AngleAxis(totRotation, Vector3.up) * sPos;
			sPos = sPos + lastPosition;

			GameObject stalag;
			float stalagNum = Random.value * 3;
			if (stalagNum < 1)
				stalag = Instantiate (stalagS, sPos, Quaternion.identity) as GameObject;
			else if (stalagNum < 2)
				stalag = Instantiate (stalagM, sPos, Quaternion.identity) as GameObject;
			else
				stalag = Instantiate (stalagL, sPos, Quaternion.identity) as GameObject;
			
			stalag.transform.SetParent (obstacleContainer.transform, false);
			stalag.transform.Rotate (Vector3.left, 90);
			stalag.GetComponent<Stalag> ().setLevel (curLevel);

			// Should we position these Stalags on the top so they can fall?
			if (GameGlobalVars.GlobalFalling) {
				if (numStagesBeforeFalling != -1 && numStagesBeforeFalling <= endOpenIndexes.Count && !bFallingStalags) {
					bFallingStalags = true;
				} else if (bFallingStalags) {
					bFallingStalags = false;
				} 
			}
				
			// Make sure the stalag is on the bottom
			if (!bFallingStalags) {
				if (Physics.Raycast (sPos, -Vector3.up, out hitStalag)) {
					stalag.transform.position = new Vector3 (sPos.x, sPos.y - hitStalag.distance, sPos.z);
				}
			} else {
				fallingStalagData fSD = new fallingStalagData ();
				fSD.codeRef = stalag.GetComponent<Stalag> ();
				fSD.positionIndex = pathPoints.Count; 
				fallingStalags.Add (fSD);
				fallingStalagIndexForTotal.Add (fallingStalags.Count - 1);
				fallingStalagTotal += 1;
				stalag.transform.Rotate (Vector3.right, 180.0f);
				if (Physics.Raycast (sPos, Vector3.up, out hitStalag)) {
					stalag.transform.position = new Vector3 (sPos.x, sPos.y + hitStalag.distance, sPos.z);
				}
			}
		}

		for (int s = 0; s < fallingStalagIndexForTotal.Count; s++) {
			fallingStalags [fallingStalagIndexForTotal [s]].totalPerSegment = fallingStalagTotal;
		}

		// Configure points for positioning
		// STRAFE
		int numPoints2 = caveSegScripts [index].path2.points.Count;
		for (int p = 0; p < numPoints2; p++) {
			Vector3 pos = caveSegScripts[index].path2.points[p].p1;
			pos = Quaternion.AngleAxis(totRotation + 180.0f, Vector3.up) * pos;
			pos = pos + lastPosition;

			if (p < numPoints2 - 1 || (boards[index + 1] == CaveSegType.EndOpen) ) {
				pathPoints.Add (pos);
				if (GameGlobalVars.GlobalDebug) {
					GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					sphere.transform.position = pos;
					sphere.transform.localScale = new Vector3(30.0f, 30.0f, 30.0f);
					spheres.Add(sphere);
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (countingDown) {
			float timeLeft = endTime - Time.time;
			if (timeLeft <= 0) {
				timeLeft = 0;
				countingDown = false;
			}
			canvasHUDCode.countdown(Mathf.Round(timeLeft).ToString ());
		} else {
			canvasHUDCode.countdown("");
		}

		if (restart) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		} else if (!gameOver && !restart && !starting) {
			// Positioning
			checkPlayerPosition();
		}

		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - update GUI text and start new interval
		canvasHUDCode.setDistance (pC.getDistance ());

		if( timeleft <= 0.0 )
		{
			// display two fractional digits (f2 format)
			// float fps = accum/frames;
			// string format = System.String.Format("{0:F2} FPS",fps);
			//textFPS.text = format;
			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}

		// Check for distance to final point
		float distToFinal = Vector3.Distance (rb.position, lastPosition);
		if (distToFinal <= targDistToEnd && !gameOver && !restart) {
			bExit = true;
			GameOver ();
		}
	}

	// Positioning
	void checkPlayerPosition() {
		int lastPointNearOld = lastPointNear0;
		float dist0 = 200000.0f;

		if (lastPointNear0 < 0)
			lastPointNear0 = 0;

		// Find the shortest distance to a point
		for (int p = lastPointNear0; p < pathPoints.Count; p++) {
			float d = Vector3.Distance(rb.transform.position, pathPoints[p]);
			if (d < dist0) {
				dist0 = d;
			} else if (d > dist0) {
				lastPointNear0 = p - 1;
				break;
			}
		}

		lastPointNear1 = lastPointNear0 + 1;
		if (Vector3.Distance (pathPoints [lastPointNear0], pathPoints [lastPointNear1]) < 
		    Vector3.Distance (rb.transform.position, pathPoints [lastPointNear1]) &&
		    lastPointNear0 > 0) {
			lastPointNear0 -= 1;
			lastPointNear1 = lastPointNear0 + 1;
		}

		if (lastPointNear0 == endOpenIndexes [endOpenIndex] && pC.bIncDistance) {
			if (endOpenIndex == 0) {
				inOpenArea (false);
			} else {
				inOpenArea ();
			}
		} else if (lastPointNear0 != endOpenIndexes [endOpenIndex] && !pC.bIncDistance) {
			if (incEndOpenIndex) {
				inCaveArea ();
			}
		}

		if (lastPointNear0 != lastPointNearOld) {
			angleCorrectNew = angleCorrectOld; // = angleCorrect;
			angleCorrect = Mathf.Atan2 (pathPoints [lastPointNear1].z - pathPoints [lastPointNear0].z, pathPoints [lastPointNear1].x - pathPoints [lastPointNear0].x) * 180 / Mathf.PI;//  + 360f;
			angleCorrectDelta = Mathf.DeltaAngle(angleCorrectOld, angleCorrect);
			if (GameGlobalVars.GlobalDebug) {
				colorSpheresDebug ();
			}
			pC.lookAtTarget = pathPoints [lastPointNear1];
			currentLerpTime = 0f;
			if (GameGlobalVars.GlobalFalling) {
				processFallingStalags ();
			}
		}
	
		if (Mathf.Round(angleCorrectOld) != Mathf.Round(angleCorrect)) {
			// Let's SmoothStep!
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > lerpTime) {
				currentLerpTime = lerpTime;
			}
			float perc = currentLerpTime / lerpTime;
			float angleCorrectStep = Mathf.SmoothStep(0f, angleCorrectDelta, perc);
			angleCorrectOld = angleCorrectNew + angleCorrectStep;
			pC.playerGuideRotation = playerRotation (angleCorrectOld);
			pC.lookAtTarget = pathPoints [lastPointNear1];
		}
	}

	private float playerRotation (float angVal) {
		return (360 - ((angVal + 180.0f) % 360) + 90) % 360;
	}

	private void colorSpheresDebug () {
		int index0 = lastPointNear0;
		if (index0 < 0)
			index0 = 0;
		int index1 = index0 + 1;
		Renderer rend0 = spheres [index0].GetComponent<Renderer> ();
		rend0.material.color = Color.red;
		Renderer rend1 = spheres [index1].GetComponent<Renderer> ();
		rend1.material.color = Color.green;
	}

	private void inOpenArea (bool bShowScore = true) {
		pC.bIncDistance = false;
		incEndOpenIndex = true;
		if (bShowScore) {
			pC.setDistanceSection (endOpenIndex);
			showScore (true);
		}
		RenderSettings.fogStartDistance = 800f;
		RenderSettings.fogEndDistance = 1000f;
	}

	private void inCaveArea() {
		pC.bIncDistance = true;
		incEndOpenIndex = false;
		endOpenIndex += 1;
		hideScore ();
		RenderSettings.fogStartDistance = 800f;
		RenderSettings.fogEndDistance = 1000f;
	}

	private void processFallingStalags () {
		int fallingStalagCounter = 0;
		for (int i = 0; i < fallingStalags.Count; i++) {
			if (!fallingStalags [i].fell) {
				if (fallingStalags [i].positionIndex == lastPointNear0) {
					fallingStalags [i].codeRef.startFall ((float) fallingStalagCounter / (float) fallingStalags [i].totalPerSegment);
					fallingStalagCounter += 1;
					fallingStalags [i].fell = true;
				} else {
					break;
				}
			}
		}
	}

	private void StartCountDown () {
		endTime = Time.time + startWait;
		canvasHUDCode.countdown(startWait.ToString());
		countingDown = true;
	}
	
	IEnumerator GameDelays() {
		canvasHUDCode.show ();
		yield return new WaitForSeconds (startWait);
		starting = false;
		EventManager.TriggerEvent ("startMoving");

		while (true) {
			// Loop for some reocurring thing
			yield return new WaitForSeconds(1);
			
			if (gameOver) {
				// restart = true;
				break;
			}
		}

		if (!bExit)
			yield return new WaitForSeconds (gameOverWait);

		if (!bExit) {
			restart = true;
			yield return new WaitForSeconds (restartWait);
		} else {
			transition.CrossFadeAlpha (1.0f, .5f, false);
			transitionText.CrossFadeAlpha (1.0f, .5f, false);
			StartCoroutine(TransitionPause());
		}
	}

	IEnumerator TransitionPause() {
		yield return new WaitForSeconds (2f);
		SceneManager.LoadScene ("Flow");
	}

	void onExit () {
		// showScore (true);
		bExit = true;
		GameOver ();
	}

	void onCollision() {
		// UI
		numCollisions += 1;
		canvasHUDCode.collision (numCollisions);
		if (numCollisions > 3)
		{
			showScore (false);
			GameOver ();
		}
	}

	void showScore(bool check) {
		if (!canvasHUDScoreCode.enabledHUD ()) {
			if (check) {
				canvasHUDScoreCode.setFinished (true);
			} else {
				canvasHUDScoreCode.setFinished (false);
			}
			if (pC.getDistance () >= highScoreLevel0)
				canvasHUDScoreCode.setRecord (pC.getDistance ());
			else 
				canvasHUDScoreCode.setRecord (highScoreLevel0);
			canvasHUDScoreCode.show ((float)gameOverWait);
			canvasHUDCode.hide (false);
		}
	}

	void onExitScoreComplete () {

	}

	void hideScore() {
		if (!canvasHUDScoreCode.hiding () && canvasHUDScoreCode.enabledHUD ()) {
			canvasHUDScoreCode.hide ();
		}
	}

	void onTimerExitScoreHideComplete () {
		canvasHUDCode.show ();
	}
	
	public void GameOver() {
		EventManager.TriggerEvent ("gameOver");
		// Score
		if (pC.getDistance () >= highScoreLevel0) {
			PlayerPrefs.SetInt ("highScoreLevel" + curLevel, pC.getDistance ());
			highScoreLevel0 = pC.getDistance ();
		}
		gameOver = true;
	}
}