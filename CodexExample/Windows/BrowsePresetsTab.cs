using System.Numerics;
using System.Threading.Tasks;
using CodexExample.Helpers;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;

namespace CodexExample.Windows;

public static class BrowsePresetsTab
{
    public static Task<CodexPlugin?>? PresetsRequest;
    private static bool QueryState;

    private static Plugin? Plugin;

    public static void Draw(Plugin plugin)
    {
        Plugin = plugin;

        if (ImGui.BeginTable("##browseTable", 2, ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("one", ImGuiTableColumnFlags.WidthFixed, 175);

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
                ImGui.TextWrapped($"Setting 1: {Plugin.Configuration.SettingOne}");
                ImGui.TextWrapped($"Setting 2: {Plugin.Configuration.SettingTwo}");
            }

            // Right column with the presets themselves
            ImGui.TableNextColumn();

            ImGui.Text("Status:");
            ImGui.SameLine();
            StatusMessage.Draw();

            if (PresetsRequest != null && PresetsRequest.IsCompletedSuccessfully)
            {
                // var availableWidth = ImGui.GetContentRegionAvail().X;
                // var iconWidth = ImGui.CalcTextSize(FontAwesomeIcon.Sync.ToIconString()).X +
                //                 (ImGui.GetStyle().ItemSpacing.X * 2);
                //
                // ImGui.SameLine(availableWidth - iconWidth);

                /*
                 * The Update button queries the Codex API for a list of all available presets. Later, this
                 * functionality will be expanded to only show presets not currently installed by checking the
                 * Preset.Metadata.Id and comparing it to the list received.
                 *
                 * TODO: Fix Get Presets button interaction with StatusMessage so this can be uncommented.
                 */

                // ClickableIcon.Draw(FontAwesomeIcon.Sync, ImGuiColors.ParsedBlue);
                // if (ImGui.IsItemClicked())
                // {
                //     PresetsRequest = CodexAPI.GetPresets();
                //     QueryState = true;
                //
                //     StatusMessage.SetStatus("Querying API", StatusMessage.Status.Warning, 3000);
                // }

                ImGui.Separator();

                if (PresetsRequest.Result?.Categories.Count > 0)
                {
                    if (QueryState)
                    {
                        StatusMessage.ResetStatus();
                        QueryState = false;
                    }

                    foreach (var category in PresetsRequest.Result.Categories)
                        DrawCategoryNode(category, category.Name);
                }
                else StatusMessage.SetStatus("No presets found", StatusMessage.Status.Warning, 3000);
            }
            else
            {
                ImGui.Separator();

                var buttonSize = new Vector2(100, 50);
                var originalPos = CenterCursor(buttonSize);

                // Disable button while query is running
                // Should be fast enough to be unnoticeable unless API is down
                using (ImRaii.Disabled(PresetsRequest?.IsCompleted == false))
                {
                    if (ImGui.Button("Get Presets", buttonSize))
                    {
                        PresetsRequest = CodexAPI.GetPresets();
                        QueryState = true;
                    }
                }

                // Reset the cursor so the tree of presets draws correctly
                ImGui.SetCursorPos(originalPos);

                if (PresetsRequest?.IsCompleted == false)
                    StatusMessage.SetStatus("Querying API", StatusMessage.Status.Warning, 3000);

                if (PresetsRequest != null && PresetsRequest.IsFaulted)
                    StatusMessage.SetStatus("Error querying API", StatusMessage.Status.Error, 3000);
            }

            ImGui.EndTable();
        }
    }

    private static void DrawCategoryNode(CodexCategory category, string topLevelCategory)
    {
        if (ImGui.TreeNode(category.Name))
        {
            if (category.Presets != null)
            {
                foreach (var preset in category.Presets)
                {
                    ImGui.BulletText($"{preset.Name} (v{preset.Version.ToString()})");

                    // Preset description icon and tooltip
                    if (preset.Description != null)
                    {
                        ImGui.SameLine();
                        ImGuiComponents.HelpMarker(preset.Description);
                    }

                    ImGui.SameLine();

                    // Import button
                    ClickableIcon.Draw(FontAwesomeIcon.ArrowCircleDown, 0xFF66AC87, preset.Name + preset.Id);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        if (ImGui.IsItemClicked())
                        {
                            string message;
                            StatusMessage.Status status;

                            /*
                             * The topLevelCategory variable here should not be used in a real plugin - this is one of
                             * a few different ways that this example can display the ability to import a preset to the
                             * plugin's configuration directly or add/update a preset to/in the plugin's preset list.
                             */

                            if (topLevelCategory == "Configuration Presets")
                            {
                                (message, status) =
                                    Plugin.Configuration.ImportConfiguration(Plugin, preset) switch
                                    {
                                        Configuration.PresetImportStatus.Success => ("Config imported",
                                                    StatusMessage.Status.Success),

                                        _ => ("Config not imported", StatusMessage.Status.Error)
                                    };
                            }

                            else if (topLevelCategory == "Plugin Presets")
                            {
                                (message, status) = Plugin.Configuration.ImportPreset(preset) switch
                                {
                                    Configuration.PresetImportStatus.Success =>
                                        ("Preset imported", StatusMessage.Status.Success),

                                    Configuration.PresetImportStatus.Updated =>
                                        ("Preset updated", StatusMessage.Status.Success),

                                    Configuration.PresetImportStatus.AlreadyExists =>
                                        ("Preset exists", StatusMessage.Status.Warning),

                                    _ => ("Preset not imported", StatusMessage.Status.Error)
                                };
                            }

                            else
                            {
                                Plugin.PluginLog.Warning(
                                    $"The plugin category for \"{preset.Name}\" is not recognized.");

                                message = "Unrecognized preset";
                                status = StatusMessage.Status.Warning;
                            }

                            StatusMessage.SetStatus(message, status, 2000);
                        }
                    }
                }
            }

            // Recursively create tree nodes for subcategories
            if (category.Subcategories != null && category.Subcategories.Count > 0)
            {
                foreach (var subcategory in category.Subcategories)
                    DrawCategoryNode(subcategory, topLevelCategory);
            }

            ImGui.TreePop();
        }
    }

    /*
     * The following functions are helpers to center buttons and text within a content region. It's being used to
     * center the "Get Presets" button and the text underneath it in the event that there's an error fetching from
     * the Codex API.
     */

    internal static Vector2 CenterCursor(Vector2 input, int verticalPadding = 0)
    {
        var originalPos = ImGui.GetCursorPos();
        var availableSize = ImGui.GetContentRegionAvail();

        var centerX = (availableSize.X - input.X) / 2f;
        var centerY = (availableSize.Y - input.Y) / 2f;

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + centerX);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + centerY + verticalPadding);

        return originalPos;
    }
}
