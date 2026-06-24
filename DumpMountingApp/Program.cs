using System;
using System.Reflection;
using System.Linq;

namespace DumpMounting
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var assembly = Assembly.LoadFrom(@"D:\Projetos\GITHUB TARKOV\tarkov-spt-4.0\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll");
                var types = assembly.GetTypes();
                
                var pwaType = types.FirstOrDefault(t => t.Name.Contains("ProceduralWeaponAnimation"));
                if (pwaType != null)
                {
                    Console.WriteLine($"Found: {pwaType.FullName}");
                    foreach (var method in pwaType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(m => m.Name.Contains("Mount") || m.Name.Contains("mount")))
                    {
                        Console.WriteLine($"Method: {method.Name} ({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
                    }

                    // Look for nested classes or fields related to mounting
                    foreach (var field in pwaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(f => f.Name.Contains("Mount") || f.FieldType.Name.Contains("Mount")))
                    {
                        Console.WriteLine($"Field: {field.FieldType.Name} {field.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("ProceduralWeaponAnimation not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
