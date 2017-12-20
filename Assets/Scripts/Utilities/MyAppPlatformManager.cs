// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;
#if WINDOWS_UWP
using Windows.Foundation.Metadata;
#endif

namespace GalaxyExplorer
{
    public class MyAppPlatformManager : SingleInstance<MyAppPlatformManager>
    {
        public enum PlatformId
        {
            HoloLens,
            ImmersiveHMD,
            Desktop,
            Phone
        };

        public static PlatformId Platform { get; private set; }

        public static float SlateScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 3.0f;
                    case PlatformId.HoloLens:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float MagicWindowScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 3.0f;
                    case PlatformId.HoloLens:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float OrbitalTrailFixedWidth
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 0.0035f;
                    case PlatformId.HoloLens:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float GalaxyScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 3.0f;
                    case PlatformId.HoloLens:
                        return 1.0f;
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 0.75f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float SolarSystemScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                    case PlatformId.HoloLens:
                        return 1.0f;
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 0.35f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float PoiMoveFactor
        {
            get
            {
                float moveFactor = 1f;
                if (ViewLoader.Instance.CurrentView.Equals("SolarSystemView"))
                {
                    moveFactor *= SolarSystemScaleFactor;
                }
                else if (ViewLoader.Instance.CurrentView.Equals("GalaxyView"))
                {
                    moveFactor *= GalaxyScaleFactor;
                }
                return moveFactor;
            }
        }

        public static float PoiScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 3.0f;
                    case PlatformId.HoloLens:
                        return 1.0f;
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 0.75f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float SpiralGalaxyTintMultConstant
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 0.22f;
                    case PlatformId.HoloLens:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 0.3f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static event Action MyAppPlatformManagerInitialized;

        void Awake()
        {
            Platform = PlatformId.Desktop;

            if (XRDevice.isPresent)
            {
                if (HolographicSettings.IsDisplayOpaque)
                {
                    Platform = PlatformId.ImmersiveHMD;
                }
                else
                {
                    Platform = PlatformId.HoloLens;
                }
            }
#if WINDOWS_UWP
            else
            {
                if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1))
                {
                    Platform = PlatformId.Phone;
                }
            }
#endif
            Debug.LogFormat("MyAppPlatformManager says its Platform is {0}", Platform.ToString());
            if (MyAppPlatformManagerInitialized != null)
            {
                MyAppPlatformManagerInitialized();
            }
        }
    }
}