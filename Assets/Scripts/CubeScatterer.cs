using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScatterer : MonoBehaviour {
    public GameObject prefab;

    public int count = 100;
    public float extent = 3f;

    private void Start()
    {
        for (int i = 0; i < count; ++i)
        {
            Vector3 position = new Vector3(
                Random.Range(-extent, extent),
                0,
                Random.Range(-extent, extent)
            );
            
            GameObject o = Instantiate(prefab, position, Quaternion.identity) as GameObject;

            o.transform.localScale = new Vector3(
                1,
                Random.Range(.5f, 10),
                1
            );
        }
    }
}
