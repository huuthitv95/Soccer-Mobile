namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "TextScaling")]

public class TextScaling : MonoBehaviour
{
	public int fontSize = 10;

	void Awake ()
	{
		ApplyFontSize();
	}

	void OnEnable ()
	{
		ApplyFontSize();
	}

	void Update ()
	{
		ApplyFontSize();
	}

	void FixedUpdate ()
	{
		ApplyFontSize();
	}

	private void ApplyFontSize()
	{
		GUIText guiText = LegacyGuiUtility.GetOrAddGUIText(gameObject);
		if(guiText)
		{
			guiText.fontSize = Mathf.Max(1, fontSize * Screen.height / 800);
		}
	}
}
}
