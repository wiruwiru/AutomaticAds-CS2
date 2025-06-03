# Plugin Behavior & Features

This document provides detailed information about AutomaticAds CS2 plugin behavior, features, and advanced usage scenarios.

## Core Features

### Automatic Announcements
- **Interval-based**: Announcements are sent automatically based on configured intervals
- **Sequential ordering**: When `SendAdsInOrder` is enabled, ads are sent in a sequential manner
- **Individual timing**: Each ad can have its own interval setting, overriding the global interval

### Display Types
The plugin supports multiple ways to display messages:

- **Chat** (default): Standard chat messages visible to all players
- **Center**: Messages displayed in the center of the screen
- **CenterHtml**: Center screen messages with HTML formatting support for rich text display

### Sound System
- **Global sounds**: Default sound played with all announcements
- **Per-ad sounds**: Individual ads can have specific sound effects
- **Sound control**: Sounds can be disabled globally or per announcement

### Multi-language Support
- **Localization**: Messages can be configured for different languages
- **Player-specific**: Each player sees messages in their preferred language when available
- **Fallback system**: Uses default language when player's language is not available

## Advanced Features

### Permission-based Filtering
- **View flags**: Control who can see specific announcements
- **Exclude flags**: Hide announcements from specific user groups
- **Admin detection**: Special variables for admin counting and listing

### Map-specific Announcements
- Configure announcements to appear only on specific maps
- Use `"all"` to show on all maps or specify exact map names

### Conditional Display
- **Warmup only**: Announcements that only appear during warmup periods
- **Spectator only**: Messages visible only to spectators
- **On death**: Announcements triggered when players die

### Manual Triggers
- **Command-based**: Players can trigger specific announcements with commands
- **Disable automatic**: Ads can be set to manual-only mode
- **Multiple triggers**: Each ad can have multiple trigger commands

### Welcome & Connection Messages
- **Welcome messages**: Customizable messages for new players
- **Join/Leave notifications**: Custom connection and disconnection messages
- **Delayed welcome**: Configurable delay before showing welcome message

## Configuration Examples

### Basic Announcement
```json
{
  "message": "{prefix} {RED}Welcome to our server!",
  "interval": 300
}
```

### VIP-only Announcement
```json
{
  "message": "{GOLD}Thank you for being a VIP member!",
  "viewFlag": "@css/vip",
  "interval": 600
}
```

### Map-specific with Custom Sound
```json
{
  "message": "{BLUE}You're playing on the famous {GREEN}{map}{BLUE}!",
  "map": "de_dust2",
  "playSoundName": "ui/panorama/popup_reveal_02",
  "interval": 450
}
```

### Manual-only Announcement
```json
{
  "message": "{prefix} {YELLOW}Server information: {WHITE}Connect at {ip}:{port}",
  "triggerAd": ["info", "serverinfo"],
  "disableInterval": true,
  "disableSound": true
}
```

### HTML Center Message
```json
{
  "message": "<font class='fontSize-l' color='red'>SPECIAL EVENT</font><br><font color='yellow'>Double XP Weekend!</font>",
  "displayType": "CenterHtml",
  "interval": 180
}
```

### Multi-language Announcement
```json
{
  "message": {
    "en": "{prefix} {WHITE}Welcome to our {GREEN}English{WHITE} server!",
    "es": "{prefix} {WHITE}¡Bienvenido a nuestro servidor en {GREEN}Español{WHITE}!",
    "fr": "{prefix} {WHITE}Bienvenue sur notre serveur {GREEN}Français{WHITE}!"
  },
  "interval": 240
}
```

## Best Practices

### Interval Management
- Use reasonable intervals (minimum 10 seconds) to avoid spam
- Consider server activity when setting intervals
- Use longer intervals for less important messages

### Permission Strategy
- Use `viewFlag` for exclusive content (VIP announcements)
- Use `excludeFlag` to hide messages from specific groups
- Set most announcements to `"all"` for maximum visibility

### Sound Usage
- Use sounds sparingly to avoid audio pollution
- Disable sounds for frequent announcements
- Use different sounds for different types of messages

### Message Design
- Keep messages concise and clear
- Use colors to highlight important information
- Include relevant variables to make messages dynamic

## Troubleshooting

### Common Issues
- **Messages not appearing**: Check `viewFlag` and `excludeFlag` settings
- **Wrong intervals**: Verify `SendAdsInOrder` setting and individual intervals
- **Sounds not playing**: Check `disableSound` settings and sound file paths
- **Variables not working**: Ensure proper variable syntax with curly braces