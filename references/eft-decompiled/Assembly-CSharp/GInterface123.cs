using UnityEngine;
using UnityEngine.Playables;

public interface GInterface123
{
	IAnimator FastAnimator { get; }

	PlayableGraph Graph { get; }

	int LayersCount { get; }

	bool ManualUpdateMode { get; }

	void Play();

	void Stop();

	void Process(bool isVisible, float dt);

	void SetLayerWeight(int layerIndex, float weight);

	float GetLayerWeight(int layerIndex);

	void RaiseImmediateTransitionHappened(int layerIndex);

	void SetCuller(GInterface124 culler);

	GInterface125 GetLayerProcessor(int layerIndex);

	AnimationClip GetClipByIndex(int layerIndex, int clipIndex);
}
