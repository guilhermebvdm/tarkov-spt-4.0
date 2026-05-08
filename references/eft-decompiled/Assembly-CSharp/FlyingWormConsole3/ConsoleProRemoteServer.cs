using UnityEngine;

namespace FlyingWormConsole3;

public class ConsoleProRemoteServer : MonoBehaviour
{
	public bool useNATPunch;

	public int port = 51000;

	public void Awake()
	{
		Debug.Log("Console Pro Remote Server is disabled in release mode, please use a Development build or define DEBUG to use it");
	}
}
