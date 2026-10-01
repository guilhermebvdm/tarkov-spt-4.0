using UnityEngine;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    // stub GInterface323 the dispatch patch hands back when GClass2970.smethod_0
    // is called for our item. without it, smethod_0 returns null and downstream
    // controller-swap code that expects a non-null instance silently breaks.
    public class LoadAmmoBundleInterfaceClass : GInterface323
    {
        public void Initialize(GameObject gameObject) { }
        public void UpdateData(GStruct337 observedUsableItemUpdatedData) { }
        public void Disable() { }
    }
}
