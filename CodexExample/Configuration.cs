using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace CodexExample;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool SettingOne { get; set; } = true;
    public int SettingTwo { get; set; } = 30;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
