using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {
    static int numberObject = 3;
    public GameObject[] dress = new GameObject[numberObject];
    
    int i = 0;
    public void changeObjLeft()
    {
        Debug.Log("Test button");
        dress[i].active = false;
        i = (int)Mathf.Round((i - 1) % 3);
        Debug.Log(i);
        dress[i].active = true;
    }
    public void changeObjRight()
    {
        dress[i].active = false;
        i = (int)Mathf.Round((i + 1) % 3);
        Debug.Log(i);
        dress[i].active = true;
    }
}
