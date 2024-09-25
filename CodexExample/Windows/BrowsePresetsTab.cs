using System.Threading.Tasks;
using CodexExample.Helpers;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace CodexExample.Windows;

public static class BrowsePresetsTab
{
    private static Task<CodexPlugin>? PresetsRequest;
    
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
                if (PresetsRequest.Result.Categories.Count > 0)
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
                // A lot of code to make the button and error text centered
                var originalPos = ImGui.GetCursorPos();
                var availableSize = ImGui.GetContentRegionAvail();
                var buttonSize = new Vector2(100, 50);
            
                var centerX = (availableSize.X - buttonSize.X) / 2;
                var centerY = (availableSize.Y - buttonSize.Y) / 2;
            
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + centerX);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + centerY);
            
                // Disable button while query is running
                // Should be fast enough to be unnoticeable unless API is down
                using (ImRaii.Disabled(PresetsRequest?.IsCompleted == false))
                {
                    if (ImGui.Button("Get Presets", buttonSize))
                    {
                        PresetsRequest = CodexAPI.GetPresets();
                    }
                }
                
                // A lot of code to make the error message centered
                if (CodexAPI.ErrorMessage.Length > 0)
                {
                    ImGui.Text(CodexAPI.ErrorMessage);
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
                    {
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.Text(FontAwesomeIcon.ArrowCircleDown.ToIconString());
                        ImGui.PopFont();
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
                            Plugin.CodexExample.Configuration.Save();
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
}
