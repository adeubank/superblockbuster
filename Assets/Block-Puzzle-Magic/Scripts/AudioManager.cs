﻿using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
    public AudioSource audioSource; //	Source of the audio
    public AudioClip clickSound; //  Plays this sound on each button click.
    public AudioClip gameOverSound; //	This sound will play on loading gameover screen.
    [HideInInspector] public bool isMusicEnabled = true;

    [HideInInspector] public bool isSoundEnabled = true;
    public static event Action<bool> OnSoundStatusChangedEvent;
    public static event Action<bool> OnMusicStatusChangedEvent;

    /// <summary>
    ///     Raises the enable event.
    /// </summary>
    private void OnEnable()
    {
        initAudioStatus();
    }

    /// <summary>
    ///     Inits the audio status.
    /// </summary>
    public void initAudioStatus()
    {
        isSoundEnabled = PlayerPrefs.GetInt("isSoundEnabled", 0) == 0 ? true : false;
        isMusicEnabled = PlayerPrefs.GetInt("isMusicEnabled", 0) == 0 ? true : false;

        if (!isSoundEnabled && OnSoundStatusChangedEvent != null) OnSoundStatusChangedEvent.Invoke(isSoundEnabled);
        if (!isMusicEnabled && OnMusicStatusChangedEvent != null) OnMusicStatusChangedEvent.Invoke(isMusicEnabled);
    }

    /// <summary>
    ///     Toggles the sound status.
    /// </summary>
    public void ToggleSoundStatus()
    {
        isSoundEnabled = isSoundEnabled ? false : true;
        PlayerPrefs.SetInt("isSoundEnabled", isSoundEnabled ? 0 : 1);

        if (OnSoundStatusChangedEvent != null) OnSoundStatusChangedEvent.Invoke(isSoundEnabled);
    }

    /// <summary>
    ///     Toggles the music status.
    /// </summary>
    public void ToggleMusicStatus()
    {
        isMusicEnabled = isMusicEnabled ? false : true;
        PlayerPrefs.SetInt("isMusicEnabled", isMusicEnabled ? 0 : 1);

        if (OnMusicStatusChangedEvent != null) OnMusicStatusChangedEvent.Invoke(isMusicEnabled);
    }

    /// <summary>
    ///     Plaies the button click sound.
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (Instance.isSoundEnabled && clickSound != null) audioSource.PlayOneShot(clickSound);
    }

    /// <summary>
    ///     Plaies the game over sound.
    /// </summary>
    public void PlayGameOverSound()
    {
        if (Instance.isSoundEnabled) audioSource.PlayOneShot(gameOverSound);
    }

    /// <summary>
    ///     Plays the sound given.
    /// </summary>
    /// <param name="clip">Clip.</param>
    public void PlaySound(AudioClip clip)
    {
        if (Instance.isSoundEnabled) audioSource.PlayOneShot(clip);
    }
}