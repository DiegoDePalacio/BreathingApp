﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BreathingApp
{
    public class Breathe : MonoBehaviour
    {
        private enum BreathState
        {
            Waiting,
            Breathing,
            Holding,
            Exhaling
        }

        private const float DOUBLE_CLICK_OFFSET = 0.5f;

        private BreathState _breathState = BreathState.Waiting;
        private Image _progressBar;
        [SerializeField] private Button _mainButton;
        [SerializeField] private Button _lockButton;
        [SerializeField] private Image _lockIcon;
        [SerializeField] private TextMeshProUGUI _lockAmountText;
        private TextMeshProUGUI _text;
        private Color _textOriginalColor;
        private float _lastClick = -DOUBLE_CLICK_OFFSET;
        private bool _waitingForDoubleClick;

        private float BreathingStart { get; set; }
        private float HoldStart { get; set; }
        private float ExhaleStart { get; set; }
        private float TimeToHold { get; set; }
        private float TimeToExhale { get; set; }
        private int BreatheCount { get; set; }
        private float LockedTime { get; set; } = 0f;
        private float BreathingTime { get; set; }
        
        private float HoldingFor => (Time.time - HoldStart);
        private float ExhalingFor => (Time.time - ExhaleStart);
        
        private void Awake()
        {
            Assert.IsNotNull(_mainButton);
            Assert.IsNotNull(_lockButton);
            Assert.IsNotNull(_lockIcon);
            Assert.IsNotNull(_lockAmountText);
            
            ResetLock();
            _progressBar = GetComponentInChildren<Image>();
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _textOriginalColor = _text.color;

            Assert.IsNotNull(_progressBar);
            Assert.IsNotNull(_text);
            
            SetState(BreathState.Waiting);
        }

        private void ResetLock()
        {
            LockedTime = 0f;
            _lockButton.gameObject.SetActive(true);
            _lockIcon.gameObject.SetActive(true);
            _lockAmountText.gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            _mainButton.onClick.AddListener(OnClick);
            _lockButton.onClick.AddListener(OnLock); 
        }

        private void OnDisable()
        {
            _mainButton.onClick.RemoveListener(OnClick);
            _lockButton.onClick.RemoveListener(OnLock); 
        }

        private void OnLock()
        {
            if (LockedTime > 0f && !(_breathState == BreathState.Waiting))
            {
                ResetLock();
            }
            else
            {
                LockedTime = (_breathState == BreathState.Waiting ? LockedTime + 1 : TimeToHold * 0.25f);
                _lockIcon.gameObject.SetActive(false);
                _lockAmountText.text = LockedTime.ToString("F0");
                _lockAmountText.gameObject.SetActive(true);
            }
        }

        private void OnClick()
        {
            if (Time.time - _lastClick < DOUBLE_CLICK_OFFSET)
            {
                switch (_breathState)
                {
                    case BreathState.Waiting:
                        SetState(BreathState.Breathing);
                        break;
                    case BreathState.Breathing:
                        SetState(BreathState.Holding);
                        break;
                    case BreathState.Holding:
                    case BreathState.Exhaling:
                        SetState(BreathState.Waiting);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                _text.color = _textOriginalColor;
                _lastClick = Time.time - DOUBLE_CLICK_OFFSET;
                _waitingForDoubleClick = false;
            }
            else
            {
                _text.color = _textOriginalColor * 0.5f;
                _lastClick = Time.time;
                _waitingForDoubleClick = true;
            }
        }

        private void SetState(BreathState breathState)
        {
            switch (breathState)
            {
                case BreathState.Waiting:
                    ResetLock();
                    BreatheCount = 0;
                    _progressBar.fillAmount = 0f;
                    break;
                case BreathState.Breathing:
                    _progressBar.fillAmount = 0f;
                    BreathingStart = Time.time;
                    break;
                case BreathState.Holding:
                    TimeToHold = (Time.time - BreathingStart) * 4;
                    if (TimeToHold > 0f) { _lockButton.gameObject.SetActive(true); }
                    TimeToExhale = TimeToHold / 2f;
                    HoldStart = Time.time;
                    ExhaleStart = HoldStart + TimeToHold;
                    break;
                case BreathState.Exhaling:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(breathState), breathState, null);
            }

            _breathState = breathState;
        }

        private void OnDestroy()
        {
            _mainButton.onClick.RemoveListener(OnClick);
        }

        private string GetStateMessage()
        {
            switch (_breathState)
            {
                case BreathState.Waiting: return "Ready?";
                case BreathState.Breathing: return LockedTime > 0f 
                    ? $"Breathe [{BreatheCount}]\n{Mathf.RoundToInt(BreathingStart + LockedTime - Time.time)}" 
                    : $"Breathe\n{BreatheCount}";
                case BreathState.Holding: return $"Hold! [{BreatheCount}]\n{Mathf.RoundToInt(HoldStart + TimeToHold - Time.time)}";
                case BreathState.Exhaling: return $"Exhale [{BreatheCount}]\n{Mathf.RoundToInt(ExhaleStart + TimeToExhale - Time.time)}";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            switch (_breathState)
            {
                case BreathState.Waiting:
                    break;
                case BreathState.Breathing:
                    if (LockedTime > 0 && Time.time > BreathingStart + LockedTime)
                    {
                        SetState(BreathState.Holding);
                    }
                    else
                    {
                        float breathingPercentage = (Time.time - BreathingStart) / LockedTime;
                        _progressBar.fillAmount = breathingPercentage;
                    }
                    break;
                case BreathState.Holding:
                    float holdPercentage = HoldingFor / TimeToHold;

                    if (holdPercentage > 1f)
                    {
                        SetState(BreathState.Exhaling);
                    }
                    else
                    {
                        _progressBar.fillAmount = holdPercentage;
                    }

                    break;
                case BreathState.Exhaling:
                    float exhalingPercentage = ExhalingFor / TimeToExhale;

                    if (exhalingPercentage > 1f)
                    {
                        SetState(BreathState.Breathing);
                        BreatheCount++;
                    }
                    else
                    {
                        _progressBar.fillAmount = 1 - exhalingPercentage;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _text.text = GetStateMessage();

            if (_waitingForDoubleClick && (Time.time - _lastClick > DOUBLE_CLICK_OFFSET))
            {
                _text.color = _textOriginalColor;
                _waitingForDoubleClick = false;
            }
        }
    }
}