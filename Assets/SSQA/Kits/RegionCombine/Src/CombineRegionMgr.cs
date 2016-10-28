using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RegionCombine {
    public class CombineRegionMgr {
        #region Contruction
        private static CombineRegionMgr _inst = null;
        private CombineRegionMgr() { }
        public static CombineRegionMgr Instance {
            get {
                if (_inst == null) {
                    _inst = new CombineRegionMgr();
                }
                return _inst;
            }
        }
        #endregion

        Quadtree m_quadtree = null;
        private List<CombineRegion> m_lstRegions = new List<CombineRegion>();
        private List<QuadtreeLeaf> m_leaves = null;

        //private List<ICombineData> m_lstIgnore = new List<ICombineData>();

        private Rect m_rect;
        private float m_fCellSize;
        private Transform m_srcRoot;
        private Transform m_targetRoot;
        public void Init(Rect rt, float cell, Transform root) {
            Clear();

            m_rect = rt;
            m_fCellSize = cell;

            m_srcRoot = root;

            _CreateTargetRoot();
            _InitQuadTree();
            _InitRegion();
        }

        public void _CreateTargetRoot() {
            GameObject _combine = GameObject.Find("_CombineModels");
            if (_combine == null) {
                _combine = new GameObject("_CombineModels");
            }
            m_targetRoot = _combine.transform.Find(m_srcRoot.name);

            if (m_targetRoot == null) {
                m_targetRoot = new GameObject(m_srcRoot.name).transform;
                m_targetRoot.parent = _combine.transform;
            }
        }

        public void Clear() {
            m_quadtree = null;
            m_lstRegions.Clear();
        }

        public void RefreshLeaves() {
            if (m_quadtree == null) {
                return;
            }

            for (int i = 0; i < m_lstRegions.Count; ++i) {
                m_leaves[i].bSelect = m_lstRegions[i].bSelect;
            }
        }

        public CombineRegion[] regions {
            get {
                return m_lstRegions.ToArray();
            }
        }

        public Transform combineSrcRoot {
            get {
                return m_srcRoot;
            }
        }

        public Transform combineTargetRoot {
            get {
                return m_targetRoot;
            }
        }

        public string[] layers {
            get {
                List<string> lstLayers = new List<string>();
                for (int i = 0; i < m_lstRegions.Count; ++i) {
                    List<string> ls = m_lstRegions[i].lstLayers;
                    for (int j = 0; j < ls.Count; ++j) {
                        if (!lstLayers.Contains(ls[j])) {
                            lstLayers.Add(ls[j]);
                        }
                    }
                }
                return lstLayers.ToArray();
            }
        }

        private void _InitQuadTree() {
            m_quadtree = new Quadtree(m_rect, m_fCellSize);
            Renderer[] renders = m_srcRoot.GetComponentsInChildren<MeshRenderer>(true);
            Debug.LogFormat("Total Render: {0}", renders.Length);
            Quadtree.nReceivedCount = 0;
            for (int i = 0; i < renders.Length; ++i) {
                m_quadtree.Receive(renders[i]);
            }
            Debug.LogFormat("Receive Render: {0}", Quadtree.nReceivedCount);
        }

        private void _InitRegion() {
            if (m_quadtree == null) {
                return;
            }
            m_lstRegions.Clear();
            //m_lstIgnore.Clear();

            m_leaves = m_quadtree.GetAllLeaves();

            for (int i = 0; i < m_leaves.Count; ++i) {
                QuadtreeLeaf leaf = m_leaves[i];
                
                CombineRegion combineRegion = new CombineRegion();
                combineRegion.Init();

                for (int j = 0, count = leaf.objects.Count; j < count; ++j) {
                    CombineMeshData data = new CombineMeshData(leaf.objects[j]);
                    combineRegion.AddCombineData(data);
                }
                m_lstRegions.Add(combineRegion);
            }
        }

        public void DrawDebugLine(float fDebugLine) {
            if (m_quadtree != null) {
                m_quadtree.DrawDebugLine(fDebugLine);
            }
        }

        public List<Material> GetMaterialsInRegions(List<CombineRegion> lstRegion) {
            List<Material> lstRetMaterials = new List<Material>();

            for (int i = 0; i < lstRegion.Count; ++i) {
                _UnionList<Material>(ref lstRetMaterials, lstRegion[i].lstMaterials);
            }
            return lstRetMaterials;
        }

        public List<ICombineData> GetCombineObjsInRegions(List<CombineRegion> lstRegion, Material mat) {
            List<ICombineData> lstRetCombineData = new List<ICombineData>();

            for (int i = 0; i < lstRegion.Count; ++i) {
                _UnionList<ICombineData>(ref lstRetCombineData, lstRegion[i].GetLstDataByMat(mat));
            }
            return lstRetCombineData;
        }


        public int GetTotalVerts(List<ICombineData> lstCombineData) {
            int nTotal = 0;
            
            for (int i = 0; i < lstCombineData.Count; ++i) {
                Mesh mesh = lstCombineData[i].GetMesh();
                if (mesh == null) {
                    //Debug.LogErrorFormat("{0} GetMesh is null", lstCombineData[i].GetName());
                    continue;
                }
                nTotal += mesh.vertexCount;
            }

            return nTotal;
        }

        public void SetSelections(List<ICombineData> lstDatas) {
#if UNITY_EDITOR
            List<UnityEngine.Object> lstObjs = new List<UnityEngine.Object>();
            for (int i = 0; i < lstDatas.Count; ++i) {
                lstObjs.Add(lstDatas[i].GetObject());
            }
            Selection.objects = lstObjs.ToArray();
#endif
        }

        private void _UnionList<T>(ref List<T> ret, List<T> _in) {
            if (_in == null || _in.Count == 0) {
                return;
            }

            for (int i = 0; i < _in.Count; ++i) {
                T value = _in[i];
                if (!ret.Contains(value)) {
                    ret.Add(value);
                }
            }
        }
    }
}
