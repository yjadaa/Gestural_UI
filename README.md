Gestural_UI
===========
Gestural UI drawing application is to let users sketch and manipulate simple 2D geometric primitives.<br\>
Shortstrow algorithm was used to detect corners and with few modification, shape should be drawn accurtly.<br\>
Corners were used to calculate distances and angles and based on shapes were detected threshold was used taking erros in considration.<br\>
All equations and methods are explained in the code by comments.<br\>
The recognition will start each time a stroke has been drawn and if a shape is recognized then the stroke will be converted to a shape.<br\>
2D Primitive Functionality<br\>
1. Square: Detected<br\>
2. Rectangle: Detected<br\>
3. Triangle: Detected<br\>
4. Ellipse: Detected<br\>
5. Circle: Detected<br\>
6. Arrow: Detected<br\>
<br\>
2D Primitive Manipulation Functionality<br\>
1. Erase primitives using a scribble gesture: I used the built in scratchout gesture and I modified it so shapes can be deleted and not only strokes.<br\>
In order to use this functionality; editing mode must be changed to scribble gesture by the drop down menu and then it will be enabled.<br\>
<br\>
MutliTouch Mode<br\>
Use One finger to select shapes then you can manipulate them<br\>
Only detecetd shapes will be selected and selcted shapes borders will be colored as darkblue.<br\>
2. Support translation of primitives: One finger to translate<br\>
3. Support rotation of primitives: two fingers to rotate<br\>
4. Support scaling of primitives: two fingers to scale<br\>
5. Support grouping primitives together: one finger to select unselected shapes and it will be added to the group and user can manipulate them all.<br\>
In order to deselect the selecetd shapes then two fast "ONE finger" touches (less the one sec) over the selecetd shape and it will deselect it and if user touched two times in an empty area then all shapes inside the group will be deselected.<br\>
