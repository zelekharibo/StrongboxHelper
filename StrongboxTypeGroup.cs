using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;
using ImGuiNET;
using StrongboxHelper.Extensions;

namespace StrongboxHelper
{
    public class StrongBoxSettingsItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Default Item";
        
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
        
        [JsonPropertyName("wisdom")]
        public bool Wisdom { get; set; } = true;

        [JsonPropertyName("alchemy")]
        public bool Alchemy { get; set; } = true;

        [JsonPropertyName("augment")]
        public bool Augment { get; set; } = true;

        [JsonPropertyName("regal")]
        public bool Regal { get; set; } = true;

        [JsonPropertyName("exalted")]
        public bool Exalted { get; set; } = false;
        
        [JsonIgnore]
        private bool _expanded = false;

        public StrongBoxSettingsItem()
        {
        }

        public StrongBoxSettingsItem(string name)
        {
            Name = name;
        }

        public void Display()
        {
            var enabled = Enabled;
            var name = Name;
            
            // enable toggle with color
            if (enabled)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Color.Lime.ToVector4());
                if (ImGui.Checkbox("Enabled", ref enabled)) Enabled = enabled;
                ImGui.PopStyleColor();
            }
            else
            {
                if (ImGui.Checkbox("Enabled", ref enabled)) Enabled = enabled;
            }
            
            ImGui.SameLine();
            
            // expand/collapse button for this item
            if (ImGui.SmallButton(_expanded ? "[-]" : "[+]"))
            {
                _expanded = !_expanded;
            }
            
            ImGui.SameLine();
            
            // always show the name
            ImGui.Text(Name);
            
            if (_expanded)
            {
                ImGui.Indent();
                
                // name input (expanded only)
                ImGui.SetNextItemWidth(200);
                if (ImGui.InputText("Edit Name", ref name, 50)) Name = name;
                
                ImGui.Spacing();
                ImGui.Text("Currencies:");
                
                // currency toggles in a table
                if (ImGui.BeginTable($"CurrencyTable_{Name}", 3, ImGuiTableFlags.None))
                {
                    ImGui.TableSetupColumn("Column1", ImGuiTableColumnFlags.WidthFixed, 150);
                    ImGui.TableSetupColumn("Column2", ImGuiTableColumnFlags.WidthFixed, 150);
                    ImGui.TableSetupColumn("Column3", ImGuiTableColumnFlags.WidthFixed, 150);

                    // first row
                    ImGui.TableNextRow();
                    
                    ImGui.TableNextColumn();
                    var wisdom = Wisdom;
                    if (ImGui.Checkbox("Wisdom Scroll", ref wisdom)) Wisdom = wisdom;
                    
                    ImGui.TableNextColumn();
                    var alchemy = Alchemy;
                    if (ImGui.Checkbox("Alchemy Orb", ref alchemy)) Alchemy = alchemy;
                    
                    ImGui.TableNextColumn();
                    var augment = Augment;
                    if (ImGui.Checkbox("Augment Orb", ref augment)) Augment = augment;
                    
                    // second row
                    ImGui.TableNextRow();
                    
                    ImGui.TableNextColumn();
                    var regal = Regal;
                    if (ImGui.Checkbox("Regal Orb", ref regal)) Regal = regal;
                    
                    ImGui.TableNextColumn();
                    var exalted = Exalted;
                    if (ImGui.Checkbox("Exalted Orb", ref exalted)) Exalted = exalted;

                    ImGui.EndTable();
                }
                
                ImGui.Unindent();
            }
        }
        
        public bool GetCurrency(CurrencyType type) => type switch
        {
            CurrencyType.Wisdom => Wisdom,
            CurrencyType.Alchemy => Alchemy,
            CurrencyType.Augment => Augment,
            CurrencyType.Regal => Regal,
            CurrencyType.Exalted => Exalted,
            _ => false
        };

        public void SetCurrency(CurrencyType type, bool value)
        {
            switch (type)
            {
                case CurrencyType.Wisdom: Wisdom = value; break;
                case CurrencyType.Alchemy: Alchemy = value; break;
                case CurrencyType.Augment: Augment = value; break;
                case CurrencyType.Regal: Regal = value; break;
                case CurrencyType.Exalted: Exalted = value; break;
            }
        }

        public void SetExpanded(bool expanded)
        {
            _expanded = expanded;
        }
    }

    public class StrongboxTypeGroup
    {
        [JsonIgnore]
        private bool _expand;
        
        [JsonIgnore]
        private int _deleteIndex = -1;

        [JsonPropertyName("items")]
        public List<StrongBoxSettingsItem> Items { get; set; } = new();
        
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Strongbox Types";

        public StrongboxTypeGroup()
        {
            Enabled = true; // always enabled
        }

        public StrongboxTypeGroup(string name = "Strongbox Types")
        {
            Name = name;
            Enabled = true; // always enabled
        }

        public void DrawSettings()
        {
            DrawGroupHeader();
            DrawGroupControls();
            DrawItemList();
            DrawAddItemButton();
            HandleDeleteConfirmation();
        }

        private void DrawGroupHeader()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Color.Lime.ToVector4());
            ImGui.Text("Strongbox Types");
            ImGui.PopStyleColor();
        }

        private void DrawGroupControls()
        {
            // expand/collapse button
            if (Items.Count > 0)
            {
                if (!_expand)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Color.Green.ToVector4());
                    if (ImGui.Button($"Expand All###ExpandHideButton"))
                    {
                        _expand = true;
                        ExpandAllItems();
                    }
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Button($"Collapse All###ExpandHideButton"))
                    {
                        _expand = false;
                        CollapseAllItems();
                    }
                }

                ImGui.SameLine();
            }

            // sort button
            if (ImGui.Button("Sort by Name"))
            {
                SortItemsByName();
            }
            
            ImGui.SameLine();
            
            // restore defaults button
            if (ImGui.Button("Restore Defaults"))
            {
                if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                {
                    RestoreDefaults();
                }
                else
                {
                    _deleteIndex = -3; // special value to indicate restore defaults
                }
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("hold Shift to restore defaults without confirmation");
            }
            
            ImGui.SameLine();
            
            // clear all button
            if (Items.Count > 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, Color.Red.ToVector4());
                if (ImGui.Button("Clear All"))
                {
                    if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                    {
                        ClearAllItems();
                    }
                    else
                    {
                        _deleteIndex = -2; // special value to indicate clear all
                    }
                }
                ImGui.PopStyleColor();
                
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("hold Shift to clear all without confirmation");
                }
            }
        }

        private void DrawItemList()
        {
            if (Items.Count == 0) return;

            // create a copy of the items list to avoid concurrent modification issues
            var itemsCopy = Items.ToList();
            
            for (var i = 0; i < itemsCopy.Count; i++)
            {
                // double-check that the item still exists in the original list
                if (i >= Items.Count || !ReferenceEquals(Items[i], itemsCopy[i]))
                {
                    break; // list was modified, stop rendering
                }
                
                ImGui.PushID($"StrongboxItem{i}");
                
                try
                {
                    if (i != 0)
                    {
                        ImGui.Separator();
                    }

                    DrawItemControls(i);
                    
                    // additional safety check before accessing Items[i]
                    if (i < Items.Count && Items[i] is not null)
                    {
                        Items[i].Display();
                    }
                }
                finally
                {
                    ImGui.PopID();
                }
            }
        }

        private void DrawItemControls(int index)
        {
            // safety check
            if (index < 0 || index >= Items.Count) {
                return;
            }
            
            // move up button
            if (index > 0 && index < Items.Count && ImGui.SmallButton("^")) {
                MoveItem(index, index - 1);
            }
            if (index > 0) {
                ImGui.SameLine();
            }

            // move down button
            if (index < Items.Count - 1 && index >= 0 && ImGui.SmallButton("v")) {
                MoveItem(index, index + 1);
            }

            if (index < Items.Count - 1) {
                ImGui.SameLine();
            }

            // delete button
            if (ImGui.Button("Delete")) {
                if (ImGui.IsKeyDown(ImGuiKey.ModShift)) {
                    RemoveAt(index);
                    return;
                }

                _deleteIndex = index;
            }
            else if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("delete this strongbox type (hold Shift to skip confirmation)");
            }

            ImGui.SameLine();
        }

        private void DrawAddItemButton()
        {
            if (ImGui.Button("Add Strongbox Type")) {
                Items.Add(new StrongBoxSettingsItem("New Strongbox Type"));
            }
        }

        private void HandleDeleteConfirmation()
        {
            if (_deleteIndex != -1) {
                ImGui.OpenPopup("StrongboxDeleteConfirmation");
            }

            string? itemName = null;
            if (_deleteIndex == -2) {
                itemName = $"ALL {Items.Count} strongbox types";
            }
            else if (_deleteIndex == -3) {
                itemName = "all settings and restore to defaults";
            }
            else if (_deleteIndex >= 0 && _deleteIndex < Items.Count) {
                itemName = $"strongbox type '{Items[_deleteIndex].Name}'";
            }

            var deleteResult = ImGuiExt.DrawDeleteConfirmationPopup(
                "StrongboxDeleteConfirmation", itemName);
                
            if (deleteResult == true) {
                if (_deleteIndex == -2) {
                    ClearAllItems();
                }
                else if (_deleteIndex == -3) {
                    RestoreDefaults();
                }
                else {
                    RemoveAt(_deleteIndex);
                }
            }

            if (deleteResult.HasValue) {
                _deleteIndex = -1;
            }
        }

        private void SortItemsByName()
        {
            Items = Items.OrderBy(x => x.Name).ToList();
        }

        private void ExpandAllItems()
        {
            foreach (var item in Items)
            {
                item.SetExpanded(true);
            }
        }

        private void CollapseAllItems()
        {
            foreach (var item in Items)
            {
                item.SetExpanded(false);
            }
        }

        private void ClearAllItems()
        {
            if (Items is not null) {
                try {
                    Items.Clear();
                }
                catch (Exception) {
                    // in case of concurrent modification, create a new list
                    Items = new List<StrongBoxSettingsItem>();
                }
            }
        }

        private void RestoreDefaults()
        {
            ClearAllItems();
            
            // populate with default strongbox types
            var researcherStrongbox = new StrongBoxSettingsItem("Researcher's Strongbox") { Exalted = true };
            
            Items.AddRange(new[]
            {
                new StrongBoxSettingsItem("Cartographer's Strongbox"),
                new StrongBoxSettingsItem("Blacksmith's Strongbox"),
                new StrongBoxSettingsItem("Jeweller's Strongbox"),
                new StrongBoxSettingsItem("Ornate Strongbox"),
                researcherStrongbox,
                new StrongBoxSettingsItem("Strongbox"),
                new StrongBoxSettingsItem("Large Strongbox"),
                new StrongBoxSettingsItem("Arcane Strongbox")
            });
        }

        public List<StrongBoxSettingsItem> GetActiveItems()
        {
            // always enabled - return all enabled items
            return Items.Where(x => x.Enabled)
                       .OrderBy(x => x.Name)
                       .ToList();
        }

        private void RemoveAt(int index)
        {
            if (Items is null) return;
            
            if (index >= 0 && index < Items.Count) {
                try {
                    Items.RemoveAt(index);
                }
                catch (ArgumentOutOfRangeException) {
                    // list was modified concurrently, ignore
                }
            }
        }

        private void MoveItem(int sourceIndex, int targetIndex)
        {
            if (Items is null || 
                sourceIndex < 0 || sourceIndex >= Items.Count ||
                targetIndex < 0 || targetIndex >= Items.Count ||
                sourceIndex == targetIndex) {
                return;
            }

            try {
                var movedItem = Items[sourceIndex];
                Items.RemoveAt(sourceIndex);
                Items.Insert(targetIndex, movedItem);
            }
            catch (ArgumentOutOfRangeException) {
                // list was modified concurrently, ignore
            }
        }
    }
}
