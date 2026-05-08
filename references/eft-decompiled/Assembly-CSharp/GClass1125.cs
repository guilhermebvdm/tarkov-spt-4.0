using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using JetBrains.Annotations;
using UnityEngine;

public class GClass1125 : Singleton<GClass1125>
{
	[Serializable]
	[CompilerGenerated]
	public class Class787
	{
		public static readonly Class787 class787_0 = new Class787();

		public static Func<BetterPropagationVolume, string> func_0;

		public static Func<BetterPropagationVolume, string> func_1;

		public string method_0(BetterPropagationVolume x)
		{
			if (!(x == null))
			{
				return x.name;
			}
			return "null";
		}

		public string method_1(BetterPropagationVolume x)
		{
			if (!(x == null))
			{
				return x.name;
			}
			return "null";
		}
	}

	[CompilerGenerated]
	public class Class788
	{
		public Vector3 firstPos;

		public Vector3 secondPos;

		public bool method_0(BetterPropagationVolume volume)
		{
			if (GClass860.ContainsPoint(volume.Collider, firstPos))
			{
				return !GClass860.ContainsPoint(volume.Collider, secondPos);
			}
			return false;
		}
	}

	[NonSerialized]
	public List<BetterPropagationVolume> List_0 = new List<BetterPropagationVolume>();

	[NonSerialized]
	public List<BetterPropagationVolume> List_1 = new List<BetterPropagationVolume>();

	[NonSerialized]
	public List<BetterPropagationVolume> List_2 = new List<BetterPropagationVolume>(50);

	[NonSerialized]
	public List<BetterPropagationVolume> List_3 = new List<BetterPropagationVolume>(50);

	[NonSerialized]
	public List<BetterPropagationVolume> List_4 = new List<BetterPropagationVolume>(50);

	[NonSerialized]
	public static bool Bool_0;

	[NonSerialized]
	public BetterPropagationGroups BetterPropagationGroups_0;

	[CanBeNull]
	public BetterPropagationVolume GetPlayerCurrentPropagationVolume()
	{
		if (List_4.Count == 0)
		{
			return null;
		}
		BetterPropagationVolume betterPropagationVolume = List_4[0];
		if (!(betterPropagationVolume != null))
		{
			return GetVolumeByPosition(CameraClass.Instance.Camera.transform.position);
		}
		return betterPropagationVolume;
	}

	public void AddPlayerCurrentPropagationVolume(BetterPropagationVolume volume)
	{
		if (List_4.Contains(volume))
		{
			int index = List_4.IndexOf(volume);
			List_4.RemoveAt(index);
			List_4.Insert(0, volume);
		}
		else
		{
			List_4.Insert(0, volume);
		}
	}

	public void RemovePlayerCurrentVolume(BetterPropagationVolume volume)
	{
		if (List_4.Contains(volume))
		{
			List_4.Remove(volume);
		}
	}

	public void RegisterVolume(BetterPropagationVolume volume)
	{
		List_0.Add(volume);
		if (volume.IsIsolated)
		{
			List_1.Add(volume);
		}
	}

	public void RemoveVolume(BetterPropagationVolume volume)
	{
		if (List_0 != null)
		{
			List_0.Remove(volume);
			if (volume.IsIsolated)
			{
				List_1.Remove(volume);
			}
		}
	}

	public void RegisterGroup(BetterPropagationGroups group)
	{
		BetterPropagationGroups_0 = group;
	}

	public bool IsPlayersInDifferentVolumes(Player listenerPlayer, Player sourcePlayer)
	{
		if ((object)listenerPlayer != null && (object)sourcePlayer != null)
		{
			if (listenerPlayer.PointOfView != EPointOfView.FirstPerson)
			{
				return false;
			}
			List<BetterPropagationVolume> propagationVolume = listenerPlayer.GetPropagationVolume();
			List<BetterPropagationVolume> propagationVolume2 = sourcePlayer.GetPropagationVolume();
			return method_0(propagationVolume, propagationVolume2);
		}
		return false;
	}

	public bool method_0(List<BetterPropagationVolume> listenerVolumes, List<BetterPropagationVolume> sourceVolumes)
	{
		if (listenerVolumes.Count == 0 && sourceVolumes.Count == 0)
		{
			return false;
		}
		if (listenerVolumes.Count != 0 && sourceVolumes.Count != 0)
		{
			for (int i = 0; i < listenerVolumes.Count; i++)
			{
				for (int j = 0; j < sourceVolumes.Count; j++)
				{
					if (listenerVolumes[i] == sourceVolumes[j])
					{
						return false;
					}
				}
			}
			return true;
		}
		return true;
	}

	public bool IsPositionsInDifferentVolume(Vector3 firstPos, Vector3 secondPos)
	{
		return List_0.Any((BetterPropagationVolume volume) => GClass860.ContainsPoint(volume.Collider, firstPos) && !GClass860.ContainsPoint(volume.Collider, secondPos));
	}

