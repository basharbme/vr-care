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

public class AdminController : MonoBehaviour {

	public Canvas introScreen;
	public Slider SliderVol, SliderBri;
	public Text SliderVolText, SliderBriText;
	public Image fade;
	public GameObject aboutPanel, resetPanel;

	private float sliderVolVal, sliderBriVal;
	private bool bVolOpen, bBriOpen;
	private Image fadeImage;
	private Color fadeColor;

	private bool loadScene;

	void Awake() {
		bVolOpen = bBriOpen = false;
		if (aboutPanel.activeSelf)
			aboutPanel.SetActive (false);
		if (resetPanel.activeSelf)
			resetPanel.SetActive (false);
	}

	// Use this for initialization
	void Start () {
		SliderVol.gameObject.SetActive (false);
		sliderVolVal = PlayerPrefs.GetFloat("GlobalVol", 1.0f);
		SliderVol.value = sliderVolVal;
		setSliderVolVal (sliderVolVal);

		fadeImage = fade.GetComponent<Image> ();
		SliderBri.gameObject.SetActive (false);
		sliderBriVal = PlayerPrefs.GetFloat("GlobalFade", 1.0f);
		SliderBri.value = sliderBriVal;
		setSliderBriVal (sliderBriVal);

		loadScene = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void enterFlow () {
		if (!loadScene) {
			loadScene = true;
			PlayerPrefs.SetFloat ("GlobalVol", sliderVolVal);
			PlayerPrefs.SetFloat ("GlobalFade", sliderBriVal);
			loadCalibration ();
		}
	}

	private void loadCalibration() {
		SceneManager.LoadScene("AdminCalibrate");
	}

	public void openAbout () {
		aboutPanel.SetActive (true);
	}

	public void closeAbout() {
		aboutPanel.SetActive (false);
	}

	public void openReset () {
		resetPanel.SetActive (true);
	}

	public void confirmReset() {
		PlayerPrefs.SetInt ("highScoreLevel0", 0);
		PlayerPrefs.SetInt ("highScoreLevel1", 0);
		closeReset ();
	}

	public void closeReset() {
		resetPanel.SetActive (false);
	}

	public void openVolume () {
		if (!bVolOpen) {
			bVolOpen = true;
			SliderVol.gameObject.SetActive (true);
		} else {
			bVolOpen = false;
			SliderVol.gameObject.SetActive (false);
		}
	}

	public void openBri () {
		if (!bBriOpen) {
			bBriOpen = true;
			SliderBri.gameObject.SetActive (true);
		} else {
			bBriOpen = false;
			SliderBri.gameObject.SetActive (false);
		}
	}

	public void onVolChanged (float volValue) {
		setSliderVolVal (volValue);
	}

	public void onBriChanged (float briValue) {
		setSliderBriVal (briValue);
	}

	private void setSliderVolVal (float volValue) {
		sliderVolVal = volValue;
		SliderVolText.text = Mathf.Round (volValue * 100f) + "%";
		AudioListener.volume = sliderVolVal;
	}

	private void setSliderBriVal (float briValue) {
		sliderBriVal = briValue;
		SliderBriText.text = Mathf.Round (briValue * 100f) + "%";
		if (briValue < 1f && !fade.gameObject.activeSelf)
			fade.gameObject.SetActive (true);
		fadeColor = fadeImage.color;
		fadeColor.a = 1f - sliderBriVal;
		fadeImage.color = fadeColor;
	}
}