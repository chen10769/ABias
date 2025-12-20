using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 挂载到每个墙面物体上
public class WallTilingAdjuster : MonoBehaviour {
    public Vector2 tilingScale = Vector2.one;

    void Start() {
        Renderer renderer = GetComponent<Renderer>();
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        
        // 获取当前材质属性
        renderer.GetPropertyBlock(props); 
        
        // 设置纹理平铺 (ST = Scale & Translation)
        props.SetVector("_MainTex_ST", new Vector4(
            tilingScale.x, 
            tilingScale.y, 
            0,  // Offset X
            0   // Offset Y
        ));
        
        renderer.SetPropertyBlock(props);
    }
}
