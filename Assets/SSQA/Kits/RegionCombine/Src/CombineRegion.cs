using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RegionCombine {
    public class CombineRegion : ISelector {
        //private List<CombineLayer> m_lstCombineLayer = null;
        private List<string> m_lstLayers = null;
        private List<Material> m_lstMaterials = null;
        private Dictionary<Material, List<ICombineData>> m_dictMatDatas = null;
        private List<ICombineData> m_lstIgnore = null;

        public void Init() {
            //m_lstCombineLayer = new List<CombineLayer>();
            m_lstLayers = new List<string>();
            m_lstMaterials = new List<Material>();
            m_dictMatDatas = new Dictionary<Material, List<ICombineData>>();
            m_lstIgnore = new List<ICombineData>();
        }

        public void UnInit() {
            /*
            if (m_lstCombineLayer != null) {
                for (int i = 0; i < m_lstCombineLayer.Count; ++i) {
                    m_lstCombineLayer[i].UnInit();
                }
            }
            m_lstCombineLayer.Clear();*/
            m_dictMatDatas.Clear();
            m_lstLayers.Clear();
            m_lstMaterials.Clear();
        }

        public bool HasData() {
            return m_dictMatDatas != null && m_dictMatDatas.Count != 0;
        }

        public void AddCombineData(ICombineData combineData) {
            if (!combineData.CanCombine()) {
                m_lstIgnore.Add(combineData);
            }

            else {
                Material mat = combineData.GetMaterial();
                if (!m_dictMatDatas.ContainsKey(mat)) {
                    m_dictMatDatas.Add(mat, new List<ICombineData>());
                    m_lstMaterials.Add(mat);
                }

                m_dictMatDatas[mat].Add(combineData);

                string layer = combineData.GetLayer();
                if (!m_lstLayers.Contains(layer)) {
                    m_lstLayers.Add(layer);
                }
            }
        }

        public List<string> lstLayers {
            get {
                return m_lstLayers;
            }
            private set {}
        }

        public List<Material> lstMaterials {
            get {
                return m_lstMaterials;
            }
        }

        public List<ICombineData> GetLstDataByMat(Material mat) {
            if (m_dictMatDatas.ContainsKey(mat)) {
                return m_dictMatDatas[mat];
            }
            return null;
        }

        public List<ICombineData> GetLstDataByLayer(string layer) {
            List<ICombineData> retList = new List<ICombineData>();

            if (lstLayers.Contains(layer)) {
                for (int i = 0; i < m_lstMaterials.Count; ++i) {
                    List<ICombineData> lstData = m_dictMatDatas[m_lstMaterials[i]];
                    for (int j = 0; j < lstData.Count; ++j) {
                        if (lstData[j].GetLayer() == layer) {
                            retList.Add(lstData[j]);
                        }
                    }
                }
            }

            return retList;
        }

        public void ShowAll(bool bShow) {
            for (int i = 0; i < m_lstMaterials.Count; ++i) {
                List<ICombineData> lstData = m_dictMatDatas[m_lstMaterials[i]];
                for (int j = 0; j < lstData.Count; ++j) {
                    lstData[j].SetActive(bShow);
                }
            }

            for (int i = 0; i < m_lstIgnore.Count; ++i) {
                m_lstIgnore[i].SetActive(bShow);
            }
        }

        /*
        public void ShowByMat(Material mat) {
            for (int i = 0; i < m_lstMaterials.Count; ++i) {
                List<ICombineData> lstData = m_dictMatDatas[m_lstMaterials[i]];
                bool bShow = mat == m_lstMaterials[i];
                for (int j = 0; j < lstData.Count; ++j) {
                    lstData[j].SetActive(bShow);
                }
            }
        }*/

        public GameObject CombineByMat(Material mat, bool bDestory = false) {
            if (!m_dictMatDatas.ContainsKey(mat)) {
                return null;
            }

            return CombineProcessor.CombineMesh(m_dictMatDatas[mat], mat, bDestory);
        }

        public GameObject CombineByMatInSameLayer(Material mat, string layer) {
            if (!m_dictMatDatas.ContainsKey(mat)) {
                return null;
            }

            if (!m_lstLayers.Contains(layer)) {
                return null;
            }

            return null;
        }

        public void CombineByMatAndLayer(Material mat, bool bDestory = false) {
            if (!m_dictMatDatas.ContainsKey(mat)) {
                return;
            }

            List<ICombineData> lstCombineData = m_dictMatDatas[mat];
            List<ICombineData> lstSameLayerData = new List<ICombineData>();

            for (int i = 0; i < m_lstLayers.Count; ++i) {
                string szLayer = m_lstLayers[i];
                lstSameLayerData.Clear();

                for (int j = 0; j < lstCombineData.Count; ++j) {
                    if (lstCombineData[j].GetLayer() == szLayer) {
                        lstSameLayerData.Add(lstCombineData[j]);
                    }
                }

                GameObject combineObj = CombineProcessor.CombineMesh(lstSameLayerData, mat, bDestory);
                if (combineObj != null) {
                    combineObj.layer = LayerMask.NameToLayer(szLayer);
                }
            }

        }
    }
}
