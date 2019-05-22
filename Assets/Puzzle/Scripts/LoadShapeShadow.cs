using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 加载图片阴影
/// </summary>
public class LoadShapeShadow : MonoBehaviour {
	// Use this for initialization
	void Start () {
		Debug.Log ("自动调整阴影应该是临时的.稍后通过在检查员中手动放置阴影来替换它");
        //获取网格过滤器组件
		this.GetComponent<MeshFilter> ().mesh = transform.parent.GetComponent<MeshFilter> ().mesh;
	}
}
