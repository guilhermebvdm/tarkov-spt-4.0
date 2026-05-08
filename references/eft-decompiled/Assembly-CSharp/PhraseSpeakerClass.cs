using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using JetBrains.Annotations;
using NLog;
using UnityEngine;

public class PhraseSpeakerClass
{
	public class Class396 : LoggerClass
	{
		public Class396(LoggerMode mode)
			: base(LogManager.GetLogger("speaker"), mode)
		{
		}
	}

	[NonSerialized]
	public const float Float_0 = 0.1f;

	[NonSerialized]
	public static Class396 Class396_0 = new Class396(LoggerMode.Add);

	public readonly Dictionary<EPhraseTrigger, TagBank> PhrasesBanks = GClass866<EPhraseTrigger>.GetDictWith<TagBank>();

	public bool OnDemandOnly;

	public float ReleaseTime;

	public bool Speaking;

	public EPhraseTrigger QueuedEvent = EPhraseTrigger.PhraseNone;

	public ETagStatus QueuedTags;

	public ETagStatus SideTag;

	public float QueuedDelay;

	[CompilerGenerated]
	private Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> action_0;

	[CompilerGenerated]
	private Action<bool> action_1;

	public TaggedClip Clip;

	public BifacialTransform TrackTransform;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public int Id
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public int Importance => Int_0;

	public bool Busy => Time.time < ReleaseTime;

	public float TimeLeft => ReleaseTime - Time.time;

