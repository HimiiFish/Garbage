using System;
using UnityEngine;
using UnityEngine.Audio;

// Token: 0x02000009 RID: 9
[Serializable]
public class AudioType
{
	// Token: 0x04000011 RID: 17
	[HideInInspector]
	public AudioSource source;

	// Token: 0x04000012 RID: 18
	public AudioClip clip;

	// Token: 0x04000013 RID: 19
	public AudioMixerGroup group;

	// Token: 0x04000014 RID: 20
	public string name;

	// Token: 0x04000015 RID: 21
	[Range(0f, 1f)]
	public float volume;

	// Token: 0x04000016 RID: 22
	[Range(0.1f, 5f)]
	public float pitch;

	// Token: 0x04000017 RID: 23
	public bool loop;
}
