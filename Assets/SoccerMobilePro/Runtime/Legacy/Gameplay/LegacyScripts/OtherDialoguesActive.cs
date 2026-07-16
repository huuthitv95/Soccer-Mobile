namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "OtherDialoguesActive")]

public class OtherDialoguesActive : MonoBehaviour {
	public GameObject matchCompleted,halfCompleted;
	public bool isOtherDialogueActive;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update ()
	{
		if(matchCompleted==null||halfCompleted==null)
		{
			isOtherDialogueActive=true;
		}
		else
			isOtherDialogueActive=false;
	}
}
}
