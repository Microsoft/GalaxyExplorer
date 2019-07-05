# Galaxy

![Galaxy](Images/ge_app_poi.png)

The galaxy rendering process is outlined in a [case study](https://docs.microsoft.com/en-us/windows/mixed-reality/case-study-creating-a-galaxy-in-mixed-reality). The article describes the art and design approach as well as the technical implementation.

## How the galaxy is rendered

The code lives in *Assets/scripts/DrawStars.cs*. It is based around 3 instances of the `SpiralGalaxy` (attached to the *milky_way_prefab* in the *galaxy_view_scene*) which make up the stars and clouds and are rendered into textures to generate layers with different effects.

The galaxy is the result of the following 3 layers:

- *Stars* rendered last but first in the hierarchy.

- *Cloud shadows* which create the dark spots that can be seen when looking at the Galaxy from the side.

- *Clouds* that make up the fluffy blue nebulas that surround the stars.

The galaxy itself is rendered through Unity's `DrawProcedural` via the `OnPostRender` method. `OnPostRender` is being called by the scripts attached to the camera and is used via a `RenderProxy` script to trigger the rendering process.

## See also

- [Microsoft Docs: Case Study - Creating a galaxy in mixed reality](https://docs.microsoft.com/en-us/windows/mixed-reality/case-study-creating-a-galaxy-in-mixed-reality)