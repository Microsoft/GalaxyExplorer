#Galaxy Explorer

Galaxy Explorer is an open-source HoloLens application that was developed
in 6-weeks as part of the Share Your Idea program where the community
submitted and voted on ideas.

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any
additional questions or comments.

The following sections serve as guide posts to help navigate the code by
explaining some of the larger systems, both how they work and how they
interact.

#HoloLens Tools

Download all of the developer tools from the
[Microsoft Developer Website](http://lens.ms/Sa37sr)

*note*, the currently supported version of Unity is 5.5.0b4 which can be
downloaded from the [Unity Beta Program site](https://unity3d.com/unity/beta#download)

#Running in Unity

This project is built with the Unity HoloLens Technical Preview. A link to download the latest version can be found with the rest of the HoloLens tools.

Note that by default when you point Unity at this repo, it will open a new scene. Navigate to /Scenes and double-click MainScene to setup the editor properly. After that, hitting Play will start the experience.

#Building Galaxy Explorer

From Unity, choose File->Build Settings to bring up the Build Settings
window. All of the scenes in the Scenes to Build section should be checked.
Choose Windows Store as the Platform. On the right side, choose Universal 10
as the SDK, "any device" as the Target device, XAML as the UWP Build Type,
check "Copy References", check "Unity C# Projects" and then click Build.
Select the folder called 'UWP' and choose this folder.

After the build completes successfully, an explorer window will pop up.
Navigate into the UWP folder and double-click GalaxyExplorer.sln to launch
Visual Studio. From Visual Studio, set the Configuration to **Release**
for faster builds (doesn't use .NET Native) or **Master** to build the
type of package the Store needs (uses .NET Native).

##Building for HoloLens

Make sure to change ARM or x64 to **x86**.
Now you can deploy to the Emulator, a Remote Device, or create a Store
package to deploy at a later time.

##Building for Windows Phone

Make sure to change x64 or x86 to **ARM**.
Now you can deploy to the Emulator, a Remote Device, or create a Store
package to deploy at a later time.

##Building for Windows Desktop

Make sure to change ARM to **x64** or **x86**.
Now you can deploy to the Emulator, a Remote Device, or create a Store
package to deploy at a later time.

#CoreSystems

CoreSystems is a scene that we load that has most of our global game objects. Things like our audio rig (which has our background music and VOManager) and things like our Input stack. CoreSystems is loaded into any scene via ViewLoader so that developers and artists can run any scene (e.g. SunView.unity) independent from running the MainScene. 

There is a callback mechanism in ViewLoader that lets scenes know when the hierarchy is ready.

#ViewLoader

The ViewLoader manages the loading of scenes used throughout the app. When the ViewLoader is first created (MainScene or any other "view" scene), the CoreSystems scene is loaded. This initializes the app to contain all of the singleton objects necessary to run. If a "view" scene is run through the UnityEditor, loading CoreSystems in this way makes it easy to iterate on content in the editor without having to load the MainScene and walk through to the point in the application that you care about. Other systems can subscribe to the CoreSystemsLoaded action in the ViewLoader to know when the CoreSystems have loaded.

The IntroductionFlow loads the starting view, which uses the Transitionmanager to preload the solar system followed by loading the earth. All other content is loaded through the TransitionManager through "next" and "prev" scene loads. When a next scene is loaded, the name of the scene is provided by a PointOfInterest. The scene is added to a stack that the ViewLoader maintains, and the stack is popped to determine which scene to load for prev scenes.

For all scene loaded, an optional callback for the loaded scene can be provided. Scenes are loaded asynchronously, so the TransitionManager uses this callback to know when new content is ready to be shown and moved for a transitions.

#IntroductionFlow

IntroductionFlow contains the on-rails experience in the demo that happens when you first start Galaxy Explorer. The editor defines timings and voice over to be played in different states of the introduction:
* AppDescription
* Developers
* Community
* SlateFadeout
	
The above states are the first slates you see when the application starts, explaining what Galaxy Explorer is all about and how it came to be. You can click or air tap in any of these states to move to the Logo state.

* Logo
* LogoFadeout

The above states show the Galaxy Explorer logo. Clicking or air tapping before the timer completes for the logo will skip to the PreloadSolarSystem state.

* PreloadSolarSystem

This state loads the solar system and the first earth that you see. The preloaded solar system is a performance optimization to prevent a hitch when transitioning from earth to the solar system for the first time.

* EarthHydrate
* PlaceEarth

At this point in the introduction, the earth becomes head locked to be placed with gaze. Click or air tap to place the earth and start the experience.

* EarthFadeout
* Earth
* SolarSystem
* Galaxy

The above states cannot be skipped and contain voice over the introduce the flow of the experience.

* Complete

When this state is entered, the IntroductionFlow object is destroyed. Other scripts will identify if the application is in the introduction (i.e.: the TransitionManager does not fade in the PointsOfInterest during the introduction because interacting with them has not yet been explained).
		
The IntroductionFlow loads the first scene in the experience by telling the TransitionManager to switch between different scenes.

#Galaxy

The galaxy rendering process was mostly described in Tech Process - Creating a Galaxy (https://microsoftstudios.com/hololens/2016/03/10/tech-process-creating-a-galaxy/)

tThe code itself lives in Assets\Galaxy and is comprised of a set of SpiralGalaxy which make up the stars and clouds (see MilkyWay.prefab).
The galaxy is the result of 3 layers:
* Stars, rendered last but first in the hierarchy
* Clouds shadows, that make up the dark spots that can be seen when looking at the Galaxy from the side
* Clouds, that make up the fluffy blue clouds that surround the stars

The galaxy itself being rendered through Unity's DrawProcedural via the OnPostRender method. OnPostRender being called only on scripts attached to Cameras, we use a RenderProxy script to trigger the Galaxy rendering.

#Tools

The Tools are contained in a ToolPanel, which manages the visibility of the Back, Grab, Zoom, Tilt, Reset, About, and Controls UI elements. It has accessor functions to fade in/out all of the UI elements together, and the update logic implements the tag-along functionality.

In the ToolPanel, there are Buttons (Back, Grab, Reset, About, and Controls) and Tools (Zoom and Tilt). The buttons perform an action on selection, and tools enter a toggle state that change the functionality of the cursor. (Even though Grab was implemented as a Button, it could also be a Tool that toggles itself off on placement of the content.)

The ToolManager handles the Button and Tool settings that can be called from anywhere in script. It also has global settings that other content is dependent on. For example, the min and max zoom sizes are calculated when new content is loaded and stored inside the ToolManager. Tools are locked and unlocked to enable/disable their functionality, and the ToolPanel can be raised or lowered through the manager. Most of the functions provided here are utility functions expected to be called anywhere a script wants to control tool functionality.

#PointOfInterest

PointOfInterests (POIs) are used to interact with a specific part of a hologram
(galaxy, solar system, a planet) as opposed to the entire hologram, which is
controlled by Tools. They are represented in the application by a line from the
content to interact with to a marker.

Parts of a PointOfInterest:
* BillboardLine - the line that connects the interest point to interact with to an indicator at the top of the line. The line is always vertical and scales with distance as a UI element. It does not rescale with content and will always start at a target point.
* Indicator - the card that is shown above the BillboardLine.
* Description - a text card that fades in when GazeSelection collides with any collider in the POI hierarchy.
* Transition Scene (optional) - the scene that is loaded through the TransitionManager when the POI is selected.

OrbitScalePointOfInterest is a toggle that converts between Realistic and Simplified
orbit and size views in the solar system.

CardPointOfInterest is used in the galaxy to inspect images through a magic window.
Parallax for the window and settings are incorporated in the POI_Porthole shader, and
the window depth is hidden with the POI_Occlusion shader. The magic window points of
interest have a moving target, but the cards are outside of the POI hierarchy.

POI controls are used outside of the POI hierarchy using a PointOfInterestReference.
Any colliders that respond to GazeSelection in the reference's hierarchy are treated
as if they are under the POI hierarchy. This is used by magic windows in the galaxy
and could be extended to add other selection sources, like using orbits in the solar
system to travel to planet views.

The font used in the PointOfInterest cards is called "Orbitron" and it can be found
[here](https://www.theleagueofmoveabletype.com/orbitron). As this font is released
under the [SIL Open Font License](http://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL_web),
developers who are interested in creating or modifying PointOfInterest cards should
treat it like any other third-party dependency and install the font on their own
development system rather than committing it directly into the git repository.

#GE_POIMaker Tool

The GE_POIMaker tool can be used to help create new point of interest billboard image files. 
This tool is located as a seperate project within the solution. It can be used to create one-off 
POI billboard .png files or to batch create all POI files when necessary. The current POI files 
were generated with this tool's default settings, and can be used to create new content quickly and easily.

The GE_POIMaker can also be used to experiment with colors, font sizes and other effects by 
clicking the "Show advanced controls" checkbox. Please consult the tooltips on each control for help on that specific control.

The GE_POIMaker tool references the Orbitron font, but will use a default font if Orbitron
is not installed on the development system.

#TransitionManager

Each view (galaxy, solar system, each planet, and the sun) is a scene in Unity. The ViewLoader handles loading these scenes and the TransitionManager manages how flow moves from an old scene to a new scene through callbacks from the ViewLoader. This system handles the animations that are run between scenes to easily flow between scenes.

Forward transitions are marked by using a PointOfInterest to load a scene. The viewer looks at a destination marker or target (i.e.: a planet in the solar system view) and clicks or air taps to start the transition to the new scene. These transitions add a new scene to a stack in the ViewLoader. Back transitions pop the ViewLoader scene stack to determine the scene to go back to. This is triggered through the UI back button or voice command.

Forward and backward transition flow:
* The next scene is loaded asynchronously, objects slow to a stop, collisions are disabled, and POIs fade out.
* After all of the above is complete, the transition starts by placing the newly loaded scene in the existing scene. The scenes are parented, translated, rotated, and scaled into position while the old scene fades out (handles deletion of the scene when it fades out completely).

For forward transitions, the new content fits in the POI target.
For backward transitions, the new content is scaled up to match up the POI target with the old scene. For these transitions the POI target is completely visible while all other content in the new scene is faded in during the transition.

* After the transition is complete, POIs are faded in, collisions are enabled, and objects start to move.

Performance choices:
* Removing the POIs during a transition is both a design and performance bonus. When the POIs are hidden, they are disabled, saving rendering costs.
* The orbit updater has some expensive computation that may not converge quickly, so the planets in the solar system stop updating during transitions.
* The first transition from the earth to the solar system is taxing, especially on load. We preload the solar system during a blank screen and delete it at the end of the introduction to expedite the first time the solar system is loaded (part of transition logic).

The TransitionManager publicly exposes the FadeContent coroutine, so any script logic can fade in/out an object and all of its children overtime, given an animation curve.

#Fader

Faders control the transition alpha for any materials that use shaders supporting _TransitionAlpha. Each Fader is responsible for changing the blend settings of material shaders for alpha blending and returning them to their original states when fading completes.

Use the TransitionManager.Instance.FadeContent() coroutine to fade in/out content over time. All faders on the object passed to the function will fade in/out. You can disable specific faders of the parent object passed to the function by calling EnableFade() on those faders before the coroutine is started; the function assumes that faders that have already been enabled are handled by other logic.

Parts of the Fader:
* Faders chained in a hierarchy will, by default, only contain materials used by renderers that do not have a closer Fader parent in its hierarchical tree.
* Call EnableFade() to allow a shader to receive and use _TransitionAlpha properly and DisableFade() to restore its shader settings to their original settings for performance outside of transitions.
* When enabled, a Fader can have its alpha changed through SetAlpha.

PointOfInterest.POIFader - CardPointOfInterest (see PointOfInterest) is a fader of this type. For shared meshes of this fader, the _TransitionAlpha is used to individually set transparency without breaking batch rendering for performance.

BillboardLine.LineFader - Forwards _TransitionAlpha to the BillboardLine (see PointOfInterest) to set POI line opacity without breaking batch rendering for performant rendering.

ToolPanel.ToolsFader - Forwards _TransitionAlpha to the ToolPanel, Buttons, and Tools (see Tools) to handle opacity changes.

MaterialsFader has all of its materials defined in the UnityEditor instead of trying to figure out which materials to fade through renderers. You can use this for batch rendering or to fade a group of objects together without needing to collect a list of faders for better performance.

SharedMaterialFader identifies the first material that the fader contains and forces all of the renderers to share the first material found. This is used to fade several objects in a hierarchy at the same time and ensures that all of the children in the fader are using the same shader for better performance.

SunLensFlareSetter specifies a single material in the UnityEditor to integrate _TransitionAlpha settings with other shader-dependent values for lens flare.

#GazeSelection

GazeSelection collects all targets that are selected through gaze by using a ray from the HoloLens position along the device's forward vector. The cursor indicates the position and direction of the ray in the demo, and the Cursor script defines how targets are found with physics.

The logic supports raycast and spherical cone collisions and processes the physics layers to test against in-order. This allows us to prioritize tool selection first and fallback to spherical cone collisions for gaze selection assistance when near interactive elements. If any collisions are found at any step in physics, all targets are cached for that frame. If the spherical cone collision finds multiple targets, those targets are prioritized and ordered from closest to the gaze ray to farthest from the gaze ray.

The GazeSelectionManager filters the gaze selection targets down to a single target and manages when the target changes for the entirety of the app. When the target would change, there is a small delay introduced before the target is actually deselected to account for an unsteady gaze. There is additional logic to prevent gaze selection from quickly switching between objects by keeping an object selected if switching to an object one frame would reselect the old object in the next. This allows targets of gaze selection to turn on/off colliders and not flicker if the colliders refer to the same target selection object.

Only a GazeSelectionTarget can be selected. This gives the app occluder support, allowing other colliders to block interactions. If there is a new component that should support gaze selection, it must inherit from the GazeSelectionTarget component and implement the IGazeSelectionTarget interface. GazeSelectionTarget and IGazeSelecdtionTarget have function calls to respond to gaze selection changes (selection and deselection), hand and clicker input, and void commands.

The following component types are GazeSelectionTargets:
* Hyperlink - used in the About Galaxy Explorer slate to explore more about Galaxy Explorer outside of the application.
* Button - the Back, Grab, Reset, About, and Controls buttons in the ToolPanel.
* Tool - the Zoom and Tilt tools in the ToolPanel.
* PointOfInterest and PointOfInterestReference - define the interactions with specific objects in the galaxy and solar system.
PlacementControl - an invisible barrier that is enabled when the Grab tool is selected. Selecting this disabled Grab and places the content in its current location.

#VOManager

VOManager is used to control how voice over clips are played and stopped. The voice over content is broken up based on where we are in the flow of the experience and requires a central control to ensure that it flows as desired. 

Playing a voice over clip will enter that clip into a queue to be played in a first in, first out order. Individual clips can have their own delay from: when they're queued up to play, to when they actually play. By default, clips will only be played once, even if Play is called with that specific clip again. However, each clip has the option to play more than once which is exposed publicly.

Stopping the playing audio will fade out what's currently playing over a user tweakable fadeout time. Stopping also has the option to clear the queue of all clips if the user wants to start a new sequence of voice audio.

Lastly, voice over audio can be disabled and enabled globally by setting the VO state to false, fading out any audio currently playing and clearing the queue of clips waiting to be played.

VOManager works best when it exists in a persistent system as it inherits the Singleton behavior pattern. Its only requirement is that an AudioSource is placed on the same object.

#WorldAnchorHelper

WorldAnchors are Unity components that create Windows Volumetric Spatial Anchors at a defined real world transform in space. The WorldAnchorHelper class wraps up all of the calls to create and maintain the WorldAnchor that defines the position where the galaxy is placed as part of the introduction process.

One important function of WorldAnchorHelper is to listen for changes in the locatability of the created WorldAnchor. A WorldAnchor will only be located when the device is able to find the playspace where the WorldAnchor was created. While the WorldAnchor is not located, the main content will be hidden. If the content is hidden for more than five seconds then the content is placed in the 'grabbed' state and shown so it can be placed again.

#Shaders

Galaxy

The galaxy is using a geometry shader to expend a particle system into screen aligned quads.

Magic Window - POI_Porthole

Because the Galaxy renders in several passes, we didn't want to have other passes for the background and have to manually clip them. Instead, we have a texture for the background and we tweak the UV depending on the direction to the camera to create a parallax effect. Essentially, we do an intersection test between the ray to the camera to the plane where we want the virtual image to be at, and shift the UV coordinates based on that.

Solar System Orbits - OrbitalTrail

The orbits lines are screen space lines expanded with a geometry shader.
Each vertex have 2 positions: one for the real scale view and one for the schematic view.
The vertex shader then interpolates between those 2 positions to compute the final position according the a reality scale that moves between 0 and 1 and then pass it to a geometry shader that generates correctly triangulated lines in screen space.
This makes the orbits have a fixed width on screen no matter what scale the solar system is being viewed at.

Earth - PlanetShaderEarth

Like all the planets, most parameters are evaluated in the vertex shader as we have a high poly version of each planet. The light is computed with a N.L contribution that we gamma correct in order to have a realistic looking light transition from the dark side to the light side.
We also have in the alpha channel of the Albedo texture a map of the night lights from NASA photographs that we use to illuminate the dark side of the planet.
You might notice that there are lights in the middle of Australia … which are actually wildfires that can be seen from space.

Saturn - PlanetShaderSaturn

In the experience we don't have dynamic shadows enabled - as they are mostly irrelevant for our scene - except for Saturn. The rings shadow pattern always plays a big part of the aesthetic look of the planet, so we spent some time making analytic shadows for it.
The logic behind is to project a sphere on a plane perpendicular to the direction to the light (the sun is approximated as a directional light) and checking if the resulting pixel is inside of the shadow or not.
For the shadow of the planet on the rings, the world space position of the pixel on the ring is compared to the radius of the planet when projected on the plane that contains the pixel.
For the shadow of the rings of the planet, we project the world space position of the pixel on the planet into the rings plane, and we compare its distance to the center of the planet to the distance to the inner ring radius and outer ring radius. The result gives a value in [0-1] which is used to sample a shadow texture.

Performance Investigation
During the development process, we used various tools to investigate possible performance optimization in our rendering tasks.
* Unity Profiler - Integrated with Unity, it gives a good overview where time is spent on the CPU and how many elements are being drawn on the screen.
* Unity's shader "compile and show code" - It shows the shader assembly and gives an idea on how expensive the shaders will be once being executed on device. A rule of thumb is that lower instructions count especially in the pixel/fragment shader is better.
* Visual Studio Graphics Debugger - Very powerful tool that gives timing on both the CPU and GPU side, can analyze shader performance and reveal hot code path on the CPU
GPU View (Integrated with Visual Studio Graphics Debugger) - Gives precise timing on the GPU and CPU workload on device. Best used to determine if the experience is GPU bound or CPU bound.
