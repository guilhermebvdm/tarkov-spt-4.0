using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class AdaptiveGridGenerationParams : MonoBehaviour
{
	[SerializeField]
	private bool _disableHierarchy;

	[SerializeField]
	private float _verticalOffset;

	public bool DisableHierarchy => _disableHierarchy;

	public float VerticalOffset => _verticalOffset;

	public void Apply()
	{
		if (_disableHierarchy)
		{
			base.gameObject.SetActive(value: false);
		}
		base.transform.position += Vector3.up * _verticalOffset;
	}

	public void Revert()
	{
		if (_disableHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
		base.transform.position -= Vector3.up * _verticalOffset;
	}
}
