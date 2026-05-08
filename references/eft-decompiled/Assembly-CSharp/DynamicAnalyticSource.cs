public class DynamicAnalyticSource : AnalyticSource
{
	public override void Start()
	{
		AmbientLight.AddDynamicAnalyticSource(this);
		UpdateSettings();
	}

	public override void OnEnable()
	{
		AmbientLight.AddDynamicAnalyticSource(this);
		UpdateSettings();
	}

	public override void OnDisable()
	{
		AmbientLight.RemoveDynamicAnalyticSource(this);
	}

	public void UpdateDynamicValues()
	{
		if (base.transform.hasChanged)
		{
			Bounds.center = base.transform.position;
			LocalToWorldMatrix = base.transform.localToWorldMatrix;
		}
	}
}
