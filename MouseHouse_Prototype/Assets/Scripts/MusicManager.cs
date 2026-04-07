using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip music; 
    
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = music;
        audioSource.volume = .09f; 
        audioSource.loop = true; 

        // 3. Play the music
        audioSource.Play();
    }
}
