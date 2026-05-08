using System;
using Audio.SpatialSystem.Data;

namespace Audio.BackendSettings;

[Serializable]
public class LocationOcclusionBackendSettings
{
	public CommonOcclusionSettings commonSettings = new CommonOcclusionSettings();

	public PropagationSettings propagationSettings = new PropagationSettings();

	public ReflectionSettings reflectionSettings = new ReflectionSettings();

	public DiffractionSettings diffractionSettings = new DiffractionSettings();

	public TransmissionSettings transmissionSettings = new TransmissionSettings();
}
