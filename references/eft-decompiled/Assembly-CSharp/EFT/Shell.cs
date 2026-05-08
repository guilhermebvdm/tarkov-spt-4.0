using System.Runtime.CompilerServices;
using EFT.Ballistics;
using UnityEngine;

namespace EFT;

public class Shell : BouncingObject
{
	private const float float_5 = 100f;

	[SerializeField]
	private ECaliber _caliber;

	private Vector3 vector3_2;

	[CompilerGenerated]
	private GInterface210 ginterface210_0;

	public GInterface210 CollisionListener
	{
		[CompilerGenerated]
		get
		{
			return ginterface210_0;
		}
		[CompilerGenerated]
		set
		{
			ginterface210_0 = value;
		}
	}

	public void Awake()
	{
		base.enabled = false;
	}

	public void ActivatePhysics(Vector3 beginPoint, Vector3 velocity, Vector3 rotationVector, Vector3 weaponForward)
	{
		vector3_2 = rotationVector;
		EFTHardSettings.ShellsSettings shells = EFTHardSettings.Instance.Shells;
		if (shells.velocityRotation > 0f)
		{
			velocity = Quaternion.AngleAxis(shells.velocityRotation, weaponForward) * velocity;
		}
		Init(beginPoint, velocity * EFTHardSettings.Instance.Shells.velocityMult, EFTHardSettings.Instance.Shells.radius, EFTHardSettings.Instance.Shells.playMult, LayerMaskClass.ShellsCollisionsMask, EFTHardSettings.Instance.Shells.maxCastCount, EFTHardSettings.Instance.Shells.deltaTimeStep, EFTHardSettings.Instance.Shells.randomReboundSpread, EFTHardSettings.Instance.Shells.bounceSpeedMult, EFTHardSettings.Instance.Shells.showDebug);
		base.enabled = true;
	}

	public void DisablePhysics()
	{
		base.enabled = false;
	}

	public void SetCaliber(ECaliber caliber)
	{
		_caliber = caliber;
	}

	public override void OnBounce(Collider collider)
	{
		base.OnBounce(collider);
		vector3_2 = base.VelocitySqrMagnitude * new Vector3(TransformHelperClass.Random(EFTHardSettings.Instance.Shells.ReboundRotationX, randomizeSign: true), TransformHelperClass.Random(EFTHardSettings.Instance.Shells.ReboundRotationY, randomizeSign: true), 0f);
		if (CollisionListener != null)
		{
			BallisticCollider component = collider.gameObject.GetComponent<BallisticCollider>();
			if (component != null)
			{
				CollisionListener.InvokeShellCollision(base.transform.position, component.GetSurfaceSound(base.transform.position), _caliber);
			}
			else
			{
				CollisionListener.InvokeShellCollision(base.transform.position, BaseBallistic.ESurfaceSound.Soil, _caliber);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		base.transform.localRotation *= Quaternion.Euler(vector3_2 * (Time.deltaTime * 100f));
	}
}
