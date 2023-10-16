using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugButton : MonoBehaviour
{
    public void ModeChangeToMenu()
    {
        GA.Input.ModeChange(MyInput.InputModeType.Menu);
    }
    public void ModeChangeInGame()
    {
        GA.Input.ModeChange(MyInput.InputModeType.InGame);
    }
}
