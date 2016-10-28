using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace RegionCombine {
    [ExecuteInEditMode]
    public class RegionCombinePanel : EditorWindow {
        private bool bDebug = true;
        private float fDebugLine = 100.0f;

        private Transform m_regionRoot = null;
        private Rect m_regionRect;
        private int m_regionCell = 32;

        private Vector2 m_scroolMat = Vector2.zero;
        private Vector2 m_scrollObj = Vector2.zero;
        private CombineRegion[] m_regions = null;

        //private LayerSwitch[] m_layerSwitch = null;
        //private List<CombineRegion> m_lstSelectRegions = new List<CombineRegion>();
        //private List<MaterialLabelInfo> m_lstMaterialLabel = new List<MaterialLabelInfo>();
        private Dictionary<Material, List<ICombineData>> m_dictMatObjs = new Dictionary<Material, List<ICombineData>>();
        private Material m_selectMat = null;

        private int nRow = 0;
        private bool bShowMapRect = false;

        [MenuItem("SSQA/RegionCombine")]
        public static void ShowCombinePanel() {
            //ScriptableWizard.DisplayWizard<RegionCombineWizard>("Region Combine", "Create");
            GetWindow<RegionCombinePanel>().Show();
        }

        void Awake() {
            JXSJDataDynamicLoadMgr dynamicLoad = GameObject.FindObjectOfType<JXSJDataDynamicLoadMgr>();
            if (dynamicLoad != null) {
                m_regionRect = dynamicLoad.mapRect;
            }

            _InitLevelTransform();
            OnSelectLevelChange(0);
            //OnSelectLayerChange(0);

            Selection.selectionChanged = OnSelectObjectChange;
        }

        void OnDisable() {
            Selection.selectionChanged = null;
            for (int i = 0; i < levels_trans.Length; ++i) {
                levels_trans[i].gameObject.SetActive(true);
            }

            if (m_regions != null) {
                for (int i = 0; i < m_regions.Length; ++i) {
                    m_regions[i].ShowAll(true);
                }
            }
            
            _UnInit();
        }


        private List<ICombineData> lstData = new List<ICombineData>();

        public void OnSelectObjectChange() {
            if (m_selectMat == null) {
                return;
            }

            lstData.Clear();

            for (int i = 0; i < Selection.gameObjects.Length; ++i) {
                CombineMeshData combineData = new CombineMeshData(Selection.gameObjects[i]);
                combineData.bSelect = true;
                /*
                if (!combineData.CanCombine()) {
                    EditorUtility.DisplayDialog("", string.Format("{0} 不支持合并", combineData.GetName()), "OK");
                    CombineRegionMgr.Instance.SetSelections(m_dictMatObjs[m_selectMat]);
                    return;
                }*/
                lstData.Add(combineData);
            }

            Repaint();
        }

        private void _Init() {

        }

        private void _UnInit() {
            _UnInitRegion();
            _UnInitLevelTransform();
        }

        /// <summary>
        ///  Level
        /// </summary>
        private void _InitLevelTransform() {
            for (int i = 0; i < 3; ++i) {
                levels_trans[i] = GameObject.Find(string.Format("Environment/Models/Level_{0}", i + 1)).transform;
                levels_name[i] = levels_trans[i].name;
            }
        }

        private void _UnInitLevelTransform() {
            levels_trans = null;
            levels_name = null;
        }

        /// <summary>
        ///  Region
        /// </summary>
        private void _InitRegion() {
            CombineRegionMgr.Instance.Init(m_regionRect, m_regionCell, m_regionRoot);

            layers_name = CombineRegionMgr.Instance.layers;
            m_regions = CombineRegionMgr.Instance.regions;

            nRow = (int)Mathf.Sqrt((float)m_regions.Length);
        }

        private void _UnInitRegion() {
            CombineRegionMgr.Instance.Clear();
            m_regions = null;
            layers_name = null;
        }

        /// <summary>
        /// UI Settings
        /// </summary>
        private int m_nButtonWidth = 50;
        private GUIStyle m_styleButton = null;
        private GUIStyle m_styleLabel = null;
        private void _InitUIStyle() {
            if (m_styleButton == null) {
                m_styleButton = new GUIStyle(GUI.skin.button);
            }
            if (m_styleLabel == null) {
                m_styleLabel = new GUIStyle(GUI.skin.label);
            }
        }

        void OnGUI() {
            _InitUIStyle();

            _ShowLevelSelectToggle();

            if (m_regionRoot == null) {
                return;
            }

            if (m_regions == null) {
                return;
            }

            _ShowLayerSelectToggle();

            GUILayout.BeginHorizontal();

            _ShowMapRect();

            _ShowCombineDataList();

            GUILayout.EndHorizontal();
        }

        private void ShowAll(bool bShow) {
            for (int i = 0; i < m_regions.Length; ++i) {
                m_regions[i].ShowAll(bShow);
                m_regions[i].bSelect = m_regions[i].HasData();
            }
        }

        public void RefreshLayerData() {
            if (m_regions == null) {
                return;
            }

            if (current_level == -1 || current_layer == -1) {
                return;
            }

            m_dictMatObjs.Clear();

            for (int i = 0; i < m_regions.Length; ++i) {
                if (!m_regions[i].HasData()) {
                    continue;
                }

                List<ICombineData> lstData = m_regions[i].GetLstDataByLayer(layers_name[current_layer]);
                if (lstData.Count == 0) {
                    continue;   
                }

                for (int j = 0; j < lstData.Count; ++j) {
                    ICombineData combineData = lstData[j];
                    _AddCombineData(combineData);
                }
            }
        }

        private void _AddCombineData(ICombineData combineData) {
            Material mat = combineData.GetMaterial();

            if (!m_dictMatObjs.ContainsKey(mat)) {
                m_dictMatObjs.Add(mat, new List<ICombineData>());
            }

            m_dictMatObjs[mat].Add(combineData);
        }


        private void _ShowMapRect() {
            GUILayout.BeginVertical(GUILayout.Width(nRow * m_nButtonWidth));

            if (bShowMapRect) {
                GUILayout.BeginVertical("Box", GUILayout.Width(nRow * m_nButtonWidth));
                for (int i = 0; i < nRow; ++i) {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < nRow; ++j) {
                        int nIndex = nRow * i + j;
                        CombineRegion region = m_regions[nIndex];
                        if (region.HasData()) {
                            m_styleButton.normal.textColor = region.bSelect ? Color.green : Color.white;
                            if (GUILayout.Button(nIndex.ToString(), m_styleButton, GUILayout.Width(m_nButtonWidth), GUILayout.Height(m_nButtonWidth))) {
                                /*
                                region.bSelect = !region.bSelect;
                                _RefreshSelectRegions();
                                _RefreshSelectObj();*/
                            }
                        }
                        else {
                            m_styleLabel.alignment = TextAnchor.MiddleCenter;
                            GUILayout.Label("Empty", m_styleLabel, GUILayout.Width(m_nButtonWidth), GUILayout.Height(m_nButtonWidth));
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            _ShowMaterialList();

            GUILayout.EndVertical();
        }

        private void _ShowMaterialList() {
            GUILayout.BeginVertical("Box", GUILayout.Width(nRow * m_nButtonWidth));
            {
                m_scroolMat = GUILayout.BeginScrollView(m_scroolMat);
                foreach (var pair in m_dictMatObjs) {
                    m_styleButton.normal.textColor = m_selectMat == pair.Key ? Color.green : Color.white;

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(pair.Key.name, m_styleButton, GUILayout.Width(m_nButtonWidth * 3))) {
                        OnSelectMaterial(pair.Key);
                    }

                    GUILayout.Label(pair.Value.Count.ToString(), GUILayout.Width(m_nButtonWidth));
                    GUILayout.Label(CombineRegionMgr.Instance.GetTotalVerts(pair.Value).ToString(), GUILayout.Width(m_nButtonWidth));
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        private bool IsPrefabInstance(GameObject gameObject) {
            PrefabType type = PrefabUtility.GetPrefabType(gameObject);
            UnityEngine.Object root = PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);

            return type == PrefabType.PrefabInstance && root.name == gameObject.name;
        }

        private bool CanCombine(List<ICombineData> lstCombineDatas) {
            if (lstCombineDatas.Count == 0) {
                return false;
            }

            int nLightMapIndex = -1;

            for (int i = 0; i < lstCombineDatas.Count; ++i) {
                if (!lstCombineDatas[i].CanCombine()) {
                    EditorUtility.DisplayDialog("", string.Format("{0} 不支持合并", lstCombineDatas[i].GetName()), "OK");
                    return false;
                }

                if (!IsPrefabInstance(lstCombineDatas[i].GetObject() as GameObject)) {
                    EditorUtility.DisplayDialog("", string.Format("{0} 包含多层级Prefab", lstCombineDatas[i].GetName()), "OK");
                    return false;
                }

                if (nLightMapIndex == -1) {
                    nLightMapIndex = lstCombineDatas[i].GetRenderer().lightmapIndex;
                }

                if (nLightMapIndex != lstCombineDatas[i].GetRenderer().lightmapIndex) {
                    EditorUtility.DisplayDialog("", "指向多张光照贴图", "OK");
                    return false;
                }
            }

            return true;
        }

        private void _ShowCombineDataList() {
            if (m_selectMat == null) {
                return;
            }

            //List<ICombineData> lstData = m_dictMatObjs[m_selectMat];

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal("Box");
            {
                if (GUILayout.Button("合并下列物件", GUILayout.Width(m_nButtonWidth * 2))) {

                    List<ICombineData> lst = new List<ICombineData>();
                    for (int i = 0; i < lstData.Count; ++i) {
                        if (((CombineMeshData)lstData[i]).bSelect) {
                            lst.Add(lstData[i]);
                        }
                    }

                    if (CanCombine(lst)) {
                        GameObject combineObj = CombineProcessor.CombineMesh(lst, m_selectMat);
                        if (combineObj != null) {
                            combineObj.layer = LayerMask.NameToLayer(layers_name[current_layer]);
                            SaveCombineObjAsPrefab(combineObj, true);
                            //OnSelectLevelChange(current_level);
                            _InitRegion();
                            RefreshLayerData();
                            m_selectMat = null;
                        }
                        else {
                            EditorUtility.DisplayDialog("Error", "合并失败，请检查日志", "OK");
                        }
                    }
                }

                GUILayout.Label(string.Format("{0} Objects", lstData.Count), GUILayout.Width(m_nButtonWidth * 2));
                GUILayout.Label(string.Format("{0} Vertexs", CombineRegionMgr.Instance.GetTotalVerts(lstData)), GUILayout.Width(m_nButtonWidth * 2));
            }

            GUILayout.EndHorizontal();

            m_scrollObj = GUILayout.BeginScrollView(m_scrollObj);

            for (int i = 0; i < lstData.Count; ++i) {
                ((CombineMeshData)lstData[i]).bSelect = GUILayout.Toggle(((CombineMeshData)lstData[i]).bSelect, lstData[i].GetName(), GUILayout.Width(m_nButtonWidth * 3));
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }


        private Transform[] levels_trans = new Transform[3];
        private string[] levels_name = new string[3];
        private int current_level = -1;

        private string[] layers_name = null;
        private int current_layer = -1;

        public void _ShowLevelSelectToggle() {
            GUILayout.BeginHorizontal("Box", GUILayout.Width(nRow * m_nButtonWidth));

            int nSelectLevel = GUILayout.SelectionGrid(current_level, levels_name, levels_name.Length, GUI.skin.toggle);
            if (nSelectLevel != current_level) {
                OnSelectLevelChange(nSelectLevel);
            }

            GUILayout.EndHorizontal();
        }

        private void _ShowLayerSelectToggle() {
            GUILayout.BeginHorizontal("Box", GUILayout.Width(nRow * m_nButtonWidth));
            {
                int nSelectLayer = GUILayout.SelectionGrid(current_layer, layers_name, layers_name.Length);
                if (nSelectLayer != current_layer) {
                    OnSelectLayerChange(nSelectLayer);
                }

                if (GUILayout.Button("ShowAll", GUILayout.Width(m_nButtonWidth * 2))) {
                    ShowAll(true);
                }

                if (GUILayout.Button("自动合并所有区域", GUILayout.Width(m_nButtonWidth * 3))) {
                    for (int i = 0; i < m_regions.Length; ++i) {
                        List<Material> lstMats = m_regions[i].lstMaterials;
                        for (int j = 0; j < lstMats.Count; ++j) {

                            string szMsg = string.Format("Combine Region:{0}, Material:{1}", i, lstMats[j].name);
                            EditorUtility.DisplayCancelableProgressBar("", szMsg, (float)j / lstMats.Count);

                            GameObject combineObj = m_regions[i].CombineByMat(lstMats[j]);
                            if (combineObj != null) {
                                //SaveCombineObjAsPrefab(combineObj, false);
                            }
                        }
                    }

                    EditorUtility.ClearProgressBar();
                }
            }

            GUILayout.EndHorizontal();
        }

        public void OnSelectLevelChange(int nLevel) {
            current_level = nLevel;
            m_regionRoot = levels_trans[current_level];

            _InitRegion();
            ShowAll(true);

            levels_trans[nLevel].gameObject.SetActive(true);
            levels_trans[(nLevel + 1) % 3].gameObject.SetActive(false);
            levels_trans[(nLevel + 2) % 3].gameObject.SetActive(false);

            current_layer = -1;
            m_dictMatObjs.Clear();

            CombineRegionMgr.Instance.RefreshLeaves();
        }

        public void OnSelectLayerChange(int nLayer) {
            current_layer = nLayer;
            m_selectMat = null;
            RefreshLayerData();
        }

        public void OnSelectMaterial(Material mat) {
            m_selectMat = mat;

            ShowAll(false);
            for (int i = 0; i < m_dictMatObjs[mat].Count; ++i) {
                m_dictMatObjs[mat][i].SetActive(true);
            }

            CombineRegionMgr.Instance.SetSelections(m_dictMatObjs[mat]);
        }

        public void Update() {
            if (bDebug) {
                CombineRegionMgr.Instance.DrawDebugLine(fDebugLine);
                SceneView.RepaintAll();
            }
        }

        public void SaveCombineObjAsPrefab(GameObject gameObject, bool bSaveObj = false) {
            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            string szScenePath = scene.path.Substring(0, scene.path.Length - scene.name.Length - ".unity".Length);
            string szPrefabPath = string.Format("{0}{1}{2}", szScenePath, scene.name, "/Prefabs");
            string szModelPath = string.Format("{0}{1}{2}", szScenePath, scene.name, "/Models");

            if (!Directory.Exists(szModelPath)) {
                Directory.CreateDirectory(szModelPath);
            }

            if (!Directory.Exists(szPrefabPath)) {
                Directory.CreateDirectory(szPrefabPath);
            }

            szPrefabPath = string.Format("{0}/{1}.prefab", szPrefabPath, gameObject.name);
            szModelPath = string.Format("{0}/{1}.asset", szModelPath, gameObject.name);

            if (bSaveObj) {
                //ObjMeshExporter.BakeMesh(gameObject, szModelPath, true);
                AssetDatabase.CreateAsset(gameObject.GetComponent<MeshFilter>().sharedMesh, szModelPath);
            }

            PrefabUtility.CreatePrefab(szPrefabPath, gameObject, ReplacePrefabOptions.ConnectToPrefab);
        }
    }
}
