using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图像下载模块
/// </summary>
public class DescargarImagenes : MonoBehaviour {

    public bool imageAddMode = false;
    public string addAddr = null;
    public TextAsset textURL;
    //谜题图片统一资源定位符
    [HideInInspector] public string puzzleImageURL;
    //谜题全部数量
    [HideInInspector] public int countPuzzlesTotal;
    //下载资源包
    bool downLoadPack;
    //加载连接网络
    [HideInInspector] public bool connectInternet;
    //连接图片列表
    string[] remoteImageList;
    WWW[] wwwImage;

    public WWW wwwRemoteImages;
    //同步操作
    public AsyncOperation async;
    //谜题图片列表纹理链表
    public List<Texture2D> puzzleImageList;
    public Texture diyPuzzleImage = null;
    public Sprite diyPuzzleSprite = null;

    ControlUI controlUI;
    //连接wifi
    [HideInInspector] public bool connectToWifi;
    //没有连接网络
    [HideInInspector] public bool noConnection;
    bool descargaTimedOut;

    private WWW wwwDiyImage = null;


    void Start() {
        this.imageAddMode = false;
        this.addAddr = "";
        //每次加载前清空之前的缓存图片
        //PlayerPrefs.DeleteAll ();
        controlUI = GetComponent<ControlUI>();
        countPuzzlesTotal = PlayerPrefs.GetInt("countPuzzlesTotal", 0);
        StartCoroutine(RutinaCheckInternet());

        puzzleImageURL = textURL.text;
    }

    public void loadDiyPack(string text) {
        //Debug.Log(text);
        StartCoroutine(DownloadPack(text));
    }

    IEnumerator DownloadPack(string text) {
        this.wwwDiyImage = new WWW(text);
        yield return this.wwwDiyImage;
        if (!string.IsNullOrEmpty(this.wwwDiyImage.error)) {
            Debug.Log("Download error: " + this.wwwDiyImage.error + "diyImage has not been loaded.");
            yield break;
        } else {
            if (!this.wwwDiyImage.isDone) {
                yield break;
            }
            this.diyPuzzleImage = this.wwwDiyImage.texture;
            this.diyPuzzleSprite = this.controlUI.TextureToSprite(this.diyPuzzleImage);
            //this.controlUI.testImage.GetComponent<Image>().sprite = this.diyPuzzleSprite;
            //this.controlUI.testImage.SetActive(true);
            this.controlUI.SeleccionarImagen(-1);
        }
    }

    /// <summary>
    /// 常规检查网络
    /// </summary>
    /// <returns></returns>
	IEnumerator RutinaCheckInternet() {
        while (true) {
            CheckInternet();
            yield return new WaitForSeconds(60);
        }
    }

