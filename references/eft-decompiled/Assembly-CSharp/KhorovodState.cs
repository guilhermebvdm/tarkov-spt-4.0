using System;

[Serializable]
public class KhorovodState
{
	public float RotationVelocity = 16f;

	public float BotsVelocity = 1f;

	public float Radius = 5f;

	public EInteraction Gesture;

	public EPhraseTrigger Phrase;

	public int speakersLimit = 999;

	public bool HaveGesture;

	public bool RandomGesture;

	public bool HavePhrase;

	public bool RandomPhrase;

	public bool ForcedAutomaticFireMode;

	public bool Shooting;

	public float LookHeight = 5f;

	public bool skip;

	public float StateDuration = 5f;
}
