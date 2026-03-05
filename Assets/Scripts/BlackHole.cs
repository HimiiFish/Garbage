using System;
using UnityEngine;
using Random = UnityEngine.Random;

// Token: 0x0200000B RID: 11
public class BlackHole : MonoBehaviour
{
	// Token: 0x06000026 RID: 38 RVA: 0x000028FC File Offset: 0x00000AFC
	private void Start()
	{
		base.GetComponent<CircleCollider2D>().radius = this.gravityRadius;
		this._centerPoint = new Vector3(0f, 0f, 0f);
		this._targetOrbitRadius = this.minOrbitRadius;
		this._orbitRadius = new Vector3(Random.Range(this.minOrbitRadius, this.maxOrbitRadius), 0f, 0f);
	}

	// Token: 0x06000027 RID: 39 RVA: 0x00002966 File Offset: 0x00000B66
	private void Update()
	{
		this.ComputeOrbitSpeed();
		this.HoleRotate();
	}

	// Token: 0x06000028 RID: 40 RVA: 0x00002974 File Offset: 0x00000B74
	private void HoleRotate()
	{
		if (Random.Range(0, 100) < 1)
		{
			this._targetOrbitRadius = Random.Range(this.minOrbitRadius, this.maxOrbitRadius);
		}
		float d = Mathf.Lerp(this._orbitRadius.magnitude, this._targetOrbitRadius, Time.deltaTime * this._lerpSpeed);
		this._orbitRadius = (Quaternion.AngleAxis(Time.deltaTime * this._orbitSpeed, Vector3.forward) * this._orbitRadius).normalized * d;
		base.transform.position = this._centerPoint + this._orbitRadius;
	}

	// Token: 0x06000029 RID: 41 RVA: 0x00002A17 File Offset: 0x00000C17
	private void ComputeOrbitSpeed()
	{
		this._orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient * 3f / this._orbitRadius.magnitude);
	}

	// Token: 0x0600002A RID: 42 RVA: 0x00002A41 File Offset: 0x00000C41
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Ship"))
		{
			StaticData.isInHole = true;
		}
	}

	// Token: 0x0600002B RID: 43 RVA: 0x00002A58 File Offset: 0x00000C58
	private void OnTriggerStay2D(Collider2D other)
	{
		if (other.CompareTag("Ship"))
		{
			StaticData.coefficient = (other.transform.position - base.transform.position).magnitude / this.gravityRadius;
		}
	}

	// Token: 0x0600002C RID: 44 RVA: 0x00002AA1 File Offset: 0x00000CA1
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Ship"))
		{
			StaticData.isInHole = false;
		}
	}

	// Token: 0x0400001B RID: 27
	[Header("运动范围半径")]
	[SerializeField]
	private float minOrbitRadius;

	// Token: 0x0400001C RID: 28
	[SerializeField]
	private float maxOrbitRadius;

	// Token: 0x0400001D RID: 29
	[Header("黑洞引力范围")]
	[SerializeField]
	private float gravityRadius;

	// Token: 0x0400001E RID: 30
	public Plant plant;

	// Token: 0x0400001F RID: 31
	private Vector3 _centerPoint;

	// Token: 0x04000020 RID: 32
	private Vector3 _orbitRadius;

	// Token: 0x04000021 RID: 33
	private float _targetOrbitRadius;

	// Token: 0x04000022 RID: 34
	private float _orbitSpeed;

	// Token: 0x04000023 RID: 35
	private readonly float _lerpSpeed = 0.1f;

	// Token: 0x04000024 RID: 36
	private ShipController _ship;
}
