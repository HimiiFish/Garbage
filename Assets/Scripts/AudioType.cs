using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioType
{
	[HideInInspector]
	public AudioSource source;

	public AudioClip clip;

	public AudioMixerGroup group;

	public string name;

	[Range(0f, 1f)]
	public float volume;

	[Range(0.1f, 5f)]
	public float pitch;

	public bool loop;
}
