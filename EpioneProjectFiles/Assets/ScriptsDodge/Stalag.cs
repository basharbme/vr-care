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

public class Stalag : MonoBehaviour {
	
	public GameObject explosion;
	public GameObject playerExplosion;
	public int scoreValue;
	public Material matLevel2;

	private const float FALL_ACCEL = 2f;

	// Audio
	private bool hitStalag = false;
	private bool shatters;
	private bool falling;
	private float fallSpeed;
	private bool fallingDelay;
	private float timeFallingDelay;

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Vector3[] verticesOrig;
	private Vector3 angle;

	private float shatterInc = .05f;
	private float shatterVMax = 3.0f;
	private float shatterVMin = 0.0f;
	private float shatterHMax = 2.0f;
	private float shatterHMin = 0.2f;
	private Material matLevel1;

	void Awake() {
		shatters = false;
		mesh = GetComponent<MeshFilter> ().mesh;
		verticesOrig = mesh.vertices;
		vertices = mesh.vertices;
		normals = mesh.normals;
		fallSpeed = 0.0f;
		shatterVMax = 3.0f;
		shatterVMin = 0.0f;
		shatterHMax = 2.0f;
		shatterHMin = 0.2f;
		matLevel1 = GetComponent<MeshRenderer> ().material;
	}

	void Start() {
		falling = false;
		fallingDelay = false;
		timeFallingDelay = 0.0f;
		fallSpeed = 0.0f;
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Boundary") {
			return;
		} else if (other.tag == "Player" && !shatters) {
			EventManager.TriggerEvent ("StalagCollision");
		}
	}

	void Update() {
		if (shatters) {
			if (hitStalag) {
				int i = 0;
				while (i < vertices.Length) {
					vertices [i] += normals [i] * shatterHMax;
					vertices [i] += Vector3.forward * shatterVMin * -1.0f;
					i++;
				}
				mesh.vertices = vertices;
				if ((shatterHMax - shatterInc) >= shatterHMin)
					shatterHMax -= shatterInc;
				if (shatterVMin + shatterInc <= shatterVMax)
					shatterVMin += shatterInc;
			}
		}
		if (falling) {
			transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
			fallSpeed += FALL_ACCEL;
		}
	}

	// Two functions for the shattering stalag
	public void setShatters(bool shatter) {
		shatters = shatter;
	}

	public void shatter () {
		vertices = mesh.vertices;
		normals = mesh.normals;
		hitStalag = true;
	}

	public void resetStalag () {
		fallSpeed = 0.0f;
		hitStalag = false;
		shatterVMax = 3.0f;
		shatterVMin = 0.0f;
		shatterHMax = 2.0f;
		shatterHMin = 0.2f;
		transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
		mesh.vertices = verticesOrig;
	}

	public void startFall (float timePer = 1.0f) {
		fallingDelay = true;
		timeFallingDelay = timePer * 2.0f;
		StartCoroutine(StalagDelays());
	}

	public void setLevel (int levelNum) {
		if (levelNum == 1) {
			GetComponent<MeshRenderer> ().material = matLevel1;
		} else if (levelNum == 2) {
			GetComponent<MeshRenderer> ().material = matLevel2;
		}
	}

	void onTimerFallComplete(){
		fallingDelay = false;
		falling = true;
	}

	protected IEnumerator StalagDelays() {
		if (fallingDelay) {
			yield return new WaitForSeconds (timeFallingDelay);
			onTimerFallComplete ();
		}
	}
}