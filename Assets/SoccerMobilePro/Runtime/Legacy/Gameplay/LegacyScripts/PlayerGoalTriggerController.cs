namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PGTController")]
public class PlayerGoalTriggerController : MonoBehaviour
{
	float lastTriggerTime = 0f;
	public GameObject Golie;
	public BallScript ballScript;

	public GameObject starterPlayer;

	void Start()
	{
		ballScript = GameObject.FindGameObjectWithTag("TheSoccerBall").GetComponent<BallScript>();
	}

//	void StartPlay()
//	{
//		GameManager.SharedObject().IsGameReady = true;
//	}

	void OnTriggerEnter(Collider other)
	{
		if(Time.time - lastTriggerTime > 5)
		{

			GoalCelebrationManager.PlayCeleberation(1);


			ballScript.isKicked = false;

			lastTriggerTime = Time.time;
			GameManager.SharedObject().opponentTeamGoals += 1;
			GameManager.SharedObject().IsGameReady = false;
			PlayerPosition.PlayerTurn = true;
//			Golie.GetComponent<PlayerGoalkeeper>().enabled = false;
//			Golie.GetComponent<PlayerGoalkeeperKickController>().enabled = true;

			ballScript.PlaceOnInitialPositon();
			starterPlayer.GetComponent<PlayerPosition>().enabled = true;
			AudioManager.PlayOnGoalRoar();

//			Invoke("StartPlay",5f);
		}
	}
}
}
