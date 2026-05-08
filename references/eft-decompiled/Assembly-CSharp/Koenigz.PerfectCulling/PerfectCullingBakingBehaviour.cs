using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling;

public abstract class PerfectCullingBakingBehaviour : MonoBehaviour, GInterface111
{
	private static GInterface115 ginterface115_0 = new GClass1230();

	public HashSet<GInterface115> SamplingProviders = new HashSet<GInterface115> { ginterface115_0 };

	[SerializeField]
	[HideInInspector]
	public PerfectCullingBakeGroup[] _bakeGroups = Array.Empty<PerfectCullingBakeGroup>();

	[SerializeField]
	public List<Renderer> _additionalOccluders = new List<Renderer>();

	[CompilerGenerated]
	private readonly PerfectCullingBakeData perfectCullingBakeData_0;

	[NonSerialized]
	public int TotalVertexCount;

	[CompilerGenerated]
	private static bool bool_0;

	public virtual PerfectCullingBakeGroup[] bakeGroups
	{
		get
		{
			return _bakeGroups;
		}
		set
		{
			_bakeGroups = value;
		}
	}

	public virtual List<Renderer> additionalOccluders
	{
		get
		{
			return _additionalOccluders;
		}
		set
		{
			_additionalOccluders = value;
		}
	}

	public virtual PerfectCullingBakeData BakeData
	{
		[CompilerGenerated]
		get
		{
			return perfectCullingBakeData_0;
		}
	}

	public static bool IsBakeActive
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void AddSamplingProvider(GInterface115 samplingProvider)
	{
		SamplingProviders.Add(samplingProvider);
	}

	public void RemoveSamplingProvider(GInterface115 samplingProvider)
	{
		SamplingProviders.Remove(samplingProvider);
	}

	public void InitializeAllSamplingProviders()
	{
		foreach (GInterface115 samplingProvider in SamplingProviders)
		{
			samplingProvider.InitializeSamplingProvider();
		}
	}

	public bool SamplingProvidersIsPositionActive(Vector3 pos)
	{
		foreach (GInterface115 samplingProvider in SamplingProviders)
		{
			if (!samplingProvider.IsSamplingPositionActive(this, pos))
			{
				return false;
			}
		}
		return true;
	}

	public virtual void Start()
	{
	}

	public void ToggleAllRenderers(bool state, bool forceNullCheck = false)
	{
		PerfectCullingBakeGroup[] array = bakeGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Toggle(state);
		}
	}

	public virtual void SetBakeData(PerfectCullingBakeData bakeData)
	{
		throw new NotImplementedException();
	}

	public virtual List<Vector3> GetSamplingPositions(Space space = Space.Self)
	{
		throw new NotImplementedException();
	}

	public virtual Vector3 GetWorldPositionForIndex(int index)
	{
		return Vector3.zero;
	}

	public virtual void GetIndicesForWorldPos(Vector3 worldPos, List<ushort> indices)
	{
		throw new NotImplementedException();
	}

	public virtual int GetIndexForWorldPos(Vector3 worldPos, out bool isOutOfBounds)
	{
		throw new NotImplementedException();
	}

	public virtual void GetIndicesForIndex(int index, List<ushort> indices)
	{
		BakeData.SampleAtIndex(index, indices);
	}

	public virtual bool PreBake()
	{
		throw new NotImplementedException();
	}

	public virtual void PostBake()
	{
		throw new NotImplementedException();
	}

	public virtual int GetBakeHash()
	{
		throw new NotImplementedException();
	}

	public virtual void CullAdditionalOccluders(ref HashSet<Renderer> additionalOccluders)
	{
		throw new NotImplementedException();
	}

	public virtual void InitializeBake()
	{
	}

	public virtual void PostProcessBakeData(PerfectCullingBakeData data)
	{
	}

	public PerfectCullingBakingBehaviour()
	{
	}
}
