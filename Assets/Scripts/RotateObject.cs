using System;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

// Token: 0x02000017 RID: 23
public class RotateObject : MonoBehaviour
{
	// Token: 0x0600004D RID: 77 RVA: 0x000031EF File Offset: 0x000013EF
	private void Update()
	{
		if (base.gameObject.CompareTag("SpaceStation"))
		{
			this.HoleRotate();
			this.ShipFace();
			return;
		}
		this.ComputeOrbitSpeed();
		this.ShipRotate();
		this.ShipFace();
	}

	// Token: 0x0600004E RID: 78 RVA: 0x00003224 File Offset: 0x00001424
	private void HoleRotate()
	{
		if (Random.Range(0, 100) < 1)
		{
			this._targetOrbitRadius = Random.Range(this.minOrbitRadius, this.maxOrbitRadius);
		}
		this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / this.orbitRadius.magnitude);
		float d = Mathf.Lerp(this.orbitRadius.magnitude, this._targetOrbitRadius, Time.deltaTime * this.lerpSpeed);
		this.orbitRadius = (Quaternion.AngleAxis(Time.deltaTime * this.orbitSpeed * 2f, Vector3.forward) * this.orbitRadius).normalized * d;
		base.transform.position = this._centerPoint + this.orbitRadius;
	}

	// Token: 0x0600004F RID: 79 RVA: 0x000032EF File Offset: 0x000014EF
	private void ShipFace()
	{
		base.transform.rotation = Quaternion.LookRotation(Vector3.forward, this._centerPoint - base.transform.position);
	}

	// Token: 0x06000050 RID: 80 RVA: 0x0000331C File Offset: 0x0000151C
	private void Start()
	{
		this.plant = GameObject.Find("Planet").GetComponent<Plant>();
		this.orbitRadius = base.transform.position - this._centerPoint;
		this._targetOrbitRadius = this.orbitRadius.magnitude;
		Observable.Timer(TimeSpan.FromSeconds(10.0)).Repeat<long>().Subscribe(delegate(long _)
		{
			this._targetOrbitRadius = Random.Range(this.minOrbitRadius, this.maxOrbitRadius);
		}).AddTo(this);
	}

	// Token: 0x06000051 RID: 81 RVA: 0x0000339C File Offset: 0x0000159C
	private void ShipRotate()
	{
		this.orbitRadius = Quaternion.AngleAxis(Time.deltaTime * this.orbitSpeed * 2f, Vector3.forward) * this.orbitRadius;
		base.transform.position = this._centerPoint + this.orbitRadius;
	}

	// Token: 0x06000052 RID: 82 RVA: 0x000033F2 File Offset: 0x000015F2
	private void ComputeOrbitSpeed()
	{
		this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / this.orbitRadius.magnitude);
	}

	// Token: 0x04000035 RID: 53
	private Plant plant;

	// Token: 0x04000036 RID: 54
	private Vector3 _centerPoint;

	// Token: 0x04000037 RID: 55
	private Vector3 orbitRadius;

	// Token: 0x04000038 RID: 56
	private float orbitSpeed;

	// Token: 0x04000039 RID: 57
	public float minOrbitRadius = 10f;

	// Token: 0x0400003A RID: 58
	public float maxOrbitRadius = 40f;

	// Token: 0x0400003B RID: 59
	public float lerpSpeed = 0.1f;

	// Token: 0x0400003C RID: 60
	private float _targetOrbitRadius;
}
