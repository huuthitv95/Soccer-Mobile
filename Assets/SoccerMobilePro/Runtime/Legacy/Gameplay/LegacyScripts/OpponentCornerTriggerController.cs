namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "OCornerTriggerController")]
public class OpponentCornerTriggerController : MonoBehaviour
{
	private BallScript ballScript;
	public GameObject Golie;

	void Start()
	{
		GameObject FootBall = GameObject.FindGameObjectWithTag("TheSoccerBall");
		ballScript = FootBall.GetComponent<BallScript>();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "TheSoccerBall")
		{
			if(ballScript.lastOwnerTag != "Player")
				GameManager.SharedObject().PlayerGotCornerKick = true;
			else
			{
				Golie.GetComponent<OpponentGoalkeeper>().enabled = false;
				Golie.GetComponent<OpponentGoalkeeperKickController>().enabled = true;

				GameManager.SharedObject().PlayerMissedGoal = true;
			}
			ballScript.ownerPlayer = null;

			if(other.gameObject.transform.position.z < 0)
				GameManager.SharedObject().foulPosition = new Vector3(55f, 0f, -37.3f);
			else
				GameManager.SharedObject().foulPosition = new Vector3(55f, 0f, 37.3f);

			other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

			other.gameObject.transform.position = GameManager.SharedObject().foulPosition;
			LegacyMatchCoreAdapter.RecordCorner(GameManager.SharedObject().GameTime);
		}
	}
}
}
