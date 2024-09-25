using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace CodexExample;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public static bool SettingOne { get; set; } = true;
    public static int SettingTwo { get; set; } = 30;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
