using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace CodexExample.Helpers;

public class ClickableIcon
{
    public static void Draw(FontAwesomeIcon icon, Vector4 color)
    {
        var iconString = icon.ToIconString();
        var isIconColored = ImGui.GetStateStorage().GetBool(ImGui.GetID(iconString), false);

        using (ImRaii.PushColor(ImGuiCol.Text, color, isIconColored))
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            ImGui.Text(iconString);
        }

        ImGui.GetStateStorage().SetBool(ImGui.GetID(iconString), ImGui.IsItemHovered());
    }

    public static void Draw(FontAwesomeIcon icon, uint color)
    {
        var vecColor = ImGui.ColorConvertU32ToFloat4(color);
        Draw(icon, vecColor);
    }
}
