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
        // 1. Only calculate fear if the cat exists
        if (cat != null)
        {
            if(Vector3.Distance(transform.position, cat.position) < 1f)
            {
                fearVal += 50f * Time.deltaTime;
            }
            else
            {
                fearVal -= 5f * Time.deltaTime;
            }
        }
        else 
        {
            // Optional: reduce fear if no cat is present at all
            fearVal -= 5f * Time.deltaTime;
        }

        // 2. Keep fear within 0 and 100
        fearVal = Mathf.Clamp(fearVal, 0, maxFearVal);

        if (fearVal >= maxFearVal)
        {
            Manager.Manager_.LoseLife();
            Debug.Log("1 life lost");
            fearVal = 0;
        }

        // 3. Update visuals (this can stay outside, it just uses the fearVal number)
        for(int i = 0; i < fearImages.Count; i++)
        {
            if(fearVal > maxFearVal/fearImages.Count * i)
                fearImages[i].color = new Color(1, 1, 1, 1);
            else
                fearImages[i].color = new Color(1, 1, 1, 0);
        }
    }
}
