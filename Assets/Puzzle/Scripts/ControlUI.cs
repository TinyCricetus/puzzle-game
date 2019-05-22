using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
#if PLATFORM_ANDROID
using UnityEngine.Advertisements;
#endif

public class ControlUI : MonoBehaviour {

    DescargarImagenes descargarImagenes;
    Analiticas analiticas;

    public GameObject testImage = null;
    public bool diyMode = false;

    public GameObject panelInit;
    public GameObject panelSelection;
    public GameObject panelPreGame;
    public GameObject panelInGame;
    public GameObject panelComplete;

    public GameObject[] botonContinue;
    public GameObject[] packBotones;
    public GameObject diyImageButton = null;
    public Image[] imagenBoton;
    Image[] contornoBoton;
    public List<Texture2D> imagenPuzzleGrises;
    Color[] colorDificultad;

    public Button botonPlay;
    public int packsCargados = 0;
    public int posicionPrimero = 0;
    /// <summary>
    /// 加载新图包
    /// </summary>
    bool cargandoNuevoPack;
    public RectTransform panelScroll;
    ScrollRect panelScrollComponent;
    public Image[] pedazoVistaPrevia;
    public Image[] tickDificultad;
    int ultimaDificultadSeleccionada = 0;
    public int puzzlePreseleccionado;

    bool mensajeCheckeoWifi;
    public Image noticeDownload;
    public Image noticeConnection;
    public Image noticeConnectionInGame;
    public Image noticeTutorial;

    bool yaPidioAyudaBackground;
    bool yaPidioSepararPiezas;


    bool ayudaActivada;
    public SpriteRenderer bgAyuda;

    bool menuIngameActivado;
    public RectTransform flechitaMenuInGame;
    public Button botonMenuInvisible;

    public GameObject seguroMenu;
    int ultimoNumImagen = -2;

    Sprite spritePreseleccionado;
    public Image miniaturaDeAyuda;

    public Text completeInfo;
    float horaInicio;
    int tiempoEnPuzzle;

    float contadorTiempoAnuncio = 0;
    float tiempoParaAnuncio = 480; //8 minutos
    bool puedeMostrarAnuncio;
    bool puedeMostrarAnuncioExtra;

    public GameObject[] prefabsPuzzle;
    GameObject nuevoPuzzle;

    public GameObject tickSort;
    public GameObject tickGuia;
    public GameObject notaTachada;

    Sprite[] imagenesAColor;
    Sprite[] imagenesEnGris;

    Texture diyImageTexture = null;
    Sprite diyImageColor = null;
    Sprite diyImageGray = null;


    [Header("SFX")]
    public AudioSource clickSFX;
    public AudioSource completeSFX;
    public AudioSource pauseSFX;
    public AudioSource barajarSFX;
    public AudioSource fixFondoSFX;
    public AudioSource fixGrupoSFX;
    public AudioSource errorSFX;
    public AudioSource desplegarSFX;

    void Start() {
        descargarImagenes = GetComponent<DescargarImagenes>();
        analiticas = GetComponent<Analiticas>();
        panelScrollComponent = panelScroll.GetComponent<ScrollRect>();
        contornoBoton = new Image[imagenBoton.Length];
        for (int i = 0; i < imagenBoton.Length; i++) {
            contornoBoton[i] = imagenBoton[i].transform.parent.parent.GetComponent<Image>();
        }

        imagenesAColor = new Sprite[imagenBoton.Length];
        imagenesEnGris = new Sprite[imagenBoton.Length];

        colorDificultad = new Color[6];
        colorDificultad[0] = new Color(0.93f, 0.93f, 0.93f, 1); //Gris
        colorDificultad[1] = new Color(0.73f, 1, 0.79f, 1); //Verde 
        colorDificultad[2] = new Color(1, 1, 0.73f, 1); //Amarillo 
        colorDificultad[3] = new Color(1, 0.87f, 0.73f, 1); //Naranja 
        colorDificultad[4] = new Color(1, 0.73f, 0.73f, 1); //Rojo
        colorDificultad[5] = new Color(0.87f, 0.73f, 1, 1); //Morado 
    }

    void Update() {
        contadorTiempoAnuncio += Time.deltaTime;
        if (contadorTiempoAnuncio >= tiempoParaAnuncio * 2 && !puedeMostrarAnuncioExtra) {
            puedeMostrarAnuncioExtra = true;
        }
        else if (contadorTiempoAnuncio >= tiempoParaAnuncio && !puedeMostrarAnuncio) {
            puedeMostrarAnuncio = true;
        }
    }

