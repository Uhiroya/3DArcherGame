using UnityEngine;

/// <summary>
/// �^�[�Q�b�g������@�\�� IK ���g���Ē񋟂���B
/// </summary>
[RequireComponent(typeof(Animator))]
public class LookAtIK : MonoBehaviour
{
    /// <summary>����^�[�Q�b�g</summary>
    [SerializeField] Transform _target = default;
    /// <summary>�ǂꂭ�炢���邩</summary>
    [SerializeField, Range(0f, 1f)] float _weight = 0;
    /// <summary>�g�̂��ǂꂭ�炢�����邩</summary>
    [SerializeField, Range(0f, 1f)] float _bodyWeight = 0;
    /// <summary>�����ǂꂭ�炢�����邩</summary>
    [SerializeField, Range(0f, 1f)] float _headWeight = 0;
    /// <summary>�ڂ��ǂꂭ�炢�����邩</summary>
    [SerializeField, Range(0f, 1f)] float _eyesWeight = 0;
    /// <summary>�֐߂̓������ǂꂭ�炢�������邩</summary>
    [SerializeField, Range(0f, 1f)] float _clampWeight = 0;
    Animator _anim = default;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        // LookAt �̏d�݂ƃ^�[�Q�b�g���w�肷��
        _anim.SetLookAtWeight(_weight, _bodyWeight, _headWeight, _eyesWeight, _clampWeight);
        _anim.SetLookAtPosition(_target.position);
        // SetLookAtWeight �̃p�����[�^�� https://docs.unity3d.com/ja/current/ScriptReference/Animator.SetLookAtWeight.html ���Q��
    }
}
