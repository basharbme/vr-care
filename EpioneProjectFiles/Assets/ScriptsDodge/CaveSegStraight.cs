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

public class CaveSegStraight : CaveSeg {

	public Material caveFloor2, caveSides2;

	private Transform mesh1, mesh2, mesh3;
	private Material[] matLevel1, matLevel2;

	protected override void Awake() 
	{ 
		base.Awake ();
		rotThis = 0.0f;
		transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0);
		if (mesh1 = transform.Find("cave_STRAIGHT1")) {
			Mesh mesh = mesh1.GetComponent<MeshFilter>().mesh;
			bounds = mesh.bounds;
			endPosition = new Vector3(-13.7f, 0, bounds.size.y - 8.1f);
		}

		mesh2 = transform.Find ("cave_STRAIGHT2");
		mesh2.gameObject.SetActive (false);
		mesh3 = transform.Find ("cave_STRAIGHT3");
		mesh3.gameObject.SetActive (false);

		matLevel1 = mesh1.gameObject.GetComponent<MeshRenderer> ().materials;
		matLevel2 = mesh1.gameObject.GetComponent<MeshRenderer> ().materials;
		matLevel2 [0] = caveSides2;
		matLevel2 [1] = caveFloor2;
	}

	public void setMesh(int meshNum) {
		mesh1.gameObject.SetActive (false);
		if (meshNum == 2) {
			mesh2.gameObject.SetActive (true);
		} else if (meshNum == 3) {
			mesh3.gameObject.SetActive (true);
		}
	}

	public void setLevel (int levelNum) {
		if (levelNum == 1) {
			mesh1.gameObject.GetComponent<MeshRenderer> ().materials = matLevel1;
			mesh2.gameObject.GetComponent<MeshRenderer> ().materials = matLevel1;
			mesh3.gameObject.GetComponent<MeshRenderer> ().materials = matLevel1;
		} else if (levelNum == 2) {
			mesh1.gameObject.GetComponent<MeshRenderer> ().materials = matLevel2;
			mesh2.gameObject.GetComponent<MeshRenderer> ().materials = matLevel2;
			mesh3.gameObject.GetComponent<MeshRenderer> ().materials = matLevel2;
		}
	}

	public float getCaveLength() {
		return bounds.size.y;
	}
}