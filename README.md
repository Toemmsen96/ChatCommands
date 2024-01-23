# ChatCommands
This mod adds some chat commands used with a / by default.  

## Disclaimers
with "/" as the default prefix this mod may be incompatible with other mods that use this prefix like for example the GameMaster mod.
you can change the prefix to anything you want, so if your keyboard is maybe incompatible or anything, try changing the prefix to something like for example cmd: in the config file of the mod. just make sure you dont leave a space between your set prefix and the command itself.  
If you encounter any issues, or you have suggestions, please open an Issue in the linked GitHub page.  

## Install
To install either use a mod Manager or just drag thr ChatCommands.dll into your BepInEx/plugins folder.  

## NonHost functionality
This can be used as Host and also as NonHost if the Host has the mod and enabled nonhost commands in the Config file.  
The config file gets created on first startup of the mod. The setting is turned on by default  
Infocommands can always be used as a NonHost, these are:  
/enemies -> lists spawnable enemies on current map (currently only for host, will try to fix for nonhost)  
  
/getscrap -> lists spawnable scrap on curent map (currently only for host, will try to fix for nonhost)  
  
/spawn -> lists what spawn commands you can use  
  
/help -> see what commands you can use  
  
/morehelp -> more commands to use get listed  
  
/cheats -> list cheat commands  
  
/credits -> list credits for mod  
  
## Host functionality
Host only commands to use (require Host to have setting enabled if wanted to be used as NonHost):  
/spawnscrap or /spwscr "scrapname" (a="amount") (p="position") (v="value") -> spawn scrap, amount, position and value arguments are optional  
position can be random, @me or @"playername",  
amount sets how many items will be spawned (without it defaults to 1),  
value sets how much it is worth (default:1000)  
with gun as scrapname you can spawn a shotgun (gun is broken at the moment, please dont use for now)  
  
/spawnenemy or /spweny "enemyname" (a="amount") (p="position") -> spawn enemy, position can be random, @me or @"playername"  
  
/infammo or /ammo -> enable infinite ammo on shotgun  
  
/speed -> toggles speed and jump hack for faster travelling  
  
/god -> toggles godmode  
  
/tp ("playername") -> teleport back to ship or if  stated to a player  
  
/buy "item" ("count") -> buy items from shop  
  
/money -> enables infinite money cheat  
  
/togglelights -> toggles lights of facility  
  
/weather -> change weather of current planet (not working properly rn)  
  
/dl "days" or /deadline "days" -> set amount of days until deadline, gets applied after quota is reached and new one is presented, so reaching the quota once is required. leaving blank after the command will reset to default  
  
/term -> use Terminal from anywhere. On exiting input you need to type /term again to enable walking again  
  
/hostcmd "playername" or /cohost "playername" -> give host commands to specific users if you turned off hostcommands for everyone in the cfg  

/togglehostcmd or /thcmd -> toggle allowing host commands for everyone  
  
/override or /ovr -> toggle overriding enemy spawns. If set to true there can be more than one girl spawned automatically etc.  
If set to false, enemies spawned using /spawnenemy might not spawn. Default can be set in cfg.  

### Legend:  
"arg" -> argument for command you have to enter (without the "")  
(x="arg") -> optional argument you have to write with the x= in front and without the () so it knows what type of argument it is  


## Additional Credits:  
Toemmsen96 - Making most of the mod  
Chrigi - Spawn Scrap and gun functionality with position parameters and stuff  
GameMaster Team - some parts of the code that were used as a base  
## Donate  
If you want to donate, to support making mods like this one:
https://ko-fi.com/ctmods
