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

public class PlanetHalo : MonoBehaviour {

	private GameObject cam;
	private SpriteRenderer sArrow;
	private SpriteRenderer sHalo;
	private Canvas sHaloFill;
	private Image fill;
	private GameObject lookAtHolder;
	private Bounds planetBounds;
	private float maxScale, minScale, curScale;
	private float scaleInc;
	private Vector3 arrowPos, vecScale;

	// Use this for initialization
	void Awake () {

		scaleInc = .005f;
		cam = GameObject.FindGameObjectWithTag ("MainCamera");

		sArrow = this.transform.FindChild("FlowPlanetArrow").gameObject.GetComponent<SpriteRenderer>();
		sArrow.transform.localRotation = Quaternion.AngleAxis(180.0f, new Vector3 (1.0f, 0.0f, 0.0f));
		sArrow.transform.localPosition = new Vector3 (0.0f, 0f, 0.0f);

		sHalo = this.transform.FindChild("FlowPlanetHalo").gameObject.GetComponent<SpriteRenderer>();
		sHalo.transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);

		sHaloFill = this.transform.FindChild("Canvas").gameObject.GetComponent<Canvas>();
		sHaloFill.transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);

		fill = sHaloFill.transform.FindChild("Image").gameObject.GetComponent<Image> ();
		fill.fillAmount = 0f;

		minScale = maxScale = 1f;
		transform.LookAt (cam.transform);
	}

	void Start() {
		sHaloFill.transform.LookAt (cam.transform);
	}

	// Update is called once per frame
	void Update () {
		transform.LookAt (cam.transform);

		if (curScale >= maxScale || curScale <= minScale) {
			scaleInc *= -1f;
		}
		curScale += scaleInc;
		vecScale = new Vector3 (curScale, curScale, 1f);
		sHalo.transform.localScale = vecScale;
		sHaloFill.transform.localScale = vecScale;
		arrowPos.y -= scaleInc * 2f;
		sArrow.transform.localPosition = arrowPos;
	}

	public void setHolder(GameObject lookAtH) {
		lookAtHolder = lookAtH;
		sHaloFill.transform.SetParent (lookAtHolder.transform, false);
	}

	public void setFill (float fillNum) {
		fill.fillAmount = fillNum;
	}

	public float getFill () {
		return fill.fillAmount;
	}

	public void SetPlanetBounds(Bounds pBounds) {
		planetBounds = pBounds;
		maxScale = planetBounds.size.x / sHalo.bounds.size.x * 2f;
		minScale = maxScale * .85f;
		curScale = maxScale;
		vecScale = new Vector3 (maxScale, maxScale, 1f);
		sHalo.transform.localScale = vecScale;
		sHaloFill.transform.localScale = vecScale;

		arrowPos = new Vector3 (
			sArrow.transform.localPosition.x, 
			sHalo.bounds.size.x * .38f * -1f,
			//planetBounds.size.x * maxScale * .75f * -1f, 
			sArrow.transform.localPosition.z
		);
		sArrow.transform.localPosition = arrowPos;
	}

	public float boundsX() {
		return sHalo.bounds.size.x;
	}
}