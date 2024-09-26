using System;
using System.Numerics;
using System.Threading;
using Dalamud.Interface.Utility.Raii;

namespace CodexExample.Helpers;

public static class StatusMessage
{
    private static string StatusText = "Idle";
    private static Vector4 StatusColor = Dalamud.Interface.Colors.ImGuiColors.DalamudWhite;
    private static Timer? Timer;

    public enum Status {
        Default,
        Success,
        Error,
        Warning
    }


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
            Status.Success => Dalamud.Interface.Colors.ImGuiColors.HealerGreen,
            Status.Error => Dalamud.Interface.Colors.ImGuiColors.DPSRed,
            Status.Warning => Dalamud.Interface.Colors.ImGuiColors.DalamudYellow,
            _ => Dalamud.Interface.Colors.ImGuiColors.DalamudWhite
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
        StatusColor = Dalamud.Interface.Colors.ImGuiColors.DalamudWhite;
        StatusText = "Idle";
        Timer?.Dispose();
    }
}
