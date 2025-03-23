using System;
using TMPro;

namespace AppStructure.AdaptiveView
{
    [Serializable]
    public class TextFullPosition : RectFullPosition
    {
        public TextAlignmentOptions TextAlignment;
        
        public override void Collect(object obj)
        {
            if (obj is not TMP_Text text)
                return;
            base.Collect(text.rectTransform);
            TextAlignment = text.alignment;
        }
        
        public override void Apply(object obj)
        {
            if (obj is not TMP_Text text)
                return;

            text.alignment = TextAlignment;
        }
    }
}