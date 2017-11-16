// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class PointOfInterestCollider : GazeSelectionTarget
    {
        public PointOfInterest SelectionPOITarget;

        private void Start()
        {
            if (SelectionPOITarget == null)
            {
                Debug.LogError("PointOfInterestCollider: No PointOfInterest was specified for the, '" + name + "' PointOfInterestCollider - component does nothing.");
                Destroy(this);
                return;
            }
        }

        public override void OnGazeSelect()
        {
            SelectionPOITarget.OnGazeSelect();
        }

        public override void OnGazeDeselect()
        {
            SelectionPOITarget.OnGazeDeselect();
        }

        public override bool OnTapped()
        {
            SelectionPOITarget.OnTapped();
            return true;
        }
    }
}