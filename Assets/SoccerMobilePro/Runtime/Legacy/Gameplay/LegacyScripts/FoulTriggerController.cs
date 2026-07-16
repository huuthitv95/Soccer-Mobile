namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "FoulTriggerController")]

public class FoulTriggerController : MonoBehaviour
{
	private BallScript ballScript;

	// Use this for initialization
	void Start ()
	{
		GameObject football = GameObject.FindGameObjectWithTag("TheSoccerBall");
		ballScript = football.GetComponent<BallScript> ();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "TheSoccerBall" && GameManager.SharedObject().opponentMadeFoul == false && GameManager.SharedObject().playerMadeFoul == false)
		{
			if(ballScript.lastOwnerTag == "Player")
			{
				GameManager.SharedObject().opponentMadeFoul = false;
				GameManager.SharedObject().playerMadeFoul = true;
			}
			else
			{
				GameManager.SharedObject().opponentMadeFoul = true;
				GameManager.SharedObject().playerMadeFoul = false;
			}
			ballScript.ownerPlayer = null;
			float z = 0f;
			if(other.gameObject.transform.position.z < 0)
				z = -37.5f;
			else
				z = 37.5f;

			GameManager.SharedObject().foulPosition = new Vector3(other.gameObject.transform.position.x,0,z);
			LegacyMatchCoreAdapter.RecordFoul(GameManager.SharedObject().gameTime);
		}
	}
}
}
