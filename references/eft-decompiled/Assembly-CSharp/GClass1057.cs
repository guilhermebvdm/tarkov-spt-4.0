using System;
using System.Runtime.CompilerServices;
using ChatShared;
using EFT;

public class GClass1057 : ISerializer<GClass1056>
{
	[Serializable]
	[CompilerGenerated]
	public class Class762
	{
		public static readonly Class762 class762_0 = new Class762();

		public static Func<string, UpdatableChatMember> func_0;

		public static Func<string, UpdatableChatMember> func_1;

		public UpdatableChatMember method_0(string id)
		{
			return new UpdatableChatMember(id);
		}

		public UpdatableChatMember method_1(string id)
		{
			return new UpdatableChatMember(id);
		}
	}

	public string _id;

	public string from;

	public string to;

	public double date;

	public UpdatableChatMember profile;

	public GClass1056 Deserialize()
	{
		return new GClass1056
		{
			_id = _id,
			Date = date,
			From = UpdatableChatMember.FindOrCreate(from, (string id) => new UpdatableChatMember(id)),
			To = UpdatableChatMember.FindOrCreate(to, (string id) => new UpdatableChatMember(id)),
			Profile = profile
		};
	}

	public object Serialize(GClass1056 t)
	{
		throw new NotImplementedException();
	}
}
