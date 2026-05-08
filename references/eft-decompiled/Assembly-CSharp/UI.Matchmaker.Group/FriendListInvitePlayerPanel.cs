using System.Linq;
using System.Runtime.CompilerServices;
using ChatShared;
using EFT.UI;
using EFT.UI.Matchmaker;
using UnityEngine;

namespace UI.Matchmaker.Group;

public class FriendListInvitePlayerPanel : UIElement
{
	[CompilerGenerated]
	public class Class724
	{
		public GroupPlayerViewModelClass groupMember;

		public bool method_0(GroupPlayerViewModelClass item)
		{
			return item.AccountId == groupMember.AccountId;
		}
	}

	[SerializeField]
	private RaidReadyList _friendsList;

	[SerializeField]
	private GroupPlayersList _groupPlayersList;

	private IMatchmakerPlayersController iMatchmakerPlayersController;

	private GClass1630<GroupPlayerViewModelClass> gclass1630_0;

	private GClass1628<UpdatableChatMember> gclass1628_0;

	public void Show(string profileAid, GClass1628<UpdatableChatMember> friendsList, IMatchmakerPlayersController matchmakerPlayersController)
	{
		UI.Dispose();
		ShowGameObject();
		iMatchmakerPlayersController = matchmakerPlayersController;
		gclass1628_0 = friendsList;
		gclass1630_0 = new GClass1630<GroupPlayerViewModelClass>();
		UI.BindEvent(friendsList.ItemsChanged, method_1);
		UI.BindEvent(iMatchmakerPlayersController.GroupPlayers.ItemsChanged, method_0);
		_friendsList.Show(gclass1630_0, matchmakerPlayersController.SentInvites, friendsList, profileAid);
		_friendsList.OnPlayerClick += RequestContextMenuForPlayer;
		_groupPlayersList.Show(matchmakerPlayersController.GroupPlayers, matchmakerPlayersController.GroupState, friendsList, profileAid);
		_groupPlayersList.OnPlayerClick += RequestContextMenuForPlayer;
		UI.AddDisposable(_friendsList);
		UI.AddDisposable(_groupPlayersList);
		UI.AddDisposable(delegate
		{
			_friendsList.OnPlayerClick -= RequestContextMenuForPlayer;
			_groupPlayersList.OnPlayerClick -= RequestContextMenuForPlayer;
		});
	}

	public void method_0()
	{
		method_1();
		GClass1628<GroupPlayerViewModelClass> groupPlayers = iMatchmakerPlayersController.GroupPlayers;
		if (groupPlayers.Count == 1)
		{
			return;
		}
		foreach (GroupPlayerViewModelClass groupMember in groupPlayers)
		{
			GroupPlayerViewModelClass groupPlayerViewModelClass = gclass1630_0.FirstOrDefault((GroupPlayerViewModelClass item) => item.AccountId == groupMember.AccountId);
			if (groupPlayerViewModelClass != null)
			{
				gclass1630_0.Remove(groupPlayerViewModelClass);
			}
		}
	}

	public void method_1()
	{
		GClass1630<GroupPlayerViewModelClass> gClass = new GClass1630<GroupPlayerViewModelClass>();
		foreach (UpdatableChatMember item in gclass1628_0)
		{
			gClass.Add(new GroupPlayerViewModelClass(new GroupPlayerDataClass
			{
				AccountId = item.AccountId,
				Id = item.Id,
				Info = GClass1410.GetFromChatMemberInfo(item.Info)
			}));
		}
		gclass1630_0.UpdateItems(gClass);
	}

	public void RequestContextMenuForPlayer(GroupPlayerViewModelClass player, Vector2 position)
	{
		iMatchmakerPlayersController.RequestContextMenuForPlayer(player, position);
	}

	[CompilerGenerated]
	public void method_2()
	{
		_friendsList.OnPlayerClick -= RequestContextMenuForPlayer;
		_groupPlayersList.OnPlayerClick -= RequestContextMenuForPlayer;
	}
}
