using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Transform _charactor;   
    [SerializeField] Transform _cameraPivot;
    [SerializeField] Transform _cameraPosition;

    void Start()
    {
    }
    //カメラ上下移動の最大、最小角度です。Inspectorウィンドウから設定してください
    [Range(-0.999f, -0.5f)]
    public float maxYAngle = -0.5f;
    [Range(0.5f, 0.999f)]
    public float minYAngle = 0.5f;
    // Update is called once per frame
    void Update()
    {
        //マウスのX,Y軸がどれほど移動したかを取得します
        float X_Rotation = Input.GetAxis("Mouse X");
        float Y_Rotation = Input.GetAxis("Mouse Y");
        //Y軸を更新します（キャラクターを回転）取得したX軸の変更をキャラクターのY軸に反映します
        _charactor.transform.Rotate(0, X_Rotation, 0);

        //次はY軸の設定です。
        float nowAngle = _cameraPivot.transform.localRotation.x;
        //最大値、または最小値を超えた場合、カメラをそれ以上動かない用にしています。
        //キャラクターの中身が見えたり、カメラが一回転しないようにするのを防ぎます。
        if (-Y_Rotation != 0)
        {
            if (0 < Y_Rotation)
            {
                if (minYAngle <= nowAngle)
                {
                    _cameraPivot.transform.Rotate(-Y_Rotation, 0, 0);
                }
            }
            else
            {
                if (nowAngle <= maxYAngle)
                {
                    _cameraPivot.transform.Rotate(-Y_Rotation, 0, 0);
                }
            }
        }
    }
}