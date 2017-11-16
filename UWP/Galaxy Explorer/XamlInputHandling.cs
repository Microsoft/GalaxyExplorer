// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using UnityEngine;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace GalaxyExplorer
{
    class XamlInputHandling
    {
        public void PointerOrSingleFingerReleased(double x, double y, MainPage mainPage)
        {
            float unityX, unityY;

            ConvertToUnityCoordinates(x, y, out unityX, out unityY, mainPage);

            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
                {
                    if (InputRouter.Instance != null)
                    {
                        InputRouter.Instance.XamlMousePosition.x = (float)unityX;
                        InputRouter.Instance.XamlMousePosition.y = (float)unityY;

                        InputRouter.Instance.InternalHandleOnTapped();
                    }
                },
                waitUntilDone: false);
        }

        public void PointerMoved(double x, double y, MainPage mainPage)
        {
            float unityX, unityY;

            ConvertToUnityCoordinates(x, y, out unityX, out unityY, mainPage);

            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
                {
                    if (InputRouter.Instance != null)
                    {
                        InputRouter.Instance.XamlMousePosition.x = (float)unityX;
                        InputRouter.Instance.XamlMousePosition.y = (float)unityY;
                    }
                },
                waitUntilDone: false);
        }

        public void ZoomHappened(double scaleDelta)
        {
            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
                {
                    if (InputRouter.Instance != null)
                    {
                        InputRouter.Instance.HandleZoomFromXaml((float)scaleDelta);
                    }
                }, waitUntilDone: false);
        }

        public void RotationHappened(double rotationDelta)
        {
            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
            {
                if (InputRouter.Instance != null)
                {
                    InputRouter.Instance.HandleRotationFromXaml((float)rotationDelta);
                }
            }, waitUntilDone: false);
        }

        public void TranslateHappened(Vector2 translateDelta)
        {
            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
            {
                if (InputRouter.Instance != null)
                {
                    InputRouter.Instance.HandleTranslateFromXaml(translateDelta);
                }
            }, waitUntilDone: false);
        }

        public void ResetHappened()
        {
            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
            {
                if (InputRouter.Instance != null)
                {
                    InputRouter.Instance.HandleResetFromXaml();
                }
            }, waitUntilDone: false);
        }

        public void AboutHappened()
        {
            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
            {
                if (InputRouter.Instance != null)
                {
                    InputRouter.Instance.HandleAboutFromXaml();
                }
            }, waitUntilDone: false);
        }

        /// <summary>
        /// Unity and XAML have different coordinate systems in two different ways that need to be accounted for.
        ///    1. Y == 0 for the top of the window in XAML and Y == 0 for the bottom of the window in Unity
        ///    2. Unity is using raw pixel values while XAML is using DPI aware logical pixels
        /// </summary>
        private void ConvertToUnityCoordinates(double xamlX, double xamlY, out float unityX, out float unityY, MainPage mainPage)
        {
            double rawPixelsPerViewPixel = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            // Adjust DPI aware xamlX to raw pixel value
            unityX = (float)(xamlX * rawPixelsPerViewPixel);

            // Adjust DPI aware xamlY to raw pixel value and change the origin
            var windowHeight = Window.Current.Bounds.Height - mainPage.BottomAppBar.ActualHeight;
            unityY = (float)((windowHeight - xamlY) * rawPixelsPerViewPixel);
        }
    }
}
