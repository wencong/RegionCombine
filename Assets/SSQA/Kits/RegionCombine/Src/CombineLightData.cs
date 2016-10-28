using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CombineLightData : MonoBehaviour {
    public int lightMapIndex = -1;
    public Vector4 lightMapScaleOffset = Vector4.zero;
    
    void Awake() {
        Renderer render = GetComponent<MeshRenderer>();
        if (render && lightMapIndex != -1) {
            render.lightmapIndex = lightMapIndex;
            render.lightmapScaleOffset = lightMapScaleOffset;
        }
    }
}

