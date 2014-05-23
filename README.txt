The assginment took a lot of time and effort from my side.
All functionalities are working fine based on my knowladge
All threshold that are used in my code are calculated based on trials and errors.
================================================================================================
I used shortstrow algorithm to detect corners and I modified a little bit, but still is not that accurate if the shape was not drawn accurately
Corners were used to calculate distances and angles and based on that I detected my shapes based on that and I used threshold (taking errors in consideration)
All equations and methods are explained in the code by comments.
The recognition will start each time a stroke has been drawn and if a shape is recognized then the stroke will be converted to a shape
2D Primitive Functionality
1. Square: Detected
2. Rectangle: Detected
3. Triangle: Detected
4. Ellipse: Detected
5. Circle: Detected
6. Arrow: Detected
================================================================================================
2D Primitive Manipulation Functionality
1. Erase primitives using a scribble gesture: I used the built in scratchout gesture and I modified it so shapes can be deleted and not only strokes
In order to use this functionality; editing mode must be changed to scribble gesture by the drop down menu and then it will be enabled then it can be used
The reason I did that becasue each time an inkcanvas in "gesture and ink" mode then it will delete most of the strokes ,so in order make the user more comfortable I did that  
================================================================================================
MutliTouch Mode
One finger to select shapes and then user can manipulate them
Only shapes will be selected and strokes are not and shapes will be converted darkblue color borders
2. Support translation of primitives: One finger to translate
3. Support rotation of primitives: two fingers to rotate
4. Support scaling of primitives: two fingers to scale
5. Support grouping primitives together: one finger to select unselected shapes and it will be added to the group and user can manipulate them all.
In order to deselect the selecetd shapes then two fast "ONE finger" touches (less the one sec) over the selecetd shape and it will deselect it and if user touched two times in an empty area then all group will be deselected.

