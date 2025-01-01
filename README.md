# AutomaticAds CS2

This plugin allows you to schedule and display announcements in the chat at customizable intervals. Each announcement is accompanied by a brief sound effect to capture players' attention seamlessly.

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
| `PlaySoundName`  | Sound played when an announcement is sent. Leave it blank to disable.                             | **NO**   |
| `Ads`            | List of advertisements to be sent. Each ad can be configured individually (see example below).    | **YES**  |

---

### Ads Configuration
Each item in the `Ads` list represents a single advertisement. Here are the fields available:

| Parameter       | Description                                                                                         | Required |
|-----------------|-----------------------------------------------------------------------------------------------------|----------|
| `message`       | The message/announcement to send in the chat. Supports colors.                                      | **YES**  |
| `flag`          | Admin flag required to receive this message. Set to `all` to make it available to all players.     | **YES**   |
| `map`           | The map where this ad should appear. Use `"all"` for all maps or specify a map name.                | **YES**   |
| `interval`      | The interval (in seconds) between sending this ad. Must be between `10` and `3600`.                 | **YES**  |
| `disableSound`  | If `true`, no sound will be played when this ad is sent.                                            | **YES**   |

---

## Configuration Example
Here is an example configuration file:

```json
{
  "ChatPrefix": " [{GREEN}AutomaticAds{WHITE}]{WHITE}",
  "PlaySoundName": "ui/panorama/popup_reveal_01",
  "Ads": [
    {
      "message": "{RED}AutomaticAds is the best plugin!",
      "flag": "all",
      "map": "all",
      "interval": 600,
      "disableSound": false
    },
    {
      "message": "{BLUE}Welcome to the server! {RED}Make sure to read the rules.",
      "flag": "all",
      "map": "all",
      "interval": 800,
      "disableSound": false
    },
    {
      "message": "{BLUE}Thank you for supporting the server! {GOLD}Your contribution is greatly appreciated.",
      "flag": "@css/vip",
      "map": "all",
      "interval": 1000,
      "disableSound": true
    },
    {
      "message": "{GOLD}Congratulations, you are playing on Mirage.",
      "flag": "all",
      "map": "de_mirage",
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
| Task                               | Status     | Description                                                                                     |
|------------------------------------|------------|-------------------------------------------------------------------------------------------------|
| Multiple ads with different intervals | **Complete**    | Configure multiple advertisements with varying intervals.                                       |
| Option to target specific flags    | Pending    | Add an option to send messages only to users with specific admin flags.                         |

---