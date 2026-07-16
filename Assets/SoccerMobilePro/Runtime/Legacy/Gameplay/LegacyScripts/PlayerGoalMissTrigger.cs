namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerGoalMissTrigger")]

public class PlayerGoalMissTrigger : MonoBehaviour
{
	private BallScript ballScript;
	public GameObject Golie;
	// Use this for initialization
	void Start()
	{
		GameObject FootBall = GameObject.FindGameObjectWithTag("TheSoccerBall");
		ballScript = FootBall.GetComponent<BallScript>();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "TheSoccerBall")
		{
			Golie.GetComponent<PlayerGoalkeeper>().enabled = false;
			Golie.GetComponent<PlayerGoalkeeperKickController>().enabled = true;

			other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}
}
}
