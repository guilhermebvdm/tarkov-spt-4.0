using UnityEngine;

public class BFX_DecalSettings : MonoBehaviour
{
	public BFX_BloodSettings BloodSettings;

	public Transform parent;

	public float TimeHeightMax = 3.1f;

	public float TimeHeightMin = -0.1f;

	[Space]
	public Vector3 TimeScaleMax = Vector3.one;

	public Vector3 TimeScaleMin = Vector3.one;

	[Space]
	public Vector3 TimeOffsetMax = Vector3.zero;

	public Vector3 TimeOffsetMin = Vector3.zero;

	[Space]
	public AnimationCurve TimeByHeight = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Vector3 startOffset;

	private Vector3 startScale;

	private float timeDelay;

	private Transform t;

	private Transform tParent;

	private BFX_ShaderProperies shaderProperies;

	private Vector3 averageRay;

	private bool isPositionInitialized;

	private Vector3 initializedPosition;

	private void Awake()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		startOffset = ((Component)this).transform.localPosition;
		startScale = ((Component)this).transform.localScale;
		t = ((Component)this).transform;
		tParent = ((Component)parent).transform;
		shaderProperies = ((Component)this).GetComponent<BFX_ShaderProperies>();
		shaderProperies.OnAnimationFinished += ShaderCurve_OnAnimationFinished;
	}

	private void ShaderCurve_OnAnimationFinished()
	{
		((Component)this).GetComponent<Renderer>().enabled = false;
	}

	private void Update()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!isPositionInitialized)
		{
			InitializePosition();
		}
		if (((Behaviour)shaderProperies).enabled && initializedPosition.x < float.PositiveInfinity)
		{
			((Component)this).transform.position = initializedPosition;
		}
	}

	private void InitializePosition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).GetComponent<Renderer>().enabled = false;
		float y = parent.position.y;
		float groundHeight = BloodSettings.GroundHeight;
		float y2 = parent.localScale.y;
		float num = TimeHeightMax * y2;
		float num2 = TimeHeightMin * y2;
		if (y - groundHeight >= num || y - groundHeight <= num2)
		{
			((Renderer)((Component)this).GetComponent<MeshRenderer>()).enabled = false;
		}
		else
		{
			((Renderer)((Component)this).GetComponent<MeshRenderer>()).enabled = true;
		}
		float num3 = (tParent.position.y - groundHeight) / num;
		num3 = Mathf.Abs(num3);
		Vector3 val = Vector3.Lerp(TimeScaleMin, TimeScaleMax, num3);
		t.localScale = new Vector3(val.x * startScale.x, startScale.y, val.z * startScale.z);
		Vector3 val2 = Vector3.Lerp(TimeOffsetMin, TimeOffsetMax, num3);
		t.localPosition = startOffset + val2;
		t.position = new Vector3(t.position.x, groundHeight + 0.05f, t.position.z);
		timeDelay = TimeByHeight.Evaluate(num3);
		((Behaviour)shaderProperies).enabled = false;
		((MonoBehaviour)this).Invoke("EnableDecalAnimation", Mathf.Max(0f, timeDelay / BloodSettings.AnimationSpeed));
		if (BloodSettings.DecalRenderinMode == BFX_BloodSettings._DecalRenderinMode.AverageRayBetwenForwardAndFloor)
		{
			averageRay = GetAverageRay(tParent.position + tParent.right * 0.05f, tParent.right);
			float num4 = Vector3.Angle(Vector3.up, averageRay);
			float num5 = Mathf.Clamp(num4, -90f, 90f);
			Quaternion localRotation = t.localRotation;
			Vector3 eulerAngles = ((Quaternion)(ref localRotation)).eulerAngles;
			t.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, (0f - num5) * 0.5f);
			float num6 = Mathf.Abs(num5) / 90f;
			Vector3 localScale = t.localScale;
			localScale.y = Mathf.Lerp(localScale.y, localScale.x * 1.5f, num6);
			t.localScale = localScale;
		}
		if (BloodSettings.ClampDecalSideSurface)
		{
			Shader.EnableKeyword("CLAMP_SIDE_SURFACE");
		}
		isPositionInitialized = true;
	}

	private void OnDisable()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (BloodSettings.ClampDecalSideSurface)
		{
			Shader.DisableKeyword("CLAMP_SIDE_SURFACE");
		}
		isPositionInitialized = false;
		initializedPosition = Vector3.positiveInfinity;
	}

	private Vector3 GetAverageRay(Vector3 start, Vector3 forward)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(start, -forward, ref val))
		{
			Vector3 val2 = ((RaycastHit)(ref val)).normal + Vector3.up;
			return ((Vector3)(ref val2)).normalized;
		}
		return Vector3.up;
	}

	private void EnableDecalAnimation()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)shaderProperies).enabled = true;
		initializedPosition = ((Component)this).transform.position;
	}

	private void OnDrawGizmos()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)t == (Object)null)
		{
			t = ((Component)this).transform;
		}
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.03f);
		Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.85f);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
}
