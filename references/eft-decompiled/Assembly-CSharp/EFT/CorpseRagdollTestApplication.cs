using System.Collections.Generic;
using UnityEngine;

namespace EFT;

public class CorpseRagdollTestApplication : AbstractApplication
{
	[SerializeField]
	private GameObject _corpsePrefab;

	[SerializeField]
	private GameObject _weaponPrefab;

	private List<CorpseRagdollTest> list_0 = new List<CorpseRagdollTest>();

	[SerializeField]
	private Vector3 _velocity;

	[SerializeField]
	private float _maxDepenetrationVelocity = 2f;

	[SerializeField]
	private CollisionDetectionMode _collisionDetectionMode = CollisionDetectionMode.Continuous;

	[SerializeField]
	private bool _keepRigidbody;

	[SerializeField]
	private bool _putToSleep;

	[SerializeField]
	private bool _dropWeapon = true;

	[SerializeField]
	private bool _weaponFixedHinge;

	public override EUpdateQueue PlayerUpdateQueue => EUpdateQueue.Update;

	public override LogConfiguratorAbstractClass CreateLogConfigurator()
	{
		return GClass733.Create();
	}

	public void SpawnCorpse(int count = 1)
	{
		for (int i = 0; i < count; i++)
		{
			CorpseRagdollTest corpseRagdollTest = Object.Instantiate(_corpsePrefab, base.transform.position, base.transform.rotation).AddComponent<CorpseRagdollTest>();
			corpseRagdollTest.Init();
			list_0.Add(corpseRagdollTest);
		}
	}

	public void DropAll()
	{
		foreach (CorpseRagdollTest item in list_0)
		{
			item.Drop(item.transform.TransformVector(_velocity), _maxDepenetrationVelocity, _collisionDetectionMode, _keepRigidbody, _putToSleep);
			if (_dropWeapon && _weaponPrefab != null)
			{
				GameObject gameObject = Object.Instantiate(_weaponPrefab);
				Rigidbody weaponRigidbody = gameObject.AddComponent<Rigidbody>();
				Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = true;
				}
				int layer = LayerMask.NameToLayer("Deadbody");
				TransformHelperClass.SetLayersRecursively(gameObject, layer);
				item.Ragdoll.AttachWeapon(weaponRigidbody, item.gameObject, item.gameObject.GetComponentInChildren<PlayerBones>(), gameObject.GetComponentInChildren<TransformLinks>(), _weaponFixedHinge, _velocity);
			}
		}
		list_0.Clear();
	}

	public void ResetAVGSimulationDeltaTime()
	{
		EFTPhysicsClass.SyncTransformsClass.SetAVGSimulationDeltaTime(Time.deltaTime);
	}

	public void LateUpdate()
	{
		EFTPhysicsClass.SyncTransforms();
	}
}
