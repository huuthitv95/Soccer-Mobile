namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "OGTController")]
public class OpponentGoalTriggerController : MonoBehaviour
{
	[UnityEngine.Serialization.FormerlySerializedAs("Golie")]
	public GameObject goalkeeper;
	public BallScript ballScript;

	float lastTriggerTime = 0f;

	void Start()
	{
		ballScript = GameObject.FindGameObjectWithTag("TheSoccerBall").GetComponent<BallScript>();
	}

	void StartPlay()
	{
		GameManager.SharedObject ().isGameReady = true;
	}

	void OnTriggerEnter(Collider other)
	{
		Invoke ("Reset",1.5f);
		AudioManager.PlayOnGoalRoar();
	}
	void Reset()
	{
		if(Time.time - lastTriggerTime > 1)
		{
			GoalCelebrationManager.PlayCeleberation(0);
			ballScript.isKicked = false;

			lastTriggerTime = Time.time;
			GameManager.SharedObject().playerTeamGoals += 1;
			GameManager.SharedObject().isGameReady = false;
			PlayerPosition.playerTurn = false;
			//goalkeeper.GetComponent<OpponentGoalkeeper>().enabled = false;
			//goalkeeper.GetComponent<OpponentGoalkeeperKickController>().enabled = true;

			ballScript.PlaceOnInitialPositon();
//			AudioManager.PlayOnGoalRoar();

			Invoke("StartPlay",5f);
//			AudioManager.PlayOnGoalRoar();
		}
	}
}
}
