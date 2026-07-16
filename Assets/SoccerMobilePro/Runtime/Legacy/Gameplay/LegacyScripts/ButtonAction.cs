namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "ButtonAction")]

public class ButtonAction : MonoBehaviour
{
	OtherDialoguesActive oda;
	private GUITexture legacyTexture;
	private ButtonController buttonController;
	private bool pressedInside;

	void Start ()
	{
		GameObject mainCamera = GameObject.Find ("Main Camera");
		oda = mainCamera == null ? null : mainCamera.GetComponent<OtherDialoguesActive> ();
		legacyTexture = LegacyGuiUtility.GetOrAddGUITexture(gameObject);
		buttonController = gameObject.GetComponent<ButtonController>();

    }

//	public GameObject matchCompleted, halfCompleted;
	public enum Buttons{
		Play,
		MoreGames,
		RateUs,
		QuickMatch,
		InternationalCup,
		Back,
		Next,
		PrevTeam,
		NextTeam,
		KickOff,
		MainMenu,
		PlaySecondHalf,
		Replay,
		YES_QUIT,
		NO_QUIT,
		Resume,
		Pause,
		None
	};

	public Buttons buttonType = Buttons.None;
	void Update()
	{
		if(SoccerInput.PausePressedThisFrame)
		{
			backPressed();
		}

		UpdateLegacyPointerInput();
	}
	void OnMouseDown()
	{
		SetPressedTexture();
	}

	void OnMouseUpAsButton()
	{
		SetNormalTexture();

		switch(buttonType)
		{
		case Buttons.Pause:
			if(oda == null || !oda.isOtherDialogueActive)
			{
				PauseController.isPaused=true;
			}


                break;
		case Buttons.Play:
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.GameModeSelection);
			break;

		case Buttons.MoreGames:
			Application.OpenURL("https://play.google.com/store/apps/developer?id=XYZ");
			break;

		case Buttons.RateUs:
			Application.OpenURL("https://play.google.com/store/apps/details?id=com.yourgame.url");
			break;

		case Buttons.QuickMatch:
			GameManager.SharedObject().isQuickMatch = true;
			GameManager.SharedObject().isFirstHalf = true;
			PlayerPosition.playerTurn = true;
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.HomeTeamSelection);
			break;

		case Buttons.InternationalCup:
			GameManager.SharedObject().isQuickMatch = false;
			GameManager.SharedObject().isFirstHalf = true;
			PlayerPosition.playerTurn = true;

