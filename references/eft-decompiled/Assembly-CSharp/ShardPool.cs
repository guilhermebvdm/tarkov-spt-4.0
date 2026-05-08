using UnityEngine;

public class ShardPool : MonoBehaviour
{
	private static ShardPool shardPool_0;

	[HideInInspector]
	[SerializeField]
	private Shard[] shards;

	[SerializeField]
	private int maxPoolSize;

	private int int_0;

	public static int ShardsUsed
	{
		get
		{
			CheckPool();
			return shardPool_0.int_0;
		}
	}

	public bool IsPopulated => shards != null;

	public static Shard NextShard
	{
		get
		{
			CheckPool();
			if (Boolean_0)
			{
				return shardPool_0.shards[shardPool_0.int_0++];
			}
			return null;
		}
	}

	public static bool Boolean_0 => shardPool_0.int_0 + 1 < shardPool_0.shards.Length;

	public void Awake()
	{
		shardPool_0 = this;
		PopulatePool();
	}

	public static void CheckPool()
	{
		if (shardPool_0 == null)
		{
			smethod_0();
		}
		shardPool_0.PopulatePool();
	}

	public static void smethod_0()
	{
		shardPool_0 = GClass870.FindUnityObjectOfType(typeof(ShardPool)) as ShardPool;
		if (!(shardPool_0 != null))
		{
			shardPool_0 = new GameObject("_Shard Pool").AddComponent<ShardPool>();
		}
	}

	public void PopulatePool()
	{
		if (IsPopulated && shards.Length == maxPoolSize)
		{
			return;
		}
		if (shards != null)
		{
			Shard[] array = shards;
			for (int i = 0; i < array.Length; i++)
			{
				Object.DestroyImmediate(array[i].gameObject);
			}
		}
		shards = new Shard[maxPoolSize];
		for (int j = 0; j < maxPoolSize; j++)
		{
			shards[j] = new GameObject("Shard").AddComponent<Shard>();
			shards[j].transform.parent = base.transform;
		}
	}
}
