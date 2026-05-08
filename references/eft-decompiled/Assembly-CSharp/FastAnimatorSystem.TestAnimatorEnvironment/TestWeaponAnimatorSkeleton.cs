using UnityEngine;

namespace FastAnimatorSystem.TestAnimatorEnvironment;

public class TestWeaponAnimatorSkeleton : MonoBehaviour
{
	public TextAsset fastAnimatorJson;

	private IAnimator ianimator_0;

	public IAnimator Animator => ianimator_0;

	public void Start()
	{
		FastAnimatorControllerClass fastAnimatorController = GClass1346.Deserialize(fastAnimatorJson.bytes);
		ianimator_0 = GClass1445.CreateAnimator(fastAnimatorController);
		ianimator_0.SetBool("Active", value: true);
		ianimator_0.SetBool("Armed", value: true);
		ianimator_0.SetBool("MagInWeapon", value: true);
		ianimator_0.SetBool("Inventory", value: true);
		ianimator_0.SetFloat("AmmoInChamber", 1f);
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.U))
		{
			ianimator_0.SetTrigger("MagOutFromInv");
		}
		ianimator_0.Update(Time.deltaTime);
	}
}
