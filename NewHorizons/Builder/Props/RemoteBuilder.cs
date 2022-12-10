using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class RemoteBuilder
    {
        private static Material _decalMaterial;
        private static Material _decalMaterialGold;
        private static GameObject _remoteCameraPlatformPrefab;
        private static GameObject _whiteboardPrefab;
        private static GameObject _shareStonePrefab;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_decalMaterial == null)
            {
                _decalMaterial = new Material(Shader.Find("Standard (Decal)")).DontDestroyOnLoad();
                _decalMaterial.name = "Decal";
                _decalMaterial.SetTexture("_MainTex", Texture2D.whiteTexture);
                _decalMaterial.SetTexture("_EmissionMap", Texture2D.whiteTexture);
                _decalMaterial.SetFloat("_Glossiness", 0);
                _decalMaterial.SetFloat("_BumpScale", 0);
                _decalMaterial.SetColor("_Color", new Color(0.3529412f, 0.3843137f, 1));
                _decalMaterial.SetColor("_EmissionColor", new Color(0.2422811f, 0.2917706f, 2.440062f));
                _decalMaterialGold = new Material(_decalMaterial);
                _decalMaterialGold.name = "DecalGold";
                _decalMaterialGold.SetColor("_Color", new Color(1, 0.6392157f, 0.3803922f));
                _decalMaterialGold.SetColor("_EmissionColor", new Color(1, 0.3662527f, 0.1195384f));
            }

            if (_remoteCameraPlatformPrefab == null)
            {
                _remoteCameraPlatformPrefab = SearchUtilities.Find("OrbitalProbeCannon_Body/Sector_OrbitalProbeCannon/Sector_Module_Broken/Interactables_Module_Broken/Prefab_NOM_RemoteViewer").InstantiateInactive().Rename("Prefab_NOM_RemoteViewer").DontDestroyOnLoad();
                var remoteCameraPlatform = _remoteCameraPlatformPrefab.GetComponent<NomaiRemoteCameraPlatform>();
                remoteCameraPlatform.enabled = true;
                remoteCameraPlatform._id = NomaiRemoteCameraPlatform.ID.None;
                remoteCameraPlatform._platformState = NomaiRemoteCameraPlatform.State.Disconnected;
                remoteCameraPlatform._dataPointID = string.Empty;
                remoteCameraPlatform._visualSector = null;
                var AstroBodySymbolRenderer = _remoteCameraPlatformPrefab.FindChild("PedestalAnchor/Prefab_NOM_SharedPedestal/SharedPedestal_side01_bottom_jnt/SharedPedestal_side01_top_jnt/AstroBodySymbolRenderer");
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.parent = AstroBodySymbolRenderer.transform.parent;
                quad.transform.localPosition = AstroBodySymbolRenderer.transform.localPosition;
                quad.transform.localRotation = AstroBodySymbolRenderer.transform.localRotation;
                quad.transform.localScale = AstroBodySymbolRenderer.transform.localScale;
                quad.AddComponent<OWRenderer>();
                quad.GetComponent<MeshRenderer>().sharedMaterial = _decalMaterial;
                quad.name = "AstroBodySymbolRenderer";
                GameObject.DestroyImmediate(AstroBodySymbolRenderer);
            }

            if (_whiteboardPrefab == null)
            {
                _whiteboardPrefab = SearchUtilities.Find("OrbitalProbeCannon_Body/Sector_OrbitalProbeCannon/Sector_Module_Broken/Interactables_Module_Broken/Prefab_NOM_Whiteboard_Shared").InstantiateInactive().Rename("Prefab_NOM_Whiteboard_Shared").DontDestroyOnLoad();
                var whiteboard = _whiteboardPrefab.GetComponent<NomaiSharedWhiteboard>();
                whiteboard.enabled = true;
                whiteboard._id = NomaiRemoteCameraPlatform.ID.None;
                _whiteboardPrefab.FindChild("ArcSocket").transform.DestroyAllChildrenImmediate();
                whiteboard._remoteIDs = new NomaiRemoteCameraPlatform.ID[0];
                whiteboard._nomaiTexts = new NomaiWallText[0];
                var AstroBodySymbolRendererW = _whiteboardPrefab.FindChild("PedestalAnchor/Prefab_NOM_SharedPedestal/SharedPedestal_side01_bottom_jnt/SharedPedestal_side01_top_jnt/AstroBodySymbolRenderer");
                var quadW = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quadW.transform.parent = AstroBodySymbolRendererW.transform.parent;
                quadW.transform.localPosition = AstroBodySymbolRendererW.transform.localPosition;
                quadW.transform.localRotation = AstroBodySymbolRendererW.transform.localRotation;
                quadW.transform.localScale = AstroBodySymbolRendererW.transform.localScale;
                quadW.AddComponent<OWRenderer>();
                quadW.GetComponent<MeshRenderer>().sharedMaterial = _decalMaterial;
                quadW.name = "AstroBodySymbolRenderer";
                GameObject.DestroyImmediate(AstroBodySymbolRendererW);
            }

            if (_shareStonePrefab == null)
            {
                GameObject stone = new GameObject("ShareStoneFallback");
                stone.layer = LayerMask.NameToLayer("Interactible");
                stone.SetActive(false);
                SphereCollider sc = stone.AddComponent<SphereCollider>();
                sc.center = Vector3.zero;
                sc.radius = 0.4f;
                sc.isTrigger = false;
                OWCollider owc = stone.AddComponent<OWCollider>();
                owc._collider = sc;
                SharedStone item = stone.AddComponent<SharedStone>();
                item._connectedPlatform = NomaiRemoteCameraPlatform.ID.None;
                item._animDuration = 0.4f;
                item._animOffsetY = 0.08f;
                GameObject animRoot = new GameObject("AnimRoot");
                animRoot.transform.parent = stone.transform;
                TransformAnimator transformAnimator = animRoot.AddComponent<TransformAnimator>();
                item._animator = transformAnimator;
                OWRenderer renderer = SearchUtilities.FindResourceOfTypeAndName<OWRenderer>("Props_NOM_SharedStone");
                if (renderer != null) GameObject.Instantiate(renderer.gameObject, animRoot.transform);
                GameObject planetDecal = GameObject.CreatePrimitive(PrimitiveType.Quad);
                planetDecal.name = "PlanetDecal";
                planetDecal.transform.parent = animRoot.transform;
                planetDecal.transform.localPosition = new Vector3(0, 0.053f, 0);
                planetDecal.transform.localEulerAngles = new Vector3(90, 0, 0);
                planetDecal.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                planetDecal.AddComponent<OWRenderer>();
                planetDecal.GetComponent<MeshRenderer>().sharedMaterial = _decalMaterialGold;
                _shareStonePrefab = stone.DontDestroyOnLoad();
            }
        }

        public static void Make(GameObject go, Sector sector, PropModule.RemoteInfo info, IModBehaviour mod)
        {
            InitPrefabs();

            var id = RemoteHandler.GetPlatformID(info.id);

            Texture2D decal = Texture2D.whiteTexture;
            if (!string.IsNullOrWhiteSpace(info.decalPath)) decal = ImageUtilities.GetTexture(mod, info.decalPath, false, false);
            else Logger.LogError($"Missing decal path on [{info.id}] for [{go.name}]");

            if (info.platform != null)
            {
                try
                {
                    RemoteBuilder.MakePlatform(go, sector, id, decal, info.platform, mod);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Couldn't make remote platform [{info.id}] for [{go.name}]:\n{ex}");
                }
            }

            if (info.whiteboard != null)
            {
                try
                {
                    RemoteBuilder.MakeWhiteboard(go, sector, id, decal, info.whiteboard, mod);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Couldn't make remote whiteboard [{info.id}] for [{go.name}]:\n{ex}");
                }
            }

            if (info.stones != null)
            {
                foreach (var stoneInfo in info.stones)
                {
                    try
                    {
                        RemoteBuilder.MakeStone(go, sector, id, decal, stoneInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make remote stone [{info.id}] for [{go.name}]:\n{ex}");
                    }
                }
            }
        }

        public static void MakeWhiteboard(GameObject go, Sector sector, NomaiRemoteCameraPlatform.ID id, Texture2D decal, PropModule.RemoteInfo.WhiteboardInfo info, IModBehaviour mod)
        {
            var detailInfo = new PropModule.DetailInfo()
            {
                position = info.position,
                rotation = info.rotation,
                parentPath = info.parentPath,
                isRelativeToParent = info.isRelativeToParent,
                rename = info.rename
            };
            var whiteboard = DetailBuilder.Make(go, sector, _whiteboardPrefab, detailInfo);
            whiteboard.SetActive(false);

            var decalMat = new Material(_decalMaterial);
            decalMat.SetTexture("_MainTex", decal);
            decalMat.SetTexture("_EmissionMap", decal);
            whiteboard.FindChild("PedestalAnchor/Prefab_NOM_SharedPedestal/SharedPedestal_side01_bottom_jnt/SharedPedestal_side01_top_jnt/AstroBodySymbolRenderer").GetComponent<OWRenderer>().sharedMaterial = decalMat;

            var component = whiteboard.GetComponent<NomaiSharedWhiteboard>();
            component._id = id;

            component._remoteIDs = new NomaiRemoteCameraPlatform.ID[info.nomaiText.Length];
            component._nomaiTexts = new NomaiWallText[info.nomaiText.Length];
            for (int i = 0; i < info.nomaiText.Length; i++)
            {
                var textInfo = info.nomaiText[i];
                component._remoteIDs[i] = RemoteHandler.GetPlatformID(textInfo.id);
                var wallText = NomaiTextBuilder.Make(whiteboard, sector, new PropModule.NomaiTextInfo
                {
                    arcInfo = textInfo.arcInfo,
                    location = textInfo.location,
                    parentPath = "ArcSocket",
                    position = new MVector3(0, 1, 0),
                    rename = textInfo.rename,
                    rotation = Vector3.zero,
                    seed = textInfo.seed,
                    type = PropModule.NomaiTextInfo.NomaiTextType.Wall,
                    xmlFile = textInfo.xmlFile
                }, mod).GetComponent<NomaiWallText>();
                wallText._showTextOnStart = false;
                component._nomaiTexts[i] = wallText;
            }

            if (info.disableWall) whiteboard.FindChild("Props_NOM_Whiteboard_Shared").SetActive(false);

            whiteboard.SetActive(true);
        }

        public static void MakePlatform(GameObject go, Sector sector, NomaiRemoteCameraPlatform.ID id, Texture2D decal, PropModule.RemoteInfo.PlatformInfo info, IModBehaviour mod)
        {
            var detailInfo = new PropModule.DetailInfo()
            {
                position = info.position,
                rotation = info.rotation,
                parentPath = info.parentPath,
                isRelativeToParent = info.isRelativeToParent,
                rename = info.rename
            };
            var platform = DetailBuilder.Make(go, sector, _remoteCameraPlatformPrefab, detailInfo);
            platform.SetActive(false);

            var decalMat = new Material(_decalMaterial);
            decalMat.SetTexture("_MainTex", decal);
            decalMat.SetTexture("_EmissionMap", decal);
            platform.FindChild("PedestalAnchor/Prefab_NOM_SharedPedestal/SharedPedestal_side01_bottom_jnt/SharedPedestal_side01_top_jnt/AstroBodySymbolRenderer").GetComponent<OWRenderer>().sharedMaterial = decalMat;

            var component = platform.GetComponent<NomaiRemoteCameraPlatform>();
            component._id = id;
            component._visualSector = sector;
            component._dataPointID = info.reveals;

            if (info.disableStructure)
            {
                platform.FindChild("Structure_NOM_RemoteViewer").SetActive(false);
                platform.FindChild("RemoteViewer_FadeGeo").SetActive(false);
            }

            if (info.disablePool) platform.FindChild("RemoteViewer_Pool").SetActive(false);

            platform.SetActive(true);
        }

        public static void MakeStone(GameObject go, Sector sector, NomaiRemoteCameraPlatform.ID id, Texture2D decal, PropModule.RemoteInfo.StoneInfo info, IModBehaviour mod)
        {
            var shareStone = _shareStonePrefab.InstantiateInactive();

            if (!string.IsNullOrEmpty(info.rename))
            {
                shareStone.name = info.rename;
            }
            else
            {
                shareStone.name = "ShareStone_" + id.ToString();
            }

            shareStone.transform.parent = sector?.transform ?? go.transform;

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = go.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    shareStone.transform.parent = newParent;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {go.name}/{info.parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? Vector3.zero);
            var rot = Quaternion.Euler((Vector3)(info.rotation ?? Vector3.zero));
            if (info.isRelativeToParent)
            {
                shareStone.transform.localPosition = pos;
                shareStone.transform.localRotation = rot;
            }
            else
            {
                shareStone.transform.position = go.transform.TransformPoint(pos);
                shareStone.transform.rotation = go.transform.TransformRotation(rot);
            }

            shareStone.GetComponent<SharedStone>()._connectedPlatform = id;

            var decalMat = new Material(_decalMaterialGold);
            decalMat.SetTexture("_MainTex", decal);
            decalMat.SetTexture("_EmissionMap", decal);
            shareStone.FindChild("AnimRoot/PlanetDecal").GetComponent<OWRenderer>().sharedMaterial = decalMat;

            shareStone.SetActive(true);
        }
    }
}
