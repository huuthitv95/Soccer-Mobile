namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerGolieKick")]
public class PlayerGoalkeeperKickController : MonoBehaviour
{
	[HideInInspector]
	public bool kickTheBall = false;
	public bool ballKicked = false;

	private Vector3 ballPosition;

	private GameObject football;
	public BallScript ballScript;

	// Use this for initialization
	void Start ()
	{
		ballPosition = new Vector3 (-48,0.15773f,0);

		football = GameObject.FindGameObjectWithTag("TheSoccerBall");
		ballScript = football.GetComponent<BallScript>();
	}

	// Update is called once per frame
	void Update ()
	{
		if(kickTheBall == false)
		{
			if(GetComponent<Animation>()["reposo"].enabled == false)
				GetComponent<Animation>().Play("reposo", PlayMode.StopAll);

			football.GetComponent<Rigidbody>().velocity = Vector3.zero;
			football.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			football.transform.position = ballPosition;

			transform.position = new Vector3(ballPosition.x-3, 0, ballPosition.z);
			transform.rotation = Quaternion.Euler(new Vector3(0,90,0));

			kickTheBall = true;
		}
		else
		{
			if(GetComponent<Animation>()["saque_esquina"].enabled == false)
				GetComponent<Animation>().Play("saque_esquina", PlayMode.StopAll);
			else if(GetComponent<Animation>()["saque_esquina"].enabled == true && GetComponent<Animation>()["saque_esquina"].normalizedTime < 0.5f)
				transform.Translate(Vector3.forward*Time.deltaTime*2.2f);
			else if(GetComponent<Animation>()["saque_esquina"].enabled == true && GetComponent<Animation>()["saque_esquina"].normalizedTime >= 0.7f)
			{
				kickTheBall = false;
				ballKicked = false;
				ballScript.ownerPlayer = null;
				gameObject.GetComponent<PlayerGoalkeeper>().enabled = true;
				gameObject.GetComponent<PlayerGoalkeeperKickController>().enabled = false;
			}
			else if(GetComponent<Animation>()["saque_esquina"].enabled == true && GetComponent<Animation>()["saque_esquina"].normalizedTime >= 0.5f)
			{
				if(ballKicked == false)
				{
					AudioManager.PlayKickSound ();

					ballKicked = true;
					Quaternion shotAngle = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x - 30,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z));
					football.transform.rotation = shotAngle;
					football.GetComponent<Rigidbody>().AddForce(football.transform.forward*2000, ForceMode.Impulse);
					GameManager.SharedObject().isGameReady = true;
				}
			}


		}
	}
}
}
