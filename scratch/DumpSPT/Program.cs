using System;
using System.Reflection;
using System.Linq;
class Program {
    static void Main() {
        var a = Assembly.LoadFrom(@"d:\Projetos\GITHUB TARKOV\tarkov-spt-4.0\mods\TarkovRedLine4.0\Server\TarkovRedLine.Server\bin\Release\SPTarkov.Server.Core.dll");
        var t = a.GetType("SPTarkov.Server.Core.Models.Eft.Profile.Info");
        foreach(var p in t.GetProperties()) Console.WriteLine(p.Name);
    }
}
