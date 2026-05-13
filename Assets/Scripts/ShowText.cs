using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x0200001C RID: 28
public class ShowText : MonoBehaviour
{
	private bool _skipCurrentDialogue;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			this._skipCurrentDialogue = true;
		}
	}

	// Token: 0x0600006E RID: 110 RVA: 0x00004062 File Offset: 0x00002262
	private void Start()
	{
		this._textMeshPro = base.GetComponent<TextMeshProUGUI>();
		Debug.Log(this._textMeshPro);
		this.ShowTexts(StaticData.strings);
	}

	// Token: 0x0600006F RID: 111 RVA: 0x00004086 File Offset: 0x00002286
	public class TextJob
	{
		public string[] strings;
		public Action onComplete;
	}

	private Queue<TextJob> _queue = new Queue<TextJob>();

	public void ShowTexts(string[] strings, Action onComplete = null)
	{
		if (strings == null) return;
		this._queue.Enqueue(new TextJob { strings = strings, onComplete = onComplete });
		if (this._queue.Count == 1)
		{
			base.StartCoroutine(this.ShowTestTexts());
		}
	}

	public void PlayConfigText(string resourcePath, Action onComplete = null)
	{
		TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
		if (textAsset != null)
		{
			StartCoroutine(PlayTextRoutine(textAsset.text, onComplete));
		}
		else
		{
			Debug.LogWarning("Text configuration not found: " + resourcePath);
			onComplete?.Invoke();
		}
	}

	private IEnumerator WaitSkippable(float seconds)
	{
		float end = Time.realtimeSinceStartup + seconds;
		while (Time.realtimeSinceStartup < end)
		{
			if (this._skipCurrentDialogue)
			{
				yield break;
			}
			yield return null;
		}
	}

	private IEnumerator PlayTextRoutine(string text, Action onComplete)
	{
		_textMeshPro.text = "";
		string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		
		foreach (string line in lines)
		{
			if (this._skipCurrentDialogue)
			{
				this._skipCurrentDialogue = false;
				onComplete?.Invoke();
				yield break;
			}
			if (line == ",")
			{
				yield return StartCoroutine(this.WaitSkippable(1f));
				if (this._skipCurrentDialogue)
				{
					this._skipCurrentDialogue = false;
					onComplete?.Invoke();
					yield break;
				}
			}
			else if (line == ".")
			{
				_textMeshPro.text = "";
				AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
				yield return StartCoroutine(this.WaitSkippable(0.06f));
				if (this._skipCurrentDialogue)
				{
					this._skipCurrentDialogue = false;
					onComplete?.Invoke();
					yield break;
				}
			}
			else
			{
				foreach (char c in line)
				{
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						onComplete?.Invoke();
						yield break;
					}
					_textMeshPro.text += c.ToString();
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
					yield return StartCoroutine(this.WaitSkippable(0.06f));
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						onComplete?.Invoke();
						yield break;
					}
				}
				_textMeshPro.text += "\n";
			}
		}
		onComplete?.Invoke();
	}

	// Token: 0x06000070 RID: 112 RVA: 0x000040AF File Offset: 0x000022AF
	public IEnumerator ShowTestTexts()
	{
		while (this._queue.Count > 0)
		{
			TextJob job = this._queue.Peek();
			string[] strings = job.strings;
			int num;
			for (int i = 0; i < strings.Length; i = num + 1)
			{
				if (this._skipCurrentDialogue)
				{
					this._skipCurrentDialogue = false;
					break;
				}
				if (strings[i].Contains(","))
				{
					yield return StartCoroutine(this.WaitSkippable(1f));
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						break;
					}
				}
				else if (strings[i].Contains("."))
				{
					this._textMeshPro.text = "";
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
					yield return StartCoroutine(this.WaitSkippable(0.06f));
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						break;
					}
				}
				else
				{
					foreach (char c in strings[i])
					{
						if (this._skipCurrentDialogue)
						{
							this._skipCurrentDialogue = false;
							goto SkipJob;
						}
						TextMeshProUGUI textMeshPro = this._textMeshPro;
						textMeshPro.text += c.ToString();
						AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
						yield return StartCoroutine(this.WaitSkippable(0.06f));
						if (this._skipCurrentDialogue)
						{
							this._skipCurrentDialogue = false;
							goto SkipJob;
						}
					}
					TextMeshProUGUI textMeshPro2 = this._textMeshPro;
					textMeshPro2.text += "\n";
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
				}
				num = i;
			}
		SkipJob:
			job.onComplete?.Invoke();
			this._queue.Dequeue();
			strings = null;
		}
		yield break;
	}

	// Token: 0x04000059 RID: 89
	private TextMeshProUGUI _textMeshPro;

	// Token: 0x0400005A RID: 90
	// 移除了 private Queue<string[]> _queue = new Queue<string[]>(); 来避免报错
}
