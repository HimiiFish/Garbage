using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x0200001C RID: 28
public class ShowText : MonoBehaviour
{
	// Token: 0x0600006E RID: 110 RVA: 0x00004062 File Offset: 0x00002262
	private void Start()
	{
		this._textMeshPro = base.GetComponent<TextMeshProUGUI>();
		Debug.Log(this._textMeshPro);
		this.ShowTexts(StaticData.strings);
	}

	// Token: 0x0600006F RID: 111 RVA: 0x00004086 File Offset: 0x00002286
	public void ShowTexts(string[] strings)
	{
		this._queue.Enqueue(strings);
		if (this._queue.Count == 1)
		{
			base.StartCoroutine(this.ShowTestTexts());
		}
	}

	// Token: 0x06000070 RID: 112 RVA: 0x000040AF File Offset: 0x000022AF
	public IEnumerator ShowTestTexts()
	{
		while (this._queue.Count > 0)
		{
			string[] strings = this._queue.Peek();
			int num;
			for (int i = 0; i < strings.Length; i = num + 1)
			{
				if (strings[i].Contains(","))
				{
					yield return new WaitForSecondsRealtime(1f);
				}
				else if (strings[i].Contains("."))
				{
					this._textMeshPro.text = "";
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
					yield return new WaitForSecondsRealtime(0.06f);
				}
				else
				{
					foreach (char c in strings[i])
					{
						TextMeshProUGUI textMeshPro = this._textMeshPro;
						textMeshPro.text += c.ToString();
						AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
						yield return new WaitForSecondsRealtime(0.06f);
					}
					string text = null;
					TextMeshProUGUI textMeshPro2 = this._textMeshPro;
					textMeshPro2.text += "\n";
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
				}
				num = i;
			}
			this._queue.Dequeue();
			strings = null;
		}
		yield break;
	}

	// Token: 0x04000059 RID: 89
	private TextMeshProUGUI _textMeshPro;

	// Token: 0x0400005A RID: 90
	private Queue<string[]> _queue = new Queue<string[]>();
}
