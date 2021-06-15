
# EscapeFromTarkov-Trainer

*I'm not responsible for any consequences that result from using this code. BattleState / BattlEye will ban you if you try to use it 'live'.*

This is an attempt -for educational purposes only- to alter a Unity game at runtime without patching the binaries (so without using [Cecil](https://github.com/jbevain/cecil) nor [Reflexil](https://github.com/sailro/reflexil)).

`master` branch can build against `EFT 0.12.10.12646`. If you are looking for another version, see [`branches`](https://github.com/sailro/EscapeFromTarkov-Trainer/branches) and [`releases`](https://github.com/sailro/EscapeFromTarkov-Trainer/releases).

> If you want to compile the code yourself, make sure you clean up your solution properly after upgrading your EFT/sptarkov bits (even removing `bin` and `obj` folders) and check all your references.
> The typical issue when something is out of sync is that EFT get stuck at the loading screen with type/tokens errors in `%LOCALAPPDATA%Low\Battlestate Games\EscapeFromTarkov\output_log.txt`

## Features

This trainer gives:
- Aimbot (hold `/` -not the keypad one-, or setup your own key in [trainer.ini](#sample-trainerini-configuration-file)).
- HUD (ammo left in chamber / magazine, fire mode).
- Door unlocker (use keypad-period, or setup your own key in [trainer.ini](#sample-trainerini-configuration-file)).
- Wallhack (you'll see players / bots / bosses with distinct colors through walls).
- Exfiltration points (green for available points, yellow for others).
- No recoil (off by default).
- Locations for taking/placing quest items (off by default). Only items related to your started quests are displayed.
- Hidden stashes like buried barrels or ground caches (off by default).
- Ability to list all lootable items and to track any item by name (even in containers).
- Unlimited stamina (off by default).
- Force all guns (even bolt action guns) to use automatic firing mode with customizable fire rate.
- Thermal and night visions (even combined).
- Crosshair (off by default) with customizable size, color, thickness and auto-hide feature when aiming.
- Grenades outline (off by default).
- No physical collisions, making you immune to bullets, grenades and barbed wires (off by default).
- Popup window to enable/disable all features (use right-alt or setup your own key in [trainer.ini](#sample-trainerini-configuration-file)).
- Load/Save all settings.

![Wallhack](https://user-images.githubusercontent.com/638167/121975075-a6c7ba00-cd35-11eb-8c39-ac27dde7356f.png)
![Exfils](https://user-images.githubusercontent.com/638167/121975112-b941f380-cd35-11eb-9a2c-3c541d933a62.png)
![Colors](https://user-images.githubusercontent.com/638167/121975147-c828a600-cd35-11eb-8782-75f5b63eede4.png)
![Quests](https://user-images.githubusercontent.com/638167/121975175-d8d91c00-cd35-11eb-86cd-6b49360fe370.png)
![Stashes](https://user-images.githubusercontent.com/638167/121975196-e42c4780-cd35-11eb-8a33-8130f2fadf88.png)
![Track](https://user-images.githubusercontent.com/638167/121975219-f6a68100-cd35-11eb-9d3a-36ffda3ca568.png)
![NightVision](https://user-images.githubusercontent.com/638167/121975256-1047c880-cd36-11eb-903a-213d5d187ad4.png)
![Popup](https://user-images.githubusercontent.com/638167/121975299-1f2e7b00-cd36-11eb-8f12-00de3e7515bd.png)

## Installation

You can try to compile the code yourself (you will need a recent Visual Studio, because we are using CSharp 9). You can use a precompiled release as well.

Copy all files in your EFT directory like `C:\Battlestate Games\EFT`:

- `EscapeFromTarkov_Data\Managed\NLog.EFT.Trainer.dll` (this is the compiled code for the trainer)
- `EscapeFromTarkov_Data\outline` (this is the dedicated shader we use to outline players [wallhack])

### If you are using the Live version (you should NOT do that, you'll be detected and banned):

Rename `EscapeFromTarkov_Data\Managed\NLog.dll.nlog-live` to `NLog.dll.nlog`

### If you are using sptarkov (https://www.sp-tarkov.com):

Overwrite the existing `EscapeFromTarkov_Data\Managed\NLog.dll.nlog` using `NLog.dll.nlog-sptarkov`, or update the existing file accordingly. We must include the following 
`<target name="EFTTarget" xsi:type="EFTTarget" />` in the `targets` section for the trainer to be loaded properly.

## Configuration

![console](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/console.jpg)

This trainer hooks into the command system, so you can easily setup features using the built-in console:

| Command   | Values              | Default | Description                         |
|-----------|---------------------|---------|-------------------------------------|
| autogun   | `on` or `off`       | `off`   | Enable/Disable automatic gun mode   |
| crosshair | `on` or `off`       | `off`   | Show/Hide crosshair                 |
| dump      |                     |         | Dump game state for analysis        |
| exfil     | `on` or `off`       | `on`    | Show/Hide exfiltration points       |
| grenade   | `on` or `off`       | `off`   | Show/Hide grenades                  |
| hud       | `on` or `off`       | `on`    | Show/Hide hud                       |
| list      | `<optional filter>` |         | List lootable items                 |
| listr     | `<optional filter>` |         | List only rare lootable items       |
| listsr    | `<optional filter>` |         | List only super rare lootable items |
| load      |                     |         | Load settings from `trainer.ini`    |
| loot      | `on` or `off`       |         | Show/Hide tracked items             |
| night     | `on` or `off`       | `off`   | Enable/Disable night vision         |
| nocoll    | `on` or `off`       | `off`   | Disable/Enable physical collisions  |
| norecoil  | `on` or `off`       | `off`   | Disable/Enable recoil               |
| quest     | `on` or `off`       | `off`   | Show/Hide quest POI                 |
| save      |                     |         | Save settings to `trainer.ini`      |
| stamina   | `on` or `off`       | `off`   | Enable/Disable unlimited stamina    |
| stash     | `on` or `off`       | `off`   | Show/Hide stashes                   |
| status    |                     |         | Show status of all features         |
| thermal   | `on` or `off`       | `off`   | Enable/Disable thermal vision       |
| track     | `[name]`            |         | Track all items matching `name`     |
| tracklist |                     |         | Show tracked items                  |
| untrack   | `[name]` or `*`     |         | Untrack a `name` or `*` for all     |
| wallhack  | `on` or `off`       | `on`    | Show/hide players (on next raid)    |

## Sample `trainer.ini` configuration file

Please note that there is no need to create this file by yourself. If you want to customize settings, use `save`, edit what you want then use `load`.
This file is located in `%USERPROFILE%\Documents\Escape from Tarkov\trainer.ini`.

```ini
; Be careful when updating this file :)
; For keys, use https://docs.unity3d.com/ScriptReference/KeyCode.html
; Colors are stored as an array of 'RGBA' floats

EFT.Trainer.Features.Aimbot.Key="Slash"
EFT.Trainer.Features.Aimbot.MaximumDistance=200.0

EFT.Trainer.Features.AutomaticGun.Enabled=false
EFT.Trainer.Features.AutomaticGun.Key="None"
EFT.Trainer.Features.AutomaticGun.Rate=500

EFT.Trainer.Features.Commands.Key="RightAlt"
EFT.Trainer.Features.Commands.X=20.0
EFT.Trainer.Features.Commands.Y=40.0

EFT.Trainer.Features.CrossHair.Color=[1.0,0.0,0.0,1.0]
EFT.Trainer.Features.CrossHair.Enabled=true
EFT.Trainer.Features.CrossHair.HideWhenAiming=true
EFT.Trainer.Features.CrossHair.Key="None"
EFT.Trainer.Features.CrossHair.Size=10.0
EFT.Trainer.Features.CrossHair.Thickness=2.0

EFT.Trainer.Features.Doors.Key="KeypadPeriod"

EFT.Trainer.Features.ExfiltrationPoints.CacheTimeInSec=7.0
EFT.Trainer.Features.ExfiltrationPoints.EligibleColor=[0.0,1.0,0.0,1.0]
EFT.Trainer.Features.ExfiltrationPoints.Enabled=true
EFT.Trainer.Features.ExfiltrationPoints.Key="F1"
EFT.Trainer.Features.ExfiltrationPoints.MaximumDistance=0.0
EFT.Trainer.Features.ExfiltrationPoints.NotEligibleColor=[1.0,0.921568632,0.0156862754,1.0]

EFT.Trainer.Features.GameState.CacheTimeInSec=2.0

EFT.Trainer.Features.Grenades.CacheTimeInSec=0.25
EFT.Trainer.Features.Grenades.Color=[1.0,0.0,0.0,1.0]
EFT.Trainer.Features.Grenades.Enabled=true
EFT.Trainer.Features.Grenades.Key="None"

EFT.Trainer.Features.Hud.Color=[1.0,1.0,1.0,1.0]
EFT.Trainer.Features.Hud.Enabled=true
EFT.Trainer.Features.Hud.Key="F2"

EFT.Trainer.Features.LootableContainers.CacheTimeInSec=11.0
EFT.Trainer.Features.LootableContainers.Color=[1.0,1.0,1.0,1.0]
EFT.Trainer.Features.LootableContainers.Enabled=false
EFT.Trainer.Features.LootableContainers.Key="F3"
EFT.Trainer.Features.LootableContainers.MaximumDistance=0.0

EFT.Trainer.Features.LootItems.CacheTimeInSec=3.0
EFT.Trainer.Features.LootItems.Color=[0.0,1.0,1.0,1.0]
EFT.Trainer.Features.LootItems.Enabled=true
EFT.Trainer.Features.LootItems.Key="F4"
EFT.Trainer.Features.LootItems.MaximumDistance=0.0
EFT.Trainer.Features.LootItems.SearchInsideContainers=true
EFT.Trainer.Features.LootItems.TrackedNames=["virtex","sg-c10","cofdm","battery","vpx","ushanka","chat","pilgrim"]

EFT.Trainer.Features.NightVision.Enabled=false
EFT.Trainer.Features.NightVision.Key="F11"

EFT.Trainer.Features.NoCollision.Enabled=false
EFT.Trainer.Features.NoCollision.Key="None"

EFT.Trainer.Features.NoRecoil.Enabled=false
EFT.Trainer.Features.NoRecoil.Key="F7"

EFT.Trainer.Features.Players.BearColor=[0.0,0.0,1.0,1.0]
EFT.Trainer.Features.Players.BossColor=[1.0,0.0,0.0,1.0]
EFT.Trainer.Features.Players.Enabled=true
EFT.Trainer.Features.Players.Key="F5"
EFT.Trainer.Features.Players.ScavColor=[1.0,0.921568632,0.0156862754,1.0]
EFT.Trainer.Features.Players.UsecColor=[0.0,1.0,0.0,1.0]

EFT.Trainer.Features.Quests.CacheTimeInSec=5.0
EFT.Trainer.Features.Quests.Color=[1.0,0.0,1.0,1.0]
EFT.Trainer.Features.Quests.Enabled=true
EFT.Trainer.Features.Quests.Key="F6"
EFT.Trainer.Features.Quests.MaximumDistance=0.0

EFT.Trainer.Features.Stamina.Enabled=true
EFT.Trainer.Features.Stamina.Key="None"

EFT.Trainer.Features.ThermalVision.Enabled=false
EFT.Trainer.Features.ThermalVision.Key="F12"
```

## Mono injection

EFT is using:
-	Battleye for process isolation, so you cannot use trivial mono injection techniques as [SharpMonoInjector](https://github.com/warbler/SharpMonoInjector).
-	Hash verification and a basic assembly obfuscation to prevent assembly patching. It is still possible to use [Reflexil](https://github.com/sailro/Reflexil) or [DnSpy](https://github.com/0xd4d/dnSpy) to patch the game and the loader, but this is to be done for each update. You can also paste a patched payload just after the hash check, and before the file is really loaded by the mono runtime.

Given I was not able to “force push” my code into the EFT AppDomain, I searched a way for my code to be pulled by EFT directly.
So, using ILSpy, I audited all calls to Assembly.Load* methods, and given EFT is using the NLog framework, I was able to find the following:

![ilspy](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/ilspy1.png)

NLog is auto-loading all assemblies located in the Managed folder (where the main Nlog.dll is located), if they start with the name NLog.

So from here I was able to load my code, given I copied my NLog.EFT.Trainer.dll to the managed folder:

![process monitor](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/process1.png)

After that, I needed a way to have an initial call to my code. In the .NET world you have something named module initializers, but before going this way, I found again another trick using NLog:

![ilspy](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/ilspy2.png)

If you create a file named “Nlog.dll.nlog” along with the NLog.dll file, it will be auto-loaded by default:

![process monitor](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/process2.png)

So I just crafted a proper config file, making NLog invoking the ctor of my stub Logger Target :

![config](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/config.png)

```csharp
[Target(nameof(EFTTarget))]
public sealed class EFTTarget : TargetWithLayout
{
	public EFTTarget()
	{
		Loader.Load();
	}
}
```

Then, I’m now loaded in the Game AppDomain, I can hook to the current gameObjects:

```csharp
public class Loader
{
    public static GameObject HookObject
    {
        get
        {
            var result = GameObject.Find("Application (Main Client)");
            if (result == null)
            {
                result = new GameObject("Trainer");
                Object.DontDestroyOnLoad(result);
            }
            return result;
        }
    }
    
    public static void Load()
    {
        HookObject.AddComponent<TrainerBehaviour>();
    }
}
```

I can then use all the EFT API surface. Finding GameObjects, getting components and calling methods. When something is private or obfuscated, I can even use Reflection:

```csharp
private static void UnlockDoors(Player player)
{
    var doors = FindObjectsOfType<Door>();
    foreach (var door in doors)
    {
        // door unlocker
        if (door == null)
            continue;

        if (door.DoorState != EDoorState.Locked)
            continue;

        var offset = player.Transform.position - door.transform.position;
        var sqrLen = offset.sqrMagnitude;

        // only unlock if near player, else you'll get a ban if you brute-force-unlock all doors
        if (sqrLen <= 20.0f)
            door.DoorState = EDoorState.Shut;
    }
}
```
