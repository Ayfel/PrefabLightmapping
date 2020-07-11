# PrefabLightmapping
Script for saving lightmapping data to prefabs. Used through the Assets tab in Unity. Place your prefabs in the scene with this script at the root. Set up your lighting and in the editor go to Assets->Bake Prefab Lightmaps. After the bake is processed you can now spawn your prefabs in different scenes and they will use the lightmapping from the original scene. 

Remember that if you are not instantiating your prefabs at runtime you should remove the static flag from the GameObjects, otherwise static batching will mess with uvs and the lightmap won't work properly.

If you find problems when building make sure to check your graphics settings under Project Settings, as shader stripping might be the cause of the issue. Try playing with the option "Lightmap Modes" and setting it to Custom if it's not working.

![Graphics Settings](https://user-images.githubusercontent.com/13970424/60190570-7dd05680-97f8-11e9-991f-f54b816a577f.png)

Original idea came from Joachim_Ante in the Unity forums

Feel free to use this project in all commercial and personal projects ;)
