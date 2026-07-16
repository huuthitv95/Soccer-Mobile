namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "aaa")]
public class PivotOrbitController : MonoBehaviour {

	public Transform pivot;
	public float x,y,z;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

		transform.RotateAround (pivot.position,new Vector3(x,y,z),60*Time.deltaTime);
	}
}
}
