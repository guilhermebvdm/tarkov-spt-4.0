using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EFT.Hideout;

public class HideoutProductionEnvironmentReaction : SerializedMonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class1906
	{
		public static readonly Class1906 class1906_0 = new Class1906();

		public static Func<GClass2431, bool> func_0;

		public bool method_0(GClass2431 p)
		{
			return p.ProducingItems.Count > 0;
		}
	}

	[SerializeField]
	private EAreaType _areaType;

	[SerializeField]
	private HideoutArea _hideoutArea;

	[SerializeField]
	private AreaLevelAudio _audio;

	[SerializeField]
	private GInterface467[] _lights;

	[SerializeField]
	private GameObject[] _objects;

	[SerializeField]
	private AudioClip[] _audioClipsOnEnable;

	[SerializeField]
	private AudioClip[] _audioClipsOnDisable;

	[SerializeField]
	private Animator _animator;

	private AreaData areaData_0;

	private List<GClass2431> list_0;

	private bool bool_0;

	public void OnEnable()
	{
		if (areaData_0 == null)
		{
			areaData_0 = Singleton<HideoutClass>.Instance.AreaDatas.First((AreaData d) => d.Template.Type == _areaType);
		}
		if (list_0 == null)
		{
			list_0 = Singleton<HideoutClass>.Instance.ProductionController.GetAreaProducers(areaData_0);
		}
		foreach (GClass2431 item in list_0)
		{
			item.OnProduceStatusChanged += method_1;
		}
		_hideoutArea.LightLevelChanged += method_0;
		method_1();
	}

	public void OnDisable()
	{
		_hideoutArea.LightLevelChanged -= method_0;
		foreach (GClass2431 item in list_0)
		{
			item.OnProduceStatusChanged -= method_1;
		}
	}

	public void method_0()
	{
		method_5(bool_0);
	}

	public void method_1()
	{
		bool_0 = list_0.Any((GClass2431 p) => p.ProducingItems.Count > 0);
		method_2(bool_0);
	}

	public void method_2(bool enable)
	{
		method_5(enable);
		method_6(enable);
		method_3(enable);
		method_4(enable);
	}

	public void method_3(bool enable)
	{
		if ((bool)_animator)
		{
			_animator.SetBool("enable", enable);
		}
	}

	public void method_4(bool enable)
	{
		AudioClip[] array = (enable ? _audioClipsOnEnable : _audioClipsOnDisable);
		foreach (AudioClip sound in array)
		{
			_audio.PlayOneShot(sound, volumetric: false);
		}
	}

	public void method_5(bool enable)
	{
		GInterface467[] lights = _lights;
		foreach (GInterface467 gInterface in lights)
		{
			if (enable)
			{
				gInterface.TurnOn();
			}
			else
			{
				gInterface.TurnOff();
			}
		}
	}

	public void method_6(bool enable)
	{
		GameObject[] objects = _objects;
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(enable);
		}
	}

	[CompilerGenerated]
	public bool method_7(AreaData d)
	{
		return d.Template.Type == _areaType;
	}
}
