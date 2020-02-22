
# EscapeFromTarkov-Trainer

-**important note**- before publishing this repository, I gave all the details to EFT developers, so that they can fix their 
game (so this trainer should not be usable anymore... or not). Indeed I don't care for offline game trainers, but I don't want a multiplayer or online game to be ruined by cheaters. I was just interested by the reverse-engineering part. 

*I'm not responsible for any consequences that result from using this code. BattleState / BattlEye will probably ban you if you try to use it.* (and if you still want to use it, you should rename all methods/types/fields, as BattlEye is collecting strings from game process).

This is an attempt -for educational purposes only- to alter a Unity game at runtime without patching the binaries (so without using [Cecil](https://github.com/jbevain/cecil) nor [Reflexil](https://github.com/sailro/reflexil)).
EFT is using BattlEye, so we **cannot** use [SharpMonoInjector](https://github.com/warbler/SharpMonoInjector).

## Features

This trainer gives:
- HUD (ammo left in chamber / magazine, fire mode)
- Door unlocker
- Wallhack
- Exfiltration points outline
- Autohealth (offline raid only)
- No bullet hits (offline raid only)

![demo](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/demo.png)
![demo](https://github.com/sailro/EscapeFromTarkov-Trainer/raw/master/Images/demo2.png)

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

        // only unlock if near player, else you'll get a ban from BattlEye if you brute-force-unlock all doors
        if (sqrLen <= 20.0f)
            door.DoorState = EDoorState.Shut;
    }
}
```
