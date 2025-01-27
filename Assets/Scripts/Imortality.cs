using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Imortality : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("swarmGenerator");
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] != gameObject)
            {
                Destroy(gameObjects[i]);
            }
        }
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
