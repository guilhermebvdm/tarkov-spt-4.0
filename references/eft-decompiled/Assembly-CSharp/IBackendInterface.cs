using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;

public interface IBackendInterface<T> : IBackendStatus where T : IBackEndSession
{
	T Session { get; }

	Task<T> CreateClientSession();

	Task<ESessionMode> GetSessionMode(ESessionMode sessionMode, bool sessionIsRecreated);

	Task<GClass1408> GetTemplates(GClass1408 returnInCaseOfCache);

	void RegisterDevice(string email, string activateCode, Callback onFinished);

	Task RegenerateToken();

	Task ValidateVersion();

	Task<Result<GClass903>> Login(string login, string password);

	Task<GClass1807> GetMainMenuLocalization(string locale);

	Task<Dictionary<string, string>> GetAvailableLanguages();

	Task Logout();

	Task Destroy();

	void EnableDiagnostics(bool webDiagnosticsEnabled);
}
