
# 前提
  * 必须使用LightmapsMode.NonDirectional进行烘培
  * 必须在静态批处理之前重设光照贴图，建议手动调用StaticBatchingUtility.Combine(this.gameObject)
  * 烘培之后，调用Tools/Bake Prefab Lightmaps进行记录烘培的光照贴图相关数据
  * 如果在真机中出错贴图错误，尝试按照下图修改重新打包测试，如果使用AB包，确保AB包是最新的打包数据，建议删除AB包缓存，重新打包

![Graphics Settings](https://github.com/AtheosCode/PrefabLightmapping/blob/master/Snipaste_2019-12-03_17-11-39.png)
