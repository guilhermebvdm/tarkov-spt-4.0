using UnityEngine;
using UnityEngine.UI;

public class HBAOControl : MonoBehaviour
{
	public HBAO hbao;

	public Slider aoRadiusSlider;

	public void ToggleShowAO()
	{
		if (hbao.generalSettings.displayMode != HBAO_Core.DisplayMode.Normal)
		{
			HBAO_Core.GeneralSettings generalSettings = hbao.generalSettings;
			generalSettings.displayMode = HBAO_Core.DisplayMode.Normal;
			hbao.generalSettings = generalSettings;
		}
		else
		{
			HBAO_Core.GeneralSettings generalSettings2 = hbao.generalSettings;
			generalSettings2.displayMode = HBAO_Core.DisplayMode.AOOnly;
			hbao.generalSettings = generalSettings2;
		}
	}

	public void UpdateAoRadius()
	{
		HBAO_Core.AOSettings aoSettings = hbao.aoSettings;
		aoSettings.radius = aoRadiusSlider.value;
		hbao.aoSettings = aoSettings;
	}
}
