using System;
using System.Collections.Generic;
using EFT;

public class DecisionQueue : GClass429
{
	[NonSerialized]
	public Queue<GClass537> DecisionQueue = new Queue<GClass537>();

	public int Count => DecisionQueue.Count;

	public DecisionQueue(BotOwner owner)
		: base(owner)
	{
	}

	public void Clear()
	{
		DecisionQueue.Clear();
	}

	public void Enqueue(GClass537 nextDecision)
	{
		DecisionQueue.Enqueue(nextDecision);
	}

	public GClass537 Peek()
	{
		return DecisionQueue.Peek();
	}

	public GClass537 Dequeue()
	{
		return DecisionQueue.Dequeue();
	}

	public bool HaveDeferredDecisions()
	{
		return DecisionQueue.Count > 0;
	}
}
