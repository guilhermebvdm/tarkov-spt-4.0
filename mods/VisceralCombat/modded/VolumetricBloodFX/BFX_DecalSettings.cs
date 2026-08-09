using UnityEngine;
using Object = UnityEngine.Object;

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
		startOffset = ((Component)this).transform.localPosition;
		startScale = ((Component)this).transform.localScale;
		t = ((Component)this).transform;
		tParent = ((Component)parent).transform;
		shaderProperies = ((Component)this).GetComponent<BFX_ShaderProperies>();
		if (shaderProperies != null)
		{
			shaderProperies.OnAnimationFinished += ShaderCurve_OnAnimationFinished;
		}
	}

	private void ShaderCurve_OnAnimationFinished()
	{
		((Component)this).GetComponent<Renderer>().enabled = false;
	}

	private void OnDestroy()
	{
		if (shaderProperies != null)
		{
			shaderProperies.OnAnimationFinished -= ShaderCurve_OnAnimationFinished;
		}
	}

	private void Update()
	{
		if (!isPositionInitialized)
		{
			InitializePosition();
		}
		if (shaderProperies != null && ((Behaviour)shaderProperies).enabled && initializedPosition.x < float.PositiveInfinity)
		{
			((Component)this).transform.position = initializedPosition;
		}
	}

	private void InitializePosition()
	{
		((Component)this).GetComponent<Renderer>().enabled = false;
		float y = parent.position.y;
		float groundHeight = (BloodSettings != null) ? BloodSettings.GroundHeight : -9999999f;
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
		if (shaderProperies != null)
		{
			((Behaviour)shaderProperies).enabled = false;
		}
		float animSpeed = (BloodSettings != null) ? BloodSettings.AnimationSpeed : 1f;
		((MonoBehaviour)this).Invoke("EnableDecalAnimation", Mathf.Max(0f, timeDelay / animSpeed));
		if (BloodSettings != null && BloodSettings.DecalRenderinMode == BFX_BloodSettings._DecalRenderinMode.AverageRayBetwenForwardAndFloor)
		{
			averageRay = GetAverageRay(tParent.position + tParent.right * 0.05f, tParent.right);
			float num4 = Vector3.Angle(Vector3.up, averageRay);
			float num5 = Mathf.Clamp(num4, -90f, 90f);
			Vector3 eulerAngles = t.localRotation.eulerAngles;
			t.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, (0f - num5) * 0.5f);
			float num6 = Mathf.Abs(num5) / 90f;
			Vector3 localScale = t.localScale;
			localScale.y = Mathf.Lerp(localScale.y, localScale.x * 1.5f, num6);
			t.localScale = localScale;
		}
		if (BloodSettings != null && BloodSettings.ClampDecalSideSurface)
		{
			Shader.EnableKeyword("CLAMP_SIDE_SURFACE");
		}
		isPositionInitialized = true;
	}

	private void OnDisable()
	{
		if (BloodSettings != null && BloodSettings.ClampDecalSideSurface)
		{
			Shader.DisableKeyword("CLAMP_SIDE_SURFACE");
		}
		isPositionInitialized = false;
		initializedPosition = Vector3.positiveInfinity;
	}

	private Vector3 GetAverageRay(Vector3 start, Vector3 forward)
	{
		if (Physics.Raycast(start, -forward, out RaycastHit val))
		{
			Vector3 val2 = val.normal + Vector3.up;
			return val2.normalized;
		}
		return Vector3.up;
	}

	private void EnableDecalAnimation()
	{
		if (shaderProperies != null)
		{
			((Behaviour)shaderProperies).enabled = true;
		}
		initializedPosition = ((Component)this).transform.position;
	}

	private void OnDrawGizmos()
	{
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
