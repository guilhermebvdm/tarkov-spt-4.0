using MirzaBeig.Scripting.Effects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParticleForceFieldsDemo : MonoBehaviour
{
	[Header("Overview")]
	public CustomTextMeshProUGUI FPSText;

	public CustomTextMeshProUGUI particleCountText;

	public Toggle postProcessingToggle;

	public MonoBehaviour postProcessing;

	[Header("Particle System Settings")]
	public ParticleSystem particleSystem;

	private ParticleSystem.MainModule mainModule_0;

	private ParticleSystem.EmissionModule emissionModule_0;

	public CustomTextMeshProUGUI maxParticlesText;

	public CustomTextMeshProUGUI particlesPerSecondText;

	public Slider maxParticlesSlider;

	public Slider particlesPerSecondSlider;

	[Header("Attraction Particle Force Field Settings")]
	public AttractionParticleForceField attractionParticleForceField;

	public CustomTextMeshProUGUI attractionParticleForceFieldRadiusText;

	public CustomTextMeshProUGUI attractionParticleForceFieldMaxForceText;

	public CustomTextMeshProUGUI attractionParticleForceFieldArrivalRadiusText;

	public CustomTextMeshProUGUI attractionParticleForceFieldArrivedRadiusText;

	public CustomTextMeshProUGUI attractionParticleForceFieldPositionTextX;

	public CustomTextMeshProUGUI attractionParticleForceFieldPositionTextY;

	public CustomTextMeshProUGUI attractionParticleForceFieldPositionTextZ;

	public Slider attractionParticleForceFieldRadiusSlider;

	public Slider attractionParticleForceFieldMaxForceSlider;

	public Slider attractionParticleForceFieldArrivalRadiusSlider;

	public Slider attractionParticleForceFieldArrivedRadiusSlider;

	public Slider attractionParticleForceFieldPositionSliderX;

	public Slider attractionParticleForceFieldPositionSliderY;

	public Slider attractionParticleForceFieldPositionSliderZ;

	[Header("Vortex Particle Force Field Settings")]
	public VortexParticleForceField vortexParticleForceField;

	public CustomTextMeshProUGUI vortexParticleForceFieldRadiusText;

	public CustomTextMeshProUGUI vortexParticleForceFieldMaxForceText;

	public CustomTextMeshProUGUI vortexParticleForceFieldRotationTextX;

	public CustomTextMeshProUGUI vortexParticleForceFieldRotationTextY;

	public CustomTextMeshProUGUI vortexParticleForceFieldRotationTextZ;

	public CustomTextMeshProUGUI vortexParticleForceFieldPositionTextX;

	public CustomTextMeshProUGUI vortexParticleForceFieldPositionTextY;

	public CustomTextMeshProUGUI vortexParticleForceFieldPositionTextZ;

	public Slider vortexParticleForceFieldRadiusSlider;

	public Slider vortexParticleForceFieldMaxForceSlider;

	public Slider vortexParticleForceFieldRotationSliderX;

	public Slider vortexParticleForceFieldRotationSliderY;

	public Slider vortexParticleForceFieldRotationSliderZ;

	public Slider vortexParticleForceFieldPositionSliderX;

	public Slider vortexParticleForceFieldPositionSliderY;

	public Slider vortexParticleForceFieldPositionSliderZ;

	public void Start()
	{
		if ((bool)postProcessing)
		{
			postProcessingToggle.isOn = postProcessing.enabled;
		}
		mainModule_0 = particleSystem.main;
		emissionModule_0 = particleSystem.emission;
		maxParticlesSlider.value = mainModule_0.maxParticles;
		particlesPerSecondSlider.value = emissionModule_0.rateOverTime.constant;
		maxParticlesText.text = "Max Particles: " + maxParticlesSlider.value;
		particlesPerSecondText.text = "Particles Per Second: " + particlesPerSecondSlider.value;
		attractionParticleForceFieldRadiusSlider.value = attractionParticleForceField.radius;
		attractionParticleForceFieldMaxForceSlider.value = attractionParticleForceField.force;
		attractionParticleForceFieldArrivalRadiusSlider.value = attractionParticleForceField.arrivalRadius;
		attractionParticleForceFieldArrivedRadiusSlider.value = attractionParticleForceField.arrivedRadius;
		Vector3 position = attractionParticleForceField.transform.position;
		attractionParticleForceFieldPositionSliderX.value = position.x;
		attractionParticleForceFieldPositionSliderY.value = position.y;
		attractionParticleForceFieldPositionSliderZ.value = position.z;
		attractionParticleForceFieldRadiusText.text = "Radius: " + attractionParticleForceFieldRadiusSlider.value;
		attractionParticleForceFieldMaxForceText.text = "Max Force: " + attractionParticleForceFieldMaxForceSlider.value;
		attractionParticleForceFieldArrivalRadiusText.text = "Arrival Radius: " + attractionParticleForceFieldArrivalRadiusSlider.value;
		attractionParticleForceFieldArrivedRadiusText.text = "Arrived Radius: " + attractionParticleForceFieldArrivedRadiusSlider.value;
		attractionParticleForceFieldPositionTextX.text = "Position X: " + attractionParticleForceFieldPositionSliderX.value;
		attractionParticleForceFieldPositionTextY.text = "Position Y: " + attractionParticleForceFieldPositionSliderY.value;
		attractionParticleForceFieldPositionTextZ.text = "Position Z: " + attractionParticleForceFieldPositionSliderZ.value;
		vortexParticleForceFieldRadiusSlider.value = vortexParticleForceField.radius;
		vortexParticleForceFieldMaxForceSlider.value = vortexParticleForceField.force;
		Vector3 eulerAngles = vortexParticleForceField.transform.eulerAngles;
		vortexParticleForceFieldRotationSliderX.value = eulerAngles.x;
		vortexParticleForceFieldRotationSliderY.value = eulerAngles.y;
		vortexParticleForceFieldRotationSliderZ.value = eulerAngles.z;
		Vector3 position2 = vortexParticleForceField.transform.position;
		vortexParticleForceFieldPositionSliderX.value = position2.x;
		vortexParticleForceFieldPositionSliderY.value = position2.y;
		vortexParticleForceFieldPositionSliderZ.value = position2.z;
		vortexParticleForceFieldRadiusText.text = "Radius: " + vortexParticleForceFieldRadiusSlider.value;
		vortexParticleForceFieldMaxForceText.text = "Max Force: " + vortexParticleForceFieldMaxForceSlider.value;
		vortexParticleForceFieldRotationTextX.text = "Rotation X: " + vortexParticleForceFieldRotationSliderX.value;
		vortexParticleForceFieldRotationTextY.text = "Rotation Y: " + vortexParticleForceFieldRotationSliderY.value;
		vortexParticleForceFieldRotationTextZ.text = "Rotation Z: " + vortexParticleForceFieldRotationSliderZ.value;
		vortexParticleForceFieldPositionTextX.text = "Position X: " + vortexParticleForceFieldPositionSliderX.value;
		vortexParticleForceFieldPositionTextY.text = "Position Y: " + vortexParticleForceFieldPositionSliderY.value;
		vortexParticleForceFieldPositionTextZ.text = "Position Z: " + vortexParticleForceFieldPositionSliderZ.value;
	}

	public void Update()
	{
		FPSText.text = "FPS: " + 1f / Time.deltaTime;
		particleCountText.text = "Particle Count: " + particleSystem.particleCount;
	}

	public void ReloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void SetMaxParticles(float value)
	{
		mainModule_0.maxParticles = (int)value;
		maxParticlesText.text = "Max Particles: " + value;
	}

	public void SetParticleEmissionPerSecond(float value)
	{
		emissionModule_0.rateOverTime = value;
		particlesPerSecondText.text = "Particles Per Second: " + value;
	}

	public void SetAttractionParticleForceFieldRadius(float value)
	{
		attractionParticleForceField.radius = value;
		attractionParticleForceFieldRadiusText.text = "Radius: " + value;
	}

	public void SetAttractionParticleForceFieldMaxForce(float value)
	{
		attractionParticleForceField.force = value;
		attractionParticleForceFieldMaxForceText.text = "Max Force: " + value;
	}

	public void SetAttractionParticleForceFieldArrivalRadius(float value)
	{
		attractionParticleForceField.arrivalRadius = value;
		attractionParticleForceFieldArrivalRadiusText.text = "Arrival Radius: " + value;
	}

	public void SetAttractionParticleForceFieldArrivedRadius(float value)
	{
		attractionParticleForceField.arrivedRadius = value;
		attractionParticleForceFieldArrivedRadiusText.text = "Arrived Radius: " + value;
	}

	public void SetAttractionParticleForceFieldPositionX(float value)
	{
		Vector3 position = attractionParticleForceField.transform.position;
		position.x = value;
		attractionParticleForceField.transform.position = position;
		attractionParticleForceFieldPositionTextX.text = "Position X: " + value;
	}

	public void SetAttractionParticleForceFieldPositionY(float value)
	{
		Vector3 position = attractionParticleForceField.transform.position;
		position.y = value;
		attractionParticleForceField.transform.position = position;
		attractionParticleForceFieldPositionTextY.text = "Position Y: " + value;
	}

	public void SetAttractionParticleForceFieldPositionZ(float value)
	{
		Vector3 position = attractionParticleForceField.transform.position;
		position.z = value;
		attractionParticleForceField.transform.position = position;
		attractionParticleForceFieldPositionTextZ.text = "Position Z: " + value;
	}

	public void SetVortexParticleForceFieldRadius(float value)
	{
		vortexParticleForceField.radius = value;
		vortexParticleForceFieldRadiusText.text = "Radius: " + value;
	}

	public void SetVortexParticleForceFieldMaxForce(float value)
	{
		vortexParticleForceField.force = value;
		vortexParticleForceFieldMaxForceText.text = "Max Force: " + value;
	}

	public void SetVortexParticleForceFieldRotationX(float value)
	{
		Vector3 eulerAngles = vortexParticleForceField.transform.eulerAngles;
		eulerAngles.x = value;
		vortexParticleForceField.transform.eulerAngles = eulerAngles;
		vortexParticleForceFieldRotationTextX.text = "Rotation X: " + value;
	}

	public void SetVortexParticleForceFieldRotationY(float value)
	{
		Vector3 eulerAngles = vortexParticleForceField.transform.eulerAngles;
		eulerAngles.y = value;
		vortexParticleForceField.transform.eulerAngles = eulerAngles;
		vortexParticleForceFieldRotationTextY.text = "Rotation Y: " + value;
	}

	public void SetVortexParticleForceFieldRotationZ(float value)
	{
		Vector3 eulerAngles = vortexParticleForceField.transform.eulerAngles;
		eulerAngles.z = value;
		vortexParticleForceField.transform.eulerAngles = eulerAngles;
		vortexParticleForceFieldRotationTextZ.text = "Rotation Z: " + value;
	}

	public void SetVortexParticleForceFieldPositionX(float value)
	{
		Vector3 position = vortexParticleForceField.transform.position;
		position.x = value;
		vortexParticleForceField.transform.position = position;
		vortexParticleForceFieldPositionTextX.text = "Position X: " + value;
	}

	public void SetVortexParticleForceFieldPositionY(float value)
	{
		Vector3 position = vortexParticleForceField.transform.position;
		position.y = value;
		vortexParticleForceField.transform.position = position;
		vortexParticleForceFieldPositionTextY.text = "Position Y: " + value;
	}

	public void SetVortexParticleForceFieldPositionZ(float value)
	{
		Vector3 position = vortexParticleForceField.transform.position;
		position.z = value;
		vortexParticleForceField.transform.position = position;
		vortexParticleForceFieldPositionTextZ.text = "Position Z: " + value;
	}
}
