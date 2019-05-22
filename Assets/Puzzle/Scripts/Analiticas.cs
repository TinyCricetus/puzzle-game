using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// 类似于用于注册和分发事件
/// </summary>
public class Analiticas : MonoBehaviour {

    /// <summary>
    /// 拼图选择
    /// </summary>
    /// <param name="numPuzzle"></param>
	public void PuzzleSeleccionado (int numPuzzle) {
		Analytics.CustomEvent("Puzzles",  new Dictionary<string, object>
			{
				{ "PuzzleSeleccionado", "Puzzle"+numPuzzle }
			});
	}

    /// <summary>
    /// 难度选择
    /// </summary>
    /// <param name="nivelDificultad"></param>
	public void DificultadSeleccionada (int nivelDificultad) {
		Analytics.CustomEvent("Puzzles",  new Dictionary<string, object>
			{
				{ "DificultadSeleccionada", "D "+nivelDificultad }
			});
	}

    /// <summary>
    /// 难度标记完成
    /// </summary>
    /// <param name="nivelDificultad"></param>
	public void DificultadCompletada (int nivelDificultad) {
		Analytics.CustomEvent("Puzzles",  new Dictionary<string, object>
			{
				{ "DificultadCompletada", "D "+nivelDificultad }
			});
	}

    /// <summary>
    /// 背景图片
    /// </summary>
	public void UsaAyudaBG () {
		Analytics.CustomEvent("Puzzles",  new Dictionary<string, object>
			{
				{ "AyudaBG", 1 }
			});
	}
    /// <summary>
    /// 边界图片
    /// </summary>
	public void UsaAyudaBordes () {
		Analytics.CustomEvent("Puzzles",  new Dictionary<string, object>
			{
				{ "AyudaBordes", 1 }
			});
	}

    /// <summary>
    /// ？？？
    /// </summary>
	public void UsaAyudaSeparar () {
		Analytics.CustomEvent("Puzzles",  new Dictionary<string, object>
			{
				{ "AyudaSeparar (Siempre hay una al iniciar Puzzle)", 1 }
			});
	}

}
