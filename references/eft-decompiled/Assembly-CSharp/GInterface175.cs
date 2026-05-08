public interface GInterface175
{
	int Count { get; }

	BotCreationDataClass Data { get; }

	bool CanBeRemoved { get; }

	float CreatedTime { get; }

	float LastCheckTime { get; set; }

	void OnDelayFinished();
}
