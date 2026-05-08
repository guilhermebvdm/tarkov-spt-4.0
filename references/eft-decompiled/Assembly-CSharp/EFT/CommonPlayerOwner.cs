using EFT.UI;
using JetBrains.Annotations;

namespace EFT;

public abstract class CommonPlayerOwner<TSession> : GamePlayerOwner where TSession : IBackEndSession
{
	protected new TSession Session;

	public static T smethod_3<T>(Player player, IInputTree inputTree, InsuranceCompanyClass insurance, IBackEndSession session, GameUI gameUI, GameDateTime gameDateTime, [CanBeNull] LocationSettingsClass.Location location) where T : CommonPlayerOwner<TSession>
	{
		return GamePlayerOwner.smethod_2<T>(player, inputTree, insurance, session, gameUI, gameDateTime, location);
	}

	public CommonPlayerOwner()
	{
	}
}
