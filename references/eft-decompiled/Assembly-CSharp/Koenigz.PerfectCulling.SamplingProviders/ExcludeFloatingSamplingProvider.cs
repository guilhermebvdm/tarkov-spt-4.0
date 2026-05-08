using UnityEngine;

namespace Koenigz.PerfectCulling.SamplingProviders;

[RequireComponent(typeof(PerfectCullingBakingBehaviour))]
[DisallowMultipleComponent]
[ExecuteAlways]
public class ExcludeFloatingSamplingProvider : SamplingProviderBase
{
	[Tooltip("Which layers will be used as colliders")]
	[SerializeField]
	private LayerMask layerMask = -1;

	[Tooltip("Box XZ half size\nShould equal to max jump distance in horizontal plane from infinite height")]
	[SerializeField]
	private float areaSize = 6f;

	[Tooltip("Player height")]
	[SerializeField]
	private float playerHeight = 3f;

	[SerializeField]
	private bool terrainVerticalCheck;

	[SerializeField]
	private float nearSurfaceCheckRadius;

	[HideInInspector]
	[SerializeField]
	private Vector3 boxSize;

	[SerializeField]
	private float _verticalVoidCheckDistance;

	private TerrainCollider[] terrainCollider_0;

	private const float float_0 = 400f;

	public LayerMask CollisionMask => layerMask;

	public bool UseTerrainVerticalCheck
	{
		get
		{
			return terrainVerticalCheck;
		}
		set
		{
			terrainVerticalCheck = value;
		}
	}

	public override string Name => "ExcludeFloatingSamplingProvider" + $": Mask: {layerMask.value.ToString()}, Distance: {boxSize}";

	public override void InitializeSamplingProvider()
	{
		terrainCollider_0 = Object.FindObjectsOfType<TerrainCollider>();
	}

	public void OnValidate()
	{
		boxSize = new Vector3(areaSize, playerHeight, areaSize);
	}

	public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
	{
		if (terrainVerticalCheck)
		{
			TerrainCollider[] array = terrainCollider_0;
			foreach (TerrainCollider obj in array)
			{
				Vector3 vector = pos + Vector3.up * 400f;
				if (obj.Raycast(new Ray(vector, -Vector3.up), out var hitInfo, 400f) && !(hitInfo.distance >= (pos - vector).magnitude))
				{
					return false;
				}
			}
		}
		if (nearSurfaceCheckRadius > 0f && Physics.CheckSphere(pos, nearSurfaceCheckRadius, layerMask.value, QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		if (_verticalVoidCheckDistance > 0f && Physics.Raycast(pos, -Vector3.up, out var hitInfo2, _verticalVoidCheckDistance, layerMask.value, QueryTriggerInteraction.Ignore) && hitInfo2.collider.gameObject.name == "TEMP_GROUND_COLIDER")
		{
			return false;
		}
		return Physics.CheckBox(pos, boxSize, Quaternion.identity, layerMask.value, QueryTriggerInteraction.Ignore);
	}
}
