using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameSound
{
    Attack,
    Button,
    Card,
    Death,
    Defeat,
    Ability,
    Victory
}

[System.Serializable]
public struct GameSoundToClip
{
    public GameSound gameSound;
    public AudioClip audioClip;
}

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] List<GameSoundToClip> gameSoundToClips;
    [SerializeField] AudioSource audioSource;

    AudioClip GetClip(GameSound gameSound)
    {
        foreach (GameSoundToClip gameSoundToClip in gameSoundToClips)
        {
            if (gameSoundToClip.gameSound == gameSound)
            {
                return gameSoundToClip.audioClip;
            }
        }
        return null;
    }

    void PlaySound(GameSound gameSound)
    {
        AudioClip audioClip = GetClip(gameSound);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    public void PlaySoundWithDelay(GameSound gameSound, float delay)
    {
        StartCoroutine(PlaySoundWithDelayCoroutine(gameSound, delay));
    }

    IEnumerator PlaySoundWithDelayCoroutine(GameSound gameSound, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySound(gameSound);
    }

    public void PlayAttackSound()
    {
        PlaySound(GameSound.Attack);
    }

    public void PlayButtonSound()
    {
        PlaySound(GameSound.Button);
    }

    public void PlayCardSound()
    {
        PlaySound(GameSound.Card);
    }

    public void PlayDeathSound()
    {
        PlaySound(GameSound.Death);
    }

    public void PlayDefeatSound()
    {
        PlaySound(GameSound.Defeat);
    }

    public void PlayAbilitySound()
    {
        PlaySound(GameSound.Ability);
    }

    public void PlayVictorySound()
    {
        PlaySound(GameSound.Victory);
    }
}
