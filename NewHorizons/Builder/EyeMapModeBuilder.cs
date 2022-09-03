using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

namespace NewHorizons.Builder
{
    public static class EyeMapModeBuilder
    {
        private static Texture2D _whiteArrow;
        private static Sprite _whiteArrowSprite;

        public static void Make(GameObject root)
        {
            var mapModeProfile = SearchUtilities.FindResourceOfTypeAndName<PostProcessingProfile>("MapCameraProfile_Runtime");

            var mapCamera = new GameObject("MapCamera");
            mapCamera.transform.SetParent(root.transform, false);
            mapCamera.SetActive(false);
            var camera = mapCamera.AddComponent<Camera>();
            var owCamera = mapCamera.AddComponent<OWCamera>();
            var flareLayer = mapCamera.AddComponent<FlareLayer>();
            var mapController = mapCamera.AddComponent<MapController>();
            var flashbackScreenGrabImageEffect = mapCamera.AddComponent<FlashbackScreenGrabImageEffect>();

            camera.fieldOfView = 60;
            camera.focalLength = 50;
            camera.allowHDR = true;
            camera.allowMSAA = false;
            camera.cameraType = CameraType.Game;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.cullingMask = 1178596351;
            camera.nearClipPlane = 0;
            camera.farClipPlane = 65000;
            camera.orthographicSize = 100;
            camera.useOcclusionCulling = false;

            owCamera._mainCamera = camera;
            owCamera._frustumDirty = true;
            owCamera.farCameraDistance = 120000;
            owCamera.renderSkybox = true;
            owCamera.enabled = false;
            ((MonoBehaviour)owCamera).enabled = true;

            mapController._mapCamera = owCamera;
            mapController._maxPanDistance = 6500;
            mapController._minZoomDistance = 1000;
            mapController._maxZoomDistance = 60000;
            mapController._defaultZoomDist = 30000;
            mapController._initialZoomDist = 100;
            mapController._gridColor = new Color(1, 1, 1, 0.1216f);
            mapController._gridSize = 100;
            mapController._gridLockOnLength = 0.5f;
            mapController._zoomSpeed = 10000;
            mapController._yawSpeed = 90;
            mapController._pitchSpeed = 45;
            mapController.BuildScreenPrompts();

            Locator._mapController = mapController;

            var markerManager = new GameObject("MarkerManager");
            var mmRect = markerManager.AddComponent<RectTransform>();
            mmRect.SetParent(mapCamera.transform, false);
            mmRect.localPosition = new Vector3(0, 0, 100);
            mmRect.localEulerAngles = Vector3.zero;
            mmRect.localScale = Vector3.one * 0.1069f;
            mmRect.sizeDelta = new Vector2(1920, 1080);
            mmRect.anchorMin = Vector2.zero;
            mmRect.anchorMax = Vector2.zero;
            mmRect.offsetMin = new Vector2(-960, -540);
            mmRect.offsetMax = new Vector2(960, 540);
            var mmCanvas = markerManager.AddComponent<Canvas>();
            var mmCanvasScaler = markerManager.AddComponent<CanvasScaler>();
            mmCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            mmCanvasScaler.matchWidthOrHeight = 1;
            mmCanvasScaler.referenceResolution = new Vector2(1920, 1080);
            var mmm = markerManager.AddComponent<MapMarkerManager>();
            mmm._mapCamera = camera;
            var fontAndLanguageController = markerManager.AddComponent<FontAndLanguageController>();
            fontAndLanguageController._rootObjectsWithTextList = new List<FontAndLanguageController.TextItemsRootObject>();
            fontAndLanguageController._textItemList = new List<FontAndLanguageController.TextItem>();
            mmm._markerFontController = fontAndLanguageController;
            mapController._mapMarkerManager = mmm;

            var canvasMarkerWithPointer = new GameObject("CanvasMarkerWithPointer");
            canvasMarkerWithPointer.SetActive(false);
            var cmwpRect = canvasMarkerWithPointer.AddComponent<RectTransform>();
            cmwpRect.SetParent(mmRect, false);
            cmwpRect.anchoredPosition3D = Vector3.zero;
            cmwpRect.localPosition = new Vector3(0, -540f, 0);
            cmwpRect.localEulerAngles = Vector3.zero;
            cmwpRect.localScale = Vector3.one;
            cmwpRect.anchorMin = Vector2.zero;
            cmwpRect.anchorMax = Vector2.one;
            cmwpRect.offsetMin = Vector2.zero;
            cmwpRect.offsetMax = Vector2.zero;
            cmwpRect.pivot = new Vector2(0.5f, 0);
            cmwpRect.sizeDelta = Vector2.zero;
            var cmwpCanvas = canvasMarkerWithPointer.AddComponent<Canvas>();
            var cmwpCanvasMapMarker = canvasMarkerWithPointer.AddComponent<CanvasMapMarker>();
            mmm._locatorMarkerTemplate = canvasMarkerWithPointer;
            MakePointerTarget(cmwpRect, cmwpCanvasMapMarker, true);

            var canvasMarker = new GameObject("CanvasMarker");
            canvasMarker.SetActive(false);
            var cmRect = canvasMarker.AddComponent<RectTransform>();
            cmRect.SetParent(mmRect, false);
            cmRect.anchoredPosition3D = Vector3.zero;
            cmRect.localPosition = new Vector3(0, -540f, 0);
            cmRect.localEulerAngles = Vector3.zero;
            cmRect.localScale = Vector3.one;
            cmRect.anchorMin = Vector2.zero;
            cmRect.anchorMax = Vector2.one;
            cmRect.offsetMin = Vector2.zero;
            cmRect.offsetMax = Vector2.zero;
            cmRect.pivot = new Vector2(0.5f, 0);
            cmRect.sizeDelta = Vector2.zero;
            var cmCanvas = canvasMarker.AddComponent<Canvas>();
            var cmCanvasMapMarker = canvasMarker.AddComponent<CanvasMapMarker>();
            mmm._canvasMarkerTemplate = canvasMarker;
            MakePointerTarget(cmRect, cmCanvasMapMarker);

            var mapLockOnCanvas = new GameObject("MapLockOnCanvas");
            var mlocRect = mapLockOnCanvas.AddComponent<RectTransform>();
            mlocRect.SetParent(mapCamera.transform, false);
            var mlocCanvas = mapLockOnCanvas.AddComponent<Canvas>();
            var mlocCanvasScaler = mapLockOnCanvas.AddComponent<CanvasScaler>();
            mlocCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            mlocCanvasScaler.matchWidthOrHeight = 1;
            mlocCanvasScaler.referenceResolution = new Vector2(1920, 1080);
            var referenceFrameGUI = mapLockOnCanvas.AddComponent<ReferenceFrameGUI>();
            referenceFrameGUI._canvas = mlocCanvas;
            referenceFrameGUI._referenceCanvas = mlocCanvas;
            referenceFrameGUI._approachingColor = new Color(0.4f, 0.502f, 0.8078f, 0.7843f);
            referenceFrameGUI._departingColor = new Color(0.7412f, 0.3176f, 0.3098f, 0.7843f);
            referenceFrameGUI._reticuleColor = Color.white;
            referenceFrameGUI._staticColor = new Color(0.502f, 0.502f, 0.502f, 0.7843f);
            referenceFrameGUI._type = ReferenceFrameGUI.ReferenceFrameGUIType.MAP;
            MakeReferenceFrameGUI(mlocRect, referenceFrameGUI);

            var mapPivotGrid = GameObject.CreatePrimitive(PrimitiveType.Plane);
            mapPivotGrid.name = "MapPivotGrid";
            mapPivotGrid.transform.SetParent(mapCamera.transform, false);
            var gridRenderer = mapPivotGrid.GetComponent<MeshRenderer>();
            gridRenderer.sharedMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Effects_SPA_OrbitGrid_mat");
            gridRenderer.enabled = false;
            mapController._gridRenderer = gridRenderer;
            GameObject.Destroy(mapPivotGrid.GetComponent<MeshCollider>());

            var mapAudioSource = new GameObject("MapAudioSource");
            mapAudioSource.transform.SetParent(mapCamera.transform, false);
            var audioSource = mapAudioSource.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            var owAudioSource = mapAudioSource.AddComponent<OWAudioSource>();
            owAudioSource._audioSource = audioSource;
            owAudioSource._audioLibraryClip = AudioType.NonDiaMapActivation;
            owAudioSource.SetLocalVolume(0);
            owAudioSource.SetMaxVolume(0.6f);
            owAudioSource.SetTrack(OWAudioMixer.TrackName.Map);
            mapController._audioSource = owAudioSource;

            if (mapModeProfile != null)
            {
                var postProcessingBehaviour = mapCamera.AddComponent<PostProcessingBehaviour>();
                postProcessingBehaviour.m_Camera = camera;
                postProcessingBehaviour.profile = mapModeProfile;
                postProcessingBehaviour.m_RenderingInSceneView = false;
                postProcessingBehaviour.OnEnable();
                owCamera._postProcessing = postProcessingBehaviour;
            }

            flashbackScreenGrabImageEffect._downsampleShader = SearchUtilities.FindResourceOfTypeAndName<Shader>("Hidden/DownsampleImageEffect");
            flashbackScreenGrabImageEffect._downsampleMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Hidden/DownsampleImageEffect");

            mapCamera.SetActive(true);
        }

