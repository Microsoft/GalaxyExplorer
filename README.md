# Galaxy Explorer

Galaxy Explorer is an open-source Windows Mixed Reality application that was originally developed in March 2016 as part of the [HoloLens 'Share Your Idea' campaign](https://blogs.windows.com/buildingapps/2015/12/01/announcing-the-microsoft-hololens-share-your-idea-campaign/) where the community submitted and voted on ideas. Since then Galaxy Explorer has been constantly updated with new features, also adding new platforms in the process.

You can read about the [development of Galaxy Explorer on Microsoft Docs](https://docs.microsoft.com/windows/mixed-reality/galaxy-explorer-update).

![Galaxy Explorer Logo](Documentation/Images/ge_app_logo_title.png)

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

This project has also adopted the MRTK approach to [contributing](https://github.com/microsoft/MixedRealityToolkit-Unity/blob/mrtk_release/Documentation/Contributing/CONTRIBUTING.md) and [documenting](https://github.com/microsoft/MixedRealityToolkit-Unity/blob/mrtk_release/Documentation/Contributing/DocumentationGuide.md).

## Supported platforms

The following platforms are supported by this project:

- HoloLens 2
- HoloLens (1st gen)
- Windows Mixed Reality Headsets (VR)
- Windows 10 Desktop

## Software versions

The different versions that the current project is developed on:

- [Unity](https://unity.com/) version 2018.3.8f1+
- [MixedRealityToolkit-Unity](https://github.com/microsoft/MixedRealityToolkit-Unity), version 2.0.0.rc1
- [Visual Studio 2017](https://visualstudio.microsoft.com/), 15.9+
- [Windows SDK 18362+](https://developer.microsoft.com/en-US/windows/downloads/windows-10-sdk)

Download all of the developer tools from the [Microsoft Developer Website](https://developer.microsoft.com/en-us/mixed-reality).

## Application building blocks

| [![Building & running the application](Documentation/Images/ge_app_icon_wide.png)](Documentation/BuildingGalaxyExplorer.md) [Building & running the application](Documentation/BuildingGalaxyExplorer.md) | [![Application flow](Documentation/Images/ge_unity_flow_manager.png)](Documentation/ApplicationFlow.md) [Application flow](Documentation/ApplicationFlow.md) | [![Audio system](Documentation/Images/ge_unity_audio_system.png)](Documentation/AudioSystem.md) [Audio system](Documentation/AudioSystem.md) |
| :--- | :--- | :--- |
| How to run the application in the Unity editor as well as how to build the application for the different target platforms. | Information how the application is structured, when scenes are triggered and so on. | Information on the audio system, how to include and trigger sound effects as well as music. |
|  [![Galaxy](Documentation/Images/ge_app_galaxy.png)](Documentation/Galaxy.md) [Galaxy](Documentation/Galaxy.md)| [![Solar system](Documentation/Images/ge_app_solar_system.png)](Documentation/SolarSystem.md) [Solar system](Documentation/SolarSystem.md) | [![Galactic center](Documentation/Images/ge_vis_galactic_center.png)](Documentation/GalacticCenter.md) [Galactic center](Documentation/GalacticCenter.md) |
| Explains how the galaxy is created and rendered. | How the solar system is created and how the elements work. | Explains the center of the galaxy scene and its elements. |
|  [![Force pull interaction](Documentation/Images/ge_app_force_grab.png)](Documentation/ForceGrabInteraction.md) [Force grab interaction](Documentation/ForceGrabInteraction.md)| [![Points of Interest](Documentation/Images/ge_app_poi.png)](Documentation/PointsOfInterest.md) [Points of Interest](Documentation/PointsOfInterest.md) | [![Menu system](Documentation/Images/ge_capture_hand_menu.png)](Documentation/MenuSystem.md) [Menu system](Documentation/MenuSystem.md) |
| Details on the "force grab" interaction, how it works and how it applies to elements. | How the points of interest work - the labels as well as the "content cards". | More information about the menu system, its elements and functionality. |