﻿global using ImGuiNET;
using System.IO;
using CodexExample.Windows;
using CodexLib;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace CodexExample;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/codex";
    public readonly WindowSystem WindowSystem = new("CodexExample");

    public Plugin()
    {
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

        /*
         * Initialize Codex with the PluginInterface so Codex can send messages to the plugin's log.
         */

        Codex.Initialize(PluginInterface, "Codex Example");
    }

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog PluginLog { get; private set; } = null!;

    public Configuration Configuration { get; set; }
    private MainWindow MainWindow { get; init; }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
        Codex.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void ToggleMainUI()
    {
        MainWindow.Toggle();
    }
}
