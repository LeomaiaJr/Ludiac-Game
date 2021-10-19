using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuessWordController : MonoBehaviour
{

    private bool levelTwo = false;

    private float firstAnswer = 3.8f;
    private float secondAnswer = 4.6f;

    public GameObject gameObject;
    void Start()
    {
        //get gameObject y coordinate
        Debug.Log(("LEFT0;RIGHT1").Split(';')[0].ToString().Substring(4, 1));
    }

    void Update()
    {

    }

    void checkCurrentWord()
    {

    }
}
