namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "GUI_MainMenu")]
public class LegacyMainMenuGui : MonoBehaviour
{
	public GameObject quitDialog;
//	public Texture background;
//	public GUIStyle playButton, moreButton, rateButton;

	// Use this for initialization
	void Start ()
	{
		AudioManager.PlayBackgroundMusic ();
//		audio.PlayOneShot (whistle);

	}

	void Update()
	{

		if(SoccerInput.PausePressedThisFrame)
		{
			quitDialog.SetActive(!quitDialog.activeInHierarchy);
			if(quitDialog.activeSelf)
				AudioListener.volume=0;
			else
			{
				if(AudioManager.isMusicOn)
					AudioListener.volume=1;
			}
		}
	}

//	void OnGUI ()
//	{
//		GUI.DrawTexture (new Rect(0f,0f,Screen.width,Screen.height), background);
//
//		float y = Screen.height / 2 - GetValue (155f) / 2 - GetValue (155f);
//
//		if(GUI.Button(new Rect(0f,y,GetValue(606f),GetValue(155f)),"",playButton))
//		{
//
//		}
//
//		y += GetValue (155f);
//		if(GUI.Button(new Rect(0f,y,GetValue(606f),GetValue(155f)),"",moreButton))
//		{
//
//		}
//
//		y += GetValue (155f);
//		if(GUI.Button(new Rect(0f,y,GetValue(606f),GetValue(155f)),"",rateButton))
//		{
//
//		}
//	}
//
//	float GetValue(float value)
//	{
//		return value * Screen.height / 1024f;
//	}
}
}
