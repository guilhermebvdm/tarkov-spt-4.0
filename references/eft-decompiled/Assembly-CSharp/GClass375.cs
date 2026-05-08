using UnityEngine;

public class GClass375
{
	public GClass374 S1;

	public GClass374 S2;

	public NavPoint Center;

	public float Ang;

	public GClass375(GClass374 s1, GClass374 s2)
	{
		S1 = s1;
		S2 = s2;
		Ang = method_0(s1, s2);
		s1.SetLeft(s2, Ang);
		s2.SetRight(s1, Ang);
	}

	public float method_0(GClass374 s1, GClass374 s2)
	{
		Vector3 vector;
		Vector3 to;
		if (s1.P1 == s2.P2)
		{
			Center = s1.P1;
			vector = s1.P2.Pos - Center.Pos;
			to = s2.P1.Pos - Center.Pos;
		}
		else if (s1.P2 == s2.P2)
		{
			Center = s1.P2;
			vector = s1.P1.Pos - Center.Pos;
			to = s2.P1.Pos - Center.Pos;
		}
		else if (s1.P1 == s2.P1)
		{
			Center = s1.P1;
			vector = s1.P2.Pos - Center.Pos;
			to = s2.P2.Pos - Center.Pos;
		}
		else
		{
			Center = s1.P2;
			vector = s1.P1.Pos - Center.Pos;
			to = s2.P2.Pos - Center.Pos;
		}
		return Vector3.Angle(vector, to);
	}

	public void DebugDraw(float up)
	{
		Vector3 offset = up * Vector3.up;
		S1.DebugDraw(offset);
		S2.DebugDraw(offset);
		GClass810.DebugWireSphere(Center.Pos, Color.white, (180f - Ang) / 180f, 10f);
	}
}
