using UnityEngine;

public class PlayOnFall : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _target;

	public void method_0()
	{
		if (!(_target == null))
		{
			_target.Play();
		}
	}
}
