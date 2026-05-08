using System.Collections.Generic;
using UnityEngine;

public class TestRandomizer : MonoBehaviour
{
	public void Awake()
	{
		List<int> list = new List<int>(2) { 1, 2 };
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 10000; i++)
		{
			if (GClass856.RandomElement(list) == 1)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		Debug.Log(">>>>>>>>" + num + "   " + num2);
	}
}
