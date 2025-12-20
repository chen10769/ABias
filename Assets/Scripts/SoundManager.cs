using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class SoundManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static SoundManager instance;
    // 用于播放音乐
    private AudioSource audioSource;
    public AudioSource audioSourceBgm;
    // 
    private Dictionary<string, AudioClip> dictAudio;
    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        dictAudio = new Dictionary<string, AudioClip>();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // 辅助函数： 加载音频，需要确保音频文件的路径在Resources文件夹下
    public AudioClip LoadAudio(string path)
    {
        return (AudioClip)Resources.Load(path);
    }

    // 辅助函数：获取音频，并且将其缓存在dictAudio中，避免重复加载
    private AudioClip GetAudio(string path)
    {
        if (!dictAudio.ContainsKey(path))
        {
            dictAudio[path] = LoadAudio(path);
        }
        return dictAudio[path];
    }

    public void PlayBGM(string name, float volume = 1.0f)
    {
        StartCoroutine(LoadAndPlayBGM(name, volume));
    }

    private IEnumerator LoadAndPlayBGM(string name, float volume)
    {
        // 停止当前播放的BGM
        audioSourceBgm.Stop();

        // 异步加载音频资源
        ResourceRequest resourceRequest = Resources.LoadAsync<AudioClip>(name);
        yield return resourceRequest; // 等待加载完成

        // 获取加载的音频资源
        AudioClip audioClip = resourceRequest.asset as AudioClip;
        if (audioClip != null)
        {
            audioSourceBgm.clip = audioClip;
            audioSourceBgm.volume = volume;
            audioSourceBgm.Play();
            //BgmPanel.instance.SetBgmTime(0f);
        }
        else
        {
            Debug.LogError("Failed to load audio: " + name);
        }
        
    }

    public void StopBGM()
    {
        audioSourceBgm.Stop();
        
    }

    public void PauseAudio()
    {
        // 暂停音频
        audioSourceBgm.Pause();
    }

    public void UnPause()
    {
        // 暂停音频
        audioSourceBgm.UnPause();
    }

    public void ChangeVolume(float volume)
    {
        audioSource.volume = volume;
    }

    // 播放音效
    public void PlaySound(string path, float volume = 1f)
    {
     
        //audioSource.loop = true;
        this.audioSource.PlayOneShot(GetAudio(path), volume);
        
    }
    



    public void StopSound()
    {
        // 停止指定路径的音效
        audioSource.Stop();
    }

    // 辅助函数：获取音频时长
    public float GetAudioLength(string path)
    {
        AudioClip clip = GetAudio(path);
        if (clip != null)
        {
            return clip.length;
        }
        else
        {
            Debug.LogError("Audio not found at path: " + path);
            return -1f; // 返回-1或者其它表示错误的值
        }
    }
   

}