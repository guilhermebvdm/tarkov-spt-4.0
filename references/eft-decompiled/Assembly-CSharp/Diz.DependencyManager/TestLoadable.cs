using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Diz.DependencyManager;

public class TestLoadable : MonoBehaviour, GInterface161
{
	[Serializable]
	[CompilerGenerated]
	public class Class1057
	{
		public static readonly Class1057 class1057_0 = new Class1057();

		public static Func<TestLoadable, string> func_0;

		public string method_0(TestLoadable x)
		{
			return x.Key;
		}
	}

	[CompilerGenerated]
	public class Class1058
	{
		public TestLoadable testLoadable_0;

		public Button tokenButton;

		public void method_0()
		{
			testLoadable_0.dictionary_0[tokenButton].Release();
			UnityEngine.Object.Destroy(tokenButton.gameObject);
		}
	}

	[SerializeField]
	private Button _loadButton;

	[SerializeField]
	private Image _mainImage;

	[SerializeField]
	private Text _refCountLabel;

	[SerializeField]
	private Transform _tokensContainer;

	[SerializeField]
	private Button _tokenTemplate;

	private global::BindableStateClass<ELoadState> gclass1643_0 = new global::BindableStateClass<ELoadState>(ELoadState.Unloaded);

	public TestLoadable[] Dependencies;

	public Action<TestLoadable> OnClicked;

	public Action<TestLoadable> OnRightClicked;

	private readonly Dictionary<Button, global::DependencyGraphClass<TestLoadable>.GClass1661> dictionary_0 = new Dictionary<Button, global::DependencyGraphClass<TestLoadable>.GClass1661>();

	public global::BindableStateClass<ELoadState> LoadState => gclass1643_0;

	public string Key => base.name;

	public IEnumerable<string> DependencyKeys => Dependencies.Select((TestLoadable x) => x.Key);

	public float Progress => 0f;

	public void Awake()
	{
		method_0();
		_loadButton.onClick.AddListener(delegate
		{
			OnClicked(this);
		});
	}

	public void method_0()
	{
		switch (gclass1643_0.Value)
		{
		case ELoadState.Loaded:
			_mainImage.color = Color.green;
			break;
		case ELoadState.Loading:
			_mainImage.color = Color.yellow;
			break;
		case ELoadState.Unloaded:
			_mainImage.color = Color.grey;
			break;
		case ELoadState.Failed:
			_mainImage.color = Color.red;
			break;
		case ELoadState.Unloading:
			break;
		}
	}

	public void AddToken(global::DependencyGraphClass<TestLoadable>.GClass1661 token)
	{
		Button tokenButton = UnityEngine.Object.Instantiate(_tokenTemplate);
		dictionary_0[tokenButton] = token;
		tokenButton.gameObject.SetActive(value: true);
		tokenButton.transform.SetParent(_tokensContainer, worldPositionStays: false);
		tokenButton.onClick.AddListener(delegate
		{
			dictionary_0[tokenButton].Release();
			UnityEngine.Object.Destroy(tokenButton.gameObject);
		});
	}

	public void Load()
	{
		StartCoroutine(Loading());
	}

	public IEnumerator Load(IProgress<float> progress, CancellationToken ct)
	{
		return Loading();
	}

	public IEnumerator Loading()
	{
		Debug.Log("Start loading " + base.name);
		gclass1643_0.Value = ELoadState.Loading;
		method_0();
		yield return new WaitForSeconds(10f);
		gclass1643_0.Value = ELoadState.Loaded;
		method_0();
	}

	public void Unload()
	{
		gclass1643_0.Value = ELoadState.Unloaded;
		method_0();
	}

	public void SetRefCount(int rc)
	{
		_refCountLabel.text = rc.ToString();
	}

	[CompilerGenerated]
	public void method_1()
	{
		OnClicked(this);
	}
}
