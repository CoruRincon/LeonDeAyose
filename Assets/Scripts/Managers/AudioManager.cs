using System;
using System.Collections.Generic;
using UnityEngine;

// Plays music and sound effects. Clips are authored in the inspector as
// name/clip pairs and copied into dictionaries for lookup by name. Update keeps
// the right music track playing for the current GameState and applies volumes.
public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;

    [SerializeField]
    private AudioSource musicSource, effectsSource, playerSource;

    [Serializable]
    public struct AudioAssets {
        public string name;
        public AudioClip effectSound;
    }

    [SerializeField]
    private AudioAssets[] Music;

    IDictionary<string, AudioClip> MusicDictionary = new Dictionary<string, AudioClip>();
    IDictionary<string, AudioClip> EffectsDictionary = new Dictionary<string, AudioClip>();

    [SerializeField]
    private AudioAssets[] Effects;
    private AudioClip MouseClick, MouseClicknt;

    public bool musicEnabled;

    public float masterVolume, musicVolume, effectVolume;
    public float actualMusicVolume, actualEffectVolume;

    public bool isMuted = false;

    // Start is called before the first frame update
    void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        if (MusicDictionary.Count == 0) {
            FillMusic();
        }

        if (EffectsDictionary.Count == 0) {
            FillEffects();
        }

        actualMusicVolume = musicVolume * masterVolume;
        actualEffectVolume = effectVolume * masterVolume;
    }

    // Update is called once per frame
    void Update() {
        musicSource.volume = actualMusicVolume;
        effectsSource.volume = actualEffectVolume;
        if (playerSource) {
            playerSource.volume = actualEffectVolume;
        }

        if (musicEnabled && !isMuted) {
            switch (GameManager.Instance.GetCurrentState()) {
                /*case GameManager.GameStates.LOADING:
                    //musicEnabled = false;
                    break;*/

                case GameManager.GameStates.MENU:
                    CheckMusic(musicSource, "MenuMusic");
                    break;

                case GameManager.GameStates.PLAYING:
                case GameManager.GameStates.LOADING:
                    CheckMusic(musicSource, "GameMusic");
                    break;

                case GameManager.GameStates.CREDITS:
                    CheckMusic(musicSource, "MenuMusic");
                    break;
            }
        }

        if (isMuted) {
            if (musicSource != null && musicSource.isPlaying) {
                musicSource.Stop();
            }

            if (effectsSource != null && effectsSource.isPlaying) {
                effectsSource.Stop();
            }

            if (playerSource != null && playerSource.isPlaying) {
                playerSource.Stop();
            }
        }

        SetMasterVolume(masterVolume);
    }

    private void FillMusic() {
        if (Music.Length == 0) { return; }
        foreach (var item in Music) {
            MusicDictionary.Add(item.name, item.effectSound);
        }
    }

    private void FillEffects() {
        if (Effects.Length == 0) { return; }
        foreach (var item in Effects) {
            EffectsDictionary.Add(item.name, item.effectSound);
        }
    }

    // Switches the source to the requested track only if it isn't already playing it.
    private void CheckMusic(AudioSource thisSource, string thisMusic) {
        if (musicEnabled) {
            if (!MusicDictionary[thisMusic] || MusicDictionary[thisMusic] == null) {
                Debug.Log("Musica not found");
                return;
            }

            AudioClip currentMusic = MusicDictionary[thisMusic];

            if (thisSource.isPlaying && thisSource.clip == currentMusic) {
                //Debug.Log("Music playing correctly");
                return;
            }
            else {
                thisSource.clip = currentMusic;
                thisSource.Play();
            }
        }
    }

    public void PlayEffect(string name) {
        if (isMuted) {
            return;
        }

        if (!EffectsDictionary[name] || EffectsDictionary[name] == null) {
            Debug.Log("Effect not found");
            return;
        }

        AudioClip currentEffect = EffectsDictionary[name];
        effectsSource.PlayOneShot(currentEffect);
    }

    public void PlayEffectPlayer(string name) {
        if (isMuted || playerSource.isPlaying) {
            return;
        }

        if (!EffectsDictionary[name] || EffectsDictionary[name] == null) {
            Debug.Log("Effect not found");
            return;
        }

        AudioClip currentEffect = EffectsDictionary[name];
        playerSource.PlayOneShot(currentEffect);
    }

    public void StopEffectPlayer() {
        playerSource.Stop();
    }

    public void Mute() {
        isMuted = true;
    }

    public void UnMute() {
        isMuted = false;
    }

    public void SetMasterVolume(float thisMultiplier) {
        masterVolume = thisMultiplier;

        actualMusicVolume = musicVolume * masterVolume;
        actualEffectVolume = effectVolume * masterVolume;
    }

    public void SetMusicVolume(float thisVolume) {
        musicVolume = thisVolume;
        actualMusicVolume = musicVolume * masterVolume;
    }

    public void SetSXFVolume(float thisVolume) {
        effectVolume = thisVolume;
        actualEffectVolume = effectVolume * masterVolume;
    }

    public void SetPlayerSource(AudioSource thisSource) {
        playerSource = thisSource;
    }
}