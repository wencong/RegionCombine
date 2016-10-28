using System;
using System.Collections.Generic;
using UnityEngine;

namespace RegionCombine {
    public class CombineProcessor {
        public static int CombineCountLimit = 1;

        public static void RemapLightmapUv(Mesh mesh, Renderer render) {
            Vector2[] uv2 = mesh.uv2;
            Vector4 lightmapScaleOffset = render.lightmapScaleOffset;
            for (int j = 0; j < uv2.Length; ++j) {
                uv2[j].x = lightmapScaleOffset.z + lightmapScaleOffset.x * uv2[j].x;
                uv2[j].y = lightmapScaleOffset.w + lightmapScaleOffset.y * uv2[j].y;
            }

            mesh.uv2 = uv2;
            //mesh.normals = null;
        }

        public static void CombineMesh(List<GameObject> lstObject, Material mat) {
            if (lstObject.Count <= 1) {
                Debug.LogWarning("lstGameObject.Count <= 1, don't need Combine");
                return;
            }
        }

        public static GameObject CombineMesh(List<ICombineData> lstCombineData, Material mat, string layer) {
            GameObject combineObj = CombineMesh(lstCombineData, mat);
            if (combineObj != null) {
                combineObj.layer = LayerMask.NameToLayer(layer);
            }
            return combineObj;
        }

        public static GameObject CombineMesh(List<ICombineData> lstCombineData, Material mat, bool bDestory = false) {
            if (lstCombineData.Count <= CombineCountLimit) {
                Debug.LogWarning("lstGameObject.Count <= 1, don't need Combine");
                return null;
            }

            List<CombineInstance> combines = new List<CombineInstance>();

            int nTotalVerts = 0;
            //int nLightMapIndex = -1;
            for (int i = 0; i < lstCombineData.Count; ++i) {
                ICombineData combineData = lstCombineData[i];

                Renderer renderer = combineData.GetRenderer();
                Mesh mesh = combineData.GetMesh();

                if (renderer == null || mesh == null) {
                    Debug.LogErrorFormat("[CombineMesh] {0} renderer == null || mesh == null", combineData.GetName());
                    return null;
                }

                if (renderer.lightmapIndex > 0) {
                    Debug.LogErrorFormat("{0}'s lightmap index is {1}", combineData.GetName(), renderer.lightmapIndex);
                    return null;
                }

                //nLightMapIndex = renderer.lightmapIndex;

                nTotalVerts += mesh.vertexCount;
                if (nTotalVerts > 16384) {
                    Debug.LogError("[CombineMesh] total verts larger than 16384");
                    return null;
                }

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = UnityEngine.Object.Instantiate<Mesh>(mesh);

                RemapLightmapUv(combineInstance.mesh, renderer);

                combineInstance.transform = renderer.transform.localToWorldMatrix;
                combines.Add(combineInstance);
            }

            int nId = CombineRegionMgr.Instance.combineTargetRoot.childCount + 1;

            string szCombineName = string.Format("_Combine_{0}", nId);

            GameObject combineGameobject = new GameObject(szCombineName);
            combineGameobject.transform.localPosition = Vector3.zero;
            combineGameobject.transform.localRotation = Quaternion.identity;
            combineGameobject.transform.localScale = Vector3.one;

            Mesh combineMesh = new Mesh();
            combineMesh.name = szCombineName;
            combineMesh.CombineMeshes(combines.ToArray(), true, true);

            combineMesh.Optimize();
            //combineMesh.normals = null;
            //combineMesh.tangents = null;

            MeshFilter filter = combineGameobject.AddComponent<MeshFilter>();
            filter.sharedMesh = combineMesh;

            Renderer render = combineGameobject.AddComponent<MeshRenderer>();

            render.sharedMaterial = mat;
            render.lightmapIndex = 0;
            render.lightmapScaleOffset = new Vector4(1, 1, 0, 0);
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if (!bDestory) {
                GameObject hideRoot = new GameObject(szCombineName);
                hideRoot.transform.parent = CombineRegionMgr.Instance.combineSrcRoot;

                for (int i = 0; i < lstCombineData.Count; ++i) {
                    lstCombineData[i].GetRenderer().transform.parent = hideRoot.transform;
                    //lstCombineData[i].SetActive(false);
                }
                hideRoot.SetActive(false);
            }
            else {
                for (int i = 0; i < lstCombineData.Count; ++i) {
                    UnityEngine.Object.DestroyImmediate(lstCombineData[i].GetRenderer().gameObject);
                }
            }

            CombineLightData combineLight = combineGameobject.AddComponent<CombineLightData>();
            combineLight.lightMapIndex = render.lightmapIndex;
            combineLight.lightMapScaleOffset = render.lightmapScaleOffset;

            combineGameobject.transform.parent = CombineRegionMgr.Instance.combineTargetRoot;

            return combineGameobject;
        }
    }
}
