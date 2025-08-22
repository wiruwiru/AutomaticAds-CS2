# AutomaticAds CS2
This plugin allows you to schedule and display announcements in the chat at customizable intervals. Each announcement is accompanied by a brief sound effect to capture players' attention seamlessly.

> [!CAUTION]
> Ads with `"Screen"` have been re-enabled. However, the current solution does not work as well as it did before the 07/28/2025 update. It is recommended not to use this feature for now, pending a more stable method.

https://github.com/user-attachments/assets/aae16cc4-7c67-477a-8c89-437d5c035211

## 📖 Complete Documentation
For detailed installation instructions, configuration examples, and advanced usage, please visit our complete documentation:

**[🔗 View Full Documentation](https://www.lucauy.dev/docs/automaticads)**

---

### Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download [AutomaticAds.zip](https://github.com/wiruwiru/AutomaticAds-CS2/releases/latest) from releases
3. Extract and upload to your game server
4. Start server and configure the generated config file

---

### Main Configuration Parameters

| Parameter            | Description                                                                                       | Required |
|----------------------|---------------------------------------------------------------------------------------------------|----------|
| `ChatPrefix`         | The prefix displayed in the chat before each announcement. Supports colors. To use it, include `{prefix}` inside the `message` field in the ads configuration.                        | **YES**  |
| `GlobalPlaySound`      | Sound that is played when an announcement is sent in case `playSoundName` is not set in the announcement and `disableSound` is not `true`. Leave it blank to disable it.                             | **YES**  |
| `GlobalInterval` | Default interval (in seconds) between announcements if no individual interval is set in the ad configuration. (**Default**: 30s) | **YES**  |
| `GlobalPositionX` | Default X position for screen messages. Range: -5.0 to 5.0. Negative values = left, positive = right. (**Default**: -1.8) | **YES**  |
| `GlobalPositionY` | Default Y position for screen messages. Range: -3.0 to 3.0. Negative values = lower, positive = higher. (**Default**: 1.0) | **YES**  |
| `AdminFlag`      | The permission flag required for a player to be considered an admin in `{admincount}` and `{adminnames}`. | **YES**  |
| `SendAdsInOrder`     | Send announcements in an orderly manner, respecting the intervals.                                | **YES**  |
| `UseWelcomeMessage`  | Set to `true` to enable the welcome message. Set to `false` to disable it.                        | **YES**   |
| `JoinLeaveMessages`  | Set `true` to enable custom connection and disconnection messages. Set `false` to disable it.                        | **YES**   |
| `WelcomeDelay`  | This is the time the plugin will wait to send the welcome message after the player connects (**Default**: 3s).                        | **YES**   |
| `CenterHtmlDisplayTime`  | Duration (in seconds) that ads with `displayType` set to `CenterHtml` will remain visible. (**Default**: 5s) | **YES**   |
| `ScreenDisplayTime`  | Duration (in seconds) that ads with `displayType` set to `Screen` will remain visible. (**Default**: 5s) | **YES**   |
| `UseMultiLang` | Enables support for multiple languages in messages. When `true`, the plugin will attempt to use the player's language if available. (**Default**: `true`) | **YES**  |
| `DefaultLanguage` | Language used as a fallback when no localized message is found for a player. (**Default**: `en`) | **YES**  |
| `Welcome`     | Configuration for the welcome announcement. Supports variables. | **NO**   |
| `JoinLeave`     | Configuration for connection and disconnection messages. Supports variables. | **NO**   |
| `Ads`                | List of advertisements to be sent. Each ad can be configured individually. | **YES**  |

### Ads Configuration Parameters

| Parameter       | Description                                                                                         | Required |
|-----------------|-----------------------------------------------------------------------------------------------------|----------|
| `message` | The message/announcement to send in the chat. Supports colors. | **YES**  |
| `viewFlag` | Flag required to view the message. Set it to `"all"` to make it available to all players (default: `"all"`). | **NO** |
| `excludeFlag` | Users with this flag will not see the message. Set it to `""` so that no players are excluded (default: `""`). | **NO** |
| `map` | The map where this announce should appear. Use `"all"` to show it on all maps or specify a map name (default: `"all"`). | **NO**   |
| `interval` | The interval **(in seconds)** between sending this ad. Must be between `10` and `3600` (default: `600`). | **NO** |
| `positionX` | X position for screen messages. Range: -5.0 to 5.0. If not set, uses `GlobalPositionX`. Only applies to `displayType: "Screen"`. | **NO** |
| `positionY` | Y position for screen messages. Range: -3.0 to 3.0. If not set, uses `GlobalPositionY`. Only applies to `displayType: "Screen"`. | **NO** |
| `disableSound` | If `true`, no sound will be played when this ad is sent (default: `false`). | **NO** |
| `onlyInWarmup` | If `true`, the ad will only be sent during the warmup period (default: `false`).   | **NO** |
| `onlySpec` | If `true`, this ad will only be sent to players on the spectator team (default: `false`). | **NO** |
| `onDead` | If `true`, this ad will be sent when a player dies (default: `false`). | **NO** |
| `triggerAd` | An array of commands that players can use to view this announcement before it is sent automatically. For example: `["command1", "command2"]`. | **NO** |
| `disableInterval` | If `true`, this ad will not be sent automatically. It will only be sent manually via `triggerAd` commands (default: `false`). | **NO** |
| `disableOrder` | If `true`, this ad will ignore the sequential sending order when `sendAdsInOrder` is enabled (default: `false`). | **NO** |
| `playSoundName` | The specific sound to play when this announcement is sent. If not set, `GlobalPlaySound` will be used if set, provided `disableSound` is not `true`. | **NO** |
| `displayType` | Controls how the message is displayed. Options: `"Chat"` (default, normal chat message), `"Center"` (center screen text), `"CenterHtml"` (center screen with HTML formatting support), `"Screen"` (floating text on the player's screen). | **NO** |

### Available Colors
`GREEN`, `RED`, `YELLOW`, `BLUE`, `ORANGE`, `WHITE`, `PURPLE`, `GREY`, `LIGHT_RED`, `LIGHT_BLUE`, `LIGHT_YELLOW`, `LIGHT_PURPLE`, `DARK_RED`, `BLUE_GREY`, `DARK_BLUE`, `LIME`, `OLIVE`, `GOLD`, `SILVER`, `MAGENTA`.

### Available Variables
You can use the following placeholders in your announcements:

| Variable        | Description                                                   |
|----------------|---------------------------------------------------------------|
| `{prefix}`       | A dynamic variable replaced with the value of ChatPrefix in each message. Enables optional inclusion or exclusion of the prefix. |
| `{ip}`           | The server's IP address.                                      |
| `{port}`         | The server's port.                                            |
| `{hostname}`     | The server's hostname.                                        |
| `{map}`          | The current map being played.                                 |
| `{time}`         | The current time in the server's timezone.                    |
| `{date}`         | The current date.                                             |
| `{playername}`   | The name the player has on Steam.                             |
| `{players}`      | The current number of players online.                         |
| `{maxplayers}`   | The maximum number of players the server can hold.            |
| `{admincount}`   | The number of administrators currently online.                |
| `{adminnames}`   | A comma-separated list of the names of online administrators. |

---

## Additional Information

- **[📋 Behavior & Features](BEHAVIOR.md)** - Detailed information about plugin behavior and features
- **[🤝 Contributing](CONTRIBUTING.md)** - Guidelines for contributing to the project
- **[📝 TO-DO](TODO.md)** - Upcoming tasks and features

---

## Support

For issues, questions, or feature requests, please visit our [GitHub Issues](https://github.com/wiruwiru/AutomaticAds-CS2/issues) page.
