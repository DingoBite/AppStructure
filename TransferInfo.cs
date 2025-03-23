using System;
using System.Collections.Generic;

namespace AppStructure
{
    [Serializable]
    public class TransferInfo<TState>
    {
        public readonly TState From;
        public readonly TState To;
        public readonly bool IsFromBack;

        private static readonly HashSet<TState> EmptyInvalidBackStates = new();
        protected virtual HashSet<TState> InvalidBackStates => EmptyInvalidBackStates;
        
        public TransferInfo(TState from, TState to, bool isFromBack = false)
        {
            IsFromBack = isFromBack;
            From = from;
            To = to;
        }
        
        public TransferInfo<TState> SwapStates(bool isFromBack) => new(To, From, isFromBack);
        public TransferInfo<TState> SwapStates() => new(To, From, IsFromBack);
        
        public bool IsNone => From != null && From.Equals(default(TState)) && To != null && To.Equals(default(TState));
        public bool ValidBack => !IsNone && !InvalidBackStates.Contains(From) && !InvalidBackStates.Contains(To);
        public static readonly TransferInfo<TState> None = new (default, default);

        public override string ToString() => $"{From} - {To} : {nameof(IsFromBack)} {IsFromBack}";
    }
}