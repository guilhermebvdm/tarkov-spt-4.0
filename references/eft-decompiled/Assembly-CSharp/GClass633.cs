using System.Collections.Generic;

public class GClass633
{
	public float distance;

	public int shoots;

	public int hits;

	public float damage;

	public float armorDamage;

	public Dictionary<string, int> bonesHits = new Dictionary<string, int>();

	public Dictionary<string, int> partsHits = new Dictionary<string, int>();

	public void BoneHit(string bone, EBodyPart bodyType)
	{
		if (bonesHits.ContainsKey(bone))
		{
			bonesHits[bone]++;
		}
		else
		{
			bonesHits.Add(bone, 1);
		}
		string key = bodyType.ToString();
		if (partsHits.ContainsKey(key))
		{
			partsHits[key]++;
		}
		else
		{
			partsHits.Add(key, 1);
		}
	}
}
