
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
    [SerializeField] PositionConstraint _resetBowStringConstraint;

    [SerializeField] GameObject _arrowObject;
    [SerializeField] GameObject _arrowParticle;
    [SerializeField] GameObject _arrowStart;
    [SerializeField] float _targetDistance = 100f;
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
            //_anim.SetIKPosition(AvatarIKGoal.LeftHand , _arrowStart.transform.position);
            //_anim.SetIKPositionWeight(AvatarIKGoal.LeftHand , 0.8f);
            
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.2f);
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.1f);
            _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.4f);
            _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.1f);
            //_anim.SetIKRotation(AvatarIKGoal.LeftFoot , Quaternion.Euler (0 , - 30f ,0) );
            //_anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.Euler(0, - 30f, 0));
            _anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0.6f);
            _anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0.6f);
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
        _resetBowStringConstraint.constraintActive = false;
        _bowStringConstraint.constraintActive = true;
        _anim.SetBool("ArrowCharge", true);
        _anim.SetLayerWeight(_arrowMortionLayerIndex, _arrowCharge + 0.5f);
        _arrowCharge += Time.deltaTime;
    }
    public void ArrowRelease()
    {
        if (_arrowCharge > 0.5f)
        {
            RaycastHit hit;
            LayerMask mask =  ~( 1 << LayerMask.NameToLayer("Wall"));
            var ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _anim.SetTrigger("ArrowRelease");
            Instantiate(_arrowParticle, _arrowStart.transform.position, (CameraManager._nowCameraMode == MyTPSCamera.CameraMode.FreeLookMode) ? _arrowStart.transform.rotation
                : Physics.Raycast(ray, out hit ,_targetDistance ,mask) ? Quaternion.LookRotation((hit.point - _arrowStart.transform.position).normalized)
                : Quaternion.LookRotation((Camera.main.transform.position + Camera.main.transform.forward * _targetDistance - _arrowStart.transform.position).normalized), null);
            // Quaternion.identity * (hit.point - _arrowStart.transform.position).normalized
        }
        _arrowCharge = 0f;
        _arrowObject.SetActive(false);
        _anim.SetBool("ArrowCharge", false);
        _bowStringConstraint.constraintActive = false;
        _resetBowStringConstraint.constraintActive = true;
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
