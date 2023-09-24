using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class TargetGenerator : MonoBehaviour
{
    [SerializeField] GameObject _targetGameObject;
    [SerializeField] float _scoreRate;
    [SerializeField] bool _moveTarget = false;
    BoxCollider _boxCollider;
    Vector3 _spawnArea;
    GameObject _target ;
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _spawnArea = _boxCollider.size / 2f;
        _target = Instantiate(_targetGameObject,transform.position, _targetGameObject.transform.rotation, transform);
    }
    private void OnEnable()
    {
        var spawnX = Random.Range(-_spawnArea.x , _spawnArea.x) + transform.position.x;
        var spawnY = Random.Range(-_spawnArea.y, _spawnArea.y) + transform.position.y;
        _target.transform.position = new Vector3(spawnX, spawnY, transform.position.z);
        _target.SetActive(true);
        //targetÇìÆÇ©ÇµÇΩÇ¢Ç∆Ç´ÅB
        if (_moveTarget)
        {
            var moveX = Random.Range(-_spawnArea.x + transform.position.x, +_spawnArea.x + transform.position.x);
            var moveY = Random.Range(-_spawnArea.y + transform.position.y, +_spawnArea.y + transform.position.y);
            _target.transform.DOMove(new Vector3(moveX, moveY, _target.transform.position.z), 3f).SetEase(Ease.Linear).SetLoops(-1 , LoopType.Yoyo);
        }
    }
    public void HitMyTarget(int score)
    {
        GameManager.Instance.AddScore((int)(score * _scoreRate));
        if (_moveTarget)
        {
            _target.transform.DOKill();
        }
        _target.SetActive(false);
    }
    private void OnDisable()
    {
        if(_target != null)
        {
            if (_moveTarget)
            {
                _target.transform.DOKill();
            }
            _target.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
