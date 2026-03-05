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

// Token: 0x02000019 RID: 25
public class ShipController : MonoBehaviour
{
	// Token: 0x06000058 RID: 88 RVA: 0x000034BC File Offset: 0x000016BC
	private void Start()
	{
		this._centerPoint = new Vector3(0f, 0f, 0f);
		MessageBroker.Default.Receive<GarbageCollectedMessage>().Subscribe(delegate(GarbageCollectedMessage _)
		{
			this.garbageCount++;
			if (this.garbageCount >= this.garbageCountMax)
			{
				this.garbageCount = this.garbageCountMax;
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
			array[0] = list[num][0];
			this.showText.ShowTexts(array);
		}).AddTo(this);
	}

	// Token: 0x06000059 RID: 89 RVA: 0x000034FC File Offset: 0x000016FC
	private void OnEnable()
	{
		this.orbitRadius = base.transform.position - this._centerPoint;
		this.ObserveEveryValueChanged((ShipController v) => v.isInSapceShip, FrameCountType.Update, false).Subscribe(delegate(bool _)
		{
			if (this.isInSapceShip)
			{
				this.startButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/主界面+控制台+结局（缺少标题）/控制台/开关off");
				base.transform.parent = this.spaceStation.transform;
				base.transform.DOScale(Vector3.zero, 0.1f);
				this.money += 10;
				this.garbageCount = 0;
				MessageBroker.Default.Publish<GenerateGarbageMessage>(new GenerateGarbageMessage());
				if (this.index >= 7)
				{
					this.index = 0;
				}
				string[] array = new string[]
				{
					"."
				};
				List<string[]> list = new List<string[]>
				{
					new string[]
					{
						"第一次来哈同志.\n我是空间站的小张。\n 干一天幸苦了吧，来吃个月饼过个中秋。\n 这个空间站里你可以补充燃料或者启动推进器将飞船推向更高的轨道。"
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
						"其实我们也很想出于人道主义去对美国宇航员进行救援。\n但是美国早在2011年就提出了沃尔沃条款来中止了我们和他们的一切太空技术合作\n太空的景色很美不是吗？但我还是会想念我老家黄土高原\n我来太空站时那里还是一望无尽的荒野呢，就像这太空一样。"
					}
				};
				this.index++;
				string[] array2 = new string[]
				{
					"\n按下空格键消耗5现金补充燃料\n按下回车键发射"
				};
				string[] array3 = new string[]
				{
					"",
					"",
					"",
					"\n 赚取10金币"
				};
				array3[0] = array[0];
				array3[1] = list[this.index][0];
				array3[2] = array2[0];
				this.showText.ShowTexts(array3);
				AudioManager.Instance.Play("金币");
				(from a in Observable.EveryUpdate()
				where Input.GetKeyDown(KeyCode.Space)
				select a).Subscribe(delegate(long _)
				{
					if (this.money >= 5)
					{
						this.fuelValue += 30f;
						this.money -= 5;
						AudioManager.Instance.Play("补油");
					}
					if (this.money < 5)
					{
						(new string[1])[0] = ".";
						string[] strings = new string[]
						{
							"燃料不足，无法补充燃料\n"
						};
						this.showText.ShowTexts(strings);
					}
				});
				this.startButton.onClick.AddListener(delegate()
				{
					this.startButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/主界面+控制台+结局（缺少标题）/控制台/开关on");
					base.transform.parent = null;
					base.transform.position = this.spaceStation.transform.position;
					base.transform.DOScale(1f, 0.1f);
					this.isInSapceShip = false;
				});
			}
		});
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
		});
	}

	// Token: 0x0600005A RID: 90 RVA: 0x00003598 File Offset: 0x00001798
	private void Update()
	{
		this.moneyText.text = this.money.ToString() + "$";
		if (!this.isInSapceShip)
		{
			this.UpOrbit();
			this.DownOrbit();
			this.ComputeOrbitSpeed();
			this.ShipRotate();
			this.ComputeFuelValue();
			this.ComputeCameraSize();
			this.CorrectAcceleration();
		}
		this.DrawCircle(this._centerPoint, this.orbitRadius.magnitude);
		this.ShipFace();
	}

	// Token: 0x0600005B RID: 91 RVA: 0x00003614 File Offset: 0x00001814
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

	// Token: 0x0600005C RID: 92 RVA: 0x000036E8 File Offset: 0x000018E8
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

	// Token: 0x0600005D RID: 93 RVA: 0x00003845 File Offset: 0x00001A45
	private void UpOrbit()
	{
		this.ChangeOrbit(this.upKey, this.changeOrbitCoefficient);
	}

	// Token: 0x0600005E RID: 94 RVA: 0x00003859 File Offset: 0x00001A59
	private void DownOrbit()
	{
		this.ChangeOrbit(this.downKey, -this.changeOrbitCoefficient);
	}

	// Token: 0x0600005F RID: 95 RVA: 0x00003870 File Offset: 0x00001A70
	private void ShipRotate()
	{
		this.orbitRadius = Quaternion.AngleAxis(Time.deltaTime * this.orbitSpeed, Vector3.forward) * this.orbitRadius;
		base.transform.position = this._centerPoint + this.orbitRadius;
	}

	// Token: 0x06000060 RID: 96 RVA: 0x000038C0 File Offset: 0x00001AC0
	private void ComputeOrbitSpeed()
	{
		this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / this.orbitRadius.magnitude);
		if (StaticData.isInHole)
		{
			this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient * StaticData.coefficient / this.orbitRadius.magnitude);
		}
	}

	// Token: 0x06000061 RID: 97 RVA: 0x00003920 File Offset: 0x00001B20
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

	// Token: 0x06000062 RID: 98 RVA: 0x0000398C File Offset: 0x00001B8C
	private void ComputeCameraSize()
	{
		float endValue = this.orbitRadius.magnitude * 1.2f;
		Camera.main.DOOrthoSize(endValue, 0.05f).SetEase(Ease.InOutSine);
		Vector3 vector = (this.plant.transform.position + base.transform.position) / 2f;
		Vector3 endValue2 = new Vector3(vector.x, vector.y, -10f);
		Camera.main.transform.DOMove(endValue2, 0.05f, false).SetEase(Ease.InOutSine);
	}

	// Token: 0x06000063 RID: 99 RVA: 0x00003A22 File Offset: 0x00001C22
	private void CorrectAcceleration()
	{
		if (this.acceleration < 0f)
		{
			this.acceleration = 0f;
		}
	}

	// Token: 0x06000064 RID: 100 RVA: 0x00003A3C File Offset: 0x00001C3C
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

	// Token: 0x06000065 RID: 101 RVA: 0x00003AD0 File Offset: 0x00001CD0
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Garbage"))
		{
			MessageBroker.Default.Publish<GameOverMessage>(new GameOverMessage());
		}
		if (other.CompareTag("SpaceStation") && this.garbageCount >= this.garbageCountMax && !this.isInSapceShip)
		{
			this.isInSapceShip = true;
		}
	}

	// Token: 0x0400003E RID: 62
	[SerializeField]
	private KeyCode upKey;

	// Token: 0x0400003F RID: 63
	[SerializeField]
	private KeyCode downKey;

	// Token: 0x04000040 RID: 64
	[Header("轨道半径")]
	[SerializeField]
	private Vector3 orbitRadius;

	// Token: 0x04000041 RID: 65
	[Header("轨道速度")]
	[SerializeField]
	private float orbitSpeed;

	// Token: 0x04000042 RID: 66
	[Header("加速度")]
	[SerializeField]
	private float acceleration;

	// Token: 0x04000043 RID: 67
	[Header("最大加速度")]
	[SerializeField]
	private float maxAcceleration;

	// Token: 0x04000044 RID: 68
	[Header("跃度")]
	[SerializeField]
	private float leap;

	// Token: 0x04000045 RID: 69
	[Header("星球")]
	[SerializeField]
	private Plant plant;

	// Token: 0x04000046 RID: 70
	[Header("燃料值")]
	[SerializeField]
	public float fuelValue;

	// Token: 0x04000047 RID: 71
	[Header("燃料值上限")]
	[SerializeField]
	public float fuelValueMax;

	// Token: 0x04000048 RID: 72
	[Header("燃料消耗率")]
	[SerializeField]
	private float fuelConsumptionRate;

	// Token: 0x04000049 RID: 73
	[Header("燃料消耗系数")]
	[SerializeField]
	private float fuelConsumptionCoefficient;

	// Token: 0x0400004A RID: 74
	[Header("变轨系数")]
	[SerializeField]
	private float changeOrbitCoefficient = 0.1f;

	// Token: 0x0400004B RID: 75
	public int money;

	// Token: 0x0400004C RID: 76
	public ParticleSystem fog1;

	// Token: 0x0400004D RID: 77
	public ParticleSystem fog2;

	// Token: 0x0400004E RID: 78
	private Vector3 _centerPoint;

	// Token: 0x0400004F RID: 79
	public int garbageCount;

	// Token: 0x04000050 RID: 80
	public int garbageCountMax = 10;

	// Token: 0x04000051 RID: 81
	public ShowText showText;

	// Token: 0x04000052 RID: 82
	private bool isUp = true;

	// Token: 0x04000053 RID: 83
	public SpaceStation spaceStation;

	// Token: 0x04000054 RID: 84
	public Button startButton;

	// Token: 0x04000055 RID: 85
	public bool isOver;

	// Token: 0x04000056 RID: 86
	public TextMeshProUGUI moneyText;

	// Token: 0x04000057 RID: 87
	private int index;

	// Token: 0x04000058 RID: 88
	private bool isInSapceShip;
}
