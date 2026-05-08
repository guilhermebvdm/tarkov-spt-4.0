namespace AnimationEventSystem;

public interface IAnimatorEventParameter
{
	bool BoolParam { get; }

	float FloatParam { get; }

	int IntParam { get; }

	string StringParam { get; }

	EAnimationEventParamType ParamType { get; }
}
