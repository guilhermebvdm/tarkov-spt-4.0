using EFT;
using UnityEngine;

public class DataSenderControl : MonoBehaviour
{
	private const KeyCode keyCode_0 = KeyCode.F5;

	private ClientPlayer clientPlayer_0;

	public ClientPlayer ClientPlayer_0
	{
		get
		{
			if (clientPlayer_0 == null)
			{
				clientPlayer_0 = Object.FindObjectOfType<ClientPlayer>();
			}
			return clientPlayer_0;
		}
	}

	public static void Add()
	{
		GClass852.Add<DataSenderControl>();
		Debug.Log("DataSenderControl switch key: " + KeyCode.F5);
	}

	public static void Remove()
	{
		GClass852.Remove<DataSenderControl>();
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.F5))
		{
			ClientPlayer.GClass2351 gClass = (ClientPlayer.GClass2351)ClientPlayer_0.GetDataSender();
			gClass.PreventDispatch = !gClass.PreventDispatch;
			Debug.LogFormat("dataSender.PreventDispatch = {0}", gClass.PreventDispatch);
		}
		_ = clientPlayer_0 != null;
	}
}
