using System;
using UnityEngine;

public class Plant : MonoBehaviour
{
	private void Start()
	{
		AudioManager.Instance.Play("BGM1");
	}

	private void Update()
	{
	}

	[Header("引力系数")]
	[SerializeField]
	public float gravityCoefficient;
}
