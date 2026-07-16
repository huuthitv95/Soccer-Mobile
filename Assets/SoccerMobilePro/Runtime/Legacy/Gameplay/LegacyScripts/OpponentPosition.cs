namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "OpponentPosition")]

public class OpponentPosition : MonoBehaviour
{
	public Transform InitialPositonTransform, SecondaryPositonTransform;
	private Vector3 InitialPosition, SecondaryPosition;

	private AiStrikerController playerScript;

	// Use this for initialization
	void Start () {
		playerScript = InitialPositonTransform.GetComponent<AiStrikerController> ();
		InitialPosition = InitialPositonTransform.position;
		SecondaryPosition = SecondaryPositonTransform.position;
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		if(PlayerPosition.PlayerTurn)
			playerScript.InitialPosition = InitialPosition;
		else
			playerScript.InitialPosition = SecondaryPosition;

		if(PlayerPosition.PlayerTurn == false && Vector3.Distance(transform.position, SecondaryPosition) < 1f)
		{
			GameManager.SharedObject().IsGameReady = true;
		}
	}
}
}
