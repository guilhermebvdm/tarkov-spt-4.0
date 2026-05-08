using Audio.BackendSettings;
using UnityEngine;

namespace Audio.SpatialSystem.Data;

[CreateAssetMenu(menuName = "Scriptable objects/Audio/OcclusionSettings/AudioOcclusionSettings", fileName = "AudioOcclusionSettings")]
public class AudioOcclusionSettings : ScriptableObject
{
	[Tooltip("Settings for all system components")]
	public CommonOcclusionSettings commonSettings = new CommonOcclusionSettings();

	[Tooltip("Settings for propagation-based occlusion calculation")]
	public PropagationSettings propagationSettings = new PropagationSettings();

	[Tooltip("Settings for reflection-based occlusion sampling")]
	public ReflectionSettings reflectionSettings = new ReflectionSettings();

	[Tooltip("Settings for diffraction-based occlusion sampling")]
	public DiffractionSettings diffractionSettings = new DiffractionSettings();

	[Tooltip("Settings for transmission-based occlusion sampling")]
	public TransmissionSettings transmissionSettings = new TransmissionSettings();

	public void Apply(LocationOcclusionBackendSettings backendSettings)
	{
		commonSettings.Apply(backendSettings.commonSettings);
		propagationSettings.Apply(backendSettings.propagationSettings);
		reflectionSettings.Apply(backendSettings.reflectionSettings);
		diffractionSettings.Apply(backendSettings.diffractionSettings);
		transmissionSettings.Apply(backendSettings.transmissionSettings);
	}
}
