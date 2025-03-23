using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AppStructure.StateMachines
{
    public abstract class GoBackSupportStateMachine<TState> : MonoBehaviour
    {
        [field: SerializeField] public TState CurrentState { get; private set; }
        
        [SerializeField] private List<TransferInfo<TState>> _transferHistory = new (); 
        [SerializeField] private int _historySize = 10;

        private bool _isDirtyBack = true;
        private bool _isValidBack;
        private TransferInfo<TState> _lastBack;
        
        protected TState LastNotNoneState { get; private set; } 
        
        protected abstract TState NoneState { get; }

        public TransferInfo<TState> GoToState(TState state)
        {
            if (CurrentState.Equals(state))
            {
                Debug.LogWarning($"Try to go to the same screen {state}");
                return TransferInfo<TState>.None;
            }
            
            _isDirtyBack = true;
            var transferInfo = new TransferInfo<TState>(CurrentState, state);
            CurrentState = state;
            if (_transferHistory.Count > 0 && _transferHistory[^1] == transferInfo)
                _transferHistory.RemoveAt(_transferHistory.Count - 1);
            else 
                PushTransfer(transferInfo);
            if (!CurrentState.Equals(NoneState))
                LastNotNoneState = CurrentState;
            return transferInfo;
        }

        public TransferInfo<TState> GoBack()
        {
            if (!IsValidBack(out var firstBack) || firstBack == null || _transferHistory.Count == 0)
                return TransferInfo<TState>.None;
            _isDirtyBack = true;
            var backIndex = _transferHistory.IndexOf(firstBack);
            _transferHistory.RemoveRange(backIndex, _transferHistory.Count - backIndex);
            if (!CurrentState.Equals(NoneState))
                LastNotNoneState = CurrentState;
            firstBack = new TransferInfo<TState>(CurrentState, firstBack.From);
            CurrentState = firstBack.To;
            return firstBack;
        }

        public bool IsValidBack() => IsValidBack(out _);
        
        public bool IsValidBack(out TransferInfo<TState> firstBack)
        {
            if (_isDirtyBack)
            {
                _isValidBack = IsValidBack(_transferHistory, out firstBack);
                _isDirtyBack = false;
                _lastBack = firstBack;
            }
            firstBack = _lastBack;
            return _isValidBack;
        }

        private bool IsValidBack(IReadOnlyList<TransferInfo<TState>> history) => IsValidBack(history, out _);
        
        protected virtual bool IsValidBack(IReadOnlyList<TransferInfo<TState>> history, out TransferInfo<TState> firstBack)
        {
            firstBack = history.LastOrDefault();
            return true;
        }

        protected void ResetState()
        {
            CurrentState = NoneState;
            LastNotNoneState = NoneState;
            _isDirtyBack = true;
            _lastBack = null;
        }

        private void PushTransfer(TransferInfo<TState> screenTransferInfo)
        {
            if (_historySize <= 0 || !screenTransferInfo.ValidBack)
                return;
            if (_transferHistory.Count >= _historySize)
                _transferHistory.RemoveAt(0);
            _transferHistory.Add(screenTransferInfo);
        }
    }
}