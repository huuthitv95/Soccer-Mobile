namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerPosition")]

public class PlayerPosition : MonoBehaviour
{
	public GUIStyle passButtonStyle;

	public static bool playerTurn = true;

	[UnityEngine.Serialization.FormerlySerializedAs("InitialPositonTransform")]
	public Transform initialPositionTransform;
	[UnityEngine.Serialization.FormerlySerializedAs("SecondaryPositonTransform")]
	public Transform secondaryPositionTransform;
	private Vector3 initialPosition, secondaryPosition;

	private Player playerScript;

	public Transform passingPlayer;
	GameObject ball;

	void Start ()
	{
		ball = GameObject.FindGameObjectWithTag("TheSoccerBall");
		playerScript = initialPositionTransform.GetComponent<Player> ();
		initialPosition = initialPositionTransform.position;
		secondaryPosition = secondaryPositionTransform.position;
	}

	void Update ()
	{
		if(playerTurn)
			playerScript.initialPosition = initialPosition;
		else
			playerScript.initialPosition = secondaryPosition;
	}

	void OnGUI()
	{
		if(!PauseController.isPaused)
		{


		if(playerTurn && GameManager.SharedObject().isGameReady == false && Vector3.Distance(transform.position,ball.transform.position)<1.5f)
		{
			if(GUI.Button(new Rect (Screen.width - GetValue(150), Screen.height - GetValue(150) - GetValue(130), GetValue(110), GetValue(110)),"",passButtonStyle))
			{
				Vector3 direction = (passingPlayer.position-ball.transform.position).normalized;

				ball.GetComponent<Rigidbody>().AddForce(direction*1200, ForceMode.Impulse);

				AudioManager.PlayResumeWhistle();
				GameManager.SharedObject().isGameReady = true;
			}
		}
		}
	}

	float GetValue(float value)
	{
		return value * Screen.height/640f;
	}
}
}
