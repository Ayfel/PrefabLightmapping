using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[ExecuteInEditMode]
[DisallowMultipleComponent]
public class PrefabLightmapData : MonoBehaviour
{

    [System.Serializable]
    struct RendererInfo
    {
        public Renderer renderer;
        public int lightmapIndex;
        public Vector4 lightmapOffsetScale;
    }
    [System.Serializable]
    struct LightInfo
    {
        public Light light;
        public int lightmapBaketype;
        public int mixedLightingMode;
    }

    [SerializeField]
    RendererInfo[] m_RendererInfo;
    [SerializeField]
    Texture2D[] m_LightmapsColor;
    [SerializeField]
    Texture2D[] m_LightmapsDir;
    [SerializeField]
    Texture2D[] m_ShadowMasks;
    [SerializeField]
    LightInfo[] m_LightInfo;

    void Awake()
    {
        if (m_RendererInfo == null || m_RendererInfo.Length == 0)
            return;

        var lightmaps = LightmapSettings.lightmaps;
        var combinedLightmaps = new LightmapData[lightmaps.Length + m_LightmapsColor.Length];
        lightmaps.CopyTo(combinedLightmaps, 0);
        for (int i = 0; i < m_LightmapsColor.Length; i++)
        {
            combinedLightmaps[i + lightmaps.Length] = new LightmapData
            {
                lightmapColor = m_LightmapsColor[i],
                lightmapDir = m_LightmapsDir[i],
                shadowMask = m_ShadowMasks[i],
            };

        }

        ApplyRendererInfo(m_RendererInfo, lightmaps.Length, m_LightInfo);
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        LightmapSettings.lightmaps = combinedLightmaps;
    }
    private void Start()
    {
        StaticBatchingUtility.Combine(this.gameObject);
    }


    static void ApplyRendererInfo(RendererInfo[] infos, int lightmapOffsetIndex, LightInfo[] lightsInfo)
    {
        for (int i = 0; i < infos.Length; i++)
        {
            var info = infos[i];
            if (info.renderer != null)
            {
                info.renderer.lightmapIndex = info.lightmapIndex + lightmapOffsetIndex;
                info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;
            }
        }
        for (int i = 0; i < lightsInfo.Length; i++)
        {
            LightBakingOutput bakingOutput = new LightBakingOutput
            {
                isBaked = true,
                lightmapBakeType = (LightmapBakeType)lightsInfo[i].lightmapBaketype,
                mixedLightingMode = (MixedLightingMode)lightsInfo[i].mixedLightingMode
            };
            lightsInfo[i].light.bakingOutput = bakingOutput;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Bake Prefab Lightmaps")]
    static void GenerateLightmapInfo()
    {
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }
        //UnityEditor.Lightmapping.Bake();

        PrefabLightmapData[] prefabs = GameObject.FindObjectsOfType<PrefabLightmapData>();

        foreach (var instance in prefabs)
        {
            var gameObject = instance.gameObject;
            var rendererInfos = new List<RendererInfo>();
            var lightmapsColor = new List<Texture2D>();
            var lightmapsDir = new List<Texture2D>();
            var shadowMasks = new List<Texture2D>();
            var lightInfos = new List<LightInfo>();

            GenerateLightmapInfo(gameObject, rendererInfos, lightmapsColor, lightmapsDir,shadowMasks,lightInfos);

            instance.m_RendererInfo = rendererInfos.ToArray();
            instance.m_LightmapsColor = lightmapsColor.ToArray();
            instance.m_LightmapsDir = lightmapsDir.ToArray();
            instance.m_LightInfo = lightInfos.ToArray();
            instance.m_ShadowMasks = shadowMasks.ToArray();


            var targetPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance.gameObject) as GameObject;
            if (targetPrefab != null)
            {
                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(instance.gameObject);// 根结点
                //如果当前预制体是是某个嵌套预制体的一部分（IsPartOfPrefabInstance）
                if (root != null)
                {
                    GameObject rootPrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance.gameObject);
                    string rootPath = AssetDatabase.GetAssetPath(rootPrefab);
                    //打开根部预制体
                    PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(root, PrefabUnpackMode.OutermostRoot);
                    try
                    {
                        //Apply各个子预制体的改变
                        PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                    }
                    catch { }
                    finally
                    {
                        //重新更新根预制体
                        PrefabUtility.SaveAsPrefabAssetAndConnect(root, rootPath, InteractionMode.AutomatedAction);
                    }
                }
                else
                {
                    PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                }
            }
        }
    }

    static void GenerateLightmapInfo(GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmapsColor, List<Texture2D> lightmapsDir,List<Texture2D> shadowMasks, List<LightInfo> lightsInfo)
    {
        var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.lightmapIndex != -1)
            {
                RendererInfo info = new RendererInfo();
                info.renderer = renderer;
                if (renderer.lightmapScaleOffset != Vector4.zero)
                {
                    info.lightmapOffsetScale = renderer.lightmapScaleOffset;
                    Texture2D lightmapColor = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                    Texture2D lightmapDir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    Texture2D shadowMask = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;

                    info.lightmapIndex = lightmapsColor.IndexOf(lightmapColor);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = lightmapsColor.Count;
                        lightmapsColor.Add(lightmapColor);
                        lightmapsDir.Add(lightmapDir);
                        shadowMasks.Add(shadowMask);
                    }
                    rendererInfos.Add(info);
                }

            }
        }
        var lights = root.GetComponentsInChildren<Light>(true);
        foreach (Light l in lights)
        {
            LightInfo lightInfo = new LightInfo();
            lightInfo.light = l;
            lightInfo.lightmapBaketype = (int)l.lightmapBakeType;
            lightInfo.mixedLightingMode = (int)UnityEditor.LightmapEditorSettings.mixedBakeMode;
            lightsInfo.Add(lightInfo);
        }
    }
#endif
}
