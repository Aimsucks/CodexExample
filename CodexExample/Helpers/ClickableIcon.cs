using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace CodexExample.Helpers;

/*
 * Because clickable icons aren't implemented in Dalamud, a function had to be made to avoid using ugly buttons. Calling
 * ClickableIcon() draws a FontAwesomeIcon that changes colors on hover and, when followed by ImGui.IsItemClicked(),
 * will call an action when the icon is clicked.
 *
 * Since this is an example plugin, it was included as a way to assist in providing users a way to interact with the
 * return of the Codex API. None of this is required to interact with the Codex API.
 */

public static class ClickableIcon
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
