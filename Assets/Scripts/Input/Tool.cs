// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using System.Collections.Generic;

namespace GalaxyExplorer
{
    public enum ToolType
    {
        Pan,
        Rotate,
        Zoom,
        Reset,
        About
    }

    public class Tool : GazeSelectionTarget, IFadeTarget
    {
        private static bool hasGaze = false;

        public GameObject TooltipObject;
        public Material DefaultMaterial;
        private Dictionary<string, float> defaultMaterialDefaults = new Dictionary<string, float>();
        public Material HighlightMaterial;
        private Dictionary<string, float> highlightMaterialDefaults = new Dictionary<string, float>();
        public Material SelectedMaterial;
        private Dictionary<string, float> selectedMaterialDefaults = new Dictionary<string, float>();
        public ToolType type;
        public float PanSpeed = 0.25f;
        public float RotationSpeed = 30.0f;
        public float ScaleSpeed = 1.0f;
        public float PanControllerSpeed = 0.75f;
        public float PanHandSpeed = 2.0f;

        public float MaxRotationAngle = 40;
        public float ClickerRotationSpeed = .1f;
        public float XamlRotationSpeed = 1f;

        private float scalePercentValue = 0.6f;
        private GameObject contentToManipulate;
        private bool selected = false;
        private MeshRenderer meshRenderer;

        private float currentOpacity = 1;

        public float Opacity
        {
            get
            {
                return currentOpacity;
            }

            set
            {
                currentOpacity = value;

                ApplyOpacity(DefaultMaterial, value);
                ApplyOpacity(HighlightMaterial, value);
                ApplyOpacity(SelectedMaterial, value);
            }
        }

