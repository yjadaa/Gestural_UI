Gestural_UI
===========
Gestural UI drawing application is to let users sketch and manipulate simple 2D geometric primitives.
Shortstrow algorithm was used to detect corners and with few modification, shape should be drawn accurtly.
Corners were used to calculate distances and angles and based on shapes were detected threshold was used taking erros in considration.
All equations and methods are explained in the code by comments.
The recognition will start each time a stroke has been drawn and if a shape is recognized then the stroke will be converted to a shape.
2D Primitive Functionality
1. Square: Detected
2. Rectangle: Detected
3. Triangle: Detected
4. Ellipse: Detected
5. Circle: Detected
6. Arrow: Detected
================================================================================================
2D Primitive Manipulation Functionality
1. Erase primitives using a scribble gesture: I used the built in scratchout gesture and I modified it so shapes can be deleted and not only strokes.
In order to use this functionality; editing mode must be changed to scribble gesture by the drop down menu and then it will be enabled.
================================================================================================
MutliTouch Mode
Use One finger to select shapes then you can manipulate them
Only detecetd shapes will be selected and selcted shapes borders will be colored as darkblue.
2. Support translation of primitives: One finger to translate
3. Support rotation of primitives: two fingers to rotate
4. Support scaling of primitives: two fingers to scale
5. Support grouping primitives together: one finger to select unselected shapes and it will be added to the group and user can manipulate them all.
In order to deselect the selecetd shapes then two fast "ONE finger" touches (less the one sec) over the selecetd shape and it will deselect it and if user touched two times in an empty area then all shapes inside the group will be deselected.
