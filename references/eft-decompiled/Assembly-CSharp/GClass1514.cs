using System.Collections.Generic;

public abstract class GClass1514
{
	public static bool AreEqual<T>(this IList<T> firstList, IList<T> secondList)
	{
		if (firstList == null && secondList == null)
		{
			return true;
		}
		if (firstList != null && secondList != null)
		{
			if (firstList.Count != secondList.Count)
			{
				return false;
			}
			int num = 0;
			while (true)
			{
				if (num < firstList.Count)
				{
					if (!object.Equals(firstList[num], secondList[num]))
					{
						break;
					}
					num++;
					continue;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