        private void ApplyOpacity(Material material, float value)
        {
            value = Mathf.Clamp01(value);

            if (material)
            {
                material.SetFloat("_TransitionAlpha", value);
                material.SetInt("_SRCBLEND", value < 1 ? (int)UnityEngine.Rendering.BlendMode.SrcAlpha : (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DSTBLEND", value < 1 ? (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha : (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWRITE", value < 1 ? 0 : 1);
            }
        }

        private void CacheMaterialDefaultAttributes(ref Dictionary<string, float> dict, Material mat)
        {
            dict.Add("_TransitionAlpha", mat.GetFloat("_TransitionAlpha"));
            dict.Add("_SRCBLEND", (float)mat.GetInt("_SRCBLEND"));
            dict.Add("_DSTBLEND", (float)mat.GetInt("_DSTBLEND"));
            dict.Add("_ZWRITE", (float)mat.GetInt("_ZWRITE"));
        }

        private void RestoreMaterialDefaultAttributes(ref Dictionary<string, float> dict, Material mat)
        {
            mat.SetFloat("_TransitionAlpha", dict["_TransitionAlpha"]);
            mat.SetInt("_SRCBLEND", (int)dict["_SRCBLEND"]);
            mat.SetInt("_DSTBLEND", (int)dict["_DSTBLEND"]);
            mat.SetInt("_ZWRITE", (int)dict["_ZWRITE"]);
        }

        private void Awake()
        {
            if (DefaultMaterial == null)
            {
                Debug.LogWarning(gameObject.name + " Tool has no active material.");
            }
            else
            {
                CacheMaterialDefaultAttributes(ref defaultMaterialDefaults, DefaultMaterial);
            }

            if (HighlightMaterial == null)
            {
                Debug.LogWarning(gameObject.name + " Tool has no highlight material.");
            }
            else
            {
                CacheMaterialDefaultAttributes(ref highlightMaterialDefaults, HighlightMaterial);
            }

            if (SelectedMaterial == null)
            {
                Debug.LogWarning(gameObject.name + " Tool has no selected material.");
            }
            else
            {
                CacheMaterialDefaultAttributes(ref selectedMaterialDefaults, SelectedMaterial);
            }

            meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (meshRenderer == null)
            {
                Debug.LogWarning(gameObject.name + " Tool has no renderer.");
            }
        }

        private void Start()
        {
            if (TooltipObject != null)
            {
                TooltipObject.SetActive(false);
            }

            if (PlayerInputManager.Instance != null)
            {
                PlayerInputManager.Instance.TapPressAction += PlayEngagedSound;
                PlayerInputManager.Instance.TapReleaseAction += PlayDisengagedSound;
            }
        }

        private void OnDestroy()
        {
            if (PlayerInputManager.Instance != null)
            {
                PlayerInputManager.Instance.TapPressAction -= PlayEngagedSound;
                PlayerInputManager.Instance.TapReleaseAction -= PlayDisengagedSound;
            }

            RestoreMaterialDefaultAttributes(ref defaultMaterialDefaults, DefaultMaterial);
            RestoreMaterialDefaultAttributes(ref highlightMaterialDefaults, HighlightMaterial);
            RestoreMaterialDefaultAttributes(ref selectedMaterialDefaults, SelectedMaterial);
        }

        public void Highlight()
        {
            if (!ToolManager.Instance.IsLocked)
            {
                if (!selected)
                {
                    ToolSounds.Instance.PlayHighlightSound();
                    meshRenderer.material = HighlightMaterial;
                }

                if (TooltipObject != null)
                {
                    TooltipObject.SetActive(true);
                }
            }
        }

        public void RemoveHighlight()
        {
            if (!ToolManager.Instance.IsLocked)
            {
                ToolSounds.Instance.PlayRemoveHighlightSound();

                if (selected)
                {
                    meshRenderer.material = SelectedMaterial;
                }
                else
                {
                    meshRenderer.material = DefaultMaterial;
                }

                if (TooltipObject != null)
                {
                    TooltipObject.SetActive(false);
                }
            }
        }

        public void Select()
        {
            if (!ToolManager.Instance.IsLocked)
            {
                selected = ToolManager.Instance.SelectTool(this);

                if (selected)
                {
                    ToolSounds.Instance.PlaySelectSound();
                    InputRouter.Instance.InputUpdated += HandleUpdatedInput;
                    contentToManipulate = ViewLoader.Instance.GetCurrentContent();
                    meshRenderer.material = SelectedMaterial;

                    if (type == ToolType.About)
                    {
                        var aboutSlate = GetComponent<ButtonSlateConnection>();
                        if (aboutSlate)
                        {
                            aboutSlate.Show();
                        }
                    }
                }
                else
                {
                    ToolSounds.Instance.PlayDisabledSelectSound();
                }

                if (type == ToolType.Zoom &&
                    MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD &&
                    MixedRealityTeleport.Instance)
                {
                    MixedRealityTeleport.Instance.EnableRotation = !selected;
                }
            }
        }

        public void Unselect()
        {
            if (!ToolManager.Instance.IsLocked)
            {
                if (selected)
                {
                    InputRouter.Instance.InputUpdated -= HandleUpdatedInput;

                    ToolSounds.Instance.PlayDeselectSound();

                    if (type == ToolType.About)
                    {
                        var aboutSlate = GetComponent<ButtonSlateConnection>();
                        if (aboutSlate)
                        {
                            aboutSlate.Hide();
                        }
                    }
                }

                contentToManipulate = null;
                ToolManager.Instance.DeselectTool(this);
                selected = false;
                meshRenderer.material = DefaultMaterial;

                if (type == ToolType.Zoom &&
                    MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD &&
                    MixedRealityTeleport.Instance)
                {
                    MixedRealityTeleport.Instance.EnableRotation = !selected;
                }
            }
        }

        private void HandleUpdatedInput(InteractionSourceKind kind, Vector3 direction, Ray ray)
        {
            if (!contentToManipulate)
            {
                return;
            }

            float y = 0.0f;

            switch (type)
            {
                case ToolType.Pan:
                    y = direction.y;

                    contentToManipulate.transform.localPosition = new Vector3(contentToManipulate.transform.localPosition.x, contentToManipulate.transform.localPosition.y + (Time.deltaTime * y * PanSpeed), contentToManipulate.transform.localPosition.z);
                    break;

                case ToolType.Rotate:
                    y = direction.y;

                    if (kind == InteractionSourceKind.Hand)
                    {
                        y = -y;
                    }

                    Vector3 rotationOrigin;
                    if (kind == InteractionSourceKind.Controller &&
                        MotionControllerInput.Instance &&
                        MotionControllerInput.Instance.UseAlternateGazeRay)
                    {
                        rotationOrigin = MotionControllerInput.Instance.AlternateGazeRayControlerInformation.position;
                    }
                    else
                    {
                        rotationOrigin = Camera.main.transform.position;
                    }
                    var toContent = (contentToManipulate.transform.position - rotationOrigin).normalized;
                    var right = Vector3.Cross(Vector3.up, toContent).normalized;

                    var targetUp = Quaternion.AngleAxis(Mathf.Sign(y) * MaxRotationAngle, right) * Vector3.up;

                    var currentRotationSpeed = kind == InteractionSourceKind.Hand ? RotationSpeed : ClickerRotationSpeed;

                    // use the hero view to determine limits on rotation; however, move the content by the
                    // change in rotation, so we are consistently moving the same content/object everywhere
                    // (works with resetting content to hero view for example)
                    GameObject heroView = ViewLoader.Instance.GetHeroView();
                    var desiredUp = Vector3.Slerp(heroView.transform.up, targetUp, Mathf.Clamp01(Time.deltaTime * Mathf.Abs(y) * currentRotationSpeed));
                    var upToNewUp = Quaternion.FromToRotation(heroView.transform.up, desiredUp);

                    contentToManipulate.transform.rotation =
                        Quaternion.LookRotation(upToNewUp * heroView.transform.forward, desiredUp) *
                        Quaternion.Inverse(heroView.transform.rotation) * // hero view rotation delta
                        contentToManipulate.transform.rotation;
                    break;

                case ToolType.Zoom:
                    float smallestScale = ToolManager.Instance.TargetMinZoomSize;

                    Bounds currentBounds = GetContentBounds();

                    float contentXSize = currentBounds.extents.x == 0 ? smallestScale : currentBounds.extents.x;
                    float zoomHandDistanceFactor = Mathf.Abs(Mathf.Pow(direction.x, 3)) * Mathf.Sign(direction.x);
                    float zoomContentSizeFactor = contentXSize / smallestScale;
                    float contentScalar = ToolManager.Instance.SmallestZoom / ToolManager.Instance.TargetMinZoomSize;
                    float newScale = contentToManipulate.transform.localScale.x + (Time.deltaTime * zoomHandDistanceFactor * ScaleSpeed * zoomContentSizeFactor * scalePercentValue * contentScalar);

                    if (newScale < ToolManager.Instance.SmallestZoom)
                    {
                        newScale = ToolManager.Instance.SmallestZoom;
                    }

                    if (newScale > ToolManager.Instance.LargestZoom)
                    {
                        newScale = ToolManager.Instance.LargestZoom;
                    }

                    contentToManipulate.transform.localScale = new Vector3(newScale, newScale, newScale);
                    ToolManager.Instance.RaiseContentZoomChanged();
                    break;
            }
        }

        private void ToolAction()
        {
            if (selected)
            {
                Unselect();
            }
            else
            {
                Select();
            }
        }

        public static Bounds GetContentBounds()
        {
            Bounds currentBounds = new Bounds();
            if (ViewLoader.Instance.GetCurrentContent() != null)
            {
                var content = ViewLoader.Instance.GetCurrentContent();
                SceneSizer contentSizer = content.GetComponent<SceneSizer>();
                if (contentSizer != null && contentSizer.TargetFillCollider != null)
                {
                    Renderer r = contentSizer.TargetFillCollider.GetComponent<Renderer>();
                    if (r != null)
                    {
                        currentBounds = r.bounds;
                    }
                    else
                    {
                        currentBounds = contentSizer.TargetFillCollider.bounds;
                    }
                }
            }

            return currentBounds;
        }

        public override void OnGazeSelect()
        {
            hasGaze = true;
            Highlight();
        }

        public override void OnGazeDeselect()
        {
            hasGaze = false;
            RemoveHighlight();
        }

        public override bool OnTapped()
        {
            if (ToolManager.Instance.IsLocked)
            {
                return false;
            }

            ToolAction();
            return true;
        }

        protected override void VoiceCommandCallback(string command)
        {
            if (!TransitionManager.Instance.InTransition)
            {
                ToolAction();
            }
        }

        public void PlayEngagedSound()
        {
            // Don't play noise if looking at tool or button
            if (ToolManager.Instance && ToolManager.Instance.SelectedTool && !hasGaze && !Button.HasGaze)
            {
                ToolSounds.Instance.PlayEngagedSound();
            }
        }

        public void PlayDisengagedSound()
        {
            // Don't play noise if looking at tool or button
            if (ToolManager.Instance && ToolManager.Instance.SelectedTool && !hasGaze && !Button.HasGaze)
            {
                ToolSounds.Instance.PlayDisengagedSound();
            }
        }
    }
}