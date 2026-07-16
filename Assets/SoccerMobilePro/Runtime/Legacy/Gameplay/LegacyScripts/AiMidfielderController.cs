namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
 using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "AI_MidfielderScript")]
public class AiMidfielderController : MonoBehaviour
{
	private Transform[] midfielders;
	private Transform[] opponents;

	public Transform goal;
	private int moveSpeed = 5;
	private Vector3 initialPosition = Vector3.zero;
	private Vector3 targetPosition = Vector3.zero;

	private GameObject football;
	private BallScript footballScript;

	static bool position1Available = true;
	static bool position2Available = true;
	static bool position3Available = true;
	static bool position4Available = true;

	private int zOffset = 0;
	private int xOffset = 0;

	private float progress = 0;

	private float timeToPass = 0;
	private float waitForPass = 0;

	[HideInInspector]
	public bool isMoving = false;
	private float lastTime = 0;

	private float attackTime = 0;
	private bool attack = false;

	private Vector2 GetForceFrom(Vector3 fromPos, Vector3 toPos)
	{
		return (new Vector2(toPos.x, toPos.y) - new Vector2(fromPos.x, fromPos.y))*football.GetComponent<Rigidbody>().mass;
	}
	// Use this for initialization
	void Start ()
	{
		/*AtPosition1 = false;
		AtPosition2 = false;
		AtPosition3 = false;*/

		initialPosition = transform.position;
		targetPosition = transform.position;

		football = GameObject.FindGameObjectWithTag("TheSoccerBall");
		footballScript = football.GetComponent<BallScript> ();

		if(position1Available)
		{
			xOffset = 5;
			zOffset = 15;
			position1Available = false;
		}
		else if(position2Available)
		{
			xOffset = 5;
			zOffset = -15;
			position2Available = false;
		}
		else if(position3Available)
		{
			xOffset = -5;
			zOffset = -15;
			position3Available = false;
		}
		else if(position4Available)
		{
			xOffset = -5;
			zOffset = 15;
			position3Available = false;
		}

		GameObject[] playersT = GameObject.FindGameObjectsWithTag("AIMidfiielder");
		GameObject[] opponentsT = GameObject.FindGameObjectsWithTag("Player");

		midfielders = new Transform[playersT.Length];
		opponents = new Transform[opponentsT.Length];

		int i = 0;
		for(i = 0; i < playersT.Length; i++)
			midfielders[i] = playersT[i].transform;

		for(i = 0; i < opponentsT.Length; i++)
			opponents[i] = opponentsT[i].transform;
	}

	bool TeamHasTheBall()
	{
		return (footballScript.ownerPlayer && footballScript.ownerPlayer.gameObject.name == transform.gameObject.name);
	}

	private void MoveForward()
	{
		targetPosition = football.transform.position;


		if(football.transform.position.x < 30 && football.transform.position.x > -30)
		{
			if(TeamHasTheBall())
				targetPosition = new Vector3(football.transform.position.x-xOffset,0,((football.transform.position.z+zOffset < 37 && football.transform.position.z+zOffset > -37)?football.transform.position.z+zOffset:targetPosition.z));
			else
				targetPosition = new Vector3(football.transform.position.x+xOffset,0,((football.transform.position.z+zOffset < 37 && football.transform.position.z+zOffset > -37)?football.transform.position.z+zOffset:targetPosition.z));
		}
		else
			targetPosition = initialPosition;

		targetPosition.y = transform.position.y;
		float rotationSpeed = 100;

		if(!GameManager.SharedObject().isGameReady)
			targetPosition = initialPosition;

		Quaternion _lookRotation = transform.rotation;
		Vector3 _direction;

		_direction = (targetPosition - transform.position).normalized;

		if(_direction.magnitude > 0.2f)
			_lookRotation = Quaternion.LookRotation(_direction);

		Vector3 currentPosition = this.transform.position;

		if(Vector3.Distance(currentPosition, targetPosition) > 2f)
		{
			isMoving = true;

			if(Time.time - lastTime > 0.3f)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * rotationSpeed);
				lastTime = Time.time;
			}

