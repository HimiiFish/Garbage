using System;
using System.Collections;
using UniRx;
using UnityEngine;

public class CampaignManager : MonoBehaviour
{
	public static CampaignManager Instance { get; private set; }

	public static GameCampaignPhase CurrentPhase { get; private set; } = GameCampaignPhase.Tutorial;

	public static int Level2CompletedOutings { get; private set; }

	public static bool UpgradesEnabledAtStation => CurrentPhase != GameCampaignPhase.Tutorial;

	public static int GarbagePerTrip => 10;

	[SerializeField]
	private ShipController ship;

	[SerializeField]
	private GenerateManager generateManager;

	[SerializeField]
	private GenerateData generateData;

	private bool _transitionRunning;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(base.gameObject);
			return;
		}
		Instance = this;
		if (LevelFadeTransition.Instance == null)
		{
			new GameObject("LevelFadeTransition").AddComponent<LevelFadeTransition>();
		}
	}

	private void Start()
	{
		if (this.ship == null)
		{
			this.ship = UnityEngine.Object.FindObjectOfType<ShipController>();
		}
		if (this.generateManager == null)
		{
			this.generateManager = GenerateManager.Instance;
		}
		this.ApplyPhaseToSystems(CurrentPhase, true);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static bool IsEndlessMode()
	{
		return CurrentPhase == GameCampaignPhase.Endless;
	}

	public void NotifyFullCargoCollected()
	{
		if (this.ship != null)
		{
			this.ship.SetCargoFullPendingDock(true);
		}
	}

	public void OnStationDockedAfterFullCargo()
	{
		if (this._transitionRunning || this.ship == null)
		{
			return;
		}
		switch (CurrentPhase)
		{
		case GameCampaignPhase.Tutorial:
			this.ship.QueueStationDialogueForCampaign(true);
			break;
		case GameCampaignPhase.UpgradeMission:
			Level2CompletedOutings++;
			this.ship.QueueStationDialogueForCampaign(Level2CompletedOutings >= 2);
			break;
		}
	}

	public void TryCompleteLevelAfterStationDialogue()
	{
		if (this._transitionRunning)
		{
			return;
		}
		bool shouldAdvance = false;
		switch (CurrentPhase)
		{
		case GameCampaignPhase.Tutorial:
			shouldAdvance = true;
			break;
		case GameCampaignPhase.UpgradeMission:
			shouldAdvance = Level2CompletedOutings >= 2;
			break;
		}
		if (!shouldAdvance)
		{
			return;
		}
		this.StartCoroutine(this.AdvanceCampaignPhaseRoutine(CurrentPhase));
	}

	public void OnLaunchedFromStation()
	{
		if (this.generateManager == null)
		{
			return;
		}
		if (IsEndlessMode())
		{
			MessageBroker.Default.Publish<GenerateGarbageMessage>(new GenerateGarbageMessage());
			return;
		}
		this.generateManager.BeginNewOutingSpawn();
	}

	private IEnumerator AdvanceCampaignPhaseRoutine(GameCampaignPhase completed)
	{
		this._transitionRunning = true;
		LevelFadeTransition fade = LevelFadeTransition.Instance;
		if (fade != null)
		{
			yield return fade.FadeOut();
		}
		if (this.generateManager != null)
		{
			this.generateManager.ClearAllActiveGarbage();
		}
		GameCampaignPhase next = completed == GameCampaignPhase.Tutorial ? GameCampaignPhase.UpgradeMission : GameCampaignPhase.Endless;
		CurrentPhase = next;
		if (next == GameCampaignPhase.UpgradeMission)
		{
			Level2CompletedOutings = 0;
		}
		this.ApplyPhaseToSystems(next, false);
		if (this.ship != null)
		{
			this.ship.PrepareForCampaignPhaseStart(next);
		}
		MessageBroker.Default.Publish<LevelCompleteMessage>(new LevelCompleteMessage(completed));
		if (fade != null)
		{
			yield return fade.FadeIn();
		}
		this._transitionRunning = false;
		if (this.ship != null)
		{
			this.ship.ShowPhaseIntroAfterTransition(next);
		}
	}

	private void ApplyPhaseToSystems(GameCampaignPhase phase, bool initialSceneLoad)
	{
		if (this.generateData == null)
		{
			return;
		}
		switch (phase)
		{
		case GameCampaignPhase.Tutorial:
			this.generateData.allowInfiniteRefill = false;
			this.generateData.maxLifetimeSpawns = GarbagePerTrip;
			break;
		case GameCampaignPhase.UpgradeMission:
			this.generateData.allowInfiniteRefill = false;
			this.generateData.maxLifetimeSpawns = GarbagePerTrip;
			break;
		case GameCampaignPhase.Endless:
			this.generateData.allowInfiniteRefill = true;
			this.generateData.maxLifetimeSpawns = 0;
			break;
		}
		if (this.generateManager != null)
		{
			this.generateManager.ApplyCampaignPhase(phase, initialSceneLoad);
		}
		if (this.ship != null)
		{
			this.ship.ApplyCampaignPhase(phase);
		}
	}
}
