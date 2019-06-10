using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拼图控制
/// </summary>
public class MovePieces : MonoBehaviour {
    //执行此选择
	bool isSelected;

	float distanceRay; //定位点击时计算
    GameObject piece; //分配点击时,将移动。
    Transform[] piecesMove;
	[HideInInspector]
	public AssemblePieces assemblePieces;
	[HideInInspector]
	public float positionZ = -0.001f;
	[HideInInspector]
	public bool puzzlePlaying;
	Vector3 offset;

	void Update () {
		//鼠标信息
		if (puzzlePlaying) {
            //按下鼠标
			if (Input.GetMouseButtonDown(0)) {
				TouchClick ();
			}
			//移动鼠标
			if (isSelected) {
				MovePiece ();
			}
			//释放鼠标
			if (Input.GetMouseButtonUp (0)) {
				FreePiece ();
			}
		}
	}

    /// <summary>
    /// 定位点击点
    /// </summary>
	void TouchClick(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);   //从相机到点的触控点
        RaycastHit hit;
		if (Physics.Raycast (ray, out hit)) {                          //接收触控，如果有击中目标
            if (hit.collider.gameObject.tag == "PiezaPuzzle") {
				positionZ -= 0.01f;
				distanceRay = hit.distance;
				piece = hit.collider.gameObject;
				offset = hit.point-hit.collider.gameObject.transform.position;
				offset.z = 0;
				hit.collider.transform.GetChild (0).GetComponent<MeshRenderer> ().enabled = false; //停用阴影
                if (piece.transform.parent.name == "GrupoPiezas") {
					BoxCollider[]sonsAReplaceBox = piece.transform.parent.GetComponentsInChildren<BoxCollider> ();
					Transform[] sonsAReplace = new Transform[sonsAReplaceBox.Length];
					for (int i = 0; i < sonsAReplace.Length; i++) {
						sonsAReplace[i] = sonsAReplaceBox [i].transform;
						sonsAReplace [i].GetChild (0).GetComponent<MeshRenderer> ().enabled = false; //停用阴影
                    }

					piecesMove = new Transform[sonsAReplace.Length];
					for (int i = 0; i < piecesMove.Length; i++) {
						piecesMove [i] = sonsAReplace [i];
					}
					sonsAReplace = piecesMove; //这四行避免了父节点被捕捉[0]

                    Vector3[] posicionRelativa = new Vector3[sonsAReplace.Length];
					GameObject newParent = piece.transform.parent.gameObject;

					for (int i = 0; i < sonsAReplace.Length; i++) {
						if (sonsAReplace [i].tag == "PiezaPuzzle" && sonsAReplace [i] != piece.transform) {
							posicionRelativa [i] = sonsAReplace [i].position - piece.transform.position;
							sonsAReplace [i].transform.parent = newParent.transform.parent;
						}
					}

					newParent.transform.position = Input.mousePosition;

					for (int i = 0; i < sonsAReplace.Length; i++) {
						if (sonsAReplace [i].tag == "PiezaPuzzle") {
							sonsAReplace [i].transform.parent = newParent.transform;
							sonsAReplace [i].localPosition = posicionRelativa [i];
						}
					}

					piece = newParent;
				} else {
					piecesMove = new Transform[1];
					piecesMove [0] = piece.transform;
				}
				isSelected = true;
			}
		}
	}

    /// <summary>
    /// 移动碎片(每帧调用)
    /// </summary>
	void MovePiece(){
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		Vector3 limitRay = ray.GetPoint (distanceRay);
		limitRay = new Vector3 (limitRay.x, limitRay.y, positionZ);
		piece.transform.position = limitRay - offset;
	}
    
    /// <summary>
    /// 释放碎片
    /// </summary>
	void FreePiece(){
		if(isSelected){
			piece.transform.position = new Vector3 (piece.transform.position.x, piece.transform.position.y, positionZ);


			for (int i = 0; i < piecesMove.Length; i++) {
				if(piecesMove [i].tag != "PiezaColocada"){
					piecesMove [i].GetChild (0).GetComponent<MeshRenderer> ().enabled = true; //重新激活阴影
                }
				string numDePieza = piecesMove[i].gameObject.name.Substring (6, piecesMove[i].gameObject.name.Length-6);
				string[] numsPiezas = numDePieza.Split('x');
				assemblePieces.ReadjustPieces (int.Parse(numsPiezas [1]), int.Parse(numsPiezas [0])); //这些碎片颠倒编号，首先是垂直顺序，然后是水平
            }

			if (piece.name == "GrupoPiezas") {
				foreach (Transform pieceSon in piece.transform) {
					pieceSon.position = new Vector3 (pieceSon.position.x, pieceSon.position.y, positionZ);
				}
			}

			piece = null;
			piecesMove = new Transform[0];
			isSelected = false;
		}
	}
}
