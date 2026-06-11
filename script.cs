using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"E:\TORRENT\Escape from Tarkov 4.0\SPT\SPTarkov.Server.Core.dll");
        foreach(var t in asm.GetTypes().Where(x => x.IsInterface && x.Name.Contains("Mod")))
        {
            Console.WriteLine(t.FullName);
        }
    }
}
