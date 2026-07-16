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
	[UnityEngine.Serialization.FormerlySerializedAs("InitialPositonTransform")]
	public Transform initialPositionTransform;
	[UnityEngine.Serialization.FormerlySerializedAs("SecondaryPositonTransform")]
	public Transform secondaryPositionTransform;
	private Vector3 initialPosition, secondaryPosition;

	private AiStrikerController playerScript;

	// Use this for initialization
	void Start () {
		playerScript = initialPositionTransform.GetComponent<AiStrikerController> ();
		initialPosition = initialPositionTransform.position;
		secondaryPosition = secondaryPositionTransform.position;
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		if(PlayerPosition.playerTurn)
			playerScript.initialPosition = initialPosition;
		else
			playerScript.initialPosition = secondaryPosition;

		if(PlayerPosition.playerTurn == false && Vector3.Distance(transform.position, secondaryPosition) < 1f)
		{
			GameManager.SharedObject().isGameReady = true;
		}
	}
}
}
