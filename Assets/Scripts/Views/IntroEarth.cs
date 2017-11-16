// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class IntroEarth : MonoBehaviour
    {
        public GameObject[] IntroObjects;
        public GameObject[] NonIntroObjects;
        public float IntroSize;
        public Animator TransitionAnimator;

        private bool IsIntro = false;
        private SceneSizer sizer;

        private void Awake()
        {
            sizer = GetComponent<SceneSizer>();
        }

        public void SetIntroMode(bool intro)
        {
            IsIntro = intro;

            if (IsIntro)
            {
                SetStateOnObjects(IntroObjects, false);
                SetStateOnObjects(NonIntroObjects, false);

                sizer.FullScreenFillPercentage = IntroSize;
            }
            else
            {
                SetStateOnObjects(IntroObjects, false);
                SetStateOnObjects(NonIntroObjects, true);
            }
        }

        public void TurnOnIntroEarth()
        {
            SetStateOnObjects(IntroObjects, true);

            if (TransitionAnimator)
            {
                TransitionAnimator.SetTrigger("Intro");
            }
        }

        public void TransitionFromIntroToReal()
        {
            if (TransitionAnimator)
            {
                TransitionAnimator.SetTrigger("Place");
            }
        }

        public void SetRealMode()
        {
            SetIntroMode(false);
        }

        private void SetStateOnObjects(GameObject[] objects, bool state)
        {
            foreach (GameObject go in objects)
            {
                go.SetActive(state);
            }
        }
    }
}