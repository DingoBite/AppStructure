using System;
using System.Collections.Generic;
using AppStructure.Utils;
using UnityEngine;

namespace AppStructure.StateMachines
{
    public abstract class OpenCloseStateMachine<TState> : MonoBehaviour
    {
        [field: SerializeField] public TState LastOpenedState { get; private set; }

        private readonly Dictionary<TState, int> _orderedOpenedStates = new();
        private int _lastOpenedOrder;

        protected abstract TState NoneState { get; }

        private void Reset()
        {
            LastOpenedState = NoneState;
        }

        public TransferInfo<TState> OpenState(TState state)
        {
            if (LastOpenedState.Equals(state))
            {
                Debug.LogWarning($"Try to go to the same screen {state}");
                return TransferInfo<TState>.None;
            }

            var transferInfo = new TransferInfo<TState>(LastOpenedState, state);
            LastOpenedState = state;
            _orderedOpenedStates.Add(state, _lastOpenedOrder++);
            return transferInfo;
        }

        public TransferInfo<TState> CloseLastState()
        {
            if (_orderedOpenedStates.Count == 0)
                return TransferInfo<TState>.None;

            var lastOpenedState = _orderedOpenedStates.MaxByScalar(p => p.Value).Key;
            return CloseState(lastOpenedState);
        }

        public TransferInfo<TState> CloseState(TState state)
        {
            if (!_orderedOpenedStates.Remove(state))
            {
                Debug.LogWarning($"Try to close not opened state: {state}");
                return TransferInfo<TState>.None;
            }

            var openedState = LastOpenedState;
            if (_orderedOpenedStates.Count == 0)
            {
                _lastOpenedOrder = 0;
                LastOpenedState = NoneState;
            }
            else
            {
                LastOpenedState = _orderedOpenedStates.MaxByScalar(p => p.Value).Key;
            }

            var transferInfo = new TransferInfo<TState>(openedState, LastOpenedState, true);
            return transferInfo;
        }
    }
}