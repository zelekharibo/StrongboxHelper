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

        [Menu("Enable only research strongbox", "Enable only research strongbox")]
        public ToggleNode EnableOnlyResearchStrongbox { get; set; } = new(false);

        [Menu("Enable wisdom scroll", "Enable wisdom scroll")]
        public ToggleNode EnableWisdomScroll { get; set; } = new(true);

        [Menu("Enable alchemy orb", "Enable alchemy orb")]
        public ToggleNode EnableAlchemyOrb { get; set; } = new(true);

        [Menu("Enable augment orb", "Enable augment orb")]
        public ToggleNode EnableAugmentOrb { get; set; } = new(true);

        [Menu("Enable regal orb", "Enable regal orb")]
        public ToggleNode EnableRegalOrb { get; set; } = new(true);

        [Menu("Enable exalted orb", "Enable exalted orb")]
        public ToggleNode EnableExaltedOrb { get; set; } = new(true);

        [Menu("Enable exalted orb only on research strongbox", "Enable exalted orb only on research strongbox")]
        public ToggleNode EnableExaltedOrbOnlyOnResearchStrongbox { get; set; } = new(false);

        [Menu("Restore mouse to original position", "Restore mouse to original position")]
        public ToggleNode RestoreMouseToOriginalPosition { get; set; } = new(true);
    }
}