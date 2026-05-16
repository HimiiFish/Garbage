using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour
{
	private bool _skipCurrentDialogue;

	private Action _configTextOnComplete;

	[SerializeField]
	[Tooltip("逐字显示间隔（秒）")]
	private float _charRevealDelay = 0.05f;

	[SerializeField]
	[Tooltip("清屏符 \".\" 后的停顿（秒）")]
	private float _clearPageDelay = 0.55f;

	[SerializeField]
	private float _commaPauseSeconds = 1f;

	[SerializeField]
	[Tooltip("单段对话因高度溢出而拆分的「软分页」上限")]
	private int _maxSoftPagesPerJob = 48;

	private readonly List<string> _archivedSoftPages = new List<string>();

	private readonly StringBuilder _currentPage = new StringBuilder(512);

	private readonly List<string> _pageHistory = new List<string>();

	private int _historyViewIndex;

	private bool _inCharReveal;

	private TextMeshProUGUI _textMeshPro;

	private void Awake()
	{
		this._textMeshPro = base.GetComponent<TextMeshProUGUI>();
		if (this._textMeshPro == null)
		{
			return;
		}
		this.UnwrapFromLegacyDialogueStacks();
		this.RemoveAutoLayoutHelpersFromTextObject();
		this._textMeshPro.enableWordWrapping = true;
		this._textMeshPro.overflowMode = TextOverflowModes.Masking;
		RectTransform parentRt = this._textMeshPro.transform.parent as RectTransform;
		if (parentRt != null && parentRt.GetComponent<RectMask2D>() == null)
		{
			parentRt.gameObject.AddComponent<RectMask2D>();
		}
	}

	private void UnwrapFromLegacyDialogueStacks()
	{
		while (base.transform.parent != null && base.transform.parent.name == "DialogueTextStack")
		{
			Transform stackTf = base.transform.parent;
			Transform dest = stackTf.parent;
			base.transform.SetParent(dest, false);
			UnityEngine.Object.Destroy(stackTf.gameObject);
		}
	}

	private void RemoveAutoLayoutHelpersFromTextObject()
	{
		ContentSizeFitter fit = base.gameObject.GetComponent<ContentSizeFitter>();
		if (fit != null)
		{
			UnityEngine.Object.Destroy(fit);
		}
		LayoutElement le = base.gameObject.GetComponent<LayoutElement>();
		if (le != null)
		{
			UnityEngine.Object.Destroy(le);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			this.SkipAllPendingDialogue();
		}
		if (this._textMeshPro == null)
		{
			return;
		}
		if (this._inCharReveal)
		{
			return;
		}
		if (this._pageHistory.Count == 0)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			this._historyViewIndex = Mathf.Max(0, this._historyViewIndex - 1);
			this.ApplyHistoryView();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			this._historyViewIndex = Mathf.Min(this._pageHistory.Count - 1, this._historyViewIndex + 1);
			this.ApplyHistoryView();
		}
	}

	private void FlushTmpFromCurrentPage()
	{
		this._textMeshPro.text = this._currentPage.ToString();
		this._textMeshPro.ForceMeshUpdate(true);
	}

	private bool CurrentPageFitsInMask()
	{
		RectTransform rt = this._textMeshPro.rectTransform;
		float limitH = rt.rect.height;
		if (limitH < 4f)
		{
			return true;
		}
		float prefH = this._textMeshPro.GetPreferredValues(this._textMeshPro.text).y;
		return prefH <= limitH + 4f;
	}

	private void AppendCharWithSoftPageBreak(char c)
	{
		this._currentPage.Append(c);
		this.FlushTmpFromCurrentPage();
		if (this.CurrentPageFitsInMask())
		{
			return;
		}
		this._currentPage.Length--;
		this.FlushTmpFromCurrentPage();
		if (this._archivedSoftPages.Count >= this._maxSoftPagesPerJob)
		{
			this._currentPage.Append(c);
			this.FlushTmpFromCurrentPage();
			return;
		}
		if (this._currentPage.Length > 0)
		{
			this._archivedSoftPages.Add(this._currentPage.ToString());
		}
		this._currentPage.Clear();
		this._currentPage.Append(c);
		this.FlushTmpFromCurrentPage();
		if (!this.CurrentPageFitsInMask() && this._currentPage.Length <= 1)
		{
			return;
		}
	}

	private string BuildSnapshotForHistory()
	{
		StringBuilder sb = new StringBuilder(1024);
		for (int i = 0; i < this._archivedSoftPages.Count; i++)
		{
			if (sb.Length > 0)
			{
				sb.Append("\n\n");
			}
			sb.Append(this._archivedSoftPages[i]);
		}
		if (this._currentPage.Length > 0)
		{
			if (sb.Length > 0)
			{
				sb.Append("\n\n");
			}
			sb.Append(this._currentPage.ToString());
		}
		return sb.ToString();
	}

	private void PushHistorySnapshot()
	{
		string s = this.BuildSnapshotForHistory();
		if (string.IsNullOrWhiteSpace(s))
		{
			return;
		}
		if (this._pageHistory.Count > 0 && this._pageHistory[this._pageHistory.Count - 1] == s)
		{
			return;
		}
		this._pageHistory.Add(s);
		this._historyViewIndex = this._pageHistory.Count - 1;
	}

	private void ClearHistory()
	{
		this._pageHistory.Clear();
		this._historyViewIndex = 0;
	}

	private void ApplyHistoryView()
	{
		if (this._pageHistory.Count == 0)
		{
			return;
		}
		string s = this._pageHistory[Mathf.Clamp(this._historyViewIndex, 0, this._pageHistory.Count - 1)];
		this.ResetTypingBuffers();
		this._textMeshPro.text = s;
		this._textMeshPro.ForceMeshUpdate(true);
	}

	private void ResetTypingBuffers()
	{
		this._archivedSoftPages.Clear();
		this._currentPage.Clear();
		if (this._textMeshPro != null)
		{
			this._textMeshPro.text = "";
		}
	}

	private void Start()
	{
		if (this._textMeshPro == null)
		{
			this._textMeshPro = base.GetComponent<TextMeshProUGUI>();
		}
		this.UnwrapFromLegacyDialogueStacks();
	}

	public void SkipAllPendingDialogue()
	{
		this._skipCurrentDialogue = true;
		base.StopAllCoroutines();
		this._inCharReveal = false;
		while (this._queue.Count > 0)
		{
			TextJob job = this._queue.Dequeue();
			job.onComplete?.Invoke();
		}
		if (this._configTextOnComplete != null)
		{
			Action done = this._configTextOnComplete;
			this._configTextOnComplete = null;
			done();
		}
		this._skipCurrentDialogue = false;
	}

	public class TextJob
	{
		public string[] strings;
		public Action onComplete;
	}

	private Queue<TextJob> _queue = new Queue<TextJob>();

	public void ShowTexts(string[] strings, Action onComplete = null)
	{
		if (strings == null)
		{
			return;
		}
		this._queue.Enqueue(new TextJob
		{
			strings = strings,
			onComplete = onComplete
		});
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
			this._configTextOnComplete = onComplete;
			this.StartCoroutine(this.PlayTextRoutine(textAsset.text, delegate
			{
				if (this._configTextOnComplete != null)
				{
					Action done = this._configTextOnComplete;
					this._configTextOnComplete = null;
					done();
				}
			}));
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
		this.ClearHistory();
		this.ResetTypingBuffers();
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
				yield return this.StartCoroutine(this.WaitSkippable(this._commaPauseSeconds));
				if (this._skipCurrentDialogue)
				{
					this._skipCurrentDialogue = false;
					onComplete?.Invoke();
					yield break;
				}
			}
			else if (line == ".")
			{
				this.PushHistorySnapshot();
				this.ResetTypingBuffers();
				AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
				yield return this.StartCoroutine(this.WaitSkippable(this._clearPageDelay));
				if (this._skipCurrentDialogue)
				{
					this._skipCurrentDialogue = false;
					onComplete?.Invoke();
					yield break;
				}
			}
			else
			{
				this._inCharReveal = true;
				foreach (char c in line)
				{
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						this._inCharReveal = false;
						onComplete?.Invoke();
						yield break;
					}
					this.AppendCharWithSoftPageBreak(c);
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
					yield return this.StartCoroutine(this.WaitSkippable(this._charRevealDelay));
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						this._inCharReveal = false;
						onComplete?.Invoke();
						yield break;
					}
				}
				this._inCharReveal = false;
				this.AppendCharWithSoftPageBreak('\n');
				AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
			}
		}
		this.PushHistorySnapshot();
		onComplete?.Invoke();
	}

	public IEnumerator ShowTestTexts()
	{
		while (this._queue.Count > 0)
		{
			this.ClearHistory();
			this.ResetTypingBuffers();
			TextJob job = this._queue.Peek();
			string[] strings = job.strings;
			int num;
			for (int i = 0; i < strings.Length; i = num + 1)
			{
				if (this._skipCurrentDialogue)
				{
					this._skipCurrentDialogue = false;
					goto SkipJob;
				}
				if (strings[i] == ",")
				{
					yield return this.StartCoroutine(this.WaitSkippable(this._commaPauseSeconds));
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						goto SkipJob;
					}
				}
				else if (strings[i] == ".")
				{
					this.PushHistorySnapshot();
					this.ResetTypingBuffers();
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
					yield return this.StartCoroutine(this.WaitSkippable(this._clearPageDelay));
					if (this._skipCurrentDialogue)
					{
						this._skipCurrentDialogue = false;
						goto SkipJob;
					}
				}
				else
				{
					this._inCharReveal = true;
					foreach (char c in strings[i])
					{
						if (this._skipCurrentDialogue)
						{
							this._skipCurrentDialogue = false;
							this._inCharReveal = false;
							goto SkipJob;
						}
						this.AppendCharWithSoftPageBreak(c);
						AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
						yield return this.StartCoroutine(this.WaitSkippable(this._charRevealDelay));
						if (this._skipCurrentDialogue)
						{
							this._skipCurrentDialogue = false;
							this._inCharReveal = false;
							goto SkipJob;
						}
					}
					this._inCharReveal = false;
					this.AppendCharWithSoftPageBreak('\n');
					AudioManager.Instance.PlaySFXWithRandomPitch("打字", 0.8f, 1.2f);
				}
				num = i;
			}
		SkipJob:
			this.PushHistorySnapshot();
			job.onComplete?.Invoke();
			this._queue.Dequeue();
		}
		yield break;
	}
}
