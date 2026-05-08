using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ChatShared;
using Comfort.Common;
using Comfort.Communication;
using Comfort.Net;
using UnityEngine;

namespace Communications;

public class ChatClient : MonoBehaviourSingleton<ChatClient>, IUnityScheduller
{
	[Serializable]
	[CompilerGenerated]
	public class Class752
	{
		public static readonly Class752 class752_0 = new Class752();

		public static Func<ChatServerClass, DateTime> func_0;

		public DateTime method_0(ChatServerClass item)
		{
			return item.DateTime;
		}
	}

	private string string_0;

	private ICommunicator icommunicator_0;

	private ITcpClient itcpClient_0;

	private ISession iSession;

	private string string_1;

	private int int_0;

	[CompilerGenerated]
	private IChatsSession ichatsSession_0;

	[CompilerGenerated]
	private ChatController chatController_0;

	[CompilerGenerated]
	private Action action_0;

	public IChatsSession Session
	{
		[CompilerGenerated]
		get
		{
			return ichatsSession_0;
		}
		[CompilerGenerated]
		set
		{
			ichatsSession_0 = value;
		}
	}

	public ChatController ChatController
	{
		[CompilerGenerated]
		get
		{
			return chatController_0;
		}
		[CompilerGenerated]
		set
		{
			chatController_0 = value;
		}
	}

	public event Action Event
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static ChatClient smethod_0(string versionId, GameObject gameObject = null, CommunicatorFactory communicatorFactory = null)
	{
		if (gameObject == null)
		{
			gameObject = new GameObject("GlobalChatClient");
		}
		ChatClient chatClient = gameObject.AddComponent<ChatClient>();
		chatClient.string_0 = versionId;
		chatClient.ChatController = ChatController.Create(chatClient);
		if (communicatorFactory == null)
		{
			communicatorFactory = GClass1049.Create(chatClient.method_9);
		}
		chatClient.icommunicator_0 = communicatorFactory.CreateCommunicator(chatClient.method_9);
		chatClient.itcpClient_0 = TcpClientFactory.CreateClient(chatClient.method_5, chatClient);
		chatClient.itcpClient_0.StatusChangedEvent += chatClient.method_7;
		return chatClient;
	}

	public void method_0(ISession backEndSession)
	{
		iSession = backEndSession;
		if (!string.IsNullOrEmpty(string_1))
		{
			method_4();
		}
		else
		{
			iSession.GetChatServers(string_0, method_2);
		}
	}

	public void method_1()
	{
		itcpClient_0.Disconnect();
		iSession = null;
	}

	public void method_2(Result<ChatServerClass[]> result)
	{
		if (iSession == null)
		{
			return;
		}
		if (result.Succeed && result.Value.Length != 0)
		{
			ChatServerClass chatServerClass = result.Value.OrderByDescending((ChatServerClass item) => item.DateTime).First();
			string_1 = $"{chatServerClass.Ip}:{chatServerClass.Port}";
			method_4();
		}
		else
		{
			base.StartCoroutine(method_8());
		}
	}

	public void Reconnect(string ip, int port)
	{
		string_1 = $"{ip}:{port}";
		ChatController.SessionStop();
		itcpClient_0.Disconnect();
		base.StartCoroutine(method_3());
	}

	public IEnumerator method_3()
	{
		while (itcpClient_0.Status != Status.Disconnected)
		{
			yield return null;
		}
		if (iSession != null)
		{
			method_4();
		}
	}

	public void method_4()
	{
		Debug.Log("ChatClient connecting to:" + string_1);
		string phpSessionId = iSession.GetPhpSessionId();
		Status status = itcpClient_0.Status;
		if (status != Status.Disconnected && status != Status.ConnectingTimeout)
		{
			Debug.Log("Invalid connection status:" + status);
		}
		else
		{
			itcpClient_0.Connect(string_1, phpSessionId, string_0, TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0));
		}
	}

	public void method_5(IConnection connection)
	{
		int_0 = 0;
		Debug.Log("ChatClient OnConnect begin");
		if (iSession == null)
		{
			connection.Close(Comfort.Net.ErrorCode.Disconnected);
			Debug.Log("ChatClient OnConnect failed");
			return;
		}
		connection.ClosedEvent += delegate(object sender, ConnectionClosedEventArgs e)
		{
			method_6(e);
		};
		icommunicator_0.Connection = connection;
		Debug.Log("ChatClient OnConnect succeed");
		Session = icommunicator_0.Get<IChatsSession>();
		ChatController.SessionStart(iSession.SocialNetwork);
	}

	public void method_6(ConnectionStateChangedEventArgs args)
	{
		Debug.Log("ConnectionClosedEventArgs ErrorCode:" + args.ErrorCode);
		ChatController.SessionStop();
		Session = null;
	}

	public void method_7(StatusChangedEventArgs args)
	{
		Debug.Log($"Client dateTime:{EFTDateTimeClass.UtcNow} OnStatusChangedEvent State:{args.Status}  ErrorCode:{args.ErrorCode} StatusOld:{args.StatusOld}");
		if (args.Status == Status.ConnectingTimeout)
		{
			icommunicator_0.Connection = null;
			base.StartCoroutine(method_8());
		}
	}

	public IEnumerator method_8()
	{
		if (int_0 < 10)
		{
			int_0++;
		}
		Debug.Log("ReconnectCoroutune Timeout:" + int_0 * 30);
		yield return new WaitForSeconds(int_0 * 30);
		iSession?.GetChatServers(string_0, method_2);
	}

	public void method_9(string error)
	{
		Debug.LogError(error);
		method_1();
	}

	public void FixedUpdate()
	{
		action_0?.Invoke();
	}

	public override void OnDestroy()
	{
		iSession = null;
		itcpClient_0?.Destroy();
		base.OnDestroy();
	}

	public new Coroutine StartCoroutine(IEnumerator enumerator)
	{
		return base.StartCoroutine(enumerator);
	}

	Coroutine IUnityScheduller.StartCoroutine(IEnumerator enumerator)
	{
		//ILSpy generated this explicit interface implementation from .override directive in StartCoroutine
		return this.StartCoroutine(enumerator);
	}

	[CompilerGenerated]
	public void method_10(object sender, ConnectionClosedEventArgs e)
	{
		method_6(e);
	}
}
