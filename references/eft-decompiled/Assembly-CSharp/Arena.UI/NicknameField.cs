using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using EFT;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arena.UI;

public class NicknameField : MonoBehaviour
{
	public readonly struct Struct274(EValidationType type, string regex)
	{
		public readonly EValidationType Type = type;

		public readonly string Regex = regex;
	}

	[SerializeField]
	private Image[] _statusChangableImages;

	[SerializeField]
	private TMP_Text _statusLabel;

	[SerializeField]
	private Color _errorStatusColor = Color.red;

	[SerializeField]
	private Color _approvedStatusColor = Color.green;

	[Space]
	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private EValidationType _validationType = EValidationType.Everything;

	[SerializeField]
	private LocalizedText _usedSymbolsCount;

	[SerializeField]
	private int _minCharsCount = -1;

	[SerializeField]
	private int _maxDigitsCount = -1;

	[Space]
	[SerializeField]
	private string _enterNicknameLocalizationKey = "Enter player nickname";

	[SerializeField]
	private string _validNicknameLocalizationKey = "ENicknameError/ValidNickname";

	private Color[] color_0;

	private static readonly Struct274[] struct274_0 = new Struct274[21]
	{
		new Struct274(EValidationType.Numbers, "0-9"),
		new Struct274(EValidationType.Latin, "a-zA-Z"),
		new Struct274(EValidationType.AnyWordChars, "\\w"),
		new Struct274(EValidationType.Underscore, "_"),
		new Struct274(EValidationType.Period, "\\."),
		new Struct274(EValidationType.Comma, "\\,"),
		new Struct274(EValidationType.Space, "\\ "),
		new Struct274(EValidationType.Slash, "/\\\\"),
		new Struct274(EValidationType.Brackets, "\\(\\)\\[\\]\\{\\}\\<\\>"),
		new Struct274(EValidationType.Hyphen, "\\-"),
		new Struct274(EValidationType.Exclamation, "\\!"),
		new Struct274(EValidationType.Question, "\\?"),
		new Struct274(EValidationType.Quotes, "\\'\\\""),
		new Struct274(EValidationType.At, "\\@"),
		new Struct274(EValidationType.Colons, "\\;\\:"),
		new Struct274(EValidationType.Math, "\\+\\=\\*\\^\\%"),
		new Struct274(EValidationType.And, "\\&"),
		new Struct274(EValidationType.Dollar, "\\$"),
		new Struct274(EValidationType.Separator, "\\|"),
		new Struct274(EValidationType.NumSign, "\\#\\№"),
		new Struct274(EValidationType.NewLine, "\n")
	};

	private readonly Regex regex_0 = new Regex("\\d");

	private Regex regex_1;

	private Regex regex_2;

	private bool bool_0;

	private bool bool_1;

	private ENicknameError enicknameError_0 = ENicknameError.ValidNickname;

	[CompilerGenerated]
	private Action<string> action_0;

	[CompilerGenerated]
	private Action<string> action_1;

