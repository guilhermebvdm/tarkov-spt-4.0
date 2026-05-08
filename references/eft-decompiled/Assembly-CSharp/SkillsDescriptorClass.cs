using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using Newtonsoft.Json;
using UnityEngine;

[GAttribute29]
public class SkillsDescriptorClass
{
	[GAttribute29]
	public class GClass2225
	{
		[JsonProperty("Id")]
		public ESkillId Id;

		[JsonProperty("Progress")]
		public float Progress;

		[JsonProperty("PointsEarnedDuringSession")]
		public float PointsEarnedDuringSession;

		[JsonProperty("LastAccess")]
		public int LastAccess;
	}

	[GAttribute29]
	public class GClass2226
	{
		[JsonProperty("Id")]
		public string Id;

		[JsonProperty("Progress")]
		public float Progress;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1418
	{
		public static readonly Class1418 class1418_0 = new Class1418();

		public static Func<SkillClass, GClass2225> func_0;

		public static Func<KeyValuePair<string, MasterSkillClass>, GClass2226> func_1;

		public GClass2225 method_0(SkillClass skill)
		{
			return new GClass2225
			{
				Id = skill.Id,
				Progress = Mathf.Clamp(skill.Current, 0f, 5100f),
				LastAccess = (int)EFTDateTimeClass.ToUnixTime(skill.LastCall),
				PointsEarnedDuringSession = skill.PointsEarned
			};
		}

		public GClass2226 method_1(KeyValuePair<string, MasterSkillClass> m)
		{
			return new GClass2226
			{
				Id = m.Value.MasteringGroup.Id,
				Progress = m.Value.Current
			};
		}
	}

	[JsonProperty("Common")]
	public GClass2225[] Common = Array.Empty<GClass2225>();

	[JsonProperty("Mastering")]
	public GClass2226[] Mastering = Array.Empty<GClass2226>();

	public SkillsDescriptorClass()
	{
	}

	public SkillsDescriptorClass(SkillManager skillManager)
	{
		if (skillManager.Skills != null)
		{
			Common = skillManager.Skills.Select((SkillClass skill) => new GClass2225
			{
				Id = skill.Id,
				Progress = Mathf.Clamp(skill.Current, 0f, 5100f),
				LastAccess = (int)EFTDateTimeClass.ToUnixTime(skill.LastCall),
				PointsEarnedDuringSession = skill.PointsEarned
			}).ToArray();
		}
		if (skillManager.Mastering != null)
		{
			Mastering = skillManager.Mastering.Select((KeyValuePair<string, MasterSkillClass> m) => new GClass2226
			{
				Id = m.Value.MasteringGroup.Id,
				Progress = m.Value.Current
			}).ToArray();
		}
	}
}
