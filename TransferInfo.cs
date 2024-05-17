using System;
using System.Collections.Generic;

namespace AppStructure
{
    [Serializable]
    public class TransferInfo<TState> : IComparable<TransferInfo<TState>> where TState : Enum
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

        public int CompareTo(TransferInfo<TState> other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var fromComparison = From.CompareTo(other.From);
            if (fromComparison != 0) return fromComparison;
            return To.CompareTo(other.To);
        }

        public bool IsNone => From.Equals(default) && To.Equals(default);
        public bool ValidBack => !IsNone && !InvalidBackStates.Contains(From) && !InvalidBackStates.Contains(To);
        public static readonly TransferInfo<TState> None = new (default, default);

        public override string ToString() => $"{From} - {To} : {nameof(IsFromBack)} {IsFromBack}";
    }
}