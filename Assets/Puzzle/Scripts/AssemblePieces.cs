using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 组合型碎片
/// </summary>
public class AssemblePieces : MonoBehaviour {
    /// <summary>
    /// 拼图碎片位置信息
    /// </summary>
	public Transform[,] piecesPuzzle;
    /// <summary>
    /// 未放置的拼图碎片
    /// </summary>
	public List<GameObject> piecesUnplaced;

    /// <summary>
    /// 余地间隔
    /// </summary>
	float margenSnap;
    /// <summary>
    /// 碎片宽
    /// </summary>
	int widthPuzzle;
    /// <summary>
    /// 碎片高
    /// </summary>
	int heightPuzzle;
    /// <summary>
    /// 初始位置
    /// </summary>
	public Vector3[,] posicionInicial;
    GameObject objetoControlador;
    MovePieces movePieces;
    ControlUI controlUI;

    
    public struct DistanceBetweenPieces {
        public float up;//上部
        public float down;//下部
        public float right;//右部
        public float left;//左部
    }
    /// <summary>
    /// 碎片与碎片之间的距离
    /// </summary>
    DistanceBetweenPieces[,] distanceBetweenPieces;


    void Start() {
        //获取主客户端游戏对象
        objetoControlador = GameObject.Find("Controlador");
        //获取碎片控制组件
        movePieces = objetoControlador.GetComponent<MovePieces>();
        //获取主客户端控制器(界面UI控制器)
        controlUI = objetoControlador.GetComponent<ControlUI>();
        //未放置的拼图碎片
        piecesUnplaced = new List<GameObject>();
        //拼图碎片尺寸(NXN?)
        string dimensionesPuzzle = this.gameObject.name.Substring(5, this.gameObject.name.Length - 5);
        string[] dosDimensiones = dimensionesPuzzle.Split('x');
        widthPuzzle = int.Parse(dosDimensiones[0]);
        heightPuzzle = int.Parse(dosDimensiones[1]);
        //注意矩阵数组和锯齿数组的区别
        piecesPuzzle = new Transform[widthPuzzle, heightPuzzle];
        posicionInicial = new Vector3[widthPuzzle, heightPuzzle];
        distanceBetweenPieces = new DistanceBetweenPieces[widthPuzzle, heightPuzzle];
        //间隔
        margenSnap = 0.2f - (widthPuzzle * 0.01f); //它从0.16（4X4的谜题）到（10X16的谜题）
        //记录初始位置
        for (int i = 0; i < heightPuzzle; i++) {
            for (int k = 0; k < widthPuzzle; k++) {
                piecesPuzzle[k, i] = transform.GetChild(i * widthPuzzle + k);
                posicionInicial[k, i] = piecesPuzzle[k, i].position;
                piecesUnplaced.Add(piecesPuzzle[k, i].gameObject);
            }
        }
        for (int i = 0; i < heightPuzzle; i++) {
            for (int k = 0; k < widthPuzzle; k++) {
                if (k > 0) {
                    distanceBetweenPieces[k, i].left = Mathf.Abs(piecesPuzzle[k, i].position.x - piecesPuzzle[k - 1, i].position.x);
                }
                if (k < widthPuzzle - 1) {
                    distanceBetweenPieces[k, i].right = Mathf.Abs(piecesPuzzle[k + 1, i].position.x - piecesPuzzle[k, i].position.x);
                }
                if (i > 0) {
                    distanceBetweenPieces[k, i].up = Mathf.Abs(piecesPuzzle[k, i - 1].position.y - piecesPuzzle[k, i].position.y);
                }
                if (i < heightPuzzle - 1) {
                    distanceBetweenPieces[k, i].down = Mathf.Abs(piecesPuzzle[k, i].position.y - piecesPuzzle[k, i + 1].position.y);
                }
            }
        }
    }

