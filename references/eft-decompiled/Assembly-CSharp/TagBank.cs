using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TagBank : ScriptableObject
{
	[Serializable]
	[CompilerGenerated]
	public class Class458
	{
		public static readonly Class458 class458_0 = new Class458();

		public static Func<SpreadGroup, IEnumerable<TaggedClip>> func_0;

		public IEnumerable<TaggedClip> method_0(SpreadGroup g)
		{
			return g.Clips;
		}
	}

	public EPhraseTrigger Trigger;

	public SpreadGroup[] SpreadGroups = Array.Empty<SpreadGroup>();

	public TaggedClip[] Clips;

	public Chain ChainEvent;

	public static int[] Sizes = new int[6] { 3, 2, 3, 2, 4, 3 };

	public int Importance;

	public float Blocker;

	public bool IgnoreTags;

	private List<TaggedClip> list_0;

	private List<TaggedClip> list_1;

	public TaggedClip Match(ETagStatus combat = ETagStatus.Unaware | ETagStatus.Aware | ETagStatus.Combat, ETagStatus speakerGroup = ETagStatus.Solo | ETagStatus.Coop, ETagStatus targetGroup = ETagStatus.TargetSolo | ETagStatus.TargetMultiple, ETagStatus health = ETagStatus.Healthy | ETagStatus.Injured | ETagStatus.BadlyInjured | ETagStatus.Dying, ETagStatus side = ETagStatus.Bear | ETagStatus.Usec | ETagStatus.Scav, ETagStatus exUsecBoss = ETagStatus.Birdeye | ETagStatus.Knight | ETagStatus.BigPipe)
	{
		return Match((int)(combat | speakerGroup | targetGroup | health | side | exUsecBoss));
	}

	public TaggedClip Match(int mask)
	{
		if (list_1 == null)
		{
			list_1 = new List<TaggedClip>(Clips.Length);
			list_0 = new List<TaggedClip>(Clips.Length);
		}
		else
		{
			list_1.Clear();
			list_0.Clear();
		}
		List<TaggedClip> list = list_1;
		List<TaggedClip> list2 = list_0;
		for (int i = 0; i < Clips.Length; i++)
		{
			TaggedClip taggedClip = Clips[i];
			if (IgnoreTags || Compare(taggedClip.Mask, mask))
			{
				if (taggedClip.Exclude)
				{
					list2.Add(taggedClip);
				}
				else
				{
					list.Add(taggedClip);
				}
			}
		}
		if (list.Count == 1)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				list2[j].Exclude = false;
			}
		}
		else if (list.Count == 0)
		{
			for (int k = 0; k < list2.Count; k++)
			{
				list2[k].Exclude = false;
			}
			list = list2;
		}
		if (list.Count == 0)
		{
			return null;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		TaggedClip taggedClip2 = list[index];
		taggedClip2.Exclude = true;
		list_0.Clear();
		list_1.Clear();
		return taggedClip2;
	}

	public void OnValidate()
	{
		Clips = SpreadGroups.SelectMany((SpreadGroup g) => g.Clips).ToArray();
		for (int num = 0; num < Clips.Length; num++)
		{
			Clips[num].NetId = num;
			if (Clips[num].Clip != null)
			{
				Clips[num].Length = Clips[num].Clip.length;
			}
		}
		method_0();
	}

	public void method_0()
	{
		for (int i = 0; i < SpreadGroups.Length; i++)
		{
			SpreadGroup spreadGroup = SpreadGroups[i];
			for (int j = 0; j < spreadGroup.Clips.Length; j++)
			{
				TaggedClip taggedClip = spreadGroup.Clips[j];
				if (taggedClip != null)
				{
					taggedClip.Volume = spreadGroup.Volume;
					taggedClip.Falloff = spreadGroup.Falloff;
				}
			}
		}
	}

	public static bool Compare(int mask1, int @event)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			if (num2 < Sizes.Length)
			{
				int width = Sizes[num2];
				if (num2 > 0)
				{
					num += Sizes[num2 - 1];
				}
				int bits = GetBits(mask1, num, width);
				int bits2 = GetBits(@event, num, width);
				if (bits != 0 && bits2 != 0 && (bits & bits2) == 0)
				{
					break;
				}
				num2++;
				continue;
			}
			return true;
		}
		return false;
	}

	public static int GetBits(int mask, int offset, int width)
	{
		return ((1 << width) - 1 << offset) & mask;
	}
}