	public bool IsPlayerAndSourceInSameVolume(Vector3 sourcePos)
	{
		BetterPropagationVolume playerCurrentPropagationVolume = GetPlayerCurrentPropagationVolume();
		BetterPropagationVolume volumeByPosition = GetVolumeByPosition(sourcePos);
		return (object)playerCurrentPropagationVolume == volumeByPosition;
	}

	[CanBeNull]
	public BetterPropagationVolume GetVolumeByPosition(Vector3 position)
	{
		BetterPropagationVolume playerCurrentPropagationVolume = GetPlayerCurrentPropagationVolume();
		if (playerCurrentPropagationVolume != null && GClass860.ContainsPoint(playerCurrentPropagationVolume.Collider, position))
		{
			return playerCurrentPropagationVolume;
		}
		foreach (BetterPropagationVolume item in List_0)
		{
			try
			{
				if (!GClass860.ContainsPoint(item.Collider, position))
				{
					continue;
				}
			}
			catch (Exception)
			{
				method_4("GetVolumeByPosition", item, position);
				continue;
			}
			return item;
		}
		return null;
	}

	public List<BetterPropagationVolume> GetVolumesByPosition(Vector3 position)
	{
		if (BetterPropagationGroups_0 != null)
		{
			BetterPropagationGroups_0.GetVolumes(position, in List_2);
			return List_2;
		}
		List_2.Clear();
		foreach (BetterPropagationVolume item in List_0)
		{
			try
			{
				if (!GClass860.ContainsPoint(item.Collider, position))
				{
					continue;
				}
			}
			catch (Exception)
			{
				method_4("GetVolumeByPosition", item, position);
				continue;
			}
			if (!item.MutuallyExclusive)
			{
				List_2.Add(item);
				continue;
			}
			List_2.Clear();
			List_2.Add(item);
			return List_2;
		}
		return List_2;
	}

	public void method_1(Vector3 position, List<BetterPropagationVolume> volumesBuffer)
	{
		foreach (BetterPropagationVolume item in volumesBuffer)
		{
			try
			{
				if (GClass860.ContainsPoint(item.Collider, position))
				{
					List_3.Add(item);
				}
			}
			catch (Exception)
			{
				method_4("volumes", item, position);
				continue;
			}
			method_2(position, item);
		}
	}

	public void method_2(Vector3 position, BetterPropagationVolume propagationVolume)
	{
		foreach (VolumeConnection connection in propagationVolume.Connections)
		{
			try
			{
				if (GClass860.ContainsPoint(connection.Volume.Collider, position))
				{
					List_3.Add(connection.Volume);
				}
			}
			catch (Exception)
			{
				method_4("connections", connection.Volume, position);
			}
		}
	}

	public void method_3(Vector3 position, List<BetterPropagationVolume> volumesBuffer)
	{
		foreach (BetterPropagationVolume item in volumesBuffer)
		{
			try
			{
				if (!GClass860.ContainsPoint(item.Collider, position))
				{
					continue;
				}
			}
			catch (Exception)
			{
				method_4("isolatedVolumes", item, position);
			}
			if (!List_3.Contains(item))
			{
				List_3.Add(item);
			}
		}
	}

	public List<BetterPropagationVolume> GetAdjustedAndIsolatedVolumes(Vector3 position, List<BetterPropagationVolume> volumesBuffer)
	{
		List_3.Clear();
		method_1(position, volumesBuffer);
		if (BetterPropagationGroups_0 != null)
		{
			return BetterPropagationGroups_0.GetIsolatedVolumes(position, List_3);
		}
		method_3(position, List_1);
		return List_3;
	}

	public List<BetterPropagationVolume> GetIsolatedVolumes()
	{
		return List_1;
	}

	public void method_4(string errorCaption, BetterPropagationVolume vol, Vector3 position)
	{
		if (Bool_0)
		{
			return;
		}
		Bool_0 = true;
		if (vol == null)
		{
			string text = "_volumes: " + string.Join("\n", List_0.Select((BetterPropagationVolume x) => (!(x == null)) ? x.name : "null").ToArray());
			text = text + "_isolatedVolumes: " + string.Join("\n", List_1.Select((BetterPropagationVolume x) => (!(x == null)) ? x.name : "null").ToArray());
			Debug.LogErrorFormat("Error.Collider.ContainsPoint.{0} vol is null in position {1}, volumes {2}", errorCaption, position, text);
		}
		else
		{
			Debug.LogErrorFormat("Error.Collider.ContainsPoint.{0} vol.name {1} position {2} box is null {3} box.name {4}", errorCaption, vol.name, position, vol.Collider == null, (vol.Collider == null) ? string.Empty : vol.Collider.name);
		}
	}
}
