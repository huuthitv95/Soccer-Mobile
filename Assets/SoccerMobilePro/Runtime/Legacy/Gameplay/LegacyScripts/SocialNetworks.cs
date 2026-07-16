namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using UnityEngine;
using System.Collections;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "SocialNetworks")]

public class SocialNetworks
{
	public static AndroidJavaClass pluginClass ;
	public static AndroidJavaObject jObject;

	public static void ShareURL(string title, string url)
	{
		pluginClass = new AndroidJavaClass("com.myplugin.test.ShareClass");
		jObject= pluginClass.CallStatic<AndroidJavaObject>("instance");
		jObject.Call("ShareOnClick", (title + "\n" + url));
	}
}
}
