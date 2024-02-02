using System;
using System.Collections.Generic;
using ZoboUI.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(InspectorImageValueDictionaryDisplay))]
    public class ImageValueDictionaryDisplayPropertyDrawer : BaseDisplayDrawerWithAssetValue
    {


        Dictionary<InspectorImageValueDictionaryDisplayType, Type> displayTypeToFieldIdMap = new Dictionary<InspectorImageValueDictionaryDisplayType, Type>
{
    { InspectorImageValueDictionaryDisplayType.Texture2D, typeof(UnityEngine.Texture2D) },
    { InspectorImageValueDictionaryDisplayType.Sprite, typeof(UnityEngine.Sprite) },
    { InspectorImageValueDictionaryDisplayType.RenderTexture, typeof(UnityEngine.RenderTexture) },
    //{ InspectorImageValueDictionaryDisplayType.VectorImage, typeof(VectorImage) }
};

        protected override Enum ConvertPropertyIntValueToEnum(int value)
        {
            return (InspectorImageValueDictionaryDisplayType)value;
        }

        protected override bool DisplayTypeisCustomValue(Enum displayType)
        {
            return (InspectorImageValueDictionaryDisplayType)displayType == InspectorImageValueDictionaryDisplayType.Custom;
        }

        protected override string GetDisplayTypePropertyName()
        {
            return "DisplayType";
        }


        protected override Type GetTypeValueForDisplayType(Enum displayType)
        {
            return displayTypeToFieldIdMap[(InspectorImageValueDictionaryDisplayType)displayType];
        }
    }


}
