using UnityEngine;

namespace AppStructure.AdaptiveView.Behaviours
{
    public interface IScreenOrientationHandler
    {
        public void Adapt(ScreenOrientation oldOrientation, ScreenOrientation newOrientation);
    }
}