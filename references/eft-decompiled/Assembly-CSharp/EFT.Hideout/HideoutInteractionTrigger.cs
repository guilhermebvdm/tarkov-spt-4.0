using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT.Interactive;
using UnityEngine;

namespace EFT.Hideout;

public class HideoutInteractionTrigger : InteractableObject, GInterface245, IPhysicsTrigger
{
	[CompilerGenerated]
	private HideoutArea hideoutArea_0;

	[CompilerGenerated]
	private readonly string string_0 = "HideoutInteractionTrigger";

	public HideoutArea Area
	{
		[CompilerGenerated]
		get
		{
			return hideoutArea_0;
		}
		[CompilerGenerated]
		set
		{
			hideoutArea_0 = value;
		}
	}

	public string Description
	{
		[CompilerGenerated]
		get
		{
			return string_0;
		}
	}

	bool IPhysicsTrigger.enabled
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public void Init(HideoutArea area)
	{
		Area = area;
	}

	public void OnTriggerEnter(Collider col)
	{
		Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
		if (!(playerByCollider == null))
		{
			HideoutPlayerOwner component = playerByCollider.GetComponent<HideoutPlayerOwner>();
			if (!(component == null))
			{
				component.HideoutAreaInteraction(Area, inSight: true);
			}
		}
	}

	public void OnTriggerExit(Collider col)
	{
		Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
		if (!(playerByCollider == null))
		{
			HideoutPlayerOwner component = playerByCollider.GetComponent<HideoutPlayerOwner>();
			if (!(component == null))
			{
				component.HideoutAreaInteraction(Area, inSight: false);
			}
		}
	}
}
