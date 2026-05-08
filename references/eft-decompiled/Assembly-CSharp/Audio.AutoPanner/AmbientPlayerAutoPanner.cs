using Audio.AmbientSubsystem;
using UnityEngine;

namespace Audio.AutoPanner;

[RequireComponent(typeof(BaseAmbientSoundPlayer), typeof(AudioSource))]
public class AmbientPlayerAutoPanner : MonoBehaviour
{
	[SerializeField]
	private BaseAmbientSoundPlayer _soundPlayer;

	[SerializeField]
	private AudioSource _source;

	[Space]
	[Header("Settings")]
	[SerializeField]
	private Vector2 _lChannelPanRange = new Vector2(-1f, -0.3f);

	[SerializeField]
	private Vector2 _rChannelPanRange = new Vector2(0.3f, 1f);

	private GInterface92 ginterface92_0;

	public void Awake()
	{
		method_2();
		ginterface92_0 = new GClass1181(_source);
		_soundPlayer.Played += method_0;
	}

	public void method_0()
	{
		ginterface92_0.SetPan(method_1());
	}

	public float method_1()
	{
		return TransformHelperClass.Random((Random.value < 0.5f) ? _lChannelPanRange : _rChannelPanRange);
	}

	public void method_2()
	{
		if (_soundPlayer == null)
		{
			_soundPlayer = GClass6.GetOrAddComponent<BaseAmbientSoundPlayer>(base.gameObject);
		}
		if (_source == null)
		{
			_source = GClass6.GetOrAddComponent<AudioSource>(base.gameObject);
		}
	}

	public void OnDestroy()
	{
		if (_soundPlayer != null)
		{
			_soundPlayer.Played -= method_0;
		}
	}
}
