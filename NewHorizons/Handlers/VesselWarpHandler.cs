using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NewHorizons.Components;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;
using static NewHorizons.Main;
using NewHorizons.Components.Orbital;

namespace NewHorizons.Handlers
{
    public static class VesselWarpHandler
    {
        public static GameObject VesselPrefab { get; private set; }
        public static GameObject VesselObject { get; private set; }
        public static VesselWarpController WarpController { get; private set; }

        private static SpawnPoint _vesselSpawnPoint;
        public static SpawnPoint VesselSpawnPoint => _vesselSpawnPoint;

        public static void Initialize()
        {
            VesselPrefab = Main.NHPrivateAssetBundle.LoadAsset<GameObject>("Vessel_Body");
        }

        public static void LoadVessel()
        {
            if (Instance.CurrentStarSystem == "EyeOfTheUniverse")
            {
                _vesselSpawnPoint = SearchUtilities.Find("Vessel_Body/SPAWN_Vessel").GetComponent<EyeSpawnPoint>();
                return;
            }

            if (Instance.IsWarpingFromVessel)
                _vesselSpawnPoint = Instance.CurrentStarSystem == "SolarSystem" ? UpdateVessel() : CreateVessel();
            else
                _vesselSpawnPoint = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension").GetComponentInChildren<SpawnPoint>();
        }

        public static void OnReceiveWarpedBody(OWRigidbody warpedBody, NomaiWarpPlatform startPlatform, NomaiWarpPlatform targetPlatform)
        {
            bool isPlayer = warpedBody.CompareTag("Player");
            if (isPlayer)
            {
                Transform player_body = Locator.GetPlayerTransform();
                OWRigidbody s_rb = Locator.GetShipBody();
                OWRigidbody p_rb = Locator.GetPlayerBody();
                Vector3 newPos = player_body.position;
                Vector3 offset = player_body.up * 10;
                newPos += offset;
                s_rb.SetPosition(newPos);
                s_rb.SetRotation(player_body.transform.rotation);
                s_rb.SetVelocity(p_rb.GetVelocity());
            }
        }

        public static void TeleportToVessel()
        {
            var playerSpawner = GameObject.FindObjectOfType<PlayerSpawner>();
            playerSpawner.DebugWarp(_vesselSpawnPoint);
            Builder.General.SpawnPointBuilder.SuitUp();

            if (Instance.CurrentStarSystem == "SolarSystem")
            {
                // Deactivate village music because for some reason it still plays.
                SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_Village/Volumes_Village/MusicVolume_Village").GetComponent<VillageMusicVolume>().Deactivate();

                // Loads it manually so the player doesn't start falling and then vessel loads in on them.
                SectorStreaming ss = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_Streaming").GetComponent<SectorStreaming>();
                ss.enabled = true;
                ss._streamingGroup.LoadRequiredAssets();
                ss._streamingGroup.LoadRequiredColliders();
                ss._streamingGroup.LoadGeneralAssets();
                StreamingManager.loadingPriority = StreamingManager.LoadingPriority.High;
            }
        }

