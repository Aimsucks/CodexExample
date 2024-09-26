using System.Numerics;
using System.Threading;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

namespace CodexExample.Helpers;

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

    public static void ResetStatus(object? state = null)
    {
        StatusColor = ImGuiColors.DalamudWhite;
        StatusText = "Idle";
        Timer?.Dispose();
    }
}
