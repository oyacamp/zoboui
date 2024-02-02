using UnityEditor;
using UnityEngine.UIElements;

namespace ZoboUI.Editor
{

    /// <summary>
    /// A visual element that displays an alert message with an icon.
    /// </summary>
    public class AlertElement : VisualElement
    {
        private VisualElement alertIcon;
        private Label alertMessage;

        public enum AlertType
        {
            Info,
            Warning,
            Error
        }

        public AlertElement()
        {
            // Load the UXML file
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/Shared/AlertElement/AlertElement_UXML.uxml");
            visualTree.CloneTree(this);

            alertIcon = this.Q<VisualElement>("AlertIcon");
            alertMessage = this.Q<Label>("AlertMessage");
        }

        public void Initialize(AlertType type, string message)
        {
            SetAlertType(type);
            SetAlertMessage(message);
        }

        public void UpdateAlert(AlertType type, string message)
        {
            SetAlertType(type);
            SetAlertMessage(message);
        }

        private void SetAlertType(AlertType type)
        {
            alertIcon.RemoveFromClassList("themeconfig__alert__icon__info");
            alertIcon.RemoveFromClassList("themeconfig__alert__icon__warning");
            alertIcon.RemoveFromClassList("themeconfig__alert__icon__error");

            switch (type)
            {
                case AlertType.Info:
                    alertIcon.AddToClassList("themeconfig__alert__icon__info");
                    break;
                case AlertType.Warning:
                    alertIcon.AddToClassList("themeconfig__alert__icon__warning");
                    break;
                case AlertType.Error:
                    alertIcon.AddToClassList("themeconfig__alert__icon__error");
                    break;
            }
        }

        private void SetAlertMessage(string message)
        {
            alertMessage.text = message;
        }

        // UxmlFactory for AlertElement
        public new class UxmlFactory : UxmlFactory<AlertElement, UxmlTraits> { }

        // UxmlTraits for AlertElement
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _alertMessage = new UxmlStringAttributeDescription { name = "message", defaultValue = "My alert message" };
            private readonly UxmlEnumAttributeDescription<AlertType> _alertType = new UxmlEnumAttributeDescription<AlertType> { name = "type", defaultValue = AlertType.Error };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var alertElement = ve as AlertElement;
                alertElement.Initialize(
                    _alertType.GetValueFromBag(bag, cc),
                    _alertMessage.GetValueFromBag(bag, cc)
                );
            }
        }
    }

}
