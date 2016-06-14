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

public class HUDDodge : HUDBaseCanvas {

	public static string HIDE = "HUDBaseCanvasHide";
	public static string END = "HUDBaseCanvasEnd";

	public Image shield;
	public Image shield1;
	public Image shield2;
	public Image shield3;
	public Image shield4;
	public Image flash;
	public Text textDistance;
	public Image target;
	public Text countdownText;

	private Animator anim;
	private int enterHash = Animator.StringToHash("EnterHUD");
	private int exitHash = Animator.StringToHash("ExitHUD");

	protected override void Awake() 
	{
		anim = GetComponent<Animator>();
		base.Awake();
		if (!flash.gameObject.activeSelf)
			flash.gameObject.SetActive(true);
		flash.CrossFadeAlpha (0.0f, 0.0f, false);
		textDistance.text = "0m"; 
		countdownText.text = "";
		target.CrossFadeAlpha (0.0f, 0.0f, false);
	}

	public void setDistance (int distance) {
		textDistance.text = distance.ToString() + "m";
	}

	public void collision(int numCollisions) {
		if (numCollisions == 1) {
			shield1.CrossFadeAlpha (0.0f, 1.0f, false);
		} else if (numCollisions == 2) {
			shield2.CrossFadeAlpha (0.0f, 1.0f, false);
		} else if (numCollisions == 3) {
			shield3.CrossFadeAlpha (0.0f, 1.0f, false);
		} else if (numCollisions == 4) {
			shield4.CrossFadeAlpha (0.0f, 1.0f, false);
		}
		flash.CrossFadeAlpha (0.5f, 0.0f, false);
		flash.CrossFadeAlpha (0.0f, 0.3f, false);
	}

	protected override void showOverride() {
		anim.SetTrigger (enterHash);
	}

	protected override void hideOverride() {
		anim.SetTrigger (exitHash);
	}

	protected override void endOverride() {
	}

	public void countdown(string countdownStr) {
		countdownText.text = countdownStr;
		if (countdownStr == "") {
			target.CrossFadeAlpha (1.0f, .5f, false);
		}
	}
}