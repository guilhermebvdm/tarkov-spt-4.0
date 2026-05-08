using UnityEngine;

public class SkirmishResultDebug : MonoBehaviour
{
	public Vector3 FromAgressor;

	public Vector3 ToVictim;

	public string Aggressor;

	public string Victim;

	public float Dist;

	public ESkirmishLogType LogType;

	public string Part;

	public string Ricochet;

	public string Penetrate;

	public void Init(Vector3 from, Vector3 to, string aggressor, string victim, ESkirmishLogType logType, string part, string isRicochet, string isPenetrate)
	{
		Aggressor = aggressor;
		Victim = victim;
		FromAgressor = from;
		ToVictim = to;
		LogType = logType;
		Part = part;
		Ricochet = isRicochet;
		Penetrate = isPenetrate;
		bool flag = FromAgressor.sqrMagnitude > 0.001f;
		bool flag2 = ToVictim.sqrMagnitude > 0.001f;
		Dist = (FromAgressor - to).magnitude;
		if (flag2 && flag)
		{
			base.transform.position = (from + to) * 0.5f;
		}
		else if (flag2)
		{
			base.transform.position = to;
		}
		else if (flag)
		{
			base.transform.position = from;
		}
	}

	public void OnDrawGizmosSelected()
	{
		bool flag = FromAgressor.sqrMagnitude > 0.001f;
		bool flag2 = ToVictim.sqrMagnitude > 0.001f;
		Color color = Color.red;
		float num = 0.2f;
		switch (LogType)
		{
		case ESkirmishLogType.shotNotApproved:
			if (flag)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawCube(FromAgressor, new Vector3(num, num, num));
			}
			if (flag2)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(ToVictim, new Vector3(num, num, num));
			}
			num = 0.05f;
			color = Color.red;
			break;
		case ESkirmishLogType.knifeHit:
			color = Color.magenta;
			break;
		case ESkirmishLogType.applyShot:
			color = Color.yellow;
			break;
		case ESkirmishLogType.shotApproved:
			color = Color.red;
			break;
		case ESkirmishLogType.onBeenKilledBy:
			color = Color.black;
			break;
		}
		if (flag)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(FromAgressor, num);
			Gizmos.DrawWireSphere(FromAgressor, num);
		}
		if (flag2)
		{
			Gizmos.color = color;
			Gizmos.DrawSphere(ToVictim, num);
			Gizmos.DrawWireSphere(ToVictim, num);
		}
		if (flag2 && flag)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(FromAgressor, ToVictim);
		}
	}
}
