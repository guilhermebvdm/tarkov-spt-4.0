using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;

namespace Audio.SpatialSystem;

[Serializable]
[RequireComponent(typeof(BoxCollider))]
public class MultiWindowPortal : BaseSpatialAudioPortal, IIdentifiable
{
	[Serializable]
	public class WindowData
	{
		public string windowID;

		public float occupiedRatio;

		public float traversalCost;

		public Vector2 size;

		public float area;

		public float depth;

		[HideInInspector]
		public Vector3 colliderCenter;

		[HideInInspector]
		public Quaternion colliderRotation;
	}

	[SerializeField]
	private List<WindowData> _serializedWindowsData = new List<WindowData>();

	[SerializeField]
	private float _totalOccupiedArea;

	[SerializeField]
	private float _totalOccupiedRatio;

	[SerializeField]
	private float _freeAreaTraversalCost;

	[SerializeField]
	private float _totalTraversalMaxCost;

	[SerializeField]
	private float _totalDepth;

	[SerializeField]
	private float _freeAreaDepth;

	[SerializeField]
	private Vector2 _freeAreaSize;

	[SerializeField]
	private float _freeArea;

	[SerializeField]
	private float _totalArea;

	[SerializeField]
	private Vector2 _totalAreaSize;

	private Dictionary<string, WindowData> _windowsMap = new Dictionary<string, WindowData>();

	public IReadOnlyCollection<string> Ids => _windowsMap.Keys;

	public override void Subscribe()
	{
		WindowBreaker.OnWindowHitAction += method_8;
		base.Subscribe();
	}

	public override void Unsubscribe()
	{
		WindowBreaker.OnWindowHitAction -= method_8;
		base.Unsubscribe();
	}

	public override async Task Init()
	{
		foreach (WindowData serializedWindowsDatum in _serializedWindowsData)
		{
			string windowID = serializedWindowsDatum.windowID;
			if (!string.IsNullOrEmpty(windowID))
			{
				_windowsMap[windowID] = serializedWindowsDatum;
			}
		}
		await base.Init();
	}

	public override void SyncState()
	{
		foreach (WindowData serializedWindowsDatum in _serializedWindowsData)
		{
			if (Singleton<GameWorld>.Instance.Windows.TryGetByKey(serializedWindowsDatum.windowID.GetHashCode(), out var value) && (value.IsDamaged || value.BrakeByDefault))
			{
				_windowsMap.Remove(serializedWindowsDatum.windowID);
				method_9(serializedWindowsDatum);
				method_10(serializedWindowsDatum);
				method_11(serializedWindowsDatum);
				if (base.State == PortalState.Open)
				{
					method_7();
					return;
				}
				SetPortalOpen(allowFade: false);
			}
		}
		base.SyncState();
	}

	public void method_7()
	{
		GlobalEventHandlerClass.CreateEvent<GClass3568>().Invoke(base.ID, PortalState.Open, base.PortalClosureLevel, Depth, traversalMaxCost);
	}

	public void method_8(WindowBreaker breaker, DamageInfoStruct info, WindowBreakingConfig.Crack crack, float damage)
	{
		if (_windowsMap.TryGetValue(breaker.Id, out var value))
		{
			_windowsMap.Remove(breaker.Id);
			method_9(value);
			method_10(value);
			method_11(value);
			if (base.State == PortalState.Open)
			{
				method_7();
			}
			else if (breaker.IsDamaged)
			{
				SetPortalOpen(allowFade: false);
			}
		}
	}

	public void method_9(WindowData data)
	{
		if (data != null)
		{
			if (_windowsMap.Count == 0)
			{
				traversalMaxCost = _totalTraversalMaxCost;
				_freeAreaTraversalCost = _totalTraversalMaxCost;
			}
			else
			{
				float freeAreaTraversalCost = Mathf.Clamp(Mathf.Min(traversalMaxCost, data.traversalCost), _totalTraversalMaxCost, 1f);
				_freeAreaTraversalCost = freeAreaTraversalCost;
				traversalMaxCost = Mathf.Clamp(_freeAreaTraversalCost, _totalTraversalMaxCost, 1f);
			}
		}
	}

	public void method_10(WindowData data)
	{
		if (data != null)
		{
			if (_windowsMap.Count == 0)
			{
				Depth = _totalDepth;
				_freeAreaDepth = Depth;
				return;
			}
			float num = data.occupiedRatio / _totalOccupiedRatio;
			float num2 = _totalDepth - _freeAreaDepth;
			Depth = Mathf.Clamp(Depth + num * num2, _freeAreaDepth, _totalDepth);
			_freeAreaDepth = Depth;
		}
	}

	public void method_11(WindowData data)
	{
		if (data != null)
		{
			if (_windowsMap.Count == 0)
			{
				_freeArea = _totalArea;
			}
			else
			{
				_freeArea = Mathf.Min(_freeArea + data.area, _totalArea);
			}
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	public Task method_12()
	{
		return base.Init();
	}
}
