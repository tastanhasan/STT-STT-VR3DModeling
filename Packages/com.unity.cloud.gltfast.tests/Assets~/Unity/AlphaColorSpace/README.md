# Alpha Color Space

## Description

This asset makes it easier to check if alpha values are applied correctly and don't suffer from incorrect color space conversions.

Alpha testing/blending is performed using the product of the following factors:

- Base Color Factor alpha value
- Base Color Texture alpha value
- Vertex color alpha value

On a high level there's two sets of identical test nodes/materials. One for pbrMetallicRoughness and one for unlit materials.

In each of them there's 3 rectangles with materials that show a single, isolated alpha factor with a value of `0.5`.

Furthermore there's 3 materials with all possible combinations of two of those alpha factors:

- Vertex color and Base Color Factor
- Vertex color and Base Color Texture
- Base Color Factor and Base Color Texture

Each factor has a value of `sqrt(1/2)` (`~0.707107`), so that their respective product is again close to `0.5`.

All six rectangles should show identical color and transparency. A potential difference highlights discrepancy in the calculation.

Lastly there's a 16 vertex mesh with the outline shape of a rectangle that if rendered/alpha blended correctly with the vertex color alpha values respected should show a white checkmark. If it only shows a flat white rectangle, vertex color alpha values have not been included.

## Extensions used

- [KHR_materials_unlit](https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_unlit)

## Legal

Model: *Alpha Color Space*

Attribution notice for the files directly associated with the above-referenced model in this directory tree, including all text, image and binary files.

&copy; 2026, Unity Technologies and the glTFast authors. Licensed under Apache 2.0. See [license file at root](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/blob/main/LICENSE.md).
