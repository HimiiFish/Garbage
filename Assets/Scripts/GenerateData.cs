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

	[Header("垃圾个体：质量与缩放（影响钩爪回收）")]
	public float garbageMassMin = 0.55f;

	public float garbageMassMax = 2.85f;

	public float garbageUniformScaleMin = 0.65f;

	public float garbageUniformScaleMax = 1.35f;

	public List<GenerateList> generateLists;
}
