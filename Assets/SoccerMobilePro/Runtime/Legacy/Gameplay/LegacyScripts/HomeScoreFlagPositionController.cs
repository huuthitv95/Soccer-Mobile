namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "scoreFieldFlagePosition")]
public class HomeScoreFlagPositionController : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (GameManager.SharedObject ().IsFirstHalf)
			transform.position = new Vector3 (0.08f, transform.position.y, transform.position.z);
		else
			transform.position = new Vector3 (0.324f, transform.position.y, transform.position.z);
	}
}
}
