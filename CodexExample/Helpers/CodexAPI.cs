using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CodexExample.Helpers;

public static class CodexAPI
{
    private const string BaseUrl = "http://localhost:3000/api/plugins/";
    private const string PluginName = "Codex Example";
    private static readonly string EscapedPluginName = Uri.EscapeDataString(PluginName);

    // Reusable HTTP client
    private static readonly HttpClient HttpClient = new();
    
    public static async Task<CodexPlugin?> GetPresets()
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl + EscapedPluginName);
            response.EnsureSuccessStatusCode();
            
            var data = await response.Content.ReadFromJsonAsync<CodexPlugin>();
            return data ?? null;
        }
        catch (Exception ex)
        {
            Plugin.PluginLog.Error(ex.ToString());
            throw;
        }
    }

    public static void Dispose()
    {
        HttpClient.Dispose();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class CodexPlugin
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    
    // ReSharper disable once CollectionNeverUpdated.Global
    public required List<CodexCategory> Categories { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class CodexCategory
{
    public required string Name {get; set;}
    public List<CodexCategory>? Subcategories { get; set; }
    public List<CodexPreset>? Presets { get; set; }
    
}

public class CodexPreset
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required int Version { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required string Data { get; set; }
}
