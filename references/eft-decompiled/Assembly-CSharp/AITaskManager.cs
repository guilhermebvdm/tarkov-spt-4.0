using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class AITaskManager
{
	public class GClass607
	{
		public int Id;

		public Action Task;

		public float Delay;

		public float StartTime;

		public bool IsCancelRequested;

		public BotOwner Bot;

		public void Init(int id, Action task, float delay, float startTime, BotOwner bot)
		{
			Bot = bot;
			Id = id;
			Task = task;
			Delay = delay;
			StartTime = startTime;
		}

		public void Dispose()
		{
			Id = -1;
			Task = null;
			StartTime = 0f;
			Delay = 0f;
			IsCancelRequested = false;
		}
	}

	public class Class284
	{
		public class Class285
		{
			public int Id;

			public GInterface13 Updatable;

			public float LastUpdateTime;

			public bool RemoveRequested;
		}

		public readonly float DefaultUpdatePeriod;

		public readonly int AverageTaskCount;

		public readonly float MaxUpdatePeriod;

		public readonly List<Class285> Datas;

		public readonly Dictionary<int, Class285> DataById;

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public int Int_1;

		public Class284(float defaultUpdPeriod, int avgTaskCount, float maxUpdPeriod)
		{
			DefaultUpdatePeriod = defaultUpdPeriod;
			AverageTaskCount = avgTaskCount;
			MaxUpdatePeriod = maxUpdPeriod;
			Datas = new List<Class285>(16);
			DataById = new Dictionary<int, Class285>(16);
		}

		public int AddNewTask(GInterface13 updatable)
		{
			int int_ = Int_0;
			Int_0++;
			Class285 @class = new Class285
			{
				Id = int_,
				Updatable = updatable,
				LastUpdateTime = -1f
			};
			Datas.Add(@class);
			DataById[int_] = @class;
			return int_;
		}

		public void RemoveTask(int id)
		{
			DataById[id].RemoveRequested = true;
		}

		public void UpdateGroup()
		{
			if (Datas.Count == 0)
			{
				return;
			}
			float num = ((Datas.Count > AverageTaskCount) ? ((float)Datas.Count / (float)AverageTaskCount) : 1f);
			float num2 = DefaultUpdatePeriod * num;
			if (num2 > MaxUpdatePeriod)
			{
				num2 = MaxUpdatePeriod;
			}
			float num3 = num2 / Time.deltaTime;
			int num4 = Mathf.CeilToInt((float)Datas.Count / num3);
			int num5 = Int_1 + 1;
			if (num5 >= Datas.Count)
			{
				num5 = 0;
			}
			for (int i = 0; i < Datas.Count; i++)
			{
				if (num4 <= 0)
				{
					break;
				}
				Class285 @class = Datas[num5];
				if (@class.RemoveRequested)
				{
					Datas.RemoveAt(num5);
					DataById.Remove(@class.Id);
					num5--;
				}
				else
				{
					float num6 = Time.time - @class.LastUpdateTime;
					if (num6 >= num2)
					{
						num4--;
						@class.Updatable.AIPeriodicUpdate(num6);
						@class.LastUpdateTime = Time.time;
					}
				}
				Int_1 = num5;
				num5++;
				if (num5 >= Datas.Count)
				{
					num5 = 0;
				}
			}
		}

		public void Dispose()
		{
			Datas.Clear();
			DataById.Clear();
		}
	}

	[NonSerialized]
	public List<GClass607> SimpleTasks;

	[NonSerialized]
	public Dictionary<int, GClass607> SimpleTaskById;

	[NonSerialized]
	public int SimpleTasksIdCounter;

	[NonSerialized]
	public Dictionary<EAITaskGroupType, Class284> RegularTasks;

	[NonSerialized]
	public Dictionary<GInterface13, (EAITaskGroupType, int)> DisposeInfoPerUpdatable;

	[NonSerialized]
	public Queue<GClass607> SimpleTaskDataPool;

	public AITaskManager()
	{
		SimpleTasks = new List<GClass607>(32);
		SimpleTaskById = new Dictionary<int, GClass607>(32);
		SimpleTaskDataPool = new Queue<GClass607>(32);
		RegularTasks = new Dictionary<EAITaskGroupType, Class284> { 
		{
			EAITaskGroupType.LookSensor,
			new Class284(0.1f, 10, 0.6f)
		} };
		DisposeInfoPerUpdatable = new Dictionary<GInterface13, (EAITaskGroupType, int)>(16);
	}

	public void Update()
	{
		method_0();
		method_1();
	}

	public int RegisterDelayedTask(float delay, Action task)
	{
		GClass607 gClass = method_2(task, delay, null);
		SimpleTasks.Add(gClass);
		SimpleTaskById[gClass.Id] = gClass;
		return gClass.Id;
	}

	public int RegisterDelayedTask(BotOwner bot, float delay, Action task)
	{
		GClass607 gClass = method_2(task, delay, bot);
		SimpleTasks.Add(gClass);
		SimpleTaskById[gClass.Id] = gClass;
		return gClass.Id;
	}

	public void CancelDelayedTask(int taskId)
	{
		if (SimpleTaskById.TryGetValue(taskId, out var value))
		{
			value.IsCancelRequested = true;
		}
	}

	public void RegisterRegularTask(EAITaskGroupType group, GInterface13 task)
	{
		int item = RegularTasks[group].AddNewTask(task);
		DisposeInfoPerUpdatable[task] = (group, item);
	}

	public void UnregisterRegularTask(GInterface13 regularTask)
	{
		if (DisposeInfoPerUpdatable.TryGetValue(regularTask, out var value))
		{
			RegularTasks[value.Item1].RemoveTask(value.Item2);
			DisposeInfoPerUpdatable.Remove(regularTask);
		}
	}

	public void Dispose()
	{
		foreach (KeyValuePair<EAITaskGroupType, Class284> regularTask in RegularTasks)
		{
			regularTask.Value.Dispose();
		}
		RegularTasks.Clear();
		DisposeInfoPerUpdatable.Clear();
		foreach (GClass607 simpleTask in SimpleTasks)
		{
			simpleTask.Dispose();
		}
		SimpleTasks.Clear();
		SimpleTaskDataPool.Clear();
		SimpleTaskById.Clear();
	}

	public void method_0()
	{
		for (int i = 0; i < SimpleTasks.Count; i++)
		{
			GClass607 gClass = SimpleTasks[i];
			if (gClass.IsCancelRequested)
			{
				SimpleTasks.RemoveAt(i);
				SimpleTaskById.Remove(gClass.Id);
				i--;
				method_3(gClass);
			}
			else
			{
				if (!(Time.time - gClass.StartTime >= gClass.Delay))
				{
					continue;
				}
				bool flag = true;
				try
				{
					if (gClass.Bot == null || gClass.Bot.BotState == EBotState.Active)
					{
						gClass.Task();
					}
				}
				catch (Exception e)
				{
					GClass398.Instance.LogException(e);
					flag = false;
				}
				finally
				{
					SimpleTasks.RemoveAt(i);
					SimpleTaskById.Remove(gClass.Id);
					i--;
					if (flag)
					{
						method_3(gClass);
					}
				}
			}
		}
	}

	public void method_1()
	{
		foreach (KeyValuePair<EAITaskGroupType, Class284> regularTask in RegularTasks)
		{
			regularTask.Value.UpdateGroup();
		}
	}

	public GClass607 method_2(Action task, float delay, BotOwner bot)
	{
		GClass607 gClass = ((SimpleTaskDataPool.Count <= 0) ? new GClass607() : SimpleTaskDataPool.Dequeue());
		gClass.Init(SimpleTasksIdCounter++, task, delay, Time.time, bot);
		return gClass;
	}

	public void method_3(GClass607 data)
	{
		data.Dispose();
		SimpleTaskDataPool.Enqueue(data);
	}
}
