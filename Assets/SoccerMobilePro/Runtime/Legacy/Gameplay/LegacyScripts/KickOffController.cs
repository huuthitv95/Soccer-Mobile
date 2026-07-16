namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "KickOffController")]

public class KickOffController : MonoBehaviour
{
	private const string DefaultPlayerTeamName = "Algeria";
	private const string DefaultPlayerTeamShortName = "ALG";
	private const string DefaultOpponentTeamName = "Angola";
	private const string DefaultOpponentTeamShortName = "ANG";

	[UnityEngine.Serialization.FormerlySerializedAs("Team1Flag")]
	public GUITexture team1Flag;
	[UnityEngine.Serialization.FormerlySerializedAs("Team2Flag")]
	public GUITexture team2Flag;
	[UnityEngine.Serialization.FormerlySerializedAs("Team1Name")]
	public GUIText team1Name;
	[UnityEngine.Serialization.FormerlySerializedAs("Team2Name")]
	public GUIText team2Name;
	[UnityEngine.Serialization.FormerlySerializedAs("Team1Material")]
	public Material team1Material;
	[UnityEngine.Serialization.FormerlySerializedAs("Team2Material")]
	public Material team2Material;
	[UnityEngine.Serialization.FormerlySerializedAs("Team1HDMaterial")]
	public Material team1HdMaterial;
	[UnityEngine.Serialization.FormerlySerializedAs("Team2HDMaterial")]
	public Material team2HdMaterial;

	// Use this for initialization
	void Start()
	{
		team1Flag = ResolveTextureField(team1Flag, "Team1Flag");
		team2Flag = ResolveTextureField(team2Flag, "Team2Flag");
		team1Name = ResolveTextField(team1Name, "Team1Name");
		team2Name = ResolveTextField(team2Name, "Team2Name");

		GameManager manager = GameManager.SharedObject();

		Texture defaultTeam1Flag = team1Flag != null ? team1Flag.texture : null;
		Texture defaultTeam2Flag = team2Flag != null ? team2Flag.texture : null;
		Texture defaultTeam1Texture = team1Material != null ? team1Material.mainTexture : null;
		Texture defaultTeam2Texture = team2Material != null ? team2Material.mainTexture : null;
		Texture defaultTeam1HDTexture = team1HdMaterial != null ? team1HdMaterial.mainTexture : null;
		Texture defaultTeam2HDTexture = team2HdMaterial != null ? team2HdMaterial.mainTexture : null;

		if(manager.playerTeamFlag == null)
			manager.playerTeamFlag = defaultTeam1Flag;
		if(manager.opponentTeamFlag == null)
			manager.opponentTeamFlag = defaultTeam2Flag;
		if(manager.playerTeamTexture == null)
			manager.playerTeamTexture = defaultTeam1Texture;
		if(manager.opponentTeamTexture == null)
			manager.opponentTeamTexture = defaultTeam2Texture;
		if(manager.playerTeamHDTexture == null)
			manager.playerTeamHDTexture = defaultTeam1HDTexture;
		if(manager.opponentTeamHDTexture == null)
			manager.opponentTeamHDTexture = defaultTeam2HDTexture;

		manager.playerTeamName = ResolveTeamName(manager.playerTeamName, "PlayerTeam", DefaultPlayerTeamName);
		manager.playerTeamShortName = ResolveTeamName(manager.playerTeamShortName, "PTM", DefaultPlayerTeamShortName);
		manager.opponentTeamName = ResolveTeamName(manager.opponentTeamName, "OpponentTeam", DefaultOpponentTeamName);
		manager.opponentTeamShortName = ResolveTeamName(manager.opponentTeamShortName, "OTM", DefaultOpponentTeamShortName);

		if(team1Flag != null && manager.playerTeamFlag != null)
			team1Flag.texture = manager.playerTeamFlag;
		if(team2Flag != null && manager.opponentTeamFlag != null)
			team2Flag.texture = manager.opponentTeamFlag;

		if(team1Name != null)
			team1Name.text = manager.playerTeamName;
		if(team2Name != null)
			team2Name.text = manager.opponentTeamName;

		if(team1Material != null && manager.playerTeamTexture != null)
			team1Material.mainTexture =  manager.playerTeamTexture;
		if(team2Material != null && manager.opponentTeamTexture != null)
			team2Material.mainTexture =  manager.opponentTeamTexture;

		if(team1HdMaterial != null && manager.playerTeamHDTexture != null)
			team1HdMaterial.mainTexture =  manager.playerTeamHDTexture;
		if(team2HdMaterial != null && manager.opponentTeamHDTexture != null)
			team2HdMaterial.mainTexture =  manager.opponentTeamHDTexture;
//		Team1HDMaterial.mainTexture =  GameManager.SharedObject ().opponentTeamHDTexture;


	}

	private static string ResolveTeamName(string candidate, string defaultValue, string fallback)
	{
		return string.IsNullOrEmpty(candidate) || candidate == defaultValue ? fallback : candidate;
	}

	private GUITexture ResolveTextureField(GUITexture current, string objectName)
	{
		if(current != null)
			return current;

		GameObject target = GameObject.Find(objectName);
		if(target == null)
			return null;

		return LegacyGuiUtility.GetOrAddGUITexture(target);
	}

	private GUIText ResolveTextField(GUIText current, string objectName)
	{
		if(current != null)
			return current;

		GameObject target = GameObject.Find(objectName);
		if(target == null)
			return null;

		return LegacyGuiUtility.GetOrAddGUIText(target);
	}
}
}
