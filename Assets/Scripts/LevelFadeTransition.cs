using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelFadeTransition : MonoBehaviour
{
	public static LevelFadeTransition Instance { get; private set; }

	[SerializeField]
	private float fadeDuration = 0.65f;

	private Canvas _canvas;

	private Image _overlay;

	private bool _busy;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(base.gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(base.gameObject);
		this.EnsureOverlay();
		this._overlay.color = new Color(0f, 0f, 0f, 0f);
		this._overlay.raycastTarget = false;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void EnsureOverlay()
	{
		if (this._overlay != null)
		{
			return;
		}
		GameObject canvasGo = new GameObject("LevelFadeCanvas");
		canvasGo.transform.SetParent(base.transform, false);
		this._canvas = canvasGo.AddComponent<Canvas>();
		this._canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		this._canvas.sortingOrder = 9999;
		canvasGo.AddComponent<CanvasScaler>();
		canvasGo.AddComponent<GraphicRaycaster>();
		GameObject imageGo = new GameObject("FadeOverlay");
		imageGo.transform.SetParent(canvasGo.transform, false);
		RectTransform rt = imageGo.AddComponent<RectTransform>();
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
		this._overlay = imageGo.AddComponent<Image>();
		this._overlay.color = Color.black;
	}

	public IEnumerator FadeOut()
	{
		if (this._busy)
		{
			yield break;
		}
		this._busy = true;
		this.EnsureOverlay();
		this._overlay.raycastTarget = true;
		this._overlay.DOKill();
		yield return this._overlay.DOFade(1f, this.fadeDuration).SetUpdate(true).WaitForCompletion();
	}

	public IEnumerator FadeIn()
	{
		this.EnsureOverlay();
		this._overlay.DOKill();
		yield return this._overlay.DOFade(0f, this.fadeDuration).SetUpdate(true).WaitForCompletion();
		this._overlay.raycastTarget = false;
		this._busy = false;
	}

	public void RunTransition(Action midFadeAction, Action onComplete = null)
	{
		base.StartCoroutine(this.RunTransitionRoutine(midFadeAction, onComplete));
	}

	private IEnumerator RunTransitionRoutine(Action midFadeAction, Action onComplete)
	{
		yield return this.FadeOut();
		midFadeAction?.Invoke();
		yield return this.FadeIn();
		onComplete?.Invoke();
	}
}
