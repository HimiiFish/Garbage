using System;
using System.Collections;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

// Token: 0x02000013 RID: 19
public class GrapnelController : MonoBehaviour
{
	// Token: 0x0600003E RID: 62 RVA: 0x00002F10 File Offset: 0x00001110
	private void Awake()
	{
		this.lineRenderer = base.GetComponent<LineRenderer>();
	}

	// Token: 0x0600003F RID: 63 RVA: 0x00002F1E File Offset: 0x0000111E
	private void Start()
	{
		this._isCrawl = false;
	}

	// Token: 0x06000040 RID: 64 RVA: 0x00002F28 File Offset: 0x00001128
	private void Update()
	{
		this._centerPoint = this.grapnelPoint.transform.position;
		base.transform.rotation = Quaternion.LookRotation(Vector3.forward, this._centerPoint - base.transform.position);
		if (!this._isCrawl)
		{
			this.GrapnelRotate();
		}
		if (Input.GetMouseButtonDown(0) && !this._isCrawl)
		{
			this._isCrawl = true;
			Vector3 a = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			this.GrapnelLogic((a - this._centerPoint).normalized, 10);
		}
		this.lineRenderer.SetPosition(0, this._centerPoint);
		this.lineRenderer.SetPosition(1, base.transform.position);
	}

	// Token: 0x06000041 RID: 65 RVA: 0x00002FF0 File Offset: 0x000011F0
	private void GrapnelRotate()
	{
		Vector3 normalized = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - this._centerPoint).normalized;
		base.transform.position = this._centerPoint + normalized * this.grapnelRadius.magnitude;
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00003048 File Offset: 0x00001248
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

	// Token: 0x06000043 RID: 67 RVA: 0x000030DB File Offset: 0x000012DB
	private void GrapnelLogic(Vector3 targetVector, int distance)
	{
		base.StartCoroutine(this.GrapnelLogicCoroutine(targetVector, distance));
	}

	// Token: 0x06000044 RID: 68 RVA: 0x000030EC File Offset: 0x000012EC
	private IEnumerator GrapnelLogicCoroutine(Vector3 targetVector, int distance)
	{
		AudioManager.Instance.Play("狗爪伸出去");
		float time = 0f;
		Vector3 fixedDistanceVector = targetVector * (float)distance;
		while (time < 1f && Vector3.Distance(base.transform.position, this._centerPoint + fixedDistanceVector) > 0.5f)
		{
			time += Time.deltaTime / 2f;
			base.transform.position = Vector3.Lerp(base.transform.position, this._centerPoint + fixedDistanceVector, time);
			yield return null;
		}
		float recoveryTime = 0f;
		AudioManager.Instance.Stop("狗爪伸出去");
		AudioManager.Instance.Play("狗爪伸回来");
		base.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/狗爪关");
		while (recoveryTime < 1f && Vector3.Distance(base.transform.position, this._centerPoint) > 1f)
		{
			if (this._isGrab)
			{
				recoveryTime += Time.deltaTime / 200f;
			}
			else
			{
				recoveryTime += Time.deltaTime / 50f;
			}
			base.transform.position = Vector3.Lerp(base.transform.position, this._centerPoint, recoveryTime);
			yield return null;
		}
		Vector3 normalized = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - this._centerPoint).normalized;
		base.transform.position = this._centerPoint + normalized * this.grapnelRadius.magnitude;
		this._isCrawl = false;
		this._isGrab = false;
		base.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/狗爪开");
		MessageBroker.Default.Publish<GarbageCollectedMessage>(new GarbageCollectedMessage());
		IEnumerator enumerator = base.transform.GetEnumerator();
		while (enumerator.MoveNext())
		{
			object obj = enumerator.Current;
			Transform transform = (Transform)obj;
			if (transform.CompareTag("Garbage"))
			{
				Object.Destroy(transform.gameObject);
			}
		}
		yield break;
		yield break;
	}

	// Token: 0x06000045 RID: 69 RVA: 0x0000310C File Offset: 0x0000130C
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Garbage") && this._isCrawl)
		{
			this._isGrab = true;
			other.transform.parent = base.transform;
			other.GetComponent<RotateObject>().enabled = false;
			AudioManager.Instance.Play("抓取");
			other.transform.DOPunchScale(new Vector3(0.6f, 0.6f, 0.6f), 0.2f, 10, 1f);
			Object.Destroy(other.GetComponent<Collider2D>());
		}
	}

	// Token: 0x0400002C RID: 44
	private Vector3 _centerPoint;

	// Token: 0x0400002D RID: 45
	private bool _isCrawl;

	// Token: 0x0400002E RID: 46
	private bool _isGrab;

	// Token: 0x0400002F RID: 47
	[SerializeField]
	private Vector3 grapnelRadius;

	// Token: 0x04000030 RID: 48
	[SerializeField]
	private float grapnelSpeed;

	// Token: 0x04000031 RID: 49
	[SerializeField]
	private GameObject grapnelPoint;

	// Token: 0x04000032 RID: 50
	private LineRenderer lineRenderer;

	// Token: 0x04000033 RID: 51
	public ShowText showText;
}
