using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FearMech : MonoBehaviour
{
    public Transform cat;
    public Slider meter;

    float fearVal = 0;
    float maxFearVal = 100;

    public List<RawImage> fearImages = new List<RawImage>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void Update()
    {   
        //should calc distance bet. cat and mosue 
        if(Vector3.Distance(transform.position, cat.position) < 1f){
            //https://www.youtube.com/watch?v=oya8_SlLXb0
            //how i learned this lol ^^^
            fearVal += 50f * Time.deltaTime;
        }
        else{
            fearVal -= 5f * Time.deltaTime;
        }
        if (fearVal >= maxFearVal){
            Manager.Manager_.LoseLife();
            Debug.Log("1 life lost");
            fearVal = 0;
        }


        //Update fear meter visuals
        for(int i = 0; i < fearImages.Count; i++)
        {
            if(fearVal > maxFearVal/fearImages.Count * i)
                fearImages[i].color = new Color(1, 1, 1, 1);
            else
                fearImages[i].color = new Color(1, 1, 1, 0);
        }
    }
}
