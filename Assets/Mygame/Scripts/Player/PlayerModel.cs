//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public class PlayerModel
//{
//    [SerializeField] Rigidbody _rb;

//    JumpStatePattern _jumpState;
//    public JumpStatePattern JumpState
//    {
//        get { return _jumpState; }
//        set { _jumpState = value; }
//    }
//    AnimStatePattern _animState;
//    public AnimStatePattern AnimState
//    { 
//        get { return _animState; }
//        set { _animState = value; }
//    }



//    public void JumpUpdate()
//    {

//    }
//    void JumpAction()
//    {
//        if (_jumpState == JumpStatePattern.JumpNow)
//        {
//            if (_rb.velocity.y < 0)
//            {
//                _jumpState = JumpStatePattern.Fall;
//            }
//        }
//        if (_jumpState == JumpStatePattern.Fall)
//        {
//            if (Mathf.Abs(_rb.velocity.y) < 0.05f)
//            {
//                _ac.JumpEnd();
//                _nextJumpTimer += Time.deltaTime;
//                if (_nextJumpTimer > 0.5f)
//                {
//                    _nextJumpTimer = 0f;
//                    _jumpState = JumpStatePattern.Landing;
//                }
//            }
//        }
//    }


//}
//public enum JumpStatePattern
//{
//    Landing,
//    JumpWait,
//    JumpNow,
//    Fall,
//}
//public enum AnimStatePattern
//{
//    Idle,
//    forward,
//    backward,
//    Jump,
//    ArrowFire,
//}