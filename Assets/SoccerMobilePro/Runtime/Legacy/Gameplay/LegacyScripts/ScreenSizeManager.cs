namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "ScreenSizeManager")]

public class ScreenSizeManager : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Rect insets = LegacyGuiUtility.GetOrAddGUITexture(gameObject).pixelInset;
        insets.width *= Screen.width / 480f;
        insets.height *= Screen.width / 480f;

        LegacyGuiUtility.GetOrAddGUITexture(gameObject).pixelInset = insets;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
}
