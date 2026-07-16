namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "BallScript")]

public class BallScript : MonoBehaviour
{
	[HideInInspector]
	public bool isKicked = false;

	private Vector3 initialPosition, lastPosition;
	public Transform ownerPlayer;
	private float lastTime = 0;

	[HideInInspector]
	public string lastOwnerTag = "";

	void Awake()
	{
		gameObject.name = "Ball";
	}

	// Use this for initialization
	void Start()
	{
		ownerPlayer = null;
		initialPosition = transform.position;
	}
//	public void invokePlaceOnInitialPosition()
//	{
//		Invoke ("PlaceOnInitialPositon",4);
//	}
	public void PlaceOnInitialPositon()
	{
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

		isKicked = false;
		ownerPlayer = null;
		lastTime = Time.time;
		lastOwnerTag = "";
		transform.position = initialPosition;
	}

	public void SetFree()
	{
		lastTime = Time.time;
		ownerPlayer = null;
	}

	// Update is called once per frame
	void Update()
	{
		if(GetComponent<Rigidbody>().velocity.magnitude > 20f)
		{
			Vector3 vel = GetComponent<Rigidbody>().velocity.normalized;
			GetComponent<Rigidbody>().velocity = vel * 20f;
		}

		if(ownerPlayer != null)
		if(GameManager.SharedObject().isGameReady == false && ownerPlayer.tag != "Hand")
			SetFree();

		if(GameManager.SharedObject().opponentMadeFoul || GameManager.SharedObject().playerMadeFoul || GameManager.SharedObject().isGameReady==false)
		{
			if(ownerPlayer)
				transform.position = ownerPlayer.position;
		}
		else
		{
			if(ownerPlayer)
			{
				transform.position = new Vector3(0,transform.position.y,0) + new Vector3(ownerPlayer.position.x,0,ownerPlayer.position.z) + ownerPlayer.forward * 0.5f;

				if(ownerPlayer.tag == "Player" && ownerPlayer.GetComponent<Player>().isMoving)
					transform.RotateAround (transform.position, ownerPlayer.right, 350*Time.deltaTime);
				else if(ownerPlayer.tag == "ComputerPlayer" && ownerPlayer.GetComponent<AiDefenderController>().isMoving)
					transform.RotateAround (transform.position, ownerPlayer.right, 350*Time.deltaTime);
			}
		}

		if(transform.position.y < -0.15f)
			transform.position = new Vector3(transform.position.x,0.15f,transform.position.z);
	}

	public void SetOwnerIfPossible(Transform owner)
	{
		if(ownerPlayer != null && owner.tag == "Hand")
			return;

		isKicked = false;

		if(ownerPlayer == null && Time.time - lastTime > 1f)
		{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			GetComponent<Rigidbody>().Sleep();
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			ownerPlayer = owner;

			lastOwnerTag = ownerPlayer.tag;
		}
	}

	public void SetOwner(Transform owner)
	{
		if(ownerPlayer != null && owner.tag == "Hand")
			return;

		isKicked = false;

		if(Time.time - lastTime > 2f)
		{
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			ownerPlayer = owner;

			lastOwnerTag = ownerPlayer.tag;
			lastTime = Time.time;
		}
	}

//	void OnGUI()
//	{
//		if(GUI.Button(new Rect(0,0,100,100),"RESET"))
//			SceneManager.LoadScene(Application.loadedLevel);
//	}
}
}
