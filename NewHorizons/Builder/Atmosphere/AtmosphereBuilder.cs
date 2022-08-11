using NewHorizons.External.Modules;
using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class AtmosphereBuilder
    {
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");
        private static readonly int SkyColor = Shader.PropertyToID("_SkyColor");
        private static readonly int SunIntensity = Shader.PropertyToID("_SunIntensity");

        public static readonly List<(GameObject, Material)> Skys = new();

        public static void Init()
        {
            Skys.Clear();
        }

        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmosphereModule, float surfaceSize)
        {
            GameObject atmoGO = new GameObject("Atmosphere");
            atmoGO.SetActive(false);
            atmoGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (atmosphereModule.useAtmosphereShader)
            {
                var atmoSphere = SearchUtilities.Find("TimberHearth_Body/Atmosphere_TH/AtmoSphere");
                if (atmoSphere != null)
                {
                    GameObject atmo = GameObject.Instantiate(atmoSphere, atmoGO.transform, true);
                    atmo.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
                    atmo.transform.localScale = Vector3.one * atmosphereModule.size * 1.2f;

                    var renderers = atmo.GetComponentsInChildren<MeshRenderer>();
                    var material = renderers[0].material; // makes a new material
                    foreach (var renderer in renderers)
                    {
                        renderer.sharedMaterial = material;
                    }
                    material.SetFloat(InnerRadius, atmosphereModule.clouds != null ? atmosphereModule.size : surfaceSize);
                    material.SetFloat(OuterRadius, atmosphereModule.size * 1.2f);
                    if (atmosphereModule.atmosphereTint != null) material.SetColor(SkyColor, atmosphereModule.atmosphereTint.ToColor());

                    atmo.SetActive(true);

                    if (atmosphereModule.atmosphereSunIntensity == 0)
                    {
                        // do it based on distance
                        Skys.Add((planetGO, material));
                    }
                    else
                    {
                        // use the override instead
                        material.SetFloat(SunIntensity, atmosphereModule.atmosphereSunIntensity);
                    }
                }
            }

            atmoGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            atmoGO.SetActive(true);
        }
    }
}
