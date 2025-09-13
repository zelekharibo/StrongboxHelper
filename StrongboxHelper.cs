using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Nodes;
using StrongboxHelper.Utils;
using ImGuiNET;

namespace StrongboxHelper
{
    public enum StrongboxType
    {
        Cartographer,
        Blacksmith,
        Jeweller,
        Ornate,
        Researcher,
        Strongbox,
        Large,
        Arcane
    }

    public enum CurrencyType
    {
        Wisdom,
        Alchemy,
        Augment,
        Regal,
        Exalted
    }

    public readonly record struct StrongboxConfig(
        string DisplayName,
        string[] Keywords, // for more robust matching
        ToggleNode Toggle,
        Dictionary<CurrencyType, bool> DefaultCurrencies
    );

    public readonly record struct CurrencyConfig(
        string DisplayName,
        string TexturePath
    );

    public class StrongboxHelper : BaseSettingsPlugin<Settings>
    {
        private bool _applyingOrbs = false;
        private readonly StringComparison _strongboxComparison = StringComparison.OrdinalIgnoreCase;
        private readonly List<(Entity entity, LabelOnGround label, string StrongboxName, Vector2 Position)> _reusableStrongboxesList = [];

        private static readonly CurrencyConfig[] s_currencies = [
            new("Wisdom Scroll", "Art/2DItems/Currency/CurrencyIdentification.dds"),
            new("Alchemy Orb", "Art/2DItems/Currency/CurrencyUpgradeToRare.dds"),
            new("Augment Orb", "Art/2DItems/Currency/CurrencyAddModToMagic.dds"),
            new("Regal Orb", "Art/2DItems/Currency/CurrencyUpgradeMagicToRare.dds"),
            new("Exalted Orb", "Art/2DItems/Currency/CurrencyAddModToRare.dds")
        ];

        private StrongboxSettings CurrencySettings => Settings.StrongboxSettings;
        private StrongboxEnableSettings EnableSettings => Settings.StrongboxEnableSettings;

        private static readonly Dictionary<StrongboxType, (string DisplayName, string[] Keywords)> s_strongboxInfo = new()
        {
            [StrongboxType.Cartographer] = ("Cartographer's Strongbox", ["cartographer"]),
            [StrongboxType.Blacksmith] = ("Blacksmith's Strongbox", ["blacksmith"]),
            [StrongboxType.Jeweller] = ("Jeweller's Strongbox", ["jeweller", "jeweler"]),
            [StrongboxType.Ornate] = ("Ornate Strongbox", ["ornate"]),
            [StrongboxType.Researcher] = ("Researcher's Strongbox", ["researcher", "research"]),
            [StrongboxType.Strongbox] = ("Strongbox", ["strongbox"]),
            [StrongboxType.Large] = ("Large Strongbox", ["large"]),
            [StrongboxType.Arcane] = ("Arcane Strongbox", ["arcane"])
        };

        public override void Render()
        {
            if (!Settings.Enable || _applyingOrbs) return;

            _applyingOrbs = true;
            ApplyOrbsOnStrongbox().ContinueWith(t => _applyingOrbs = false);
        }

        private StrongboxType? GetStrongboxType(string strongboxName)
        {
            var nameLower = strongboxName.ToLower();
            
            foreach (var (type, (_, keywords)) in s_strongboxInfo)
            {
                if (keywords.Any(keyword => nameLower.Contains(keyword, _strongboxComparison)))
                {
                    return type;
                }
            }
            
            return null;
        }

        public override void DrawSettings()
        {
            base.DrawSettings();

            ImGui.Separator();
            ImGui.Text("Strongbox Configuration");
            ImGui.Separator();
            DrawStrongboxSettings();
        }

        private void DrawStrongboxSettings()
        {
            foreach (var (strongboxType, (displayName, _)) in s_strongboxInfo)
            {
                var enabled = EnableSettings.GetEnabled(strongboxType);
                if (ImGui.Checkbox(displayName, ref enabled))
                    EnableSettings.SetEnabled(strongboxType, enabled);

                if (enabled)
                {
                    ImGui.Indent();
                    DrawCurrencyToggles(strongboxType);
                    ImGui.Unindent();
                }
                ImGui.Spacing();
            }
        }

        private void DrawCurrencyToggles(StrongboxType strongboxType)
        {
            var strongboxSettings = CurrencySettings.GetSettings(strongboxType);
            
            if (ImGui.BeginTable($"{strongboxType}CurrencyTable", 3, ImGuiTableFlags.None))
            {
                ImGui.TableSetupColumn("Column1", ImGuiTableColumnFlags.WidthFixed, 120);
                ImGui.TableSetupColumn("Column2", ImGuiTableColumnFlags.WidthFixed, 120);
                ImGui.TableSetupColumn("Column3", ImGuiTableColumnFlags.WidthFixed, 120);

                for (int row = 0; row < 2; row++)
                {
                    ImGui.TableNextRow();
                    for (int col = 0; col < 3; col++)
                    {
                        var index = row * 3 + col;
                        if (index >= s_currencies.Length) break;
                        
                        ImGui.TableNextColumn();
                        var currencyType = (CurrencyType)index;
                        var currencyConfig = s_currencies[index];
                        var enabled = strongboxSettings.GetCurrency(currencyType);
                        
                        if (ImGui.Checkbox(currencyConfig.DisplayName, ref enabled))
                            strongboxSettings.SetCurrency(currencyType, enabled);
                    }
                }

                ImGui.EndTable();
            }
        }

