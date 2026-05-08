using EFT.InventoryLogic;
using UnityEngine;

public class SightModVisualControllers : MonoBehaviour
{
	private ScopePrefabCache scopePrefabCache_0;

	private ScopeZoomHandler scopeZoomHandler_0;

	private SightComponent sightComponent_0;

	public SightComponent SightMod
	{
		get
		{
			return sightComponent_0;
		}
		set
		{
			sightComponent_0 = value;
			UpdateSightMode();
		}
	}

	public void Awake()
	{
		Init();
	}

	public void Init()
	{
		if (scopePrefabCache_0 == null)
		{
			scopePrefabCache_0 = GetComponent<ScopePrefabCache>();
		}
		if (scopeZoomHandler_0 == null)
		{
			scopeZoomHandler_0 = GetComponent<ScopeZoomHandler>();
		}
	}

	public void OnEnable()
	{
		if (scopePrefabCache_0 != null)
		{
			UpdateSightMode(setupZeroModeAnyway: true);
		}
	}

	public bool TryGetZoomHandler(out ScopeZoomHandler zoomHandler)
	{
		zoomHandler = scopeZoomHandler_0;
		return scopeZoomHandler_0 != null;
	}

	public void UpdateSightMode(bool setupZeroModeAnyway = false)
	{
		if (SightMod == null || scopePrefabCache_0 == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < SightMod.ScopesCount && i < SightMod.ScopesSelectedModes.Length; i++)
		{
			if (SightMod.GetScopeModesCount(i) == scopePrefabCache_0.ModesCount)
			{
				scopePrefabCache_0.SetMode(SightMod.ScopesSelectedModes[i]);
				flag = true;
				break;
			}
		}
		if (!flag && setupZeroModeAnyway)
		{
			scopePrefabCache_0.SetMode(0);
		}
	}

	public void ForceChangeScopeState()
	{
		if (TryGetZoomHandler(out var zoomHandler))
		{
			zoomHandler.ForceChangeScopeState();
		}
	}
}
