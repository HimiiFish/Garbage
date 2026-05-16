using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShipController : MonoBehaviour
{
	private CompositeDisposable _enableLifetime;

	private CompositeDisposable _stationPurchaseDisposable;

	private void Start()
	{
		this.EnsureCampaignManager();
		this._grapnel = base.GetComponentInChildren<GrapnelController>(true);
		this.RefreshGrapnelUpgrades();
		this.index = -1;
		this.isInSapceShip = true;
		this._centerPoint = new Vector3(0f, 0f, 0f);
		this.ApplyCampaignPhase(CampaignManager.CurrentPhase);
		MessageBroker.Default.Receive<GarbageCollectedMessage>().Subscribe(delegate(GarbageCollectedMessage msg)
		{
			if (this.isInSapceShip)
			{
				return;
			}
			this.money += Mathf.Max(0, msg.MoneyAwarded);
			this.garbageCount++;
			if (this.garbageCount >= this.garbageCountMax)
			{
				this.garbageCount = this.garbageCountMax;
				this.SetCargoFullPendingDock(true);
				if (CampaignManager.Instance != null)
				{
					CampaignManager.Instance.NotifyFullCargoCollected();
				}
				string[] strings = new string[]
				{
					"."
				};
				this.showText.ShowTexts(strings);
				string[] strings2 = new string[]
				{
					"垃圾已经收集完毕，返回空间站吧。"
				};
				this.showText.ShowTexts(strings2);
				return;
			}
			string[] strings3 = new string[]
			{
				"."
			};
			this.showText.ShowTexts(strings3);
			List<string[]> list = new List<string[]>
			{
				new string[]
				{
					" 你捡到了一些垃圾\n"
				},
				new string[]
				{
					"你清除了一块太空垃圾。\n这垃圾让你想起了一个有五十个州的国家，这让你感觉自己的英文水平得到了提升。\n"
				},
				new string[]
				{
					"你又清除了一块垃圾。\n你提前为我国空间站解决了一个隐患。\n"
				},
				new string[]
				{
					"你清除了一块太空垃圾。\n你望着这块圆圆的垃圾想起了今天是中秋，\n你觉得这太孤独了只有垃圾陪着你。\n但你想到美国那三个宇航员至今还没回地球心里就舒服多了\n"
				},
				new string[]
				{
					"你清除了一些垃圾。\n你十分肯定这是星链的碎片"
				},
				new string[]
				{
					"你清除了一些垃圾。\n这次的垃圾是一些烟花，椅子，还有古装的碎片\n难道......万户真的成功了？"
				},
				new string[]
				{
					"你清除了一些垃圾。\n你搞不清楚这样的东西还有多少。"
				},
				new string[]
				{
					"你清除了一些垃圾。\n你举得以你的辛勤程度一定能回月球基地评上生产标兵。"
				}
			};
			int num = Random.Range(0, list.Count);
			string[] array = new string[]
			{
				"",
				"搜集垃圾:\n" + this.garbageCount.ToString() + "/" + this.garbageCountMax.ToString()
			};
			string bonus = msg.MoneyAwarded > 0 ? "\n获得现金 " + msg.MoneyAwarded.ToString() + "$。\n" : "\n";
			array[0] = list[num][0] + bonus;
			this.showText.ShowTexts(array);
		}).AddTo(this);
	}

	public void ApplyFlyingVisualState()
	{
		base.transform.DOKill(false);
		base.transform.SetParent(null, true);
		base.transform.localScale = Vector3.one;
		this.isInSapceShip = false;
		this.SetDockedVisuals(false);
		this.orbitRadius = base.transform.position - this._centerPoint;
	}

	private void SetDockedVisuals(bool docked)
	{
		foreach (TrailRenderer trailRenderer in base.GetComponentsInChildren<TrailRenderer>(true))
		{
			if (docked)
			{
				trailRenderer.emitting = false;
				trailRenderer.Clear();
			}
			else
			{
				trailRenderer.Clear();
				trailRenderer.emitting = true;
			}
		}
		LineRenderer lineRenderer = base.GetComponent<LineRenderer>();
		if (lineRenderer != null)
		{
			lineRenderer.enabled = !docked;
		}
	}

	private void CompleteStationLaunch()
	{
		if (this.spaceStation == null)
		{
			return;
		}
		base.transform.DOKill(false);
		Vector3 worldPos = this.spaceStation.transform.position;
		base.transform.SetParent(null, true);
		base.transform.position = worldPos;
		this.orbitRadius = worldPos - this._centerPoint;
		base.transform.DOScale(Vector3.one, 0.1f);
		this.startButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/主界面+控制台+结局（缺少标题）/控制台/开关on");
		this.isInSapceShip = false;
		this.SetDockedVisuals(false);
		StaticData.BeginOrbitalMotion(this._centerPoint);
		if (CampaignManager.Instance != null)
		{
			CampaignManager.Instance.OnLaunchedFromStation();
		}
		else if (GenerateManager.Instance != null)
		{
			GenerateManager.Instance.BeginNewOutingSpawn();
		}
	}

	public void SetCargoFullPendingDock(bool value)
	{
		this._cargoFullPendingDock = value;
	}

	public void ApplyCampaignPhase(GameCampaignPhase phase)
	{
		this.garbageCountMax = CampaignManager.GarbagePerTrip;
		if (phase == GameCampaignPhase.Tutorial)
		{
			this._orbitSpeedUpgradeLevel = 0;
			this._grapnelSpeedUpgradeLevel = 0;
			this._grapnelLengthUpgradeLevel = 0;
			this.RefreshGrapnelUpgrades();
		}
	}

	public void PrepareForCampaignPhaseStart(GameCampaignPhase phase)
	{
		this.ApplyCampaignPhase(phase);
		this.garbageCount = 0;
		this._cargoFullPendingDock = false;
		this._stationLevelCompletePending = false;
		this.isOver = false;
		this.fuelValue = this.fuelValueMax;
		if (this.spaceStation != null)
		{
			base.transform.SetParent(this.spaceStation.transform, true);
			base.transform.localScale = Vector3.zero;
			this.isInSapceShip = true;
			this.SetDockedVisuals(true);
		}
	}

	public void ShowPhaseIntroAfterTransition(GameCampaignPhase phase)
	{
		string body;
		switch (phase)
		{
		case GameCampaignPhase.UpgradeMission:
			body = "第二关：完成两次清运任务。\n第一次回站后可使用现金升级飞船。\n收集满10块垃圾后靠港，按回车发射。";
			break;
		case GameCampaignPhase.Endless:
			body = "第三关：无尽清运。\n空间站会持续补充轨道垃圾，尽可能多赚取现金并升级。";
			break;
		default:
			body = "教学关：用钩爪收集10块垃圾后返回空间站。";
			break;
		}
		this.showText.ShowTexts(new string[]
		{
			".",
			body
		}, delegate
		{
			this.BindStationLaunchControls(false);
		});
	}

	public void QueueStationDialogueForCampaign(bool levelCompleteAfterLaunch)
	{
		this._stationLevelCompletePending = levelCompleteAfterLaunch;
		this.BindStationLaunchControls(levelCompleteAfterLaunch);
		if (this.index == -1)
		{
			this.index++;
			this.showText.PlayConfigText("text/TestText", null);
			return;
		}
		if (CampaignManager.CurrentPhase == GameCampaignPhase.Tutorial)
		{
			string[] tutorialEnd = levelCompleteAfterLaunch ? new string[]
			{
				".",
				"教学任务完成！\n你已掌握移动、钩爪与回站。\n按回车键进入下一关。"
			} : new string[]
			{
				".",
				"欢迎，同志。\n用钩爪收集轨道上的10块垃圾，集满后飞回空间站靠港。"
			};
			this.showText.ShowTexts(tutorialEnd);
			return;
		}
		if (CampaignManager.CurrentPhase == GameCampaignPhase.UpgradeMission)
		{
			if (levelCompleteAfterLaunch)
			{
				this.showText.ShowTexts(new string[]
				{
					".",
					"两次作业已全部完成！\n按回车键进入无尽清运关卡。"
				});
				return;
			}
			int outing = CampaignManager.Level2CompletedOutings;
			if (outing == 1)
			{
				string[] upgradePages = new string[]
				{
					string.Format("\n第一次回站成功！\n按下空格键消耗5现金补充燃料\n按数字键1：轨道环绕速度升级（{0}$，当前Lv{1}/{2}）", 8, this._orbitSpeedUpgradeLevel, 8),
					string.Format("按数字键2：钩爪伸出/回收速度升级（{0}$，当前Lv{1}/{2}）", 8, this._grapnelSpeedUpgradeLevel, 8),
					string.Format("按数字键3：钩爪长度升级（{0}$，当前Lv{1}/{2}，增加伸出距离与待机半径）\n完成第二次作业后再回站", 10, this._grapnelLengthUpgradeLevel, 8)
				};
				string stationBody = "欢迎回来，同志。\n本轮可升级飞船，然后出发完成第二次清运。\n\n" + string.Join("\n", upgradePages) + "\n\n 赚取10金币";
				this.showText.ShowTexts(new string[]
				{
					".",
					stationBody
				});
				return;
			}
			this.showText.ShowTexts(new string[]
			{
				".",
				"欢迎回来，同志。\n第二次作业：再收集10块垃圾后返回空间站。"
			});
			return;
		}
		this.ShowEndlessStationDialogue();
	}

	private void ShowEndlessStationDialogue()
	{
		List<string[]> list = new List<string[]>
		{
			new string[]
			{
				"第一次来哈同志.\n我是空间站的小张。",
				"干一天幸苦了吧，来吃个月饼过个中秋。\n 这个空间站里你可以补充燃料或者启动推进器将飞船推向更高的轨道。"
			},
			new string[]
			{
				"欢迎回来，同志。"
			},
			new string[]
			{
				"你太棒了同志，等回地球了我一定要请你吃一顿。"
			},
			new string[]
			{
				"欢迎回来，你已经完成了这个月指标的一半了。"
			},
			new string[]
			{
				"哈哈，今年月球基地的模范标兵非你莫属。"
			},
			new string[]
			{
				"调动？你才来多久啊同志。\n要想想我们可是在为人类文明的环境做贡献呢！"
			},
			new string[]
			{
				"听吧新征程~号角吹响。。。。。。"
			},
			new string[]
			{
				"其实我们也很想出于人道主义去对美国宇航员进行救援。\n但是美国早在2011年就提出了沃尔沃条款来中止了我们和他们的一切太空技术合作",
				"太空的景色很美不是吗？但我还是会想念我老家黄土高原\n我来太空站时那里还是一望无尽的荒野呢，就像这太空一样。"
			}
		};
		if (this.index >= 7)
		{
			this.index = 0;
		}
		this.index++;
		string[] upgradePages = new string[]
		{
			string.Format("\n按下空格键消耗5现金补充燃料\n按数字键1：轨道环绕速度升级（{0}$，当前Lv{1}/{2}，每级约+6%环绕角速度）", 8, this._orbitSpeedUpgradeLevel, 8),
			string.Format("按数字键2：钩爪伸出/回收速度升级（{0}$，当前Lv{1}/{2}）", 8, this._grapnelSpeedUpgradeLevel, 8),
			string.Format("按数字键3：钩爪长度升级（{0}$，当前Lv{1}/{2}，增加伸出距离与待机半径）\n按下回车键发射", 10, this._grapnelLengthUpgradeLevel, 8)
		};
		string[] dialoguePages = list[this.index];
		string fullDialogue = string.Join("\n\n", dialoguePages);
		string fullTips = string.Join("\n", upgradePages);
		string stationBody = fullDialogue + "\n\n" + fullTips + "\n\n 赚取10金币";
		this.showText.ShowTexts(new string[]
		{
			".",
			stationBody
		});
	}

	private void BindStationLaunchControls(bool levelCompleteAfterLaunch)
	{
		this._launchInputDisposable?.Dispose();
		this._launchInputDisposable = new CompositeDisposable();
		this.startButton.gameObject.SetActive(true);
		string hint = levelCompleteAfterLaunch ? "\n按回车键完成本关并进入下一阶段" : CampaignManager.UpgradesEnabledAtStation ? "\n文本显示完毕，按下回车键Launch发射飞船\n（停靠期间可按空格补燃料，按1/2/3购买升级；方向键←→可翻阅前几屏对话）" : "\n文本显示完毕，按下回车键Launch发射飞船\n（教学关：按W/S变轨，鼠标左键使用钩爪）";
		this.showText.ShowTexts(new string[]
		{
			hint
		});
		(from a in Observable.EveryUpdate()
			where Input.GetKeyDown(KeyCode.Return)
			select a).Subscribe(delegate(long _)
		{
			if (this._stationLevelCompletePending && CampaignManager.Instance != null)
			{
				CampaignManager.Instance.TryCompleteLevelAfterStationDialogue();
				return;
			}
			this.CompleteStationLaunch();
		}).AddTo(this._launchInputDisposable);
		this.startButton.onClick.RemoveAllListeners();
		this.startButton.onClick.AddListener(delegate()
		{
			if (this._stationLevelCompletePending && CampaignManager.Instance != null)
			{
				CampaignManager.Instance.TryCompleteLevelAfterStationDialogue();
				return;
			}
			this.CompleteStationLaunch();
		});
	}

	private void HandleStationDocked()
	{
		this.startButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/主界面+控制台+结局（缺少标题）/控制台/开关off");
		base.transform.parent = this.spaceStation.transform;
		base.transform.DOScale(Vector3.zero, 0.1f);
		this.SetDockedVisuals(true);
		this.money += 10;
		this.garbageCount = 0;
		bool returnedFromOuting = this._cargoFullPendingDock;
		this._cargoFullPendingDock = false;
		if (!returnedFromOuting)
		{
			this.startButton.gameObject.SetActive(false);
			if (this.index == -1)
			{
				this.index++;
				this.showText.PlayConfigText("text/TestText", delegate
				{
					this.BindStationLaunchControls(false);
				});
			}
			else
			{
				this.BindStationLaunchControls(false);
			}
			return;
		}
		if (CampaignManager.IsEndlessMode())
		{
			MessageBroker.Default.Publish<GenerateGarbageMessage>(new GenerateGarbageMessage());
			this.QueueStationDialogueForCampaign(false);
		}
		else if (CampaignManager.Instance != null)
		{
			CampaignManager.Instance.OnStationDockedAfterFullCargo();
		}
		else
		{
			this.QueueStationDialogueForCampaign(false);
		}
		if (CampaignManager.UpgradesEnabledAtStation && (CampaignManager.IsEndlessMode() || CampaignManager.Level2CompletedOutings >= 1))
		{
			this.RegisterStationPurchaseInputs();
		}
		AudioManager.Instance.Play("金币");
	}

	// Token: 0x06000059 RID: 89 RVA: 0x000034FC File Offset: 0x000016FC
	private void OnEnable()
	{
		this.EnsureCampaignManager();
		this._enableLifetime?.Dispose();
		this._enableLifetime = new CompositeDisposable();
		this._stationPurchaseDisposable?.Dispose();
		this._stationPurchaseDisposable = null;
		this.orbitRadius = base.transform.position - this._centerPoint;
		this.ObserveEveryValueChanged((ShipController v) => v.isInSapceShip, FrameCountType.Update, false).Subscribe(delegate(bool docked)
		{
			if (docked)
			{
				this.HandleStationDocked();
			}
			else
			{
				this._launchInputDisposable?.Dispose();
				this._launchInputDisposable = null;
			}
		}).AddTo(this._enableLifetime);
		this.ObserveEveryValueChanged((ShipController v) => v.fuelValue, FrameCountType.Update, false).Subscribe(delegate(float _)
		{
			if (this.fuelValue <= 0f && !this.isOver)
			{
				this.isOver = true;
				MessageBroker.Default.Publish<GameOverMessage>(new GameOverMessage());
				(new string[1])[0] = ".";
				string[] strings = new string[]
				{
					"燃料耗尽，飞船坠毁"
				};
				this.showText.ShowTexts(strings);
			}
		}).AddTo(this._enableLifetime);
	}

	private void OnDisable()
	{
		this._stationPurchaseDisposable?.Dispose();
		this._stationPurchaseDisposable = null;
		this._launchInputDisposable?.Dispose();
		this._launchInputDisposable = null;
		this._enableLifetime?.Dispose();
		this._enableLifetime = null;
	}

	private void EnsureCampaignManager()
	{
		if (CampaignManager.Instance != null)
		{
			return;
		}
		GameObject go = new GameObject("CampaignManager");
		go.AddComponent<CampaignManager>();
	}

	private void Update()
	{
		this.moneyText.text = this.money.ToString() + "$";
		if (!this.isInSapceShip)
		{
			this.UpOrbit();
			this.DownOrbit();
			this.ComputeOrbitSpeed();
			this.ShipRotate();
			this.ApplyBlackHoleAttraction();
			this.ComputeFuelValue();
			this.ComputeCameraSize();
			this.CorrectAcceleration();
		}
		if (!this.isInSapceShip)
		{
			this.DrawCircle(this._centerPoint, this.orbitRadius.magnitude);
		}
		this.ShipFace();
	}

	private void ShipFace()
	{
		if (Input.GetKey(this.downKey))
		{
			Vector3 vector = base.transform.position - this._centerPoint;
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num - 180f));
		}
		if (Input.GetKey(this.upKey))
		{
			Vector3 vector2 = base.transform.position - this._centerPoint;
			float num2 = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
			base.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num2 - 360f));
		}
	}

	private void ChangeOrbit(KeyCode key, float changeCoefficient)
	{
		if (Input.GetKeyDown(key))
		{
			this.fog1.Play();
			this.fog2.Play();
			AudioManager.Instance.Play("喷气");
		}
		if (Input.GetKeyUp(key))
		{
			this.fog1.Stop();
			this.fog2.Stop();
			AudioManager.Instance.Stop("喷气");
		}
		if (Input.GetKey(key))
		{
			this.acceleration += this.leap * Time.deltaTime;
			if (this.acceleration > this.maxAcceleration)
			{
				this.acceleration = this.maxAcceleration;
			}
			this.orbitRadius = Vector3.MoveTowards(this.orbitRadius, this.orbitRadius.normalized * (this.orbitRadius.magnitude + changeCoefficient), Time.deltaTime * this.acceleration);
			return;
		}
		if (this.acceleration > 0f && !Input.GetKey((key == this.upKey) ? this.downKey : this.upKey))
		{
			this.acceleration = Mathf.Lerp(this.acceleration, 0f, Time.deltaTime);
			this.orbitRadius = Vector3.MoveTowards(this.orbitRadius, this.orbitRadius.normalized * (this.orbitRadius.magnitude + changeCoefficient), Time.deltaTime * this.acceleration);
		}
	}

	private void UpOrbit()
	{
		this.ChangeOrbit(this.upKey, this.changeOrbitCoefficient);
	}

	private void DownOrbit()
	{
		this.ChangeOrbit(this.downKey, -this.changeOrbitCoefficient);
	}

	private void ShipRotate()
	{
		float num = this.orbitSpeed * this.GetOrbitSpeedMultiplier();
		this.orbitRadius = Quaternion.AngleAxis(StaticData.OrbitAngularDirectionSign * Time.deltaTime * num, Vector3.forward) * this.orbitRadius;
		base.transform.position = this._centerPoint + this.orbitRadius;
	}

	private void ApplyBlackHoleAttraction()
	{
		if (this.isInSapceShip || !StaticData.isInHole || this.isOver)
		{
			return;
		}
		Vector3 hole = StaticData.blackHoleWorldPosition;
		Vector3 toHole = hole - base.transform.position;
		float dist = toHole.magnitude;
		if (dist < 1e-4f)
		{
			return;
		}
		float R = Mathf.Max(StaticData.blackHoleGravityRadius, 0.1f);
		float depth = Mathf.Clamp01(1f - dist / R);
		float step = StaticData.blackHolePullSpeed * Time.deltaTime * (0.2f + 0.8f * depth);
		Vector3 delta = toHole.normalized * Mathf.Min(step, dist * 0.98f);
		Vector3 newWorld = base.transform.position + delta;
		base.transform.position = newWorld;
		this.orbitRadius = newWorld - this._centerPoint;
	}

	public bool IsDockedInStation
	{
		get
		{
			return this.isInSapceShip;
		}
	}

	private float GetOrbitSpeedMultiplier()
	{
		return 1f + 0.06f * (float)Mathf.Min(this._orbitSpeedUpgradeLevel, 8);
	}

	private void RefreshGrapnelUpgrades()
	{
		if (this._grapnel == null)
		{
			return;
		}
		this._grapnel.SetLengthUpgradeLevel(this._grapnelLengthUpgradeLevel);
		this._grapnel.SetGrapnelAnimSpeedLevel(this._grapnelSpeedUpgradeLevel);
	}

	private void RegisterStationPurchaseInputs()
	{
		this._stationPurchaseDisposable?.Dispose();
		this._stationPurchaseDisposable = new CompositeDisposable();
		(from a in Observable.EveryUpdate()
			where Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3)
			select a).Subscribe(delegate(long _)
		{
			if (!this.isInSapceShip)
			{
				return;
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (this.money >= 5)
				{
					this.fuelValue += 30f;
					this.money -= 5;
					AudioManager.Instance.Play("补油");
					return;
				}
				this.showText.ShowTexts(new string[]
				{
					"现金不足，无法补充燃料（需要5$）\n"
				});
				return;
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				this.TryBuyOrbitSpeedUpgrade();
				return;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				this.TryBuyGrapnelSpeedUpgrade();
				return;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				this.TryBuyGrapnelLengthUpgrade();
			}
		}).AddTo(this._stationPurchaseDisposable);
	}

	private void TryBuyOrbitSpeedUpgrade()
	{
		if (this._orbitSpeedUpgradeLevel >= 8)
		{
			this.showText.ShowTexts(new string[]
			{
				"轨道环绕速度已达最高等级。\n"
			});
			return;
		}
		if (this.money < 8)
		{
			this.showText.ShowTexts(new string[]
			{
				"现金不足，无法购买轨道速度升级（需要8$）。\n"
			});
			return;
		}
		this.money -= 8;
		this._orbitSpeedUpgradeLevel++;
		AudioManager.Instance.Play("金币");
		this.showText.ShowTexts(new string[]
		{
			string.Format("已升级轨道环绕速度，当前等级：{0}/8。\n", this._orbitSpeedUpgradeLevel)
		});
	}

	private void TryBuyGrapnelSpeedUpgrade()
	{
		if (this._grapnelSpeedUpgradeLevel >= 8)
		{
			this.showText.ShowTexts(new string[]
			{
				"钩爪速度已达最高等级。\n"
			});
			return;
		}
		if (this.money < 8)
		{
			this.showText.ShowTexts(new string[]
			{
				"现金不足，无法购买钩爪速度升级（需要8$）。\n"
			});
			return;
		}
		this.money -= 8;
		this._grapnelSpeedUpgradeLevel++;
		this.RefreshGrapnelUpgrades();
		AudioManager.Instance.Play("金币");
		this.showText.ShowTexts(new string[]
		{
			string.Format("已升级钩爪伸出/回收速度，当前等级：{0}/8。\n", this._grapnelSpeedUpgradeLevel)
		});
	}

	private void TryBuyGrapnelLengthUpgrade()
	{
		if (this._grapnelLengthUpgradeLevel >= 8)
		{
			this.showText.ShowTexts(new string[]
			{
				"钩爪长度已达最高等级。\n"
			});
			return;
		}
		if (this.money < 10)
		{
			this.showText.ShowTexts(new string[]
			{
				"现金不足，无法购买钩爪长度升级（需要10$）。\n"
			});
			return;
		}
		this.money -= 10;
		this._grapnelLengthUpgradeLevel++;
		this.RefreshGrapnelUpgrades();
		AudioManager.Instance.Play("金币");
		this.showText.ShowTexts(new string[]
		{
			string.Format("已升级钩爪长度，当前等级：{0}/8。\n", this._grapnelLengthUpgradeLevel)
		});
	}

	private void ComputeOrbitSpeed()
	{
		this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / this.orbitRadius.magnitude);
		if (StaticData.isInHole)
		{
			this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient * StaticData.coefficient / this.orbitRadius.magnitude);
		}
	}

	private void ComputeFuelValue()
	{
		if (!Input.GetKey(this.upKey) && !Input.GetKey(this.downKey))
		{
			this.fuelConsumptionRate = 0f;
			return;
		}
		this.fuelConsumptionRate = Mathf.Sqrt(this.fuelConsumptionCoefficient / this.orbitRadius.magnitude);
		this.fuelValue -= this.fuelConsumptionRate * Time.deltaTime;
	}

	private void ComputeCameraSize()
	{
		float endValue = this.orbitRadius.magnitude * 1.2f;
		Camera.main.DOOrthoSize(endValue, 0.05f).SetEase(Ease.InOutSine);
		Vector3 vector = (this.plant.transform.position + base.transform.position) / 2f;
		Vector3 endValue2 = new Vector3(vector.x, vector.y, -10f);
		Camera.main.transform.DOMove(endValue2, 0.05f, false).SetEase(Ease.InOutSine);
	}

	private void CorrectAcceleration()
	{
		if (this.acceleration < 0f)
		{
			this.acceleration = 0f;
		}
	}

	private void DrawCircle(Vector3 center, float radius)
	{
		LineRenderer component = base.GetComponent<LineRenderer>();
		component.loop = true;
		component.startWidth = 0.1f;
		component.endWidth = 0.1f;
		component.positionCount = 360;
		for (int i = 0; i < component.positionCount; i++)
		{
			float f = (float)i * 3.1415927f * 2f / 360f;
			float x = Mathf.Cos(f) * radius + center.x;
			float y = Mathf.Sin(f) * radius + center.y;
			component.SetPosition(i, new Vector3(x, y, 0f));
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Garbage"))
		{
			if (this.isInSapceShip) return;
			if (other.transform.IsChildOf(base.transform))
			{
				return;
			}
			MessageBroker.Default.Publish<GameOverMessage>(new GameOverMessage());
		}
		if (other.CompareTag("SpaceStation") && this.garbageCount >= this.garbageCountMax && !this.isInSapceShip)
		{
			this.isInSapceShip = true;
		}
	}

	[SerializeField]
	private KeyCode upKey;

	[SerializeField]
	private KeyCode downKey;

	[Header("轨道半径")]
	[SerializeField]
	private Vector3 orbitRadius;

	[Header("轨道速度")]
	[SerializeField]
	private float orbitSpeed;

	[Header("加速度")]
	[SerializeField]
	private float acceleration;

	[Header("最大加速度")]
	[SerializeField]
	private float maxAcceleration;

	[Header("跃度")]
	[SerializeField]
	private float leap;

	[Header("星球")]
	[SerializeField]
	private Plant plant;

	[Header("燃料值")]
	[SerializeField]
	public float fuelValue;

	[Header("燃料值上限")]
	[SerializeField]
	public float fuelValueMax;

	[Header("燃料消耗率")]
	[SerializeField]
	private float fuelConsumptionRate;

	[Header("燃料消耗系数")]
	[SerializeField]
	private float fuelConsumptionCoefficient;

	[Header("变轨系数")]
	[SerializeField]
	private float changeOrbitCoefficient = 0.1f;

	public int money;

	public ParticleSystem fog1;

	public ParticleSystem fog2;

	private Vector3 _centerPoint;

	public int garbageCount;

	public int garbageCountMax = 10;

	public ShowText showText;

	private bool isUp = true;

	public SpaceStation spaceStation;

	public Button startButton;

	public bool isOver;

	public TextMeshProUGUI moneyText;

	private int index;

	private bool isInSapceShip;

	private GrapnelController _grapnel;

	private int _orbitSpeedUpgradeLevel;

	private int _grapnelSpeedUpgradeLevel;

	private int _grapnelLengthUpgradeLevel;

	private bool _cargoFullPendingDock;

	private bool _stationLevelCompletePending;

	private CompositeDisposable _launchInputDisposable;
}
