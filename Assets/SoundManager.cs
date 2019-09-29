using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance = null;
    private AudioSource soundEffectAudio;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this; ;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
