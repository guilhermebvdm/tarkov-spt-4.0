using ChatShared;
using Diz.Binding;

public class GClass1070 : IUpdatable<GClass1070>
{
	public string _id;

	public UpdatableChatMember From;

	public UpdatableChatMember To;

	public double Date;

	public UpdatableChatMember Profile;

	public bool InputInvitation;

	public GClass1070()
	{
	}

	public GClass1070(GClass1056 invitation, bool input)
	{
		_id = invitation._id;
		From = invitation.From;
		To = invitation.To;
		Date = invitation.Date;
		Profile = invitation.Profile;
		InputInvitation = input;
	}

	public bool Compare(GClass1070 other)
	{
		return _id == other._id;
	}

	public void UpdateFromAnotherItem(GClass1070 other)
	{
		From = other.From;
		To = other.To;
		Date = other.Date;
		Profile = other.Profile;
		InputInvitation = other.InputInvitation;
	}
}
