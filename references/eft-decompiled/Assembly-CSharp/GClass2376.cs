using UnityEngine;

public abstract class GClass2376
{
	public static string GetKeyNameAlias(KeyCode keyCode)
	{
		string text = keyCode.ToString();
		switch (keyCode)
		{
		case KeyCode.Escape:
			text = "Esc";
			break;
		case KeyCode.DoubleQuote:
			text = "\"";
			break;
		case KeyCode.Ampersand:
			text = "&";
			break;
		case KeyCode.LeftParen:
			text = "(";
			break;
		case KeyCode.RightParen:
			text = ")";
			break;
		case KeyCode.Asterisk:
			text = "*";
			break;
		case KeyCode.Plus:
			text = "+";
			break;
		case KeyCode.Comma:
			text = ",";
			break;
		case KeyCode.Minus:
			text = "-";
			break;
		case KeyCode.Slash:
			text = "/";
			break;
		case KeyCode.Alpha0:
		case KeyCode.Alpha1:
		case KeyCode.Alpha2:
		case KeyCode.Alpha3:
		case KeyCode.Alpha4:
		case KeyCode.Alpha5:
		case KeyCode.Alpha6:
		case KeyCode.Alpha7:
		case KeyCode.Alpha8:
		case KeyCode.Alpha9:
			text = text.Replace("Alpha", string.Empty);
			break;
		case KeyCode.Semicolon:
			text = ";";
			break;
		case KeyCode.Less:
			text = "<";
			break;
		case KeyCode.Greater:
			text = ">";
			break;
		case KeyCode.Backspace:
			text = "BSpace";
			break;
		case KeyCode.None:
			text = "";
			break;
		case KeyCode.Keypad0:
		case KeyCode.Keypad1:
		case KeyCode.Keypad2:
		case KeyCode.Keypad3:
		case KeyCode.Keypad4:
		case KeyCode.Keypad5:
		case KeyCode.Keypad6:
		case KeyCode.Keypad7:
		case KeyCode.Keypad8:
		case KeyCode.Keypad9:
			text = text.Replace("Keypad", "Kp ");
			break;
		case KeyCode.KeypadDivide:
			text = "/";
			break;
		case KeyCode.KeypadMultiply:
			text = "Num *";
			break;
		case KeyCode.KeypadMinus:
			text = "Num -";
			break;
		case KeyCode.KeypadPlus:
			text = "Num +";
			break;
		case KeyCode.KeypadEnter:
			text = "⏎";
			break;
		case KeyCode.KeypadEquals:
			text = "=";
			break;
		case KeyCode.UpArrow:
			text = "↑";
			break;
		case KeyCode.DownArrow:
			text = "↓";
			break;
		case KeyCode.RightArrow:
			text = "→";
			break;
		case KeyCode.LeftArrow:
			text = "←";
			break;
		case KeyCode.Insert:
			text = "Ins";
			break;
		case KeyCode.PageUp:
			text = "PgUp";
			break;
		case KeyCode.PageDown:
			text = "PgDn";
			break;
		case KeyCode.Numlock:
			text = "NLock";
			break;
		case KeyCode.CapsLock:
			text = "Caps";
			break;
		case KeyCode.ScrollLock:
			text = "SLock";
			break;
		case KeyCode.RightShift:
			text = "RShift";
			break;
		case KeyCode.LeftShift:
			text = "LShift";
			break;
		case KeyCode.RightControl:
			text = "RCtrl";
			break;
		case KeyCode.LeftControl:
			text = "LCtrl";
			break;
		case KeyCode.RightAlt:
			text = "RAlt";
			break;
		case KeyCode.LeftAlt:
			text = "LAlt";
			break;
		case KeyCode.LeftMeta:
		case KeyCode.LeftWindows:
			text = "LWin";
			break;
		case KeyCode.RightMeta:
		case KeyCode.RightWindows:
			text = "RWin";
			break;
		case KeyCode.SysReq:
			text = "PrtScn";
			break;
		case KeyCode.Mouse0:
		case KeyCode.Mouse1:
		case KeyCode.Mouse2:
			text = GClass2348.Localized(text);
			break;
		case KeyCode.JoystickButton0:
		case KeyCode.JoystickButton1:
		case KeyCode.JoystickButton2:
		case KeyCode.JoystickButton3:
		case KeyCode.JoystickButton4:
		case KeyCode.JoystickButton5:
		case KeyCode.JoystickButton6:
		case KeyCode.JoystickButton7:
		case KeyCode.JoystickButton8:
		case KeyCode.JoystickButton9:
		case KeyCode.JoystickButton10:
		case KeyCode.JoystickButton11:
		case KeyCode.JoystickButton12:
		case KeyCode.JoystickButton13:
		case KeyCode.JoystickButton14:
		case KeyCode.JoystickButton15:
		case KeyCode.JoystickButton16:
		case KeyCode.JoystickButton17:
		case KeyCode.JoystickButton18:
		case KeyCode.JoystickButton19:
			text = text.Replace("JoystickButton", "JB ");
			break;
		case KeyCode.Delete:
			text = "Del";
			break;
		case KeyCode.LeftBracket:
			text = "[";
			break;
		case KeyCode.Backslash:
			text = "\\";
			break;
		case KeyCode.RightBracket:
			text = "]";
			break;
		case KeyCode.BackQuote:
			text = "`";
			break;
		}
		return text;
	}
}
