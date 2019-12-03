
# 前提
  必须使用LightmapsMode.NonDirectional进行烘培
  必须在静态批处理之前重设光照贴图，建议手动调用StaticBatchingUtility.Combine(this.gameObject)
  烘培之后，调用Tools/Bake Prefab Lightmaps进行记录烘培的光照贴图相关数据
  If you find problems when building make sure to check your graphics settings under Project Settings, as shader stripping might be the cause of the issue. Try playing with the option "Lightmap Modes" and setting it to Custom if it's not working.

![Graphics Settings](https://user-images.githubusercontent.com/13970424/60190570-7dd05680-97f8-11e9-991f-f54b816a577f.png)
