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

        public StrongboxEnableSettings StrongboxEnableSettings { get; set; } = new();
        public StrongboxSettings StrongboxSettings { get; set; } = new();
    }
}