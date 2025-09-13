# StrongboxHelper

ExileCore2 plugin that automatically applies currency orbs to strongboxes with full customization support.

## Features

### Custom Strongbox Types
- **User-defined strongbox types**: Create and manage your own strongbox configurations
- **Default types included**: Cartographer's, Blacksmith's, Jeweller's, Ornate, Researcher's, Strongbox, Large, and Arcane
- **Name-based matching**: Strongboxes are matched by their display names
- **Individual enable/disable**: Toggle each strongbox type independently

### Currency Management
- **Per-strongbox currency settings**: Each strongbox type has its own currency configuration
- **Supported currencies**:
  - Wisdom Scroll
  - Alchemy Orb
  - Augmentation Orb
  - Regal Orb
  - Exalted Orb (default enabled only for Researcher's Strongbox)
- **Priority application**: Currencies are applied in order until one is successfully used

### User Interface
- **Always-enabled system**: Strongbox types are always active, no need to enable the feature
- **Individual item expansion**: Expand/collapse each strongbox type independently to edit settings
- **Global controls**: Expand/collapse all items at once for quick overview
- **Item management**:
  - Move items up/down to reorder
  - Delete individual items with confirmation
  - Add new custom strongbox types
  - Sort alphabetically by name
  - Restore to defaults
  - Clear all items

### Advanced Features
- **Smart detection**: Automatically detects strongboxes on the ground
- **Configurable distance threshold**: Set minimum distance to strongbox before applying orbs
- **Mouse position restoration**: Option to restore mouse to original position after applying orbs
- **Confirmation dialogs**: Safety confirmations for destructive actions (can be bypassed with Shift key)

## Installation

1. Add repository via PluginUpdater
2. Enable the plugin in ExileCore2

## Configuration

### Strongbox Types
Navigate to the plugin settings to configure strongbox types:

1. **Enable/Disable**: Use the checkbox next to each strongbox type
2. **Edit Name**: Expand an item and modify the "Edit Name" field to change how strongboxes are matched
3. **Currency Settings**: Expand an item to configure which currencies to use for that strongbox type
4. **Management**: Use the control buttons to add, delete, move, or sort strongbox types

### Global Settings
- **Minimum distance to strongbox**: Set how close you need to be before orbs are applied
- **Restore mouse position**: Toggle whether to return mouse to original position after applying orbs

### Quick Actions
- **Expand All / Collapse All**: Quickly expand or collapse all strongbox type details
- **Sort by Name**: Alphabetically sort all strongbox types
- **Restore Defaults**: Reset to the original 8 strongbox types with default settings
- **Clear All**: Remove all strongbox types (use with caution)

## Usage

1. **Initial Setup**: The plugin automatically creates default strongbox types on first run
2. **Customize**: Modify strongbox names and currency settings as needed
3. **Play**: Simply enable the plugin and it will automatically apply orbs to nearby strongboxes based on your configuration
4. **Fine-tune**: Adjust settings on-the-fly as you discover new strongbox types or change your currency preferences

## License

MIT License - see LICENSE file for details.
