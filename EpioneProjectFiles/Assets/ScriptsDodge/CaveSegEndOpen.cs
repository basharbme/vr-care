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

public class CaveSegEndOpen : CaveSeg {

	public GameObject yeti;
	public GameObject redLights;
	public Material matLevel2;

	private Yeti yC;
	private Transform yetiPos, mesh1, mesh2;
	private GameObject caveEntry, deadEnd; 
	private GameObject exitSign;
	private DodgeExit dExit;
	private Material matLevel1;

	protected override void Awake()
	{ 
		base.Awake ();
		rotThis = 0.0f;
		Mesh mesh = transform.Find("open_plain").GetComponent<MeshFilter>().mesh;
		mesh1 = transform.Find ("open_plain");
		mesh1.gameObject.SetActive (true);
		mesh2 = transform.Find ("open_cliffs");
		mesh2.gameObject.SetActive (false);
		bounds = mesh.bounds;

		// X = Width, Z = Height, Y + Length of Cave/
		transform.localEulerAngles = new Vector3(270.0f, 180.0f, 0);
		transform.localPosition =  new Vector3(0, 0, 0);
		endPosition = new Vector3(0.0f, -4.5f, bounds.size.y - 5.0f);
		deadEnd = transform.Find ("cave_facade_deadend").gameObject;
		caveEntry = transform.Find("cave_facade-baked").gameObject;
		exitSign = transform.Find ("exitsign").gameObject;
		dExit = transform.Find ("CubeExit").gameObject.GetComponent<DodgeExit>();
		yetiPos = transform.FindChild ("YetiPos").transform;
		yeti = Instantiate (yeti) as GameObject;
		yeti.transform.SetParent (transform, false);
		yeti.transform.position = yetiPos.position;
		yeti.transform.rotation = yetiPos.rotation;
		yC = yeti.GetComponent<Yeti> ();

		redLights = Instantiate (redLights) as GameObject;
		redLights.transform.SetParent (transform, false);
		redLights.transform.localPosition = Vector3.zero;
		redLights.transform.Rotate (Vector3.right, 90f);
		redLights.transform.localPosition = new Vector3(0, 0, -73.25f);

		MeshRenderer ren1 = caveEntry.GetComponent<MeshRenderer> ();
		matLevel1 = ren1.material;
	}

	public override Vector3 GetEndPosition(Vector3 lastPosition, float totRot) {
		endPosition = Quaternion.AngleAxis(totRot, Vector3.up) * endPosition;
		transform.position = lastPosition;
		endPosition = lastPosition + endPosition;
		return endPosition;
	}

	public void setLevel(int levelNum) {
		if (levelNum == 1) {
			mesh1.gameObject.SetActive (true);
			mesh2.gameObject.SetActive (false);
			MeshRenderer ren1 = caveEntry.GetComponent<MeshRenderer> ();
			ren1.material = matLevel1;
			MeshRenderer ren2 = deadEnd.GetComponent<MeshRenderer> ();
			ren2.material = matLevel1;
		}
		else if (levelNum == 2) {
			mesh1.gameObject.SetActive (false);
			mesh2.gameObject.SetActive (true);
			MeshRenderer ren1 = caveEntry.GetComponent<MeshRenderer> ();
			ren1.material = matLevel2;
			MeshRenderer ren2 = deadEnd.GetComponent<MeshRenderer> ();
			ren2.material = matLevel2;
		}
	}

	public void setEnd() {
		deadEnd.SetActive (true);
		exitSign.SetActive (false);
		dExit.ignoreExit = true;
		yC.Wave2 ();
	}

	public void wave() {
		yC.Wave();
	}

	public void walk() {
		yC.Walk ();
	}

	public void setBegin() {
		yeti.SetActive (false);
	}

	public float getCaveLength() {
		return bounds.size.y;
	}
}