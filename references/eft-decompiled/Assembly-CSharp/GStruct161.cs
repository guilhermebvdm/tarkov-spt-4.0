using EFT;

public struct GStruct161
{
	public EDisconnectionCode DisconnectionCode;

	public string AdditionalInfo;

	public string OriginalTechnicalMessage;

	public override string ToString()
	{
		return $"DisconnectionCode: {DisconnectionCode}, AdditionalInfo: {AdditionalInfo}, OriginalTechnicalMessage: {OriginalTechnicalMessage}";
	}
}
