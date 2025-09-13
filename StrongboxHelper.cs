using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore2;
using ExileCore2.Shared;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.Elements.InventoryElements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using ImGuiNET;
using StrongboxHelper.Utils;

namespace StrongboxHelper
{
    public class StrongboxHelper : BaseSettingsPlugin<Settings>
    {
        private bool _applyingOrbs = false;
        private readonly StringComparison _strongboxComparison = StringComparison.OrdinalIgnoreCase;
        private readonly List<(Entity entity, LabelOnGround label, string StrongboxName, Vector2 Position)> _reusableStrongboxesList = new();
        private readonly HashSet<string> _reusableStrongboxNames = new();

        public override void Render()
        {
            if (!Settings.Enable || _applyingOrbs) return;

            _applyingOrbs = true;
            ApplyOrbsOnStrongbox().ContinueWith(t => _applyingOrbs = false);
        }

        private Element RecursiveFindChildWithText(Element element, string text, HashSet<Element> visited = null)
        {
            // initialize visited set on first call to prevent infinite recursion
            visited ??= new HashSet<Element>();
            
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
                        if (strongBoxName != null) {
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

        private Element RecursiveFindChildWithTextureName(Element element, string textureName)
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

            var firstStrongbox = closestGroup.First();
            var isResearchStrongbox = firstStrongbox.StrongboxName.Contains("Research", _strongboxComparison);


            if (Settings.EnableOnlyResearchStrongbox.Value && !isResearchStrongbox) {
                return;
            }

            var distance = firstStrongbox.entity.DistancePlayer;
            if (distance > Settings.MinimumDistanceToStrongboxToApplyOrbs.Value) {
                return;   
            }

            // wisdom scroll -> Art/2DItems/Currency/CurrencyIdentification.dds
            // alchemy orb -> Art/2DItems/Currency/CurrencyUpgradeToRare.dds
            // augment orb -> Art/2DItems/Currency/CurrencyAddModToMagic.dds
            // regal orb -> Art/2DItems/Currency/CurrencyUpgradeMagicToRare.dds
            // exalted orb -> Art/2DItems/Currency/CurrencyAddModToRare.dds

            var orbs = new List<string> {
                "Art/2DItems/Currency/CurrencyIdentification.dds",
                "Art/2DItems/Currency/CurrencyUpgradeToRare.dds",
                "Art/2DItems/Currency/CurrencyAddModToMagic.dds",
                "Art/2DItems/Currency/CurrencyUpgradeMagicToRare.dds",
                "Art/2DItems/Currency/CurrencyAddModToRare.dds",
            };

            // priority -> alchemy orb -> augment orb -> regal orb -> exalted orb

            // recursive find child with TextureName in priority order
            string textureName = null;
            Element child = null;
            for (var i = 0; i < orbs.Count; i++) {
                switch (i) {
                case 0:
                    if (!Settings.EnableWisdomScroll.Value) {
                        continue;
                    }
                    break;
                case 1:
                    if (!Settings.EnableAlchemyOrb.Value) {
                        continue;
                    }
                    break;
                case 2:
                    if (!Settings.EnableAugmentOrb.Value) {
                        continue;
                    }
                    break;
                case 3:
                    if (!Settings.EnableRegalOrb.Value) {
                        continue;
                    }
                    break;
                case 4:
                    if (!Settings.EnableExaltedOrb.Value || !isResearchStrongbox && Settings.EnableExaltedOrbOnlyOnResearchStrongbox.Value) {
                        continue;
                    }
                    break;
                default:
                    return;
                }

                textureName = orbs[i];
                child = RecursiveFindChildWithTextureName(firstStrongbox.label.Label, textureName);
                if (child != null) {
                    break;
                }
            }

            if (child == null || textureName == null) {
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
                child = RecursiveFindChildWithTextureName(firstStrongbox.label.Label, textureName);
            }

            if (Settings.RestoreMouseToOriginalPosition)
            {
                await Mouse.MoveMouse(previousMousePosition);
            }
        }
    }
}