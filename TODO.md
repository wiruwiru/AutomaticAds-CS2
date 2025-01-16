## TO-DO List
| **Task**                              | **Status**     | **Description**                                                                                     | **Priority** |
|---------------------------------------|----------------|-----------------------------------------------------------------------------------------------------|--------------|
| Multiple ads with different intervals | **Complete**   | Configure multiple advertisements with varying intervals.                                          | High         |
| Advertisements order configuration    | **Complete**   | Add configuration to toggle between displaying configured ads in order or randomly.               | High         |
| Support for server variables          | **Complete**   | Allow the use of server variables to retrieve information such as IP, HOSTNAME, MAP, TIME, DATE, PLAYERS, MAXPLAYERS. | High         |
| Option to target specific flags       | **Complete**   | Add an option to send messages only to users with specific admin flags.                            | Medium       |
| Option to target specific maps        | **Complete**   | Add an option to send messages only on specific maps.                                              | Medium       |
| Exclude players with a certain flag   | **Complete**   | Add an option to send messages to everyone except players with a certain flag (use **excludeflag**). | Medium       |
| Option to disable sound               | **Complete**   | Add an option to disable sound for announcements.                                                  | Low          |
| Line breaks in messages               | **Complete**   | Support for line breaks in messages ***(e.g., for displaying multiple lines of text)***.          | Low          |
| Welcome message                       | **Complete**   | Configure a welcome message sent when a player connects to the server **(OnPlayerConnectFull event)**. | Low         |
| PlayerName variable                   | **Complete**   | Add the `{PlayerName}` variable to personalize advertisements by including the player's name.       | Low          |
| Reload Plugin                         | **Complete**   | Add command to reload plugin for easier management.                                                | Low          |
| Common commands shortcut              | **Complete**   | Add shortcut commands like `!help` to display server commands and `!dc` to share the Discord link.  | High         |
| {prefix} as an available variable     | **Pending**    | Add `{prefix}` as an available variable to improve flexibility in messages.                        | Medium       |
| Improved triggerAd functionality      | **Pending**    | Add an option for triggerAds without intervals for specific scenarios.       | Medium       |
| Connect & disconnect messages         | **Pending**    | Add messages for connect/disconnect events (e.g., `PLAYER XXX (STEAMID64) connected from (COUNTRY)`). | Medium          |
| Support for changing message method   | **Pending**    | Add support for sending messages via chat, HTML Center, or Panel, allowing users to choose.        | Medium       |
| Server/Client Commands in triggerAds  | **Pending**    | Add support for triggerAds to execute server or client commands. | Medium       |
| Multi-language ads                    | **Pending**    | Allow users to configure ad language and support ads in multiple languages.                        | Low       |