## v2.0.0 Complete Rework
- Completely reworked the entire mod so it now works better and more stable.  
- Added SpawnItem Command that can be used anywhere and also can spawn unreleased Items.  
- Added IsEditor Patch to enable DebugMenu (can be turned off in the Config)  
- Added /revive command to revive all players (will try to add specific players later on)  
- Added /getitems to list all items  
- Added /setmoney to set the money instead of just infinite money  
- Added /spwobj to spawn Map Objects such as Landmines, Turrets and the Spike Trap, you need to load into any moon that has those first tho (for example Titan)    
- Added /spawntruck to spawn in the Truck (more truck commands planned)  
- Made /speed or /speedhack to be able to set speed and jump force  
- Added /term to send single commands to the terminal instead of the weird Terminal mode that was in there  
- Added Dependency to taffyko's NiceChat, because this mod works a lot better with that (not required)  
- Improved /help command to also include detailed info with /help \[commandname\]  
- idk probably forgot stuff, i changed a lot  
- Please report Issues in the linked GitHub Page. Im very unsure about CoHost and Networking stuff, that probably doesn't work as i can't test those rn.  

## v1.1.92 Fixed Godmode  
- MouthDog and ForestGiant now don't break interactability when godmode is enabled  

## v1.1.91 Update for newer version  
- just some simple stuff updated  

## v1.1.9 Hotfix for /pos  
- fixed some /pos stuff not working  

## v1.1.8 Added toggle for Spawn override
- added a config setting to turn on or off overriding spawns
- added command /override or /ovr to toggle overriding enemy spawns
- added ability to spawn mines or turrets using /spawnmine (not fully tested yet)
- added ability to use coordinates when using p=. FE: /spweny p=3,2,1   
- added ability to check own position using /pos or /position  
- fixed some playernames not being recognized right by p=@

## v1.1.7 Fix NonHost not loading
- fixed Hostcheck when loading level

## v1.1.6 Fix @me argument for p=@me
- remember using p=@me and not just @me for position

## v1.1.5 Hotfix because i broke something in 1.1.4
- hotfix for nonhost commands

## v1.1.4 Trying to fix some Host - NonHost issues
- trying to fix some prefix issues
- trying to fix some communication and chat visibility issues
- added ability to give host commands to specific users using /hostcmd "playername" or /cohost "playername"
- added config entry disable sending the commands to clients (for host only)

## v1.1.3 Fixed needing lowercase for spwscr
- fixed lowercase issue

## v1.1.2 Important fixes to Spawn functions
- fixed Inside enemies not being the right ones
- fixed scrap spawning issues with error being displayed even if everything worked


## v1.1.1 Improved Host - NonHost communication
- also fixed some prefix stuff

## v1.1.0 Stability and working improvements
- improved stuff to work better


## v1.0.1 Hotfixes and Improvements
- fixed lots of stuff
- added better NonHost to Host communication
- fixed jumpforce when disabling speedhack
- fixed p=@playername command not spawning in the right position

## v1.0.0 Release
- Release

</details>