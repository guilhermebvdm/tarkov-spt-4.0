using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CustomPluginExampleBrain : MonoBehaviour
{
	public Text txtCustomRange;

	private GStruct56 gstruct56_0 = new GStruct56(0f, 10f);

	private static GClass873 gclass873_0 = new GClass873();

	public void Start()
	{
		DOTween.To(gclass873_0, () => gstruct56_0, delegate(GStruct56 x)
		{
			gstruct56_0 = x;
		}, new GStruct56(20f, 100f), 4f);
	}

	public void Update()
	{
		txtCustomRange.text = gstruct56_0.min + "\n" + gstruct56_0.max;
	}

	[CompilerGenerated]
	public GStruct56 method_0()
	{
		return gstruct56_0;
	}

	[CompilerGenerated]
	public void method_1(GStruct56 x)
	{
		gstruct56_0 = x;
	}
}
