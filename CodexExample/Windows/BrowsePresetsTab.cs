using System.Threading.Tasks;
using CodexExample.Helpers;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace CodexExample.Windows;

public static class BrowsePresetsTab
{
    private static Task<CodexPlugin?>? PresetsRequest;
    
    public static void Draw()
    {
        if (ImGui.BeginTable("##browseTable", 2, ImGuiTableFlags.BordersInnerV))
        {
            // Set the first column width to 200 so it doesn't scale with the window
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

            using (ImRaii.PushColor(ImGuiCol.Text, 0xFF62DDD8))
            {
                ImGui.TextWrapped($"Setting 1: {Plugin.CodexExample.Configuration.SettingOne}");
                ImGui.TextWrapped($"Setting 2: {Plugin.CodexExample.Configuration.SettingTwo}");
            }

            // Right column with the presets themselves
            ImGui.TableNextColumn();

            if (PresetsRequest != null && PresetsRequest.IsCompletedSuccessfully)
            {
                if (PresetsRequest.Result?.Categories.Count > 0)
                {
                    foreach (var category in PresetsRequest.Result.Categories)
                    {
                        DrawCategoryNode(category);
                    }
                }
                else
                {
                    ImGui.Text("No presets found.");
                }
            }
            else
            {
                var buttonSize = new Vector2(100, 50);
                var originalPos = CenterCursor(buttonSize);
            
                // Disable button while query is running
                // Should be fast enough to be unnoticeable unless API is down
                using (ImRaii.Disabled(PresetsRequest?.IsCompleted == false))
                {
                    if (ImGui.Button("Get Presets", buttonSize))
                    {
                        PresetsRequest = CodexAPI.GetPresets();
                    }
                }
                
                ImGui.SetCursorPos(originalPos);
                
                if (PresetsRequest != null && PresetsRequest.IsFaulted)
                {
                    var errorMessage = "Error querying Codex API!";
                    _ = CenterCursor(errorMessage, 50);
                    ImGui.Text(errorMessage);
                }
            
                // Reset the cursor so the tree of presets draws correctly
                ImGui.SetCursorPos(originalPos);
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
                    // Leaving this here for now as an alternative - creates a "clickable" tree leaf without a bullet
                    // Could do tooltip on hover and right click -> import instead of the two buttons
                    // ImGui.TreeNodeEx($"{preset.Name} (v{preset.Version.ToString()})",
                    //                  ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                    
                    ImGui.BulletText($"{preset.Name} (v{preset.Version.ToString()})");
                    
                    // Preset description icon and tooltip
                    if (preset.Description != null)
                    {
                        ImGui.SameLine();
                        ImGuiComponents.HelpMarker(preset.Description);
                    }

                    ImGui.SameLine();
                    
                    // Import button
                    // Boolean to determine if button should be colored green on hover
                    var isColored = ImGui.GetStateStorage()
                                         .GetBool(ImGui.GetID($"ImportButton##{preset.Id}"), false);
                    
                    using (ImRaii.PushColor(ImGuiCol.Text, 0xFF66AC87, isColored))
                    using (ImRaii.PushFont(UiBuilder.IconFont))
                    {
                        ImGui.Text(FontAwesomeIcon.ArrowCircleDown.ToIconString());
                    }
                    
                    ImGui.GetStateStorage()
                         .SetBool(ImGui.GetID($"ImportButton##{preset.Id}"), ImGui.IsItemHovered());
                    
                    // Hover and click actions
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        if (ImGui.IsItemClicked())
                        {
                            Plugin.PluginLog.Debug($"Clicked! {preset.Data}");
                            Plugin.CodexExample.Configuration.ImportConfiguration(preset.Data);
                            // Plugin.CodexExample.Configuration.Save();
                        }
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

    private static Vector2 CenterCursor(object input, int verticalPadding = 0)
    {
        Vector2 size = new (0,0);

        if (input is Vector2 vec) size = vec;
        else if (input is string str) size = ImGui.CalcTextSize(str);
        
        var originalPos = ImGui.GetCursorPos();
        var availableSize = ImGui.GetContentRegionAvail();
            
        var centerX = (availableSize.X - size.X) / 2f;
        var centerY = (availableSize.Y - size.Y) / 2f;
            
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + centerX);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + centerY + verticalPadding);
        
        return originalPos;
    }
}
