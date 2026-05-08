using UnityEngine;
using uLipSync;

namespace Cutscene;

[CreateAssetMenu(menuName = "uLipSync/Backed data with random variants")]
public class LipSyncBackedDataRandomVariants : ScriptableObject
{
	[SerializeField]
	private BakedData[] bakedData;

	public BakedData[] BakedData => bakedData;
}