			transform.Translate(Vector3.forward*Time.deltaTime*moveSpeed * 0.7f);
		}
		else
		{
			isMoving = false;

			_direction = (football.transform.position - transform.position).normalized;
			_lookRotation = Quaternion.LookRotation(_direction);
			_lookRotation.x = _lookRotation.z = 0f;
			transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * rotationSpeed);
		}

		if(isMoving == false)
			GetComponent<Animation>().Play("reposo", PlayMode.StopAll);
		else// if(animation["corriendo"].enabled == false)
			GetComponent<Animation>().Play("corriendo", PlayMode.StopAll);
	}

	private void MoveTowardsTheBall()
	{
		if(GetComponent<Animation>()["tiro"].enabled == true || GetComponent<Animation>()["pase"].enabled == true) return;
		isMoving = true;

		targetPosition = football.transform.position;

		if(football.transform.position.x < 30 && football.transform.position.x > -30)
		{
			attackTime += Time.deltaTime;

			if(attack == false && attackTime > 3)
			{
				attack = true;
				attackTime = 0;
			}
			else if(attack == true && attackTime>4)
			{
				attackTime = 0;
				attack = false;
			}

			if((attack || footballScript.ownerPlayer == null) && !TeamHasTheBall())
				targetPosition = football.transform.position;
			else if(TeamHasTheBall())
				targetPosition = new Vector3(football.transform.position.x-10,0,football.transform.position.z+zOffset/2);
			else
				targetPosition = new Vector3(football.transform.position.x+10,0,football.transform.position.z);
		}
		else
			targetPosition = initialPosition;

		if(!GameManager.SharedObject().isGameReady)
			targetPosition = initialPosition;

		targetPosition.y = transform.position.y;
		float rotationSpeed = 100;

		Quaternion _lookRotation = transform.rotation;
		Vector3 _direction;

		_direction = (targetPosition - transform.position).normalized;

		if(_direction.magnitude > 0.2f)
			_lookRotation = Quaternion.LookRotation(_direction);

		//rotate us over time according to speed until we are in the required rotation

		if(Time.time - lastTime > 0.3f)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * rotationSpeed);
			lastTime = Time.time;
		}

		Vector3 currentPosition = this.transform.position;

		if(Vector3.Distance(currentPosition, targetPosition) > 0.4f)
		{
			transform.Translate(Vector3.forward*Time.deltaTime*moveSpeed * 0.65f);
		}
		else
		{
			isMoving = false;

			_direction = (football.transform.position - transform.position).normalized;
			_lookRotation = Quaternion.LookRotation(_direction);
			_lookRotation.x = _lookRotation.z = 0f;
			transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * rotationSpeed);
		}

		if(isMoving == false)
			GetComponent<Animation>().Play("reposo", PlayMode.StopAll);
		else// if(animation["corriendo"].enabled == false)
			GetComponent<Animation>().Play("corriendo", PlayMode.StopAll);
	}

	private void MoveForGoal()
	{
		if(GetComponent<Animation>()["tiro"].enabled == true || GetComponent<Animation>()["pase"].enabled == true) return;
		isMoving = true;

		if(GetComponent<Animation>()["corriendo"].enabled == false)
			GetComponent<Animation>().Play("corriendo", PlayMode.StopAll);

		Vector3 targetPosition = goal.transform.position;

		if(!GameManager.SharedObject().isGameReady)
			targetPosition = initialPosition;

		targetPosition.y = transform.position.y;
		float rotationSpeed = 100;

		Quaternion _lookRotation;
		Vector3 _direction;

		//find the vector pointing from our position to the target
		_direction = (targetPosition - transform.position).normalized;

		//create the rotation we need to be in to look at the target
		_lookRotation = Quaternion.LookRotation(_direction);

		transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * rotationSpeed);

		if(Vector3.Distance(transform.position, targetPosition) > .5f)
			transform.Translate(Vector3.forward*Time.deltaTime*moveSpeed * 0.65f); //****StatesAndBehaviours.oppPlayerSpeed
	}

	// Update is called once per frame
	void Update()
	{
		if(GameManager.SharedObject().opponentMadeFoul || GameManager.SharedObject().playerMadeFoul)
		{
			gameObject.GetComponent<AiMidfielderController>().enabled = false;
			gameObject.GetComponent<OpponentFoulHandler>().enabled = true;
			return;
		}

		if(GameManager.SharedObject().playerGotCornerKick || GameManager.SharedObject().opponentGotCornerKick)
		{
			gameObject.GetComponent<AiMidfielderController>().enabled = false;
			gameObject.GetComponent<OpponentCornerKickHandler>().enabled = true;
			return;
		}

		if(Vector3.Distance(football.transform.position,transform.position) < 0.5f && GameManager.SharedObject().isGameReady)
			footballScript.SetOwnerIfPossible(transform);

		isMoving = false;

		if((waitForPass <= 0 || Vector3.Distance(transform.position, football.transform.position)<5f) && isMoving == false && transform == ControllablePlayer() && !HasTheBall()  && ((footballScript.ownerPlayer == null || !TeamHasTheBall() && GameManager.SharedObject().isGameReady) /*&& Vector3.Distance(transform.position, football.transform.position)<10f*/))
			MoveTowardsTheBall();

		if(waitForPass <= 0 && transform != ControllablePlayer() && !HasTheBall() && GameManager.SharedObject().isGameReady)
			MoveForward();

		if(transform == ControllablePlayer() && HasTheBall() && GameManager.SharedObject().isGameReady)
			MoveForGoal();

		if(isMoving == false && GetComponent<Animation>()["reposo"].enabled == false && GetComponent<Animation>()["tiro"].enabled == false && GetComponent<Animation>()["pase"].enabled == false && GetComponent<Animation>()["entrada"].enabled == false)
			GetComponent<Animation>().Play("reposo", PlayMode.StopAll);

		if(Vector3.Distance(transform.position, football.transform.position)<0.5f && transform == ControllablePlayer() && !HasTheBall() && GetComponent<Animation>()["entrada"].enabled == false && GameManager.SharedObject().isGameReady)
		{
			if(footballScript.ownerPlayer)
				footballScript.ownerPlayer.gameObject.GetComponent<Animation>().Play("entrada", PlayMode.StopAll);

			footballScript.SetOwner(transform);
			timeToPass = 5f;
		}

		timeToPass -= Time.deltaTime;
		waitForPass -= Time.deltaTime;
		/*
		if(transform == ControllablePlayer() && HasTheBall())
			Debug.Log ("time: "+timeToPass);
		*/
		if(transform == ControllablePlayer() && HasTheBall() && OpponentNearBy() && timeToPass <0f)
			StartCoroutine(MakeAPass());
		else if(transform == ControllablePlayer() && HasTheBall() && (transform.position.x < -32f))
		{
			transform.rotation = Quaternion.LookRotation((goal.position - transform.position).normalized);
			StartCoroutine(KickTheBall());
		}

		transform.position = new Vector3 (transform.position.x,0,transform.position.z);
	}

	void WaitForPass()
	{
		waitForPass = 1;
	}

	IEnumerator MakeAPass()
	{
		if(GetComponent<Animation>()["pase"].enabled == false)
			GetComponent<Animation>().Play("pase", PlayMode.StopAll);

		yield return new WaitForSeconds(0.3f);

		Quaternion _lookRotation;
		Vector3 _direction;

		Transform idealPlayer = null;

		float dot = -1f;

		foreach(Transform player in midfielders)
		{
			Vector3 heading = player.position - transform.position;
			if(Vector3.Dot(heading, transform.forward) > dot)
			{
				dot = Vector3.Dot(heading, transform.forward);
				idealPlayer = player;
			}
		}

		/*
		idealPlayer.GetComponent<ComputerPlayer> ().WaitForPass ();
		*/

		footballScript.SetFree();

		AudioManager.PlayKickSound ();

		//find the vector pointing from our position to the target
		_direction = (idealPlayer.position - transform.position).normalized;

		//create the rotation we need to be in to look at the target
		_lookRotation = Quaternion.LookRotation(_direction);
		_lookRotation.x = _lookRotation.z = 0f;
		transform.rotation = _lookRotation;//Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * 9999999999);

		Quaternion shotAngle = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x - 25 * progress,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z));
		football.transform.rotation = shotAngle;
		football.GetComponent<Rigidbody>().AddForce(football.transform.forward*300, ForceMode.Impulse);
		//football.GetComponent<Rigidbody>().AddForce(GetForceFrom(football.transform.position,idealPlayer.position), ForceMode.Impulse);
		progress = 0;
	}

	IEnumerator KickTheBall()
	{
		if(GetComponent<Animation>()["tiro"].enabled == false)
			GetComponent<Animation>().Play("tiro", PlayMode.StopAll);

		yield return new WaitForSeconds(0.3f);

		AudioManager.PlayKickSound ();

		footballScript.SetFree();
		footballScript.isKicked = true;

		Quaternion shotAngle = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x - 20,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z));
		football.transform.rotation = shotAngle;
		football.GetComponent<Rigidbody>().AddForce(football.transform.forward*150, ForceMode.Impulse);
	}

	Transform ControllablePlayer()
	{
		if(HasTheBall()) return transform;

		Transform idealPlayer = transform;

		foreach(Transform player in midfielders)
		{
			if(Vector3.Distance(football.transform.position,player.position) - Vector3.Distance(football.transform.position,idealPlayer.position) < 1f)
				idealPlayer = player;
		}
		return idealPlayer;
	}

	private bool OpponentNearBy()
	{
		Transform opponentNB = null;

		foreach(Transform opponent in opponents)
		{
			if(Vector3.Distance(transform.position,opponent.position) < 2f)
				opponentNB = opponent;
		}
		return opponentNB != null;
	}

	bool HasTheBall()
	{
		return (footballScript.ownerPlayer == transform);
	}
}
}
