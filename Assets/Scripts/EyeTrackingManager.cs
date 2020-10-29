using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

public class EyeTrackingManager : MonoBehaviour
{
    enum EyeState
    {
        Idle,
        LeftWink,
        RightWink,
        Blink
    }

    [SerializeField] private CameraController cameraController;
    
    [SerializeField]
    private EyeState _eyeState = EyeState.Idle;

    // 複数回まばたきの検出の際のインターバル
    private const float BlinkInterval = 0.2f;

    // 目を閉じた判定の閾値
    private const float BlinkThreshold = 0.4f;

    // 連続瞬きの際の入力時間
    private const float MultiBlinkInterval = 1f;
    
    // 最後に両目瞬きをした瞬間からの経過時間
    private float _timeSinceLastBlink = 0;
    
    private readonly Dictionary<EyeIndex, float> _prevEyeOpenness = new Dictionary<EyeIndex, float>()
    {
        {EyeIndex.LEFT, 1},
        {EyeIndex.RIGHT, 1},
    };

    private readonly Dictionary<EyeIndex, bool> _isClosing = new Dictionary<EyeIndex, bool>
    {
        {EyeIndex.LEFT, false},
        {EyeIndex.RIGHT, false}
    };

    private readonly Dictionary<EyeIndex, float> _closingTime = new Dictionary<EyeIndex, float>
    {
        {EyeIndex.LEFT, 0},
        {EyeIndex.RIGHT, 0}
    };

    private readonly Dictionary<EyeIndex, bool> _isRightWinking = new Dictionary<EyeIndex, bool>
    {
        {EyeIndex.LEFT, false},
        {EyeIndex.RIGHT, false}
    };


    private void Start()
    {
    }

    private void Update()
    {
        SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.LEFT, out var currentLeftOpenness);
        SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.RIGHT, out var currentRightOpenness);

        UpdateEyeOpenness(EyeIndex.LEFT);
        UpdateEyeOpenness(EyeIndex.RIGHT);

        JudgeEyePattern();
    }

    private void UpdateEyeOpenness(EyeIndex eyeIndex)
    {
        var anotherEye = eyeIndex == EyeIndex.RIGHT ? EyeIndex.LEFT : EyeIndex.RIGHT;

        SRanipal_Eye_v2.GetEyeOpenness(eyeIndex, out var currentOpenness);

        // 指定の目をつむっているとき
        if (currentOpenness < BlinkThreshold)
        {
            // 目を閉じてる経過時間を加算
            _closingTime[eyeIndex] += Time.deltaTime;

            // 前回の入力では空いてるとき (目をつむった瞬間)
            if (!_isClosing[eyeIndex])
            {
                // OnWink(eyeIndex);

                _closingTime[eyeIndex] = 0;

                //これはこのif文の最後でやる
                _isClosing[eyeIndex] = true;
            }
        }
        // 指定の目が空いているとき
        else
        {
            _closingTime[eyeIndex] = 0;
            _isClosing[eyeIndex] = false;
        }

        _prevEyeOpenness[eyeIndex] = currentOpenness;
    }

    private void JudgeEyePattern()
    {
        // 1回の両目まばたき
        if (0 < _closingTime[EyeIndex.LEFT] && _closingTime[EyeIndex.LEFT] < BlinkInterval &&
            0 < _closingTime[EyeIndex.RIGHT] && _closingTime[EyeIndex.RIGHT] < BlinkInterval)
        {
            SetEyeState(EyeState.Blink);
        }
        // 左目ウインク    
        else if (BlinkInterval < _closingTime[EyeIndex.LEFT] && !_isClosing[EyeIndex.RIGHT])
        {
            SetEyeState(EyeState.LeftWink);
        }
        // 右目ウインク    
        else if (BlinkInterval < _closingTime[EyeIndex.RIGHT] && !_isClosing[EyeIndex.LEFT])
        {
            SetEyeState(EyeState.RightWink);
        }
        else
        {
            SetEyeState(EyeState.Idle);
        }

        _timeSinceLastBlink += Time.deltaTime;
    }

    private void SetEyeState(EyeState eyeState)
    {
        if (eyeState == _eyeState)
            return;

        switch (eyeState)
        {
            case EyeState.Idle:
                break;
            case EyeState.LeftWink:
                OnWink(EyeIndex.LEFT);
                break;
            case EyeState.RightWink:
                OnWink(EyeIndex.RIGHT);
                break;
            case EyeState.Blink:
                OnNaturalBlink();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eyeState), eyeState, null);
        }

        _eyeState = eyeState;
    }

    private void OnNaturalBlink()
    {
        Debug.Log("Natural Blink");
        
        if(_timeSinceLastBlink < MultiBlinkInterval)
            OnDoubleBlink();
            
        _timeSinceLastBlink = 0;
    }

    private void OnDoubleBlink()
    {
        Debug.Log("Double Blink");
        cameraController.MoveToLookingNode();
        _timeSinceLastBlink = 0;
    }
    
    private void OnWink(EyeIndex eyeIndex)
    {
        Debug.Log("Wink! : " + eyeIndex.ToString());
        
        var lookingNode = MyUtility.GetLookingNode(out var hit);
        if (lookingNode)
        {
            if (lookingNode.CompareTag("Node/Resource"))
            {
                var resourceNode = lookingNode.GetComponent<ResourceNode>();
                if (eyeIndex == EyeIndex.LEFT)
                {
                    resourceNode.InstantiateLinkedNodes(SystemManager.Instance.LinkedNodeLimit);
                }

                if (eyeIndex == EyeIndex.RIGHT)
                {
                    resourceNode.InstantiateStatsNodes();
                }
            }
        }
    }
}