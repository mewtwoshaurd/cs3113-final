using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXObject : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        audioSource.Play();
        Destroy(gameObject, 5);
    }
}
