namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "flag1script")]
public class PrimaryTeamFlagController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		LegacyGuiUtility.GetOrAddGUITexture(gameObject).texture = GameManager.SharedObject ().playerTeamFlag;
	}

	// Update is called once per frame
	void Update () {

	}
}
}
