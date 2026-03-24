using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    void Start()
    {
        NetworkServerProcessing.Init();
        NetworkServerProcessing.SetGameLogic(this);

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        
    }

}