	public event Action<string> OnValueChanged
	{
		[CompilerGenerated]
		add
		{
			Action<string> action = action_0;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string> action = action_0;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<string> OnNicknameSubmited
	{
		[CompilerGenerated]
		add
		{
			Action<string> action = action_1;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string> action = action_1;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		_inputField.onValueChanged.AddListener(method_1);
		_inputField.onEndEdit.AddListener(method_2);
	}

	public void method_0()
	{
		if (color_0 == null)
		{
			color_0 = new Color[_statusChangableImages.Length];
			for (int i = 0; i < _statusChangableImages.Length; i++)
			{
				color_0[i] = _statusChangableImages[i].color;
			}
		}
	}

	public void Init(string reservedNickname)
	{
		if (regex_1 == null || regex_2 == null)
		{
			method_0();
			string text = smethod_0(_validationType);
			regex_1 = new Regex("^[" + text + "]+$");
			regex_2 = new Regex("[^" + text + "]");
			if (string.IsNullOrEmpty(_inputField.text) && !string.IsNullOrEmpty(reservedNickname))
			{
				_inputField.text = reservedNickname;
				method_3(_inputField.text);
			}
			method_4();
			_statusLabel.text = GClass2348.Localized(_enterNicknameLocalizationKey);
		}
	}

	public void UpdateStatusLabel()
	{
		method_6(enicknameError_0, bool_1);
	}

	public void NicknameForceSubmit()
	{
		method_2(_inputField.text);
	}

	public void method_1(string nickname)
	{
		action_0?.Invoke(nickname);
		method_3(nickname);
	}

	public void method_2(string nickname)
	{
		method_3(nickname);
		if (!bool_0)
		{
			action_1?.Invoke(nickname);
			_inputField.interactable = false;
		}
	}

	public void ValidationCallback(ENicknameError error)
	{
		method_6(error, isFromBackend: true);
		_inputField.interactable = true;
	}

	public void method_3(string nickname)
	{
		method_6(method_5(nickname));
		method_4();
	}

	public void method_4()
	{
		if (_usedSymbolsCount != null)
		{
			_usedSymbolsCount.SetFormatValues("<color=#C3C5BBFF>" + _inputField.text.Length + "/" + _inputField.characterLimit + "</color>");
		}
	}

	public ENicknameError method_5(string textValue)
	{
		if (textValue.Length > 0 && !regex_1.IsMatch(textValue))
		{
			_inputField.SetTextWithoutNotify(regex_2.Replace(_inputField.text, string.Empty));
			return ENicknameError.WrongSymbol;
		}
		if (_minCharsCount > 0 && textValue.Length < _minCharsCount)
		{
			return ENicknameError.TooShort;
		}
		if (textValue.Length > _inputField.characterLimit)
		{
			return ENicknameError.CharacterLimit;
		}
		if (_maxDigitsCount >= 0 && regex_0.Matches(textValue).Count > _maxDigitsCount)
		{
			return ENicknameError.DigitsLimit;
		}
		return ENicknameError.ValidNickname;
	}

	public void method_6(ENicknameError error, bool isFromBackend = false)
	{
		bool_1 = isFromBackend;
		enicknameError_0 = error;
		bool flag;
		string message = ((!(flag = error == ENicknameError.ValidNickname)) ? GClass2348.Localized(error, EStringCase.None) : string.Empty);
		bool_0 = !flag;
		method_7(message, !flag, isFromBackend);
	}

	public void method_7(string message, bool isError, bool isFromBackend = false)
	{
		if (!isFromBackend)
		{
			if (_statusLabel != null)
			{
				_statusLabel.text = ((!string.IsNullOrEmpty(message)) ? message : GClass2348.Localized(_enterNicknameLocalizationKey));
			}
			for (int i = 0; i < _statusChangableImages.Length; i++)
			{
				if (_statusChangableImages[i] != null)
				{
					_statusChangableImages[i].color = (isError ? _errorStatusColor : color_0[i]);
				}
			}
			return;
		}
		if (_statusLabel != null)
		{
			_statusLabel.text = (isError ? message : GClass2348.Localized(_validNicknameLocalizationKey));
		}
		for (int j = 0; j < _statusChangableImages.Length; j++)
		{
			if (_statusChangableImages[j] != null)
			{
				_statusChangableImages[j].color = (isError ? _errorStatusColor : _approvedStatusColor);
			}
		}
	}

	public static string smethod_0(EValidationType validationType)
	{
		string text = string.Empty;
		Struct274[] array = struct274_0;
		for (int i = 0; i < array.Length; i++)
		{
			Struct274 @struct = array[i];
			if (!string.IsNullOrEmpty(@struct.Regex) && GClass867.HasFlagNoBox(validationType, @struct.Type))
			{
				text += @struct.Regex;
			}
		}
		return text;
	}
}
