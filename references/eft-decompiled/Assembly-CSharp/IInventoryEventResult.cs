using Diz.LanguageExtensions;

public interface IInventoryEventResult
{
	Error Error { get; }

	bool Succeeded { get; }

	bool Failed { get; }
}
