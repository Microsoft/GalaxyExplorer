# Building & running the application

![BuildingGalaxyExplorer](Images/ge_app_icon_banner.png)

## Running in Unity

Note that by default when you download / clone this repository and open it in Unity, it will open a new scene. Navigate to the *Assets/scenes* folder and double-click main_scene to set up the editor properly. After that, you can hit the play button to succesfully start the experience.

To enable the [MRTK diagnostics system](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Diagnostics/DiagnosticsSystemGettingStarted.html), select the MixedRealityToolkit GameObject in the core_systems_scene and check the box "Enable Diagnostics System".

## Building Galaxy Explorer

In the Unity editor, choose *File > Build Settings* to bring up the build settings window. All of the scenes in the *Scenes In Build* section should be checked.

Choose *Universal Windows Platform* as the platform. On the right side, choose *Any device* as the target device, *XAML* as the build type, and *10.0.18361.0* as the target SDK version.

You can now build the project.

After the build completes successfully, an explorer window will pop up. Navigate into the build folder you created and double-click the generated *GalaxyExplorer.sln* file in its root to launch the solution in Visual Studio.

In Visual Studio, set the configuration to *Release* for faster builds (doesn't use .NET Native) or *Master* to build the type of package the Microsoft Store needs (uses .NET Native).

### Building for HoloLens (1st gen)

In *File > Build Settings*:

Architecture should be set to **x86**. The target device should be **HoloLens**.

### Building for HoloLens 2

In *File > Build Settings*:

Architecture should be set to **ARM**.

### Building for Windows Mixed Reality headsets

In *File > Build Settings*:

Architecture should be set to **x64**.

### Building for Windows Desktop

In *File > Build Settings*:

Architecture should be set to **x64** or **x86**. The target device should be **PC**.

It's now possible to deploy to the emulator, a remote device, or create a Microsoft Store package to deploy at a later time.

## Implementing a 3D app launcher icon

When you open the project in Visual Studio (exported from Unity), it will create a default app launcher tile that is used in Mixed Reality. To replace this 2D tile with the actual 3D app launcher, the applications' app manifest needs to include the “MixedRealityModel” element as the default tile definition.

First, locate the app package manifest in the project. By default, the manifest will be named Package.appxmanifest.
```xml
<Package xmlns:mp="http://schemas.microsoft.com/appx/2014/phone/manifest" 
        xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" 
        xmlns:uap2="http://schemas.microsoft.com/appx/manifest/uap/windows10/2" 
        xmlns:uap5="http://schemas.microsoft.com/appx/manifest/uap/windows10/5"
        IgnorableNamespaces="uap uap2 uap5 mp"
        xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
```

Next specify the "MixedRealityModel" as shown below. Note that the 3D galaxy model is called '3DTile.glb' and is located in the app's Assets folder.

```xml
<Applications>
    <Application Id="App"
      Executable="$targetnametoken$.exe"
      EntryPoint="ExampleApp.App">
      <uap:VisualElements
        DisplayName="ExampleApp"
        Square150x150Logo="Assets\Logo.png"
        Square44x44Logo="Assets\SmallLogo.png"
        Description="ExampleApp"
        BackgroundColor="#464646">
        <uap:DefaultTile Wide310x150Logo="Assets\WideLogo.png" >
          <uap5:MixedRealityModel Path="Assets\3DTile.glb" />
        </uap:DefaultTile>
        <uap:SplashScreen Image="Assets\SplashScreen.png" />
      </uap:VisualElements>
    </Application>
</Applications>
```

## See also

- [Unity Manual: Universal Windows Platform](https://docs.unity3d.com/Manual/windowsstore-il2cpp.html)
- For general instructions on implementing 3D launcher icons, also refer to the [Mixed Reality online documentation](https://docs.microsoft.com/en-us/windows/mixed-reality/implementing-3d-app-launchers)