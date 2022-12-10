using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons
{

    public class NewHorizonsApi : INewHorizons
    {
        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config)
        {
            Create(config, null);
        }

        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config, IModBehaviour mod)
        {
            try
            {
                var name = (string)config["Name"];

                Logger.LogWarning($"Recieved API request to create planet [{name}]");

                if (name == null) return;

                var relativePath = $"temp/{name}.json";
                var fullPath = Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, relativePath);
                if (!Directory.Exists(Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "temp")))
                {
                    Directory.CreateDirectory(Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "temp"));
                }
                JsonHelper.SaveJsonObject(fullPath, config);
                var body = Main.Instance.LoadConfig(Main.Instance, relativePath);
                File.Delete(fullPath);

                // Update it to point to their mod for textures and stuff
                body.Mod = mod ?? Main.Instance;

                if (!Main.BodyDict.ContainsKey(body.Config.starSystem)) Main.BodyDict.Add(body.Config.starSystem, new List<NewHorizonsBody>());
                Main.BodyDict[body.Config.starSystem].Add(body);
            }
            catch(Exception ex)
            {
                Logger.LogError($"Error in Create API:\n{ex}");
            }
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            Main.Instance.LoadConfigs(mod);
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyDict.Values.SelectMany(x => x)?.ToList()?.FirstOrDefault(x => x.Config.name == name)?.Object;
        }

        public string GetCurrentStarSystem() => Main.Instance.CurrentStarSystem;
        public UnityEvent<string> GetChangeStarSystemEvent() => Main.Instance.OnChangeStarSystem;
        public UnityEvent<string> GetStarSystemLoadedEvent() => Main.Instance.OnStarSystemLoaded;
        public UnityEvent<string> GetBodyLoadedEvent() => Main.Instance.OnPlanetLoaded;

        public bool SetDefaultSystem(string name)
        {
            if (!Main.SystemDict.ContainsKey(name)) return false;

            Main.Instance.SetDefaultSystem(name);
            return true;
        }

        public bool ChangeCurrentStarSystem(string name)
        {
            if (!Main.SystemDict.ContainsKey(name)) return false;

            Main.Instance.ChangeCurrentStarSystem(name);
            return true;
        }

        public string[] GetInstalledAddons()
        {
            try
            {
                return Main.MountedAddons.Select(x => x?.ModHelper?.Manifest?.UniqueName).ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Couldn't get installed addons:\n{ex}");
                return new string[] { };
            }
        }

        private static object QueryJson(Type outType, string filePath, string jsonPath)
        {
            if (filePath == "") return null;
            try
            {
                var jsonText = File.ReadAllText(filePath);
                var jsonData = JObject.Parse(jsonText);
                return jsonData.SelectToken(jsonPath)?.ToObject(outType);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (JsonException e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }

        public object QueryBody(Type outType, string bodyName, string jsonPath)
        {
            var planet = Main.BodyDict[Main.Instance.CurrentStarSystem].Find((b) => b.Config.name == bodyName);
            return planet == null
                ? null
                : QueryJson(outType, Path.Combine(planet.Mod.ModHelper.Manifest.ModFolderPath, planet.RelativePath), jsonPath);
        }

        public T QueryBody<T>(string bodyName, string jsonPath)
        {
            var data = QueryBody(typeof(T), bodyName, jsonPath);
            if (data is T result) {
                return result;
            }
            return default;
        }

        public object QuerySystem(Type outType, string jsonPath)
        {
            var system = Main.SystemDict[Main.Instance.CurrentStarSystem];
            return system == null 
                ? null 
                : QueryJson(outType, Path.Combine(system.Mod.ModHelper.Manifest.ModFolderPath, system.RelativePath), jsonPath);
        }

        public T QuerySystem<T>(string jsonPath) {
            var data = QuerySystem(typeof(T), jsonPath);
            if (data is T result) {
                return result;
            }
            return default;
        }

        public GameObject SpawnObject(GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles,
            float scale, bool alignWithNormal)
        {
            var prefab = SearchUtilities.Find(propToCopyPath);
            var detailInfo = new PropModule.DetailInfo() {
                position = position,
                rotation = eulerAngles,
                scale = scale,
                alignToNormal = alignWithNormal
            };
            return DetailBuilder.Make(planet, sector, prefab, detailInfo);
        }

        public AudioSignal SpawnSignal(IModBehaviour mod, GameObject root, string audio, string name, string frequency,
            float sourceRadius = 1f, float detectionRadius = 20f, float identificationRadius = 10f, bool insideCloak = false,
            bool onlyAudibleToScope = true, string reveals = "")
        {
            var info = new SignalModule.SignalInfo()
            {
                audio = audio,
                detectionRadius = detectionRadius,
                frequency = frequency,
                identificationRadius = identificationRadius,
                insideCloak = insideCloak,
                name = name,
                onlyAudibleToScope = onlyAudibleToScope,
                position = Vector3.zero,
                reveals = reveals,
                sourceRadius = sourceRadius
            };

            return SignalBuilder.Make(root, null, info, mod).GetComponent<AudioSignal>();
        }

        public (CharacterDialogueTree, RemoteDialogueTrigger) SpawnDialogue(IModBehaviour mod, GameObject root, string xmlFile, float radius = 1f, 
            float range = 1f, string blockAfterPersistentCondition = null, float lookAtRadius = 1f, string pathToAnimController = null, 
            float remoteTriggerRadius = 0f)
        {
            var info = new PropModule.DialogueInfo()
            {
                blockAfterPersistentCondition = blockAfterPersistentCondition,
                lookAtRadius = lookAtRadius,
                pathToAnimController = pathToAnimController,
                position = Vector3.zero,
                radius = radius,
                remoteTriggerPosition = null,
                range = range,
                remoteTriggerRadius = remoteTriggerRadius,
                xmlFile = xmlFile
            };

            return DialogueBuilder.Make(root, null, info, mod);
        }
    }
}
