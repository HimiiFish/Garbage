using UnityEngine;

public class DeadLine : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		other.CompareTag("Ship");
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		CircleCollider2D circle = base.GetComponent<CircleCollider2D>();
		if (circle == null)
		{
			return;
		}
		Vector3 worldCenter = base.transform.TransformPoint(circle.offset);
		float r = circle.radius * Mathf.Max(Mathf.Abs(base.transform.lossyScale.x), Mathf.Abs(base.transform.lossyScale.y));
		UnityEditor.Handles.color = new Color(1f, 0.92f, 0.1f, 0.65f);
		UnityEditor.Handles.DrawWireDisc(worldCenter, Vector3.forward, Mathf.Max(0.01f, r));
		UnityEditor.Handles.color = new Color(1f, 0.85f, 0.2f, 0.85f);
		UnityEditor.Handles.Label(worldCenter + Vector3.down * (r + 0.25f), "DeadLine 碰撞圆");
	}
#endif
}
