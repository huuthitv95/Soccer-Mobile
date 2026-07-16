namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "AudienceScript")]

public class AudienceScript : MonoBehaviour
{
	private string currentAnimation = "idle";

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update ()
	{
		if(GetComponent<Animation>()[currentAnimation].enabled == false)
		{
			switch(Random.Range(0,6))
			{
			case 0:
				currentAnimation = "idle";
				break;

			case 1:
				currentAnimation = "applause";
				break;

			case 2:
				currentAnimation = "applause2";
				break;

			case 3:
				currentAnimation = "celebration";
				break;

			case 4:
				currentAnimation = "celebration2";
				break;

			case 5:
				currentAnimation = "celebration3";
				break;
			}
			GetComponent<Animation>().Play(currentAnimation,PlayMode.StopAll);
		}

	}
}
}
