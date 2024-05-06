![Deadlands logo](/mod/thumbnail.png)
Encircling the long-dead remains of Sliver of Straw, **Deadlands** is a large-scale mod providing a new slugcat (Nomad) as well as many new regions, creatures, and mechanics, all set in a completely new environment from the original game.

# Build Instructions

## Requirements
To build and run this mod, you need:
- Rainworld
- More Slugcats DLC
- Visual Studio 2022 (or anything else that can compile a .csproj for C# 12)

**In addition, the following mods should be subscribed to on the Steam workshop**:
- SlugBase
- Fisobs
- RegionKit
- Placed Objects Manager (POM)

## Build process
1. Create a new folder in `...Steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods`, preferably entitled `Deadlands`.
2. Compile the csproj located in /src with your compiler of choice.
3. Copy the resulting dll, as well as the contents of the /mod folder, into the folder you created earlier.
4. Activate the mod in-game and restart.
