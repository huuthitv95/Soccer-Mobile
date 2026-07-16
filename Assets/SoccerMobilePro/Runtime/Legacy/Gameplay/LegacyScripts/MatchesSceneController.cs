namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "MatchesSceneController")]

public class MatchesSceneController : MonoBehaviour
{
	[UnityEngine.Serialization.FormerlySerializedAs("Flags")]
	public Texture[] flags;

	public Transform flag1,flag2,flag3,flag4,flag5,flag6,flag7;
	private int match1TeamIndex=-1,match2TeamIndex=-1,match3TeamIndex=-1,match4TeamIndex=-1,match5TeamIndex=-1,match6TeamIndex=-1,match7TeamIndex=-1;
	private string teamGroup = "";

	private int match1score1,match2score1,match3score1,match4score1,match5score1,match6score1,match7score1;
	private int match1score2,match2score2,match3score2,match4score2,match5score2,match6score2,match7score2;
	private int matchNumber = 1;

	public static bool HasPendingCup()
	{
		return (PlayerPrefs.GetInt ("HasPendingCup")==1?true:false);
	}


	// Use this for initialization
	void Start ()
	{
		if(HasPendingCup())
		{
			match1TeamIndex = PlayerPrefs.GetInt("match1TeamIndex");
			match2TeamIndex = PlayerPrefs.GetInt("match2TeamIndex");
			match3TeamIndex = PlayerPrefs.GetInt("match3TeamIndex");
			match4TeamIndex = PlayerPrefs.GetInt("match4TeamIndex");
			match5TeamIndex = PlayerPrefs.GetInt("match5TeamIndex");
			match6TeamIndex = PlayerPrefs.GetInt("match6TeamIndex");
			match7TeamIndex = PlayerPrefs.GetInt("match7TeamIndex");
		}
		else
		{
			int selectedGroupStartIndex = 0;
			int selectedGroupEndIndex = 3;

			if(HomeTeamSelectionController.teamIndex>=0 && HomeTeamSelectionController.teamIndex<=3)
			{
				teamGroup = "A";
				selectedGroupStartIndex = 0;
				selectedGroupEndIndex = 3;
			}
			else if(HomeTeamSelectionController.teamIndex>=4 && HomeTeamSelectionController.teamIndex<=7)
			{
				teamGroup = "B";
				selectedGroupStartIndex = 4;
				selectedGroupEndIndex = 7;
			}
			else if(HomeTeamSelectionController.teamIndex>=8 && HomeTeamSelectionController.teamIndex<=11)
			{
				teamGroup = "C";
				selectedGroupStartIndex = 8;
				selectedGroupEndIndex = 11;
			}
			else if(HomeTeamSelectionController.teamIndex>=12 && HomeTeamSelectionController.teamIndex<=15)
			{
				teamGroup = "D";
				selectedGroupStartIndex = 12;
				selectedGroupEndIndex = 15;
			}
			else if(HomeTeamSelectionController.teamIndex>=16 && HomeTeamSelectionController.teamIndex<=19)
			{
				teamGroup = "E";
				selectedGroupStartIndex = 16;
				selectedGroupEndIndex = 19;
			}
			else if(HomeTeamSelectionController.teamIndex>=20 && HomeTeamSelectionController.teamIndex<=23)
			{
				teamGroup = "F";
				selectedGroupStartIndex = 20;
				selectedGroupEndIndex = 23;
			}
			else if(HomeTeamSelectionController.teamIndex>=24 && HomeTeamSelectionController.teamIndex<=27)
			{
				teamGroup = "G";
				selectedGroupStartIndex = 24;
				selectedGroupEndIndex = 27;
			}
			else if(HomeTeamSelectionController.teamIndex>=28 && HomeTeamSelectionController.teamIndex<=31)
			{
				teamGroup = "H";
				selectedGroupStartIndex = 28;
				selectedGroupEndIndex = 31;
			}

			// GROUP MATCHES
			int tIndex = 0;
			for(int i = selectedGroupStartIndex; i <= selectedGroupEndIndex; i++)
			{
				if(i != HomeTeamSelectionController.teamIndex)
				{
					if(match1TeamIndex == -1)
						match1TeamIndex = i;
					else if(match2TeamIndex == -1)
						match2TeamIndex = i;
					else if(match3TeamIndex == -1)
						match3TeamIndex = i;

					tIndex += 1;
				}
			}

			int nextGroupStartIndex = 0;
			int nextGroupEndIndex = 3;

			if(teamGroup == "A")
			{
				nextGroupStartIndex = 4;
				nextGroupStartIndex = 7;
			}
			else if(teamGroup == "B")
			{
				nextGroupStartIndex = 0;
				nextGroupStartIndex = 3;
			}
			else if(teamGroup == "C")
			{
				nextGroupStartIndex = 12;
				nextGroupStartIndex = 15;
			}
			else if(teamGroup == "D")
			{
				nextGroupStartIndex = 8;
				nextGroupStartIndex = 11;
			}
			else if(teamGroup == "E")
			{
				nextGroupStartIndex = 20;
				nextGroupStartIndex = 23;
			}
			else if(teamGroup == "F")
			{
				nextGroupStartIndex = 16;
				nextGroupStartIndex = 19;
			}
			else if(teamGroup == "G")
			{
				nextGroupStartIndex = 28;
				nextGroupStartIndex = 31;
			}
			else if(teamGroup == "H")
			{
				nextGroupStartIndex = 24;
				nextGroupStartIndex = 27;
			}

			match4TeamIndex = Random.Range(nextGroupStartIndex,nextGroupEndIndex+1);

			int quarterFinalStartIndex = 0;
			int quarterFinalEndIndex = 0;

			if(teamGroup == "C" || teamGroup == "B" || teamGroup == "C" || teamGroup == "D")
			{
				quarterFinalStartIndex = 0;
				quarterFinalEndIndex = 15;
			}
			else
			{
				quarterFinalStartIndex = 16;
				quarterFinalEndIndex = 31;
			}
			//Quarter Final
			while(match5TeamIndex == -1)
			{
				int index = Random.Range(quarterFinalStartIndex,quarterFinalEndIndex+1);
				if(index != HomeTeamSelectionController.teamIndex && index != match1TeamIndex && index != match2TeamIndex && index != match3TeamIndex && index != match4TeamIndex)
					match5TeamIndex = index;
			}
			//Semi Final
			while(match6TeamIndex == -1)
			{
				int index = Random.Range(0,32);
				if(index != HomeTeamSelectionController.teamIndex && index != match1TeamIndex && index != match2TeamIndex && index != match3TeamIndex && index != match4TeamIndex && index != match5TeamIndex)
					match6TeamIndex = index;
			}
			//Final
			while(match7TeamIndex == -1)
			{
				int index = Random.Range(0,32);
				if(index != HomeTeamSelectionController.teamIndex && index != match1TeamIndex && index != match2TeamIndex && index != match3TeamIndex && index != match4TeamIndex && index != match5TeamIndex && index != match6TeamIndex)
					match7TeamIndex = index;
			}

			PlayerPrefs.SetInt("match1TeamIndex",match1TeamIndex);
			PlayerPrefs.SetInt("match2TeamIndex",match2TeamIndex);
			PlayerPrefs.SetInt("match3TeamIndex",match3TeamIndex);
			PlayerPrefs.SetInt("match4TeamIndex",match4TeamIndex);
			PlayerPrefs.SetInt("match5TeamIndex",match5TeamIndex);
			PlayerPrefs.SetInt("match6TeamIndex",match6TeamIndex);
			PlayerPrefs.SetInt("match7TeamIndex",match7TeamIndex);
			PlayerPrefs.SetInt("playerTeamIndex",HomeTeamSelectionController.teamIndex);

			PlayerPrefs.SetInt("HasPendingCup",1);

			PlayerPrefs.SetInt("match1score1",-1); PlayerPrefs.SetInt("match1score2",-1);
			PlayerPrefs.SetInt("match2score1",-1); PlayerPrefs.SetInt("match2score2",-1);
			PlayerPrefs.SetInt("match3score1",-1); PlayerPrefs.SetInt("match3score2",-1);
			PlayerPrefs.SetInt("match4score1",-1); PlayerPrefs.SetInt("match4score2",-1);
			PlayerPrefs.SetInt("match5score1",-1); PlayerPrefs.SetInt("match5score2",-1);
			PlayerPrefs.SetInt("match6score1",-1); PlayerPrefs.SetInt("match6score2",-1);
			PlayerPrefs.SetInt("match7score1",-1); PlayerPrefs.SetInt("match7score2",-1);
			PlayerPrefs.SetInt("matchNumber",1);
		}

		match1score1 = PlayerPrefs.GetInt("match1score1");
		match2score1 = PlayerPrefs.GetInt("match2score1");
		match3score1 = PlayerPrefs.GetInt("match3score1");
		match4score1 = PlayerPrefs.GetInt("match4score1");
		match5score1 = PlayerPrefs.GetInt("match5score1");
		match6score1 = PlayerPrefs.GetInt("match6score1");
		match7score1 = PlayerPrefs.GetInt("match7score1");

		match1score2 = PlayerPrefs.GetInt("match1score2");
		match2score2 = PlayerPrefs.GetInt("match2score2");
		match3score2 = PlayerPrefs.GetInt("match3score2");
		match4score2 = PlayerPrefs.GetInt("match4score2");
		match5score2 = PlayerPrefs.GetInt("match5score2");
		match6score2 = PlayerPrefs.GetInt("match6score2");
		match7score2 = PlayerPrefs.GetInt("match7score2");

		matchNumber = PlayerPrefs.GetInt("matchNumber");

		LegacyGuiUtility.GetOrAddGUITexture(flag1).texture = flags[match1TeamIndex];
		LegacyGuiUtility.GetOrAddGUITexture(flag2).texture = flags[match2TeamIndex];
		LegacyGuiUtility.GetOrAddGUITexture(flag3).texture = flags[match3TeamIndex];

		if(matchNumber > 3)
			LegacyGuiUtility.GetOrAddGUITexture(flag4).texture = flags[match4TeamIndex];

		if(matchNumber > 4)
			LegacyGuiUtility.GetOrAddGUITexture(flag5).texture = flags[match5TeamIndex];

		if(matchNumber > 5)
			LegacyGuiUtility.GetOrAddGUITexture(flag6).texture = flags[match6TeamIndex];

		if(matchNumber > 6)
			LegacyGuiUtility.GetOrAddGUITexture(flag7).texture = flags[match7TeamIndex];

		GameManager.SharedObject ().currentMatch = CurrentMatchIndex();
		GameManager.SharedObject ().isFirstHalf = true;
		AwayTeamSelectionController.teamIndex = GameManager.SharedObject ().currentMatch;
		HomeTeamSelectionController.teamIndex = PlayerPrefs.GetInt("playerTeamIndex");
	}

	private int CurrentMatchIndex()
	{
		int cti = 1;
		switch(matchNumber)
		{
			case 1: cti = match1TeamIndex; break;
			case 2: cti = match2TeamIndex; break;
			case 3: cti = match3TeamIndex; break;
			case 4: cti = match4TeamIndex; break;
			case 5: cti = match5TeamIndex; break;
			case 6: cti = match6TeamIndex; break;
			case 7: cti = match7TeamIndex; break;
		}
		return cti;
	}

	// Update is called once per frame
	void Update () {

	}
}
}
