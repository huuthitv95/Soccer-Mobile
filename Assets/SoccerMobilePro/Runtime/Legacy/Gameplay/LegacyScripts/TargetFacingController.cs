namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "pointTheTargetForTank")]
public class TargetFacingController : MonoBehaviour {

	public Transform model;  //Follow
	public Transform player;  //Target
	public Transform positionPlayer;
	// Use this for initialization
	void Start () {
		if (player == null)
			player = GameObject.Find ("BBHelicopterApache").transform;
	}

	// Update is called once per frame
	void Update () {
		transform.position = new Vector3 (positionPlayer.position.x, transform.position.y, positionPlayer.position.z);
		if(player!=null)
		{
			Vector3 tfmPosition=player.position - model.position;
//
			model.rotation = Quaternion.Slerp (model.rotation, Quaternion.LookRotation (tfmPosition), Time.deltaTime * 30);
		}
	}
}
}
