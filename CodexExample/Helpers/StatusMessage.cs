using System.Numerics;
using System.Threading;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

namespace CodexExample.Helpers;

/*
 * This class is used to draw a status message on both the "Installed Presets" and "Browse Presets" tabs of the
 * example plugin. The Draw() function renders the default text, which is "Idle" in the color white. When the plugin
 * calls SetStatus(), it will change the text and color for however long the duration provided is.
 *
 * Since this is an example plugin, it was included as a way to assist in informing users what the plugin is actually
 * doing when browsing, importing, and updating presets. None of this is required to interact with the Codex API.
 */

public static class StatusMessage
{
    public enum Status
    {
        Default,
        Success,
        Error,
        Warning
    }

    private static string StatusText = "Idle";
    private static Vector4 StatusColor = ImGuiColors.DalamudWhite;
    private static Timer? Timer;

    public static void Draw()
    {
        using (ImRaii.PushColor(ImGuiCol.Text, StatusColor))
        {
            ImGui.Text(StatusText);
        }
    }

    public static void SetStatus(string text, Status status = Status.Default, int duration = 5000)
    {
        StatusColor = status switch
        {
            Status.Success => ImGuiColors.HealerGreen,
            Status.Error => ImGuiColors.DPSRed,
            Status.Warning => ImGuiColors.DalamudYellow,
            _ => ImGuiColors.DalamudWhite
        };

        StatusText = text;

        if (duration > 0)
        {
            Timer?.Dispose();
            Timer = new Timer(ResetStatus, null, duration, Timeout.Infinite);
        }
    }

    /*
     * ResetStatus() is helpful to go back to an idle state after doing something like querying the API.
     */

    public static void ResetStatus(object? state = null)
    {
        StatusColor = ImGuiColors.DalamudWhite;
        StatusText = "Idle";
        Timer?.Dispose();
    }
}
