using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Class1088 : Class1087
{
	public struct Struct272
	{
		[NonSerialized]
		[CompilerGenerated]
		public Class1086 Class1086_0;

		[NonSerialized]
		[CompilerGenerated]
		public Class1085 Class1085_0;

		public Class1086 Row
		{
			[CompilerGenerated]
			readonly get
			{
				return Class1086_0;
			}
			[CompilerGenerated]
			set
			{
				Class1086_0 = value;
			}
		}

		public Class1085 Column
		{
			[CompilerGenerated]
			readonly get
			{
				return Class1085_0;
			}
			[CompilerGenerated]
			set
			{
				Class1085_0 = value;
			}
		}

		public Struct272(Class1086 row, Class1085 column)
		{
			this = default(Struct272);
			Row = row;
			Column = column;
		}

		public bool IsIn(GInterface163 item)
		{
			if (Row != item && Column != item)
			{
				return false;
			}
			return true;
		}

		public bool IsInRow(Class1086 row)
		{
			return row == Row;
		}

		public bool IsInColumn(Class1085 column)
		{
			return column == Column;
		}

		public static bool operator ==(Struct272 a, Struct272 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Struct272 a, Struct272 b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Struct272 @struct)
			{
				if (@struct.Row == Row && @struct.Column == Column)
				{
					return true;
				}
				return false;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Column.GetHashCode() ^ Row.GetHashCode();
		}
	}

	[NonSerialized]
	public Dictionary<Struct272, double> Dictionary_0 = new Dictionary<Struct272, double>();

	[NonSerialized]
	public Dictionary<Class1084, int> Dictionary_1 = new Dictionary<Class1084, int>();

	[NonSerialized]
	public KeyValuePair<Struct272, double>? Nullable_0;

	[NonSerialized]
	public KeyValuePair<Struct272, double>? Nullable_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public double[,] Double_0;

	public Class1073 mColumns;

	public Class1074 mRows;

	public bool SuspendEvents
	{
		get
		{
			return Bool_1;
		}
		set
		{
			if (Bool_1 && !value && Bool_0)
			{
				method_0();
				Bool_0 = false;
			}
			Bool_1 = value;
		}
	}

	public override Class1073 Columns => mColumns;

	public override Class1074 Rows => mRows;

	public Class1088()
	{
		mColumns = new Class1073();
		mRows = new Class1074();
		mColumns.NameChanged += method_3;
		mRows.NameChanged += method_2;
		mColumns.OrderChanged += mRows_OrderChanged;
		mRows.OrderChanged += mRows_OrderChanged;
		mColumns.ItemRemoved += method_11;
		mRows.ItemRemoved += method_10;
	}

	public void method_2(string arg1, GInterface163 arg2)
	{
	}

	public void method_3(string arg1, GInterface163 arg2)
	{
	}

	public void mRows_OrderChanged(object sender, EventArgs e)
	{
		Double_0 = null;
		Dictionary_1.Clear();
		if (!Bool_1)
		{
			method_0();
		}
		else
		{
			Bool_0 = true;
		}
	}

	public void method_4()
	{
		Nullable_0 = null;
		Nullable_1 = null;
		KeyValuePair<Struct272, double>? keyValuePair = null;
		KeyValuePair<Struct272, double>? keyValuePair2 = null;
		foreach (KeyValuePair<Struct272, double> item in Dictionary_0)
		{
			if (!keyValuePair.HasValue || keyValuePair.Value.Value < item.Value)
			{
				keyValuePair = item;
			}
			if (!keyValuePair2.HasValue || keyValuePair2.Value.Value > item.Value)
			{
				keyValuePair2 = item;
			}
		}
		if (keyValuePair.HasValue)
		{
			Nullable_0 = keyValuePair.Value;
		}
		if (keyValuePair2.HasValue)
		{
			Nullable_1 = keyValuePair2.Value;
		}
	}

	public void Clear()
	{
		Columns.Clear();
		Rows.Clear();
	}

	public void method_5()
	{
		Dictionary_1.Clear();
		for (int i = 0; i < mColumns.Count; i++)
		{
			Class1085 key = mColumns[i];
			Dictionary_1.Add(key, i);
		}
		for (int j = 0; j < mRows.Count; j++)
		{
			Class1086 key2 = mRows[j];
			Dictionary_1.Add(key2, j);
		}
		Double_0 = new double[mRows.Count, mColumns.Count];
		foreach (KeyValuePair<Struct272, double> item in Dictionary_0)
		{
			if (Dictionary_1.TryGetValue(item.Key.Column, out var value) && Dictionary_1.TryGetValue(item.Key.Row, out var value2))
			{
				Double_0[value2, value] = item.Value;
			}
		}
		method_4();
	}

	public bool method_6()
	{
		int num = mRows.Count * mColumns.Count;
		if (Dictionary_0.Count < num)
		{
			return true;
		}
		return false;
	}

	public double? method_7()
	{
		method_12();
		if (!Nullable_0.HasValue)
		{
			return null;
		}
		if (!Dictionary_0.TryGetValue(Nullable_0.Value.Key, out var value))
		{
			return null;
		}
		if (method_6() && value < 0.0)
		{
			return 0.0;
		}
		return value;
	}

	public double? method_8()
	{
		method_12();
		if (!Nullable_1.HasValue)
		{
			return null;
		}
		if (!Dictionary_0.TryGetValue(Nullable_1.Value.Key, out var value))
		{
			return null;
		}
		if (method_6() && value > 0.0)
		{
			return 0.0;
		}
		return value;
	}

	public void method_9(GInterface163 item)
	{
		Dictionary<Struct272, double> dictionary = new Dictionary<Struct272, double>();
		foreach (KeyValuePair<Struct272, double> item2 in Dictionary_0)
		{
			if (!item2.Key.IsIn(item))
			{
				dictionary.Add(item2.Key, item2.Value);
			}
		}
		Dictionary_0 = dictionary;
		Double_0 = null;
		if (!Bool_1)
		{
			method_0();
		}
		else
		{
			Bool_0 = true;
		}
	}

	public void method_10(Class1086 obj)
	{
		method_9(obj);
	}

	public void method_11(Class1085 obj)
	{
		method_9(obj);
	}

	public void method_12()
	{
		if (Double_0 == null)
		{
			method_5();
		}
	}

	public override double[,] getRawData()
	{
		method_12();
		return Double_0;
	}

	public bool method_13(Struct272 element, double value)
	{
		bool flag = false;
		bool result = false;
		if (Nullable_0.HasValue && Nullable_0.Value.Value >= value)
		{
			if (Nullable_0.Value.Key == element)
			{
				flag = true;
			}
		}
		else
		{
			Nullable_0 = new KeyValuePair<Struct272, double>(element, value);
			result = true;
		}
		if (Nullable_1.HasValue && Nullable_1.Value.Value <= value)
		{
			if (Nullable_1.Value.Key == element)
			{
				flag = true;
			}
		}
		else
		{
			Nullable_1 = new KeyValuePair<Struct272, double>(element, value);
			result = true;
		}
		if (flag)
		{
			method_4();
			result = true;
		}
		return result;
	}

	public void method_14(Class1085 column, Class1086 row, double amount)
	{
		method_12();
		if (!Dictionary_1.TryGetValue(column, out var value))
		{
			throw new Exception0("value cannot be set");
		}
		if (!Dictionary_1.TryGetValue(row, out var value2))
		{
			throw new Exception0("value cannot be set");
		}
		Double_0[value2, value] = amount;
		Struct272 @struct = new Struct272(row, column);
		if (!Dictionary_0.TryGetValue(@struct, out var value3))
		{
			value3 = 0.0;
		}
		Dictionary_0[@struct] = amount;
		bool minMaxChanged = method_13(@struct, amount);
		if (!Bool_1)
		{
			method_1(new EventArgs0(value2, value, 0.0, amount, minMaxChanged));
		}
		else
		{
			Bool_0 = true;
		}
	}

	public double method_15(Class1085 column, Class1086 row)
	{
		Struct272 key = new Struct272(row, column);
		if (!Dictionary_0.TryGetValue(key, out var value))
		{
			return 0.0;
		}
		return value;
	}

	public double GetValue(string ColumnName, string RowName)
	{
		Class1085 column = mColumns[ColumnName];
		Class1086 row = mRows[RowName];
		return method_15(column, row);
	}

	public double GetValue(string ColumnName, int rowIndex)
	{
		Class1085 column = mColumns[ColumnName];
		Class1086 row = mRows[rowIndex];
		return method_15(column, row);
	}

	public double GetValue(int columnIndex, int rowIndex)
	{
		Class1085 column = mColumns[columnIndex];
		Class1086 row = mRows[rowIndex];
		return method_15(column, row);
	}

	public void AddLabel(string columnName, int rowIndex, string text)
	{
	}

	public void SetValue(string ColumnName, string RowName, double amount)
	{
		Class1085 column = mColumns[ColumnName];
		Class1086 row = mRows[RowName];
		method_14(column, row, amount);
	}

	public void SetValue(string ColumnName, int rowIndex, double amount)
	{
		Class1085 column = mColumns[ColumnName];
		Class1086 row = mRows[rowIndex];
		method_14(column, row, amount);
	}

	public void SetValue(int columnIndex, int rowIndex, double amount)
	{
		Class1085 column = mColumns[columnIndex];
		Class1086 row = mRows[rowIndex];
		method_14(column, row, amount);
	}
}
