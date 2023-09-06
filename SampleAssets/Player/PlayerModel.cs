// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Player
{
    public class PlayerModel : MonoBehaviour
    {
        private enum PlayerModelState
        {
            Idle,
            Happy,
            Walk,
            Run,
            Sprint
        }

        [SerializeField]
        private Animator _animator;

        private static readonly int YetiWalkAnimation = Animator.StringToHash("YetiWalk");
        private static readonly int YetiSprintAnimation = Animator.StringToHash("YetiSprint");
        private static readonly int YetiRunAnimation = Animator.StringToHash("YetiRun");
        private static readonly int YetiHappyAnimation = Animator.StringToHash("YetiHappy");
        private static readonly int YetiIdleAnimation = Animator.StringToHash("YetiIdle");

        private const float WalkThreshold = 0.5f;
        private const float RunThreshold = 10f;
        private const float SprintThreshold = 20f;
        private const float IdleToHappyThreshold = 8f;

        private float _lastIdleUpdateTime;
        private PlayerModelState _currentPlayerState = PlayerModelState.Idle;

        private bool IsYetiMoving => _currentPlayerState is
            PlayerModelState.Walk or PlayerModelState.Run or PlayerModelState.Sprint;

        private bool IsYetiIdle => _currentPlayerState is
            PlayerModelState.Idle or PlayerModelState.Happy;

        public void UpdatePlayerState(float movementDistance)
        {
            switch (movementDistance)
            {
                case > SprintThreshold:
                {
                    if (_currentPlayerState != PlayerModelState.Sprint)
                    {
                        _animator.CrossFade(YetiSprintAnimation, 1f);
                        _currentPlayerState = PlayerModelState.Sprint;
                    }

                    break;
                }
                case > RunThreshold:
                {
                    if (_currentPlayerState != PlayerModelState.Run)
                    {
                        _animator.CrossFade(YetiRunAnimation, 1f);
                        _currentPlayerState = PlayerModelState.Run;
                    }

                    break;
                }
                case > WalkThreshold:
                {
                    if (IsYetiIdle)
                    {
                        _animator.CrossFade(YetiWalkAnimation, 0f);
                        _currentPlayerState = PlayerModelState.Walk;
                    }
                    else if (_currentPlayerState != PlayerModelState.Walk)
                    {
                        _animator.CrossFade(YetiWalkAnimation, 1f);
                        _currentPlayerState = PlayerModelState.Walk;
                    }

                    break;
                }
                default:
                    HandleIdleState();
                    break;
            }
        }

        [UsedImplicitly]
        public void ReturnIdle()
        {
            _animator.CrossFade(YetiIdleAnimation, 1f);
            _currentPlayerState = PlayerModelState.Idle;
            _lastIdleUpdateTime = 0f;
        }

        private void HandleIdleState()
        {
            if (IsYetiMoving)
            {
                _animator.CrossFade(YetiIdleAnimation, 1f);
                _currentPlayerState = PlayerModelState.Idle;
                _lastIdleUpdateTime = 0f;
            }

            _lastIdleUpdateTime += Time.deltaTime;

            if (_lastIdleUpdateTime >= IdleToHappyThreshold &&
                _currentPlayerState != PlayerModelState.Happy)
            {
                _animator.CrossFade(YetiHappyAnimation, 1f);
                _currentPlayerState = PlayerModelState.Happy;
            }
        }
    }
}
