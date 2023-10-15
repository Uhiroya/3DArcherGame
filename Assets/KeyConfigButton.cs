using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyConfigButton : MonoBehaviour
{
    [SerializeField] Text _buttonText;
    [SerializeField] string _actionName;
    private void Start()
    {
        DisplayText(InputRebinder.FindKeyName(_actionName));
    }
    public void OnClick()
    {
        if(_actionName != null)
        {
            InputRebinder.ReBind(_actionName , DisplayText);
        }
    }
    public void DisplayText(string keyName)
    {
        _buttonText.text = keyName;
    }
}
