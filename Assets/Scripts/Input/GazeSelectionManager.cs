// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class GazeSelectionManager : SingleInstance<GazeSelectionManager>
    {
        public GazeSelection GazeSelector;
        public bool LockSelectedTarget = false;
        [Tooltip("If the viewer gazes away from a target for this amount of time, the target will become unselected and can switch to a new target.")]
        public float DelayTimeSwitchingTargets = 0.15f;

        public float timeUnselected = 0.0f;

        private GazeSelectionTarget selectedTarget;

        public GazeSelectionTarget SelectedTarget
        {
            get
            {
                return selectedTarget;
            }

            set
            {
                if (selectedTarget != value)
                {
                    if (selectedTarget != null)
                    {
                        selectedTarget.OnGazeDeselect();
                    }

                    if (value != null)
                    {
                        value.OnGazeSelect();
                    }

                    selectedTarget = value;
                }
            }
        }

        private void Update()
        {
            if (GazeSelector != null && !LockSelectedTarget)
            {
                IList<RaycastHit> targets = GazeSelector.SelectedTargets;
                GazeSelectionTarget desiredTarget = (targets != null && targets.Count > 0 && targets[0].transform != null)
                    ? GetGazeSelectionTarget(targets[0].transform.gameObject)
                    : null;

                // reset our unselected time
                if (desiredTarget == selectedTarget)
                {
                    timeUnselected = 0.0f;
                }
                else
                {
                    timeUnselected += Time.deltaTime;

                    // unselected long enough to have a new target
                    if (timeUnselected >= DelayTimeSwitchingTargets)
                    {
                        SelectedTarget = desiredTarget;
                        timeUnselected = 0.0f;

                        // the selected target change can cause states to change; update selected target once more to ensure that our target really switched
                        // for example: POIs turn different cards on and off and depending on their bounds, the target may swap states repeatedly; this
                        // prevents that from happening
                        GazeSelector.Update();
                        SelectedTarget = desiredTarget = (targets != null && targets.Count > 0 && targets[0].transform != null)
                            ? GetGazeSelectionTarget(targets[0].transform.gameObject)
                            : null;
                    }
                }
            }
        }

        private GazeSelectionTarget GetGazeSelectionTarget(GameObject target)
        {
            GazeSelectionTarget selectionTarget = null;

            while (target != null && selectionTarget == null)
            {
                selectionTarget = target.GetComponent<GazeSelectionTarget>();

                target = target.transform.parent != null
                    ? target.transform.parent.gameObject
                    : null;
            }

            return selectionTarget;
        }
    }
}