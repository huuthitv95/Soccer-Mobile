namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "HalfInfoController")]

public class HalfInfoController : MonoBehaviour {
	GameManager manager;
	// Use this for initialization
	void Start () {
		manager = GameManager.SharedObject();
	}

	// Update is called once per frame
	void Update () {
		if(manager.IsFirstHalf) ///*** in order to switch teamNames and scores
			LegacyGuiUtility.GetOrAddGUIText(gameObject).text = "1st";
		else
			LegacyGuiUtility.GetOrAddGUIText(gameObject).text = "2nd";
	}
}
}
