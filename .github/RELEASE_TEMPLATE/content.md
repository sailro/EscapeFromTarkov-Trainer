[![Sponsor](https://img.shields.io/badge/sponsor-%E2%9D%A4-lightgrey?logo=github&style=flat-square)](https://github.com/sponsors/sailro)

> Name: Installer.exe
> Size: FILE_SIZE_DISPLAY
> SHA256: FILE_SHA256

This is an universal installer for the trainer. In most cases you just need to run `installer.exe` from a command prompt.

## 1- It will discover EscapeFromTarkov installations. 
- You can also pass a custom installation path on the command line: `installer.exe "C:\Program Files\EFT"` or copy the installer to the target directory.
- Depending on where the game is located, perhaps you will need to use a command prompt with **admin privileges**. 
- You just need to re-run the Installer to update your trainer to the latest version. No need to re-download the Installer.

![image](https://user-images.githubusercontent.com/638167/144761827-e233ec42-3541-4309-967b-43878b21c5bd.png)

## 2- It will download the latest source code from this repository.
- Then will try to compile the trainer. 
- If the latest version is not working with your installation, it will try to select a compatible (but older) version. You can also use the `--branch` command switch to provide a specific trainer version to compile against. See existing [`branches`](https://github.com/sailro/EscapeFromTarkov-Trainer/branches).
- Still not working? It will then try to disable faulting features.
- No need for SDKs, third party dependencies or Visual Studio, the Installer is self-contained. 
- Before EscapeFromTarkov `0.13.0.21531`, it should work for `live` (do not do that, you'll be detected & banned!).
- Given EscapeFromTarkov `0.13.0.21531`  or later prevent this trainer to be loaded using NLog configuration. It is now mandatory to use `SPT/BepInEx` for recent versions.
- **Important**: if you are using `SPT`, please make sure you have run the game at least once before installing the trainer. `SPT` is patching binaries during the first run, and we need to compile against those patched binaries. If you install this trainer on stock binaries, the game will freeze at the startup screen.

![image](https://user-images.githubusercontent.com/638167/146071436-401b4f80-f4bb-4dfb-8cdc-23ef5bfc79c3.png)

## 3- It will copy all needed files for you.
- The freshly-compiled trainer bits will be copied to `EscapeFromTarkov_Data\Managed\NLog.EFT.Trainer.dll`.
- The outline shader will be copied to `EscapeFromTarkov_Data\outline`
- `EscapeFromTarkov_Data\Managed\NLog.dll.nlog` will be created or patched, depending on the content. (This is for legacy EscapeFromTarkov versions, before `0.13.0.21531`).
- It will compile a SPT/BepInEx plugin named `spt-efttrainer.dll` in `BepInEx\plugins` 

![image](https://user-images.githubusercontent.com/638167/211163262-e49bca08-642c-4512-b92f-c2c1de4bead1.png)

## Troubleshooting:
- The game is stuck after installing the trainer : if you are using `SPT-AKI`, please make sure you have run the game at least once before installing the trainer. `SPT-AKI` is patching binaries during the first run, and we need to compile against those patched binaries. If you install this trainer on stock binaries, the game will freeze at the startup screen.
- The trainer is not loaded : are you sure you are running the proper EFT instance? you can double check with the file `%LOCALAPPDATA%Low\Battlestate Games\EscapeFromTarkov\Player.log`, search for an installation path (often along with `Fallback handler could not load library` errors). Perhaps you forgot to update your shortcuts to `server.exe`/`launcher.exe` files.
- The installer is unable to compile the trainer for an old EFT version : sorry I do not plan to support old versions, please upgrade.
- The installer is unable to compile the trainer for a new EFT version : please file an [issue](https://github.com/sailro/EscapeFromTarkov-Trainer/issues/new/choose).

## Usage examples:
- `Installer --help` to display general help.
- `Installer install --help` to display help for the `install` command.
- `Installer "C:\Battlestate Games\EFT"` to install (default command) the trainer, adding `C:\Battlestate Games\EFT` to the search list.
- `Installer -b dev-0.12.11.13771` to use a specific branch/version, this case `0.12.11.13771`.
- `Installer -f <feature>` to disable a feature. Example: `Installer -f ThermalVision -f NightVision`. Use feature names found in the `trainer.ini` file.
- `Installer -c <command>` to disable a command. Example: `Installer -c Spawn`.
- `Installer uninstall` to remove the trainer.
- `Installer uninstall "C:\Battlestate Games\EFT"` to remove the trainer, adding `C:\Battlestate Games\EFT` to the search list.
- `Installer -l <language>` to compile the trainer for a specific language (like `zh-cn` for Chinese simplified or `fr` for French). See the supported ones [here](https://github.com/sailro/EscapeFromTarkov-Trainer/tree/master/Properties).

[![Sponsor](https://img.shields.io/badge/sponsor-%E2%9D%A4-lightgrey?logo=github&style=flat-square)](https://github.com/sponsors/sailro)
