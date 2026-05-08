using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponTestController : MonoBehaviour
{
	public Animator Animator;

	public bool Active;

	public float AmmoInChamber = 1f;

	public float AmmoInMag = 30f;

	public bool MagInWeapon = true;

	public bool Inventory;

	public float Idle = 1f;

	public bool Fire;

	public int FireModes = 2;

	private float FireMode;

	private int MagTypeOld;

	public int MagTypes = 3;

	private int MagTypeNew;

	private bool inv;

	public float IdleTime;

	public float IdleTimeMax = 30f;

	public float IdleRandomValue;

	public float IdleRandomMin;

	public float IdleRandomMax = 3f;

	public List<int> AltFireVars = new List<int>();

	public int CurrentAltFire;

	protected Dictionary<string, int> LayerIntDict = new Dictionary<string, int>();

	public void Start()
	{
		for (int i = 0; i < 10; i++)
		{
			try
			{
				string layerName = Animator.GetLayerName(i);
				if (string.IsNullOrEmpty(layerName))
				{
					break;
				}
				LayerIntDict.Add(layerName, i);
			}
			catch (Exception)
			{
			}
		}
	}

	public int method_0(string name)
	{
		int value = 0;
		LayerIntDict.TryGetValue(name, out value);
		return value;
	}

	public void OnEnable()
	{
		IdleTime = 0f;
	}

	public void FiringBullet()
	{
		Debug.Log("firing a bullet");
		StartCoroutine(method_1("FiringBullet", 0.01f));
		Animator.SetFloat("ShellsInWeapon", Animator.GetFloat("ShellsInWeapon") + 1f);
	}

	public void DelAmmoChamber()
	{
		Debug.Log("deleting ammo from chamber");
		StartCoroutine(method_1("DelAmmoChamber", 0.01f));
		Animator.SetFloat("AmmoInChamber", Animator.GetFloat("AmmoInChamber") - 1f);
	}

	public void ShellEject()
	{
		Debug.Log("ejecting shell");
		StartCoroutine(method_1("ShellEject", 0.01f));
	}

	public void DelAmmoFromMag()
	{
		Debug.Log("deleting ammo from mag");
		StartCoroutine(method_1("DelAmmoFromMag", 0.01f));
		Animator.SetFloat("AmmoInMag", Animator.GetFloat("AmmoInMag") - 1f);
	}

	public void AddAmmoInChamber()
	{
		Debug.Log("adding ammo in chamber");
		StartCoroutine(method_1("AddAmmoInChamber", 0.01f));
		Animator.SetFloat("AmmoInChamber", Animator.GetFloat("AmmoInChamber") + 1f);
	}

	public void OnBoltCatch()
	{
		Debug.Log("setting on bolt catch");
		StartCoroutine(method_1("OnBoltCatch", 0.1f));
		Animator.SetBool("BoltCatch", value: true);
	}

	public void OffBoltCatch()
	{
		Debug.Log("disabling bolt catch");
		StartCoroutine(method_1("OffBoltCatch", 0.1f));
		Animator.SetBool("BoltCatch", value: false);
	}

	public void FiremodeSwitch()
	{
		Debug.Log("setting to firemode 1");
		StartCoroutine(method_1("SetFiremode1", 0.01f));
		FireMode = ((int)FireMode + 1) % FireModes;
		Animator.SetFloat("Firemode", FireMode);
	}

	public void MagOut()
	{
		Debug.Log("removing magazine");
		StartCoroutine(method_1("MagOut", 0.01f));
		Animator.SetFloat("AmmoInMag", 0f);
		Animator.SetBool("MagInWeapon", value: false);
	}

	public void MagIn()
	{
		Debug.Log("inserting magazine");
		StartCoroutine(method_1("MagIn", 0.01f));
		Animator.SetFloat("AmmoInMag", 10f);
		Animator.SetBool("MagInWeapon", value: true);
	}

	public void Arm()
	{
		Debug.Log("arming weapon");
		StartCoroutine(method_1("Arm", 0.01f));
		Animator.SetBool("Armed", value: true);
	}

	public void Disarm()
	{
		Debug.Log("disarming weapon");
		StartCoroutine(method_1("Disarm", 0.01f));
		Animator.SetBool("Armed", value: false);
	}

	public void MisfireOff()
	{
		Debug.Log("removing misfiring ammo");
		StartCoroutine(method_1("MisfireOff", 0.01f));
		Animator.SetBool("Misfire", value: false);
	}

	public void AddAmmoInMag()
	{
		Debug.Log("adding ammo to magazine");
		StartCoroutine(method_1("AddAmmoInMag", 0.01f));
		Animator.SetFloat("AmmoInMag", Animator.GetFloat("AmmoInMag") + 1f);
	}

	public void MagSwapEnd()
	{
		Debug.Log("ending mag swap");
		StartCoroutine(method_1("MagSwapEnd", 0.01f));
		Animator.SetBool("MagSwap", value: false);
	}

	public void RemoveShell()
	{
		Debug.Log("removing shell");
		Animator.SetFloat("ShellsInWeapon", Animator.GetFloat("ShellsInWeapon") - 1f);
	}

	public void NullShell()
	{
		Debug.Log("adding shell");
		Animator.SetFloat("ShellsInWeapon", 0f);
	}

	public IEnumerator method_1(string name, float time)
	{
		Animator.SetBool(name, value: true);
		yield return new WaitForSeconds(time);
		Animator.SetBool(name, value: false);
	}

	public void Update()
	{
		if (Animator.GetBool("BoltCatch"))
		{
			Animator.SetLayerWeight(2, 1f);
		}
		else
		{
			Animator.SetLayerWeight(2, 0f);
		}
		if (!Animator.GetBool("Armed"))
		{
			Animator.SetLayerWeight(method_0("Hammer"), 1f);
		}
		else
		{
			Animator.SetLayerWeight(method_0("Hammer"), 0f);
		}
		bool num = FireMode == 0f;
		bool flag = false;
		if ((!num) ? Input.GetKeyDown(KeyCode.Mouse0) : Input.GetKey(KeyCode.Mouse0))
		{
			Animator.SetBool("Fire", value: true);
		}
		else
		{
			Animator.SetBool("Fire", value: false);
		}
		if (Input.GetKey(KeyCode.Keypad7))
		{
			Animator.SetBool("MagInFromInv", value: true);
		}
		else
		{
			Animator.SetBool("MagInFromInv", value: false);
		}
		if (Input.GetKey(KeyCode.Keypad8))
		{
			Animator.SetBool("MagOutFromInv", value: true);
		}
		else
		{
			Animator.SetBool("MagOutFromInv", value: false);
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			if (AmmoInChamber > 0f)
			{
				Animator.SetBool("Misfire", value: true);
			}
		}
		else
		{
			Animator.SetBool("Misfire", value: false);
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			FiremodeSwitch();
		}
		Animator.SetBool("Reload", Input.GetKeyDown(KeyCode.R));
		Animator.SetBool("FiremodeSwitch", Input.GetKeyDown(KeyCode.X));
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			MagTypeNew = (MagTypeNew + 1) % MagTypes;
			Animator.SetInteger("MagTypeNew", MagTypeNew);
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			MagTypeOld = (MagTypeOld + 1) % MagTypes;
			Animator.SetInteger("MagTypeOld", MagTypeOld);
		}
		Animator.SetBool("ReloadFast", Input.GetKeyDown(KeyCode.T));
		if (Input.GetKeyDown(KeyCode.Y))
		{
			Animator.SetBool("LoadOne", value: true);
		}
		else
		{
			Animator.SetBool("LoadOne", value: false);
		}
		Animator.SetBool("Look", Input.GetKeyDown(KeyCode.L));
		Animator.SetBool("Inventory", inv);
		if (Input.GetKeyDown(KeyCode.I))
		{
			inv = !inv;
		}
		Animator.SetBool("GoMele", Input.GetKeyDown(KeyCode.B));
		Animator.SetBool("QuickFire", Input.GetKeyDown(KeyCode.V));
		if (Input.GetKey(KeyCode.Mouse1))
		{
			Debug.Log("MOUSE2");
			CurrentAltFire++;
			if (CurrentAltFire < 0)
			{
				CurrentAltFire = 0;
			}
			if (CurrentAltFire >= AltFireVars.Count)
			{
				CurrentAltFire = 0;
			}
			if (AltFireVars.Count > 0)
			{
				Animator.SetBool("AltFire", value: true);
				Animator.SetInteger("AltFireVar", AltFireVars[CurrentAltFire]);
			}
		}
		else
		{
			Animator.SetBool("AltFire", value: false);
		}
		AnimatorStateInfo currentAnimatorStateInfo = Animator.GetCurrentAnimatorStateInfo(1);
		IdleTime += Time.deltaTime;
		if (!currentAnimatorStateInfo.IsName("IDLE"))
		{
			IdleTime = 0f;
		}
		if (IdleTime > IdleTimeMax)
		{
			IdleTime = 0f;
			IdleRandomValue = Mathf.RoundToInt(UnityEngine.Random.Range(IdleRandomMin, IdleRandomMax + 1f));
			Animator.SetFloat("IdleVar", IdleRandomValue);
			Animator.SetTrigger("IdleTime");
		}
		Animator.SetBool("MagFull", (int)Animator.GetFloat("AmmoInMag") >= Animator.GetInteger("MagAmmoMax"));
		if (Math.Abs(Input.mouseScrollDelta.y) >= float.Epsilon)
		{
			Animator.SetBool("Active", !Animator.GetBool("Active"));
		}
	}
}
