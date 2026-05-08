using Prism.Utils;
using UnityEngine;

namespace Prism.Demo;

public class PrismLerpPresetExample : MonoBehaviour
{
	private PrismEffects prismEffects_0;

	[Header("NOTE: This is an example script, you should only have one per scene")]
	[Header("This script lerps a PRISM preset based on distance to the camera")]
	public float maxDistance = 500f;

	public float t;

	[Tooltip("The Prism-Preset to lerp TO")]
	public PrismPreset target;

	public AnimationCurve cameraDistanceCurve;

	public void Start()
	{
		prismEffects_0 = Camera.main.GetComponent<PrismEffects>();
		if (!prismEffects_0)
		{
			Debug.LogWarning("Main camera had no PRISM on it! Can't initialize demo script.");
			base.enabled = false;
		}
	}

	public void Update()
	{
		t = Vector3.Distance(base.transform.position, Camera.main.transform.position);
		t = cameraDistanceCurve.Evaluate(t);
		prismEffects_0.LerpToPreset(target, t);
	}
}
