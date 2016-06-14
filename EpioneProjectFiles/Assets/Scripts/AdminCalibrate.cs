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
using UnityEngine.SceneManagement;

public class AdminCalibrate : MonoBehaviour {

	public Canvas canvas;
	public Image arrowUp;
	public Image arrowDn;
	public Text debugText;
	public Image imageProgress;
	public float steadyNum;
	public int timeHold;

	private const float ARROW_INC = .05f;

	// Arrows
	private Color cArrowUp;
	private float arrowUpMin, arrowDnMin, arrowUpMax, arrowDnMax;
	private Vector3 arrowUpVec, arrowDnVec;
	private float localScaleVal;
	private float startTime;
	private float arrowPer;

	void Awake() {
		startTime = 100000000f;
		localScaleVal = 1 / canvas.transform.localScale.x;
		arrowUpMin = arrowUp.transform.localPosition.y;
		arrowUpMax = arrowUpMin + 3f * localScaleVal;
		arrowUpVec = arrowUp.transform.localPosition;
		arrowDnMin = arrowDn.transform.localPosition.y;
		arrowDnMax = arrowDnMin - 3f * localScaleVal;
		arrowDnVec = arrowDn.transform.localPosition;
		cArrowUp = arrowUp.color;
		imageProgress.fillAmount = 0f;
	}

	// Use this for initialization
	void Start () {
		imageProgress.fillAmount = 0f;
		startTime = 100000000f;
		debugText.text = "";
	}

	// Update is called once per frame
	void Update () {

		// Arrow Movement
		if (arrowUp.transform.localPosition.y <= arrowUpMin) {
			arrowUpVec.y = arrowUpMax;
			arrowUp.transform.localPosition = arrowUpVec;
			arrowDnVec.y = arrowDnMax;
			arrowDn.transform.localPosition = arrowDnVec;
		}

		arrowUpVec.y -= ARROW_INC * localScaleVal;
		arrowUp.transform.localPosition = arrowUpVec;
		arrowDnVec.y = arrowDnMax + (arrowUpMax - arrowUpVec.y);
		arrowDn.transform.localPosition = arrowDnVec;

		arrowPer = (arrowUpVec.y - arrowUpMin) / (arrowUpMax - arrowUpMin);
		cArrowUp.a = 1f - arrowPer;
		arrowUp.color = arrowDn.color = cArrowUp;

		if (PlayerInputSingleton.Instance.getDX() < steadyNum && PlayerInputSingleton.Instance.getDZ() < steadyNum) {
			imageProgress.fillAmount = (Time.time - startTime) / timeHold;
			if (imageProgress.fillAmount >= 1f) {
				onComplete ();
			}
		} else {
			imageProgress.fillAmount = 0f;
			startTime = Time.time;
		}
	}

	private void onComplete() {
		PlayerInputSingleton.Instance.reset ();
		SceneManager.LoadScene("AdminChoose");
	}
}