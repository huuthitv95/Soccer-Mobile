namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "ColliderStopTheBall")]

public class ColliderStopTheBall : MonoBehaviour
{
	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			if(contact.otherCollider.tag == "TheSoccerBall")
			{
				contact.otherCollider.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
				contact.otherCollider.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				break;
			}
		}
	}
}
}
