using AnimationEventSystem;

public interface IEventsConsumer
{
	void ReceiveEvent(AnimationEvent @event, IAnimator animator, in AnimatorStateInfoWrapper stateInfo, float normalizedTime);
}
