Thank you for downloading Curved VR Keyboard.

Full setup:
- Drop keyboard prefab to your scene. 
- Set „Raycasting source" field by dragging camera on it, or any other object that will be used to control raycasting direction.
- Create canvas with UI text, InputField or "Text Mesh Pro" for output.
- Set UI text object in „Gameobject Output" field in „Keyboard Status" script.

Optional setup:
- Changing space image
	- Set desired texture type to "Sprite(2D and UI).
	- Add some borders in "Sprite Editor".
	- Set sprite in "9sliced sprite" field in "Keyboard Creator".
	- Use "slice proportions" to get desired look (try ranges from 0.01 to 20).
	- If any visual glitches appear on space button try setting keyboard materials rendering mode to "Fade" and press "Refresh space material" button

After Update:
	- If any error occurs set raycasting source once more and setup keyboard variables' again 

Changing key colors:
You can change color or transparency by editing materials in "Handcrafted Mobile/CurvedVRKeyboard/Resources/Materials"
just as you would do with any other materials.
 
Changing keys values:
To change values used on keyboard open
"Handcrafted Mobile/CurvedVRKeyboard/Resources/Scripts/KeyboardComponent"
script and edit values you would like to change.

Changing Key Fonts:
In Hierearchy window type "Value",
select all found components and check text parameter.

Known Issues:
	- 2 keyboards on same scene aren't supported


Need more help? Found some bugs?
https://handcraftedmobile.com/ or contact@handcraftedmobile.com