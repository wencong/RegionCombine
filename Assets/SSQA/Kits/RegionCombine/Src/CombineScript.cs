using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RegionCombine;

public class CombineScript : MonoBehaviour {
    private Rect m_regionRect;
    private int m_regionCell = 32;

    void Awake() {
        JXSJDataDynamicLoadMgr dynamicLoad = GameObject.FindObjectOfType<JXSJDataDynamicLoadMgr>();
        if (dynamicLoad != null) {
            m_regionRect = dynamicLoad.mapRect;
        }
    }

    void OnLevelWasLoaded() {
    }

    void Start() {
        Combine();
    }

    public void Combine() {
        CombineRegionMgr.Instance.Init(m_regionRect, m_regionCell, transform);

        CombineRegion[] regions = CombineRegionMgr.Instance.regions;

        for (int i = 0; i < regions.Length; ++i) {
            List<Material> lstMats = regions[i].lstMaterials;

            for (int j = 0; j < lstMats.Count; ++j) {
                string szMsg = string.Format("Combine Region:{0}, Material:{1}", i, lstMats[j].name);
                Log.Info(szMsg);
                //EditorUtility.DisplayCancelableProgressBar("", szMsg, (float)j / lstMats.Count);
                GameObject combineObj = regions[i].CombineByMat(lstMats[j]);
                if (combineObj == null) {
                    //SaveCombineObjAsPrefab(combineObj, false);
                }
            }
        }
    }
}


