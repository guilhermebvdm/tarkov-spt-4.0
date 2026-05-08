using UnityEngine;

public abstract class GClass10
{
	public static void Clear()
	{
	}

	public static void LogToFilter(string inLog, string inFilterName, Object inContext = null)
	{
		Debug.Log(inLog + "\nCPAPI:{\"cmd\":\"Filter\", \"name\":\"" + inFilterName + "\"}", inContext);
	}

	public static void LogAsType(string inLog, string inTypeName, Object inContext = null)
	{
		Debug.Log(inLog + "\nCPAPI:{\"cmd\":\"LogType\", \"name\":\"" + inTypeName + "\"}", inContext);
	}

	public static void Watch(string inName, string inValue)
	{
		Debug.Log(inName + " : " + inValue + "\nCPAPI:{\"cmd\":\"Watch\", \"name\":\"" + inName + "\"}");
	}

	public static void Search(string inText)
	{
		Debug.Log("\nCPAPI:{\"cmd\":\"Search\", \"text\":\"" + inText + "\"}");
	}
}
