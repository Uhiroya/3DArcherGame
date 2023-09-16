using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    static CameraManager instance;
    public static CameraManager Instance => instance;
    [SerializeField] GameObject FreeLookCamera;
    [SerializeField] GameObject TPSCamera;
    public static bool _changeFlag = false;

    private void Awake()
    {
        //if (instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(!_changeFlag)
            {
                FreeLookCamera.SetActive(false);
                TPSCamera.SetActive(true);
                _changeFlag = true;

            }
            else
            {
                FreeLookCamera.SetActive(true);
                TPSCamera.SetActive(false);
                _changeFlag = false;
            }
            
        }
    }
}
