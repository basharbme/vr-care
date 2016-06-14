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
using UnityEngine.UI;
using System.Collections;

public class HUDBaseCanvas: MonoBehaviour {

	protected Canvas baseCanvas;
	protected bool bHiding;
	protected bool inited;
	protected float timeToShow;
	protected float timeToExit = 1.0f;
	protected bool disable;

	protected virtual void Awake() { 
		init ();
	}

	protected void init() {
		baseCanvas = GetComponent<Canvas> ();
		baseCanvas.enabled = false;
		bHiding = false;
		inited = true;
		disable = true;
		timeToShow = 0.0f;
	}

	// Use this for initialization
	protected virtual void Start () {
		
	}

	// Update is called once per frame
	protected virtual void Update () {

	}

	protected IEnumerator HUDDelays(bool exit = false) {
		if (!exit) {
			yield return new WaitForSeconds (timeToShow);
			hide ();
		} else {
			yield return new WaitForSeconds (timeToExit);
			end ();
		}
	}

	public void show (float timeShow = 0.0f) {
		if (!inited) {
			init ();
		}
		baseCanvas.enabled = true;
		bHiding = false;
		showOverride ();

		if (timeShow > 0.0f) {
			timeToShow = timeShow;
			StartCoroutine(HUDDelays());
		}
	}

	protected virtual void showOverride() {

	}

	public void hide(bool tDisable = true) {
		disable = tDisable;
		bHiding = true;
		hideOverride ();
		StartCoroutine(HUDDelays(true));
	}

	protected virtual void hideOverride() {

	}

	public void end() {
		bHiding = false;
		if (disable) 
			baseCanvas.enabled = false;
		endOverride ();
	}

	protected virtual void endOverride() {

	}

	public bool enabledHUD() {
		return baseCanvas.enabled;
	}

	public bool hiding() {
		return bHiding;
	}
}