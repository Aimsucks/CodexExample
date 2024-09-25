using CodexExample.Helpers;

namespace CodexExample.Windows;

public static class InstalledPresetsTab
{
    public static void Draw()
    {
        ImGui.Text("Installed Presets");
        
        if (ImGui.Button("Reset Config"))
        {
            PluginConfig.Reset();
        }
    }
}
