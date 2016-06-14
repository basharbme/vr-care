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

public class HUDDodgeScore : HUDBaseCanvas {

	public static string HIDE = "HUDBaseCanvasHide";
	public static string END = "HUDBaseCanvasEnd";

	public Image imgRec;
	public Text textRecord;

	private Animator anim;
	int enterHash = Animator.StringToHash("EnterScore");
	int exitHash = Animator.StringToHash("ExitScore");

	private bool bFinished;
	private float waitSad = 1f;

	private AudioSource happy;
	private AudioSource sad;
	private AudioSource[] aSources;

	protected override void Awake() 
	{ 
		anim = GetComponent<Animator>();
		base.Awake ();
		textRecord.text = "";
		bFinished = true;
		aSources = GetComponents<AudioSource>(); happy = aSources[0]; sad = aSources[1];
	}

	public void setFinished (bool bFinish) {
		bFinished = bFinish;
		// anim.SetBool ("scoreCheck", bFinish);
		if (bFinished)
			happy.Play ();
		else
			StartCoroutine(playSad());
	}

	private IEnumerator playSad() {
		yield return new WaitForSeconds (waitSad);
		sad.Play ();
	}
		
	protected override void showOverride() {
		anim.SetTrigger (enterHash);
	}

	protected override void hideOverride() {
		anim.SetTrigger (exitHash);
		EventManager.TriggerEvent (HIDE);
	}

	protected override void endOverride() {
		EventManager.TriggerEvent (END);
	}

	public void setRecord (int distance) {
		textRecord.text = distance.ToString() + "m"; 
	}
}