    /// <summary>
    /// 网络连接
    /// </summary>
    /// <returns></returns>
	public bool CheckInternet() {
        //是否未连接网络
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            noConnection = true;
            connectToWifi = false;
            return false;
        }
        else {
            noConnection = false;
            //是否连接wifi
            if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) {
                connectToWifi = true;
            }
            else {
                connectToWifi = false;
            }
            //连接网络成功(不管是否为wifi,4G也行)
            return true;
        }
    }

    /// <summary>
    /// 加载图像列表
    /// </summary>
	public void CargarLista() {
        if (!downLoadPack) {
            //downLoadPack = true;
            //启动协程加载图像列表
            //StartCoroutine(DescargarLista());
            CargarPack();
        }
    }

    /// <summary>
    /// 加载资源包
    /// </summary>
	public void CargarPack() {
        if (!connectInternet) {
            connectInternet = true;
            string listaBrutaEnlaces = this.textURL.text;
            if (this.imageAddMode) {
                listaBrutaEnlaces += this.addAddr;
            }

            remoteImageList = listaBrutaEnlaces.Split(';');
            //给出图片网址时需要注意是否在最后一张图片的网址尾部也加了分号，这会造成分割字符串时的数量错误，需要处理
            //注：Trim方法是删除字符串头尾两端的空格
            if (remoteImageList[remoteImageList.Length - 1].Trim() == "") {
                List<string> listaEnlacesAux = new List<string>();
                for (int i = 0; i < remoteImageList.Length - 1; i++) {
                    listaEnlacesAux.Add(remoteImageList[i]);
                }
                remoteImageList = new string[(remoteImageList.Length - 1)];
                for (int i = 0; i < remoteImageList.Length; i++) {
                    remoteImageList[i] = listaEnlacesAux[i];
                }
            }
            wwwImage = new WWW[remoteImageList.Length];
            countPuzzlesTotal = remoteImageList.Length;
            //Unity自带的本地持久化存储方法，就和CocosCreator的localStorage操作类似,采用键值对进行储存和读取
            PlayerPrefs.SetInt("countPuzzlesTotal", countPuzzlesTotal);
            StartCoroutine(DescargarPack());
        }
        else {
            StartCoroutine(DescargarPack());
            if (downLoadPack) {
                downLoadPack = false;
            }
        }
    }

    /// <summary>
    /// 加载本地缓存图像
    /// </summary>
    /// <param name="numeroPuzzle"></param>
    /// <returns></returns>
	Texture2D CargarImagenPlayerPrefs(int numeroPuzzle) {
        string puzzleGuardado = PlayerPrefs.GetString("puzzleGuardado" + numeroPuzzle, "");
        Texture2D texturaCargada = ReadTextureFromPlayerPrefs(numeroPuzzle);
        return texturaCargada;
    }

    /// <summary>
    /// 将纹理写入本地缓存
    /// </summary>
    /// <param name="numeroPuzzle"></param>
    /// <param name="tex"></param>
	void WriteTextureToPlayerPrefs(int numeroPuzzle, Texture2D tex) {
        byte[] texByte = tex.EncodeToJPG();
        string base64Tex = System.Convert.ToBase64String(texByte);
        PlayerPrefs.SetString("puzzleGuardado" + numeroPuzzle, base64Tex);
        PlayerPrefs.Save();
        Debug.Log("Imagen " + numeroPuzzle + " guardada");
    }

    /// <summary>
    /// 在本地缓存中读取纹理
    /// </summary>
    /// <param name="numeroPuzzle"></param>
    /// <returns></returns>
	Texture2D ReadTextureFromPlayerPrefs(int numeroPuzzle) {
        string base64Tex = PlayerPrefs.GetString("puzzleGuardado" + numeroPuzzle, null);
        if (!string.IsNullOrEmpty(base64Tex)) {
            byte[] texByte = System.Convert.FromBase64String(base64Tex);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(texByte)) {
                return tex;
            }
        }
        return null;
    }

    /// <summary>
    /// 下载图像列表
    /// </summary>
    /// <returns></returns>
	IEnumerator DescargarLista() {
        wwwRemoteImages = new WWW(puzzleImageURL + "?t=" + Random.Range(0, 1000));
        StartCoroutine(LimiteTiempoDescargaLista());
        yield return wwwRemoteImages;
        CargarPack();
    }


    IEnumerator DescargarPack() {
        int cantidad = 4 + Mathf.Clamp(controlUI.packsCargados, 0, 1);

        //如果你开始离线，没有列表，这里有互联网下载
        if (!connectInternet && CheckInternet()) {
            CargarLista();
            yield break;
        }

        while (controlUI.posicionPrimero + cantidad > countPuzzlesTotal) {
            cantidad--;
            if (cantidad <= 0) {
                Debug.Log("Its trying to load more puzzles than we have.");
            }
        }

        for (int i = controlUI.posicionPrimero; i < controlUI.posicionPrimero + cantidad; i++) {
            if (i > countPuzzlesTotal - 1) {
                break;
            }
            if (CheckInternet() && PlayerPrefs.GetString("puzzleGuardado" + i, "") == "") {
                remoteImageList[i] = remoteImageList[i].Trim();
                wwwImage[i] = new WWW(remoteImageList[i]);
                StartCoroutine(LimiteTiempoDescarga(i));
                yield return wwwImage[i];
                if (descargaTimedOut || !string.IsNullOrEmpty(wwwImage[i].error)) {
                    if (!string.IsNullOrEmpty(wwwImage[i].error)) {
                        Debug.Log("Download error: " + wwwImage[i].error + ". Image " + i + " has not been loaded.");
                    }
                    else {
                        Debug.Log("It took so long to load. Image " + i + " has not been loaded.");
                    }
                    descargaTimedOut = false;
                    for (int borrarErrores = i - 1; borrarErrores >= controlUI.posicionPrimero; borrarErrores--) {
                        //按照下标删除
                        puzzleImageList.RemoveAt(borrarErrores);
                        controlUI.imagenPuzzleGrises.RemoveAt(borrarErrores);
                    }
                    controlUI.ErrorDuranteDescarga();
                    yield break;
                }

                Debug.Log("Downloading image " + i + " from that link: " + remoteImageList[i]);
                WriteTextureToPlayerPrefs(i, wwwImage[i].texture);
                puzzleImageList.Add(wwwImage[i].texture);
                controlUI.TexturaABoton(i, puzzleImageList[i]);
            }
            else {
                if (PlayerPrefs.GetString("puzzleGuardado" + i, "") != "") {
                    puzzleImageList.Add(CargarImagenPlayerPrefs(i));

                    Debug.Log("Image " + i + " loaded from prefs.");
                    controlUI.TexturaABoton(i, puzzleImageList[i]);
                }
                else {
                    Debug.Log("There is no internet nor a saved image. This should never happen.");
                }
            }
            yield return null;
        }

        controlUI.ActivarAnimBoton();
    }

    /// <summary>
    /// 超时下载处理
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
	IEnumerator LimiteTiempoDescarga(int i) {
        float timer = 0;
        float timeOut = 10f; //置定一张贴图的加载时间(超时时间)
        descargaTimedOut = false;

        while (!wwwImage[i].isDone) {
            if (timer > timeOut) {
                //纹理资源加载超时
                descargaTimedOut = true;
                //销毁，垃圾回收
                wwwImage[i].Dispose();
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 超时下载图像列表处理
    /// </summary>
    /// <returns></returns>
	IEnumerator LimiteTiempoDescargaLista() {
        float timer = 0;
        float timeOut = 10f; //下载图像的最大时间
        descargaTimedOut = false;

        while (!wwwRemoteImages.isDone) {
            if (timer > timeOut) {
                descargaTimedOut = true;
                //超时，终止加载
                wwwRemoteImages.Dispose();
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (descargaTimedOut) {
            controlUI.ErrorDuranteDescarga();
        }
        yield return null;
    }
}
