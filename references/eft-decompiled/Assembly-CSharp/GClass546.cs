using System.Collections.Generic;
using System.Text;

public class GClass546
{
	public List<string> _collectedErrors = new List<string>();

	public void AddLog(string log)
	{
		_collectedErrors.Add(log);
	}

	public void ClearAndPrint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string collectedError in _collectedErrors)
		{
			stringBuilder.Append("\n" + collectedError);
		}
		_collectedErrors.Clear();
	}
}