    /// <summary>
    /// 调整碎片,检查是否组合
    /// </summary>
    /// <param name="k"></param>
    /// <param name="i"></param>
	public void ReadjustPieces(int k, int i) {
        bool isLeft = false;
        bool isRight = false;
        bool isUp = false;
        bool isDown = false;

        if (k > 0 && distanceBetweenPieces[k, i].left <= Mathf.Abs(piecesPuzzle[k, i].position.x - piecesPuzzle[k - 1, i].position.x) + margenSnap
             && distanceBetweenPieces[k, i].left >= Mathf.Abs(piecesPuzzle[k, i].position.x - piecesPuzzle[k - 1, i].position.x) - margenSnap
             && piecesPuzzle[k, i].position.y <= piecesPuzzle[k - 1, i].position.y + margenSnap
             && piecesPuzzle[k, i].position.y >= piecesPuzzle[k - 1, i].position.y - margenSnap
             && piecesPuzzle[k, i].position.x > piecesPuzzle[k - 1, i].position.x) {
            isLeft = true;

            Vector3 positionBefore = piecesPuzzle[k, i].transform.position;
            piecesPuzzle[k, i].transform.position = new Vector3(piecesPuzzle[k - 1, i].position.x + distanceBetweenPieces[k - 1, i].right, piecesPuzzle[k - 1, i].position.y, piecesPuzzle[k - 1, i].position.z);
            Vector3 positionCurrent = piecesPuzzle[k, i].transform.position;

            if (piecesPuzzle[k, i].parent.name == "GrupoPiezas") {
                piecesPuzzle[k, i].transform.position = positionBefore;
                piecesPuzzle[k, i].parent.transform.position += positionCurrent - positionBefore;
            }

            MakeGroupPieces(k, i, "left");
        }
        if (k < widthPuzzle - 1 && distanceBetweenPieces[k, i].right <= Mathf.Abs(piecesPuzzle[k + 1, i].position.x - piecesPuzzle[k, i].position.x) + margenSnap
             && distanceBetweenPieces[k, i].right >= Mathf.Abs(piecesPuzzle[k + 1, i].position.x - piecesPuzzle[k, i].position.x) - margenSnap
             && piecesPuzzle[k, i].position.y <= piecesPuzzle[k + 1, i].position.y + margenSnap
             && piecesPuzzle[k, i].position.y >= piecesPuzzle[k + 1, i].position.y - margenSnap
             && piecesPuzzle[k, i].position.x < piecesPuzzle[k + 1, i].position.x) {
            isRight = true;

            Vector3 positionBefore = piecesPuzzle[k, i].transform.position;
            piecesPuzzle[k, i].transform.position = new Vector3(piecesPuzzle[k + 1, i].position.x - distanceBetweenPieces[k + 1, i].left, piecesPuzzle[k + 1, i].position.y, piecesPuzzle[k + 1, i].position.z);
            Vector3 positionCurrent = piecesPuzzle[k, i].transform.position;

            if (piecesPuzzle[k, i].parent.name == "GrupoPiezas") {
                piecesPuzzle[k, i].transform.position = positionBefore;
                piecesPuzzle[k, i].parent.transform.position += positionCurrent - positionBefore;
            }
            MakeGroupPieces(k, i, "right");
        }
        if (i > 0 && distanceBetweenPieces[k, i].up <= Mathf.Abs(piecesPuzzle[k, i - 1].position.y - piecesPuzzle[k, i].position.y) + margenSnap
             && distanceBetweenPieces[k, i].up >= Mathf.Abs(piecesPuzzle[k, i - 1].position.y - piecesPuzzle[k, i].position.y) - margenSnap
             && piecesPuzzle[k, i].position.x <= piecesPuzzle[k, i - 1].position.x + margenSnap
             && piecesPuzzle[k, i].position.x >= piecesPuzzle[k, i - 1].position.x - margenSnap
             && piecesPuzzle[k, i].position.y < piecesPuzzle[k, i - 1].position.y) {
            isUp = true;

            Vector3 positionBefore = piecesPuzzle[k, i].transform.position;
            piecesPuzzle[k, i].transform.position = new Vector3(piecesPuzzle[k, i - 1].position.x, piecesPuzzle[k, i - 1].position.y - distanceBetweenPieces[k, i - 1].down, piecesPuzzle[k, i - 1].position.z);
            Vector3 positionCurrent = piecesPuzzle[k, i].transform.position;

            if (piecesPuzzle[k, i].parent.name == "GrupoPiezas") {
                piecesPuzzle[k, i].transform.position = positionBefore;
                piecesPuzzle[k, i].parent.transform.position += positionCurrent - positionBefore;
            }
            MakeGroupPieces(k, i, "up");
        }
        if (i < heightPuzzle - 1 && distanceBetweenPieces[k, i].down <= Mathf.Abs(piecesPuzzle[k, i].position.y - piecesPuzzle[k, i + 1].position.y) + margenSnap
             && distanceBetweenPieces[k, i].down >= Mathf.Abs(piecesPuzzle[k, i].position.y - piecesPuzzle[k, i + 1].position.y) - margenSnap
             && piecesPuzzle[k, i].position.x <= piecesPuzzle[k, i + 1].position.x + margenSnap
             && piecesPuzzle[k, i].position.x >= piecesPuzzle[k, i + 1].position.x - margenSnap
             && piecesPuzzle[k, i].position.y > piecesPuzzle[k, i + 1].position.y) {
            isDown = true;

            Vector3 positionBefore = piecesPuzzle[k, i].transform.position;
            piecesPuzzle[k, i].transform.position = new Vector3(piecesPuzzle[k, i + 1].position.x, piecesPuzzle[k, i + 1].position.y + distanceBetweenPieces[k, i + 1].up, piecesPuzzle[k, i + 1].position.z);
            Vector3 positionCurrent = piecesPuzzle[k, i].transform.position;

            if (piecesPuzzle[k, i].parent.name == "GrupoPiezas") {
                piecesPuzzle[k, i].transform.position = positionBefore;
                piecesPuzzle[k, i].parent.transform.position += positionCurrent - positionBefore;
            }
            MakeGroupPieces(k, i, "down");
        }
        AdjustPiece();
        CheckCompleted();
    }

