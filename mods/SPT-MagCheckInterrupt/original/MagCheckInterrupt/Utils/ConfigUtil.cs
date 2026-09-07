using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using EFT.Communications;
using UnityEngine;

namespace MagCheckInterrupt.Utils;

public static class ConfigUtil
{
    public static ConfigEntry<KeybindsUtil.EReloadMode> ReloadMode { get; private set; }
    public static ConfigEntry<float> ReloadWindowStart { get; private set; }
    public static ConfigEntry<float> ReloadWindowEnd { get; private set; }
    public static ConfigEntry<bool> SlowAnimation { get; private set; }
    public static ConfigEntry<float> SlowAnimationStart { get; private set; }
    public static ConfigEntry<float> SlowAnimationEnd { get; private set; }
    public static ConfigEntry<float> SlowPercentage { get; private set; }
    public static ConfigEntry<float> SlowSmoothing { get; private set; }

    private static readonly List<ConfigEntryBase> _allConfigs = [];
    private static ConfigFile _configFile;
    private static ConfigEntry<bool> _fikaHostConfig;
    private static GUIStyle _centeredStyle;

    public static void Init(ConfigFile configFile)
    {
        _configFile = configFile;

        _fikaHostConfig = _configFile.Bind(
            string.Empty,
            "FikaHostConfig",
            false,
            new ConfigDescription(
                "This is displayed when a Fika Client is using the Host's config",
                null,
                new ConfigurationManagerAttributes
                {
                    Order = 33,
                    Browsable = false,
                    HideDefaultButton = true,
                    HideSettingName = true,
                    CustomDrawer = (_) =>
                    {
                        _centeredStyle ??= new GUIStyle(GUI.skin.label)
                        {
                            richText = true,
                            wordWrap = true,
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18,
                        };

                        GUILayout.Label("<color=#4DA6FF><b>Configuration is set by the Fika Host</b></color>", _centeredStyle);
                    },
                }
            )
        );

        ReloadMode = _configFile.Bind(
            "General",
            "Reload Mode",
            KeybindsUtil.EReloadMode.Press,
            new ConfigDescription(
                "Special case for when reload and mag check keybinds are conflicting",
                null,
                new ConfigurationManagerAttributes { Order = 41 }
            )
        );
        ReloadWindowStart = _configFile.Bind(
            "General",
            "Reload Window Start",
            0.1f,
            new ConfigDescription(
                "How early you can reload during the magazine check animation, in normalized time",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { Order = 32, ShowRangeAsPercent = true, IsAdvanced = true }
            )
        );
        ReloadWindowEnd = _configFile.Bind(
            "General",
            "Reload Window End",
            0.6f,
            new ConfigDescription(
                "How late you can reload during the magazine check animation, in normalized time",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { Order = 31, ShowRangeAsPercent = true, IsAdvanced = true }
            )
        );

        SlowAnimation = _configFile.Bind(
            "Slow Animation",
            "Enable",
            true,
            new ConfigDescription(
                "Slow down the magazine check animation for a certain time",
                null,
                new ConfigurationManagerAttributes { Order = 15 }
            )
        );
        SlowPercentage = _configFile.Bind(
            "Slow Animation",
            "Slow Percentage",
            0.25f,
            new ConfigDescription(
                "Multiplier for the magazine check animation speed when Slow Animation is enabled",
                new AcceptableValueRange<float>(0.01f, 1f),
                new ConfigurationManagerAttributes { Order = 14, ShowRangeAsPercent = true, IsAdvanced = true }
            )
        );
        SlowAnimationStart = _configFile.Bind(
            "Slow Animation",
            "Start",
            0.3f,
            new ConfigDescription(
                "When to start slowing down the magazine check animation, in normalized time",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { Order = 13, ShowRangeAsPercent = true, IsAdvanced = true }
            )
        );
        SlowAnimationEnd = _configFile.Bind(
            "Slow Animation",
            "End",
            0.4f,
            new ConfigDescription(
                "When to restore speed of the magazine check animation, in normalized time",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { Order = 12, ShowRangeAsPercent = true, IsAdvanced = true }
            )
        );
        SlowSmoothing = _configFile.Bind(
            "Slow Animation",
            "Smoothing Max Delta",
            2f,
            new ConfigDescription(
                "Max delta for the smoothing of the slow animation. A higher value slows/restores the animation faster",
                new AcceptableValueRange<float>(0.01f, 10f),
                new ConfigurationManagerAttributes { Order = 11, IsAdvanced = true }
            )
        );

        _allConfigs.Add(ReloadWindowStart);
        _allConfigs.Add(ReloadWindowEnd);
        _allConfigs.Add(SlowAnimation);
        _allConfigs.Add(SlowAnimationStart);
        _allConfigs.Add(SlowAnimationEnd);
        _allConfigs.Add(SlowPercentage);
        _allConfigs.Add(SlowSmoothing);
    }

    public static void RegisterSettingsChanged(EventHandler<SettingChangedEventArgs> eventArgs)
    {
        _configFile.SettingChanged += eventArgs;
    }

    public static void SetReadOnly(bool readOnly)
    {
        foreach (var config in _allConfigs)
        {
            foreach (var tag in config.Description.Tags)
            {
                if (tag is not ConfigurationManagerAttributes attr) continue;

                attr.ReadOnly = readOnly;
                break;
            }
        }
    }

    public static void DisplayIsUsingFikaHostConfig(bool usingFikaHost)
    {
        foreach (var tag in _fikaHostConfig.Description.Tags)
        {
            if (tag is not ConfigurationManagerAttributes attr) continue;

            attr.Browsable = usingFikaHost;
            return;
        }
    }

    public static bool SetConfigValues(string[] values)
    {
        if (values.Length != _allConfigs.Count)
        {
            LoggerUtil.Error(
                $"ConfigUtil::SetConfigValues ArgumentOutOfRange {nameof(values)}. Arg: {values.Length} != {_allConfigs.Count}"
            );
            NotificationManagerClass.DisplayWarningNotification(
                "MagCheckInterrupt: Unable to set config values. Different mod version with the host?",
                ENotificationDurationType.Long
            );
            return false;
        }

        for (var i = 0; i < _allConfigs.Count; i++)
        {
            var config = _allConfigs[i];
            config.SetSerializedValue(values[i]);
        }

        return true;
    }

    public static string[] GetConfigValues(string[] preAllocated = null)
    {
        var configValues = preAllocated ?? new string[_allConfigs.Count];
        for (var i = 0; i < _allConfigs.Count; i++)
        {
            var config = _allConfigs[i];
            configValues[i] = config.GetSerializedValue();
        }

        return configValues;
    }
}
