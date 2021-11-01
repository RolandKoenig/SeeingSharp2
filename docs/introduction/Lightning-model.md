[Home](README.md)

## Lightning model for StandardMaterial
The lightning model in Seeing# is very simple. You don't have to define any light. Seeing# uses a per-pixel lightning system where there are two light sources:
* The ambient light which is defined with one ambient factor in the per-view configuration
* A directional light which is placed on the same position as the camera. It points in the same direction as the camera so you can imaging that the current camera of a view does also act as the "sun" for this view. This light does not have any impact on other views on the same scene - each one has it's own light placed on the camera

## Lightning model for other materials
It is possible to implement other lightning models using new materials. Currently only the above one is implemented.