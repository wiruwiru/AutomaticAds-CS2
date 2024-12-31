# AutomaticAds CS2
This plugin allows you to schedule and display announcements in the chat at customizable intervals. Each announcement is accompanied by a brief sound effect to capture players' attention seamlessly."

https://github.com/user-attachments/assets/aae16cc4-7c67-477a-8c89-437d5c035211

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)

2. Download [AutomaticAds.zip](https://github.com/wiruwiru/AutomaticAds-CS2/releases) from the releases section.

3. Unzip the archive and upload it to the game server

4. Start the server and wait for the configuration file to be generated.

5. Complete the configuration file with the parameters of your choice.

---

# Commands
`!announce` `css_announce`  - You can use it to force the ad to be sent (Recommended for administrators).

---

# Config
| Parameter | Description | Required     |
| :------- | :------- | :------- |
| `ChatPrefix` | This is the prefix that will appear in the chat when an advertisement is sent. |**YES** |
| `ChatMessage` | This is the message/announcement that will be sent automatically by the plugin. |**YES** |
| `AdminFlag` | Flag to allow access to “css_announce”.  |**YES** |
| `ForceCommand` | You can choose the command you will use to force the sending of an announcement, for example “css_announce” or “css_anuncio”. |**YES** |
| `SendForceCommand` | This is the message that will confirm that you have forced the ad to be sent. |**YES** |
| `NoPermissions` | Message that will appear when someone without permissions tries to use the force command. |**YES** |
| `ChatInterval` | Interval for the announcement to be sent (in seconds). |**YES** |
| `PlaySoundName` | Sound that is played when sending the ad (if you don't know what you are doing leave it default), you can deactivate this function leaving the configuration empty ( example: “PlaySoundName”: “” ) |**YES** |

---

## Configuration example
```
{
  "ChatPrefix": " [{GREEN}AutomaticAds]",
  "ChatMessage": "{RED}AutomaticAds is the best plugin!",
  "AdminFlag": "css_root",
  "ForceCommand": "css_announce",
  "SendForceCommand": "{RED}You have successfully forced the ad to be sent."
  "NoPermissions": "{RED}You do not have permissions to use this command."
  "ChatInterval": 600,
  "PlaySoundName": "ui/panorama/popup_reveal_01",
  "ConfigVersion": 1
}
```
List of available colors:
`GREEN` `RED` `YELLOW` `BLUE` `ORANGE` `WHITE` `PURPLE` `GREY` `LIGHT_RED` `LIGHT_BLUE` `LIGHT_YELLOW` `LIGHT_PURPLE` `DARK_RED` 
`BLUE_GREY` `DARK_BLUE` `LIME` `OLIVE` `GOLD` `SILVER` `MAGENTA`

---

## TO-DO
| Task                          | Status     | Description                                                                                              |
|-------------------------------|------------|----------------------------------------------------------------------------------------------------------|
| Multiple ads with different intervals | Pending    | Add functionality to configure multiple advertisements with varying time intervals.                      |
| Option to send a message only to users with a certain flag | Pending    | Implement an option to specifically target messages to users with specific flags.                            |
