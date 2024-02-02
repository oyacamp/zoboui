using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.Tests
{
    /// <summary>
    /// Sample class for testing whether classes can be detected in the uss file
    /// </summary>
    public class TestScriptWithUss : MonoBehaviour
    {

        private readonly string classForRedHoverText = "hover_text_red-500";


        public void HideVisualElement(VisualElement visualElement)
        {
            visualElement.AddToClassList("hidden");
        }

        public void MakeBackgroundTransparent(VisualElement visualElement)
        {
            visualElement.AddToClassList("bg-transparent");
        }

        public void MakeBackgroundTransparentAndHoverTextRed(VisualElement visualElement)
        {
            visualElement.AddToClassList("bg-transparent");
            visualElement.AddToClassList(classForRedHoverText);
        }
    }
}