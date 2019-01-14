﻿using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : Singleton<BackgroundMusic>
{
    [SerializeField] private AudioClip BGMusicMain;
    private AudioSource CurrentAudioSource;

    private void Awake()
    {
        //Init audio source to play background music.
        CurrentAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Invoke("StartBGMusic", 1F);
    }

    private void OnEnable()
    {
        //Register Audio status event
        AudioManager.OnMusicStatusChangedEvent += OnMusicStatusChangedEvent;
    }


    private void OnDisable()
    {
        //Unregister Audio status event
        AudioManager.OnMusicStatusChangedEvent -= OnMusicStatusChangedEvent;
    }

    private void StartBGMusic()
    {
        ///Start playing music is music setting is enabled.
        if (AudioManager.Instance.isMusicEnabled && !CurrentAudioSource.isPlaying)
        {
            CurrentAudioSource.clip = BGMusicMain;
            CurrentAudioSource.loop = true;
            CurrentAudioSource.Play();
        }
    }

    /// <summary>
    ///     Pauses the background music.
    /// </summary>
    public void PauseBGMusic()
    {
        if (CurrentAudioSource.isPlaying) CurrentAudioSource.Pause();
    }

    /// <summary>
    ///     Resumes the background music.
    /// </summary>
    public void ResumeBGMusic()
    {
        if (!CurrentAudioSource.isPlaying) CurrentAudioSource.Play();
    }

    /// <summary>
    ///     Raises the music status changed event event.
    /// </summary>
    /// <param name="isMusicEnabled">If set to <c>true</c> is music enabled.</param>
    private void OnMusicStatusChangedEvent(bool isMusicEnabled)
    {
        if (!isMusicEnabled)
            PauseBGMusic();
        else
            ResumeBGMusic();
    }
}