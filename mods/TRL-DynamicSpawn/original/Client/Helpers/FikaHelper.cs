using System;
using System.Linq;
using UnityEngine;

namespace TRLDynamicSpawn.Helpers
{
    public static class FikaHelper
    {
        private static bool _reflectionEvaluated = false;
        private static System.Reflection.PropertyInfo _isClientProp;

        private static void EnsureReflection()
        {
            if (_reflectionEvaluated) return;
            _reflectionEvaluated = true;

            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Fika.Core");

                if (assembly != null)
                {
                    var type = assembly.GetType("Fika.Core.Main.Utils.FikaBackendUtils");
                    if (type != null)
                    {
                        _isClientProp = type.GetProperty("IsClient", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Error initializing Fika reflection helper: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna true se a partida for FIKA Coop e esta máquina for um CLIENTE CONVIDADO (guest peer).
        /// Em modo Headless, Host (servidor local) ou Solo sem FIKA, retorna false.
        /// </summary>
        public static bool IsClient()
        {
            if (Application.isBatchMode)
            {
                return false;
            }

            EnsureReflection();

            if (_isClientProp != null)
            {
                try
                {
                    return (bool)_isClientProp.GetValue(null);
                }
                catch { }
            }

            return false;
        }

        /// <summary>
        /// Retorna true se esta máquina for o HOST/Servidor da partida (Host local, Solo ou Headless Dedicated Server).
        /// Retorna false se for um cliente convidado no FIKA.
        /// </summary>
        public static bool IsHostOrSolo()
        {
            return !IsClient();
        }
    }
}
