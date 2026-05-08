using UnityEngine;

public class GAttribute20 : PropertyAttribute
{
	public string name;

	public string color = "white";

	public GAttribute20(string name)
	{
		this.name = name;
		color = "white";
	}

	public GAttribute20(string name, string color)
	{
		this.name = name;
		this.color = color;
	}
}
