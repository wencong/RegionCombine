using System;
using System.Collections.Generic;
using UnityEngine;

namespace RegionCombine {
    public class CombineLayer : ISelector {
        private string m_szLayer = string.Empty;
        public string layer {
            get {
                return m_szLayer;
            }
        }

        public CombineLayer(string szLayer) {
            m_szLayer = szLayer;
        }

        private Dictionary<Material, List<ICombineData>> m_dictMatCombine = null;
        private List<Material> m_lstMaterials = null;
        private List<ICombineData> m_lstIgnore = null;

        public void Init() {
            m_dictMatCombine = new Dictionary<Material, List<ICombineData>>();
            m_lstMaterials = new List<Material>();
            m_lstIgnore = new List<ICombineData>();
        }

        public void UnInit() {
            if (m_dictMatCombine != null) {
                m_dictMatCombine.Clear();
                m_dictMatCombine = null;
            }
            if (m_lstIgnore != null) {
                m_lstIgnore.Clear();
                m_lstIgnore = null;
            }
            if (m_lstMaterials != null) {
                m_lstMaterials.Clear();
                m_lstMaterials = null;
            }
        }

        public void AddCombineData(ICombineData data) {
            if (layer != data.GetLayer()) {
                Debug.LogErrorFormat("can't add {0}[layer:{1}] to  layer[{2}]:", data.GetName(), data.GetLayer(), layer);
                return;
            }

            if (!data.CanCombine()) {
                m_lstIgnore.Add(data);
            }
            else {
                Material mat = data.GetMaterial();
                if (!m_dictMatCombine.ContainsKey(mat)) {
                    m_dictMatCombine.Add(mat, new List<ICombineData>());
                }
                m_dictMatCombine[mat].Add(data);
            }
        }

        public void ShowByMat(Material mat) {
            foreach (var pair in m_dictMatCombine) {
                bool bActive = mat == pair.Key;

                for (int i = 0; i < pair.Value.Count; ++i) {
                    pair.Value[i].SetActive(bActive);
                }
            }
        }

        public void ShowAll(bool bShow) {
            foreach (var pair in m_dictMatCombine) {
                for (int i = 0; i < pair.Value.Count; ++i) {
                    pair.Value[i].SetActive(bShow);
                }
            }
        }

        public List<ICombineData> GetCombineDataByMat(Material mat) {
            if (m_dictMatCombine.ContainsKey(mat)) {
                return m_dictMatCombine[mat];
            }
            return null;
        }

        public List<Material> lstMaterial {
            get {
                return m_lstMaterials;
            }
        }
    }
}
