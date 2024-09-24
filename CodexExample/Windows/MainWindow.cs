using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace CodexExample.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly string IconImagePath;
    private readonly Plugin Plugin;
    
    private readonly int IconWidth = 100;
    private readonly int HeaderPadding = 20;
    
    // Create a larger font for the header
    private readonly IFontHandle HeaderFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(
        e => e.OnPreBuild(
            t => t.AddDalamudDefaultFont(20)));
    
    public MainWindow(Plugin plugin, string iconImagePath)
        : base("Codex Example##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        IconImagePath = iconImagePath;
        Plugin = plugin;
    }

    public void Dispose()
    {
        HeaderFont.Dispose();
    }

    public override void Draw()
    {
        ImGui.Text($"The random config bool is {Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");
        ImGui.Separator();
        
        if (ImGui.BeginTable("headerTable", 2))
        {
            ImGui.TableSetupColumn("one", ImGuiTableColumnFlags.WidthFixed, IconWidth + HeaderPadding);
            // ImGui.TableSetupColumn("two", ImGuiTableColumnFlags.WidthStretch);
            
            ImGui.TableNextRow();
            
            // Icon column
            ImGui.TableNextColumn();
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (HeaderPadding / 2f));
            var iconImage = Plugin.TextureProvider.GetFromFile(IconImagePath).GetWrapOrDefault();
            if (iconImage != null) ImGui.Image(iconImage.ImGuiHandle, new Vector2(IconWidth, IconWidth));

            // Text column
            ImGui.TableNextColumn();
            
            // Header text
            if(HeaderFont.Available)
            {
                HeaderFont.Push();
                ImGui.Text("Codex Example");
                HeaderFont.Pop();
            } else ImGui.Text("Codex Example");
            
            // Description text
            ImGui.TextWrapped("Here you will find an example of how a plugin can interact with the Codex plugin preset API. Please review the two tabs below to understand how the plugin functions.");

            ImGui.EndTable();
        }
        
        ImGui.Separator();

        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        ImGui.Spacing();

        ImGui.Text("Have an icon:");
        var iconImage = Plugin.TextureProvider.GetFromFile(IconImagePath).GetWrapOrDefault();
        if (iconImage != null)
        {
            ImGuiHelpers.ScaledIndent(55f);
            ImGui.Image(iconImage.ImGuiHandle, new Vector2(iconImage.Width, iconImage.Height));
            ImGuiHelpers.ScaledIndent(-55f);
        }
        else
        {
            ImGui.Text("Image not found.");
        }
    }
}
