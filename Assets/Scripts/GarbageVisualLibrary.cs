using UnityEngine;

[CreateAssetMenu(fileName = "GarbageVisualLibrary", menuName = "Garbage/Garbage Visual Library")]
public class GarbageVisualLibrary : ScriptableObject
{
	[Tooltip("生成的垃圾只会从这些 Sprite 中随机；请在 Inspector 中拖入需要的贴图")]
	public Sprite[] wasteSprites;
}
