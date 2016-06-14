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

[System.Serializable]
public class BeamData {
	public float yVal;
	public List<int> indexes;
	public List<Vector3> beamVerts; 
}

public class PlayerBeam : MonoBehaviour {

	public GameObject beam;

	private const float MAX_ROT = 20f;
	private const int MAX_INDEX = 33;

	private Mesh meshBeam;
	private MeshFilter mfBeam;
	private Vector3[] verticesBeam, verticesBeamOrig;
	private List<BeamData> beamData;
	private float curRot;
	private bool bending;

	void Awake () {
		bending = false;
		curRot = 0f;
	}

	// Use this for initialization
	void Start () {
		// Bend the Beam
		mfBeam = beam.GetComponent<MeshFilter>();
		meshBeam = mfBeam.mesh;
		verticesBeam = meshBeam.vertices;
		verticesBeamOrig = meshBeam.vertices;
		beamData = new List<BeamData> ();
		bool found = false;
		for (var i = 0; i < verticesBeam.Length; i++) {
			for (var k = 0; k < beamData.Count; k++) {
				if (verticesBeam [i].y < beamData [k].yVal + .01 && verticesBeam [i].y > beamData [k].yVal - .01) {
					found = true;
					beamData[k].beamVerts.Add (verticesBeam [i]);
					beamData[k].indexes.Add(i);
					break;
				}
			}
			if (!found) {
				BeamData bD = new BeamData ();
				bD.yVal = verticesBeam [i].y;
				bD.beamVerts = new List<Vector3>();
				bD.beamVerts.Add (verticesBeam [i]);
				bD.indexes = new List<int> ();
				bD.indexes.Add(i);
				beamData.Add(bD);
			}
			found = false;
		}
			
		beamData.Sort(delegate(BeamData a, BeamData b) {
			return (a.yVal).CompareTo(b.yVal);
		});
	}
	
	// Update is called once per frame
	void Update () {
		if (bending) {
			for (int i = MAX_INDEX; i >= 0; i--) {
				float angPer = ((float)MAX_INDEX - (float)i) / (float)MAX_INDEX;
				float xVal = Mathf.Tan (Mathf.Deg2Rad * (angPer * curRot)) * (beamData [MAX_INDEX].yVal - beamData [i].yVal);
				if (xVal < 1f && xVal > -1f) {
					for (int k = 0; k < beamData [i].indexes.Count; k++) {
						float x = verticesBeamOrig [beamData [i].indexes [k]].x + xVal;
						verticesBeam [beamData [i].indexes [k]] = new Vector3 (x, verticesBeamOrig [beamData [i].indexes [k]].y, verticesBeamOrig [beamData [i].indexes [k]].z);
					}
				}
			}
			meshBeam.vertices = verticesBeam;
			meshBeam.RecalculateNormals ();
		}
	}

	public void setBend(float turnVel) {
		if (turnVel == 0) {
			bending = false;
			meshBeam.vertices = verticesBeamOrig;
			meshBeam.RecalculateNormals ();
		} else {
			bending = true;
			curRot = turnVel > MAX_ROT ? MAX_ROT : turnVel;
			curRot = turnVel < MAX_ROT * -1f ? MAX_ROT * -1f  : turnVel;
		}
	}
}