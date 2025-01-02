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

| Parameter        | Description                                                                                       | Required |
|------------------|---------------------------------------------------------------------------------------------------|----------|
| `ChatPrefix`     | Prefix displayed in the chat before each announcement. Supports colors.                          | **YES**  |
| `PlaySoundName`  | Sound played when an announcement is sent. Leave it blank to disable.                             | **YES**   |
| `sendAdsInOrder`  | Send announcements in an orderly manner, respecting the intervals.                             | **YES**   |
| `Ads`            | List of advertisements to be sent. Each ad can be configured individually (see example below).    | **YES**  |

---

### Ads Configuration
Each item in the `Ads` list represents a single advertisement. Here are the fields available:

| Parameter       | Description                                                                                         | Required |
|-----------------|-----------------------------------------------------------------------------------------------------|----------|
| `message`       | The message/announcement to send in the chat. Supports colors.                                      | **YES**  |
| `interval`      | The interval **(in seconds)** between sending this ad. Must be between `10` and `3600` ***(if you don't add it to the announce configuration it will be set to `600` by default)***.                 | **NO**  |
| `viewFlag`          | Flag required to view the message. Set it to `“all”` to make it available to all players ***(if you don't add it to the announce settings it will be set to `“all”` by default)***.     | **NO**   |
| `excludeFlag`          | Users with this flag will not see the message. Set it to `“”` so that no players are excluded ***(if you do not add it to the announce settings it will be set to `“”` by default)***.     | **NO**   |
| `map`           | The map where this announce should appear. Use `“all”` to show it on all maps or specify a map name ***(if you don't add it to the announce configuration it will be set to `“all”` by default)***.                | **NO**   |
| `disableSound`  | If `true`, no sound will be played when this ad is sent (if you don't add it to the announce configuration it will be set to `false` by default).                                            | **NO**   |

---

## Configuration Example
Here is an example configuration file:
```json
{
  "ChatPrefix": " [{GREEN}AutomaticAds{WHITE}]{WHITE}",
  "PlaySoundName": "ui/panorama/popup_reveal_01",
  "sendAdsInOrder": true,
  "Ads": [
    {
      "message": "{RED}AutomaticAds is the best plugin!",
      "viewFlag": "all",
      "excludeFlag": "@css/vip",
      "map": "all",
      "interval": 600,
      "disableSound": false
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
      "message": "{GOLD}Congratulations, you are playing on {map}.",
      "excludeFlag": "@css/vip",
      "interval": 1400,
      "disableSound": true
    }
  ],
  "ConfigVersion": 1
}
```

### List of Available Colors:
`GREEN`, `RED`, `YELLOW`, `BLUE`, `ORANGE`, `WHITE`, `PURPLE`, `GREY`, `LIGHT_RED`, `LIGHT_BLUE`, `LIGHT_YELLOW`, `LIGHT_PURPLE`, `DARK_RED`, `BLUE_GREY`, `DARK_BLUE`, `LIME`, `OLIVE`, `GOLD`, `SILVER`, `MAGENTA`.

---

## TO-DO
| Task                                 | Status       | Description                                                                                     | Priority   |
|--------------------------------------|--------------|-------------------------------------------------------------------------------------------------|------------|
| Multiple ads with different intervals | **Complete** | Configure multiple advertisements with varying intervals.                                       | High       |
| Option to target specific flags      | **Complete** | Add an option to send messages only to users with specific admin flags.                         | Medium     |
| Option to target specific maps       | **Complete** | Add an option to send messages only on specific maps.                                           | Medium     |
| Option to disable sound              | **Complete** | Add an option to disable sound for announcements.                                               | Low        |
| Line breaks in messages              | **Complete** | Support for line breaks in messages ***(e.g., for displaying multiple lines of text)***.               | Low        |
| Advertisements order configuration   | **Complete** | Add configuration to toggle between displaying configured ads in order or randomly.            | High       |
| Exclude players with a certain flag  | **Complete**      | Add an option to send messages to everyone with access except players with a certain flag, by setting the flag in the message **(excludeflag)**. | Medium     |
| Support for server variables         | **Complete**      | Allow the use of server variables to retrieve information such as IP, HOSTNAME, MAP, TIME, DATE, PLAYERS, MAXPLAYERS. | High     |
| Support for changing message method  | Pending      | Add support for sending messages via chat, HTML Center, or Panel, allowing users to choose the method for each message. | Medium     |
| Multi-language ads                   | Pending      | Allow users to configure their ad language and support ad configuration in multiple languages.  | Medium     |
| Welcome message                      | Pending      | Configure a welcome message to be sent when a player connects to the server **(OnPlayerConnectFull event)**. | Low       |

---
