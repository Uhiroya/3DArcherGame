using Cinemachine;
using UnityEngine;

public class OrbitalTest : CinemachineExtension
{
    private const float RotateSpeed = 2f;

    private Vector2 _currentRotate;
    private Transform _lookAt;

    private CinemachineVirtualCamera _virtualCamera;

    private void Update()
    {
        // 簡易的に操作を受け付けてみる
        if (Input.GetKey(KeyCode.LeftArrow)) Rotation(new Vector2(1f, 0f));

        if (Input.GetKey(KeyCode.RightArrow)) Rotation(new Vector2(-1f, 0f));

        if (Input.GetKey(KeyCode.UpArrow)) Rotation(new Vector2(0f, 1f));

        if (Input.GetKey(KeyCode.DownArrow)) Rotation(new Vector2(0f, -1f));

        // 左右上下回転操作
        void Rotation(Vector2 diffDelta)
        {
            diffDelta *= RotateSpeed * Time.deltaTime * 30;
            _currentRotate.x += diffDelta.y;
            _currentRotate.y += diffDelta.x;
        }
    }

    protected override void ConnectToVcam(bool connect)
    {
        base.ConnectToVcam(connect);
        if (connect == false) return;

        // 初期化処理
        // VirtualCameraのLookAt対象を生成しておく。わざわざ生成しなくても直接対象物を設定も可。
        _lookAt = GameObject.Find("LookAt")?.transform;
        if (_lookAt == null)
        {
            _lookAt = new GameObject("LookAt").transform;
            _lookAt.position = new Vector3(0f, 0.5f, 0f);
        }

        _virtualCamera = VirtualCamera as CinemachineVirtualCamera;
        _virtualCamera.LookAt = _lookAt;
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        var (pos, rotate) = GetTransform(_virtualCamera.LookAt.localToWorldMatrix);
        state.RawPosition = pos;
        state.RawOrientation = rotate;
    }

    /// <summary>
    /// カメラのTransformを取得
    /// </summary>
    private (Vector3 pos, Quaternion rotate) GetTransform(Matrix4x4 baseCenterMatrix)
    {
        // 中心点を回転させる
        var centerMatrix = baseCenterMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(_currentRotate.y, Vector3.up) *
                                                               Quaternion.AngleAxis(_currentRotate.x, Vector3.right));

        // 回転に基づいたカメラの位置を取得
        var distanceVector = new Vector3(0f, 0f, -3f);
        var pos = centerMatrix.MultiplyPoint3x4(distanceVector);

        return (pos, centerMatrix.rotation);
    }
}