        public static EyeSpawnPoint CreateVessel()
        {
            var system = SystemDict[Instance.CurrentStarSystem];

            Logger.LogVerbose("Checking for Vessel Prefab");
            if (VesselPrefab == null) return null;

            Logger.LogVerbose("Creating Vessel");
            var vesselObject = VesselPrefab.InstantiateInactive();
            VesselObject = vesselObject;
            vesselObject.name = VesselPrefab.name;
            vesselObject.transform.parent = null;

            var vesselAO = vesselObject.AddComponent<EyeAstroObject>();
            vesselAO._owRigidbody = vesselObject.GetComponent<OWRigidbody>();
            vesselAO._rootSector = vesselObject.GetComponentInChildren<Sector>(true);
            vesselAO._customName = "Vessel";
            vesselAO._name = AstroObject.Name.CustomString;
            vesselAO._type = AstroObject.Type.SpaceStation;
            vesselAO.Register();
            vesselObject.GetComponentInChildren<ReferenceFrameVolume>(true)._referenceFrame._attachedAstroObject = vesselAO;

            if (system.Config.Vessel?.vesselPosition != null)
                vesselObject.transform.position = system.Config.Vessel.vesselPosition;

            if (system.Config.Vessel?.vesselRotation != null)
                vesselObject.transform.eulerAngles = system.Config.Vessel.vesselRotation;

            VesselSingularityRoot singularityRoot = vesselObject.GetComponentInChildren<VesselSingularityRoot>(true);

            VesselWarpController vesselWarpController = vesselObject.GetComponentInChildren<VesselWarpController>(true);
            WarpController = vesselWarpController;

            GameObject WarpPlatform = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_VesselBridge/Interactibles_VesselBridge/WarpController/Prefab_NOM_WarpPlatform");
            GameObject warpBH = WarpPlatform.transform.Find("BlackHole").gameObject;
            GameObject warpWH = WarpPlatform.transform.Find("WhiteHole").gameObject;

            GameObject sourceBH = GameObject.Instantiate(warpBH, vesselWarpController._sourceWarpPlatform.transform, false);
            sourceBH.name = "BlackHole";
            vesselWarpController._sourceWarpPlatform._blackHole = sourceBH.GetComponentInChildren<SingularityController>();
            vesselWarpController._sourceWarpPlatform._blackHole.OnCollapse += vesselWarpController._sourceWarpPlatform.OnBlackHoleCollapse;

            GameObject sourceWH = GameObject.Instantiate(warpWH, vesselWarpController._sourceWarpPlatform.transform, false);
            sourceWH.name = "WhiteHole";
            vesselWarpController._sourceWarpPlatform._whiteHole = sourceWH.GetComponentInChildren<SingularityController>();
            vesselWarpController._sourceWarpPlatform._whiteHole.OnCollapse += vesselWarpController._sourceWarpPlatform.OnWhiteHoleCollapse;

            GameObject targetBH = GameObject.Instantiate(warpBH, vesselWarpController._targetWarpPlatform.transform, false);
            targetBH.name = "BlackHole";
            vesselWarpController._targetWarpPlatform._blackHole = targetBH.GetComponentInChildren<SingularityController>();
            vesselWarpController._targetWarpPlatform._blackHole.OnCollapse += vesselWarpController._targetWarpPlatform.OnBlackHoleCollapse;

            GameObject targetWH = GameObject.Instantiate(warpWH, vesselWarpController._targetWarpPlatform.transform, false);
            targetWH.name = "WhiteHole";
            vesselWarpController._targetWarpPlatform._whiteHole = targetWH.GetComponentInChildren<SingularityController>();
            vesselWarpController._targetWarpPlatform._whiteHole.OnCollapse += vesselWarpController._targetWarpPlatform.OnWhiteHoleCollapse;

            GameObject blackHole = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_VesselBridge/Interactibles_VesselBridge/BlackHole");
            GameObject newBlackHole = GameObject.Instantiate(blackHole, Vector3.zero, Quaternion.identity, singularityRoot.transform);
            newBlackHole.name = "BlackHole";
            vesselWarpController._blackHole = newBlackHole.GetComponentInChildren<SingularityController>();
            vesselWarpController._blackHoleOneShot = vesselWarpController._blackHole.transform.parent.Find("BlackHoleAudio_OneShot").GetComponent<OWAudioSource>();

            GameObject whiteHole = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_VesselBridge/Interactibles_VesselBridge/WhiteHole");
            GameObject newWhiteHole = GameObject.Instantiate(whiteHole, Vector3.zero, Quaternion.identity, singularityRoot.transform);
            newWhiteHole.name = "WhiteHole";
            vesselWarpController._whiteHole = newWhiteHole.GetComponentInChildren<SingularityController>();
            vesselWarpController._whiteHoleOneShot = vesselWarpController._whiteHole.transform.parent.Find("WhiteHoleAudio_OneShot").GetComponent<OWAudioSource>();

            vesselWarpController._targetWarpPlatform.OnReceiveWarpedBody += OnReceiveWarpedBody;

            if (system.Config.Vessel?.warpExitPosition != null)
                vesselWarpController._targetWarpPlatform.transform.localPosition = system.Config.Vessel.warpExitPosition;

            if (system.Config.Vessel?.warpExitRotation != null)
                vesselWarpController._targetWarpPlatform.transform.localEulerAngles = system.Config.Vessel.warpExitRotation;

            vesselObject.GetComponent<MapMarker>()._labelID = (UITextType)TranslationHandler.AddUI("Vessel");

            EyeSpawnPoint eyeSpawnPoint = vesselObject.GetComponentInChildren<EyeSpawnPoint>(true);
            system.SpawnPoint = eyeSpawnPoint;

            vesselObject.SetActive(true);

            Delay.FireOnNextUpdate(() => SetupWarpController(vesselWarpController));

            return eyeSpawnPoint;
        }

