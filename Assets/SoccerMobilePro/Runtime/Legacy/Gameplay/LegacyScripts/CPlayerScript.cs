namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "CPlayerScript")]

public class CPlayerScript : MonoBehaviour
{
	private Vector3 InitialPosition;

	void Start()
	{
		InitialPosition = transform.position;
	}

	public void Reset()
	{
		GetComponent<Animation>().Stop ();
		transform.position = InitialPosition;
	}

	public void AnimatePlayer()
	{
		GetComponent<Animation>().Play("Anim_Soccer_happy_run_and_hand_move");
		GetComponent<Animation>().PlayQueued("Anim_Soccer_happy_after_running");
		GetComponent<Animation>().PlayQueued("Anim_Soccer_happy_after_running_02");
	}

	void Update()
	{
		if (GetComponent<Animation>()["Anim_Soccer_happy_run_and_hand_move"].enabled == true
		    && GetComponent<Animation>()["Anim_Soccer_happy_run_and_hand_move"].normalizedTime < 0.9f)
		{
			transform.Translate(Vector3.forward*4*Time.deltaTime);
		}
	}
}
}
