using UnityEngine;

namespace EFT;

public class SubtitlesEventFire : MonoBehaviour
{
	public const string SUBTITLES_PREFIX = "Subtitles";

	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private ESubtitlesSource _source;

	private bool bool_0;

	public void Awake()
	{
		if (_audioSource == null)
		{
			base.enabled = false;
		}
	}
}
