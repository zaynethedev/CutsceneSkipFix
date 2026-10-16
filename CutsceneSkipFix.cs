using HarmonyLib;
using MelonLoader;
using StressLevelZero.Utilities;
using UnityEngine.SceneManagement;
using Valve.VR;

[assembly: MelonInfo(typeof(CutsceneSkipFix.CutsceneSkipFixMod), "CutsceneSkipFix", "1.0.0", "zaynethedev")]
[assembly: MelonGame("Stress Level Zero", "BONEWORKS")]

namespace CutsceneSkipFix
{
    public class CutsceneSkipFixMod : MelonMod
    {
        internal static void SkipToNextLevel(GameControl_Theatrigon theatrigon)
        {
            if (theatrigon._hasLoadedLevel) return;
            theatrigon._hasLoadedLevel = true;

            KillActiveLoader();

            int next = NextSceneIndex();
            MelonLogger.Msg($"Skipping cutscene, loading scene {next}");
            BoneworksSceneManager.LoadScene(next);
        }

        /* SceneLoader.Trigger() will ignore any new load if an active loader exists. 
           The loader that loads you into the cutscene stays active until it finishes fading in, so we must
           destroy it to allow the next scene to load. */
        
        private static void KillActiveLoader()
        {
            var loader = SceneLoader._active;
            if (loader == null) return;

            loader.StopAllCoroutines();

            var overlay = OpenVR.Overlay;
            if (overlay != null)
            {
                if (loader.progressBarOverlayHandle != 0) overlay.HideOverlay(loader.progressBarOverlayHandle);
                if (loader.loadingScreenOverlayHandle != 0) overlay.HideOverlay(loader.loadingScreenOverlayHandle);
            }

            UnityEngine.Object.Destroy(loader.gameObject);
            SceneLoader._active = null;
            PreloadLevel.onPreloadStartDelegate = null;
            PreloadLevel.onPreloadCompleteDelegate = null;
        }

        // LoadNext() loads the next scene based off of the active scene index, which isn't always correct when the level is mid-load.
        
        private static int NextSceneIndex()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name.StartsWith("scene_theatrigon_movie"))
                    return scene.buildIndex + 1;
            }
            return SceneManager.GetActiveScene().buildIndex + 1;
        }
    }

    /* The skip button calls Stop(). The original function will only load the next level if 
       the video is already playing, so skipping before the cutscene starts playing causes the level to never load. */
    
    [HarmonyPatch(typeof(GameControl_Theatrigon), nameof(GameControl_Theatrigon.Stop))]
    internal static class StopPatch
    {
        private static bool Prefix(GameControl_Theatrigon __instance)
        {
            __instance._preShow = false;
            __instance._hasPlayed = true;

            var playlist = __instance._playPlaylistCoroutine;
            if (playlist != null)
            {
                __instance.StopCoroutine(playlist);
                __instance._playPlaylistCoroutine = null;
                __instance.videoPlayer?.Stop();
            }

            CutsceneSkipFixMod.SkipToNextLevel(__instance);
            return false;
        }
    }

    // Used by the skip trigger volume.
    
    [HarmonyPatch(typeof(GameControl_Theatrigon), nameof(GameControl_Theatrigon.NEXTLEVEL))]
    internal static class NextLevelPatch
    {
        private static bool Prefix(GameControl_Theatrigon __instance)
        {
            CutsceneSkipFixMod.SkipToNextLevel(__instance);
            return false;
        }
    }
}
