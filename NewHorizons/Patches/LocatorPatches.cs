using HarmonyLib;
using NewHorizons.Components;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class LocatorPatches
    {
        public static AstroObject _eye;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.RegisterCloakFieldController))]
        public static bool Locator_RegisterCloakFieldController()
        {
            return Locator._cloakFieldController == null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isPlayerInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isPlayerInsideCloak(CloakFieldController __instance, ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isPlayerInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isProbeInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isProbeInsideCloak(CloakFieldController __instance, ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isProbeInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isShipInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isShipInsideCloak(CloakFieldController __instance, ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isShipInside;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.GetAstroObject))]
        public static bool Locator_GetAstroObject(AstroObject.Name astroObjectName, ref AstroObject __result)
        {
            if (astroObjectName == AstroObject.Name.Eye && _eye != null)
            {
                __result = _eye;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.RegisterAstroObject))]
        public static bool Locator_RegisterAstroObject(AstroObject astroObject)
        {
            if (astroObject._name == AstroObject.Name.Eye)
            {
                _eye = astroObject;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.ClearReferences))]
        public static void Locator_ClearReferences()
        {
            _eye = null;
        }
    }
}
