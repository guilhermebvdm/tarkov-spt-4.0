using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public abstract class GClass953
{
	[Serializable]
	[CompilerGenerated]
	public class Class602
	{
		public static readonly Class602 class602_0 = new Class602();

		public static Func<string, KeyValuePair<string, string>, string> func_0;

		public string method_0(string current, KeyValuePair<string, string> key)
		{
			return current.Replace(key.Key, key.Value);
		}
	}

	[NonSerialized]
	public static Dictionary<string, string> Dictionary_0;

	public static string Transliterate(this string text, string locale)
	{
		if (!(locale == "ru"))
		{
			return Dictionary_0.Aggregate(text, (string current, KeyValuePair<string, string> key) => current.Replace(key.Key, key.Value));
		}
		return text;
	}

	public static string Transliterate(this string text)
	{
		return Transliterate(text, LocaleManagerClass.LocaleManagerClass.String_0);
	}

	static GClass953()
	{
		Dictionary_0 = new Dictionary<string, string>();
		Dictionary_0.Add("Є", "Ye");
		Dictionary_0.Add("І", "I");
		Dictionary_0.Add("Ѓ", "G");
		Dictionary_0.Add("і", "i");
		Dictionary_0.Add("№", "#");
		Dictionary_0.Add("є", "ye");
		Dictionary_0.Add("ѓ", "g");
		Dictionary_0.Add("А", "A");
		Dictionary_0.Add("Б", "B");
		Dictionary_0.Add("В", "V");
		Dictionary_0.Add("Г", "G");
		Dictionary_0.Add("Д", "D");
		Dictionary_0.Add("Е", "E");
		Dictionary_0.Add("Ё", "Yo");
		Dictionary_0.Add("Ж", "Zh");
		Dictionary_0.Add("З", "Z");
		Dictionary_0.Add("И", "I");
		Dictionary_0.Add("Й", "Y");
		Dictionary_0.Add("К", "K");
		Dictionary_0.Add("Л", "L");
		Dictionary_0.Add("М", "M");
		Dictionary_0.Add("Н", "N");
		Dictionary_0.Add("О", "O");
		Dictionary_0.Add("П", "P");
		Dictionary_0.Add("Р", "R");
		Dictionary_0.Add("С", "S");
		Dictionary_0.Add("Т", "T");
		Dictionary_0.Add("У", "U");
		Dictionary_0.Add("Ф", "F");
		Dictionary_0.Add("Х", "H");
		Dictionary_0.Add("Ц", "C");
		Dictionary_0.Add("Ч", "Ch");
		Dictionary_0.Add("Ш", "Sh");
		Dictionary_0.Add("Щ", "Sch");
		Dictionary_0.Add("Ъ", "'");
		Dictionary_0.Add("Ы", "Y");
		Dictionary_0.Add("Ь", "");
		Dictionary_0.Add("Э", "E");
		Dictionary_0.Add("Ю", "Yu");
		Dictionary_0.Add("Я", "Ya");
		Dictionary_0.Add("а", "a");
		Dictionary_0.Add("б", "b");
		Dictionary_0.Add("в", "v");
		Dictionary_0.Add("г", "g");
		Dictionary_0.Add("д", "d");
		Dictionary_0.Add("е", "e");
		Dictionary_0.Add("ё", "yo");
		Dictionary_0.Add("ж", "zh");
		Dictionary_0.Add("з", "z");
		Dictionary_0.Add("и", "i");
		Dictionary_0.Add("й", "y");
		Dictionary_0.Add("к", "k");
		Dictionary_0.Add("л", "l");
		Dictionary_0.Add("м", "m");
		Dictionary_0.Add("н", "n");
		Dictionary_0.Add("о", "o");
		Dictionary_0.Add("п", "p");
		Dictionary_0.Add("р", "r");
		Dictionary_0.Add("с", "s");
		Dictionary_0.Add("т", "t");
		Dictionary_0.Add("у", "u");
		Dictionary_0.Add("ф", "f");
		Dictionary_0.Add("х", "h");
		Dictionary_0.Add("ц", "c");
		Dictionary_0.Add("ч", "ch");
		Dictionary_0.Add("ш", "sh");
		Dictionary_0.Add("щ", "sch");
		Dictionary_0.Add("ъ", "");
		Dictionary_0.Add("ы", "y");
		Dictionary_0.Add("ь", "");
		Dictionary_0.Add("э", "e");
		Dictionary_0.Add("ю", "yu");
		Dictionary_0.Add("я", "ya");
		Dictionary_0.Add("«", "");
		Dictionary_0.Add("»", "");
		Dictionary_0.Add("—", "-");
	}
}
