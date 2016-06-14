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

public class PassengerTorus : MonoBehaviour {

	// HUD Arrow
	public float arrowRadius; 
	public float arrowRadiusMargin; 
	public GameObject arrowHUD;
	public int segments = 40;

	private GameObject arrowHUDContainer;
	private GameObject torus;
	private GameObject passenger;
	private Color colorPyr;
	private SpriteRenderer renderSpr;
	private MeshRenderer renderPassenger;
	private Color colorTorus;
	private MeshRenderer renderTorus;
	private Color colorPassenger;
	private GameController gameController;
	private GameObject player;
	private GameObject cam;
	private bool inOrbit;
	private bool safe;
	private bool onBoard;

	void Awake() {
		inOrbit = false;
		safe = false;
		onBoard = false;
		cam = GameObject.FindGameObjectWithTag ("MainCamera");
		player = GameObject.FindGameObjectWithTag ("Player");
		torus = transform.Find("PassengerTorusCollider").gameObject;
		passenger = transform.Find ("PassengerCollider").gameObject;
		renderPassenger = passenger.GetComponent<MeshRenderer> ();
		renderTorus = torus.GetComponent<MeshRenderer> ();

		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent<GameController>();
		}
		if (gameController == null) {
		}

		// HUD Arrow 
		arrowHUDContainer = GameObject.FindWithTag ("ArrowContain");
		arrowHUD = Instantiate (arrowHUD) as GameObject;
		arrowHUD.transform.SetParent (arrowHUDContainer.transform, false);
		foreach (Transform child in arrowHUD.transform){
			if (child.name == "ArrowHUDPassenger") {
				renderSpr = child.GetComponent<SpriteRenderer> ();
				break;
			}
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (!inOrbit && !onBoard) {
			// HUD Arrow
			float dist = Vector3.Distance (transform.position, player.transform.position);
			if (dist > arrowRadius - arrowRadiusMargin) {
				float per = 1.0f;
				if (dist <= arrowRadius)
					per = (dist - (arrowRadius - arrowRadiusMargin)) / arrowRadiusMargin;
				colorPyr = renderSpr.material.color;
				colorPyr.a = per;
				renderSpr.material.color = colorPyr;
				if (!arrowHUD.activeSelf)
					arrowHUD.SetActive (true);
				arrowHUD.transform.LookAt (new Vector3 (transform.position.x, arrowHUD.transform.position.y, transform.position.z));
			} else {
				if (arrowHUD.activeSelf)
					arrowHUD.SetActive (false);
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (!safe && gameController != null && !onBoard && !inOrbit) {
			if (!gameController.getWaitingToStart ()) {
				MoveOnBoard ();
			}
		}
	}

	private void MoveOnBoard () {
		gameObject.SetActive(false);
		onBoard = true;
	}

	public void Disable () {
		onBoard = false;
		inOrbit = false;
		gameObject.SetActive (false);
		if (arrowHUD.activeSelf)
			arrowHUD.SetActive (false);
	}

	public void MoveToSpace (Vector3 startPos, Vector3 endPos, float time) {
		if (!inOrbit && (onBoard || gameController.getWaitingToStart () || gameObject.activeSelf == false)) {
			gameObject.SetActive (true);
			transform.LookAt (cam.transform);
			safe = true;
			onBoard = false;
			Physics.IgnoreCollision (player.GetComponent<Collider> (), torus.GetComponent<Collider> ());
			transform.localPosition = startPos;
			transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
			float angle = Random.value * Mathf.PI * 2f;
			Vector3 midPos = new Vector3 (Mathf.Cos(angle) * 20f, -15.0f, Mathf.Sin(angle) * 20f);
			StartCoroutine (MoveObject (startPos, midPos, endPos, time));
		}
	}
		
	IEnumerator MoveObject (Vector3 startPos, Vector3 midPos, Vector3 endPos, float time) {
		float i = 0.0f;
		float rate = 1.0f / (time*.5f);
		while (i < 1.0f) {
			i += Time.deltaTime * rate;
			transform.localPosition = Vector3.Lerp(startPos, midPos, i);
			transform.LookAt (cam.transform);
			colorTorus = renderTorus.material.color;
			colorTorus.a = (1.0f - i)  * .75f;
			renderTorus.material.color = colorTorus;
			colorPassenger = renderPassenger.material.color;
			colorPassenger.a = (1.0f - i);
			renderPassenger.material.color = colorPassenger;
			yield return new WaitForSeconds(.01f);
		}
		transform.LookAt (cam.transform);
		i = 0.0f;
		rate = 1.0f/(time * .5f);
		while (i < 1.0f) {
			i += Time.deltaTime * rate;
			transform.localPosition = Vector3.Lerp(midPos, endPos, i);
			transform.LookAt (cam.transform);
			yield return new WaitForSeconds(.01f);
		}
		transform.LookAt (cam.transform);
		colorTorus = renderTorus.material.color;
		colorTorus.a = .75f;
		renderTorus.material.color = colorTorus;
		colorPassenger = renderPassenger.material.color;
		colorPassenger.a = 1.0f;
		renderPassenger.material.color = colorPassenger;
		safe = false;
		Physics.IgnoreCollision(player.GetComponent<Collider>() , torus.GetComponent<Collider>(), safe);
	}
		
	public void setInOrbit() {
		if (onBoard) {
			inOrbit = true;
			onBoard = false;
		}
	}

	public void setColor(Color col) {
		colorPassenger = renderPassenger.material.color;
		colorPassenger.r = col.r;
		colorPassenger.g = col.g;
		colorPassenger.b = col.b;
		renderPassenger.material.color = colorPassenger;
	}
}