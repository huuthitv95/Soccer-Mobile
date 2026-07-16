namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "GameManager")]

public class GameManager
{
	public bool showHalfTimeDialog = false;
	public bool showMatchEndDialog = false;

	public int gameTime = 0;

	private static GameManager sharedObject = null;

	public string playerTeamName = "PlayerTeam";
	public string playerTeamShortName = "PTM";

	public string opponentTeamName = "OpponentTeam";
	public string opponentTeamShortName = "OTM";

	public Texture playerTeamFlag;
	public Texture opponentTeamFlag;

	public Texture playerTeamTexture;
	public Texture opponentTeamTexture;

	public Texture playerTeamHDTexture;
	public Texture opponentTeamHDTexture;

	public int playerTeamGoals = 0;
	public int opponentTeamGoals = 0;

	public bool isGameReady = true;
	public bool isFirstHalf = true;

	public bool opponentMadeFoul = false;
	public bool playerMadeFoul = false;

	public bool opponentGotCornerKick = false;
	public bool playerGotCornerKick = false;

	public bool playerMissedGoal = false;
	public bool opponentMissedGoal = false;

	public Vector3 foulPosition = Vector3.zero;

	public bool isQuickMatch = false;

	public int currentMatch = 0;

	public static GameManager SharedObject()
	{
		if(sharedObject == null)
			sharedObject = new GameManager();

		return sharedObject;
	}

	public GameManager()
	{
		playerTeamGoals = 0;
		opponentTeamGoals = 0;
		isGameReady = true;
		isFirstHalf = true;

		opponentMadeFoul = false;
		playerMadeFoul = false;

		foulPosition = Vector3.zero;
	}
}
}
