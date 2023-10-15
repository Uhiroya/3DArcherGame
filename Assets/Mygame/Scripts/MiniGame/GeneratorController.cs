using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    int targetCount;
    private void Start()
    {
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        targetCount = GetComponentsInChildren<TargetGenerator>().Length;
    }
    public void DestroyTarget()
    {
        targetCount--;
        print(targetCount);
        if(targetCount == 0)
        {
            GameManager.Instance.GameOver();
            this.gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {
        targetCount = 0;
    }
}
