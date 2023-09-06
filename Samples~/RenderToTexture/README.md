# Render-to-Texture Sample

This sample demonstrates how to render a `LightshipMap` into a render texture using a secondary camera.  This is helpful for integrations with the ARDK where the main camera is a custom AR camera that cannot be used to render the map.

The scene's `LightshipMap` is assigned to a new Unity layer called "LightshipMap".  A second camera, called "Map Camera", is used to render the map to a render texture which is then displayed to the user on a `RawImage` canvas element.  The map camera's culling mask is set to exclude everything but the "LightshipMap" layer, and, similarly, the main camera excludes the map by excluding this layer from its culling mask.
