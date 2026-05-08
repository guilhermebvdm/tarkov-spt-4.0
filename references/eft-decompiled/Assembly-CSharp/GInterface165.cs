using System.Collections.Generic;
using UnityEngine;

public interface GInterface165
{
	float StartSpeed { get; }

	float Weight { get; }

	float Drag { get; }

	ICollection<MeshRenderer> MeshRenderers { get; }

	Collider SelfCollider { get; }

	Rigidbody Rigidbody { get; }

	Material ExplosionParticleMaterial { get; }
}
