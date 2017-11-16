// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class OrbitPicker : GazeSelectionTarget
    {
        public PointOfInterest pointOfInterest;
        private MeshCollider orbitMesh;
        private GameObject displayCard;

        private void Start()
        {
            orbitMesh = GetComponent<MeshCollider>();
            if (orbitMesh && pointOfInterest)
            {
                // Create focus object that'll face the camera
                var focus = new GameObject("OrbitFocus");
                focus.transform.SetParent(transform);
                var faceCamera = focus.AddComponent<FaceCamera>();
                faceCamera.forceToWorldUp = true;

                // Create the display text and parent it to the focus object to ensure that
                // the text will always be facing the camera
                displayCard = Instantiate(pointOfInterest.Description);
                displayCard.transform.SetParent(focus.transform, worldPositionStays: false);
                displayCard.transform.rotation = Quaternion.Euler(0, 180, 0);
                displayCard.SetActive(false);
            }
        }

        public override void OnGazeSelect()
        {
            Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hitInfo;
            if (orbitMesh && orbitMesh.Raycast(cameraRay, out hitInfo, 1000.0f))
            {
                displayCard.transform.position = hitInfo.point;
                displayCard.SetActive(true);
            }
        }

        public override void OnGazeDeselect()
        {
            if (orbitMesh)
            {
                displayCard.SetActive(false);
            }
        }

        public override bool OnTapped()
        {
            if (orbitMesh)
            {
                pointOfInterest.GoToScene();
                displayCard.SetActive(false);
                return true;
            }

            return false;
        }
    }
}