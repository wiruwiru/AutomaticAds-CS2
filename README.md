# AutomaticAds CS2
This plugin allows you to schedule and display announcements in the chat at customizable intervals. Each announcement is accompanied by a brief sound effect to capture players' attention seamlessly.

https://github.com/user-attachments/assets/aae16cc4-7c67-477a-8c89-437d5c035211

---

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master).

2. Download [AutomaticAds.zip](https://github.com/wiruwiru/AutomaticAds-CS2/releases) from the releases section.

3. Unzip the archive and upload it to the game server.

4. Start the server and wait for the configuration file to be generated.

5. Edit the configuration file with the parameters of your choice.

---

## Config
The configuration file will be automatically generated when the plugin is first loaded. Below are the parameters you can customize:

| Parameter            | Description                                                                                       | Required |
|----------------------|---------------------------------------------------------------------------------------------------|----------|
| `ChatPrefix`         | The prefix displayed in the chat before each announcement. Supports colors. To use it, include `{prefix}` inside the `message` field in the ads configuration.                        | **YES**  |
| `GlobalPlaySound`      | Sound that is played when an announcement is sent in case `playSoundName` is not set in the announcement and `disableSound` is not `true`. Leave it blank to disable it.                             | **YES**  |
| `AdminFlag`      | The permission flag required for a player to be considered an admin in `{admincount}` and `{adminnames}`. | **YES**  |
| `sendAdsInOrder`     | Send announcements in an orderly manner, respecting the intervals.                                | **YES**  |
| `UseWelcomeMessage`  | Set to `true` to enable the welcome message. Set to `false` to disable it.                        | **YES**   |
| `JoinLeaveMessages`  | Set `true` to enable custom connection and disconnection messages. Set `false` to disable it.                        | **YES**   |
| `WelcomeDelay`  | This is the time the plugin will wait to send the welcome message after the player connects (**Default**: 3s).                        | **YES**   |
| `centerHtmlDisplayTime`  | "Duration (in seconds) that ads with `displayType` set to `CenterHtml` will remain visible. (**Default**: 5s) | **YES**   |
| `useMultiLang` | Enables support for multiple languages in messages. When `true`, the plugin will attempt to use the player's language if available. (**Default**: `true`) | **YES**  |
| `defaultLanguage` | Language used as a fallback when no localized message is found for a player. (**Default**: `en`) | **YES**  |
| `Welcome`     | Configuration for the welcome announcement. Supports variables ***(see example below)***. | **NO**   |
| `JoinLeave`     | Configuration for connection and disconnection messages. Supports variables ***(see example below)***. | **NO**   |
| `Ads`                | List of advertisements to be sent. Each ad can be configured individually ***(see example below)***. | **YES**  |


---

### Ads Configuration
Each item in the `Ads` list represents a single advertisement. Here are the fields available:

| Parameter       | Description                                                                                         | Required |
|-----------------|-----------------------------------------------------------------------------------------------------|----------|
| `message` | The message/announcement to send in the chat. Supports colors. | **YES**  |
| `displayType` | Controls how the message is displayed. Options: `"Chat"` (default, normal chat message), `"Center"` (center screen text), `"CenterHtml"` (center screen with HTML formatting support). | **NO** |
| `interval` | The interval **(in seconds)** between sending this ad. Must be between `10` and `3600` ***(if you don't add it to the announce configuration it will be set to `600` by default)***. | **NO** |
| `viewFlag` | Flag required to view the message. Set it to `“all”` to make it available to all players ***(if you don't add it to the announce settings it will be set to `“all”` by default)***. | **NO** |
| `excludeFlag` | Users with this flag will not see the message. Set it to `“”` so that no players are excluded ***(if you do not add it to the announce settings it will be set to `“”` by default)***. | **NO** |
| `map` | The map where this announce should appear. Use `“all”` to show it on all maps or specify a map name ***(if you don't add it to the announce configuration it will be set to `“all”` by default)***. | **NO**   |
| `disableSound` | If `true`, no sound will be played when this ad is sent ***(if you don't add it to the announce configuration it will be set to `false` by default)***. | **NO** |
| `onlyInWarmup` | If `true`, the ad will only be sent during the warmup period. If `false` or not specified, it will be sent normally regardless of the warmup ***(if you don't add it to the announce configuration it will be set to `false` by default)***.   | **NO** |
| `onlySpec` | If `true`, this ad will only be sent to players on the spectator team ***(if you don't add it to the announce configuration it will be set to `false` by default)***. | **NO** |
| `onDead` | If `true`, this ad will be sent when a player dies ***(if you don't add it to the announce configuration it will be set to `false` by default)***. | **NO** |
| `playSoundName` | The specific sound to play when this announcement is sent. If not set, `GlobalPlaySound` will be used if set, provided `disableSound` is not `true`. | **NO** |
| `triggerAd` | An array of commands that players can use to view this announcement before it is sent automatically. For example: `["command1", "command2"]`. | **NO** |
| `disableinterval` | If `true`, this ad will not be sent automatically. It will only be sent manually via `triggerAd` commands. (if you don't add it to the announce configuration it will be set to `false` by default). | **NO** |
---

## Configuration Example
Here is an example configuration file:
```json
{
  "ChatPrefix": " [{GREEN}AutomaticAds{WHITE}]{WHITE}",
  "GlobalPlaySound": "ui/panorama/popup_reveal_01",
  "AdminFlag": "@css/generic",
  "sendAdsInOrder": true,
  "UseWelcomeMessage": true,
  "JoinLeaveMessages": true,
  "WelcomeDelay": 3,
  "centerHtmlDisplayTime": 5,
  "useMultiLang": true,
  "defaultLanguage": "en",
  "Welcome": [
    {
      "WelcomeMessage": "{prefix} {BLUE}Welcome to the server {playername}! {RED}Playing on {map} with {players}/{maxplayers} players.",
      "viewFlag": "all",
      "excludeFlag": "",
      "disableSound": false
    }
  ],
  "JoinLeave": [
    {
      "JoinMessage": "{BLUE}{playername} ({id64}) {GREEN}joined the server from {country} ({country_code})! {WHITE}Online: {GOLD}{players}{WHITE}/{RED}{maxplayers}.",
      "LeaveMessage": "{BLUE}{playername} ({id64}) {RED}left the server!"
    }
  ],
  "Ads": [
    {
      "message": "{prefix} {RED}AutomaticAds is the best plugin!",
      "viewFlag": "all",
      "excludeFlag": "@css/vip",
      "map": "all",
      "interval": 600,
      "disableSound": false,
      "onlyInWarmup": true
    },
    {
      "message": "{BLUE}Welcome to {hostname}! {RED}The time is {time} of {date}, playing in {map} with {players}/{maxplayers}. Connect {ip}",
      "interval": 800
    },
    {
      "message": "{BLUE}Thank you for supporting the server! {GOLD}Your contribution is greatly appreciated.",
      "viewFlag": "@css/vip",
      "map": "de_mirage",
      "interval": 1000,
      "disableSound": true
    },
    {
      "message": "{prefix} {GOLD}Congratulations, you are playing on {map}.",
      "excludeFlag": "@css/vip",
      "disableSound": true,
      "triggerAd": ["map", "currentmap"],
      "Disableinterval": true,
      "playSoundName": "sound/ui/beep22.wav"
    },
    {
      "message": "<font class='fontSize-m' color='orange'>This server uses</font><br><font class='fontSize-l' style='color:red;'>AutomaticAds</font></font>",
      "displayType": "CenterHtml",
      "disableSound": true,
    },
    {
      "message": {
        "en": "{prefix} {WHITE}Message in {GREEN}English{WHITE}!",
        "es": "{prefix} {WHITE}¡Mensaje en {GREEN}Español{WHITE}!"
      },
      "interval": 120,
      "disableSound": true
    }
  ],
  "ConfigVersion": 1
}
```

### List of Available Colors:
`GREEN`, `RED`, `YELLOW`, `BLUE`, `ORANGE`, `WHITE`, `PURPLE`, `GREY`, `LIGHT_RED`, `LIGHT_BLUE`, `LIGHT_YELLOW`, `LIGHT_PURPLE`, `DARK_RED`, `BLUE_GREY`, `DARK_BLUE`, `LIME`, `OLIVE`, `GOLD`, `SILVER`, `MAGENTA`.

---

### List of Available Variables:
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

## TO-DO  
The list of upcoming tasks and features has been moved to a dedicated file. You can find it here: [TO-DO.md](TODO.md)  

# **Contributions**  
I sincerely appreciate all contributions that help improve this project.  

You may submit pull requests directly to the `main` branch or create a new feature-specific branch.  
Please ensure that your commits include clear and detailed explanations of the changes made to facilitate understanding and review.

---