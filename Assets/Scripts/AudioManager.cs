using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// Token: 0x02000008 RID: 8
public class AudioManager : MonoBehaviour
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000014 RID: 20 RVA: 0x000022D8 File Offset: 0x000004D8
	public static AudioManager Instance
	{
		get
		{
			if (AudioManager._instance == null)
			{
				AudioManager._instance = Object.FindObjectOfType<AudioManager>();
				if (AudioManager._instance == null)
				{
					AudioManager._instance = new GameObject(typeof(AudioManager).Name).AddComponent<AudioManager>();
				}
			}
			return AudioManager._instance;
		}
	}

	// Token: 0x06000015 RID: 21 RVA: 0x0000232C File Offset: 0x0000052C
	private void Awake()
	{
		if (AudioManager._instance == null)
		{
			AudioManager._instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		foreach (global::AudioType audioType in this.audioTypes)
		{
			audioType.source = base.gameObject.AddComponent<AudioSource>();
			audioType.source.clip = audioType.clip;
			audioType.source.name = audioType.name.ToString();
			audioType.source.volume = audioType.volume;
			audioType.source.pitch = audioType.pitch;
			audioType.source.loop = audioType.loop;
			if (audioType.group != null)
			{
				audioType.source.outputAudioMixerGroup = audioType.group;
			}
		}
		for (int j = 0; j < this.poolSize; j++)
		{
			AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.enabled = false;
			this.audioSourcePool.Add(audioSource);
		}
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00002440 File Offset: 0x00000640
	private void Start()
	{
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00002444 File Offset: 0x00000644
	public void Play(string name)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				audioType.source.Play();
				return;
			}
		}
		Debug.LogWarning("无" + name + "音频");
	}

	// Token: 0x06000018 RID: 24 RVA: 0x0000249C File Offset: 0x0000069C
	public void PlaySFX(string name)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
				audioSource.clip = audioType.clip;
				audioSource.volume = audioType.volume;
				audioSource.pitch = audioType.pitch;
				audioSource.loop = audioType.loop;
				if (audioType.group != null)
				{
					audioSource.outputAudioMixerGroup = audioType.group;
				}
				audioSource.Play();
				base.StartCoroutine(this.DisableAfterFinished(audioSource));
				return;
			}
		}
		Debug.LogWarning("无" + name + "音频");
	}

	// Token: 0x06000019 RID: 25 RVA: 0x00002558 File Offset: 0x00000758
	public void PlaySFXWithRandomPitch(string name, float minPitch, float maxPitch)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				AudioSource pooledAudioSource = this.GetPooledAudioSource();
				pooledAudioSource.clip = audioType.clip;
				pooledAudioSource.volume = audioType.volume;
				pooledAudioSource.pitch = Random.Range(minPitch, maxPitch);
				pooledAudioSource.loop = audioType.loop;
				if (audioType.group != null)
				{
					pooledAudioSource.outputAudioMixerGroup = audioType.group;
				}
				pooledAudioSource.Play();
				base.StartCoroutine(this.DisableAfterFinished(pooledAudioSource));
				return;
			}
		}
		Debug.LogWarning("无" + name + "音频");
	}

	// Token: 0x0600001A RID: 26 RVA: 0x0000260C File Offset: 0x0000080C
	private AudioSource GetPooledAudioSource()
	{
		foreach (AudioSource audioSource in this.audioSourcePool)
		{
			if (!audioSource.enabled)
			{
				audioSource.enabled = true;
				return audioSource;
			}
		}
		AudioSource audioSource2 = base.gameObject.AddComponent<AudioSource>();
		this.audioSourcePool.Add(audioSource2);
		return audioSource2;
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00002688 File Offset: 0x00000888
	private IEnumerator DisableAfterFinished(AudioSource source)
	{
		yield return new WaitWhile(() => source.isPlaying);
		source.enabled = false;
		yield break;
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002698 File Offset: 0x00000898
	public void PlayWithRandomPitch(string name, float minPitch, float maxPitch)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				audioType.source.pitch = Random.Range(minPitch, maxPitch);
				audioType.source.Play();
				return;
			}
		}
	}

	// Token: 0x0600001D RID: 29 RVA: 0x000026EC File Offset: 0x000008EC
	public void Pause(string name)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				audioType.source.Pause();
				return;
			}
		}
		Debug.LogWarning("无" + name + "音频");
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00002744 File Offset: 0x00000944
	public void StopAll()
	{
		global::AudioType[] array = this.audioTypes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].source.Stop();
		}
	}

	// Token: 0x0600001F RID: 31 RVA: 0x00002774 File Offset: 0x00000974
	public void Stop(string name)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				audioType.source.Stop();
				return;
			}
		}
		Debug.LogWarning("无" + name + "音频");
	}

	// Token: 0x06000020 RID: 32 RVA: 0x000027CC File Offset: 0x000009CC
	public bool IsPlaying(string name)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				return audioType.source.isPlaying;
			}
		}
		Debug.LogWarning("无" + name + "音频");
		return false;
	}

	// Token: 0x06000021 RID: 33 RVA: 0x00002824 File Offset: 0x00000A24
	public void SetPitch(string name, float targetPicth)
	{
		foreach (global::AudioType audioType in this.audioTypes)
		{
			if (audioType.name == name)
			{
				audioType.source.pitch = targetPicth;
				return;
			}
		}
		Debug.LogWarning("无" + name + "音频");
	}

	// Token: 0x0400000D RID: 13
	public global::AudioType[] audioTypes;

	// Token: 0x0400000E RID: 14
	private static AudioManager _instance;

	// Token: 0x0400000F RID: 15
	private List<AudioSource> audioSourcePool = new List<AudioSource>();

	// Token: 0x04000010 RID: 16
	private int poolSize = 10;
}
