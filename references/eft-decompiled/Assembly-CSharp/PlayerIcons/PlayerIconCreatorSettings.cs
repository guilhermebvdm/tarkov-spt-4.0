using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using Diz.Jobs;
using Sirenix.Utilities;
using UnityEngine;

namespace PlayerIcons;

[CreateAssetMenu(fileName = "PlayerIconCreatorSettings", menuName = "Scriptable objects/PlayerIconCreatorSettings")]
public class PlayerIconCreatorSettings : ScriptableObject
{
	[Serializable]
	[CompilerGenerated]
	public class Class738
	{
		public static readonly Class738 class738_0 = new Class738();

		public static Action<GameObject> action_0;

		public void method_0(GameObject o)
		{
			o.SetActive(value: true);
		}
	}

	public const string PlayerIconCreatorSettingsName = "PlayerIconCreatorSettings";

	public const float GRAPHIC_SETTINGS_PREPARE = 0.1f;

	public const float MODEL_VIEW_PREPARE = 0.2f;

	public const float BUNDLES_LOADED = 0.6f;

	public const float PLAYER_BODY_INITED = 0.7f;

	public const float MODEL_CREATED = 0.8f;

	public const float SPRITE_RENDER_PREPARE = 0.9f;

	[Space]
	public Vector3 orthographicLocalPosition = new Vector3(0f, -1.5f, 3f);

	public Vector3 orthographicRotation = new Vector3(0f, 185f, 0f);

	public RenderingPath orthographicRenderingPath;

	public float orthographicSize = 0.4f;

	[Space]
	public Vector3 perspectiveLocalPosition = new Vector3(0f, -1.5f, 1.5f);

	public Vector3 perspectiveRotation = new Vector3(0f, 185f, 0f);

	public RenderingPath perspectiveRenderingPath;

	[Space]
	public bool isOrthographic = true;

	[Space]
	[Range(0.01f, 179f)]
	public float fieldOfView = 30f;

	public float nearClipPlane = 0.3f;

	public float farClipPlane = 10f;

	[Space]
	public bool isForceRenderActive;

	[Space]
	public bool isForceSetHelmetState;

	public bool isHelmetOn;

	[Space]
	public Vector2 textureSize = new Vector2(500f, 450f);

	[Space]
	public List<MeshMaterialSetting> meshMaterialSettings = new List<MeshMaterialSetting>();

	[Space]
	public bool isEnablePreview;

	[NonSerialized]
	public bool isEditInProgress;

	private GClass932 gclass932_0;

	public Vector3 LocalPosition
	{
		get
		{
			if (!isOrthographic)
			{
				return perspectiveLocalPosition;
			}
			return orthographicLocalPosition;
		}
	}

	public Quaternion Rotation => Quaternion.Euler(isOrthographic ? orthographicRotation : perspectiveRotation);

	public RenderingPath RenderingPath
	{
		get
		{
			if (!isOrthographic)
			{
				return perspectiveRenderingPath;
			}
			return orthographicRenderingPath;
		}
	}

	public void SaveLastRequest(GClass932 lastRequest)
	{
		gclass932_0 = lastRequest;
	}

	public async Task CheckForcePause(params GameObject[] objects)
	{
		if (isEditInProgress)
		{
			Debug.Log("icon render pause", objects.FirstOrDefault());
			objects.ForEach(delegate(GameObject o)
			{
				o.SetActive(value: true);
			});
			while (isEditInProgress)
			{
				await JobScheduler.Yield();
			}
		}
	}

	public bool TryForceRenderLastIco(out GClass929 icon)
	{
		if (gclass932_0 == null)
		{
			Debug.LogError("last request is empty");
			icon = null;
			return false;
		}
		isForceRenderActive = true;
		icon = Singleton<GClass927>.Instance.GetIcon(gclass932_0);
		return true;
	}
}
