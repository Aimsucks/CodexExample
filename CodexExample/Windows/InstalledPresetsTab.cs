namespace CodexExample.Windows;

public static class InstalledPresetsTab
{
    public static string Status = "Idle";

    public static void Draw()
    {
        if (ImGui.BeginTable("##installedTable", 2, ImGuiTableFlags.BordersInnerV))
        {
            // Set the first column width to 200 so it doesn't scale with the window
            ImGui.TableSetupColumn("one", ImGuiTableColumnFlags.WidthFixed, 150);

            // The height parameter here is used to draw the border between the columns
            ImGui.TableNextRow(ImGuiTableRowFlags.None, ImGui.GetContentRegionAvail().Y - 2);

            // Left column
            ImGui.TableNextColumn();
            ImGui.TextWrapped("The list to the right displays all installed plugin presets. The plugin comes with " +
                              "the preset \"Mod. Preset\", which is an older version than what's available in the API.");

            ImGui.TextWrapped("Pressing the \"Update\" button will pull an update for the preset. Pressing the " +
                              "\"Reset\" button will reset the plugin's configuration to the default, which will " +
                              "allow you to update the old preset again.");

            // Right column with the presets themselves
            ImGui.TableNextColumn();

            ImGui.Text($"Status: {Status}");

            ImGui.SameLine();

            if (ImGui.Button("Reset")) Plugin.CodexExample.Configuration.Reset();

            ImGui.Separator();

            // ImGui.GetStateStorage().SetInt(ImGui.GetID("theExactSameIDAsYourTreeNode"), 0);

            // FontAwesomeIcon.ArrowsSpin

            if (ImGui.BeginChild("##presetList"))
            {
                foreach (var preset in Plugin.CodexExample.Configuration.Presets)
                    if (ImGui.TreeNode(
                            $"{preset.Name} (v{preset.Metadata?.Version.ToString()})##{preset.Metadata?.Id}"))
                    {
                        ImGui.TreeNodeEx($"String Data: {preset.StringData}",
                                         ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);

                        ImGui.TreeNodeEx($"Int Data: {preset.IntData.ToString()}",
                                         ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);

                        ImGui.TreePop();
                    }
            }

            // EndChild() is explicitly outside of BeginChild()'s if statement
            ImGui.EndChild();

            ImGui.EndTable();
        }
    }
}
