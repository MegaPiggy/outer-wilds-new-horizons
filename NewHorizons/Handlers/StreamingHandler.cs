using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Handlers
{
    /// <summary>
    /// handles streaming meshes so they stay loaded
    /// </summary>
    public static class StreamingHandler
    {
        private static readonly Dictionary<Material, string> _materialCache = new();
        private static readonly Dictionary<GameObject, string[]> _objectCache = new();
        private static readonly Dictionary<string, List<Sector>> _sectorCache = new();

        public static void Init()
        {
            _materialCache.Clear();
            _objectCache.Clear();
            _sectorCache.Clear();
        }

        /// <summary>
        /// makes it so that this object's streaming stuff will be connected to the given sector
        /// </summary>
        public static void SetUpStreaming(GameObject obj, Sector sector)
        {
            // find the asset bundles to load
            // tries the cache first, then builds
            if (!_objectCache.TryGetValue(obj, out var assetBundles))
            {
                var assetBundlesList = new List<string>();

                var tables = Resources.FindObjectsOfTypeAll<StreamingMaterialTable>();
                foreach (var streamingHandle in obj.GetComponentsInChildren<StreamingMeshHandle>())
                {
                    var assetBundle = streamingHandle.assetBundle;
                    assetBundlesList.SafeAdd(assetBundle);

                    if (streamingHandle is StreamingRenderMeshHandle or StreamingSkinnedMeshHandle)
                    {
                        var materials = streamingHandle.GetComponent<Renderer>().sharedMaterials;

                        if (materials.Length == 0) continue;

                        // Gonna assume that if theres more than one material its probably in the same asset bundle anyway right
                        if (_materialCache.TryGetValue(materials[0], out assetBundle))
                        {
                            assetBundlesList.SafeAdd(assetBundle);
                        }
                        else
                        {
                            foreach (var table in tables)
                            {
                                foreach (var lookup in table._materialPropertyLookups)
                                {
                                    if (materials.Contains(lookup.material))
                                    {
                                        _materialCache.SafeAdd(lookup.material, table.assetBundle);
                                        assetBundlesList.SafeAdd(table.assetBundle);
                                    }
                                }
                            }
                        }
                    }
                }

                assetBundles = assetBundlesList.ToArray();
                _objectCache[obj] = assetBundles;
            }

            foreach (var assetBundle in assetBundles)
            {
                StreamingManager.LoadStreamingAssets(assetBundle);

                // Track the sector even if its null. null means stay loaded forever
                if (!_sectorCache.TryGetValue(assetBundle, out var sectors))
                {
                    sectors = new List<Sector>();
                    _sectorCache.Add(assetBundle, sectors);
                }
                sectors.SafeAdd(sector);
            }

            if (sector)
            {
                sector.OnOccupantEnterSector += _ =>
                {
                    if (sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        foreach (var assetBundle in assetBundles)
                            StreamingManager.LoadStreamingAssets(assetBundle);
                };
                /*
                sector.OnOccupantExitSector += _ =>
                {
                    // UnloadStreamingAssets is patched to check IsBundleInUse first before unloading
                    if (!sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        foreach (var assetBundle in assetBundles)
                            StreamingManager.UnloadStreamingAssets(assetBundle);
                };
                */
            }
        }

        public static bool IsBundleInUse(string assetBundle)
        {
            if (_sectorCache.TryGetValue(assetBundle, out var sectors))
                foreach (var sector in sectors)
                    // If a sector in the list is null then it is always in use
                    if (sector == null || sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
                        return true;
            return false;
        }
    }
}