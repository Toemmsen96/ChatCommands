# ChatCommands
This mod adds some chat commands used with a / by default.  
## Donate  
If you want to donate, to support making mods like this one:  
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I2I1SFBR7)

## Disclaimers
with "/" as the default prefix this mod may be incompatible with other mods that use this prefix like for example the GameMaster mod.
you can change the prefix to anything you want, so if your keyboard is maybe incompatible or anything, try changing the prefix to something like for example cmd: in the config file of the mod. just make sure you dont leave a space between your set prefix and the command itself.  
If you encounter any issues, or you have suggestions, please open an Issue in the linked GitHub page.  

## Install
To install either use a mod Manager or just drag the ChatCommands.dll into your BepInEx/plugins folder.  
### Recommendation
Install NiceChat Plugin alongside this, provides nicer Navigation etc to the chat and a better layout.  

## Commands  
- Spawn Enemy: Format: /spawnenemy [enemyname] ([p=position]) ([a=amount]) ([s=state]) - Alt: /spweny [enemyname] ([a=amount]) ([s=state]) ([p=position]) Description: Spawns an enemy at the specified location. Either use the player's position or specify a position. Position, state and amount are optional. Use them with the following format: position=@(playername/me) amount=(number) state=(alive/dead), For Host only  
  
Set Custom Deadline: Format: /deadline [days] - Alt: /dl [days]
Description: Sets a custom deadline for the game. If no argument is provided, the custom deadline will be toggled., For Host only  
  
Spawn Scrap: Format: /spawnscrap [scrapname] ([p=position]) ([a=amount]) ([v=value]) - Alt: /spwscr [scrapname] ([a=amount]) ([v=value]) ([p=position])
Description: Spawns Scrap at the specified location. Either use the player's position or specify a position. Position, value and amount are optional. Use them with the following format: position=@(playername/me) amount=(number) value=(number), For Host only
  
Teleport: Format: /teleport ([position=(random/@<playername>/@me/Vector3)]) - Alt: /tp ([position=(random/@<playername>/@me/Vector3)])
Description: Teleport to where you want. Use the following format: position=@(playername/me) or position=(random) or without arguments to teleport to the terminal., Host and Client
  
View Help: Format: /help ([command]) - Alt: /h ([command])  
Description: Displays a list of available commands with their format. Use <color=#00FFFF>/help [command]</color> to get more information about a specific command., Host and Client  
  
Toggle Lights: Format: /togglelights - Alt: /toglig  
Description: Toggles the lights on and off., For Host only  
  
Get Spawnable Enemies: Format: /getenemies - Alt: /enemies  
Description: Gets the spawnable enemies for the current level., For Host only  
  
Buy Stuff: Format: /buy [itemname] - Alt: /buyitem [itemname]  
Description: Buy stuff from the shop. Gets delivered via DropShip. (old function, you better use /term buy [itemname] instead) For Host only  
  
Change Weather: Format: /changeweather [weathername] - Alt: /chwe [weathername]  
Description: Change Weather to a specific type., For Host only  
  
Spawn Item: Format: /spawnitem [itemname] ([p=position]) ([a=amount]) ([v=value]) - Alt: /spwitm [itemname] ([a=amount]) ([p=position]) ([v=value])  
Description: Spawns Items at a specified position or at a random position. Args are optional, use them like this: p=@me a=5 v=1234.  
Note: Value doesnt work for everything., For Host only  
  
Get Spawnable Enemies: Format: /getenemies - Alt: /enemies  
Description: Gets the spawnable enemies for the current level., For Host only  
  
Get Spawnable Scrap Items: Format: /getscrap - Alt: /scrap  
Description: Gets the spawnable scrap items for the current level., For Host only  
  
Give Co-Host to a player: Format: /cohost [playername] - Alt: /hostcmd [playername]  
Description: Gives co-host to a player, which allows them to use host commands, when you turned them off for everyone else., For Host only  
  
Toggle God Mode: Format: /godmode - Alt: /god  
Description: Toggles if invincibility is enabled., Host and Client  
  
Get Alive Players: Format: /getalive - Alt: /getap  
Description: Returns a list of all players and if they are alive or dead., For Host only  
  
Revive all players: Format: /revive - Alt: /rev  
Description: Revives everyone., For Host only  
  
Speed Hack: Format: /speed ([speed]) ([jumpforce]) - Alt: /speedhack ([speed]) ([jumpforce])  
Description: Toggles Speed Hack, if speed is provided it will set the speed to that value.  
If jump force is provided it will set the jump force to that value., Host and Client  
  
Spawn Truck: Format: /spawntruck ([p=position]) - Alt: /spwtrk ([p=position])  
Description: Spawns the Truck, either at the default position or at a specified position., For Host only  
  
Get ALL Spawnable Items: Format: /getitems - Alt: /items  
Description: Gets the spawnable items you can spawn with /spawnitem or /spwitm., Host and Client  
  
Infinite Ammo: Format: /infammo - Alt: /ammo  
Description: Toggle Infinite Ammo., Host and Client  
  
Get your current Position: Format: /getposition - Alt: /getpos  
Description: Returns your current position., Host and Client  
  
Set Money: Format: /setmoney ([value]) - Alt: /money ([value])  
Description: Set Terminal Money to defined value, or without value to toggle infinite money., For Host only  

Spawn Map Object: Format: /spawnobject [objectname] ([p=position]) ([a=amount]) - Alt: /spwobj [objectname] ([a=amount]) ([p=position])  
Description: Spawns Map Object at the specified location. Either use the player's position or specify a position. Position and amount are optional. Use them with the following format: position=@(playername/me) amount=(number), For Host only  
  
Send Terminal Command: Format: /term [terminalcommand] - Alt: /terminal [terminalcommand]  
Description: Send a terminal commmand to the terminal and receive a response., Host and Client  
  
Toggle Host Commands: Format: /togglehostcmd - Alt: /thcmd  
Description: Toggle if connecting clients can use host commands., For Host only  
  
Toggle Overrride Spawns: Format: /override - Alt: /ovr  
Description: Toggles if Monster Spawns are overriden or not. This affects how many monsters spawn with the spawn command and natural spawns., For Host only  
  
View Credits: Format: /getcredits - Alt: /credits  
Description: Shows the credits for the ChatCommands mod., Host and Client  

## Config
- Customize Prefix -> default: /
- Display Messages as Popup -> Determines if chat messages are displayed as popup messages. default: false
- Has to be Host -> (for server host only): determines if clients can also use the host commands. default: true
- Send Host Commands -> (for server host only): determines if commands get sent to the clients, so for example god mode is enabled for them too. default: true
- Override Spawns -> (for server host only): determines if the spawn command overrides the default spawns. If enabled there can be spawned more than one girl etc. Can be toggled ingame by using /override command. default: false
- Enable Debug Mode -> Enables Unity Debug mode and with it the Debug modmenu ingame. default: true
- Log To Chat -> Enables logging to (local) chat. default: false  

### Legend:  
[arg] -> argument for command you have to enter (without the [])  
([arg]) -> optional argument for a command you can enter (without ([]) around it)  
arg -> placeholder for stuff like position, amount, value, playername etc. in this example  
#### Disclaimer for Spawn commands and Position arguments:
- Use for example like this: /spwitem cog v=1232 p=@me a=3 -> Spawns 3 "Cog" Items with the value 1232 at your location.


## Additional Credits:  
Toemmsen96 - Making most of the mod  
Chrigi - old Spawn Scrap and gun functionality with position parameters and stuff  
GameMaster Team - some parts of the code that were used as a base  
