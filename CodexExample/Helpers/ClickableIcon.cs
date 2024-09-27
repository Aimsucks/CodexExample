using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace CodexExample.Helpers;

public class ClickableIcon
{
    public static void Draw(FontAwesomeIcon icon, Vector4 color, string extraId = "")
    {
        var iconString = icon.ToIconString();
        var iconStringWithId = iconString + "##" + extraId;
        var isIconColored = ImGui.GetStateStorage().GetBool(ImGui.GetID(iconStringWithId), false);

        using (ImRaii.PushColor(ImGuiCol.Text, color, isIconColored))
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            ImGui.Text(iconString);
        }

        ImGui.GetStateStorage().SetBool(ImGui.GetID(iconStringWithId), ImGui.IsItemHovered());
    }

    public static void Draw(FontAwesomeIcon icon, uint color, string extraId = "")
    {
        var vecColor = ImGui.ColorConvertU32ToFloat4(color);
        Draw(icon, vecColor, extraId);
    }
}