    void AjustarNuevoPack() {
        Debug.Log("There is a total of " + descargarImagenes.countPuzzlesTotal + " images");
        if (packsCargados == 1) {
            for (int i = 0; i < packBotones[packsCargados - 1].transform.childCount; i++) {
                if (i + 1 > descargarImagenes.countPuzzlesTotal) {
                    packBotones[0].transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        else if (packsCargados == 2) {
            for (int i = 0; i < packBotones[packsCargados - 1].transform.childCount; i++) {
                if (i + 1 + 4/*第一包中的4个*/ > descargarImagenes.countPuzzlesTotal) {
                    packBotones[packsCargados - 1].transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        else if (packsCargados >= 3) {
            for (int i = 0; i < packBotones[packsCargados - 1].transform.childCount; i++) {
                if (i + 1 + 4 + (5 * (packsCargados - 2))/*以下5个包，-2因为我们已经在第3包中*/ > descargarImagenes.countPuzzlesTotal) {
                    packBotones[packsCargados - 1].transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        AjustarLimitesScrollVertical();
    }

    void AjustarLimitesScrollVertical() {
        if (packsCargados >= 2 && panelScrollComponent.enabled == false) { //通过从第二个包中滚动来激活移动
            panelScrollComponent.enabled = true;
        }
        panelScroll.offsetMin = new Vector2(0, panelScroll.offsetMax.y + (-140 * (packsCargados - 1) - 20 * (packsCargados - 2))); //校准以留下余量
    }

    /// <summary>
    /// 选择文件按钮响应事件
    /// </summary>
    public void OnClickDiyButton() {
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "图片(jpg|png格式)\0*.jpg;*.png";
        //用于储存文件的所在路径,便于加载资源(注意格式筛选)
        openFileName.file = new string(new char[256]);
        //最长文件路径大小
        openFileName.maxFile = openFileName.file.Length;
        //用于储存文件名
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        openFileName.title = "选择自定义图片";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetOpenFileName(openFileName)) {
            //选取文件成功后进行的操作
            //Debug.Log(openFileName.file);
            //Debug.Log(openFileName.fileTitle);
            this.descargarImagenes.loadDiyPack("file://" + openFileName.file);
        }
    }

    //触发关卡选择功能(主界面PLAY响应事件)
    public void CargarMasPuzzles() {
        //始放关卡加载声音
        clickSFX.Play();
        //显示自定义图品图片按钮
        this.diyImageButton.SetActive(true);

        if (!cargandoNuevoPack) {
            //对于0包
            if (packsCargados == 0) {
                botonPlay.interactable = false;
                if (!descargarImagenes.CheckInternet()) {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 4; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + i, "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }
                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        descargarImagenes.CargarPack();
                        botonPlay.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                    else {
                        Debug.Log("您没有保存整个包");
                        errorSFX.Play();
                        ErrorDuranteDescarga();
                    }
                }
                else {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 4; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + i, "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }

                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        descargarImagenes.CargarPack();
                        botonPlay.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                    else if (!descargarImagenes.connectToWifi && !mensajeCheckeoWifi) {
                        errorSFX.Play();
                        noticeDownload.gameObject.SetActive(true);
                    }
                    else if (!descargarImagenes.connectInternet) {
                        cargandoNuevoPack = true;
                        descargarImagenes.CargarLista();
                        botonPlay.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                    else {
                        cargandoNuevoPack = true;
                        descargarImagenes.CargarPack();
                        botonPlay.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                }
            }
            else { //对于剩下的包
                if (!descargarImagenes.CheckInternet()) {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 5; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + (i - 1 + packsCargados * 5), "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }
                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        descargarImagenes.CargarPack();
                        botonContinue[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                    else {
                        Debug.Log("您没有保存整个包");
                        errorSFX.Play();
                        ErrorDuranteDescarga();
                    }
                }
                else {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 5; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + (i - 1 + packsCargados * 5), "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }
                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        descargarImagenes.CargarPack();
                        botonContinue[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                    else if (!descargarImagenes.connectToWifi && !mensajeCheckeoWifi) {
                        errorSFX.Play();
                        noticeDownload.gameObject.SetActive(true);
                    }
                    else {
                        cargandoNuevoPack = true;
                        descargarImagenes.CargarPack();
                        botonContinue[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                }
            }
        }
    }

    public void AceptarDescargaWifi() {
        mensajeCheckeoWifi = true;
        if (packsCargados == 0) {
            if (!descargarImagenes.connectInternet) {
                cargandoNuevoPack = true;
                descargarImagenes.CargarLista();
                botonPlay.transform.GetChild(0).GetComponent<Animator>().enabled = true;
            }
            else {
                cargandoNuevoPack = true;
                descargarImagenes.CargarPack();
                botonPlay.transform.GetChild(0).GetComponent<Animator>().enabled = true;
            }
        }
        else {
            cargandoNuevoPack = true;
            descargarImagenes.CargarPack();
            botonContinue[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
        }
        clickSFX.Play();
        noticeDownload.gameObject.SetActive(false);
    }

    public void CancelarDescargaWifi() {
        cargandoNuevoPack = false;
        if (packsCargados > 0) {
            botonContinue[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = false;
            botonContinue[packsCargados - 1].transform.GetChild(0).GetChild(0).GetComponent<Text>().enabled = true;
            botonContinue[packsCargados - 1].transform.GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
        }
        else if (packsCargados == 0) {
            DevuelveBotonPlay();
            botonPlay.interactable = true;
        }
        clickSFX.Play();
        noticeDownload.gameObject.SetActive(false);
    }

    public void ActivarAnimBoton() {
        Invoke("MostrarNuevoPack", 0.2f);
    }

    void MostrarNuevoPack() {
        desplegarSFX.Play();
        cargandoNuevoPack = false; //包已经加载
        if (packsCargados > 0) { //禁用上一个按钮
            botonContinue[packsCargados - 1].SetActive(false);
        }
        packBotones[packsCargados].SetActive(true);
        posicionPrimero = posicionPrimero + 4 + Mathf.Clamp(packsCargados, 0, 1);
        packsCargados++;
        AjustarNuevoPack();
        if (packsCargados == 1) { //当您已装入第二个包时,在第一个按钮上重写PLAY
            DevuelveBotonPlay();
        }
    }

    /// <summary>
    /// 精灵按钮化
    /// </summary>
    /// <param name="numeroBoton"></param>
    /// <param name="imagenTex"></param>
	public void TexturaABoton(int numeroBoton, Texture imagenTex) {
        Texture texImagenBoton = Instantiate(imagenTex) as Texture;
        Sprite spriteImagen = ConvertirASprite(numeroBoton, texImagenBoton);
        imagenBoton[numeroBoton].sprite = spriteImagen;
        contornoBoton[numeroBoton].color = colorDificultad[PlayerPrefs.GetInt("puzzleCompleto" + numeroBoton, 0)];
    }

    /// <summary>
    /// 将纹理转为精灵(图片)
    /// </summary>
    /// <param name="numeroBoton"></param>
    /// <param name="imagenTex"></param>
    /// <returns></returns>
    Sprite ConvertirASprite(int numeroBoton, Texture imagenTex) {
        Texture2D imagenTex2D = imagenTex as Texture2D;
        if (PlayerPrefs.GetInt("puzzleCompleto" + numeroBoton, 0) < 5) {
            imagenTex2D = ConvertToGrayscale(imagenTex2D);
        }
        imagenPuzzleGrises.Add(imagenTex2D);
        Rect rect = new Rect(0, 0, imagenTex2D.width, imagenTex2D.height);
        return Sprite.Create(imagenTex2D, rect, Vector2.zero, imagenTex2D.width);
    }

    public Sprite TextureToSprite(Texture imageTexture) {
        Texture2D imageTexture2D = imageTexture as Texture2D;
        Debug.Log(imageTexture2D);
        Rect rect = new Rect(0, 0, imageTexture2D.width, imageTexture2D.height);
        return Sprite.Create(imageTexture2D, rect, Vector2.zero, imageTexture2D.width);
    }

    /// <summary>
    /// 正常转精灵(彩色)
    /// </summary>
    /// <param name="imagenTex">图像纹理</param>
    /// <returns></returns>
	Sprite ConversionRapidaASprite(Texture imagenTex) {
        Texture2D imagenTex2D = imagenTex as Texture2D;
        Rect rect = new Rect(0, 0, imagenTex2D.width, imagenTex2D.height);
        return Sprite.Create(imagenTex2D, rect, Vector2.zero, imagenTex2D.width);
    }

    /// <summary>
    /// 转为灰度图
    /// </summary>
    /// <param name="imagenTex2D"></param>
    /// <returns></returns>
	Texture2D ConvertToGrayscale(Texture2D imagenTex2D) {
        float lightness = 0.2f;
        Color32[] pixels = imagenTex2D.GetPixels32();
        for (int x = 0; x < imagenTex2D.width; x++) {
            for (int y = 0; y < imagenTex2D.height; y++) {
                Color32 pixel = pixels[x + y * imagenTex2D.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                Color c = new Color(l + lightness, l + lightness, l + lightness, 1); //
                imagenTex2D.SetPixel(x, y, c);
            }
        }
        imagenTex2D.Apply(true);
        return imagenTex2D;
    }

    public void CerrarAvisoConectarse() {
        clickSFX.Play();
        noticeConnection.gameObject.SetActive(false);
        noticeConnectionInGame.gameObject.SetActive(false);
    }

    /// <summary>
    /// 激活引导
    /// </summary>
	void ActivarAvisoTutorial() {
        if (panelInGame.activeSelf) {
            noticeTutorial.gameObject.SetActive(true);
        }
    }

    public void CerrarAvisoTutorialAbriendoMenuInGame() {
        CerrarAvisoTutorial();
        ActivarMenuInGame();
    }

    /// <summary>
    /// 启动游戏中的向导菜单
    /// </summary>
    public void CerrarAvisoTutorial() {
        clickSFX.Play();
        PlayerPrefs.SetInt("yaUsoBotonSort", 1);
        noticeTutorial.gameObject.SetActive(false);
    }

    /// <summary>
    /// 加载错误
    /// </summary>
    public void ErrorDuranteDescarga() {
        cargandoNuevoPack = false;
        if (packsCargados > 0) {
            botonContinue[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = false;
            botonContinue[packsCargados - 1].transform.GetChild(0).GetChild(0).GetComponent<Text>().enabled = true;
            botonContinue[packsCargados - 1].transform.GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
        }
        else if (packsCargados == 0) {
            DevuelveBotonPlay();
            botonPlay.interactable = true;
        }
        errorSFX.Play();
        noticeConnection.gameObject.SetActive(true);
    }

    void DevuelveBotonPlay() {
        botonPlay.transform.GetChild(0).GetComponent<Animator>().enabled = false;
        botonPlay.transform.GetChild(0).GetChild(0).GetComponent<Text>().enabled = true;
        botonPlay.transform.GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
    }

    public void VolverAMenu() {
#if PLATFORM_ANDROID
        if (puedeMostrarAnuncioExtra && Advertisement.IsReady("rewardedVideo")) {
            contadorTiempoAnuncio = 0;
            puedeMostrarAnuncioExtra = false;
            puedeMostrarAnuncio = false;
            ShowRewardedAdExtra();
        }
        else if (puedeMostrarAnuncio && Advertisement.IsReady()) {
            contadorTiempoAnuncio = 0;
            puedeMostrarAnuncioExtra = false;
            puedeMostrarAnuncio = false;
            ShowAd();
        }
#endif
        clickSFX.Play();
        panelInit.SetActive(true);
        panelSelection.SetActive(false);
        panelPreGame.SetActive(false);
        panelInGame.SetActive(false);
        panelComplete.SetActive(false);
    }


    public void SeleccionarImagen(int numImagen) {
        clickSFX.Play();

        int piezasColoreadas = 0;
        int dificultadCompletada = CargarSaveDificultades(numImagen);
        if (numImagen == -1) {
            dificultadCompletada = 1;
            this.diyMode = true;//自定义图片
        }
        else {
            this.diyMode = false;//非自定义图片
        }

        switch (dificultadCompletada) {
            case 1:
                piezasColoreadas = 2;
                break;
            case 2:
                piezasColoreadas = 5;
                break;
            case 3:
                piezasColoreadas = 7;
                break;
            case 4:
                piezasColoreadas = 10;
                break;
            case 5:
                piezasColoreadas = 12;
                break;
            default:
                break;
        }

        if (ultimoNumImagen != numImagen) {
            //Debug.Log("excute here");
            puzzlePreseleccionado = numImagen;
            if (this.diyMode) {
                Texture tex = this.descargarImagenes.diyPuzzleImage;
                spritePreseleccionado = ConversionRapidaASprite(tex);

                Texture2D texturaColor = Instantiate(tex) as Texture2D;
                Texture2D texturaGrises = Instantiate(tex) as Texture2D;

                Sprite imagenAColor = ConversionRapidaASprite(texturaColor);
                Sprite imagenEnGris = ConversionRapidaASprite(ConvertToGrayscale(texturaGrises));
                diyImageColor = imagenAColor;
                diyImageGray = imagenEnGris;

            }
            else {
                spritePreseleccionado = ConversionRapidaASprite(descargarImagenes.puzzleImageList[puzzlePreseleccionado]);

                if (imagenesAColor[numImagen] == null || imagenesEnGris[numImagen] == null) {
                    Texture2D texturaColor = Instantiate(descargarImagenes.puzzleImageList[numImagen]) as Texture2D;
                    Texture2D texturaGrises = Instantiate(descargarImagenes.puzzleImageList[numImagen]) as Texture2D;

                    Sprite imagenAColor = ConversionRapidaASprite(texturaColor);
                    Sprite imagenEnGris = ConversionRapidaASprite(ConvertToGrayscale(texturaGrises));
                    imagenesAColor[numImagen] = imagenAColor;
                    imagenesEnGris[numImagen] = imagenEnGris;
                }
            }
        }

        if (this.diyMode) {

            for (int i = 0; i < pedazoVistaPrevia.Length; i++) {
                if (i < piezasColoreadas) {
                    pedazoVistaPrevia[i].sprite = diyImageColor;
                }
                else {
                    pedazoVistaPrevia[i].sprite = diyImageGray;
                }
            }
        }
        else {
            for (int i = 0; i < pedazoVistaPrevia.Length; i++) {
                if (i < piezasColoreadas) {
                    pedazoVistaPrevia[i].sprite = imagenesAColor[numImagen];
                }
                else {
                    pedazoVistaPrevia[i].sprite = imagenesEnGris[numImagen];
                }
            }
        }


        panelInit.SetActive(false);
        panelSelection.SetActive(true);
        panelPreGame.SetActive(false);
        panelInGame.SetActive(false);
        panelComplete.SetActive(false);

        ultimoNumImagen = numImagen;
    }

    int CargarSaveDificultades(int numImagen) {
        int dificultadCompletada = PlayerPrefs.GetInt("puzzleCompleto" + numImagen, 0);
        for (int i = 0; i < tickDificultad.Length; i++) {
            if (i < dificultadCompletada) {
                tickDificultad[i].enabled = true;
            }
            else {
                tickDificultad[i].enabled = false;
            }
        }
        return dificultadCompletada;
    }

    /// <summary>
    /// 初始化拼图碎片(参数由按钮传递)
    /// </summary>
    /// <param name="dificultadSeleccionada">按钮传递参数</param>
    public void IniciarPuzzle(int dificultadSeleccionada) {
        Debug.Log("现在的难度等级为" + dificultadSeleccionada);
        clickSFX.Play();
        ultimaDificultadSeleccionada = dificultadSeleccionada;
        Vector3 posicionPuzzle = Vector3.zero;
        switch (dificultadSeleccionada) {
            case 0:
                posicionPuzzle = new Vector3(0.2f, -0.5f, 0);
                break;
            case 1:
                posicionPuzzle = new Vector3(0.2f, -0.5f, 0);
                break;
            case 2:
                posicionPuzzle = Vector3.zero;
                break;
            case 3:
                posicionPuzzle = Vector3.zero;
                break;
            case 4:
                posicionPuzzle = Vector3.zero;
                break;
        }

        GameObject nuevoPuzzle = Instantiate(prefabsPuzzle[dificultadSeleccionada], posicionPuzzle, prefabsPuzzle[dificultadSeleccionada].transform.rotation) as GameObject;
        nuevoPuzzle.name = prefabsPuzzle[dificultadSeleccionada].name; //我们从名称中删除“（克隆）”
        this.nuevoPuzzle = nuevoPuzzle;
        panelInit.SetActive(false);
        panelSelection.SetActive(false);
        panelPreGame.SetActive(true);
        panelInGame.SetActive(false);
        panelComplete.SetActive(false);
        CargarTextura();
        PonerMiniaturaDeAyuda();
    }

    /// <summary>
    /// 这里就是把图片碎片化
    /// </summary>
    void CargarTextura() {
        GameObject[] piezasPuzzle = GameObject.FindGameObjectsWithTag("PiezaPuzzle");
        if (this.diyMode) {
            for (int i = 0; i < piezasPuzzle.Length; i++) {
                piezasPuzzle[i].GetComponent<Renderer>().material.mainTexture = this.descargarImagenes.diyPuzzleImage;
            }
        }
        else {
            for (int i = 0; i < piezasPuzzle.Length; i++) {
                piezasPuzzle[i].GetComponent<Renderer>().material.mainTexture = descargarImagenes.puzzleImageList[puzzlePreseleccionado];
            }
        }
    }

    public void StartPuzzle() {
        //获取碎片控制组件
        MoverPiezas moverPiezas = this.gameObject.GetComponent<MoverPiezas>();
        moverPiezas.juntarPiezas = GameObject.FindGameObjectWithTag("MatrizPuzzle").GetComponent<JuntarPiezas>();
        DesactivarAyudaBG();
        DesordenarPiezas(nuevoPuzzle.transform); //随机碎片
        horaInicio = Time.time;
        panelInit.SetActive(false);
        panelSelection.SetActive(false);
        panelPreGame.SetActive(false);
        panelInGame.SetActive(true);
        panelComplete.SetActive(false);
        Invoke("ActivarControles", 0.5f);
        if (PlayerPrefs.GetInt("yaUsoBotonSort", 0) == 0) {
            Invoke("ActivarAvisoTutorial", 10);
        }
        analiticas.DificultadSeleccionada(ultimaDificultadSeleccionada + 1);
        analiticas.PuzzleSeleccionado(puzzlePreseleccionado);
    }

    void ActivarControles() {
        MoverPiezas moverPiezas = this.gameObject.GetComponent<MoverPiezas>();
        moverPiezas.puzzlePlaying = true;
    }

    void DesordenarPiezas(Transform nuevoPuzzle) { //随机碎片
        this.gameObject.GetComponent<MoverPiezas>().posicionZ = 0.0f;
        List<int> listaProfundidades = new List<int>();
        int profundidad = 0;
        foreach (Transform hijo in nuevoPuzzle) {
            profundidad = Random.Range(-nuevoPuzzle.childCount, 0);
            while (listaProfundidades.Contains(profundidad)) {
                profundidad++;
                if (profundidad > 0) {
                    profundidad = -nuevoPuzzle.childCount;
                }
            }
            listaProfundidades.Add(profundidad);
            StartCoroutine(LerpHijo(hijo, profundidad * 0.001f, 2.25f));
        }
        barajarSFX.Play();
    }

    IEnumerator LerpHijo(Transform pieza, float profundidad, float velocidadRecogida) {
        Vector3 posInicial = pieza.position;
        Vector3 posFinal = new Vector3(Random.Range(-2.6f, 2.6f), Random.Range(5.2f, 5.5f), profundidad);
        float t = 0;
        while (t < 0.5f) {
            t += velocidadRecogida * Time.deltaTime;
            pieza.position = Vector3.Lerp(posInicial, posFinal, t * 2);
            yield return null;
        }
        yield return null;
    }

    public void OrdenarPiezasRestantes() {
        this.gameObject.GetComponent<MoverPiezas>().posicionZ = 0.0f;
        PlayerPrefs.SetInt("yaUsoBotonSort", 1);
        List<int> listaProfundidades = new List<int>();
        int profundidad = 0;
        foreach (Transform hijo in nuevoPuzzle.transform) {
            if (hijo.tag != "PiezaColocada") {
                profundidad = Random.Range(-nuevoPuzzle.transform.childCount, 0);
                while (listaProfundidades.Contains(profundidad)) {
                    profundidad++;
                    if (profundidad > 0) {
                        profundidad = -nuevoPuzzle.transform.childCount;
                    }
                }
                listaProfundidades.Add(profundidad);
                StartCoroutine(LerpHijo(hijo, profundidad * 0.001f, 5));
            }
        }
        barajarSFX.Play();
        analiticas.UsaAyudaBordes();
    }

    public void SepararPiezasDeBorde() {
        if (yaPidioSepararPiezas) {
            SepararPiezasDeBordeAceptado();
        }
        else if (descargarImagenes.CheckInternet()) {
#if PLATFORM_ANDROID
            //if (Advertisement.IsReady("rewardedVideo")) {
            //    ShowRewardedAdSepararPiezas();
            //}
            //else if (Advertisement.IsReady()) {
            //    ShowAd();
            //    SepararPiezasDeBordeAceptado();
            //}
#else
				//SepararPiezasDeBordeAceptado ();
#endif
            SepararPiezasDeBordeAceptado();
        }
        else {
            errorSFX.Play();
            noticeConnectionInGame.gameObject.SetActive(true);
        }
    }

    void SepararPiezasDeBordeAceptado() {
        PlayerPrefs.SetInt("yaUsoBotonSort", 1);
        yaPidioSepararPiezas = true;
        if (!IsInvoking("ReiniciarYaPidioSepararPiezas")) {
            Invoke("ReiniciarYaPidioSepararPiezas", 300); //5 minutos
        }
        List<int> listaProfundidades = new List<int>();
        int profundidad = 0;
        foreach (Transform hijo in nuevoPuzzle.transform) {
            if (hijo.tag != "PiezaColocada") {
                profundidad = Random.Range(-nuevoPuzzle.transform.childCount, 0);
                while (listaProfundidades.Contains(profundidad)) {
                    profundidad++;
                    if (profundidad > 0) {
                        profundidad = -nuevoPuzzle.transform.childCount;
                    }
                }
                listaProfundidades.Add(profundidad);
                StartCoroutine(LerpHijoBordes(hijo, profundidad * 0.001f, 5));
            }
        }
        barajarSFX.Play();
        ActualizarBotonesPause();
        analiticas.UsaAyudaBordes();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pieza">碎片</param>
    /// <param name="profundidad">深度</param>
    /// <param name="velocidadRecogida">速度集</param>
    /// <returns></returns>
    IEnumerator LerpHijoBordes(Transform pieza, float profundidad, float velocidadRecogida) {
        Vector3 posInicial = pieza.position;
        Vector3 posFinal;

        string dimensionesPuzzle = nuevoPuzzle.name.Substring(5, nuevoPuzzle.name.Length - 5);
        string[] dosDimensiones = dimensionesPuzzle.Split('x');
        int anchoPuzzle = int.Parse(dosDimensiones[0]) - 1;
        int altoPuzzle = int.Parse(dosDimensiones[1]) - 1;

        if (pieza.name.Contains("_0x") || pieza.name.Contains("x0") || pieza.name.Contains("x" + anchoPuzzle) || pieza.name.Contains("_" + altoPuzzle + "x")) {
            posFinal = new Vector3(Random.Range(-2.6f, -1), Random.Range(5.2f, 5.5f), profundidad);
        }
        else {
            posFinal = new Vector3(Random.Range(1, 2.6f), Random.Range(5.2f, 5.5f), profundidad);
        }


        float t = 0;
        while (t < 0.5f) {
            t += velocidadRecogida * Time.deltaTime;
            pieza.position = Vector3.Lerp(posInicial, posFinal, t * 2);
            yield return null;
        }
        yield return null;
    }

    public void VolverDesdePuzzle(bool confirmacion) {
        clickSFX.Play();
        if (confirmacion) {
            MoverPiezas moverPiezas = this.gameObject.GetComponent<MoverPiezas>();
            moverPiezas.puzzlePlaying = false;
            if (nuevoPuzzle != null) {
                Destroy(nuevoPuzzle);
            }
            SeleccionarImagen(puzzlePreseleccionado);
        }
        seguroMenu.SetActive(false);
    }

    void PonerMiniaturaDeAyuda() {
        miniaturaDeAyuda.sprite = spritePreseleccionado;
    }


    void DesactivarAyudaBG() {
        ayudaActivada = false;
        bgAyuda.sprite = null;

        ActualizarBotonesPause();
    }

    public void ActivarAyudaBackground() {
        if (ayudaActivada) {
            ayudaActivada = false;
            bgAyuda.sprite = null;
            ActualizarBotonesPause();
            return;
        }

        if (yaPidioAyudaBackground) {
            clickSFX.Play();
            ActivarAyudaBackgroundAceptado();
        }
        else if (descargarImagenes.CheckInternet()) {
            clickSFX.Play();
#if PLATFORM_ANDROID
            //if (Advertisement.IsReady("rewardedVideo")) {
            //    ShowRewardedAdMostrarBG();
            //}
            //else if (Advertisement.IsReady()) {
            //    ShowAd();
            //    ActivarAyudaBackgroundAceptado();
            //}
#else
				//ActivarAyudaBackgroundAceptado();
#endif  
            ActivarAyudaBackgroundAceptado();
        }
        else {
            errorSFX.Play();
            noticeConnectionInGame.gameObject.SetActive(true);
        }
    }

    void ActivarAyudaBackgroundAceptado() {
        yaPidioAyudaBackground = true;
        if (!IsInvoking("ReiniciarYaPidioAyudaBackground")) {
            Invoke("ReiniciarYaPidioAyudaBackground", 480); //8 minutos
        }
        ayudaActivada = !ayudaActivada;
        if (ayudaActivada) {
            bgAyuda.sprite = spritePreseleccionado;
            analiticas.UsaAyudaBG();
        }
        else {
            bgAyuda.sprite = null;
        }
        ActualizarBotonesPause();
    }

    public void ActivarMenuInGame() {
        desplegarSFX.Play();
        menuIngameActivado = !menuIngameActivado;
        botonMenuInvisible.gameObject.SetActive(menuIngameActivado);

        MoverPiezas moverPiezas = this.gameObject.GetComponent<MoverPiezas>();
        moverPiezas.puzzlePlaying = !menuIngameActivado;

        if (menuIngameActivado) {
            ActualizarBotonesPause();
            flechitaMenuInGame.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
            if (noticeTutorial.gameObject.activeSelf) {
                CerrarAvisoTutorial();
            }
            StartCoroutine(DesplazarMenuInGame(true));
        }
        else {
            flechitaMenuInGame.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            StartCoroutine(DesplazarMenuInGame(false));
        }
    }

    IEnumerator DesplazarMenuInGame(bool mostrarMenu) {

        RectTransform panelRect = panelInGame.GetComponent<RectTransform>();

        float posMostradoBottom = 0;
        float posMostradoTop = -235;
        float posOcultoBottom = -369;
        float posOcultoTop = -603;

        float transicion = (mostrarMenu) ? 0 : 1;
        float velocidadTransicion = 5;



        while (transicion >= 0 && transicion <= 1) {
            if (mostrarMenu) {
                transicion += Time.deltaTime * velocidadTransicion;
            }
            else {
                transicion -= Time.deltaTime * velocidadTransicion;
            }
            flechitaMenuInGame.anchoredPosition = new Vector2(0, Mathf.Lerp(15, 10, transicion));
            panelRect.offsetMin = new Vector2(0, Mathf.Lerp(posOcultoBottom, posMostradoBottom, transicion));
            panelRect.offsetMax = new Vector2(0, Mathf.Lerp(posOcultoTop, posMostradoTop, transicion));
            yield return null;
        }
        yield return null;
    }

    public void VerificarBoton(string opcion) {
        ActivarMenuInGame();
        switch (opcion) {
            case "Menu":
                seguroMenu.SetActive(true);
                break;
        }
    }

    public void ActivarPanelCompleto() {
        panelInit.SetActive(false);
        panelSelection.SetActive(false);
        panelPreGame.SetActive(false);
        panelInGame.SetActive(false);
        panelComplete.SetActive(true);
        tiempoEnPuzzle = Mathf.RoundToInt(Time.time - horaInicio);

        string horas = "";
        int intHoras = 0;
        string minutos = "";
        if (tiempoEnPuzzle >= 60 * 60) {
            horas = Mathf.Floor(tiempoEnPuzzle / 3600).ToString() + ":";
            intHoras = tiempoEnPuzzle / 3600;
        }
        if (tiempoEnPuzzle >= 60) {
            if (tiempoEnPuzzle >= 60 * 60) {
                minutos = Mathf.Floor((tiempoEnPuzzle - (intHoras * 3600)) / 60).ToString("00") + ":";
            }
            else {
                minutos = Mathf.Floor(tiempoEnPuzzle / 60).ToString() + ":";
            }
        }
        string segundos = (tiempoEnPuzzle % 60).ToString("00");

        string letraTiempo = (tiempoEnPuzzle < 3600) ? "m" : "";
        letraTiempo = (tiempoEnPuzzle < 60) ? "s" : letraTiempo;
        if (Application.systemLanguage.ToString() == "Spanish") {
            completeInfo.text = "Nivel de dificultad: " + (ultimaDificultadSeleccionada + 1).ToString() + "\n\nTiempo: " + horas + minutos + segundos + letraTiempo;
        }
        else {
            completeInfo.text = "Difficulty level: " + (ultimaDificultadSeleccionada + 1).ToString() + "\n\nTime: " + horas + minutos + segundos + letraTiempo;
        }

        analiticas.DificultadCompletada(ultimaDificultadSeleccionada + 1);
    }

    public void VolverAMenuTrasCompletar() {
        clickSFX.Play();
        MoverPiezas moverPiezas = this.gameObject.GetComponent<MoverPiezas>();
        moverPiezas.puzzlePlaying = false;
        if (nuevoPuzzle != null) {
            Destroy(nuevoPuzzle);
        }
        SeleccionarImagen(puzzlePreseleccionado);

        //颜色，如果你刚刚完成困难
        if (this.diyMode) {

        }
        else {
            contornoBoton[puzzlePreseleccionado].color = colorDificultad[PlayerPrefs.GetInt("puzzleCompleto" + puzzlePreseleccionado, 0)];
            if (PlayerPrefs.GetInt("puzzleCompleto" + puzzlePreseleccionado, 0) == 5) {
                TexturaABoton(puzzlePreseleccionado, descargarImagenes.puzzleImageList[puzzlePreseleccionado]);
            }
        }

#if PLATFORM_ANDROID
        if (puedeMostrarAnuncioExtra && Advertisement.IsReady("rewardedVideo")) {
            contadorTiempoAnuncio = 0;
            puedeMostrarAnuncioExtra = false;
            puedeMostrarAnuncio = false;
            ShowRewardedAdExtra();
        }
        else if (puedeMostrarAnuncio && Advertisement.IsReady()) {
            contadorTiempoAnuncio = 0;
            puedeMostrarAnuncioExtra = false;
            puedeMostrarAnuncio = false;
            ShowAd();
        }
#endif
        panelInit.SetActive(true);
        panelSelection.SetActive(false);
        panelPreGame.SetActive(false);
        panelInGame.SetActive(false);
        panelComplete.SetActive(false);
    }

    void ReiniciarYaPidioAyudaBackground() {
        yaPidioAyudaBackground = false;
        ActualizarBotonesPause();
    }
    void ReiniciarYaPidioSepararPiezas() {
        yaPidioSepararPiezas = false;
        ActualizarBotonesPause();
    }



    public void ShowAd() {
#if PLATFORM_ANDROID
        if (Advertisement.IsReady()) {
            Advertisement.Show();
        }
#endif
    }


    public void ShowRewardedAdSepararPiezas() {
#if PLATFORM_ANDROID
        if (Advertisement.IsReady("rewardedVideo")) {
            var options = new ShowOptions { resultCallback = HandleShowResultSepararPiezas };
            Advertisement.Show("rewardedVideo", options);
        }
#endif
    }
#if PLATFORM_ANDROID
    private void HandleShowResultSepararPiezas(ShowResult result) {
        switch (result) {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                //
                // YOUR CODE TO REWARD THE GAMER
                SepararPiezasDeBordeAceptado();
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }

    public void ShowRewardedAdMostrarBG() {
        if (Advertisement.IsReady("rewardedVideo")) {
            var options = new ShowOptions { resultCallback = HandleShowResultMostrarBG };
            Advertisement.Show("rewardedVideo", options);
        }
    }

    private void HandleShowResultMostrarBG(ShowResult result) {
        switch (result) {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                //
                // YOUR CODE TO REWARD THE GAMER
                ActivarAyudaBackgroundAceptado();
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }

    public void ShowRewardedAdExtra() {
        if (Advertisement.IsReady("rewardedVideo")) {
            var options = new ShowOptions { resultCallback = HandleShowResultExtra };
            Advertisement.Show("rewardedVideo", options);
        }
    }

    private void HandleShowResultExtra(ShowResult result) {
        switch (result) {
            case ShowResult.Finished:
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }
#endif

    /// <summary>
    /// 更新按钮暂停
    /// </summary>
    void ActualizarBotonesPause() {
#if PLATFORM_ANDROID
        if (yaPidioSepararPiezas) {
            tickSort.SetActive(false);
        }
        else {
            tickSort.SetActive(true);
        }
        if (!yaPidioAyudaBackground && !ayudaActivada) {
            tickGuia.SetActive(true);
        }
        else {
            tickGuia.SetActive(false);
        }
#else
			tickSort.SetActive (false);
			tickGuia.SetActive (false);
#endif
    }
}