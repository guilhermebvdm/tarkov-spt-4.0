namespace EFT;

public enum NetworkError
{
	Ok,
	WrongHost,
	WrongConnection,
	WrongChannel,
	NoResources,
	BadMessage,
	Timeout,
	MessageToLong,
	WrongOperation,
	VersionMismatch,
	CRCMismatch,
	DNSFailure,
	UsageError
}
