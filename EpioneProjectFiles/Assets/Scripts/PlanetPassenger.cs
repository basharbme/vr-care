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

public class PlanetPassenger : MonoBehaviour {

	public GameObject passengerOrbit;
	public GameObject passengerTorus;
	public int segments = 40;

	private const float XRADIUS = 1.15f;
	private const float YRADIUS = 1.15f;
	private const float NUM_DEGREES = 320f;
	private const float ROT_INC = 2f;

	private GameObject cam;
	private GameObject orbit;
	private Vector3 rotVec;
	private LineRenderer line;

	// Use this for initialization
	void Start () {
		
		passengerOrbit = Instantiate (passengerOrbit) as GameObject;
		passengerTorus = Instantiate (passengerTorus) as GameObject;

		cam = GameObject.FindGameObjectWithTag ("MainCamera");
		orbit = this.transform.FindChild("PassengerHolder").gameObject;
		rotVec = new Vector3 (0.0f, 1.0f, 0.0f);
		passengerOrbit.transform.SetParent (orbit.transform, false);
		passengerTorus.transform.SetParent (orbit.transform, false);

		line = passengerOrbit.gameObject.AddComponent<LineRenderer>();
		line.material = new Material(Shader.Find("Particles/Additive"));
		line.SetColors (new Color (1.0f, 1.0f, 1.0f, .2f), new Color (1.0f, 1.0f, 1.0f, .2f));
		line.SetWidth(0.1F, 0.1F);
		line.SetVertexCount (segments + 1);
		line.useWorldSpace = false;
		CreatePoints ();

		transform.LookAt (cam.transform);
	}

	// Update is called once per frame
	void Update () {
		orbit.transform.Rotate(rotVec, ROT_INC);
	}

	void CreatePoints () {
		float x;
		float y;
		float z = 0f;
		float angle = (360f - NUM_DEGREES) * .5f;
		for (int i = 0; i < (segments + 1); i++)
		{
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * XRADIUS;
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * YRADIUS;
			line.SetPosition (i,new Vector3(x,y,z) );
			angle += (NUM_DEGREES / segments);
		}
	}
}