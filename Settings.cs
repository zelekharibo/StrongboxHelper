using System.Linq;
using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;

namespace StrongboxHelper
{
    public class Settings : ISettings
    {
        public ToggleNode Enable { get; set; } = new(true);

        [Menu("Minimum distance to strongbox to apply orbs", "Minimum distance to strongbox to apply orbs")]
        public RangeNode<int> MinimumDistanceToStrongboxToApplyOrbs { get; set; } = new(50, 0, 1000);

        [Menu("Restore mouse to original position", "Restore mouse to original position")]
        public ToggleNode RestoreMouseToOriginalPosition { get; set; } = new(true);

        public StrongboxTypeGroup CustomStrongboxTypes { get; set; } = new();
        
        private bool _defaultsInitialized = false;
        
        public void EnsureDefaultsInitialized()
        {
            if (_defaultsInitialized || CustomStrongboxTypes.Items.Any()) return;
            
            // populate with default strongbox types
            var researcherStrongbox = new StrongBoxSettingsItem("Researcher's Strongbox") { Exalted = true };
            
            CustomStrongboxTypes.Items.AddRange(new[]
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
            
            _defaultsInitialized = true;
        }
    }
}