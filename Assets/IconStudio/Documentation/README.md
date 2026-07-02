##### 🎨 **IconStudio**

Ultra-Simple Icon Generator for Unity



Create clean, transparent 2D icons from 3D models in seconds.



No setup headaches.

No manual importing.

No confusing RenderTexture steps.



Just drag → click → done.





Table of Contents



Quick Start



Using the Exported Icon in UI



Export Options



Scene Overview



Tips for Best Results



Troubleshooting



Designed For



Philosophy



Version Notes



1. ##### **Quick Start (Under 60 Seconds)**



1. Open the IconStudio scene.



2\. Drag your 3D model under ItemSpawn.



3\. Adjust position/rotation if needed.



**4. Click:**

Tools → Icon Studio → Export Icon (Auto Save to Project)



Your icon will:



* Save automatically into

&nbsp;  Assets/IconStudio/Exports/

&nbsp;   

* Import automatically as a Sprite (2D and UI)



* Be selected and highlighted in the Project window



* Be ready to drag directly into a UI Image







###### **2. Using the Exported Icon in UI**



The exported PNG is automatically configured as:



* Texture Type: Sprite (2D and UI)



* Alpha Transparency: Enabled



* Compression: Disabled



* Filter Mode: Point (crisp edges)



* Wrap Mode: Clamp



* MipMaps: Disabled



You can immediately drag it into:



* UI → Image component



* SpriteRenderer



* Button Image



* Any 2D UI element



No manual import changes needed.





**3. Export Options**

**Export Icon (Auto Save to Project) ⭐ Recommended**



* Saves into project



* Automatically converts to Sprite



* Selects and highlights the file



* Opens file location



Best for beginners



###### 

###### **Export Icon (Auto)**



* Lets you choose a save location



* Useful for exporting outside Unity



* Does not auto-convert to Sprite (Unity limitation)



###### 

###### **Open Exports Folder**



Quickly opens:

Assets/IconStudio/Exports/



###### **4. Scene Overview**



**ItemSpawn**

Place your 3D model here.



**IconCamera**

Captures the icon.



**PreviewCamera**

Displays a large preview in Game View so you can clearly see the final result before exporting.





##### **5. Tips for Best Results**



Keep the model centered.



Use Orthographic projection for clean item-style icons.



Adjust camera Size to zoom in/out.



Use consistent lighting for uniform icon style.



##### 

##### **6. Troubleshooting**



My icon looks empty



Ensure your model is inside the camera view.



Ensure the model is under ItemSpawn.



Ensure IconCamera has a Target Texture assigned.





###### **I can’t drag the icon into a UI Image**



Make sure you used:



Export Icon (Auto Save to Project)



That version auto-converts the texture to a Sprite.



###### 

###### **7. Designed For**



RPG inventory icons



Item icons



Ability icons



Equipment previews



Crafting UI



Card art thumbnails



Store thumbnails



Skill icons



##### 

##### 

##### 8\. Philosophy



IconStudio is built to be:



Beginner friendly



Minimal setup



No technical configuration



Fast



Clean



Predictable



It removes technical friction so developers can focus on building their game.

##### 

##### 

##### 9\. Version Notes



This version includes:



Automatic Sprite import conversion



Automatic transparency handling



Automatic asset selection + Project focus



Auto-creation of Exports folder



Built-in preview camera



One-click workflow

