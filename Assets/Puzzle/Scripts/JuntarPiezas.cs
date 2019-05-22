using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 组合型碎片
/// </summary>
public class JuntarPiezas : MonoBehaviour {
    /// <summary>
    /// 拼图碎片位置信息
    /// </summary>
	public Transform[,] piezasPuzzle;
    /// <summary>
    /// 未放置的拼图碎片
    /// </summary>
	public List<GameObject> piezasSinColocar;

    /// <summary>
    /// 余地间隔
    /// </summary>
	float margenSnap;
    /// <summary>
    /// 碎片宽
    /// </summary>
	int anchoPuzzle;
    /// <summary>
    /// 碎片高
    /// </summary>
	int altoPuzzle;
    /// <summary>
    /// 初始位置
    /// </summary>
	public Vector3[,] posicionInicial;
    GameObject objetoControlador;
    MoverPiezas moverPiezas;
    ControlUI controlUI;

    //碎片之间的距离
    public struct DistanciaEntrePiezas {
        public float up;//上部
        public float down;//下部
        public float right;//右部
        public float left;//左部
    }
    DistanciaEntrePiezas[,] distanciaEntrePiezas;


    void Start() {
        //获取主客户端游戏对象
        objetoControlador = GameObject.Find("Controlador");
        //获取碎片控制组件
        moverPiezas = objetoControlador.GetComponent<MoverPiezas>();
        //获取主客户端控制器(界面UI控制器)
        controlUI = objetoControlador.GetComponent<ControlUI>();
        //未放置的拼图碎片
        piezasSinColocar = new List<GameObject>();
        //拼图碎片尺寸(NXN?)
        string dimensionesPuzzle = this.gameObject.name.Substring(5, this.gameObject.name.Length - 5);
        string[] dosDimensiones = dimensionesPuzzle.Split('x');
        anchoPuzzle = int.Parse(dosDimensiones[0]);
        altoPuzzle = int.Parse(dosDimensiones[1]);
        //注意矩阵数组和锯齿数组的区别
        piezasPuzzle = new Transform[anchoPuzzle, altoPuzzle];
        posicionInicial = new Vector3[anchoPuzzle, altoPuzzle];
        distanciaEntrePiezas = new DistanciaEntrePiezas[anchoPuzzle, altoPuzzle];
        //间隔？？？
        margenSnap = 0.2f - (anchoPuzzle * 0.01f); //它从0.16（4X4的谜题）到0.10（10X10的谜题）

        for (int i = 0; i < altoPuzzle; i++) {
            for (int k = 0; k < anchoPuzzle; k++) {
                piezasPuzzle[k, i] = transform.GetChild(i * anchoPuzzle + k);
                posicionInicial[k, i] = piezasPuzzle[k, i].position;
                piezasSinColocar.Add(piezasPuzzle[k, i].gameObject);
            }
        }
        for (int i = 0; i < altoPuzzle; i++) {
            for (int k = 0; k < anchoPuzzle; k++) {
                if (k > 0) {
                    distanciaEntrePiezas[k, i].left = Mathf.Abs(piezasPuzzle[k, i].position.x - piezasPuzzle[k - 1, i].position.x);
                }
                if (k < anchoPuzzle - 1) {
                    distanciaEntrePiezas[k, i].right = Mathf.Abs(piezasPuzzle[k + 1, i].position.x - piezasPuzzle[k, i].position.x);
                }
                if (i > 0) {
                    distanciaEntrePiezas[k, i].up = Mathf.Abs(piezasPuzzle[k, i - 1].position.y - piezasPuzzle[k, i].position.y);
                }
                if (i < altoPuzzle - 1) {
                    distanciaEntrePiezas[k, i].down = Mathf.Abs(piezasPuzzle[k, i].position.y - piezasPuzzle[k, i + 1].position.y);
                }
            }
        }
    }

