namespace SoccerMobilePro.Legacy.TeamSelection
{
using SoccerMobilePro.Legacy.Gameplay;
using SoccerMobilePro.Legacy.Compatibility;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "TeamSelectionController")]
public class HomeTeamSelectionController : MonoBehaviour
{
	public static int teamIndex = 0;
	public Texture[] teams;
	public Texture[] textures;
	[UnityEngine.Serialization.FormerlySerializedAs("HDTextures")]
	public Texture[] hdTextures;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if(teamIndex > 31) teamIndex = 0;
		if(teamIndex < 0) teamIndex = 31;

		if(LegacyGuiUtility.GetOrAddGUITexture(gameObject))
			LegacyGuiUtility.GetOrAddGUITexture(gameObject).texture = teams[teamIndex];

		GameManager.SharedObject ().playerTeamFlag = teams[teamIndex];
		GameManager.SharedObject ().playerTeamTexture = textures[teamIndex];

		GameManager.SharedObject ().playerTeamHDTexture = hdTextures[teamIndex];
	}
}
}
