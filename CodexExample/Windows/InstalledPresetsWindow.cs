using CodexExample.Helpers;

namespace CodexExample.Windows;

public static class InstalledPresetsWindow
{
    public static void Draw()
    {
        ImGui.Text("Installed Presets");
        
        if (ImGui.Button("Reset Config"))
        {
            ResetPluginConfig.Reset();
        }
    }
}
