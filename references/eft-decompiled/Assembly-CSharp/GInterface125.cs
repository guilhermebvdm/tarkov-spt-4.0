using System.Collections.Generic;
using UnityEngine.Animations;

public interface GInterface125
{
	int NodesCount { get; }

	int LayerIndex { get; }

	AnimationMixerPlayable Mixer { get; }

	void EnableProfiling(bool val);

	List<GStruct141> GetLastProcessedActiveNodes();

	bool IsNextStateClip(int clipIndex);

	float GetNodeTime(int clipIndex);

	float GetNodePreviousTime(int clipIndex);

	void RaiseImmediateTransitionHappened();
}
