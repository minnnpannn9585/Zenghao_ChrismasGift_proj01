using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameBtn : MonoBehaviour
{
    public GameObject instruction;
    public GameObject startCanvas;
    public void StartGame()
    {
        instruction.SetActive(true);
        startCanvas.SetActive(false);
    }
}