        private Element? RecursiveFindChildWithText(Element element, string text, HashSet<Element>? visited = null)
        {
            // initialize visited set on first call to prevent infinite recursion
            visited ??= [];
            
            // null checks and circular reference protection
            if (element == null || visited.Contains(element))
                return null;
                
            visited.Add(element);

            // check if current element has the text we're looking for
            if (!string.IsNullOrEmpty(element.Text) && element.Text.Contains(text, _strongboxComparison))
                return element;
                
            // recursively check children
            if (element.Children != null)
            {
                foreach (var child in element.Children)
                {
                    var result = RecursiveFindChildWithText(child, text, visited);
                    if (result != null) return result;
                }
            }
            
            return null;
        }

        private bool IsStrongboxTypeEnabled(string strongboxName)
        {
            var strongboxType = GetStrongboxType(strongboxName);
            return strongboxType.HasValue && EnableSettings.GetEnabled(strongboxType.Value);
        }

        private bool IsCurrencyEnabledForStrongbox(string strongboxName, CurrencyType currencyType)
        {
            var strongboxType = GetStrongboxType(strongboxName);
            return strongboxType.HasValue && CurrencySettings.GetSettings(strongboxType.Value).GetCurrency(currencyType);
        }

        private List<(Entity entity, LabelOnGround label, string StrongboxName, Vector2 Position)> DetectStrongboxesOnGround()
        {
            _reusableStrongboxesList.Clear();
            try
            {
                var itemLabels = GameController.IngameState.IngameUi.ItemsOnGroundLabelElement;
                var labelsVisible = itemLabels?.LabelsOnGroundVisible;
                if (labelsVisible?.Any() != true) 
                {
                    return _reusableStrongboxesList;
                }

                foreach (var label in labelsVisible)
                {
                    try
                    {
                        var itemOnGround = label?.ItemOnGround;
                        var metadata = itemOnGround?.Metadata;
                        if (metadata?.Contains("StrongBoxes") != true) continue;

                        var labelElement = label?.Label;
                        if (labelElement == null) continue;

                        // cache rect calculation for reuse
                        var labelRect = labelElement.GetClientRectCache;
                        var position = new Vector2(labelRect.Center.X, labelRect.Center.Y);
                        
                        // check label's children for strongbox name
                        var strongBoxName = RecursiveFindChildWithText(labelElement, "Strongbox");
                        if (strongBoxName != null && itemOnGround != null && label != null) {
                            _reusableStrongboxesList.Add((itemOnGround, label, strongBoxName.Text, position));
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error detecting strongboxes: {ex.Message}");
            }

            return _reusableStrongboxesList;
        }

        static private Element? RecursiveFindChildWithTextureName(Element element, string textureName)
        {
            if (element.TextureName == textureName) return element;
            if (element.Children == null) return null;
            foreach (var child in element.Children)
            {
                var result = RecursiveFindChildWithTextureName(child, textureName);
                if (result != null) return result;
            }
            return null;
        }

        private async Task ApplyOrbsOnStrongbox()
        {
            var strongboxesOnGround = DetectStrongboxesOnGround();
            if (strongboxesOnGround.Count == 0) {
                return;
            }

            // group essences by position (same position = same monolith)
            var strongboxGroups = strongboxesOnGround.GroupBy(e => e.Position).ToList();
            
            // find the closest monolith group by parent element
            var closestGroup = strongboxGroups.OrderBy(g => g.First().entity.DistancePlayer).FirstOrDefault();
            if (closestGroup == null) {
                return;
            }

            var (entity, label, StrongboxName, Position) = closestGroup.First();

            // check if this strongbox type is enabled
            if (!IsStrongboxTypeEnabled(StrongboxName)) {
                return;
            }

            var distance = entity.DistancePlayer;
            if (distance > Settings.MinimumDistanceToStrongboxToApplyOrbs.Value) {
                return;   
            }

            // recursive find child with TextureName in priority order
            string? textureName = null;
            Element? child = null;
            for (var i = 0; i < s_currencies.Length; i++) {
                var currencyConfig = s_currencies[i];
                var currencyType = (CurrencyType)i;
                
                // check if this currency type is enabled for this strongbox
                if (!IsCurrencyEnabledForStrongbox(StrongboxName, currencyType)) {
                    continue;
                }

                textureName = currencyConfig.TexturePath;
                child = RecursiveFindChildWithTextureName(label.Label, textureName);
                if (child != null) {
                    break;
                }
            }

            if (child == null || textureName == null || label.Label == null) {
                return;
            }

            var previousMousePosition = Mouse.GetCursorPosition();

            // repeat clicking until no more child with texture is found
            while (child != null)
            {
                await Mouse.MoveMouse(child.GetClientRectCache.Center + GameController.Window.GetWindowRectangleTimeCache.TopLeft);
                await Task.Delay(10);
                await Mouse.LeftDown();
                await Task.Delay(10);
                await Mouse.LeftUp();
                await Task.Delay(250);
                
                // look for another child with the same texture
                child = RecursiveFindChildWithTextureName(label.Label, textureName);
            }

            if (Settings.RestoreMouseToOriginalPosition)
            {
                await Mouse.MoveMouse(previousMousePosition);
            }
        }
    }
}