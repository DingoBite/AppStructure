using UnityEngine;

namespace AppStructure.AdaptiveView
{
    public interface IScreenOrientationHandler
    {
        public void Adapt(ScreenOrientation oldOrientation, ScreenOrientation newOrientation);
    }
}