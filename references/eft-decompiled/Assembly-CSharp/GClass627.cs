public class GClass627
{
	public float Damage;

	public string Bone = "null";

	public string Weapon = "null";

	public GClass627(DamageInfoStruct d)
	{
		Damage = d.Damage;
		if (d.HitCollider != null)
		{
			Bone = d.HitCollider.name;
		}
		if (d.Weapon != null)
		{
			Weapon = d.Weapon.TemplateId;
		}
	}
}
