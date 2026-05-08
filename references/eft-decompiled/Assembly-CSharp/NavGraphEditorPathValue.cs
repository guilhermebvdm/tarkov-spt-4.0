using System;

[Serializable]
public class NavGraphEditorPathValue
{
	public NavGraphEditorPath Path;

	public int Value;

	public float Coef = 1f;

	public NavGraphEditorPathValue(NavGraphEditorPath path, int farDist)
	{
		Path = path;
		Value = farDist;
		int num = 10 - farDist + 1;
		Coef = (float)num * 100f / 10f;
	}

	public void DropCoef()
	{
		Path.DropCoef(Coef);
	}

	public void SetCoef()
	{
		Path.SetCoef(Coef);
	}
}
