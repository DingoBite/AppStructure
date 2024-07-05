using System;
using TMPro;

namespace AppStructure.AdaptiveView
{
    [Serializable]
    public class TextFullPosition : RectFullPosition
    {
        public TextAlignmentOptions TextAlignment;
        
        public TextFullPosition(TMP_Text text) : base(text.rectTransform)
        {
            TextAlignment = text.alignment;
        }
        
        public TextFullPosition() { }
        
        public void Apply(TMP_Text text)
        {
            if (text == null)
                return;

            text.alignment = TextAlignment;
        }
        
        public override bool Apply<T>(T obj)
        {
            var isType = typeof(T) == typeof(TMP_Text);
            if (isType)
            {
                var tmpText = obj as TMP_Text;
                base.Apply(tmpText?.rectTransform);
                Apply(tmpText);
            }
            return isType;
        }
    }
}