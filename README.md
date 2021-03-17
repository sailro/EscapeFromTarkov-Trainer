
# EscapeFromTarkov-Trainer

*I'm not responsible for any consequences that result from using this code. BattleState / BattlEye will ban you if you try to use it.*

This is an attempt -for educational purposes only- to alter a Unity game at runtime without patching the binaries (so without using [Cecil](https://github.com/jbevain/cecil) nor [Reflexil](https://github.com/sailro/reflexil)).

## Features

This trainer gives:
- HUD (ammo left in chamber / magazine, fire mode)
- Door unlocker (use keypad period)
- Wallhack (you'll see players / bots / bosses with distinct colors through walls)
- Exfiltration points (green for available points, yellow for others)
- No recoil (off by default)
- Quest locations to place items (off by default)

![demo](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/demo.png)
![demo](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/demo2.png)
![demo](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/quests.png)

## Installation

You can try to compile the code yourself (you will need a recent Visual Studio, because we are using CSharp 9). you can use a precompiled release as well.

Copy all files in your EFT directory like `C:\Battlestate Games\EFT`:

- `EscapeFromTarkov_Data\Managed\NLog.EFT.Trainer.dll` (this is the compiled code for the trainer)
- `EscapeFromTarkov_Data\outline` (this is the dedicated shader we use to outline players [wallhack])

### If you are using the Live version (you should NOT do that, you'll be detected and banned):

Rename `EscapeFromTarkov_Data\Managed\NLog.dll.nlog-live` to `NLog.dll.nlog`

### If you are using sptarkov (https://www.sp-tarkov.com):

Overwrite the existing `EscapeFromTarkov_Data\Managed\NLog.dll.nlog` using `NLog.dll.nlog-sptarkov`, or update the existing file accordingly. We must include the following 
`<target name="EFTTarget" xsi:type="EFTTarget" />` in the `targets` section for the trainer to be loaded properly.

## Configuration

![console](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/console.png)

This trainer hooks into command system, so you can easily setup features using the built-in console:

| Command  | Values        | Default | Description                      | 
|----------|---------------|---------|----------------------------------|
| dump     |               |         | Dump game state for analysis     |
| exfil    | `on` or `off` | `on`    | Show/hide exfiltration points    |
| hud      | `on` or `off` | `on`    | Show/hide hud.                   |
| norecoil | `on` or `off` | `off`   | Disable/Enable recoil            |
| quest    | `on` or `off` | `off`   | Disable/Enable quest locations   |
| status   |               |         | Show status of all features      |
| wallhack | `on` or `off` | `on`    | Show/hide players (on next raid) |

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
