using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppStructure.StateMachines
{
    public abstract class GoBackSupportStateMachine<TState> : MonoBehaviour where TState : Enum
    {
        [field: SerializeField] public TState CurrentState { get; private set; }
        
        [SerializeField] private List<TransferInfo<TState>> _transferHistory = new (); 
        [SerializeField] private int _historySize = 10;

        protected TState LastNotNoneState { get; private set; } 
        
        protected abstract TState NoneState { get; }

        protected void ResetState()
        {
            CurrentState = NoneState;
            LastNotNoneState = NoneState;
        }

        public TransferInfo<TState> GoToState(TState state)
        {
            if (CurrentState.Equals(state))
            {
                Debug.LogWarning($"Try to go to the same screen {state}");
                return TransferInfo<TState>.None;
            }
            
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
            if (_transferHistory.Count == 0)
                return TransferInfo<TState>.None;

            var lastTransfer = _transferHistory[^1];
            _transferHistory.RemoveAt(_transferHistory.Count - 1);
            if (!CurrentState.Equals(NoneState))
                LastNotNoneState = CurrentState;
            CurrentState = lastTransfer.From;
            return lastTransfer.SwapStates(true);
        }
        
        private void PushTransfer(TransferInfo<TState> screenTransferInfo)
        {
            if (!screenTransferInfo.ValidBack)
                return;
            if (_transferHistory.Count >= _historySize)
                _transferHistory.RemoveAt(0);
            _transferHistory.Add(screenTransferInfo);
        }
    }
}