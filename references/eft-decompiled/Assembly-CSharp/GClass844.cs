using System;
using System.Collections;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass844
{
	public BotOwner Bo;

	[NonSerialized]
	public List<Stack<IEnumerator>> List_0 = new List<Stack<IEnumerator>>();

	public void Update()
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			if (num2 < List_0.Count)
			{
				Stack<IEnumerator> stack = List_0[num2];
				method_0(stack);
				if (stack.Count == 0)
				{
					List_0.RemoveAt(num2);
					num2--;
				}
				num++;
				if (!((float)num <= 10000f))
				{
					break;
				}
				num2++;
				continue;
			}
			return;
		}
		Debug.LogError("More ");
	}

	public void method_0(Stack<IEnumerator> coroutineStack)
	{
		IEnumerator enumerator = coroutineStack.Peek();
		bool flag = enumerator.MoveNext();
		int num = 0;
		while (true)
		{
			if (enumerator.Current is IEnumerator)
			{
				num++;
				if (!((float)num <= 10000f))
				{
					break;
				}
				coroutineStack.Push((IEnumerator)enumerator.Current);
				enumerator = (IEnumerator)enumerator.Current;
				flag = enumerator.MoveNext();
				continue;
			}
			if (!flag)
			{
				coroutineStack.Pop();
			}
			return;
		}
		Debug.LogError("More ");
	}

	public void StartCoroutine(IEnumerator coroutine)
	{
		Stack<IEnumerator> stack = new Stack<IEnumerator>();
		stack.Push(coroutine);
		List_0.Add(stack);
		method_0(stack);
	}
}
