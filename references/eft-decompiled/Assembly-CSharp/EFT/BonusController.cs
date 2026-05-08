using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace EFT;

public class BonusController
{
	public class Class1131
	{
		[NonSerialized]
		public List<SkillBonusAbstractClass> List_0 = new List<SkillBonusAbstractClass>();

		[NonSerialized]
		[CompilerGenerated]
		public SkillBonusAbstractClass SkillBonusAbstractClass;

		public SkillBonusAbstractClass Result
		{
			[CompilerGenerated]
			get
			{
				return SkillBonusAbstractClass;
			}
			[CompilerGenerated]
			set
			{
				SkillBonusAbstractClass = value;
			}
		}

		public Class1131(SkillBonusAbstractClass bonus)
		{
			Add(bonus);
		}

		public double CalculateValue(double input)
		{
			if (Result == null)
			{
				return input;
			}
			return Result.CalculateValue(input);
		}

		public void Add(SkillBonusAbstractClass bonus)
		{
			if (bonus != null && !method_0(bonus.Id, out var _))
			{
				List_0.Add(bonus);
				method_1();
				method_2();
			}
		}

		public void Remove(SkillBonusAbstractClass bonus)
		{
			if (bonus != null && method_0(bonus.Id, out bonus) && List_0.Remove(bonus))
			{
				method_2();
			}
		}

		public bool method_0(MongoID id, out SkillBonusAbstractClass bonus)
		{
			foreach (SkillBonusAbstractClass item in List_0)
			{
				if (!(item.Id != id))
				{
					bonus = item;
					return true;
				}
			}
			bonus = null;
			return false;
		}

		public bool HasBonus()
		{
			return List_0.Count > 0;
		}

		public void SetBoost(double boostValue)
		{
			if (!(Result is GClass1817 gClass))
			{
				return;
			}
			gClass.BoostValue = boostValue;
			foreach (GClass1817 item in List_0.OfType<GClass1817>())
			{
				item.BoostValue = boostValue;
			}
		}

		public void method_1()
		{
			if (Result is GClass1817 gClass)
			{
				SetBoost(gClass.BoostValue);
			}
		}

		public void method_2()
		{
			Result = null;
			foreach (SkillBonusAbstractClass item in List_0)
			{
				if (Result == null)
				{
					Result = item.Clone();
				}
				else
				{
					Result.Add(item);
				}
			}
		}

		public IEnumerable<ProfileBonusesClass> ToDescriptors()
		{
			foreach (SkillBonusAbstractClass item in List_0)
			{
				yield return item.ToDescriptor();
			}
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1133
	{
		public static readonly Class1133 class1133_0 = new Class1133();

		public static Func<Class1131, IEnumerable<ProfileBonusesClass>> func_0;

		public IEnumerable<ProfileBonusesClass> method_0(Class1131 bonus)
		{
			return bonus.ToDescriptors();
		}
	}

	[NonSerialized]
	public Dictionary<Enum, Class1131> ActiveBonuses_1 = new Dictionary<Enum, Class1131>();

	[NonSerialized]
	public SkillManager SkillManager;

	public float Single_0 => SkillManager?.ZoneBonusBoost.Value ?? 0f;

	[JsonIgnore]
	[field: NonSerialized]
	public string StashTemplateId { get; set; }

	[JsonIgnore]
	public IEnumerable<EBonusType> ActiveBonuses
	{
		get
		{
			foreach (var (obj2, class2) in ActiveBonuses_1)
			{
				if (obj2 is EBonusType eBonusType && class2.HasBonus())
				{
					yield return eBonusType;
				}
			}
		}
	}

	public event Action<bool, EBonusType> OnBonusChanged;

	public BonusController(IEnumerable<SkillBonusAbstractClass> bonuses, SkillManager skillManager)
	{
		foreach (SkillBonusAbstractClass bonuse in bonuses)
		{
			AddBonus(bonuse);
		}
		method_0(skillManager);
	}

	public void method_0(SkillManager skillManager)
	{
		if (SkillManager != null)
		{
			SkillManager.OnSkillLevelChanged -= method_1;
		}
		SkillManager = skillManager;
		SkillManager.OnSkillLevelChanged += method_1;
		method_2();
	}

	public void method_1(AbstractSkillClass baseSkill)
	{
		if (baseSkill.Id == ESkillId.HideoutManagement)
		{
			method_2();
		}
	}

	public void method_2()
	{
		foreach (KeyValuePair<Enum, Class1131> item in ActiveBonuses_1)
		{
			item.Deconstruct(out var _, out var value);
			value.SetBoost(Single_0);
		}
	}

	public void method_3(SkillBonusAbstractClass bonus)
	{
		if (bonus is GClass1817 gClass)
		{
			gClass.BoostValue = Single_0;
		}
	}

	public void AddBonus(SkillBonusAbstractClass bonus, bool silent = false)
	{
		if (bonus != null)
		{
			method_3(bonus);
			if (bonus is GClass1816 gClass)
			{
				StashTemplateId = gClass.TemplateId;
			}
			if (!ActiveBonuses_1.TryGetValue(bonus.TargetType, out var value))
			{
				ActiveBonuses_1.Add(bonus.TargetType, new Class1131(bonus));
			}
			else
			{
				value.Add(bonus);
			}
			if (!silent)
			{
				this.OnBonusChanged?.Invoke(arg1: true, bonus.BonusType);
			}
		}
	}

	public void RemoveBonus(SkillBonusAbstractClass bonus, bool silent = false)
	{
		if (bonus != null && ActiveBonuses_1.TryGetValue(bonus.TargetType, out var value))
		{
			value.Remove(bonus);
			if (value.Result == null)
			{
				ActiveBonuses_1.Remove(bonus.TargetType);
			}
			if (!silent)
			{
				this.OnBonusChanged?.Invoke(arg1: false, bonus.BonusType);
			}
		}
	}

	public bool HasBonus(EBonusType type)
	{
		if (ActiveBonuses_1.TryGetValue(type, out var value))
		{
			return value.HasBonus();
		}
		return false;
	}

	public double Calculate(SkillClass skill, double input)
	{
		input = method_4(skill.Class, input);
		return method_4(skill.Id, input);
	}

	public double Calculate(EBonusType type, double input)
	{
		return method_4(type, input);
	}

	public double method_4(Enum type, double input)
	{
		if (!ActiveBonuses_1.TryGetValue(type, out var value))
		{
			return input;
		}
		return value.CalculateValue(input);
	}

	public void Reset()
	{
		List<EBonusType> list = new List<EBonusType>();
		foreach (var (_, class2) in ActiveBonuses_1)
		{
			if (class2.Result != null)
			{
				EBonusType bonusType = class2.Result.BonusType;
				if (!list.Contains(bonusType))
				{
					list.Add(bonusType);
				}
			}
		}
		ActiveBonuses_1.Clear();
		foreach (EBonusType item in list)
		{
			this.OnBonusChanged?.Invoke(arg1: false, item);
		}
	}

	public IEnumerable<SkillBonusAbstractClass> GetAttachedBonuses(EAreaType areaType)
	{
		foreach (var (_, class2) in ActiveBonuses_1)
		{
			if (class2.Result is GInterface172 gInterface && gInterface.AreaType == areaType)
			{
				yield return class2.Result.Clone();
			}
		}
	}

	public ProfileBonusesClass[] ToDescriptor()
	{
		return ActiveBonuses_1.Values.SelectMany((Class1131 bonus) => bonus.ToDescriptors()).ToArray();
	}
}
