using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
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

	private void Start()
	{
	}

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

	private IEnumerator DisableAfterFinished(AudioSource source)
	{
		yield return new WaitWhile(() => source.isPlaying);
		source.enabled = false;
		yield break;
	}

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

	public void StopAll()
	{
		global::AudioType[] array = this.audioTypes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].source.Stop();
		}
	}

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

	public global::AudioType[] audioTypes;

	private static AudioManager _instance;

	private List<AudioSource> audioSourcePool = new List<AudioSource>();

	private int poolSize = 10;
}
