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

public class CaveSeg : MonoBehaviour {
	
	public BezierPath path; 
	public BezierPath path2; 
	public Bounds bounds;

	protected Vector3 endPosition;
	protected float rotThis;
	
	protected virtual void Awake()
	{ 
		endPosition = new Vector3 (0, 0, 0);
		Transform transformBezier, transformBezier2;
		if (transformBezier = transform.Find ("BezierPath")) {
			path = transformBezier.gameObject.GetComponent<BezierPath> ();
			if (!GameGlobalVars.GlobalDebug) {
				path.color = Color.clear;
				path.lineColor = Color.clear;
			}
		}

		if (transformBezier2 = transform.Find ("BezierPath2")) {
			path2 = transformBezier2.gameObject.GetComponent<BezierPath> ();
			if (!GameGlobalVars.GlobalDebug) {
				path2.color = Color.clear;
				path2.lineColor = Color.clear;
			}
		}
	}
	
	// Use this for initialization
	protected virtual void Start () {
		
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		
	}

	public virtual void CreateLine(Vector3 lastPosition) {
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		Color c1 = new Color (255, 0, 0);
		Color c2 = new Color (255, 255, 255);
		float width = 2.0f;
		lineRenderer.SetVertexCount(2);
		lineRenderer.SetColors(c1, c2); 
		lineRenderer.SetPosition(0, lastPosition); 
		lineRenderer.SetPosition(1, endPosition); 
		lineRenderer.SetWidth(width, width); 
	}
	
	public virtual Vector3 GetEndPosition(Vector3 lastPosition, float totRot) {
		endPosition = Quaternion.AngleAxis(totRot, Vector3.up) * endPosition;
		transform.position = lastPosition;
		endPosition = lastPosition + endPosition;
		return endPosition;
	}
	
	public virtual float GetRotation () {
		return rotThis;
	}
}