    /// <summary>
    /// 调整碎片位置
    /// </summary>
	void AdjustPiece() {
        //成功贴合一个碎片
        bool colocaAlgunaPieza = false;
        for (int i = 0; i < heightPuzzle; i++) {
            for (int k = 0; k < widthPuzzle; k++) { //如果它未贴合正确位置，或者它是，但它的位置不精确，只是一个大概值
                if (piecesPuzzle[k, i].tag != "PiezaColocada" ||
                   (piecesPuzzle[k, i].tag != "PiezaColocada"
                   && piecesPuzzle[k, i].position.x != posicionInicial[k, i].x
                   && piecesPuzzle[k, i].position.y != posicionInicial[k, i].y)) {
                    if (Vector2.Distance(piecesPuzzle[k, i].position, posicionInicial[k, i]) < margenSnap) {
                        piecesPuzzle[k, i].tag = "PiezaColocada";//表示已经贴合正确位置
                        piecesUnplaced.Remove(piecesPuzzle[k, i].gameObject);
                        piecesPuzzle[k, i].GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                        colocaAlgunaPieza = true;
                        if (piecesPuzzle[k, i].parent.name == "GrupoPiezas") {
                            Transform padreADestruir = piecesPuzzle[k, i].parent;
                            while (padreADestruir.childCount > 0) {
                                padreADestruir.GetChild(0).SetParent(padreADestruir.parent);
                            }
                            if (padreADestruir.childCount == 0) {
                                Destroy(padreADestruir.gameObject);
                            }
                        }
                        piecesPuzzle[k, i].position = new Vector3(posicionInicial[k, i].x, posicionInicial[k, i].y, 1);
                    }
                }
            }
        }
        //如果成功贴合，播放贴合音效
        if (colocaAlgunaPieza) {
            controlUI.fixFondoSFX.Play();
        }
    }