    /// <summary>
    /// 重置碎片
    /// </summary>
    /// <param name="k"></param>
    /// <param name="i"></param>
	public void ReajustarPiezas(int k, int i) {
        bool tieneIzquierda = false;
        bool tieneDerecha = false;
        bool tieneSuperior = false;
        bool tieneInferior = false;

        if (k > 0 && distanciaEntrePiezas[k, i].left <= Mathf.Abs(piezasPuzzle[k, i].position.x - piezasPuzzle[k - 1, i].position.x) + margenSnap
             && distanciaEntrePiezas[k, i].left >= Mathf.Abs(piezasPuzzle[k, i].position.x - piezasPuzzle[k - 1, i].position.x) - margenSnap
             && piezasPuzzle[k, i].position.y <= piezasPuzzle[k - 1, i].position.y + margenSnap
             && piezasPuzzle[k, i].position.y >= piezasPuzzle[k - 1, i].position.y - margenSnap
             && piezasPuzzle[k, i].position.x > piezasPuzzle[k - 1, i].position.x) {
            tieneIzquierda = true;

            Vector3 posicionAnterior = piezasPuzzle[k, i].transform.position;
            piezasPuzzle[k, i].transform.position = new Vector3(piezasPuzzle[k - 1, i].position.x + distanciaEntrePiezas[k - 1, i].right, piezasPuzzle[k - 1, i].position.y, piezasPuzzle[k - 1, i].position.z);
            Vector3 posicionActual = piezasPuzzle[k, i].transform.position;

            if (piezasPuzzle[k, i].parent.name == "GrupoPiezas") {
                piezasPuzzle[k, i].transform.position = posicionAnterior;
                piezasPuzzle[k, i].parent.transform.position += posicionActual - posicionAnterior;
            }

            AgruparPiezas(k, i, "left");
        }
        if (k < anchoPuzzle - 1 && distanciaEntrePiezas[k, i].right <= Mathf.Abs(piezasPuzzle[k + 1, i].position.x - piezasPuzzle[k, i].position.x) + margenSnap
             && distanciaEntrePiezas[k, i].right >= Mathf.Abs(piezasPuzzle[k + 1, i].position.x - piezasPuzzle[k, i].position.x) - margenSnap
             && piezasPuzzle[k, i].position.y <= piezasPuzzle[k + 1, i].position.y + margenSnap
             && piezasPuzzle[k, i].position.y >= piezasPuzzle[k + 1, i].position.y - margenSnap
             && piezasPuzzle[k, i].position.x < piezasPuzzle[k + 1, i].position.x) {
            tieneDerecha = true;

            Vector3 posicionAnterior = piezasPuzzle[k, i].transform.position;
            piezasPuzzle[k, i].transform.position = new Vector3(piezasPuzzle[k + 1, i].position.x - distanciaEntrePiezas[k + 1, i].left, piezasPuzzle[k + 1, i].position.y, piezasPuzzle[k + 1, i].position.z);
            Vector3 posicionActual = piezasPuzzle[k, i].transform.position;

            if (piezasPuzzle[k, i].parent.name == "GrupoPiezas") {
                piezasPuzzle[k, i].transform.position = posicionAnterior;
                piezasPuzzle[k, i].parent.transform.position += posicionActual - posicionAnterior;
            }
            AgruparPiezas(k, i, "right");
        }
        if (i > 0 && distanciaEntrePiezas[k, i].up <= Mathf.Abs(piezasPuzzle[k, i - 1].position.y - piezasPuzzle[k, i].position.y) + margenSnap
             && distanciaEntrePiezas[k, i].up >= Mathf.Abs(piezasPuzzle[k, i - 1].position.y - piezasPuzzle[k, i].position.y) - margenSnap
             && piezasPuzzle[k, i].position.x <= piezasPuzzle[k, i - 1].position.x + margenSnap
             && piezasPuzzle[k, i].position.x >= piezasPuzzle[k, i - 1].position.x - margenSnap
             && piezasPuzzle[k, i].position.y < piezasPuzzle[k, i - 1].position.y) {
            tieneSuperior = true;

            Vector3 posicionAnterior = piezasPuzzle[k, i].transform.position;
            piezasPuzzle[k, i].transform.position = new Vector3(piezasPuzzle[k, i - 1].position.x, piezasPuzzle[k, i - 1].position.y - distanciaEntrePiezas[k, i - 1].down, piezasPuzzle[k, i - 1].position.z);
            Vector3 posicionActual = piezasPuzzle[k, i].transform.position;

            if (piezasPuzzle[k, i].parent.name == "GrupoPiezas") {
                piezasPuzzle[k, i].transform.position = posicionAnterior;
                piezasPuzzle[k, i].parent.transform.position += posicionActual - posicionAnterior;
            }
            AgruparPiezas(k, i, "up");
        }
        if (i < altoPuzzle - 1 && distanciaEntrePiezas[k, i].down <= Mathf.Abs(piezasPuzzle[k, i].position.y - piezasPuzzle[k, i + 1].position.y) + margenSnap
             && distanciaEntrePiezas[k, i].down >= Mathf.Abs(piezasPuzzle[k, i].position.y - piezasPuzzle[k, i + 1].position.y) - margenSnap
             && piezasPuzzle[k, i].position.x <= piezasPuzzle[k, i + 1].position.x + margenSnap
             && piezasPuzzle[k, i].position.x >= piezasPuzzle[k, i + 1].position.x - margenSnap
             && piezasPuzzle[k, i].position.y > piezasPuzzle[k, i + 1].position.y) {
            tieneInferior = true;

            Vector3 posicionAnterior = piezasPuzzle[k, i].transform.position;
            piezasPuzzle[k, i].transform.position = new Vector3(piezasPuzzle[k, i + 1].position.x, piezasPuzzle[k, i + 1].position.y + distanciaEntrePiezas[k, i + 1].up, piezasPuzzle[k, i + 1].position.z);
            Vector3 posicionActual = piezasPuzzle[k, i].transform.position;

            if (piezasPuzzle[k, i].parent.name == "GrupoPiezas") {
                piezasPuzzle[k, i].transform.position = posicionAnterior;
                piezasPuzzle[k, i].parent.transform.position += posicionActual - posicionAnterior;
            }
            AgruparPiezas(k, i, "down");
        }
        AjustarPiezaAlLienzo();
        CheckearCompleted();
    }

