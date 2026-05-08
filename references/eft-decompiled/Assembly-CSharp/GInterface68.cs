using Audio;

public interface GInterface68
{
	void Log(EAudioLogLevel level, string message);

	void SetLogLevel(EAudioLogLevel levelMask);

	EAudioLogLevel GetCurrentLogLevelMask();
}