			if(MatchesSceneController.HasPendingCup())
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.TournamentMatches);
			else
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.HomeTeamSelection);
			break;

		case Buttons.Back:
			backPressed();
			break;

		case Buttons.Next:
			if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.HomeTeamSelection && GameManager.SharedObject().isQuickMatch)
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.AwayTeamSelection);
			else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.HomeTeamSelection && !GameManager.SharedObject().isQuickMatch)
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.TournamentGroups);
			else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.TournamentGroups && !GameManager.SharedObject().isQuickMatch)
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.TournamentMatches);
			else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.TournamentMatches && !GameManager.SharedObject().isQuickMatch)
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.Kickoff);
			else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.AwayTeamSelection)
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.Kickoff);
			break;

		case Buttons.PrevTeam:
			if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.HomeTeamSelection)
				HomeTeamSelectionController.teamIndex -= 1;
			else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.AwayTeamSelection)
				AwayTeamSelectionController.teamIndex -= 1;
			break;

		case Buttons.NextTeam:
			if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.HomeTeamSelection)
				HomeTeamSelectionController.teamIndex += 1;
			else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.AwayTeamSelection)
				AwayTeamSelectionController.teamIndex += 1;
			break;

		case Buttons.KickOff:
			PlayerPrefs.Save();
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.Match);
			break;

		case Buttons.MainMenu:
			InitGame.matchcomplete=false;
			InitGame.halfComplete=false;
			if(AudioManager.isSFXOn)
				AudioListener.volume=1;

			PauseController.isPaused = false;
			Time.timeScale = 1f;
			PlayerPosition.playerTurn = !PlayerPosition.playerTurn;
			if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.TournamentCelebration)
			{
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.MainMenu);
			}
			else if(GameManager.SharedObject().isQuickMatch == false && PlayerPrefs.GetInt("matchNumber")>7)
			{
				PlayerPrefs.SetInt("HasPendingCup",0);
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.TournamentCelebration);
			}
			else if(GameManager.SharedObject().isQuickMatch == false && PlayerPrefs.GetInt("matchNumber")<7)
			{
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.TournamentMatches);
			}
			else
			{
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.MainMenu);
			}

			//if(GameManager.SharedObject().isQuickMatch)	SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.MainMenu);
			//else	SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.TournamentMatches);

			break;

		case Buttons.PlaySecondHalf:
			InitGame.halfComplete=false;
			PlayerPosition.playerTurn = false;
			GameManager.SharedObject().gameTime = 0;
			GameManager.SharedObject().isFirstHalf = false;
			if(GameManager.SharedObject().isQuickMatch)
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.Match);
			else
				SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.Kickoff);
			break;

		case Buttons.Replay:
			GameManager.SharedObject().isFirstHalf = true;
			GameManager.SharedObject().isGameReady = true;
			GameManager.SharedObject().showHalfTimeDialog = false;
			GameManager.SharedObject().showMatchEndDialog = false;
			GameManager.SharedObject().playerTeamGoals = 0;
			GameManager.SharedObject().opponentTeamGoals = 0;
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.Match);
			if(AudioManager.isSFXOn)
				AudioListener.volume=1;
			break;

		case Buttons.YES_QUIT:
			Application.Quit();
			break;

		case Buttons.NO_QUIT:
			if(AudioManager.isMusicOn)
				AudioListener.volume=1;
			GameObject.Find("QuitDialog").SetActive(false);
			break;

		case Buttons.Resume:
			PauseController.isPaused = false;
			break;
		}
	}

	void backPressed()
	{
		if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.GameModeSelection)
		{
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.MainMenu);
		}
		else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.HomeTeamSelection)
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.GameModeSelection);
		else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.AwayTeamSelection)
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.HomeTeamSelection);
		else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.TournamentMatches && MatchesSceneController.HasPendingCup())
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.GameModeSelection);

		else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.Kickoff && !GameManager.SharedObject().isQuickMatch && MatchesSceneController.HasPendingCup())
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.TournamentMatches);
		else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.Kickoff && !GameManager.SharedObject().isQuickMatch && !MatchesSceneController.HasPendingCup())
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.HomeTeamSelection);

		else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.Kickoff && GameManager.SharedObject().isQuickMatch)
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.AwayTeamSelection);
		else if(SceneManager.GetActiveScene().name == SoccerMobilePro.Platform.SceneIds.TournamentGroups && !GameManager.SharedObject().isQuickMatch)
			SceneManager.LoadScene(SoccerMobilePro.Platform.SceneIds.HomeTeamSelection);
	}

	private void UpdateLegacyPointerInput()
	{
		if (legacyTexture == null)
		{
			legacyTexture = LegacyGuiUtility.GetOrAddGUITexture(gameObject);
		}

		if (legacyTexture == null)
		{
			return;
		}

		if (SoccerInput.PointerPressedThisFrame)
		{
			pressedInside = legacyTexture.HitTest(SoccerInput.PointerPosition);
			if (pressedInside)
			{
				SetPressedTexture();
			}
		}

		if (SoccerInput.PointerReleasedThisFrame)
		{
			bool releaseInside = legacyTexture.HitTest(SoccerInput.PointerPosition);
			if (pressedInside && releaseInside)
			{
				OnMouseUpAsButton();
			}
			else if (pressedInside)
			{
				SetNormalTexture();
			}

			pressedInside = false;
		}

		for (int i = 0; i < SoccerInput.TouchCount; i++)
		{
			SoccerTouch touch = SoccerInput.GetTouch(i);
			Vector3 touchPosition = new Vector3(touch.position.x, touch.position.y, 0f);
			if (touch.phase == SoccerTouchPhase.Began && legacyTexture.HitTest(touchPosition))
			{
				pressedInside = true;
				SetPressedTexture();
			}
			else if ((touch.phase == SoccerTouchPhase.Ended || touch.phase == SoccerTouchPhase.Canceled) && pressedInside)
			{
				if (touch.phase == SoccerTouchPhase.Ended && legacyTexture.HitTest(touchPosition))
				{
					OnMouseUpAsButton();
				}
				else
				{
					SetNormalTexture();
				}

				pressedInside = false;
			}
		}
	}

	private void SetPressedTexture()
	{
		if (buttonController == null)
		{
			buttonController = gameObject.GetComponent<ButtonController>();
		}

		if (legacyTexture == null)
		{
			legacyTexture = LegacyGuiUtility.GetOrAddGUITexture(gameObject);
		}

		if (buttonController != null && buttonController.hoverTexture != null && legacyTexture != null)
		{
			legacyTexture.texture = buttonController.hoverTexture;
		}
	}

	private void SetNormalTexture()
	{
		if (buttonController == null)
		{
			buttonController = gameObject.GetComponent<ButtonController>();
		}

		if (legacyTexture == null)
		{
			legacyTexture = LegacyGuiUtility.GetOrAddGUITexture(gameObject);
		}

		if (buttonController != null && legacyTexture != null)
		{
			legacyTexture.texture = buttonController.normalTexture;
		}
	}
}
}
