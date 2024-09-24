using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CodexExample.Helpers;

public static class CodexAPI
{
    public static CodexPlugin? Presets;
    
    private const string BaseUrl = "http://localhost:3000/api/plugins/";
    private const string PluginName = "pluginOne";

    // Reusable HTTP client
    private static readonly HttpClient HttpClient = new();
    
    public static async Task GetPresets()
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl + PluginName);
            var data = await response.Content.ReadFromJsonAsync<CodexPlugin>();

            if (data != null && data.Categories.Count > 0) Presets = data;
        }
        catch (Exception ex)
        {
            Plugin.PluginLog.Error(ex.ToString());
        }
    }
}

public abstract class CodexPlugin
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public abstract required List<CodexCategory> Categories { get; set; }
}

public abstract class CodexCategory
{
    public required string Name {get; set;}
    public List<CodexCategory>? Subcategories { get; set; }
    public List<CodexPreset>? Presets { get; set; }
    
}

public abstract class CodexPreset
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int Version { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required string Data { get; set; }
    
}
