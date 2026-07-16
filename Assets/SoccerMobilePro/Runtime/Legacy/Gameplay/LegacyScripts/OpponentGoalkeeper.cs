namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "OpponentGolie")]
public class OpponentGoalkeeper : MonoBehaviour
{
	[UnityEngine.Serialization.FormerlySerializedAs("LeftHand")]
	public Transform leftHand;

	private GameObject football;
	public BallScript ballScript;

	private bool playStandAnimation = false;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	float timeSinceCaught = 0f;

	// Use this for initialization
	void Start ()
	{
		timeSinceCaught = Time.time;

		initialPosition = transform.position;
		initialRotation = transform.rotation;

		football = GameObject.FindGameObjectWithTag("TheSoccerBall");
		ballScript = football.GetComponent<BallScript>();
	}

	// Update is called once per frame
	void Update ()
	{
		if(GameManager.SharedObject().playerGotCornerKick || GameManager.SharedObject().playerMadeFoul || GameManager.SharedObject().opponentMadeFoul)
			return;

		if(ballScript.ownerPlayer == leftHand && Time.time - timeSinceCaught > 2)
		{
			ballScript.isKicked = false;
			ballScript.ownerPlayer = null;
			gameObject.GetComponent<OpponentGoalkeeper>().enabled = false;
			gameObject.GetComponent<OpponentGoalkeeperKickController>().enabled = true;
			return;
		}

		if(ballScript.isKicked && Vector3.Distance(transform.position,football.transform.position) <= 5f  && ballScript.ownerPlayer != leftHand)
		{
			//portero_despeje_lateral_izquierdo_raso down left
			//portero_despeje_lateral_izquierdo_alto up left
			//portero_despeje_lateral_derecho_raso down right
			//portero_despeje_lateral_derecho_alto up right
			//
			//
			if(transform.position.z - football.transform.position.z < -0.2f && football.transform.position.y < 0.5f)
			{
				if(GetComponent<Animation>()["portero_despeje_lateral_izquierdo_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_izquierdo_alto"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_alto"].enabled == false)
					GetComponent<Animation>().Play("portero_despeje_lateral_izquierdo_raso", PlayMode.StopAll);

				transform.Translate(Vector3.right*Time.deltaTime*-2f);
			}
			else if(transform.position.z - football.transform.position.z < -0.2f && football.transform.position.y >= 0.5f)
			{
				if(GetComponent<Animation>()["portero_despeje_lateral_izquierdo_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_izquierdo_alto"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_alto"].enabled == false)
					GetComponent<Animation>().Play("portero_despeje_lateral_izquierdo_alto", PlayMode.StopAll);

				transform.Translate(Vector3.right*Time.deltaTime*-2f);
			}
			else if(transform.position.z - football.transform.position.z > 0.2f && football.transform.position.y < 0.5f)
			{
				if(GetComponent<Animation>()["portero_despeje_lateral_izquierdo_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_izquierdo_alto"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_alto"].enabled == false)
					GetComponent<Animation>().Play("portero_despeje_lateral_derecho_raso", PlayMode.StopAll);

				transform.Translate(Vector3.right*Time.deltaTime*2f);
			}
			else if(transform.position.z - football.transform.position.z > 0.2f && football.transform.position.y >= 0.5f)
			{
				if(GetComponent<Animation>()["portero_despeje_lateral_izquierdo_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_izquierdo_alto"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_raso"].enabled == false && GetComponent<Animation>()["portero_despeje_lateral_derecho_alto"].enabled == false)
					GetComponent<Animation>().Play("portero_despeje_lateral_derecho_alto", PlayMode.StopAll);

				transform.Translate(Vector3.right*Time.deltaTime*2f);
			}

			if(Vector3.Distance(football.transform.position, transform.position) <= 2.5f)
			{
				GameManager.SharedObject().isGameReady = false;
				ballScript.SetOwner(leftHand);
				timeSinceCaught = Time.time;
				playStandAnimation = true;
				ballScript.isKicked = false;
			}
		}
		else if((football.transform.position.x > 43 || football.transform.position.x > transform.position.x) && ballScript.ownerPlayer != leftHand) // run towards ball
		{
			if(Vector3.Distance(transform.position,football.transform.position) > 1f && GetComponent<Animation>()["portero_parada_frontal_rasa"].enabled == false)
			{
				if(GetComponent<Animation>()["corriendo"].enabled == false)
					GetComponent<Animation>().Play("corriendo", PlayMode.StopAll);

				Quaternion rot = Quaternion.LookRotation((football.transform.position - transform.position).normalized);
				rot.x = 0;
				transform.rotation = rot;
				transform.Translate(Vector3.forward*Time.deltaTime*5);
			}
			else
			{
				if(GetComponent<Animation>()["portero_parada_frontal_rasa"].enabled == false)
				{
					GetComponent<Animation>().Play("portero_parada_frontal_rasa", PlayMode.StopAll);
					if(Vector3.Distance(transform.position,football.transform.position) < 4.5f)
					{
						timeSinceCaught = Time.time;
						GameManager.SharedObject().isGameReady = false;
						playStandAnimation = true;
						ballScript.isKicked = false;
						ballScript.SetOwner(leftHand);
					}
				}
				else if(GetComponent<Animation>()["portero_parada_frontal_rasa"].enabled == true/* && animation["portero_parada_frontal_rasa"].normalizedTime>=0.1f*/)
				{
					if(Vector3.Distance(transform.position,football.transform.position) < 2.5f)
					{
						timeSinceCaught = Time.time;
						GameManager.SharedObject().isGameReady = false;
						ballScript.SetOwner(leftHand);
						playStandAnimation = true;
						ballScript.isKicked = false;
					}
				}
			}
		}
		else if(football.transform.position.x > 43f && ballScript.ownerPlayer == leftHand && playStandAnimation)
		{
			playStandAnimation = false;
			if(GetComponent<Animation>()["portero_levanta_balon"].enabled == false)
				GetComponent<Animation>().Play("portero_levanta_balon", PlayMode.StopAll);

			ballScript.isKicked = false;
		}
		else if(football.transform.position.x >34f)
		{
			if(transform.position.z - football.transform.position.z < -1 && football.transform.position.z > -3.4f && football.transform.position.z < 3.4f) // ball to left side
			{
				if(GetComponent<Animation>()["portero_guardia_izquierda"].enabled == false)
					GetComponent<Animation>().Play("portero_guardia_izquierda", PlayMode.StopAll);

				transform.Translate(Vector3.right*Time.deltaTime*-2f);
			}
			else if(transform.position.z - football.transform.position.z > 1f && football.transform.position.z > -3.4f && football.transform.position.z < 3.4f)// ball to right side
			{
				if(GetComponent<Animation>()["portero_guardia_derecha"].enabled == false)
					GetComponent<Animation>().Play("portero_guardia_derecha", PlayMode.StopAll);

				transform.Translate(Vector3.right*Time.deltaTime*2f);
			}
			else if(GetComponent<Animation>()["portero_levanta_balon"].enabled == false)// ball in front
			{
				if(GetComponent<Animation>()["portero_guardia_reposo"].enabled == false)
					GetComponent<Animation>().Play("portero_guardia_reposo", PlayMode.StopAll);
			}
		}
		else
		{
			if(GetComponent<Animation>()["reposo"].enabled == false)
				GetComponent<Animation>().Play("reposo", PlayMode.StopAll);

			transform.rotation = initialRotation;
			transform.position = initialPosition;
		}

		Vector3 pos = transform.position;
		pos.y = 0f;
		transform.position = pos;
	}
}
}
