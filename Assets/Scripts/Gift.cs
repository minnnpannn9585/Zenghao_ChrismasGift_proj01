using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gift : MonoBehaviour
{
    public void OpenGift()
    {
        SceneManager.LoadScene(2);
    }
}
