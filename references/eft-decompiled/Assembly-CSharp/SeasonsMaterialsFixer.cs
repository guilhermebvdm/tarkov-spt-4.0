using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class SeasonsMaterialsFixer : MonoBehaviourSingleton<SeasonsMaterialsFixer>
{
	[FormerlySerializedAs("WinterMaterials")]
	[SerializeField]
	private SeasonsMaterialsMap _materialsMap;

	private SeasonsMaterialsGroup seasonsMaterialsGroup_0;

	public static LoggerClass LoggerClass => Class443.Logger;

	public override void Awake()
	{
		seasonsMaterialsGroup_0 = SeasonsMaterialsGroup.Create("RuntimeGroup");
		base.Awake();
	}

	public void Add(GInterface43 material)
	{
		if (seasonsMaterialsGroup_0 != null)
		{
			LoggerClass.LogTrace("SeasonsMaterialsFixer add");
			seasonsMaterialsGroup_0.Materials.Add(material);
		}
		else
		{
			LoggerClass.LogTrace("ISeasonsMaterial.Fix()");
			material.Fix();
		}
	}

	public void Remove(GInterface43 material)
	{
		if (seasonsMaterialsGroup_0 != null)
		{
			seasonsMaterialsGroup_0.Materials.Remove(material);
		}
	}

	public Task method_0()
	{
		Task task = ((_materialsMap != null) ? _materialsMap.LoadSummer() : Task.CompletedTask);
		Task task2 = ((seasonsMaterialsGroup_0 != null) ? seasonsMaterialsGroup_0.LoadSummer() : Task.CompletedTask);
		return Task.WhenAll(task, task2);
	}

	public Task method_1()
	{
		Task task = ((_materialsMap != null) ? _materialsMap.LoadWinter() : Task.CompletedTask);
		Task task2 = ((seasonsMaterialsGroup_0 != null) ? seasonsMaterialsGroup_0.LoadWinter() : Task.CompletedTask);
		return Task.WhenAll(task, task2);
	}

	public Task method_2()
	{
		Task task = ((_materialsMap != null) ? _materialsMap.LoadSpringEarly() : Task.CompletedTask);
		Task task2 = ((seasonsMaterialsGroup_0 != null) ? seasonsMaterialsGroup_0.LoadSpringEarly() : Task.CompletedTask);
		return Task.WhenAll(task, task2);
	}

	public Task method_3()
	{
		Task task = ((_materialsMap != null) ? _materialsMap.LoadSpring() : Task.CompletedTask);
		Task task2 = ((seasonsMaterialsGroup_0 != null) ? seasonsMaterialsGroup_0.LoadSpring() : Task.CompletedTask);
		return Task.WhenAll(task, task2);
	}

	public Task method_4()
	{
		Task task = ((_materialsMap != null) ? _materialsMap.LoadAutumn() : Task.CompletedTask);
		Task task2 = ((seasonsMaterialsGroup_0 != null) ? seasonsMaterialsGroup_0.LoadAutumn() : Task.CompletedTask);
		return Task.WhenAll(task, task2);
	}

	public Task method_5()
	{
		Task task = ((_materialsMap != null) ? _materialsMap.LoadAutumnLate() : Task.CompletedTask);
		Task task2 = ((seasonsMaterialsGroup_0 != null) ? seasonsMaterialsGroup_0.LoadAutumnLate() : Task.CompletedTask);
		return Task.WhenAll(task, task2);
	}

	public void FixMaterials()
	{
		LoggerClass.LogTrace("FixMaterials() begin");
		if (_materialsMap != null)
		{
			_materialsMap.Fix();
		}
		else
		{
			LoggerClass.LogError("_materialsMap is null", this);
		}
		SeasonsMaterialsGroup seasonsMaterialsGroup = Interlocked.Exchange(ref seasonsMaterialsGroup_0, null);
		if (seasonsMaterialsGroup != null)
		{
			seasonsMaterialsGroup.Fix();
		}
		else
		{
			LoggerClass.LogError("group is null");
		}
		LoggerClass.LogTrace("FixMaterials() complete");
	}

	public void Unload()
	{
		if (_materialsMap != null)
		{
			_materialsMap.Unload();
		}
		if (seasonsMaterialsGroup_0 != null)
		{
			seasonsMaterialsGroup_0.Unload();
		}
	}
}
