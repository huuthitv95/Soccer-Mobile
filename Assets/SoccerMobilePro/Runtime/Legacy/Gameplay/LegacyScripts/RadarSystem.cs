namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
/// <summary>
/// This is Radar System. using to detection an objects and showing on minimap by Tags[]
/// </summary>

using UnityEngine;
using System.Collections;

public enum Alignment { None,LeftTop, RightTop, LeftBot, RightBot ,MiddleTop ,MiddleBot}

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "RadarSystem")]

public class RadarSystem : MonoBehaviour {

	private Vector2 minimapPosition;
	[UnityEngine.Serialization.FormerlySerializedAs("Size")]
	public float size = 400;
	[UnityEngine.Serialization.FormerlySerializedAs("Distance")]
	public float distance = 100;
	[UnityEngine.Serialization.FormerlySerializedAs("Alpha")]
	public float alpha = 0.5f;
	[UnityEngine.Serialization.FormerlySerializedAs("Navtexture")]
	public Texture2D[] navigationTextures;
	[UnityEngine.Serialization.FormerlySerializedAs("EnemyTag")]
	public string[] trackedTags;
	[UnityEngine.Serialization.FormerlySerializedAs("NavCompass")]
	public Texture2D compassTexture;
	[UnityEngine.Serialization.FormerlySerializedAs("NavBG")]
	public Texture2D backgroundTexture;
	[UnityEngine.Serialization.FormerlySerializedAs("PositionOffset")]
	public Vector2 positionOffset = new Vector2(0,0);
	[UnityEngine.Serialization.FormerlySerializedAs("Scale")]
	public float scale = 1;
	[UnityEngine.Serialization.FormerlySerializedAs("PositionAlignment")]
	public Alignment positionAlignment = Alignment.None;
	[UnityEngine.Serialization.FormerlySerializedAs("MapRotation")]
	public bool mapRotation;
	[UnityEngine.Serialization.FormerlySerializedAs("Player")]
	public GameObject player;
	[UnityEngine.Serialization.FormerlySerializedAs("Show")]
	public bool show = true;
	[UnityEngine.Serialization.FormerlySerializedAs("ColorMult")]
	public Color colorMultiplier = Color.white;

	void Start ()
	{
		size = size * Screen.height / 640f;
	}


	void Update () {
		if(!player){
			player = this.gameObject;
		}

		if(scale<=0){
			scale = 1;
		}

		switch(positionAlignment){
		case Alignment.None:
			minimapPosition = positionOffset;
		break;
		case Alignment.LeftTop:
			minimapPosition = Vector2.zero + positionOffset;
		break;
		case Alignment.RightTop:
			minimapPosition = new Vector2(Screen.width - size-75,0) + positionOffset;
		break;
		case Alignment.LeftBot:
			minimapPosition = new Vector2(0,Screen.height - size) + positionOffset;
		break;
		case Alignment.RightBot:
			minimapPosition = new Vector2(Screen.width - size,Screen.height - size) + positionOffset;
		break;
		case Alignment.MiddleTop:
			minimapPosition = new Vector2((Screen.width/2) - (size/2),size) + positionOffset;
		break;
		case Alignment.MiddleBot:
			minimapPosition = new Vector2((Screen.width/2) - (size/2),Screen.height - size*0.75f) + positionOffset;
		break;
		}

	}

	Vector2 ConvertToNavPosition(Vector3 pos)
	{
		if(GameManager.SharedObject().isFirstHalf == false)
		{
			pos.x *= -1f;
			pos.z *= -1f;
		}

		Vector2 res = Vector2.zero;
		if(player)
		{
			res.x = minimapPosition.x + ((pos.x+55)/110 * size);
			res.y = minimapPosition.y + ((-pos.z+37)/74 * (size*0.684f));
		}
		return res;
	}


	void DrawNav(GameObject[] enemylists,Texture2D navtexture){
		if(player)
		{
		for(int i=0;i<enemylists.Length;i++)
		{
			//if(Vector3.Distance(Player.transform.position,enemylists[i].transform.position)<= (Distance * Scale))
				{
				Vector2 pos = ConvertToNavPosition(enemylists[i].transform.position);

				//if(Vector2.Distance(pos,(inposition+ new Vector2(Size/2f,Size/2f))) + (navtexture.width/2) < (Size/2f))
					{
					float navscale = scale;
					if(navscale<1){
						navscale = 1;
					}
						GUI.DrawTexture(new Rect(pos.x - (10*Screen.height/640)/2,pos.y - (10*Screen.height/640)/2,10*Screen.height/640,10*Screen.height/640),navtexture);
				}
			}
		}
		}
	}

	float[] list;

	void OnGUI()
	{
		if(GameManager.SharedObject().isGameReady == false || PauseController.isPaused)
			return;

		if(backgroundTexture)
			GUI.DrawTexture(new Rect(minimapPosition.x,minimapPosition.y,size,size*0.684f),backgroundTexture);

		if(!show)
			return;

		GUI.color = new Color(colorMultiplier.r,colorMultiplier.g,colorMultiplier.b,alpha);
		if(mapRotation){
			GUIUtility.RotateAroundPivot (-(this.transform.eulerAngles.y), minimapPosition + new Vector2(size/2f, size/2f));
		}

		for(int i=0;i<trackedTags.Length;i++){
			DrawNav(GameObject.FindGameObjectsWithTag(trackedTags[i]),navigationTextures[i]);
		}

		GUIUtility.RotateAroundPivot ((this.transform.eulerAngles.y), minimapPosition + new Vector2(size/2f, size/2f));
		if(compassTexture)
		GUI.DrawTexture(new Rect(minimapPosition.x + (size/2f)-(compassTexture.width/2f),minimapPosition.y + (size/2f) - (compassTexture.height/2f),compassTexture.width,compassTexture.height),compassTexture);

	}
}
}
