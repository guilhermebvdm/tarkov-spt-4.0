public interface GInterface84 : GInterface69
{
	void Init();

	void SetTargetLowPassSettings(float targetFrequency, float targetQ, bool applyImmediately = false);

	void SetTargetHighPassSettings(float targetFrequency, float targetQ, bool applyImmediately = false);

	void ResetValues();

	void SetActiveHighPass(bool active);

	void SetActiveLowPass(bool active);
}
