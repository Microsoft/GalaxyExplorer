// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using UnityEngine;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace GalaxyExplorer
{
    class InputHandling
    {
        public void PointerOrSingleFingerReleased(double x, double y)
        {
            float unityX, unityY;

            ConvertToUnityCoordinates(x, y, out unityX, out unityY);

            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
                {
                    if (InputRouter.Instance != null)
                    {
                        InputRouter.Instance.xamlMousePosition.x = (float)unityX;
                        InputRouter.Instance.xamlMousePosition.y = (float)unityY;

                        InputRouter.Instance.OnTapped(
                            UnityEngine.VR.WSA.Input.InteractionSourceKind.Other,
                            0,
                            Camera.main.ScreenPointToRay(InputRouter.Instance.xamlMousePosition));
                    }
                },
                waitUntilDone: false);
        }

        public void PointerMoved(double x, double y)
        {
            float unityX, unityY;

            ConvertToUnityCoordinates(x, y, out unityX, out unityY);

            UnityPlayer.AppCallbacks.Instance.InvokeOnAppThread(() =>
                {
                    if (InputRouter.Instance != null)
                    {
                        InputRouter.Instance.xamlMousePosition.x = (float)unityX;
                        InputRouter.Instance.xamlMousePosition.y = (float)unityY;
                    }
                },
                waitUntilDone: false);
        }

        /// <summary>
        /// Unity and XAML have different coordinate systems in two different ways that need to be accounted for.
        ///    1. Y == 0 for the top of the window in XAML and Y == 0 for the bottom of the window in Unity
        ///    2. Unity is using raw pixel values while XAML is using DPI aware logical pixels
        /// </summary>
        private void ConvertToUnityCoordinates(double xamlX, double xamlY, out float unityX, out float unityY)
        {
            double rawPixelsPerViewPixel = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            // Adjust DPI aware xamlX to raw pixel value
            unityX = (float)(xamlX * rawPixelsPerViewPixel);
           
            // Adjust DPI aware xamlY to raw pixel value and change the origin
            unityY = (float)((Window.Current.Bounds.Height - xamlY) * rawPixelsPerViewPixel);
        }
    }
}