    /// <summary>
    /// 调整碎片位置
    /// </summary>
	void AjustarPiezaAlLienzo() {
        //成功贴合一个碎片
        bool colocaAlgunaPieza = false;
        for (int i = 0; i < altoPuzzle; i++) {
            for (int k = 0; k < anchoPuzzle; k++) { //如果它未贴合正确位置，或者它是，但它的位置很差
                if (piezasPuzzle[k, i].tag != "PiezaColocada" ||
                   (piezasPuzzle[k, i].tag != "PiezaColocada"
                   && piezasPuzzle[k, i].position.x != posicionInicial[k, i].x
                   && piezasPuzzle[k, i].position.y != posicionInicial[k, i].y)) {
                    if (Vector2.Distance(piezasPuzzle[k, i].position, posicionInicial[k, i]) < margenSnap) {
                        piezasPuzzle[k, i].tag = "PiezaColocada";//表示已经贴合正确位置
                        piezasSinColocar.Remove(piezasPuzzle[k, i].gameObject);
                        piezasPuzzle[k, i].GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                        colocaAlgunaPieza = true;
                        if (piezasPuzzle[k, i].parent.name == "GrupoPiezas") {
                            Transform padreADestruir = piezasPuzzle[k, i].parent;
                            while (padreADestruir.childCount > 0) {
                                padreADestruir.GetChild(0).SetParent(padreADestruir.parent);
                            }
                            if (padreADestruir.childCount == 0) {
                                Destroy(padreADestruir.gameObject);
                            }
                        }
                        piezasPuzzle[k, i].position = new Vector3(posicionInicial[k, i].x, posicionInicial[k, i].y, 1);
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
    /// 难度标记
    /// </summary>
	void CheckearCompleted() {
        if (piezasSinColocar.Count == 0) {
            switch (altoPuzzle) {
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

    void AgruparPiezas(int k, int i, string tieneLado) {
        int kk = k;
        int ii = i;
        if (tieneLado == "left") {
            kk = kk - 1;
        }
        else if (tieneLado == "right") {
            kk = kk + 1;
        }
        else if (tieneLado == "up") {
            ii = ii - 1;
        }
        else if (tieneLado == "down") {
            ii = ii + 1;
        }


        //如果两个贴图有不同的群体
        if (piezasPuzzle[k, i].parent.name == "GrupoPiezas"
            && piezasPuzzle[kk, ii].parent.name == "GrupoPiezas"
            && piezasPuzzle[k, i].parent != piezasPuzzle[kk, ii].parent) {

            Transform antiguoParent = piezasPuzzle[kk, ii].parent;
            Transform[] hijosAReorganizar = antiguoParent.GetComponentsInChildren<Transform>();
            for (int j = 0; j < hijosAReorganizar.Length; j++) {
                if (hijosAReorganizar[j].name != "Sombra") {
                    hijosAReorganizar[j].SetParent(piezasPuzzle[k, i].parent);
                }
            }
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
            Destroy(antiguoParent.gameObject);
        }

        //如果只有这个人有一个组
        else if (piezasPuzzle[k, i].parent.name == "GrupoPiezas"
            && piezasPuzzle[kk, ii].parent.name != "GrupoPiezas") {

            piezasPuzzle[kk, ii].SetParent(piezasPuzzle[k, i].parent);
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
        }
        //如果其他的只有一个组
        else if (piezasPuzzle[k, i].parent.name != "GrupoPiezas"
            && piezasPuzzle[kk, ii].parent.name == "GrupoPiezas") {

            piezasPuzzle[k, i].SetParent(piezasPuzzle[kk, ii].parent);
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
        }
        //如果没有组
        else if (piezasPuzzle[k, i].parent.name != "GrupoPiezas"
            && piezasPuzzle[kk, ii].parent.name != "GrupoPiezas"
            && piezasPuzzle[k, i].tag != "PiezaColocada"
            && piezasPuzzle[kk, ii].tag != "PiezaColocada") {
            //新组
            GameObject nuevoGrupo = new GameObject();
            piezasPuzzle[k, i].SetParent(nuevoGrupo.transform);
            piezasPuzzle[kk, ii].SetParent(nuevoGrupo.transform);
            nuevoGrupo.name = "GrupoPiezas";
            nuevoGrupo.transform.SetParent(this.transform);
            if (!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play();
        }
    }
}
