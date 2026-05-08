using System.Runtime.CompilerServices;
using UnityEngine;

namespace Diz.DependencyManager;

public class SystemTester : MonoBehaviour
{
	[CompilerGenerated]
	public class Class1056
	{
		public TestLoadable l;

		public SystemTester systemTester_0;

		public void method_0(TestLoadable t)
		{
			l.AddToken(systemTester_0.System.Retain(t.gameObject.name));
		}
	}

	public global::DependencyGraphClass<TestLoadable> System;

	private TestLoadable[] testLoadable_0;

	public void Start()
	{
		testLoadable_0 = GClass870.FindUnityObjectsOfType<TestLoadable>();
		System = new global::DependencyGraphClass<TestLoadable>(testLoadable_0, string.Empty, null);
		TestLoadable[] array = testLoadable_0;
		foreach (TestLoadable testLoadable in array)
		{
			TestLoadable l = testLoadable;
			l.OnClicked = delegate(TestLoadable t)
			{
				l.AddToken(System.Retain(t.gameObject.name));
			};
		}
	}

	public void Update()
	{
		TestLoadable[] array = testLoadable_0;
		foreach (TestLoadable testLoadable in array)
		{
			testLoadable.SetRefCount(System.Nodes[testLoadable.Key].RefCount);
		}
	}
}
