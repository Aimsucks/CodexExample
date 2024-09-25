global using ImGuiNET;

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using CodexExample.Helpers;
using CodexExample.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace CodexExample;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
    internal static Plugin CodexExample { get; private set; } = null!;

    private const string CommandName = "/codex";

    public Configuration Configuration { get; set; }
    public readonly WindowSystem WindowSystem = new("CodexExample");
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        CodexExample = this;
        
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        
        var iconImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "icon.png");
        
        MainWindow = new MainWindow(this, iconImagePath);
        
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the plugin window."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
        CodexAPI.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();
    
    public void ToggleMainUI() => MainWindow.Toggle();
}
