using System;
using EFT;
using UnityEngine;

namespace CommonAssets.Scripts.ArtilleryShelling.Client.Audio;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/ArtilleryShellingSounds", fileName = "ArtilleryShellingSounds")]
public class ArtilleryShellingSoundsSO : ScriptableObject
{
	[Header("Shell Whistle")]
	public AudioMultipleClipContainer shellWhistleLoop;

	public Vector2 whistleVolumeRange = new Vector2(0.25f, 0.4f);

	public Vector2 whistlePitchRange = new Vector2(0.8f, 0.85f);

	[Range(0f, 1f)]
	public float shellWhistleDopplerLevel = 0.7f;

	public AnimationCurve whistleRolloffCurve = new AnimationCurve();

	[Header("MortarShot")]
	[Space]
	public AudioMultipleClipContainer mortarShots;

	public Vector2 mortarShotPitchRange = new Vector2(0.85f, 1.15f);
}
