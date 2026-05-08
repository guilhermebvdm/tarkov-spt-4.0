using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BotInfoIconsPanel : MonoBehaviour
{
	public BotInfoIcon GrenadeObj;

	public BotInfoIcon HaveTargetCover;

	public BotInfoIcon CoverObj;

	public BotInfoIcon EnemyVisibleObj;

	public BotInfoIcon LookThoughtGrass;

	public BotInfoIcon CanShoot;

	public BotInfoIcon InBuildingTarget;

	public BotInfoIcon InBuildingMe;

	public BotInfoIcon UnderFire;

	public BotInfoIcon HealObj;

	public BotInfoIcon DamagedObj;

	public BotInfoIcon SurgicalObj;

	public BotInfoIcon StimulatorObj;

	public BotInfoIcon BotMoverCurrentStateIcon;

	public BotInfoIcon ParametersChange;

	public TextMeshProUGUI AmmoRemainField;

	public Image IconsBackImageUpper;

	public Image IconsBackImageLower;

	public BotInfoIcon UnderbarrelLauncherActivityIcon;

	public BotInfoIcon UnderbarrelLauncherAmmoIcon;

	private Color color_0 = new Color(0.5f, 0.5f, 0.5f, 0.15f);

	private RectTransform rectTransform_0;

	public RectTransform RectTransform => rectTransform_0 ?? (rectTransform_0 = GClass949.RectTransform(base.transform));

	public void SetBackgroundColor(Color color)
	{
		IconsBackImageUpper.color = color;
		IconsBackImageLower.color = color;
	}

	public void UpdateIcons(ActorDataStruct actorDataStruct, EEnemyPartVisibleType panelsPlayerVisibleType)
	{
		BotDataStruct botData = actorDataStruct.BotData;
		AmmoRemainField.text = botData.Ammo.ToString();
		AmmoRemainField.color = (botData.Shooting ? Color.red : Color.white);
		CoverObj.SetBool(botData.InCover);
		HaveTargetCover.SetBool(botData.CoverIndex > 0);
		UnderFire.SetBool(botData.UnderFire);
		ParametersChange.SetBool(botData.IsParametesChanged);
		EnemyVisibleObj.SetInt((int)panelsPlayerVisibleType);
		LookThoughtGrass.SetBool(botData.CanLookThoughtGrass);
		CanShoot.SetBool(botData.CanShoot);
		InBuildingTarget.SetBool(botData.InBuildingTarget);
		InBuildingMe.SetBool(botData.InBuildingMe);
		if (botData.HaveSurgical)
		{
			SurgicalObj.SetBool(botData.UseSurgical);
		}
		else
		{
			SurgicalObj.SetColor(color_0);
		}
		if (botData.HaveMeds)
		{
			HealObj.SetBool(botData.UseMeds);
		}
		else
		{
			HealObj.SetColor(color_0);
		}
		if (botData.Damaged)
		{
			DamagedObj.SetBool(botData.UseMeds);
		}
		else
		{
			DamagedObj.SetColor(color_0);
		}
		if (botData.HaveStimulator)
		{
			StimulatorObj.SetBool(botData.UseStimulator);
		}
		else
		{
			StimulatorObj.SetColor(color_0);
		}
		GrenadeObj.SetBool(botData.HaveGrenade);
		if (BotMoverCurrentStateIcon != null)
		{
			BotMoverCurrentStateIcon.SetBool(botData.BotMoverCurrentState == 2);
		}
		UnderbarrelLauncherActivityIcon.gameObject.SetActive(botData.HaveUnderbarrelLauncher);
		if (botData.HaveUnderbarrelLauncher)
		{
			UnderbarrelLauncherActivityIcon.SetBool(botData.UnderbarrelLauncherActive);
			UnderbarrelLauncherAmmoIcon.SetBool(botData.UnderbarrelLauncherHasAmmoInChamber);
		}
	}
}
