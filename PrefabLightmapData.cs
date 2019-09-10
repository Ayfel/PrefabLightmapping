using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class PrefabLightmapData : MonoBehaviour
{
	[System.Serializable]
	struct RendererInfo
	{
		public Renderer 	renderer;
		public int 			lightmapIndex;
		public Vector4 		lightmapOffsetScale;
	}

	[SerializeField]
	RendererInfo[]	m_RendererInfo;
	[SerializeField]
	Texture2D[] 	m_Lightmaps;
    

    void Awake ()
	{
        Init();
	}

    void Init()
    {
        if (m_RendererInfo == null || m_RendererInfo.Length == 0)
            return;

        var lightmaps = LightmapSettings.lightmaps;
        int[] offsetsindexes = new int[m_Lightmaps.Length];
        int counttotal = lightmaps.Length;
        List<LightmapData> combinedLightmaps = new List<LightmapData>();

        for (int i = 0; i < m_Lightmaps.Length; i++)
        {
            bool exists = false;
            for (int j = 0; j < lightmaps.Length; j++)
            {

                if (m_Lightmaps[i] == lightmaps[j].lightmapColor)
                {
                    exists = true;
                    offsetsindexes[i] = j;

                }

            }
            if (!exists)
            {
                offsetsindexes[i] = counttotal;
                var newlightmapdata = new LightmapData();
                newlightmapdata.lightmapColor = m_Lightmaps[i];
                combinedLightmaps.Add(newlightmapdata);

                counttotal += 1;


            }

        }

        var combinedLightmaps2 = new LightmapData[counttotal];

        lightmaps.CopyTo(combinedLightmaps2, 0);
        combinedLightmaps.ToArray().CopyTo(combinedLightmaps2, lightmaps.Length);
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        ApplyRendererInfo(m_RendererInfo, offsetsindexes);
        LightmapSettings.lightmaps = combinedLightmaps2;
    }

    void OnEnable()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Init();
    }

    // called when the game is terminated
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }



    static void ApplyRendererInfo (RendererInfo[] infos, int[] lightmapOffsetIndex)
	{
		for (int i=0;i<infos.Length;i++)
		{
			var info = infos[i];
            
			info.renderer.lightmapIndex = lightmapOffsetIndex[info.lightmapIndex];
			info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;

            // You have to release shaders.
            Material[] mat = info.renderer.sharedMaterials;
            for (int j = 0; j < mat.Length; j++)
            {
                if (mat[j] != null && Shader.Find(mat[j].shader.name)!=null)
                    mat[j].shader = Shader.Find(mat[j].shader.name);
            }

        }


        

    }

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Bake Prefab Lightmaps")]
	static void GenerateLightmapInfo ()
	{
		if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
		{
			Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
			return;
		}
		UnityEditor.Lightmapping.Bake();

		PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

		foreach (var instance in prefabs)
		{
			var gameObject = instance.gameObject;
			var rendererInfos = new List<RendererInfo>();
			var lightmaps = new List<Texture2D>();
			
			GenerateLightmapInfo(gameObject, rendererInfos, lightmaps);
			
			instance.m_RendererInfo = rendererInfos.ToArray();
			instance.m_Lightmaps = lightmaps.ToArray();

			var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
			if (targetPrefab != null)
			{
				//UnityEditor.Prefab
				UnityEditor.PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
			}
		}
	}

	static void GenerateLightmapInfo (GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmaps)
	{
		var renderers = root.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers)
		{
			if (renderer.lightmapIndex != -1)
			{
				RendererInfo info = new RendererInfo();
				info.renderer = renderer;
				info.lightmapOffsetScale = renderer.lightmapScaleOffset;

				Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;

				info.lightmapIndex = lightmaps.IndexOf(lightmap);
				if (info.lightmapIndex == -1)
				{
					info.lightmapIndex = lightmaps.Count;
					lightmaps.Add(lightmap);
				}

				rendererInfos.Add(info);
			}
		}
	}
#endif

}