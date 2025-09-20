using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;

namespace StrongboxHelper
{
    public enum CurrencyType
    {
        Wisdom,
        Alchemy,
        Augment,
        Regal,
        Exalted
    }

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

        private StrongboxTypeGroup CustomStrongboxTypes => Settings.CustomStrongboxTypes;

        public override void Render()
        {
            if (!Settings.Enable || _applyingOrbs) return;
            
            // ensure defaults are initialized on first run
            Settings.EnsureDefaultsInitialized();

            _applyingOrbs = true;
            ApplyOrbsOnStrongbox().ContinueWith(t => _applyingOrbs = false);
        }

        private StrongBoxSettingsItem? GetMatchingStrongboxType(string strongboxName)
        {
            var nameLower = strongboxName.ToLower();
            var activeTypes = CustomStrongboxTypes.GetActiveItems();
            
            // find the first matching strongbox type by name
            return activeTypes
                .Where(item => nameLower.Contains(item.Name.ToLower(), _strongboxComparison))
                .FirstOrDefault();
        }

        public override void DrawSettings()
        {
            base.DrawSettings();
            
            // ensure defaults are initialized before drawing UI
            Settings.EnsureDefaultsInitialized();
            
            CustomStrongboxTypes.DrawSettings();
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
            var matchingType = GetMatchingStrongboxType(strongboxName);
            return matchingType != null && matchingType.Enabled;
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

            // group strongboxes by position (same position = same strongbox)
            var strongboxGroups = strongboxesOnGround.GroupBy(e => e.Position).ToList();
            
            // find the closest strongbox group by parent element
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

            // get the matching strongbox type to check its currency settings
            var matchingType = GetMatchingStrongboxType(StrongboxName);
            if (matchingType == null) {
                return;
            }

            // apply currencies in priority order, but only those enabled for this strongbox type
            string? textureName = null;
            Element? child = null;
            for (var i = 0; i < s_currencies.Length; i++) {
                var currencyConfig = s_currencies[i];
                var currencyType = (CurrencyType)i;
                
                // check if this currency is enabled for this specific strongbox type
                if (!matchingType.GetCurrency(currencyType)) {
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

            var previousMousePosition = Input.MousePosition;

            // repeat clicking until no more child with texture is found
                Input.SetCursorPos(child.GetClientRectCache.Center + GameController.Window.GetWindowRectangleTimeCache.TopLeft);
                await Task.Delay(10);
                Input.MouseMove();
                await Task.Delay(10);
                Input.Click(System.Windows.Forms.MouseButtons.Left);

            if (Settings.RestoreMouseToOriginalPosition)
            {
                Input.SetCursorPos(previousMousePosition);
                Input.MouseMove();
            }
        }
    }
}