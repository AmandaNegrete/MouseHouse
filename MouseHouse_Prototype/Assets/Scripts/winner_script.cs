using UnityEngine;
using UnityEngine.SceneManagement;
public class winner_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other){

        if(other.CompareTag("Player")){
            SceneManager.LoadScene("Level 2"); 
        }
    }
}
