using System;
using System.Collections.Generic;
using System.Linq;
using ZoboUI.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(InspectorFontAssetValueDictionaryDisplay))]
    public class FontAssetDictionaryValueDisplayInspector : BaseDisplayDrawerWithAssetValue
    {
        Dictionary<InspectorFontValueDisplayType, Type> displayTypeToFieldIdMap = new Dictionary<InspectorFontValueDisplayType, Type>
        {
            { InspectorFontValueDisplayType.FontAsset, typeof(FontAsset) },
        };

        protected override Enum ConvertPropertyIntValueToEnum(int value)
        {
            return (InspectorFontValueDisplayType)value;
        }

        protected override bool DisplayTypeisCustomValue(Enum displayType)
        {
            return (InspectorFontValueDisplayType)displayType == InspectorFontValueDisplayType.Custom;
        }


        protected override string GetDisplayTypePropertyName()
        {
            return "DisplayType";
        }


        protected override Type GetTypeValueForDisplayType(Enum displayType)
        {
            return displayTypeToFieldIdMap[(InspectorFontValueDisplayType)displayType];
        }
    }

}