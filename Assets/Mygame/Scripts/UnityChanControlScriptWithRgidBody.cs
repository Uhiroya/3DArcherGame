//
// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
// サンプル
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.Animations;
using DG.Tweening;
//弓のアニメーション
//https://usaho3d.com/bow-for-vrc/
namespace UnityChan
{
// 必要なコンポーネントの列記
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Rigidbody))]

	public class UnityChanControlScriptWithRgidBody : MonoBehaviour
	{
        //public bool useCurves = true;				// Mecanimでカーブ調整を使うか設定する
        // このスイッチが入っていないとカーブは使われない
        //public float useCurvesHeight = 0.5f;		// カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）
        [SerializeField] ParentConstraint _bowStringConstraint;
        [SerializeField] GameObject _arrowObject;
        [SerializeField] GameObject _arrowParticle;
        [SerializeField] GameObject _arrowStart;
		[SerializeField] float _forwardSpeed = 7.0f;
		[SerializeField] float _backwardSpeed = 2.0f;
		[SerializeField] float _rotateSpeed = 2.0f;
		[SerializeField] float _jumpPower = 3.0f;
		private CapsuleCollider _col;
		private Rigidbody _rb;
		private float _orgColHight;
		private Vector3 _orgVectColCenter;
		private Animator _anim;							
		private AnimatorStateInfo _currentBaseState;   
		private AnimatorStateInfo _arrowState;   
        
        float _inputHorizonal;             
        float _inputVertical;
        float _motionDrag = 1f;
        float _arrowCharge = 0f;
        int _arrowMortionLayerIndex;
        int _WalkingLayerIndex;
        Vector3 _nowVelocity;
        float _nextJumpTimer = 0f;
        public enum JumpStatePattern
        {
            Landing,
            JumpWait,
            JumpNow,
            Fall,
        }
        public enum AnimStatePattern
        {
            Idle,
            forward,
            backward,
            Jump,
            ArrowFire,
        }
        JumpStatePattern _jumpState;
        AnimStatePattern _animState;
        Quaternion _defAnimRotation;
        void Start ()
		{
            
            _anim = GetComponent<Animator> ();
            _arrowMortionLayerIndex = _anim.GetLayerIndex("ArrowMortionLayer");
            _WalkingLayerIndex = _anim.GetLayerIndex("WalkingMask");

            _defAnimRotation = _anim.bodyRotation;
            _col = GetComponent<CapsuleCollider> ();
			_rb = GetComponent<Rigidbody> ();
			// CapsuleColliderコンポーネントのHeight、Centerの初期値を保存する
			_orgColHight = _col.height;
			_orgVectColCenter = _col.center;
		}
		void Update ()
		{
            _currentBaseState = _anim.GetCurrentAnimatorStateInfo(_anim.GetLayerIndex("MoveControll"));
            _arrowState = _anim.GetCurrentAnimatorStateInfo(_arrowMortionLayerIndex);
            _inputHorizonal = Input.GetAxis("Horizontal");              // 入力デバイスの水平軸をhで定義
            _inputVertical = Input.GetAxis("Vertical");
                                
            if(_animState == AnimStatePattern.ArrowFire)
            {
                _anim.SetFloat("Speed", Mathf.Clamp(_inputVertical , -1f ,0.2f));
                _anim.SetFloat("Direction", 0f);
            }
            else
            {
                _anim.SetFloat("Speed", _inputVertical);
                _anim.SetFloat("Direction", _inputHorizonal);
            }
            _anim.SetFloat("velocity_y", _rb.velocity.y);
            if (_jumpState == JumpStatePattern.JumpWait)
                _motionDrag = 0.7f;
            else if (_animState == AnimStatePattern.ArrowFire)
                _motionDrag = 0.5f;
            else
                _motionDrag = 1f;
            if (!CameraManager._changeFlag)
            {
                JumpAnimation();
            }
            ArrowAnimation();
        }
        IEnumerator ArrowFire()
        {
            Instantiate(_arrowParticle, _arrowStart.transform.position, _arrowStart.transform.rotation, null);
            yield return null;
        }
        IEnumerator FadeWeight()
        {
            float timer = 1f;
            while(timer >= 0f)
            {

                timer -= Time.deltaTime * 2f;
                if(timer < 0.5f)
                    flag = false;
                _anim.SetLayerWeight(_arrowMortionLayerIndex, timer);
                yield return null;
            }
            
        }
        bool flag = false;
        float timer = 0f;
        float time = 0.1f ;
        void OnAnimatorIK()
        {
            if (flag)
            {
                timer += Time.deltaTime;
                timer = Mathf.Clamp(timer , 0f , time);
                _anim.bodyRotation = _anim.bodyRotation * Quaternion.Euler(0, 90 * (timer / time) , 0);
                _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.2f);
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.4f);
                _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.2f);
                _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.4f);
            }
            else
            {
                //if (!_anim.IsInTransition(_arrowMortionLayerIndex))
                //{
                timer -= Time.deltaTime;
                timer = Mathf.Clamp(timer, 0f, time);
                _anim.bodyRotation = _anim.bodyRotation * Quaternion.Euler(0, 90 * (timer / time), 0);
                //}
            }

        }
        void ArrowAnimation()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _animState = AnimStatePattern.ArrowFire ;
                flag = true;
            }
            if (Input.GetMouseButton(0))
            {
                _arrowObject.SetActive(true);
                _bowStringConstraint.constraintActive = true;
                _arrowCharge += Time.deltaTime;
                _anim.SetBool("ArrowCharge", true);
                _anim.SetLayerWeight(_arrowMortionLayerIndex, 1.0f);
                
            }
            if (Input.GetMouseButtonUp(0))
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
                
                _animState = AnimStatePattern.Idle ;
                StartCoroutine(FadeWeight());
            }

        }
        void JumpAnimation()
        {
            if (Input.GetButton("Jump") && _jumpState == JumpStatePattern.Landing)
            {
                _jumpState = JumpStatePattern.JumpWait;
                _anim.SetBool("JumpWait", true);
            }
            if (Input.GetButtonUp("Jump") && _jumpState == JumpStatePattern.JumpWait)
            {
                _jumpState = JumpStatePattern.JumpNow;
                _anim.SetBool("Jump", true);
                _rb.AddForce(Vector3.up * _jumpPower, ForceMode.VelocityChange);
            }
            if (_currentBaseState.IsName("JumpRising"))
            {
                if (_rb.velocity.y <= 0)
                {
                    _jumpState = JumpStatePattern.Fall;

                }
            }
            if (_jumpState == JumpStatePattern.Fall)
            {
                if ( Mathf.Abs(_rb.velocity.y) < 0.05f)
                {
                    _anim.SetBool("JumpWait", false);
                    _anim.SetBool("Jump", false);
                    if (!_anim.IsInTransition(0))
                    {
                        _nextJumpTimer += Time.deltaTime;
                        if(_nextJumpTimer > 0.2f)
                        {
                            _jumpState = JumpStatePattern.Landing;
                            _nextJumpTimer = 0f;
                        }
                        
                    }
                }
            }
        }
		// 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
		void FixedUpdate ()
		{
            float moveRate;
            if (_inputVertical >= 0)
            {
                moveRate = _forwardSpeed;
            }
            else
            {
                moveRate = _backwardSpeed;
            }
            if (!CameraManager._changeFlag)
            {

                var moveVelo = (transform.forward * _inputVertical).normalized * moveRate * _motionDrag;
                _nowVelocity = new Vector3(moveVelo.x, _rb.velocity.y, moveVelo.z);
                transform.Rotate(0, _inputHorizonal * _rotateSpeed, 0);
            }
            else
            {
                var CForward = Camera.main.transform.forward;
                var CRight = Camera.main.transform.right;
                var _moveX = new Vector3(CForward.x, 0f , CForward.z) * _inputVertical;
                var _moveZ = new Vector3(CRight.x, 0f, CRight.z) * _inputHorizonal;
                _nowVelocity = (_moveX + _moveZ).normalized * moveRate * _motionDrag ;
            }

            _rb.velocity = _nowVelocity ;


        }
        // JUMP中の処理
        // 現在のベースレイヤーがjumpStateの時
        //else if (currentBaseState.fullPathHash == jumpState) {

        //		// ステートがトランジション中でない場合
        //		if (!_anim.IsInTransition (0)) {

        //			// 以下、カーブ調整をする場合の処理
        //			if (useCurves) {
        //				// 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
        //				// JumpHeight:JUMP00でのジャンプの高さ（0〜1）
        //				// GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
        //				float jumpHeight = _anim.GetFloat ("JumpHeight");
        //				float gravityControl = _anim.GetFloat ("GravityControl"); 
        //				if (gravityControl > 0)
        //					_rb.useGravity = false;	//ジャンプ中の重力の影響を切る

        //				// レイキャストをキャラクターのセンターから落とす
        //				Ray ray = new Ray (transform.position + Vector3.up, -Vector3.up);
        //				RaycastHit hitInfo = new RaycastHit ();
        //				// 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
        //				if (Physics.Raycast (ray, out hitInfo)) {
        //					if (hitInfo.distance > useCurvesHeight) {
        //						_col.height = _orgColHight - jumpHeight;			// 調整されたコライダーの高さ
        //						float adjCenterY = _orgVectColCenter.y + jumpHeight;
        //						_col.center = new Vector3 (0, adjCenterY, 0);	// 調整されたコライダーのセンター
        //					} else {
        //						// 閾値よりも低い時には初期値に戻す（念のため）					
        //						resetCollider ();
        //					}
        //				}
        //			}
        //			// Jump bool値をリセットする（ループしないようにする）				
        //			_anim.SetBool ("Jump", false);
        //		}
        //	}
        // キャラクターのコライダーサイズのリセット関数
        void resetCollider ()
		{
			// コンポーネントのHeight、Centerの初期値を戻す
			_col.height = _orgColHight;
			_col.center = _orgVectColCenter;
		}
	}
}