        public static GameObject MakePointerTarget(RectTransform root, CanvasMapMarker canvasMapMarker, bool hasImage = false)
        {
            var pointerTarget = new GameObject("PointerTarget");
            var ptRect = pointerTarget.AddComponent<RectTransform>();
            ptRect.SetParent(root, false);
            ptRect.localPosition = new Vector3(-960, 0, 0);
            ptRect.localEulerAngles = Vector3.zero;
            ptRect.localScale = Vector3.one;
            ptRect.sizeDelta = Vector2.zero;
            canvasMapMarker._onScreenMarkerRoot = ptRect;

            var textObject = new GameObject("Text");
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.SetParent(ptRect, false);
            textRect.localEulerAngles = Vector3.zero;
            textRect.localScale = Vector3.one;
            textRect.anchoredPosition3D = new Vector3(30, -34, 0);
            textRect.localPosition = new Vector3(30, -34, 0);
            textRect.sizeDelta = new Vector2(225, 0);
            textRect.offsetMin = new Vector2(30, -34);
            textRect.offsetMax = new Vector2(255, -34);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.zero;
            textObject.AddComponent<CanvasRenderer>();
            var text = textObject.AddComponent<Text>();
            text.font = SearchUtilities.FindResourceOfTypeAndName<Font>("SpaceMono-Regular");
            text.fontSize = 15;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.resizeTextMinSize = 1;
            canvasMapMarker._textField = text;
            var contentSizeFitter = textObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var attachPoint = new GameObject("AttachPoint");
            var attachPointRect = attachPoint.AddComponent<RectTransform>();
            attachPointRect.SetParent(textRect, false);
            attachPointRect.localPosition = new Vector3(112.5f, 0, 0);
            attachPointRect.localEulerAngles = Vector3.zero;
            attachPointRect.localScale = Vector3.one;
            attachPointRect.sizeDelta = Vector2.zero;

            if (hasImage)
            {
                if (_whiteArrow == null)
                {
                    _whiteArrow = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/WhiteArrow.png");
                    _whiteArrowSprite = Sprite.Create(_whiteArrow, new Rect(0, 0, _whiteArrow.width, _whiteArrow.height), new Vector2(_whiteArrow.width / 2, _whiteArrow.height / 2));
                    _whiteArrowSprite.name = _whiteArrow.name;
                }

                var imageObject = new GameObject("Image");
                var imageRect = imageObject.AddComponent<RectTransform>();
                imageRect.SetParent(ptRect, false);
                imageRect.anchoredPosition3D = Vector3.zero;
                imageRect.localPosition = Vector3.zero;
                imageRect.localEulerAngles = new Vector3(0, 0, 225);
                imageRect.localScale = new Vector3(1, -1, 1);
                imageRect.sizeDelta = Vector2.one * 30;
                imageRect.pivot = new Vector2(0.5f, 1);
                imageRect.anchorMin = new Vector2(0.5f, 1);
                imageRect.anchorMax = new Vector2(0.5f, 1);
                imageRect.offsetMin = new Vector2(-15, -30);
                imageRect.offsetMax = new Vector2(15, 0);
                imageObject.AddComponent<CanvasRenderer>();
                var image = imageObject.AddComponent<Image>();
                image.sprite = _whiteArrowSprite;
                canvasMapMarker._pointerImg = image;
            }

            return pointerTarget;
        }

        public static void MakeReferenceFrameGUI(RectTransform root, ReferenceFrameGUI rfGUI)
        {
            var originalUI = SearchUtilities.Find("PlayerHUD/HelmetOffUI/HelmetOffLockOn/LockOnGUI");
            var originalReticule1 = originalUI.FindChild("Reticule1");
            var originalReticule2 = originalUI.FindChild("Reticule2");
            var originalOffScreenIndicator = originalUI.FindChild("OffScreenIndicator");

            var reticule1 = originalReticule1.InstantiateInactive();
            reticule1.transform.SetParent(root, false);
            rfGUI._reticule1 = reticule1.GetComponent<LockOnReticule>();
            var reticule2 = originalReticule2.InstantiateInactive();
            reticule2.transform.SetParent(root, false);
            rfGUI._reticule2 = reticule2.GetComponent<LockOnReticule>();
            var offScreenIndicator = originalOffScreenIndicator.InstantiateInactive();
            offScreenIndicator.transform.SetParent(root, false);
            rfGUI._offScreenIndicator = offScreenIndicator.GetComponent<OffScreenIndicator>();
        }
    }
}
