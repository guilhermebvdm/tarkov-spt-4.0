using System;
using System.Runtime.InteropServices;

public class RNNoiseTest {
    [DllImport("rnnoise.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int rnnoise_get_frame_size();

    public static void Main() {
        try {
            int size = rnnoise_get_frame_size();
            Console.WriteLine("SUCCESS! Frame size is: " + size);
        } catch (Exception ex) {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
