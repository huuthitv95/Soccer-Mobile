namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "OpponentGoalMissTrigger")]

public class OpponentGoalMissTrigger : MonoBehaviour
{
	private BallScript ballScript;
	[UnityEngine.Serialization.FormerlySerializedAs("Golie")]
	public GameObject goalkeeper;
	// Use this for initialization
	void Start()
	{
		GameObject football = GameObject.FindGameObjectWithTag("TheSoccerBall");
		ballScript = football.GetComponent<BallScript>();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "TheSoccerBall")
		{
			goalkeeper.GetComponent<OpponentGoalkeeper>().enabled = false;
			goalkeeper.GetComponent<OpponentGoalkeeperKickController>().enabled = true;

			other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}
}
}
