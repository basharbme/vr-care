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

public class PlayerInputSingleton : MonoBehaviour {

	private static PlayerInputSingleton instance = null;
	private Quaternion rot;
	private float xRot, xRotMin, xRotMax, zRot, zRotMin, zRotMax;
	private float xRotVar = .3f, zRotVar = .6f;
	private float xPer, zPer;
	private float xPerZeroMove = .05f, zPerZeroMove = .05f;
	private float rotXLast, rotZLast;
	private float dXLast, dZLast;

	private float xRotDeg, xRotMinDeg, xRotMaxDeg, zRotDeg, zRotMinDeg, zRotMaxDeg;
	private float xRotVarDeg = 20f, zRotVarDeg = 40f;
	private float xPerDeg, zPerDeg;

	// Game Instance Singleton
	public static PlayerInputSingleton Instance
	{
		get
		{ 
			return instance; 
		}
	}

	private void Awake()
	{
		// if the singleton hasn't been initialized yet
		if (instance != null && instance != this) 
		{
			Destroy(this.gameObject);
			return;
		}
		// GvrViewer.Instance.AutoDriftCorrection = false;
		// GvrViewer.Instance.autoUntiltHead = false;
		instance = this;
		DontDestroyOnLoad( this.gameObject );
	}

	private void Start() {
		reset ();
	}

	void Update()
	{
		rot = GvrViewer.Instance.HeadPose.Orientation; // Cardboard.SDK.HeadPose.Orientation;
		zRot = rot.z * -1.0f;
		zPer = (Mathf.Clamp (zRot, zRotMin, zRotMax) - zRotMin) / (zRotMax - zRotMin);
		if (zPer < .5f + zPerZeroMove && zPer > .5f - zPerZeroMove)
			zPer = .5f;

		xRot = rot.x;
		xPer = (Mathf.Clamp (xRot, xRotMin, xRotMax) - xRotMin) / (xRotMax - xRotMin);
		if (xPer < .5f + xPerZeroMove && xPer > .5f - xPerZeroMove)
			xPer = .5f;

		dXLast = Mathf.Abs(rot.x - rotXLast);
		dZLast = Mathf.Abs(rot.z - rotZLast);

		rotXLast = rot.x;
		rotZLast = rot.z;

		// Degrees
		zRotDeg = Mathf.DeltaAngle(0f, rot.eulerAngles.z) *  -1f;
		zPerDeg = (Mathf.Clamp (zRotDeg, zRotMinDeg, zRotMaxDeg) - zRotMinDeg) / (zRotMaxDeg - zRotMinDeg);
		if (zPerDeg < .5f + zPerZeroMove && zPerDeg > .5f - zPerZeroMove)
			zPerDeg = .5f;

		xRotDeg = Mathf.DeltaAngle (0f, rot.eulerAngles.x);
		xPerDeg = (Mathf.Clamp (xRotDeg, xRotMinDeg, xRotMaxDeg) - xRotMinDeg) / (xRotMaxDeg - xRotMinDeg);
		if (xPerDeg < .5f + xPerZeroMove && xPerDeg > .5f - xPerZeroMove)
			xPerDeg = .5f;
	}

	public void reset() {

		GvrViewer.Instance.Recenter (); //  Cardboard.SDK.Recenter ();
		rot = GvrViewer.Instance.HeadPose.Orientation; // Cardboard.SDK.HeadPose.Orientation;

		xRot = rot.x;
		xRotMax = xRot + xRotVar;
		xRotMin = xRot - xRotVar;

		zRotMax = zRotVar;
		zRotMin = zRotVar * -1.0f;

		rotXLast = rotZLast = dXLast = dZLast = 0f;
		xPer = zPer = .5f;

		xRotDeg = Mathf.DeltaAngle (0f, rot.eulerAngles.x); 
		xRotMaxDeg = xRotDeg + xRotVarDeg;
		xRotMinDeg = xRotDeg - xRotVarDeg;

		zRotMaxDeg = zRotVarDeg;
		zRotMinDeg = zRotVarDeg * -1.0f;

		xPerDeg = zPerDeg = .5f;
	}

	public float getXPer() {
		return xPer;
	}

	public float getZPer() {
		return zPer;
	}

	public float getXPerDeg() {
		return xPerDeg;
	}

	public float getZPerDeg() {
		return zPerDeg;
	}

	public float getDX() {
		return dXLast;
	}

	public float getDZ() {
		return dZLast;
	}
}