using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AYellowpaper.SerializedCollections.Editor.Search
{
    public class NameMatcher : Matcher
    {
        public override bool IsMatch(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.boxedValue is Object obj && obj != null)
            {
                if (obj.name.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}