using System;
using System.Numerics;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Windowing;

namespace CodexExample.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly string IconImagePath;
    
    // ReSharper disable once NotAccessedField.Local
    private readonly Plugin Plugin;
    
    private readonly int IconWidth = 100;
    private readonly int HeaderPadding = 20;
    
    // Create a larger font for the header
    private readonly IFontHandle HeaderFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(
        e => e.OnPreBuild(
            t => t.AddDalamudDefaultFont(20)));
    
    public MainWindow(Plugin plugin, string iconImagePath)
        : base("Codex Example", 
               ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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
        // Header
        ImGui.Separator();
        
        if (ImGui.BeginTable("##headerTable", 2))
        {
            ImGui.TableSetupColumn("one",
                                   ImGuiTableColumnFlags.WidthFixed,
                                   IconWidth + HeaderPadding);
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
            ImGui.TextWrapped("Here you will find an example of how a plugin can interact with the Codex plugin " +
                              "preset API. Please review the two tabs below to understand how the plugin functions.");

            ImGui.EndTable();
        }
        
        ImGui.Separator();

        // Body
        ImGui.BeginTabBar("##tabBar", ImGuiTabBarFlags.NoTooltip);

        // Set tab width to half of the window so they scale appropriately
        float windowWidth = ImGui.GetWindowWidth();
        
        ImGui.SetNextItemWidth(windowWidth / 2);
        if (ImGui.BeginTabItem("Installed Presets"))
        {
            // Call a separate class's Draw() function to move code out of this class
            InstalledPresetsWindow.Draw();
            ImGui.EndTabItem();
        }

        ImGui.SetNextItemWidth(windowWidth / 2);
        if (ImGui.BeginTabItem("Browse Presets"))
        {
            BrowsePresetsWindow.Draw();
            ImGui.EndTabItem();
        }
        
        ImGui.EndTabBar();
    }
}
