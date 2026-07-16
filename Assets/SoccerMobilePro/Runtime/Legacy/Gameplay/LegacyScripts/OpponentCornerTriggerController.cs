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
	[UnityEngine.Serialization.FormerlySerializedAs("Golie")]
	public GameObject goalkeeper;

	void Start()
	{
		GameObject football = GameObject.FindGameObjectWithTag("TheSoccerBall");
		ballScript = football.GetComponent<BallScript>();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "TheSoccerBall")
		{
			if(ballScript.lastOwnerTag != "Player")
				GameManager.SharedObject().playerGotCornerKick = true;
			else
			{
				goalkeeper.GetComponent<OpponentGoalkeeper>().enabled = false;
				goalkeeper.GetComponent<OpponentGoalkeeperKickController>().enabled = true;

				GameManager.SharedObject().playerMissedGoal = true;
			}
			ballScript.ownerPlayer = null;

			if(other.gameObject.transform.position.z < 0)
				GameManager.SharedObject().foulPosition = new Vector3(55f, 0f, -37.3f);
			else
				GameManager.SharedObject().foulPosition = new Vector3(55f, 0f, 37.3f);

			other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

			other.gameObject.transform.position = GameManager.SharedObject().foulPosition;
			LegacyMatchCoreAdapter.RecordCorner(GameManager.SharedObject().gameTime);
		}
	}
}
}
