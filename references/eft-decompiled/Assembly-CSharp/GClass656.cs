using System;
using System.Collections.Generic;

public class GClass656
{
	[NonSerialized]
	public List<GClass654> List_0 = new List<GClass654>();

	public int Count => List_0.Count;

	public void Enqueue(GClass654 newReq)
	{
		List_0.Add(newReq);
	}

	public GClass654 Dequeue()
	{
		if (List_0.Count == 0)
		{
			return null;
		}
		GClass654 result = List_0[0];
		List_0.RemoveAt(0);
		return result;
	}

	public void FinishTimedOutRequests()
	{
		for (int num = List_0.Count - 1; num >= 0; num--)
		{
			GClass654 gClass = List_0[num];
			if (gClass.CheckTimedOut())
			{
				List_0.RemoveAt(num);
				gClass.FinishWithTimeout();
			}
		}
	}
}
