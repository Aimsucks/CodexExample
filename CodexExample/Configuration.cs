using System;
using System.Collections.Generic;
using System.Text.Json;
using Dalamud.Configuration;

namespace CodexExample;

/*

 */

[Serializable]
public class Configuration : IPluginConfiguration
{
    /*
     * Configuration Presets
     * These are presets that cover the plugin's settings itself. Updating these will immediately update how the
     * plugin functions.
     */
    
    public int Version { get; set; } = 0;
    public bool SettingOne { get; set; } = true;
    public int SettingTwo { get; set; } = 30;
    public List<Preset> Presets { get; set; } = [];
    
    /*
     * Helper Functions
     * Some text will go here eventually.
     */
    
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public bool ImportConfiguration(string data)
    {
        var previousConfig = Plugin.CodexExample.Configuration;
        var updatedConfig = JsonSerializer.Deserialize<Configuration>(data);
        
        /*
         * Setting the current configuration to the updated configuration will overwrite the *entire* configuration,
         * so you need to bring anything that doesn't get exported with an entire plugin configuration. In this case,
         * presets and the plugin version number need to be set separately.
         */
        
        updatedConfig!.Version = previousConfig.Version;
        updatedConfig.Presets = previousConfig.Presets;
        
        Plugin.CodexExample.Configuration = updatedConfig;
        previousConfig.Save();

        return true;
    }

    public bool ImportPreset(string data)
    {
        var newPreset = JsonSerializer.Deserialize<Preset>(data);
        
        /*
         * Searching the existing presets for the current one and checking the versions will allow us to let the
         * user know it's going to be updated instead of imported or prevent the user from importing an existing
         * preset.
         */
        
        Presets.Add(newPreset);

        return true;
    }
}

/*
 * Module Presets
 * The word "module" refers to anything that isn't baked into the actual functionality of the plugin and will add
 * functionality to it by including it as a preset. A good example of this would be filters in Allagan Tools. The
 * configurations that are imported are supplementary to the plugin's configuration.
 *
 * In these presets, you will need to add a "metadata" object with the following parameters:
 * - An "id" integer to identify the preset in the API
 * - A "version" integer to provide updates to the preset when comparing it to the API version of that preset
 *
 * Anything pre-existing is fine and will be contained in the "data" string attribute of the preset coming from
 * Codex's API.
 */

public class PresetMetadata
{
    public required int Id { get; set; }
    public required int Version { get; set; }
}


/*
 * This preset class is included as an example of a class that another plugin might define. Its data is meaningless.
 */

public class Preset
{
    public required PresetMetadata Metadata { get; set; }
    public required string Name { get; set; }
    public string StringData { get; set; }
    public int IntData { get; set; }
}
