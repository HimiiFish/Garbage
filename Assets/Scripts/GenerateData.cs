using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000010 RID: 16
[CreateAssetMenu(fileName = "GenerateData", menuName = "GenerateData")]
public class GenerateData : ScriptableObject
{
	[Tooltip("开启：场上少于 maxGarbageCount 时持续补满（原逻辑）。关闭：本关累计生成达到上限后不再生成新垃圾。")]
	public bool allowInfiniteRefill = true;

	[Min(0)]
	[Tooltip("仅当「允许无限补满」关闭时生效：本关最多实例化多少次垃圾（含已被清理的）。为 0 时表示取各列表 maxGarbageCount 之和作为总配额（约等于只刷一波）。")]
	public int maxLifetimeSpawns;

	[Header("垃圾生成节奏（非每帧）")]
	[Min(0.05f)]
	[Tooltip("每隔多少秒检查一次是否需要补垃圾；仅在当前数量 < maxGarbageCount 时尝试生成。")]
	public float spawnCheckInterval = 0.45f;

	[Header("垃圾轨道：随机高度 + 缓慢向低轨沉降")]
	[Tooltip("生成时绕行星随机轨道半径下限（世界单位）。")]
	public float garbageSpawnRadiusMin = 22f;

	[Tooltip("生成时绕行星随机轨道半径上限。")]
	public float garbageSpawnRadiusMax = 34f;

	[Tooltip("轨道沉降可到达的最小半径（低轨），不低于行星安全距离即可。")]
	public float garbageDecayRadiusFloor = 11f;

	[Tooltip("垃圾沿径向向行星缓慢靠近的最小速度（世界单位/秒）。")]
	public float garbageDecaySpeedMin = 0.08f;

	[Tooltip("垃圾沿径向向行星缓慢靠近的最大速度（世界单位/秒）。")]
	public float garbageDecaySpeedMax = 0.22f;

	[Header("垃圾个体：质量与缩放（同一随机因子：质量大则体型大，影响钩爪回收）")]
	public float garbageMassMin = 0.55f;

	public float garbageMassMax = 2.85f;

	public float garbageUniformScaleMin = 0.65f;

	public float garbageUniformScaleMax = 1.35f;

	[Header("垃圾拾取现金（与质量/体型同向：最轻对应最少，最重对应最多）")]
	public int garbagePickupMoneyMin = 1;

	public int garbagePickupMoneyMax = 6;

	[Header("垃圾公转角速度（原逻辑为 √(g/r)×2；此处用 global 整体压低，再按体型 t 在轻/重间插值）")]
	[Tooltip("乘在旧公式 2 倍基础上的总缩放，建议 0.35～0.55")]
	public float garbageOrbitGlobalMul = 0.42f;

	[Tooltip("最轻垃圾相对 global 的额外倍率（越大转得越快）")]
	public float garbageOrbitLightMul = 1f;

	[Tooltip("最重垃圾相对 global 的额外倍率（应小于 Light，越大越慢）")]
	public float garbageOrbitHeavyMul = 0.38f;

	[Header("垃圾与飞船轨道关系")]
	[Tooltip("设计中飞船可达的最大轨道半径（世界单位）。生成垃圾时实际下限 = max(本资源 garbageSpawnRadiusMin, 该值 + 下方间距)。")]
	public float shipMaxOrbitRadiusForGarbageLayout = 15f;

	[Tooltip("垃圾生成半径下限相对「飞船最大轨道参考」的额外间距，保证垃圾环整体高于飞船活动带。")]
	public float garbageSpawnMinMarginAboveShipOrbit = 2f;

	public List<GenerateList> generateLists;
}
