using System;
using System.Collections.Generic;
using CodexExample.Helpers;
using Dalamud.Configuration;
using Newtonsoft.Json;

namespace CodexExample;

[Serializable]
public class Configuration : IPluginConfiguration
{
    /*
     * This enum was created to send data to "react" to messages depending on if they were imported, updated, etc. This
     * data is then used to call StatusMessage.cs and draw a status message so the end user knows what just happened.
     */

    public enum PresetImportStatus
    {
        Success,
        Updated,
        AlreadyExists,
        Failure
    }

    public bool SettingOne { get; set; } = true;
    public int SettingTwo { get; set; } = 30;

    /*
     * A pre-existing preset was added so you can observe the update behavior. In the API, this preset already exists,
     * but its version is 2 instead of 1. When you check for updates, it will return as a preset that can be updated.
     */

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<Preset> Presets { get; set; } =
    [
        new()
        {
            Name = "Mod. Preset",
            StringData = "String Data 3",
            IntData = 30,
            Metadata = new PresetMetadata
            {
                Id = 6,
                Version = 1
            }
        }
    ];

    public int Version { get; set; }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public void Reset()
    {
        Plugin.CodexExample.Configuration = new Configuration();
        Plugin.CodexExample.Configuration.Save();
    }

    /*
     * ImportConfiguration brings in a preset from the Codex API that overwrites raw plugin configuration values.
     * Plugins might use this to allow users to share visual or functional configurations between each other.
     * These are called "configuration presets" in the example and are presets that cover the plugin's settings itself.
     * Updating these should immediately update how the plugin functions.
     */

    internal PresetImportStatus ImportConfiguration(CodexPreset preset)
    {
        Plugin.PluginLog.Debug($"Importing configuration preset \"{preset.Name}\" (v{preset.Version})");

        var previousConfig = Plugin.CodexExample.Configuration;
        var updatedConfig = JsonConvert.DeserializeObject<Configuration>(preset.Data);

        if (updatedConfig == null) return PresetImportStatus.Failure;

        /*
         * Setting the current configuration to the updated configuration will overwrite the *entire* configuration,
         * so you need to bring anything that doesn't get exported with an entire plugin configuration. In this case,
         * presets and the plugin version number need to be set separately.
         */

        updatedConfig.Version = previousConfig.Version;
        updatedConfig.Presets = previousConfig.Presets;

        Plugin.CodexExample.Configuration = updatedConfig;
        Plugin.CodexExample.Configuration.Save();

        return PresetImportStatus.Success;
    }

    /*
     * ImportPreset brings in a preset from the Codex API that adds or updates individual items in a "Presets" list in
     * the plugin configuration. This is especially useful if the plugin's functionality can be expanded by importing
     * presets others have created, i.e. waymarks or list filters. These are generally referred to as "modular presets"
     * or "plugin presets" in Codex.
     *
     * The word "modular" refers to anything that isn't baked into the actual functionality of the plugin and will add
     * functionality to it by including it as a preset. A good example of this would be filters in Allagan Tools. The
     * configurations that are imported are supplementary to the plugin's configuration itself.
     *
     * For these presets, you will need to add a "metadata" object with the following parameters:
     * - An "id" integer to identify the preset in the API
     * - A "version" integer to provide updates to the preset when comparing it to the API version of that preset
     *
     * This will be explained in further detail below where the PluginMetadata class is defined. Additionally, anything
     * pre-existing is fine and will be contained in the "data" string attribute of the preset coming from
     * Codex's API. You will have to bake functionality into your plugin to check if any presets contain the required
     * metadata and handle "old" presets appropriately.
     */

    internal PresetImportStatus ImportPreset(CodexPreset preset)
    {
        Plugin.PluginLog.Debug($"Importing plugin preset \"{preset.Name}\" (v{preset.Version})");

        var newPreset = JsonConvert.DeserializeObject<Preset>(preset.Data);

        if (newPreset == null) return PresetImportStatus.Failure;

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
         * user know it's going to be updated instead of imported, or prevent the user from importing an existing
         * preset.
         */

        var existingPreset = Presets.Find(p => p.Metadata?.Id == newPreset.Metadata.Id);

        if (existingPreset?.Metadata != null)
        {
            if (existingPreset.Metadata.Version < newPreset.Metadata.Version)
            {
                /*
                 * By grabbing the old index and replacing the preset at the old index, the list order should be
                 * maintained. As a backup, it will add the updated preset to the bottom of the list.
                 */

                var existingIndex = Presets.IndexOf(existingPreset);

                if (existingIndex != -1) Presets[existingIndex] = newPreset;
                else
                {
                    Presets.Remove(existingPreset);
                    Presets.Add(newPreset);
                }

                Save();

                return PresetImportStatus.Updated;
            }

            return PresetImportStatus.AlreadyExists;
        }

        Presets.Add(newPreset);
        Save();

        return PresetImportStatus.Success;
    }
}

/*
 * This preset class is included as an example of a class that another plugin might define. Its data is meaningless
 * outside the Name and Metadata parameters. To support preset updates, it needs to implement the IPreset interface
 * to make sure CodexLib can properly pull out the metadata and name of the preset.
 */

public class Preset : IPreset
{
    public string? StringData { get; set; }
    public int? IntData { get; set; }
    public PresetMetadata? Metadata { get; set; }
    public required string Name { get; set; }
}

/*
 * In order to use the CodexLib submodule, the plugin will need to define a "Metadata" parameter containing an ID
 * integer and a Version integer. This information is used when querying the Codex API to make sure the correct
 * presets are being pulled from the database as well as to tell if any presets need to be updated when compared to
 * the list returned from the API.
 */

public class PresetMetadata
{
    public required int Id { get; set; }
    public required int Version { get; set; }
}

/*
 * Additionally, the plugin needs to add and implement the IPreset interface so CodexLib can pull the name and
 * metadata properties from the plugin's existing presets.
 */

public interface IPreset
{
    public PresetMetadata? Metadata { get; set; }
    public string Name { get; set; }
}
