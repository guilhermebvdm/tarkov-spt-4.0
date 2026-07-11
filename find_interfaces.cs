using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"d:\Projetos\GITHUB TARKOV\tarkov-spt-4.0\mods\TRL-ImmersiveCombatMedicine\client\References\Assembly-CSharp.dll");
        foreach(var t in asm.GetTypes().Where(t => t.IsInterface && t.Name.StartsWith("I"))) {
            if(t.Name.Contains("Bleed") || t.Name.Contains("Fracture")) {
                Console.WriteLine(t.FullName);
            }
        }
    }
}
