// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class ToolManager : SingleInstance<ToolManager>
    {
        public Tool SelectedTool = null;
        public GameObject BackButton;
        public GameObject ShowButton;
        public GameObject HideButton;
        public float TargetMinZoomSize = 0.15f;
        public float LargestZoom = 3.0f;

        [HideInInspector]
        public bool ToolsVisible = false;

        public event Action ContentZoomChanged;

        private bool locked = false;
        private ToolPanel panel;
        private ToolSounds toolSounds;

        public bool IsLocked
        {
            get { return locked; }
        }

        private float smallestZoom;

        public float SmallestZoom
        {
            get { return smallestZoom; }
        }

        private void Awake()
        {
            if (TransitionManager.Instance == null)
            {
                Debug.LogWarning("ToolManager: No TransitionManager was found, so the zoom tool will not properly size content - transition manager is needed to identify when new content has loaded.");
            }

            smallestZoom = TargetMinZoomSize;
            panel = GetComponent<ToolPanel>();

            if (panel == null)
            {
                Debug.LogError("ToolManager couldn't find ToolPanel. Hiding and showing of Tools unavailable.");
            }

            toolSounds = GetComponentInChildren<ToolSounds>();

            if (toolSounds == null)
            {
                Debug.LogError("ToolManager couldn't find ToolSounds.");
            }

            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.Desktop ||
                MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.Phone)
            {
                HideTools(true);
            }
        }

        private void Start()
        {
            if (TransitionManager.Instance)
            {
                TransitionManager.Instance.ContentLoaded += ViewContentLoaded;
            }

            // It would be nice if we had callbacks for registering voice
            // commands, but it requires finding game objects that are not
            // active; this bypasses that issue by getting all of the children
            // gaze selection targets and manually registering them in the tool
            // panel which is active
            GazeSelectionTarget[] selectionTargets = GetComponentsInChildren<GazeSelectionTarget>(true);
            foreach (GazeSelectionTarget selectionTarget in selectionTargets)
            {
                selectionTarget.RegisterVoiceCommands();
            }

            ShowButton.SetActive(false);
            BackButton.SetActive(false);
            toolSounds.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            if (TransitionManager.Instance)
            {
                TransitionManager.Instance.ContentLoaded -= ViewContentLoaded;
            }
            base.OnDestroy();
        }

        private void ViewContentLoaded()
        {
            if (ViewLoader.Instance != null)
            {
                GameObject content = ViewLoader.Instance.GetCurrentContent();
                SceneSizer contentSizer = content.GetComponent<SceneSizer>();
                smallestZoom = Mathf.Max(content.transform.localScale.x, content.transform.localScale.y, content.transform.localScale.z);

                // make sure all content is to the same scale by removing the fill percentage, so this is the scale that fits to view volume
                if (contentSizer != null)
                {
                    smallestZoom /= contentSizer.FullScreenFillPercentage;
                }

                // adjust the smallest zoom from the content's loaded state to our target min zoom size (currently fitted to view volume)
                if (TransitionManager.Instance != null)
                {
                    smallestZoom *= TargetMinZoomSize / Mathf.Max(TransitionManager.Instance.ViewVolume.transform.lossyScale.x, TransitionManager.Instance.ViewVolume.transform.lossyScale.y, TransitionManager.Instance.ViewVolume.transform.lossyScale.z);
                }
            }
        }

        // prevents tools from being accessed
        public void LockTools()
        {
            if (!locked)
            {
                UnselectAllTools();
                locked = true;
            }
        }

        // re-enables tool access
        public void UnlockTools()
        {
            locked = false;
        }

        public void UnselectAllTools(bool removeHighlight = true)
        {
            SelectedTool = null;

            Tool[] tools = GetComponentsInChildren<Tool>();
            foreach (Tool tool in tools)
            {
                if (removeHighlight)
                {
                    tool.RemoveHighlight();
                }

                tool.Unselect();
            }

            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                button.RemoveHighlight();
            }
        }

        public bool SelectTool(Tool tool)
        {
            if (locked)
            {
                return false;
            }

            UnselectAllTools(removeHighlight: false);
            SelectedTool = tool;

            if (Cursor.Instance)
            {
                Cursor.Instance.ApplyToolState(tool.type);
            }

            return true;
        }

        public bool DeselectTool(Tool tool)
        {
            if (locked)
            {
                return false;
            }

            if (Cursor.Instance)
            {
                Cursor.Instance.ClearToolState();
            }

            if (SelectedTool == tool)
            {
                SelectedTool = null;
                return true;
            }

            return false;
        }

        public void LowerTools()
        {
            panel.IsLowered = true;

            if (ShowButton && HideButton)
            {
                ToolSounds.Instance.PlayMoveToolsDownSound();
                ShowButton.SetActive(true);
                HideButton.SetActive(false);
            }
        }

        public void RaiseTools()
        {
            panel.IsLowered = false;

            if (ShowButton && HideButton)
            {
                ToolSounds.Instance.PlayMoveToolsUpSound();
                ShowButton.SetActive(false);
                HideButton.SetActive(true);
            }
        }

        public void ToggleTools()
        {
            if (panel.IsLowered)
            {
                RaiseTools();
            }
            else
            {
                LowerTools();
            }
        }

        [ContextMenu("Hide Tools")]
        public void HideTools()
        {
            HideTools(false);
        }

        public void HideTools(bool instant)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HideToolsAsync(instant));
            }
        }

        [ContextMenu("Show Tools")]
        public void ShowTools()
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowToolsAsync());
        }

        public IEnumerator HideToolsAsync(bool instant)
        {
            ToolsVisible = false;
            yield return StartCoroutine(panel.FadeOut(instant));
            GetComponentInChildren<ToolSounds>(true).gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public IEnumerator ShowToolsAsync()
        {
            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.HoloLens ||
                MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                ToolsVisible = true;
                GetComponentInChildren<ToolSounds>(true).gameObject.SetActive(true);
                yield return StartCoroutine(panel.FadeIn());
            }
        }

        public Tool FindToolByType(ToolType type)
        {
            Tool[] tools = gameObject.GetComponentsInChildren<Tool>(true);
            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i].type == type)
                {
                    return tools[i];
                }
            }
            return null;
        }

        public Button FindButtonByType(ButtonType type)
        {
            Button[] buttons = gameObject.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].type == type)
                {
                    return buttons[i];
                }
            }
            return null;

        }

        public void ShowBackButton()
        {
            if (ToolManager.BackButtonVisibilityChangeRequested != null)
            {
                ToolManager.BackButtonVisibilityChangeRequested(visible: true);
            }
            else if (BackButton)
            {
                BackButton.SetActive(true);
            }
        }

        public void HideBackButton()
        {
            if (ToolManager.BackButtonVisibilityChangeRequested != null)
            {
                ToolManager.BackButtonVisibilityChangeRequested(visible: false);
            }
            else if (BackButton)
            {
                BackButton.SetActive(false);
            }
        }

        public void UpdateZoomFromXaml(float delta)
        {
            if (!IsLocked)
            {
                // TODO: Normalize this zoom code with what is in Tool.HandleUpdatedInput
                float newScale = ViewLoader.Instance.GetCurrentContent().transform.localScale.x * delta;

                if (newScale < ToolManager.Instance.SmallestZoom)
                {
                    newScale = ToolManager.Instance.SmallestZoom;
                }

                if (newScale > ToolManager.Instance.LargestZoom)
                {
                    newScale = ToolManager.Instance.LargestZoom;
                }

                ViewLoader.Instance.GetCurrentContent().transform.localScale = new Vector3(newScale, newScale, newScale);
                RaiseContentZoomChanged();
            }
        }

        /// <summary>
        /// Uses input from Xaml to rotate the current content
        /// </summary>
        /// <param name="direction">The direction to rotate; we only care about the sign (+/-) of this value.</param>
        public void UpdateRotationFromXaml(int direction)
        {
            if (!IsLocked)
            {
                // TODO: Normalize this rotate code with what is in Tool.HandleUpdatedInput
                var contentToManipulate = ViewLoader.Instance.GetCurrentContent();
                Tool rotateTool = FindToolByType(ToolType.Rotate);
                if (rotateTool == null)
                {
                    Debug.Log("Couldn't find the rotate tool.");
                    return;
                }
                var cam = Camera.main;
                var toContent = (contentToManipulate.transform.position - cam.transform.position).normalized;
                var right = Vector3.Cross(Vector3.up, toContent).normalized;

                var targetUp = Quaternion.AngleAxis(direction * rotateTool.MaxRotationAngle, right) * Vector3.up;

                float rotationSpeed = rotateTool.XamlRotationSpeed;

                // use the hero view to determine limits on rotation; however, move the content by the
                // change in rotation, so we are consistently moving the same content/object everywhere
                // (works with resetting content to hero view for example)
                GameObject heroView = ViewLoader.Instance.GetHeroView();
                var desiredUp = Vector3.Slerp(heroView.transform.up, targetUp, Mathf.Clamp01(Time.deltaTime * rotationSpeed));
                var upToNewUp = Quaternion.FromToRotation(heroView.transform.up, desiredUp);

                contentToManipulate.transform.rotation =
                    Quaternion.LookRotation(upToNewUp * heroView.transform.forward, desiredUp) *
                        Quaternion.Inverse(heroView.transform.rotation) * // hero view rotation delta
                        contentToManipulate.transform.rotation;
            }
        }

        public void RaiseContentZoomChanged()
        {
            if (ContentZoomChanged != null)
            {
                ContentZoomChanged();
            }
        }

        public delegate void ButtonVisibilityRequest(bool visible);
        public static event ButtonVisibilityRequest BackButtonVisibilityChangeRequested;
    }
}
