using NewHorizons.Components.Orbital;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.Orbital
{
    public static class FocalPointBuilder
    {
        public static void Make(GameObject go, AstroObject ao, PlanetConfig config, IModBehaviour mod)
        {
            var module = config.FocalPoint;

            bool isTrinary = !string.IsNullOrWhiteSpace(module.Tertiary);
            var binary = isTrinary ? go.AddComponent<TrinaryFocalPoint>() : go.AddComponent<BinaryFocalPoint>();
            binary.PrimaryName = module.Primary;
            binary.SecondaryName = module.Secondary;
            if (isTrinary) ((TrinaryFocalPoint)binary).TertiaryName = module.Tertiary;

            // Below is the stupid fix for making circumbinary planets or wtv

            // Grab the bodies from the main dictionary
            NewHorizonsBody primary = null;
            NewHorizonsBody secondary = null;
            NewHorizonsBody tertiary = null;
            foreach (var body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                if (body.Config.Name == module.Primary)
                {
                    primary = body;
                }
                else if (body.Config.Name == module.Secondary)
                {
                    secondary = body;
                }
                else if (isTrinary && body.Config.Name == module.Tertiary)
                {
                    tertiary = body;
                }
                if (primary != null && secondary != null)
                {
                    if ((isTrinary && tertiary != null) || !isTrinary) break;
                }
            }

            if (isTrinary)
            {
                if (primary == null || secondary == null || tertiary == null)
                {
                    Logger.LogError($"Couldn't make focal point between {module.Primary} and {module.Secondary} and {module.Tertiary}");
                    return;
                }
            }
            else
            {
                if (primary == null || secondary == null)
                {
                    Logger.LogError($"Couldn't make focal point between [{module.Primary} = {primary}] and [{module.Secondary} = {secondary}]");
                    return;
                }
            }

            var gravitationalMass = GetGravitationalMass(primary.Config) + GetGravitationalMass(secondary.Config);

            // Copying it because I don't want to modify the actual config
            var fakeMassConfig = new PlanetConfig();

            // Now need to fake the 3 values to make it return this mass
            fakeMassConfig.Base.SurfaceSize = 1;
            fakeMassConfig.Base.SurfaceGravity = gravitationalMass * GravityVolume.GRAVITATIONAL_CONSTANT;
            fakeMassConfig.Base.GravityFallOff = primary.Config.Base.GravityFallOff;

            // Other stuff to make the fake barycenter not interact with anything in any way
            fakeMassConfig.Name = config.Name + "_FakeBarycenterMass";
            fakeMassConfig.Base.SphereOfInfluence = 0;
            fakeMassConfig.Base.HasMapMarker = false;
            fakeMassConfig.Base.HasReferenceFrame = false;

            fakeMassConfig.Orbit = new OrbitModule();
            fakeMassConfig.Orbit.CopyPropertiesFrom(config.Orbit);

            if (isTrinary)
            {
                var secondGravitationalMass = gravitationalMass + GetGravitationalMass(tertiary.Config);
                // Copying it because I don't want to modify the actual config
                var fakeMassConfig2 = new PlanetConfig();

                // Now need to fake the 3 values to make it return this mass
                fakeMassConfig2.Base.SurfaceSize = 1;
                fakeMassConfig2.Base.SurfaceGravity = secondGravitationalMass * GravityVolume.GRAVITATIONAL_CONSTANT;
                fakeMassConfig2.Base.GravityFallOff = primary.Config.Base.GravityFallOff;

                // Other stuff to make the fake barycenter not interact with anything in any way
                fakeMassConfig2.Name = config.Name + "_FakeBarycenterMass2";
                fakeMassConfig2.Base.SphereOfInfluence = 0;
                fakeMassConfig2.Base.HasMapMarker = false;
                fakeMassConfig2.Base.HasReferenceFrame = false;

                fakeMassConfig2.Orbit = new OrbitModule();
                fakeMassConfig2.Orbit.CopyPropertiesFrom(config.Orbit);

                fakeMassConfig.Orbit.PrimaryBody = fakeMassConfig2.Name;

                ((TrinaryFocalPoint)binary).FakeMassBody2 = PlanetCreationHandler.GenerateBody(new NewHorizonsBody(fakeMassConfig2, mod));
            }

            binary.FakeMassBody = PlanetCreationHandler.GenerateBody(new NewHorizonsBody(fakeMassConfig, mod));
        }

        private static float GetGravitationalMass(PlanetConfig config)
        {
            var surfaceAcceleration = config.Base.SurfaceGravity;
            var upperSurfaceRadius = config.Base.SurfaceSize;
            int falloffExponent = config.Base.GravityFallOff.ToUpper().Equals("LINEAR") ? 1 : 2;

            return surfaceAcceleration * Mathf.Pow(upperSurfaceRadius, falloffExponent) / GravityVolume.GRAVITATIONAL_CONSTANT;
        }
    }
}