    /// <summary>
    /// 结束检测
    /// </summary>
	void CheckCompleted() {
        if (piecesUnplaced.Count == 0) {
            switch (heightPuzzle) {
                case 4: //小白难度
                    if (PlayerPrefs.GetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 0) < 1) {
                        PlayerPrefs.SetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 1);
                    }
                    break;
                case 6: //一般难度
                    if (PlayerPrefs.GetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 0) < 2) {
                        PlayerPrefs.SetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 2);
                    }
                    break;
                case 10: //中等难度
                    if (PlayerPrefs.GetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 0) < 3) {
                        PlayerPrefs.SetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 3);
                    }
                    break;
                case 14: //困难难度
                    if (PlayerPrefs.GetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 0) < 4) {
                        PlayerPrefs.SetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 4);
                    }
                    break;
                case 16: //史诗难度
                    if (PlayerPrefs.GetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 0) < 5) {
                        PlayerPrefs.SetInt("puzzleCompleto" + controlUI.puzzlePreseleccionado, 5);
                    }
                    break;
            }
            controlUI.completeSFX.Play();
            controlUI.ActivarPanelCompleto();
        }
    }

    void MakeGroupPieces(int k, int i, string dir) {
        int kk = k;
        int ii = i;
        if (dir == "left") {
            kk = kk - 1;
        }
        else if (dir == "right") {
            kk = kk + 1;
        }
        else if (dir == "up") {
            ii = ii - 1;
        }
        else if (dir == "down") {
            ii = ii + 1;
        }


        //如果两个碎片有不同的组
        if (piecesPuzzle[k, i].parent.name == "GrupoPiezas"
            && piecesPuzzle[kk, ii].parent.name == "GrupoPiezas"
            && piecesPuzzle[k, i].parent != piecesPuzzle[kk, ii].parent) {

            Transform antiguoParent = piecesPuzzle[kk, ii].parent;
            Transform[] sonsAReorganizar = antiguoParent.GetComponentsInChildren<Transform>();
            for (int j = 0; j < sonsAReorganizar.Length; j++) {
                if (sonsAReorganizar[j].name != "Sombra") {
                    sonsAReorganizar[j].SetParent(piecesPuzzle[k, i].parent);
                }
            }
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
            Destroy(antiguoParent.gameObject);
        }

        //如果只有这个碎片已经有一个组
        else if (piecesPuzzle[k, i].parent.name == "GrupoPiezas"
            && piecesPuzzle[kk, ii].parent.name != "GrupoPiezas") {

            piecesPuzzle[kk, ii].SetParent(piecesPuzzle[k, i].parent);
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
        }
        //如果另一个碎片的只有一个组
        else if (piecesPuzzle[k, i].parent.name != "GrupoPiezas"
            && piecesPuzzle[kk, ii].parent.name == "GrupoPiezas") {

            piecesPuzzle[k, i].SetParent(piecesPuzzle[kk, ii].parent);
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
        }
        //如果没有组
        else if (piecesPuzzle[k, i].parent.name != "GrupoPiezas"
            && piecesPuzzle[kk, ii].parent.name != "GrupoPiezas"
            && piecesPuzzle[k, i].tag != "PiezaColocada"
            && piecesPuzzle[kk, ii].tag != "PiezaColocada") {
            //新组
            GameObject newGroup = new GameObject();
            piecesPuzzle[k, i].SetParent(newGroup.transform);
            piecesPuzzle[kk, ii].SetParent(newGroup.transform);
            newGroup.name = "GrupoPiezas";
            newGroup.transform.SetParent(this.transform);
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
        }
    }
}
