
# EscapeFromTarkov-Trainer

[![Sponsor](https://img.shields.io/badge/sponsor-%E2%9D%A4-lightgrey?logo=github&style=flat-square)](https://github.com/sponsors/sailro)

*I'm not responsible for any consequences that result from using this code. BattleState / BattlEye will ban you if you try to use it 'live'.*

***TLDR => Use the [Universal Installer](https://github.com/sailro/EscapeFromTarkov-Trainer/releases).*** Default key for in-game GUI is `Right-Alt`.

This is an attempt -for educational purposes only- to alter a Unity game at runtime without patching the binaries (so without using [Cecil](https://github.com/jbevain/cecil) nor [Reflexil](https://github.com/sailro/reflexil)).

`master` branch can build against `EFT 0.13.0.23399` (tested with [`spt-aki Version 3.5.8`](https://hub.sp-tarkov.com/files/file/16-spt-aki/#versions)). If you are looking for another version, see [`branches`](https://github.com/sailro/EscapeFromTarkov-Trainer/branches) and [`releases`](https://github.com/sailro/EscapeFromTarkov-Trainer/releases).

> If you want to compile the code yourself, make sure you cleaned-up your solution properly after upgrading your EFT/sptarkov bits (even removing `bin` and `obj` folders) and check all your references.

> If you are using `SPT-AKI`, please make sure you have run the game at least once before compiling/installing the trainer. `SPT-AKI` is patching binaries during the first run, and we need to compile against those patched binaries.

> The typical issue when something is out of sync is that the game will freeze at the startup screen with type/tokens errors in `%LOCALAPPDATA%Low\Battlestate Games\EscapeFromTarkov\Player.log`

## Features

| trainer.ini section       | GUI/console  | Description |
|---------------------------|--------------|-------------|
| `Aimbot`                  | `aimbot`     | Aimbot (distance, smoothness, silent aim with speed factor and shot delay, fov radius, fov circle). |
| `Ammunition`              | `ammo`       | Unlimited ammo. |
| `AutomaticGun`            | `autogun`    | Force all guns (even bolt action guns) to use automatic firing mode with customizable fire rate. |
| `Commands`                | `commands`   | Popup window to enable/disable all features (use right-alt or setup your own key in [trainer.ini](#sample-trainerini-configuration-file)). |
| `CrossHair`               | `crosshair`  | Crosshair with customizable size, color, thickness and auto-hide feature when aiming. |
| `Examine`                 | `examine`    | All items already examined. |
| `ExfiltrationPoints`      | `exfil`      | Exfiltration points with customizable colors given eligibility, status filter, distance. |
| `FovChanger`              | `fovchanger` | Change Field Of View (FOV). |
| `FreeCamera`              | `camera`     | Free camera with fast mode and teleportation. |
| `Grenades`                | `grenade`    | Grenades outline. |
| `Health`                  | `health`     | Full health, prevent any damage (so even when falling), keep energy and hydration at maximum. |
| `Hits`                    | `hits`       | Hit markers (hit, armor, health with configurable colors). |
| `Hud`                     | `hud`        | HUD (compass, ammo left in chamber / magazine, fire mode, coordinates). |
| `LootableContainers`      | `stash`      | Hidden stashes like buried barrels or ground caches. |
| `LootItems`               | `loot`       | List all lootable items and track any item by name or rarity or in-game wishlist in raid (even in containers and corpses), with prices. |
| `Map`                     | `map`        | Full screen 3D map with radar esp. |
| `NightVision`             | `night`      | Night vision. |
| `NoCollision`             | `nocoll`     | No physical collisions, making you immune to bullets, grenades and barbed wires. |
| `NoMalfunctions`          | `nomal`      | No weapon malfunctions: no misfires or failures to eject or feed. No jammed bolts or overheating. |
| `NoRecoil`                | `norecoil`   | No recoil. |
| `NoSway`                  | `nosway`     | No sway. |
| `NoVisor`                 | `novisor`    | No visor, so even when using a face shield-visor you won't see it. |
| `Players`                 | `wallhack`   | Wallhack (you'll see Bear/Boss/Cultist/Scav/Usec with configurable colors through walls). Charms, boxes, info (weapon and health), skeletons and distance. |
| `Quests`                  | `quest`      | Locations for taking/placing quest items. Only items related to your started quests are displayed. |
| `QuickThrow`              | `quickthrow` | Quick-throw grenades. |
| `Radar`                   | `radar`      | 2D radar. |
| `Skills`                  | `skills`     | All skills to Elite level (51) and all weapons mastering to level 3. |
| `Speed`                   | `speed`      | Speed boost to be able to go through walls/objects, or to move faster. Be careful to not kill yourself. |
| `Stamina`                 | `stamina`    | Unlimited stamina. |
| `ThermalVision`           | `thermal`    | Thermal vision. |
| `Train`                   | `train`      | Summon train on compatible maps like Reserve or Lighthouse. |
| `WallShoot`               | `wallshoot`  | Shoot through wall/helmet/vest/material with maximum penetration and minimal deviation/ricochet. |
| `Weather`                 | `weather`    | Clear weather. |
| `WorldInteractiveObjects` | `opener`     | Door/Keycard reader/Car unlocker. |

You can Load/Save all settings using the `console` or the `GUI`.

![Wallhack](https://user-images.githubusercontent.com/638167/222186879-a88a267e-16ba-4532-85ec-8cb385737947.png)
![Radar](https://user-images.githubusercontent.com/638167/222524208-589dc7ff-f053-4b0c-902b-49fa8d1f7ddd.png)
![Map](https://user-images.githubusercontent.com/769465/224330696-d09960a2-8940-4980-8489-0533b44534f9.png)
![Exfils](https://user-images.githubusercontent.com/638167/135586735-143ab160-ca20-4ec9-8ad4-9ce7bde58295.png)
![Loot](https://user-images.githubusercontent.com/638167/135587083-938a3d9b-2082-4231-9fa8-e7807ad4a3d1.png)
![Quests](https://user-images.githubusercontent.com/638167/121975175-d8d91c00-cd35-11eb-86cd-6b49360fe370.png)
![Stashes](https://user-images.githubusercontent.com/638167/135586933-4cf57740-aff2-47c8-9cec-94e2eb062dd0.png)
![Track](https://user-images.githubusercontent.com/638167/222189119-f25413a7-511b-43cf-b6d8-a57320347034.png)
![NightVision](https://user-images.githubusercontent.com/638167/135586268-c175e999-a60d-40db-9960-06cdf5fe27d7.png)
![Popup](https://user-images.githubusercontent.com/638167/222188079-3ddb81e2-fb4c-446d-8716-5f54a40ad01b.png)

## Easy and automatic installation

Simply use the [Universal Installer](https://github.com/sailro/EscapeFromTarkov-Trainer/releases).

## Manual installation 

You can try to compile the code yourself (you will need a recent Visual Studio, because we are using CSharp 9). You can use a precompiled release as well.

Copy all files in your EFT directory like `C:\Battlestate Games\EFT`:

- `EscapeFromTarkov_Data\Managed\NLog.EFT.Trainer.dll` (this is the compiled code for the trainer)
- `EscapeFromTarkov_Data\outline` (this is the dedicated shader we use to outline players [wallhack])

### If you are using the Live version (you should NOT do that, you'll be detected and banned):

Rename `EscapeFromTarkov_Data\Managed\NLog.dll.nlog-live` to `NLog.dll.nlog`. This will work only for legacy versions. Given EscapeFromTarkov `0.13.0.21531` or later prevent this trainer to be loaded using NLog configuration. It is now mandatory to use SPT-AKI/BepInEx for recent versions.

### If you are using sptarkov (https://www.sp-tarkov.com):

Overwrite the existing `EscapeFromTarkov_Data\Managed\NLog.dll.nlog` using `NLog.dll.nlog-sptarkov`, or update the existing file accordingly. We must include the following 
`<target name="EFTTarget" xsi:type="EFTTarget" />` in the `targets` section for the trainer to be loaded properly. This is for legacy versions before EscapeFromTarkov `0.13.0.21531`.

For newer versions, copy `aki-efttrainer.dll` (this is the compiled code for the BepInEx plugin) to `BepInEx\plugins`.

## Configuration

![console](https://user-images.githubusercontent.com/638167/149630825-7d76b102-0836-4eb9-a27f-d33fb519452f.png)

This trainer hooks into the command system, so you can easily setup features using the built-in console:

| Command    | Values              | Default | Description                         |
|------------|---------------------|---------|-------------------------------------|
| ammo       | `on` or `off`       | `off`   | Enable/Disable unlimited ammo       |
| autogun    | `on` or `off`       | `off`   | Enable/Disable automatic gun mode   |
| crosshair  | `on` or `off`       | `off`   | Show/Hide crosshair                 |
| dump       |                     |         | Dump game state for analysis        |
| examine    | `on` or `off`       | `off`   | Enable/Disable all item examined    |
| exfil      | `on` or `off`       | `on`    | Show/Hide exfiltration points       |
| fovchanger | `on` or `off`       | `off`   | Change FOV value                    |
| grenade    | `on` or `off`       | `off`   | Show/Hide grenades                  |
| health     | `on` or `off`       | `off`   | Enable/Disable full health          |
| hits       | `on` or `off`       | `off`   | Show/Hide hit markers               |
| hud        | `on` or `off`       | `on`    | Show/Hide hud                       |
| list       | `[name]` or `*`     |         | List lootable items                 |
| listr      | `[name]` or `*`     |         | List only rare lootable items       |
| listsr     | `[name]` or `*`     |         | List only super rare lootable items |
| load       |                     |         | Load settings from `trainer.ini`    |
| loadtl     | `[filename]`        |         | Load current tracklist from file    |
| loot       | `on` or `off`       |         | Show/Hide tracked items             |
| night      | `on` or `off`       | `off`   | Enable/Disable night vision         |
| nocoll     | `on` or `off`       | `off`   | Disable/Enable physical collisions  |
| nomal      | `on` or `off`       | `off`   | Disable/Enable weapon malfunctions  |
| norecoil   | `on` or `off`       | `off`   | Disable/Enable recoil               |
| nosway     | `on` or `off`       | `off`   | Disable/Enable sway                 |
| novisor    | `on` or `off`       | `off`   | Disable/Enable visor                |
| quest      | `on` or `off`       | `off`   | Show/Hide quest POI                 |
| radar      | `on` or `off`       | `off`   | Show/Hide radar                     |
| save       |                     |         | Save settings to `trainer.ini`      |
| savetl     | `[filename]`        |         | Save current tracklist to file      |
| stamina    | `on` or `off`       | `off`   | Enable/Disable unlimited stamina    |
| stash      | `on` or `off`       | `off`   | Show/Hide stashes                   |
| status     |                     |         | Show status of all features         |
| thermal    | `on` or `off`       | `off`   | Enable/Disable thermal vision       |
| track      | `[name]` or `*`     |         | Track all items matching `name`     |
| track      | `[name]` `<color>`  |         | Ex: track `roler` `red`             |
| track      | `[name]` `<rgba>`   |         | Ex: track `roler` `[1,1,1,0.5]`     |
| trackr     | same as `track`     |         | Track rare items only               |
| tracksr    | same as `track`     |         | Track super rare items only         |
| tracklist  |                     |         | Show tracked items                  |
| untrack    | `[name]` or `*`     |         | Untrack a `name` or `*` for all     |
| wallhack   | `on` or `off`       | `on`    | Show/hide players                   |
| wallshoot  | `on` or `off`       | `on`    | Enable/Disable shoot through walls  |
