using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {
    static int numberObject = 30;
    public GameObject[] Adress = new GameObject[numberObject];
    
    int j = 0;
    public void changeObjLeft()
    {
        Adress[j].active = false;
        j = (int)Mathf.Round((j - 1) % 3);
        Debug.Log(j);
        Adress[j].active = true;
    }
    public void changeObjRight()
    {
        Adress[j].active = false;
        j = (int)Mathf.Round((j + 1) % 3);
        Debug.Log(j);
        Adress[j].active = true;
    }

    static void DisableActive(GameObject[] Dress)
    {
        for (int i = 0; i < numberObject; i++)
        {
            Dress[i].active = false;
        }
    }


}
