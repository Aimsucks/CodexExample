using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Newtonsoft.Json;
using CodexExample.Helpers;

namespace CodexExample;

[Serializable]
public class Configuration : IPluginConfiguration
{
    /*
     * Configuration Presets
     * These are presets that cover the plugin's settings itself. Updating these will immediately update how the
     * plugin functions.
     */
    
    public int Version { get; set; }
    public bool SettingOne { get; set; } = true;
    public int SettingTwo { get; set; } = 30;
    
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<Preset> Presets { get; set; } = [
    new()
    {
        Name = "Mod. Preset",
        StringData = "String Data 3",
        IntData = 30,
        Metadata = new PresetMetadata
        {
            Id = 4,
            Version = 3
        }
    }];
    
    /*
     * Helper Functions
     * ImportConfiguration brings in a preset from the Codex API that overwrites raw plugin configuration values.
     * Plugins might use this to allow users to share visual or functional configurations between each other.
     *
     * ImportPreset brings in a preset from the Codex API that adds or updates individual items in a "Presets" list in
     * the plugin configuration. This is especially useful if the plugin's functionality can be expanded by importing
     * presets others have created, i.e. waymarks or list filters. These are generally referred to as "Modules" in
     * Codex.
     */
    
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public void Reset()
    {
        Plugin.CodexExample.Configuration = new Configuration();
        Plugin.CodexExample.Configuration.Save();
    }

    internal bool ImportConfiguration(CodexPreset preset)
    {
        Plugin.PluginLog.Debug($"Importing configuration preset \"{preset.Name}\" (v{preset.Version})");
        
        var previousConfig = Plugin.CodexExample.Configuration;
        var updatedConfig = JsonConvert.DeserializeObject<Configuration>(preset.Data);
        
        if (updatedConfig == null) return false;
        
        /*
         * Setting the current configuration to the updated configuration will overwrite the *entire* configuration,
         * so you need to bring anything that doesn't get exported with an entire plugin configuration. In this case,
         * presets and the plugin version number need to be set separately.
         */
        
        updatedConfig.Version = previousConfig.Version;
        updatedConfig.Presets = previousConfig.Presets;
        
        Plugin.CodexExample.Configuration = updatedConfig;
        Plugin.CodexExample.Configuration.Save();

        return true;
    }

    internal bool ImportPreset(CodexPreset preset)
    {
        Plugin.PluginLog.Debug($"Importing plugin preset \"{preset.Name}\" (v{preset.Version})");
        
        var newPreset = JsonConvert.DeserializeObject<Preset>(preset.Data);

        if (newPreset == null) return false;
        
        /*
         * Preset metadata needs to be added before import since it's stripped from the preset when uploading to
         * the Codex API. The information is already stored in the API outside the Data parameter. Note that the
         * "name" parameter is inside the preset data. This is intentional - it allows you to have a different
         * "internal" name that's displayed when the preset is imported from what's visible in the API itself when
         * searching for presets.
         */
        
        newPreset.Metadata = new PresetMetadata
        {
            Id = preset.Id,
            Version = preset.Version
        };
        
        /*
         * Searching the existing presets for the current one and checking the versions will allow us to let the
         * user know it's going to be updated instead of imported or prevent the user from importing an existing
         * preset.
         */
        
        Presets.Add(newPreset);
        Save();

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
 * This preset class is included as an example of a class that another plugin might define. Its data is meaningless
 * outside the Name and Metadata parameters.
 */

public class Preset
{
    public PresetMetadata? Metadata { get; set; }
    public required string Name { get; set; }
    public string? StringData { get; set; }
    public int? IntData { get; set; }
}
