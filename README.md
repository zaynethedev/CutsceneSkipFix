# CutsceneSkipFix

This mod fixes two bugs in the game. The first bug is with the skip button, where when pressed it calls `GameControl_Theatrigon.Stop()`, which loads the next level ONLY if the cutscene has started. If you press it before the cutscene starts, the level fails to load and since the button only fires once, you can't retry.

The second bug is with the actual level loading, where every time a load is requested, `SceneLoader` ignores it if any other loaders are active, so pressing the button too early means the previous loader hasn't been deleted, which in return fails to load the next level.

This mod stops the video when the button is pressed, deletes any existing level loaders, and then tries to load the next scene.


# Installation

- Make sure [Melon Loader](https://melonwiki.xyz/#/?id=what-is-melonloader) is installed in Boneworks
- Download [CutsceneSkipFix from Thunderstore](https://boneworks.thunderstore.io/package/zaynethedev/CutsceneSkipFix/)
- Open the downloaded `.zip` file and extract `Mods/CutsceneSkipFix.dll` into `BONEWORKS/Mods/CutsceneSkipFix.dll`
  - To find your `BONEWORKS` directory, go into Steam, right click on BONEWORKS, and go to `"Manage > Browse local files"`
