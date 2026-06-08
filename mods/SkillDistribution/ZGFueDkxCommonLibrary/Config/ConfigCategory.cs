using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGFueDkx.ZGCLib.Config
{
    internal class ConfigCategory(ConfigFile config, int order, string name)
    {
        private readonly int _originalOrder = order;
        private int _order = order * 1000;
        private readonly string _name = name;

        private readonly List<ConfigEntryBase> configs = [];

        public ConfigFile Config { get; } = config;
        public string Name => $"{_originalOrder}. {_name}";
        public int GetNextOrder() => _order--;

        public ConfigEntry<T> BindConfig<T>(
            string key,
            T defaultValue,
            string description,
            bool hide = false,
            AcceptableValueBase? acceptableValue = null
        )
        {
            ConfigEntry<T> entry = Config.Bind(
                Name,
                key,
                defaultValue,
                new ConfigDescription(
                    description,
                    acceptableValue,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = hide,
                        Order = GetNextOrder(),
                    }
                )
            );

            configs.Add(entry);

            return entry;
        }

        public void BindButton(string key, string text, string description, Action action, bool hide = false)
        {
            ConfigEntry<string> entry = Config.Bind(
                Name,
                key,
                "",
                new ConfigDescription(
                    description,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = entry =>
                        {
                            if (GUILayout.Button(text, GUILayout.ExpandWidth(true)))
                            {
                                action();
                            }
                        },
                        IsAdvanced = hide,
                        Order = GetNextOrder(),
                    }
                )
            );

            configs.Add(entry);
        }

        public void SetReadOnly(bool readOnly, Func<ConfigEntryBase, bool>? predicate = null)
        {
            foreach (ConfigEntryBase entry in configs)
            {
                if (predicate is not null && !predicate(entry))
                {
                    continue;
                }

                entry.SetReadOnly(readOnly);
            }
        }

        public void SetAdvanced(bool advanced, Func<ConfigEntryBase, bool>? predicate = null)
        {
            foreach (ConfigEntryBase entry in configs)
            {
                if (predicate is not null && !predicate(entry))
                {
                    continue;
                }

                entry.SetAdvanced(advanced);
            }
        }
    }
}
