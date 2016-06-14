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
using UnityEngine.SceneManagement;

public class AdminChoose : MonoBehaviour {

	public Canvas canvas;
	public Text debugText;
	public Image imageFlow, imageDodge;
	public Text instructText;
	public Image instructFlow;
	public Image instructDodge;
	public Image arrowUp;
	public Image arrowDn;
	public float steadyNum;
	public float timeHold;

	private const float PER_UNTIL_CHANGE = .1f;
	private const float ARROW_INC = .05f;
	private const float START_WAIT = 2f;
	private const float MOVE_SPEED = 1f;
	private const float CANVAS_MOVE_MASK = 34f;

	private CanvasGroup cG;
	private Color cInstructF, cInstructD, cArrowUp;
	private float distUntilChange;
	private float startTime;

	// Movement variables
	private bool bMove;
	private bool bHoldFlow, bHoldDodge;
	private Vector3 canvasMoveVec, arrowUpVec, arrowDnVec;

	// Arrow Looping
	private float arrowUpMin, arrowDnMin, arrowUpMax, arrowDnMax;
	private float localScaleVal;
	private float arrowPer; 

	void Awake() {
		startTime = 100000000f;
		bMove = false; 
		bHoldFlow = bHoldDodge = false;
		canvasMoveVec = new Vector3 (0f, 0f, 30f);

		cInstructF = instructFlow.color;
		cInstructF.a = 0f;
		cInstructD = instructDodge.color;
		cInstructD.a = 0f;

		instructFlow.color = cInstructF;
		instructDodge.color = cInstructD;

		cG = instructText.GetComponent<CanvasGroup> ();
		distUntilChange = PER_UNTIL_CHANGE * CANVAS_MOVE_MASK;

		localScaleVal = 1 / canvas.transform.localScale.x;

		arrowUpMin = arrowUp.transform.localPosition.y;
		arrowUpMax = arrowUpMin + 3f * localScaleVal;
		arrowUpVec = arrowUp.transform.localPosition;
		arrowDnMin = arrowDn.transform.localPosition.y;
		arrowDnMax = arrowDnMin - 3f * localScaleVal;
		arrowDnVec = arrowDn.transform.localPosition;
		cArrowUp = arrowUp.color;
	}

	// Use this for initialization
	void Start () {
		imageFlow.fillAmount = imageDodge.fillAmount = 0f;
		startTime = 100000000f;
		debugText.text = "";
		StartCoroutine(StartWait());
	}

	IEnumerator StartWait() {
		yield return new WaitForSeconds (START_WAIT);
		bMove = true;
	}

	// Update is called once per frame
	void Update () {
		if (arrowUp.transform.localPosition.y >= arrowUpMax) {
			//arrowInc *= -1f;
			arrowUpVec.y = arrowUpMin;
			arrowUp.transform.localPosition = arrowUpVec;
		}

		if (arrowDn.transform.localPosition.y <= arrowDnMax) {
			arrowDnVec.y = arrowDnMin;
			arrowDn.transform.localPosition = arrowDnVec;
		}

		arrowUpVec.y += ARROW_INC * localScaleVal;
		arrowUp.transform.localPosition = arrowUpVec;
		arrowDnVec.y -= ARROW_INC * localScaleVal;
		arrowDn.transform.localPosition = arrowDnVec;
		arrowPer = (arrowUpVec.y - arrowUpMin) / (arrowDnMax - arrowDnMin);
		cArrowUp.a = 1f - Mathf.Abs(arrowPer);
		arrowUp.color = arrowDn.color = cArrowUp;

		if (bMove) {
			canvasMoveVec.y = canvasMoveVec.y + (PlayerInputSingleton.Instance.getXPerDeg () - .5f) * MOVE_SPEED;
			if (canvasMoveVec.y <= CANVAS_MOVE_MASK * -1) {
				if (!bHoldFlow)
					startTime = Time.time;
				bHoldFlow = true;
				bHoldDodge = false;
				canvasMoveVec.y = CANVAS_MOVE_MASK * -1;
			} else if (canvasMoveVec.y >= CANVAS_MOVE_MASK) {
				if (!bHoldDodge)
					startTime = Time.time;
				bHoldDodge = true;
				bHoldFlow = false;
				canvasMoveVec.y = CANVAS_MOVE_MASK;
			} else {
				bHoldFlow = bHoldDodge = false;
			}
			canvas.transform.position = canvasMoveVec;

			float perAlpha = 0f;
			if (Mathf.Abs(canvasMoveVec.y) >= distUntilChange) 
				perAlpha = Mathf.Abs ((canvasMoveVec.y - distUntilChange) / (CANVAS_MOVE_MASK - distUntilChange));
			if (canvasMoveVec.y < distUntilChange * -1) {
				cInstructF.a = perAlpha;
				instructFlow.color = cInstructF;
				cInstructD.a = 0f;
				instructDodge.color = cInstructD;
				cG.alpha = 1f - perAlpha;
			} else if (canvasMoveVec.y > distUntilChange) {
				cInstructF.a = 0f;
				instructFlow.color = cInstructF;
				cInstructD.a = perAlpha;
				instructDodge.color = cInstructD;
				cG.alpha = 1f - perAlpha;
			} else {
				cInstructF.a = 0f;
				instructFlow.color = cInstructF;
				cInstructD.a = 0f;
				instructDodge.color = cInstructD;
				cG.alpha = 1f - perAlpha;
			}
		}

		if (bHoldFlow) {
			imageFlow.fillAmount = (Time.time - startTime) / timeHold;
			if (imageFlow.fillAmount >= 1f) {
				onFlow ();
			}
		} else if (bHoldDodge) {
			imageDodge.fillAmount = (Time.time - startTime) / timeHold;
			if (imageDodge.fillAmount >= 1f) {
				onDodge ();
			}
		} else {
			imageFlow.fillAmount = imageDodge.fillAmount = 0f;
		}
	}

	private void onFlow() {
		SceneManager.LoadScene("Flow");
		imageFlow.fillAmount = imageDodge.fillAmount = 0f;
	}

	private void onDodge() {
		SceneManager.LoadScene("Dodge");
		imageFlow.fillAmount = imageDodge.fillAmount = 0f;
	}
}