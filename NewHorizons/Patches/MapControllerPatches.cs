using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class MapControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapController), nameof(MapController.Awake))]
        public static void MapController_Awake(MapController __instance)
        {
            __instance._maxPanDistance = Mathf.Max(__instance._maxPanDistance, Main.FurthestOrbit * 1.5f);
            __instance._maxZoomDistance *= 6f;
            __instance._minPitchAngle = -90f;
            __instance._zoomSpeed *= 4f;
            __instance._mapCamera.farClipPlane = Mathf.Max(__instance._mapCamera.farClipPlane, Main.FurthestOrbit * 10f);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapController), nameof(MapController.OnTargetReferenceFrame))]
        public static void MapController_OnTargetReferenceFrame(MapController __instance, ReferenceFrame __0)
        {
            __instance._isLockedOntoMapSatellite = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapController), nameof(MapController.MapInoperable))]
        public static bool MapController_MapInoperable(MapController __instance, ref bool __result)
        {
            if (SceneManager.GetActiveScene().name != "SolarSystem") return true;

            try
            {
                if (Main.SystemDict[Main.Instance.CurrentStarSystem].Config.mapRestricted)
                {
                    __instance._playerMapRestricted = true;
                    __result = true;
                    return false;
                }
            }
            catch { }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ReferenceFrameTracker), nameof(ReferenceFrameTracker.UntargetReferenceFrame), new System.Type[] { typeof(bool) })]
        public static bool ReferenceFrameTracker_UntargetReferenceFrame(ReferenceFrameTracker __instance, bool playAudio)
        {
            return __instance != null && __instance._hasTarget && __instance._currentReferenceFrame != null;
        }
    }
}
