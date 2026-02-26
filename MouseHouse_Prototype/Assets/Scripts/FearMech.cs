using UnityEngine;
using UnityEngine.UI;
public class FearMech : MonoBehaviour
{
    public Transform cat;
    public Slider meter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void Update()
    {   
        //should calc distance bet. cat and mosue 
        if(Vector3.Distance(transform.position, cat.position) < 1f){
            //https://www.youtube.com/watch?v=oya8_SlLXb0
            //how i learned this lol ^^^
            meter.value += 50f * Time.deltaTime;
        }
        else{
            meter.value -= 5f * Time.deltaTime;
        }
        if (meter.value >= meter.maxValue){
            Manager.Manager_.LoseLife();
            Debug.Log("1 life lost");
            meter.value = 0;
        }
    }
}
