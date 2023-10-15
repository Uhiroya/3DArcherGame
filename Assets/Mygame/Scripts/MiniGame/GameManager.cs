using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] Text _scoreText;
    [SerializeField] Text _highScoreText;
    [SerializeField] Text _countDownText;
    [SerializeField] Text _timeText;
    [SerializeField] float _timeUp = 60f;
    [SerializeField] UnityEvent _startEvent;
    [SerializeField] UnityEvent _endEvent;
    static GameManager instance;
    public static GameManager Instance => instance ;
    static int _highScore = 0;

    int _nowScore = 0;
    Coroutine _nowGame;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
    public void AddScore(int point)
    {
        _nowScore += point;
        UpdateScore();
    }
    public void UpdateScore()
    {
        _scoreText.text = _nowScore.ToString();
    }
    public void GameStart()
    {
        if(_nowGame == null)
        {
            _nowScore = 0;
            UpdateScore();
            _nowGame = StartCoroutine(CountDown());
        }
    }
    public IEnumerator CountDown()
    {
        float timer = 3f;
        _timeText.color = Color.red;
        _scoreText.gameObject.transform.parent.gameObject.SetActive(true);
        _timeText.gameObject.transform.parent.gameObject.SetActive(true);
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            _timeText.text = timer.ToString("0");
            yield return null;
        }
        _timeText.text = " Start !! ";
        _startEvent?.Invoke();
        _nowGame = StartCoroutine(GameTimer());
    }
    public IEnumerator GameTimer()
    {
        float timer = _timeUp;
        _timeText.color = Color.black;
        while (timer > 0f)
        {
            _timeText.text = timer.ToString("00.00");
            timer -= Time.deltaTime;
            yield return null;
        }
        GameOver();
    }
    public void GameOver()
    {
        _endEvent?.Invoke();
        if(_nowScore > _highScore)
        {
            _highScore = _nowScore;
            _highScoreText.text = _highScore.ToString("0");
        }
        StopCoroutine(_nowGame);
        _nowGame = null;
    }
}
