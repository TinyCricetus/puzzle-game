using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSetting : MonoBehaviour {


	string language;

	public Text play;
	public Text adv;
	public Text areYouSure;
	public Text msgTutorial;
	public Text cancel;
	public Text classify;
	public Text complete;
	public Text connectInternet1;
	public Text connectInternet2;
	public Text download;
	public Text easy;
	public Text epic;
	public Text guide;
	public Text hard;
	public Text medio;
	public Text preview;
	public Text regroup;
	public Text start;
	public Text yes;
    public Text no;



	void Start () {
        //获取当前计算机主语言
		this.language = Application.systemLanguage.ToString();

		if (language == "Chinese") {  //如果它不是中文，它什么都不做，因为默认情况下它是英文的。
            TipsInit ();
		} 
	}

    /// <summary>
    /// 小贴士提示语初始化
    /// </summary>
	void TipsInit(){
		play.text = "开始";
        //Debug.Log("已经执行到这里了！");
		play.fontSize = 52;
		areYouSure.text = "您将返回初始菜单.\n\n您同意吗？";
		msgTutorial.text = "嘿!你看过下面的菜单了吗?\n\n你可以把碎片从中央边缘分开.";
		cancel.text = "取消";
		classify.text = "分组";
		classify.fontSize = 22;
		complete.text = "拼图\n完成!";
		complete.fontSize = 34;
		connectInternet1.text = "不好意思,请重新连接网络.";
		connectInternet2.text = "不好意思,请重新连接网络.";
		download.text = "下载";
		easy.text = "容易\n16碎片";
		medio.text = "适中\n24碎片";
		adv.text = "困难\n60碎片";
		hard.text = "专业\n112碎片";
		epic.text = "史诗\n160碎片";
		guide.text = "引导";
		preview.text = "参考预览";
		preview.fontSize = 22;
		regroup.text = "重组";
		regroup.fontSize = 22;
		start.text = "启动";
		start.fontSize = 36;
		yes.text = "确认";
        no.text = "取消";
	}

}
