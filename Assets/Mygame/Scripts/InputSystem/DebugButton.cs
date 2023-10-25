using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugButton : MonoBehaviour
{
    public void ModeChangeToMenu()
    {
        GA.Input.ModeChange(MyInput.InputMode.Menu);
    }
    public void ModeChangeInGame()
    {
        GA.Input.ModeChange(MyInput.InputMode.InGame);
    }
}
