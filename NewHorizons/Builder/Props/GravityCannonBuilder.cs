using Epic.OnlineServices.Presence;
using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Shuttle;
using NewHorizons.External.Modules.TranslatorText;
using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.IO;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class GravityCannonBuilder
    {
        private static GameObject _prefab;

        internal static void InitPrefab()
        {
            if (_prefab == null)
            {
                var original = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/Prefab_NOM_GravityCannonInterface");
                _prefab = original?.InstantiateInactive()?.Rename("Prefab_GravityCannon")?.DontDestroyOnLoad();
                _prefab.transform.position = original.transform.position;
                _prefab.transform.rotation = original.transform.rotation;
                var gravityCannonController = _prefab.GetComponent<GravityCannonController>();
                gravityCannonController._shuttleID = NomaiShuttleController.ShuttleID.HourglassShuttle;
                gravityCannonController._shuttleSocket = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/Prefab_NOM_ShuttleSocket"), _prefab.transform, true).Rename("ShuttleSocket").transform;
                gravityCannonController._warpEffect = gravityCannonController._shuttleSocket.GetComponentInChildren<SingularityWarpEffect>();
                gravityCannonController._recallProxyGeometry = gravityCannonController._shuttleSocket.gameObject.FindChild("ShuttleRecallProxy");
                gravityCannonController._forceVolume = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Interactables_GravityCannon/CannonForceVolume"), _prefab.transform, true).Rename("ForceVolume").GetComponent<DirectionalForceVolume>();
                gravityCannonController._platformTrigger = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Volumes_GravityCannon/CannonPlatformTrigger"), _prefab.transform, true).Rename("PlatformTrigger").GetComponent<OWTriggerVolume>();
                gravityCannonController._nomaiComputer = null;
            }
        }

        public static GameObject Make(GameObject planetGO, Sector sector, GravityCannonInfo info, IModBehaviour mod)
        {
            InitPrefab();

            if (_prefab == null || planetGO == null || sector == null) return null;

            var detailInfo = new DetailInfo(info);
            var gravityCannonObject = DetailBuilder.Make(planetGO, sector, _prefab, detailInfo);
            gravityCannonObject.SetActive(false);

            StreamingHandler.SetUpStreaming(gravityCannonObject, sector);

            var gravityCannonController = gravityCannonObject.GetComponent<GravityCannonController>();
            gravityCannonController._shuttleID = ShuttleHandler.GetShuttleID(info.shuttleID);
            gravityCannonController._retrieveShipLogFact = info.retrieveReveal;
            gravityCannonController._launchShipLogFact = info.launchReveal;

            if (info.computer != null)
            {
                gravityCannonController._nomaiComputer = CreateComputer(planetGO, sector, info.computer, mod);
            }
            else
            {
                gravityCannonController._nomaiComputer = CreateComputer(planetGO, sector, new NomaiGravityCannonComputerInfo
                {
                    position = new MVector3(-2.556838f, -0.8018004f, 10.01348f),
                    rotation = new MVector3(8.293f, 2.403f, 0.9f),
                    isRelativeToParent = true,
                    rename = "Computer",
                    xmlFile = "Assets/GravityCannonComputer.xml",
                    parentPath = gravityCannonObject.transform.GetPath().Remove(0, planetGO.name.Length + 1)
                }, Main.Instance);
            }

            gravityCannonObject.SetActive(true);

            return gravityCannonObject;
        }

        private static NomaiComputer CreateComputer(GameObject planetGO, Sector sector, NomaiGravityCannonComputerInfo computerInfo, IModBehaviour mod)
        {
            var computerObject = DetailBuilder.Make(planetGO, sector, TranslatorTextBuilder.ComputerPrefab, new DetailInfo(computerInfo));

            var computer = computerObject.GetComponentInChildren<NomaiComputer>();
            computer.SetSector(sector);

            var xmlContent = !string.IsNullOrEmpty(computerInfo.xmlFile) ? File.ReadAllText(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, computerInfo.xmlFile)) : null;
            if (xmlContent != null)
            {
                computer._dictNomaiTextData = TranslatorTextBuilder.MakeNomaiTextDict(xmlContent);
                computer._nomaiTextAsset = new TextAsset(xmlContent);
                computer._nomaiTextAsset.name = Path.GetFileNameWithoutExtension(computerInfo.xmlFile);
                TranslatorTextBuilder.AddTranslation(xmlContent);
            }
            else
            {
                NHLogger.LogError($"Failed to create gravity cannon computer because {nameof(computerInfo.xmlFile)} was not set to a valid text .xml file path");
            }

            Delay.FireOnNextUpdate(computer.ClearAllEntries);

            computerObject.SetActive(true);

            return computer;
        }
    }
}
