using System;
using System.Collections;
using System.Collections.Generic;

public class GClass1209<T> : IEnumerable<T>, IEnumerable
{
	public class Class824 : IEnumerator, IEnumerator<T>, IDisposable
	{
		[NonSerialized]
		public T[] Gparam_0;

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public int Int_1;

		object IEnumerator.Current => Gparam_0[Int_1];

		T IEnumerator<T>.Current => Gparam_0[Int_1];

		public Class824(T[] buffer, int validNum)
		{
			Gparam_0 = buffer;
			Int_0 = validNum;
			Int_1 = 0;
		}

		public void Dispose()
		{
		}

		void IDisposable.Dispose()
		{
			//ILSpy generated this explicit interface implementation from .override directive in Dispose
			this.Dispose();
		}

		public bool MoveNext()
		{
			Int_1++;
			return Int_1 < Int_0;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		public void Reset()
		{
			Int_1 = 0;
		}

		void IEnumerator.Reset()
		{
			//ILSpy generated this explicit interface implementation from .override directive in Reset
			this.Reset();
		}
	}

	public T[] buffer;

	public int numIndices;

	public int Length => numIndices;

	public T[] BufferCopy
	{
		get
		{
			T[] array = new T[numIndices];
			Array.Copy(buffer, array, numIndices);
			return array;
		}
	}

	public T this[int i]
	{
		get
		{
			return buffer[i];
		}
		set
		{
			buffer[i] = value;
		}
	}

	public GClass1209(int cap)
	{
		if (cap > 0)
		{
			buffer = new T[cap];
			numIndices = 0;
		}
	}

	public void Reset()
	{
		numIndices = 0;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new Class824(buffer, numIndices);
	}

	public IEnumerator GetEnumerator_1()
	{
		return new Class824(buffer, numIndices);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}
}
