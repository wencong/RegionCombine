using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RegionCombine {
    public abstract class ISelector {
        public bool bSelect = false;
    }

    public enum DetailLevel : int {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3
    }

    public interface ICombineData {
        //是否支持合并
        bool CanCombine();
        //名字
        string GetName();
        //获取层级名称
        string GetLayer();
        void SetLayer(string szLayer);
        //获取材质名称
        Material GetMaterial();
        //获取网格
        Mesh GetMesh();

        void SetActive(bool bActive);

        UnityEngine.Object GetObject();
        Renderer GetRenderer();
    }

    public class CombineMeshData : ISelector, ICombineData {
        public string GetLayer() {
            if (m_mr != null) {
                return LayerMask.LayerToName(m_mr.gameObject.layer);
            }
            return null;
        }

        public void SetLayer(string szLayer) {
            if (m_mr != null) {
                m_mr.gameObject.layer = LayerMask.NameToLayer(szLayer);
            }
        }

        public string GetName() {
            return m_name;
        }

        public Material GetMaterial() {
            if (m_mat != null && m_mat.Length == 1) {
                return m_mat[0];
            }
            return null;
        }

        public Mesh GetMesh() {
            if (m_mf != null) {
                return m_mf.sharedMesh;
            }
            return null;
        }

        public bool CanCombine() {
            bool bRet = false;

            if (m_mr == null) {
                goto Exit0;
            }
            
            if (m_mf == null) {
                goto Exit0;
            }

            if (m_mat == null || m_mat.Length > 1) {
                goto Exit0;
            }

            //已经在合并节点下的不能再合成。
            if (m_mr.transform.parent != null && m_mr.transform.parent.name.Contains("_Combine_")) {
                goto Exit0;
            }

            bRet = true;
        Exit0:
            return bRet;
        }

        public void SetActive(bool bActive) {
            if (m_mr != null) {
                m_mr.gameObject.SetActive(bActive);
            }
            bSelect = bActive;
        }

        public UnityEngine.Object GetObject() {
            if (m_mr != null) {
                return m_mr.gameObject;
            }
            return null;
        }

        public Renderer GetRenderer() {
            return m_mr;
        }

        public CombineMeshData(MeshRenderer meshRenderer) {
            m_mr = meshRenderer;

            if (m_mr != null) {
                m_mf = m_mr.gameObject.GetComponent<MeshFilter>();
                m_mat = m_mr.sharedMaterials;
            }
        }

        public CombineMeshData(GameObject gameObject) {
            m_name = gameObject.name;
            m_mr = gameObject.GetComponent<MeshRenderer>();

            if (m_mr != null) {
                m_mf = m_mr.gameObject.GetComponent<MeshFilter>();
                m_mat = m_mr.sharedMaterials;
            }
        }

        private MeshRenderer m_mr = null;
        private MeshFilter m_mf = null;
        private Material[] m_mat = null;
        private string m_name;
    }
}
