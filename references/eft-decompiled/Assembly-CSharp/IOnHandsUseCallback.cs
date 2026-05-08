using Comfort.Common;

public interface IOnHandsUseCallback : IHandsController
{
	void SetOnUsedCallback(Callback<IOnHandsUseCallback> callback);

	Callback<IOnHandsUseCallback> GetOnUsedCallback();
}
