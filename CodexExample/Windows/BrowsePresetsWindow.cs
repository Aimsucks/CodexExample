using CodexExample.Helpers;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace CodexExample.Windows;

public static class BrowsePresetsWindow
{
    public static void Draw()
    {
        if (ImGui.BeginTable("##browseTable", 2, ImGuiTableFlags.BordersInnerV))
        {
            // Set the first column width to 150 so it doesn't scale with the window
            ImGui.TableSetupColumn("one", ImGuiTableColumnFlags.WidthFixed, 200);
            
            // The height parameter here is used to draw the border between the columns
            ImGui.TableNextRow(ImGuiTableRowFlags.None, ImGui.GetContentRegionAvail().Y - 2);
            
            // Left column
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Clicking \"Get Presets\" on the right side will download presets from the Codex API, " +
                              "displaying categories and sub-categories with actual presets being marked by a bullet " +
                              "point.");
            
            ImGui.TextWrapped("Below are options that \"Configuration Presets\" can modify. Test it out by importing " +
                              "one of the presets under the aforementioned category.");
            
            ImGui.Spacing();
            
            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF62DDD8);
            ImGui.TextWrapped("Setting 1: True");
            ImGui.TextWrapped("Setting 2: 30");
            ImGui.PopStyleColor();

            // Right column with the presets themselves
            ImGui.TableNextColumn();
            
            // A lot of code to make the button centered follows
            var availableSize = ImGui.GetContentRegionAvail();
            var originalPos = ImGui.GetCursorPos();
            var buttonSize = new Vector2(100, 50);
            
            var centerX = (availableSize.X - buttonSize.X) / 2;
            var centerY = (availableSize.Y - buttonSize.Y) / 2;
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + centerX);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + centerY);
            
            if (CodexAPI.Presets == null && ImGui.Button("Get Presets", buttonSize))
            {
                _ = CodexAPI.GetPresets();
            }
            
            // Reset the cursor so the tree of presets draws correctly
            ImGui.SetCursorPos(originalPos);
        
            if (CodexAPI.Presets != null && CodexAPI.Presets.Categories.Count > 0)
            {
                foreach (var category in CodexAPI.Presets.Categories)
                {
                    DrawCategoryNode(category);
                }
            }

            ImGui.EndTable();
        }
    }

    private static void DrawCategoryNode(CodexCategory category)
    {
        if (ImGui.TreeNode(category.Name))
        {
            if (category.Presets != null)
            {
                foreach (var preset in category.Presets)
                {
                    ImGui.BulletText($"{preset.Name}"); 
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Import"))
                    {
                        Plugin.PluginLog.Debug("Clicked!");
                    }
                }
            }

            // Recursively create tree nodes for subcategories
            if (category.Subcategories != null && category.Subcategories.Count > 0)
            {
                foreach (var subcategory in category.Subcategories)
                {
                    // Recursion for subcategories
                    DrawCategoryNode(subcategory); 
                }
            }
            
            ImGui.TreePop();
        }
    }
}
