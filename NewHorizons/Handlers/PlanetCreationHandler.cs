using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Body;
using NewHorizons.Builder.General;
using NewHorizons.Builder.Orbital;
using NewHorizons.Builder.Props;
using NewHorizons.Builder.Updater;
using NewHorizons.Components;
using NewHorizons.External.VariableSize;
using NewHorizons.Utility;
using NewHorizons.Utility.CommonResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class PlanetCreationHandler
    {
        public static List<NewHorizonsBody> NextPassBodies = new List<NewHorizonsBody>();

        public static void Init(List<NewHorizonsBody> bodies)
        {
            Main.FurthestOrbit = 30000;

            // Set up stars
            // Need to manage this when there are multiple stars
            var sun = GameObject.Find("Sun_Body");
            var starController = sun.AddComponent<StarController>();
            starController.Light = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>();
            starController.AmbientLight = GameObject.Find("Sun_Body/AmbientLight_SUN").GetComponent<Light>();
            starController.FaceActiveCamera = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<FaceActiveCamera>();
            starController.CSMTextureCacher = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<CSMTextureCacher>();
            starController.ProxyShadowLight = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<ProxyShadowLight>();
            starController.Intensity = 0.9859f;
            starController.SunColor = new Color(1f, 0.8845f, 0.6677f, 1f);

            var starLightGO = GameObject.Instantiate(sun.GetComponentInChildren<SunLightController>().gameObject);
            foreach (var comp in starLightGO.GetComponents<Component>())
            {
                if (!(comp is SunLightController) && !(comp is SunLightParamUpdater) && !(comp is Light) && !(comp is Transform))
                {
                    GameObject.Destroy(comp);
                }
            }
            GameObject.Destroy(starLightGO.GetComponent<Light>());
            starLightGO.name = "StarLightController";

            starLightGO.AddComponent<StarLightController>();
            StarLightController.AddStar(starController);

            starLightGO.SetActive(true);

            // Order by stars then planets then moons (not necessary but probably speeds things up, maybe) ALSO only include current star system
            var toLoad = bodies
                .OrderBy(b =>
                (b.Config.BuildPriority != -1 ? b.Config.BuildPriority :
                (b.Config.FocalPoint != null ? 0 :
                (b.Config.Star != null) ? 0 :
                (b.Config.Orbit.IsMoon ? 2 : 1)
                ))).ToList();

            var passCount = 0;
            while (toLoad.Count != 0)
            {
                Logger.Log($"Starting body loading pass #{++passCount}");
                var flagNoneLoadedThisPass = true;
                foreach (var body in toLoad)
                {
                    if (LoadBody(body)) flagNoneLoadedThisPass = false;
                }
                if (flagNoneLoadedThisPass)
                {
                    Logger.LogWarning("No objects were loaded this pass");
                    // Try again but default to sun
                    foreach (var body in toLoad)
                    {
                        if (LoadBody(body, true)) flagNoneLoadedThisPass = false;
                    }
                }
                if (flagNoneLoadedThisPass)
                {
                    // Give up
                    Logger.Log($"Couldn't finish adding bodies.");
                    return;
                }

                toLoad = NextPassBodies;
                NextPassBodies = new List<NewHorizonsBody>();

                // Infinite loop failsafe
                if (passCount > 10)
                {
                    Logger.Log("Something went wrong");
                    break;
                }
            }

            Logger.Log("Done loading bodies");

            // I don't know what these do but they look really weird from a distance
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(PlanetDestroyer.RemoveAllProxies);

            if (Main.Instance.CurrentStarSystem != "SolarSystem") PlanetDestroyer.RemoveSolarSystem();
        }

        public static bool LoadBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            // I don't remember doing this why is it exceptions what am I doing
            GameObject existingPlanet = null;
            try
            {
                Logger.Log("Loading body for " + body.Config.Name);
                existingPlanet = AstroObjectLocator.GetAstroObject(body.Config.Name).gameObject;
            }
            catch (Exception)
            {
                if (body?.Config?.Name == null) Logger.LogError($"How is there no name for {body}");
                else existingPlanet = GameObject.Find(body.Config.Name.Replace(" ", "") + "_Body");
            }

            if (existingPlanet != null)
            {
                try
                {
                    if (body.Config.Destroy)
                    {
                        var ao = existingPlanet.GetComponent<AstroObject>();
                        if (ao != null) Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => PlanetDestroyer.RemoveBody(ao), 2);
                        else Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => existingPlanet.SetActive(false), 2);
                    }
                    else UpdateBody(body, existingPlanet);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't update body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                    return false;
                }
            }
            else
            {
                try
                {
                    GameObject planetObject = GenerateBody(body, defaultPrimaryToSun);
                    if (planetObject == null) return false;
                    Logger.Log("SetActive " + body.Config.Name);
                    planetObject.SetActive(true);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't generate body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                    return false;
                }
            }
            Logger.Log("Loaded body for " + body.Config.Name);
            return true;
        }

        public static GameObject UpdateBody(NewHorizonsBody body, GameObject go)
        {
            Logger.Log($"Updating existing Object {go.name}");

            var sector = go.GetComponentInChildren<Sector>();
            var rb = go.GetAttachedOWRigidbody();

            if (body.Config.ChildrenToDestroy != null && body.Config.ChildrenToDestroy.Length > 0)
            {
                foreach (var child in body.Config.ChildrenToDestroy)
                {
                    Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => GameObject.Find(go.name + "/" + child).SetActive(false), 2);
                }
            }

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, rb);

            // Update a position using CommonResources
            // Since orbits are always there just check if they set a semi major axis
            if (body.Config.Orbit != null && body.Config.Orbit.SemiMajorAxis != 0f)
            {
                OrbitUpdater.Update(body, go);
            }

            return go;
        }

        public static GameObject GenerateBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            AstroObject primaryBody;
            if (body.Config.Orbit.PrimaryBody != null)
            {
                primaryBody = AstroObjectLocator.GetAstroObject(body.Config.Orbit.PrimaryBody);
                if (primaryBody == null)
                {
                    if (defaultPrimaryToSun)
                    {
                        Logger.Log($"Couldn't find {body.Config.Orbit.PrimaryBody}, defaulting to Sun");
                        primaryBody = AstroObjectLocator.GetAstroObject("Sun");
                    }
                    else
                    {
                        NextPassBodies.Add(body);
                        return null;
                    }
                }
            }
            else
            {
                primaryBody = null;
            }

            Logger.Log($"Begin generation sequence of [{body.Config.Name}]");

            var go = new GameObject(body.Config.Name.Replace(" ", "").Replace("'", "") + "_Body");
            go.SetActive(false);

            if (body.Config.Base.GroundSize != 0) GeometryBuilder.Make(go, body.Config.Base.GroundSize);

            var atmoSize = body.Config.Atmosphere != null ? body.Config.Atmosphere.Size : 0f;
            float sphereOfInfluence = Mathf.Max(Mathf.Max(atmoSize, 50), body.Config.Base.SurfaceSize * 2f);
            var overrideSOI = body.Config.Base.SphereOfInfluence;
            if (overrideSOI != 0) sphereOfInfluence = overrideSOI;

            var outputTuple = BaseBuilder.Make(go, primaryBody, body.Config);
            var ao = (AstroObject)outputTuple.Item1;
            var owRigidBody = (OWRigidbody)outputTuple.Item2;

            Logger.Log($"GravityBuilder [{body.Config.Name}]");
            GravityVolume gv = null;
            if (body.Config.Base.SurfaceGravity != 0)
                gv = GravityBuilder.Make(go, ao, body.Config);

            Logger.Log($"RFVolumeBuilder [{body.Config.Name}]");

            if (body.Config.Base.HasReferenceFrame)
                RFVolumeBuilder.Make(go, owRigidBody, sphereOfInfluence);

            Logger.Log($"MarkerBuilder [{body.Config.Name}]");

            if (body.Config.Base.HasMapMarker)
                MarkerBuilder.Make(go, body.Config.Name, body.Config);

            Logger.Log($"AmbientLightBuilder [{body.Config.Name}]");

            if (body.Config.Base.HasAmbientLight)
                AmbientLightBuilder.Make(go, sphereOfInfluence);

            Logger.Log($"MakeSector [{body.Config.Name}]");

            var sector = MakeSector.Make(go, owRigidBody, sphereOfInfluence * 2f);
            ao._rootSector = sector;

            Logger.Log($"VolumesBuilder [{body.Config.Name}]");

            VolumesBuilder.Make(go, body.Config.Base.SurfaceSize, sphereOfInfluence, body.Config);

            Logger.Log($"HeightMapBuilder [{body.Config.Name}]");

            if (body.Config.HeightMap != null)
                HeightMapBuilder.Make(go, body.Config.HeightMap, body.Mod);

            Logger.Log($"ProcGenBuilder [{body.Config.Name}]");

            if (body.Config.ProcGen != null)
                ProcGenBuilder.Make(go, body.Config.ProcGen);

            Logger.Log($"StarBuilder [{body.Config.Name}]");

            if (body.Config.Star != null) StarLightController.AddStar(StarBuilder.Make(go, sector, body.Config.Star));

            Logger.Log($"FocalPointBuilder [{body.Config.Name}]");

            if (body.Config.FocalPoint != null)
                FocalPointBuilder.Make(go, ao, body.Config, body.Mod);

            Logger.Log($"SharedGenerateBody [{body.Config.Name}]");

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, owRigidBody);

            body.Object = go;

            Logger.Log($"UpdatePosition [{body.Config.Name}]");

            // Now that we're done move the planet into place
            UpdatePosition(go, body, primaryBody);

            Logger.Log($"InitialMotionBuilder [{body.Config.Name}]");

            // Have to do this after setting position
            var initialMotion = InitialMotionBuilder.Make(go, primaryBody, owRigidBody, body.Config.Orbit);

            Logger.Log($"SpawnPointBuilder [{body.Config.Name}]");

            // Spawning on other planets is a bit hacky so we do it last
            if (body.Config.Spawn != null)
            {
                Logger.Log("Doing spawn point thing");
                Main.SystemDict[body.Config.StarSystem].SpawnPoint = SpawnPointBuilder.Make(go, body.Config.Spawn, owRigidBody);
            }

            Logger.Log($"OrbitlineBuilder [{body.Config.Name}]");

            if (body.Config.Orbit.ShowOrbitLine && !body.Config.Orbit.IsStatic) OrbitlineBuilder.Make(body.Object, ao, body.Config.Orbit.IsMoon, body.Config);

            Logger.Log($"DetectorBuilder [{body.Config.Name}]");

            if (!body.Config.Orbit.IsStatic) DetectorBuilder.Make(go, owRigidBody, primaryBody, ao, body.Config);

            Logger.Log($"AstroObjectLocator [{body.Config.Name}]");

            if (ao.GetAstroObjectName() == AstroObject.Name.CustomString) AstroObjectLocator.RegisterCustomAstroObject(ao);

            Logger.Log($"HeavenlyBodyBuilder [{body.Config.Name}]");

            HeavenlyBodyBuilder.Make(go, body.Config, sphereOfInfluence, gv, initialMotion);

            Logger.Log($"Planet Created [{body.Config.Name}]");

            return go;
        }

        private static GameObject SharedGenerateBody(NewHorizonsBody body, GameObject go, Sector sector, OWRigidbody rb)
        {
            if (body.Config.Ring != null)
                RingBuilder.Make(go, body.Config.Ring, body.Mod);

            if (body.Config.AsteroidBelt != null)
                AsteroidBeltBuilder.Make(body.Config.Name, body.Config, body.Mod);

            if (body.Config.Base.HasCometTail)
                CometTailBuilder.Make(go, body.Config, go.GetComponent<AstroObject>().GetPrimaryBody());

            // Backwards compatability
            if (body.Config.Base.LavaSize != 0)
            {
                var lava = new LavaModule();
                lava.Size = body.Config.Base.LavaSize;
                LavaBuilder.Make(go, sector, rb, lava);
            }

            if (body.Config.Lava != null)
                LavaBuilder.Make(go, sector, rb, body.Config.Lava);

            // Backwards compatability
            if (body.Config.Base.WaterSize != 0)
            {
                var water = new WaterModule();
                water.Size = body.Config.Base.WaterSize;
                water.Tint = body.Config.Base.WaterTint;
                WaterBuilder.Make(go, sector, rb, water);
            }

            if (body.Config.Water != null)
                WaterBuilder.Make(go, sector, rb, body.Config.Water);

            if (body.Config.Sand != null)
                SandBuilder.Make(go, sector, rb, body.Config.Sand);

            if (body.Config.Atmosphere != null)
            {
                AirBuilder.Make(go, body.Config.Atmosphere.Size, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasOxygen);

                if (body.Config.Atmosphere.Cloud != null)
                {
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere, body.Mod);
                    SunOverrideBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere);
                }

                if (body.Config.Atmosphere.HasRain || body.Config.Atmosphere.HasSnow)
                    EffectsBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere.Size, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasSnow);

                if (body.Config.Atmosphere.FogSize != 0)
                    FogBuilder.Make(go, sector, body.Config.Atmosphere);

                AtmosphereBuilder.Make(go, body.Config.Atmosphere, body.Config.Base.SurfaceSize);
            }

            if (body.Config.Props != null)
                PropBuildManager.Make(go, sector, body.Config, body.Mod, body.Mod.ModHelper.Manifest.UniqueName);

            if (body.Config.Signal != null)
                SignalBuilder.Make(go, sector, body.Config.Signal, body.Mod);

            if (body.Config.Base.BlackHoleSize != 0 || body.Config.Singularity != null)
                SingularityBuilder.Make(go, sector, rb, body.Config);

            if (body.Config.Funnel != null)
                FunnelBuilder.Make(go, go.GetComponentInChildren<ConstantForceDetector>(), rb, body.Config.Funnel);

            // Has to go last probably
            if (body.Config.Base.CloakRadius != 0f)
                CloakBuilder.Make(go, sector, body.Config.Base.CloakRadius);

            return go;
        }

        private static void UpdatePosition(GameObject go, NewHorizonsBody body, AstroObject primaryBody)
        {
            go.transform.parent = Locator.GetRootTransform();
            go.transform.position = CommonResourcesUtilities.GetPosition(body.Config.Orbit) + (primaryBody == null ? Vector3.zero : primaryBody.transform.position);

            if (go.transform.position.magnitude > Main.FurthestOrbit)
            {
                Main.FurthestOrbit = go.transform.position.magnitude + 30000f;
            }
        }
    }
}
