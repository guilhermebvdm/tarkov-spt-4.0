using System;
using UnityEngine;

namespace EFT;

[Serializable]
public class ChannelQOS
{
	[SerializeField]
	public QosType m_Type;

	[SerializeField]
	public bool m_BelongsSharedOrderChannel;

	public QosType QOS => m_Type;

	public bool BelongsToSharedOrderChannel => m_BelongsSharedOrderChannel;

	public ChannelQOS(QosType value)
	{
		m_Type = value;
		m_BelongsSharedOrderChannel = false;
	}

	public ChannelQOS()
	{
		m_Type = QosType.Unreliable;
		m_BelongsSharedOrderChannel = false;
	}

	public ChannelQOS(ChannelQOS channel)
	{
		if (channel == null)
		{
			throw new NullReferenceException("channel is not defined");
		}
		m_Type = channel.m_Type;
		m_BelongsSharedOrderChannel = channel.m_BelongsSharedOrderChannel;
	}
}
