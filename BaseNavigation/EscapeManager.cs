using System;

namespace AppStructure.BaseNavigation
{
    public static class EscapeManager
    {
        public static event Action<object, int> EscapePressed;

        public static void Escape(object source, int order) => EscapePressed?.Invoke(source, order);
    }
}