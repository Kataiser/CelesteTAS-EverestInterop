# CelesteTAS

## TAS tools mod in Everest

### License: MIT

----
- Install [Everest](https://everestapi.github.io/) if you haven't already.
- (Recommended) Use the 1-click installer [here](https://gamebanana.com/tools/6715). (Alternatively) [Download the latest autobuild](https://nightly.link/EverestAPI/CelesteTAS-EverestInterop/workflows/NetFramework.Legacy.CI/master/CelesteTAS.zip) and put it in the game_path/mods folder.
- Enable the mod in the in-game mod options.
- If on Linux, enable `Unix RTC` in the mod options and restart Celeste.
- Open Celeste Studio, our input editor. It should be in your main Celeste directory. (Note that Studio is not supported on Mac and may not work on Linux.) [Studio documentation can be found here.](https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/master/Docs/Studio.md)

## Input File
The input file should be called Celeste.tas and needs to be in the main Celeste directory. Celeste Studio will automatically create this file for you. The tools will not work if there are no inputs in this file.

Format for the input file is (Frames),(Actions)

e.g. 123,R,J (For 123 frames, hold Right and Jump)

## Actions Available
- R = Right
- L = Left
- U = Up
- D = Down
- J = Jump / Confirm
- K = Jump Bind 2
- X = Dash / Talk
- C = Dash Bind 2
- G = Grab
- S = Pause
- Q = Quick Reset
- F = Feather Aim (Format: F,angle)
- O = Confirm
- N = Journal (Used only for Cheat Code)

## Controls
While in game or in Studio:
- Start/Stop Playback: RightControl
- Restart Playback: Equals
- Fast Forward / Frame Advance Continuously: RightShift
- Pause / Frame Advance: [
- Unpause: ]
- Toggle Hitboxes: B
- Toggle Simplified Graphics: N
- Toggle Center Camera: M
- Save State: RightAlt + Minus ([Experimental](#savestate))
- Clear State: RightAlt + Back ([Experimental](#savestate))

- These can be rebound in Mod Options (Note that controller is not supported.)
  - You will have to rebind some of these if you are on a non-US keyboard layout.
  - Binding multiple keys to a control will cause those keys to act as a keycombo.
  
## Special Input
### Breakpoints
- You can create a breakpoint in the input file by typing `***` by itself on a single line
- The program when played back from the start will fast forward until it reaches that line and then go into frame stepping mode
- You can specify the speed with `***X`, where `X` is the speedup factor. e.g. `***10` will go at 10x speed
- `***!` will force the TAS to pause even if there are breakpoints afterward in the file
- `***S` means that TAS will auto save state when it reaches this breakpoint, so that future TAS starts directly from this breakpoint instead of starting from the beginning. ([Experimental](#savestate))

### Commands
- Various commands exist to facilitate TAS playback. Documentation can be found [here.](https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/master/Docs/Commands.md)

## Savestate
- Experimental feature require [SpeedrunTool](https://gamebanana.com/tools/6597)
- According to tests savestate is very reliable for official maps
- Savestate may not be able to restore 100% of the state in custom maps due to the presence of various helpers. Recommend testing roughly before using it to see if it will cause desync, and use it before the screen transition
- Currently cannot savestate during skipping cutscene or player death
- SpeedrunTool has a known issue that hard to solve, the game may crash when the memory is automatically recycled due to frequent loading state, currently SpeedrunTool can only minimized memory usage to reduce the chance of this issue occurr
- 
