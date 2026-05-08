using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;

[GAttribute8(21000)]
public abstract class DisablerCullingObjectBase : UpdateInEditorSystemComponent<DisablerCullingObjectBase>, IBotController
{
	[SerializeField]
	protected List<Collider> _colliders;

	[SerializeField]
	protected List<Collider> _inverseColliders;

	protected List<ColliderReporter> _enteredColliders = new List<ColliderReporter>(4);

	protected List<ColliderReporter> _enteredInverseColliders = new List<ColliderReporter>(4);

	protected List<ColliderReporter> _colliderReporters = new List<ColliderReporter>(4);

	protected List<ColliderReporter> _inverseColliderReporters = new List<ColliderReporter>(4);

	protected bool _updateComponentsStatus;

	public virtual bool HasEntered
	{
		get
		{
			if (_enteredColliders.Count > 0)
			{
				return _enteredInverseColliders.Count == 0;
			}
			return false;
		}
	}

	public virtual void OnPreProcess()
	{
		PrepareCullingObject();
	}

	public void CollidersNullCheck()
	{
		CollidersNullCheck(ref _colliders);
		CollidersNullCheck(ref _inverseColliders);
	}

	public void CollidersNullCheck(ref List<Collider> colliders)
	{
		if (colliders != null)
		{
			for (int num = colliders.Count - 1; num >= 0; num--)
			{
				if (colliders[num] == null)
				{
					colliders.RemoveAt(num);
				}
			}
		}
		else
		{
			colliders = new List<Collider>();
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public virtual void Awake()
	{
		Validate();
		_updateComponentsStatus = true;
	}

	public virtual void PrepareCullingObject()
	{
		CollidersNullCheck();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		foreach (ColliderReporter colliderReporter in _colliderReporters)
		{
			colliderReporter.OnTriggerEnterEvent -= OnTriggerEnterEvent;
			colliderReporter.OnTriggerExitEvent -= OnTriggerExitEvent;
			colliderReporter.RemoveOwner(this);
		}
		_colliderReporters.Clear();
	}

	public override void ManualUpdate(float dt)
	{
		if (_updateComponentsStatus)
		{
			SetComponentsEnabled(HasEntered);
			_updateComponentsStatus = false;
		}
	}

	public abstract void SetComponentsEnabled(bool hasEntered);

	public void OnValidate()
	{
		Validate();
	}

	public virtual void Validate()
	{
		method_0(_colliders, _colliderReporters, inverse: false);
		method_0(_inverseColliders, _inverseColliderReporters, inverse: true);
	}

	public void method_0(List<Collider> colliders, List<ColliderReporter> colliderReporters, bool inverse)
	{
		if (colliders == null)
		{
			return;
		}
		colliderReporters.Clear();
		for (int i = 0; i < colliders.Count; i++)
		{
			Collider collider = colliders[i];
			if (!(collider == null))
			{
				ColliderReporter orAddComponent = GClass6.GetOrAddComponent<ColliderReporter>(collider.gameObject);
				orAddComponent.OnTriggerEnterEvent -= OnTriggerEnterEvent;
				orAddComponent.OnTriggerExitEvent -= OnTriggerExitEvent;
				orAddComponent.OnTriggerEnterEvent += OnTriggerEnterEvent;
				orAddComponent.OnTriggerExitEvent += OnTriggerExitEvent;
				orAddComponent.AddOwner(this);
				orAddComponent.IsInverse = inverse;
				if (colliderReporters.IndexOf(orAddComponent) == -1)
				{
					colliderReporters.Add(orAddComponent);
				}
				if (collider.gameObject.layer != LayerMaskClass.DisablerCullingObjectLayer)
				{
					collider.gameObject.layer = LayerMaskClass.DisablerCullingObjectLayer;
				}
				if (!collider.isTrigger)
				{
					collider.isTrigger = true;
				}
			}
		}
	}

	public bool IsRightCollider(Collider col)
	{
		Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(col);
		if (playerByCollider != null)
		{
			return playerByCollider.IsYourPlayer;
		}
		return false;
	}

	public virtual void OnTriggerEnterEvent(ColliderReporter colliderReporter, Collider collider)
	{
		if (colliderReporter.IsInverse)
		{
			method_1(_enteredInverseColliders, colliderReporter, collider);
		}
		else
		{
			method_1(_enteredColliders, colliderReporter, collider);
		}
	}

	public void method_1(List<ColliderReporter> enteredColliders, ColliderReporter colliderReporter, Collider collider)
	{
		if (!enteredColliders.Contains(colliderReporter) && IsRightCollider(collider))
		{
			bool hasEntered = HasEntered;
			enteredColliders.Add(colliderReporter);
			if (hasEntered != HasEntered)
			{
				UpdateComponentsStatusOnUpdate();
			}
		}
	}

	public virtual void OnTriggerExitEvent(ColliderReporter colliderReporter, Collider collider)
	{
		if (colliderReporter.IsInverse)
		{
			method_2(_enteredInverseColliders, colliderReporter, collider);
		}
		else
		{
			method_2(_enteredColliders, colliderReporter, collider);
		}
	}

	public void method_2(List<ColliderReporter> enteredColliders, ColliderReporter colliderReporter, Collider collider)
	{
		if (enteredColliders.Contains(colliderReporter) && IsRightCollider(collider))
		{
			bool hasEntered = HasEntered;
			enteredColliders.Remove(colliderReporter);
			if (hasEntered != HasEntered)
			{
				UpdateComponentsStatusOnUpdate();
			}
		}
	}

	public virtual void UpdateComponentsStatusOnUpdate()
	{
		_updateComponentsStatus = true;
	}

	public DisablerCullingObjectBase()
	{
	}
}