        public static SpawnPoint UpdateVessel()
        {
            var system = SystemDict[Instance.CurrentStarSystem];

            Logger.LogVerbose("Updating DB Vessel");
            var vectorSector = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension");
            VesselObject = vectorSector;

            var spawnPoint = vectorSector.GetComponentInChildren<SpawnPoint>();

            VesselWarpController vesselWarpController = vectorSector.GetComponentInChildren<VesselWarpController>(true);
            WarpController = vesselWarpController;

            if (vesselWarpController._targetWarpPlatform != null)
                vesselWarpController._targetWarpPlatform.OnReceiveWarpedBody += OnReceiveWarpedBody;

            if (vesselWarpController._whiteHole == null)
            {
                GameObject whiteHole = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension/Sector_VesselBridge/Interactibles_VesselBridge/WhiteHole");
                vesselWarpController._whiteHole = whiteHole.GetComponentInChildren<SingularityController>();
                vesselWarpController._whiteHoleOneShot = vesselWarpController._whiteHole.transform.parent.Find("WhiteHoleAudio_OneShot").GetComponent<OWAudioSource>();
            }

            Delay.FireOnNextUpdate(() => SetupWarpController(vesselWarpController, true));

            return spawnPoint;
        }

        public static void SetupWarpController(VesselWarpController vesselWarpController, bool db = false)
        {
            if (db)
            {
                //Make warp core
                foreach (WarpCoreItem core in Resources.FindObjectsOfTypeAll<WarpCoreItem>())
                {
                    if (core.GetWarpCoreType().Equals(WarpCoreType.Vessel))
                    {
                        var newCore = GameObject.Instantiate(core, AstroObjectLocator.GetAstroObject("Vessel Dimension")?.transform ?? Locator.GetPlayerBody()?.transform);
                        newCore._visible = true;
                        foreach (OWRenderer render in newCore._renderers)
                        {
                            if (render)
                            {
                                render.enabled = true;
                                render.SetActivation(true);
                                render.SetLODActivation(true);
                                if (render.GetRenderer()) render.GetRenderer().enabled = true;
                            }
                        }
                        foreach (ParticleSystem particleSystem in newCore._particleSystems)
                        {
                            if (particleSystem) particleSystem.Play(true);
                        }
                        foreach (OWLight2 light in newCore._lights)
                        {
                            if (light)
                            {
                                light.enabled = true;
                                light.SetActivation(true);
                                light.SetLODActivation(true);
                                if (light.GetLight()) light.GetLight().enabled = true;
                            }
                        }
                        vesselWarpController._coreSocket.PlaceIntoSocket(newCore);
                        break;
                    }
                }
            }
            vesselWarpController.OnSlotDeactivated(vesselWarpController._coordinatePowerSlot);
            if (!db) vesselWarpController.OnSlotActivated(vesselWarpController._coordinatePowerSlot);
            vesselWarpController._gravityVolume.SetFieldMagnitude(vesselWarpController._origGravityMagnitude);
            vesselWarpController._coreCable.SetPowered(true);
            vesselWarpController._coordinateCable.SetPowered(!db);
            vesselWarpController._warpPlatformCable.SetPowered(false);
            vesselWarpController._cageClosed = true;
            if (vesselWarpController._cageAnimator != null)
            {
                vesselWarpController._cageAnimator.TranslateToLocalPosition(new Vector3(0.0f, -8.1f, 0.0f), 0.1f);
                vesselWarpController._cageAnimator.RotateToLocalEulerAngles(new Vector3(0.0f, 180f, 0.0f), 0.1f);
                vesselWarpController._cageAnimator.OnTranslationComplete -= new TransformAnimator.AnimationEvent(vesselWarpController.OnCageAnimationComplete);
                vesselWarpController._cageAnimator.OnTranslationComplete += new TransformAnimator.AnimationEvent(vesselWarpController.OnCageAnimationComplete);
            }
        }
    }
}
