using JetBrains.Annotations;
using UnityEngine;

public interface IEasyBundle : GInterface161
{
	[CanBeNull]
	Object[] Assets { get; }

	[CanBeNull]
	Object SameNameAsset { get; }
}
