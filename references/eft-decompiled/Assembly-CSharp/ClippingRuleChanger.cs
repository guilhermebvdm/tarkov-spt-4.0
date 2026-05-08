using UnityEngine;

public class ClippingRuleChanger : MonoBehaviour
{
	[SerializeField]
	private EClippingCustoms _clippingCustoms;

	public EClippingCustoms GetClippingCustoms => _clippingCustoms;
}
