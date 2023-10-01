using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixieActivatePlatform : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Platform")
        {
            other.gameObject.GetComponent<MovingPlatform>().Activate();
        }
    }
}