	public string PlayerVoice
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
		[CompilerGenerated]
		set
		{
			String_0 = value;
		}
	}

	public event Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> OnPhraseTold
	{
		[CompilerGenerated]
		add
		{
			Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> action = action_0;
			Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> action2;
			do
			{
				action2 = action;
				Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> value2 = (Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> action = action_0;
			Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> action2;
			do
			{
				action2 = action;
				Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass> value2 = (Action<EPhraseTrigger, TaggedClip, TagBank, PhraseSpeakerClass>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<bool> OnRelease
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_1;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_1;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Queue(EPhraseTrigger trigger, ETagStatus tags, float delay, bool demand)
	{
		if (!OnDemandOnly || (OnDemandOnly && demand))
		{
			QueuedEvent = trigger;
			QueuedTags = tags;
			QueuedDelay = Time.time + delay;
		}
	}

	public void Shut()
	{
		QueuedEvent = EPhraseTrigger.PhraseNone;
		ReleaseTime = Time.time;
	}

	[CanBeNull]
	public TagBank Play(EPhraseTrigger trigger, ETagStatus tags, bool demand = false, int? importance = null)
	{
		if (trigger == EPhraseTrigger.None)
		{
			return null;
		}
		string key = ResourceKeyManagerAbstractClass.TakePhrasePath(PlayerVoice);
		if (!GClass1857.TryGetAsset<Voice>(Singleton<IEasyAssets>.Instance, out var asset, key))
		{
			Class396_0.LogError("For some reason " + PlayerVoice + " is null");
			return null;
		}
		TagBank tagBank = null;
		TagBank[] banks = asset.Banks;
		foreach (TagBank tagBank2 in banks)
		{
			if (tagBank2.Trigger == trigger)
			{
				tagBank = tagBank2;
				break;
			}
		}
		if (tagBank == null)
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " can't be played due " + PlayerVoice + " doesn't have bank for it");
			return null;
		}
		if (!importance.HasValue)
		{
			importance = tagBank.Importance;
		}
		if (Busy && importance <= Int_0)
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " skipped due speaker is busy with " + Int_0);
			return null;
		}
		if (!Singleton<GameWorld>.Instance.SpeakerManager.FreeToSpeak(trigger, Id))
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " skipped due speaker is not free to speak");
			return null;
		}
		tags |= Singleton<GameWorld>.Instance.SpeakerManager.GetGroupStatus(this);
		TaggedClip taggedClip = tagBank.Match((int)tags);
		if (taggedClip == null)
		{
			Class396_0.LogInfo("Bank " + tagBank.name + "  has no match for " + GClass867.ToStringNoBox(trigger) + " with " + tags);
			return null;
		}
		if (Busy)
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " will overwrite current and queued triggers");
			QueuedEvent = EPhraseTrigger.PhraseNone;
		}
		Speaking = true;
		ReleaseTime = Time.time + taggedClip.Length + 0.1f;
		Clip = taggedClip;
		action_0?.Invoke(trigger, taggedClip, tagBank, this);
		if (tagBank.ChainEvent.Who == Chain.ESpeaker.Self && UnityEngine.Random.Range(0, 100) < tagBank.ChainEvent.Probability)
		{
			Queue(tagBank.ChainEvent.Event, tags, taggedClip.Length, demand: false);
		}
		Int_0 = importance.Value;
		return tagBank;
	}

	public void PlayDirect(EPhraseTrigger trigger, int index)
	{
		if (PhrasesBanks.TryGetValue(trigger, out var value))
		{
			if (index >= value.Clips.Length)
			{
				Debug.LogErrorFormat("Can't find clip with index:{0}, in phrases bank '{1}'", index, trigger);
			}
			else
			{
				TaggedClip taggedClip = value.Clips[index];
				Speaking = true;
				ReleaseTime = Time.time + taggedClip.Length + 0.1f;
				Clip = taggedClip;
				action_0?.Invoke(trigger, taggedClip, value, this);
			}
		}
	}

	public void PlayExternal(TagBank bank, EPhraseTrigger trigger, ETagStatus tags, int? importance = null)
	{
		if (trigger == EPhraseTrigger.None)
		{
			return;
		}
		if (bank == null)
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " can't be played");
			return;
		}
		if (!importance.HasValue)
		{
			importance = bank.Importance;
		}
		if (Busy && importance <= Int_0)
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " skipped due speaker is busy with " + Int_0);
			return;
		}
		if (!Singleton<GameWorld>.Instance.SpeakerManager.FreeToSpeak(trigger, Id))
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " skipped due speaker is not free to speak");
			return;
		}
		tags |= Singleton<GameWorld>.Instance.SpeakerManager.GetGroupStatus(this);
		TaggedClip taggedClip = bank.Match((int)tags);
		if (taggedClip == null)
		{
			Class396_0.LogInfo("Bank " + bank.name + "  has no match for " + GClass867.ToStringNoBox(trigger) + " with " + tags);
			return;
		}
		if (Busy)
		{
			Class396_0.LogInfo("Trigger " + GClass867.ToStringNoBox(trigger) + " will overwrite current and queued triggers");
			QueuedEvent = EPhraseTrigger.PhraseNone;
		}
		Speaking = true;
		ReleaseTime = Time.time + taggedClip.Length + 0.1f;
		Clip = taggedClip;
		action_0?.Invoke(trigger, taggedClip, bank, this);
		if (bank.ChainEvent.Who == Chain.ESpeaker.Self && UnityEngine.Random.Range(0, 100) < bank.ChainEvent.Probability)
		{
			Queue(bank.ChainEvent.Event, tags, taggedClip.Length, demand: false);
		}
		Int_0 = importance.Value;
	}

	public virtual void Init(EPlayerSide side, int id, string playerVoice, bool registerInSpeakerManager = true)
	{
		SideTag = side switch
		{
			EPlayerSide.Usec => ETagStatus.Usec, 
			EPlayerSide.Bear => ETagStatus.Bear, 
			_ => ETagStatus.Scav, 
		};
		TagBank[] banks = GClass1857.GetAsset<Voice>(Singleton<IEasyAssets>.Instance, ResourceKeyManagerAbstractClass.TakePhrasePath(playerVoice)).Banks;
		foreach (TagBank tagBank in banks)
		{
			if (tagBank != null && !PhrasesBanks.ContainsKey(tagBank.Trigger))
			{
				PhrasesBanks.Add(tagBank.Trigger, tagBank);
			}
		}
		PlayerVoice = playerVoice;
		Id = id;
		if (registerInSpeakerManager)
		{
			Singleton<GameWorld>.Instance.SpeakerManager.AddSpeaker(this);
		}
	}

	public void PlayDirectRandom(EPhraseTrigger trigger)
	{
		if (PhrasesBanks.TryGetValue(trigger, out var value))
		{
			PlayDirect(trigger, UnityEngine.Random.Range(0, value.Clips.Length));
		}
	}

	public void ReplaceVoice(EPlayerSide side, string playerVoice)
	{
		Voice asset = GClass1857.GetAsset<Voice>(Singleton<IEasyAssets>.Instance, ResourceKeyManagerAbstractClass.TakePhrasePath(playerVoice));
		if (asset == null)
		{
			return;
		}
		PhrasesBanks.Clear();
		TagBank[] banks = asset.Banks;
		foreach (TagBank tagBank in banks)
		{
			if (tagBank != null && !PhrasesBanks.ContainsKey(tagBank.Trigger))
			{
				PhrasesBanks.Add(tagBank.Trigger, tagBank);
			}
		}
	}

	public void ManualUpdate(float dt)
	{
		if (Busy)
		{
			return;
		}
		if (QueuedEvent != EPhraseTrigger.PhraseNone)
		{
			if (QueuedDelay < Time.time)
			{
				Play(QueuedEvent, QueuedTags, demand: true);
				QueuedEvent = EPhraseTrigger.PhraseNone;
			}
		}
		else if (Speaking)
		{
			Speaking = false;
			action_1?.Invoke(obj: false);
		}
	}

	public void OnDestroy()
	{
		action_1?.Invoke(obj: true);
	}
}
