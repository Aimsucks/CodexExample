using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodexExample.Helpers;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace CodexExample.Windows;

public static class InstalledPresetsTab
{
    public static Task<List<CodexPreset>>? PresetUpdatesRequest;
    private static bool QueryState;
    private static bool PresetJustUpdated;
    private static bool IdleState;

    public static void Draw()
    {
        if (ImGui.BeginTable("##installedTable", 2, ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("one", ImGuiTableColumnFlags.WidthFixed, 175);

            ImGui.TableNextRow(ImGuiTableRowFlags.None, ImGui.GetContentRegionAvail().Y - 2);

            // Left column
            ImGui.TableNextColumn();
            ImGui.TextWrapped("The list to the right displays all installed plugin presets. The plugin comes with " +
                              "the preset \"Mod. Preset\", which is an older version than what's available in the API.");

            ImGui.TextWrapped("Pressing the \"Update\" button will pull an update for the preset. Pressing the " +
                              "\"Reset\" button will reset the plugin's configuration to the default, which will " +
                              "allow you to update the old preset again.");

            ImGui.TextWrapped("Open the preset and click \"Update\" to import the update.");

            // Right column with the presets themselves
            ImGui.TableNextColumn();

            ImGui.Text("Status:");
            ImGui.SameLine();
            StatusMessage.Draw();

            // Right-align the following two icons
            var availableWidth = ImGui.GetContentRegionAvail().X;
            var iconWidth = ImGui.CalcTextSize(FontAwesomeIcon.Undo.ToIconString()).X +
                            ImGui.CalcTextSize(FontAwesomeIcon.Sync.ToIconString()).X +
                            (ImGui.GetStyle().ItemSpacing.X * 2);

            ImGui.SameLine(availableWidth - iconWidth);

            /*
             * The Reset button here will update the plugin's configuration to the default state, which has one
             * out-of-date preset. It will also remove the query result from the update task. This is used to see
             * how updating presets works.
             */

            ClickableIcon.Draw(FontAwesomeIcon.Undo, ImGuiColors.DPSRed);
            if (ImGui.IsItemClicked())
            {
                Plugin.CodexExample.Configuration.Reset();
                PresetUpdatesRequest = null;
                PresetJustUpdated = false;

                StatusMessage.SetStatus("Config reset", StatusMessage.Status.Warning, 2000);
            }

            ImGui.SameLine();

            /*
             * The Update button queries the Codex API using a list of presets (or a list of preset IDs) and will
             * return a list of CodexPresets correlating to the list of presets or IDs that you sent over. These
             * presets can be used to update presets contained in the plugin's configuration.
             */

            ClickableIcon.Draw(FontAwesomeIcon.Sync, ImGuiColors.ParsedBlue);
            if (ImGui.IsItemClicked())
            {
                if (PresetUpdatesRequest != null && PresetUpdatesRequest.IsCompletedSuccessfully)
                    PresetUpdatesRequest = null;
                PresetUpdatesRequest = CodexAPI.GetPresetUpdates(Plugin.CodexExample.Configuration.Presets);
                QueryState = true;
                IdleState = false;
                PresetJustUpdated = false;
            }

            ImGui.Separator();

            Action? postIteration = null;

            /*
             * What follows this comment before the foreach() loop is code that can be ignored - it contains a lot of
             * conditionals to draw different status messages.
             */

            if (PresetUpdatesRequest != null && PresetUpdatesRequest.IsCompletedSuccessfully && !PresetJustUpdated)
            {
                if (PresetUpdatesRequest.Result.Count > 0 && QueryState)
                {
                    StatusMessage.ResetStatus();
                    QueryState = false;
                    PresetJustUpdated = false;
                }

                // Compare the two lists and return true if any presets are outdated
                var anyOutdatedPresets = Plugin.CodexExample.Configuration.Presets.Any(
                    preset => preset.Metadata != null && PresetUpdatesRequest.Result.Any(
                                  codexPreset =>
                                      codexPreset.Id == preset.Metadata.Id &&
                                      preset.Metadata.Version < codexPreset.Version));

                if (anyOutdatedPresets)
                    StatusMessage.SetStatus("Updates found", StatusMessage.Status.Success, 3000);
                else if (!IdleState)
                {
                    StatusMessage.SetStatus("No updates found", StatusMessage.Status.Warning, 3000);
                    IdleState = true;
                }
            }

            if (PresetUpdatesRequest?.IsCompleted == false)
                StatusMessage.SetStatus("Querying API", StatusMessage.Status.Warning, 3000);

            if (PresetUpdatesRequest != null && PresetUpdatesRequest.IsFaulted)
                StatusMessage.SetStatus("Error querying API", StatusMessage.Status.Error, 3000);

            /*
             * Iterate through the preset list from the plugin configuration and show if there are any updates available
             * for presets.
             */

            foreach (var preset in Plugin.CodexExample.Configuration.Presets)
            {
                var presetUpdatesResult =
                    PresetUpdatesRequest != null && PresetUpdatesRequest.IsCompletedSuccessfully
                        ? PresetUpdatesRequest.Result.First(p => p.Id == preset.Metadata?.Id)
                        : null;

                var presetCanBeUpdated = presetUpdatesResult?.Version > preset.Metadata?.Version;

                // Variables to clean up the TreeNode line
                var presetNameString = $"{preset.Name} (v{preset.Metadata?.Version.ToString()})";
                var presetUpdateString = presetCanBeUpdated ? " (Update available!)" : "";
                var presetIdString = (string type) => $"{type}{preset.Name}{preset.Metadata?.Id}";

                if (ImGui.TreeNode($"{presetNameString}{presetUpdateString}##{presetIdString("tree")}"))
                {
                    /*
                     * Moving the execution of Plugin.CodexExample.Configuration.ImportPreset() to the outside of
                     * the foreach() loop prevents throwing an InvalidOperationException by modifying the list while
                     * iterating through it.
                     */

                    if (presetUpdatesResult != null && presetCanBeUpdated &&
                        ImGui.Button($"Update##{presetIdString("update")}"))
                    {
                        postIteration = () =>
                        {
                            string message;
                            StatusMessage.Status status;

                            (message, status) =
                                Plugin.CodexExample.Configuration.ImportPreset(presetUpdatesResult) switch
                                {
                                    Configuration.PresetImportStatus.Success =>
                                        ("Preset imported", StatusMessage.Status.Success),

                                    Configuration.PresetImportStatus.Updated =>
                                        ("Preset updated", StatusMessage.Status.Success),

                                    Configuration.PresetImportStatus.AlreadyExists =>
                                        ("Preset exists", StatusMessage.Status.Warning),

                                    _ => ("Preset not imported", StatusMessage.Status.Error)
                                };

                            PresetJustUpdated = true;
                            StatusMessage.SetStatus(message, status, 2000);
                        };
                    }

                    ImGui.BulletText($"String Data: {preset.StringData}");
                    ImGui.BulletText($"Int Data: {preset.IntData.ToString()}");
                    ImGui.TreePop();
                }
            }

            postIteration?.Invoke();

            ImGui.EndTable();
        }
    }
}
