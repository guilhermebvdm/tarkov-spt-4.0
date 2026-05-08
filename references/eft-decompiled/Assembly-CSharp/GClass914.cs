using System;
using System.Runtime.InteropServices;

public class GClass914
{
	[DllImport("hwecho", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
	public static extern void HWEcho_Json(ref IntPtr output, ref IntPtr errors);

	public static (string, string) GetMetrics()
	{
		IntPtr output = IntPtr.Zero;
		IntPtr errors = IntPtr.Zero;
		string item = string.Empty;
		string item2 = string.Empty;
		HWEcho_Json(ref output, ref errors);
		if (output != IntPtr.Zero)
		{
			item = Marshal.PtrToStringBSTR(output);
			Marshal.FreeBSTR(output);
		}
		if (errors != IntPtr.Zero)
		{
			item2 = Marshal.PtrToStringBSTR(errors);
			Marshal.FreeBSTR(errors);
		}
		return (item, item2);
	}
}
