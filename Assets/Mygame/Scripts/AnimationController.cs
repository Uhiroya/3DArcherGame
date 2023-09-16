
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Animations;
using DG.Tweening;
//弓のアニメーション
//https://usaho3d.com/bow-for-vrc/

// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]

public class AnimationController : MonoBehaviour
{
    [SerializeField] ParentConstraint _bowStringConstraint;
    [SerializeField] GameObject _arrowObject;
    [SerializeField] GameObject _arrowParticle;
    [SerializeField] GameObject _arrowStart;
    private Animator _anim;
    private AnimatorStateInfo _currentBaseState;
    private AnimatorStateInfo _arrowState;
    float _arrowCharge = 0f;
    int _arrowMortionLayerIndex;
    int _WalkingLayerIndex;
    void Start()
    {
        _anim = GetComponent<Animator>();
        _arrowMortionLayerIndex = _anim.GetLayerIndex("ArrowMortionLayer");
        _WalkingLayerIndex = _anim.GetLayerIndex("WalkingMask");
    }
    void Update()
    {
        _currentBaseState = _anim.GetCurrentAnimatorStateInfo(_anim.GetLayerIndex("MoveControll"));
        _arrowState = _anim.GetCurrentAnimatorStateInfo(_arrowMortionLayerIndex);
    }
    IEnumerator ArrowFire()
    {
        Instantiate(_arrowParticle, _arrowStart.transform.position, _arrowStart.transform.rotation, null);
        yield return null;
    }
    bool flag = false;
    float timer = 0f;
    float time = 0.1f;
    void OnAnimatorIK()
    {
        if (flag)
        {
            timer += Time.deltaTime;
            timer = Mathf.Clamp(timer, 0f, time);
            _anim.bodyRotation = _anim.bodyRotation * Quaternion.Euler(0, 90 * (timer / time), 0);
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.2f);
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.2f);
            _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.4f);
            _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.4f);
            //_anim.SetIKRotation(AvatarIKGoal.LeftFoot , Quaternion.Euler (0 , - 30f ,0) );
            //_anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.Euler(0, - 30f, 0));
            _anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1f);
            _anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1f);
        }
        else
        {
            timer = 0f;
        }
    }
    public void ArrowChargeStart()
    {
        flag = true;
    }
    public void ArrowCharge()
    {
        _arrowObject.SetActive(true);
        _bowStringConstraint.constraintActive = true;
        _anim.SetBool("ArrowCharge", true);
        _anim.SetLayerWeight(_arrowMortionLayerIndex, 1.0f);
        _arrowCharge += Time.deltaTime;
    }
    public void ArrowRelease()
    {
        if (_arrowCharge > 0.5f)
        {
            _anim.SetTrigger("ArrowRelease");
            StartCoroutine(ArrowFire());
        }
        _arrowCharge = 0f;
        _arrowObject.SetActive(false);
        _anim.SetBool("ArrowCharge", false);
        _bowStringConstraint.constraintActive = false;
        flag = false;
        _anim.SetLayerWeight(_arrowMortionLayerIndex, 0f);
    }

    public void JumpWait()
    {
        _anim.SetBool("JumpWait", true);
    }
    public void JumpUp()
    {
        _anim.SetBool("Jump", true);
    }
    public void JumpEnd()
    {
        _anim.SetBool("JumpWait", false);
        _anim.SetBool("Jump", false);
    }
    
       
}
