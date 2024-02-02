using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ZoboUI.Editor.Tests
{
    [TestFixture]
    public class DefaultClassExtractorTests
    {

        private DefaultClassExtractor.Config configWithDefaultSeparator;

        private DefaultClassExtractor.Config configWithCustomSeparator;
        private DefaultClassExtractor.Config configWithCustomPrefix;

        private DefaultClassExtractor.Config configWithClassTagFilter;

        private DefaultClassExtractor.Config configWithModifierFilter;

        private DefaultClassExtractor.Config configWithKnownKeysFilter;



        [SetUp]
        public void SetUp()
        {
            configWithDefaultSeparator = new DefaultClassExtractor.Config("_");

            configWithCustomSeparator = new DefaultClassExtractor.Config("--");

            configWithCustomPrefix = new DefaultClassExtractor.Config("_", "zb-");

            configWithClassTagFilter = new DefaultClassExtractor.Config("_");
            configWithClassTagFilter.FilterByKnownClassTags = new List<string> { "bg", "text", "rounded" };

            configWithModifierFilter = new DefaultClassExtractor.Config();
            configWithModifierFilter.FilterByKnownModifiers = new List<string> { "hover", "focus" };

            configWithKnownKeysFilter = new DefaultClassExtractor.Config();
            configWithKnownKeysFilter.FilterByKnownKeys = new List<string> { "sm", "xl", "12", "none", "bold" };


        }

        [Test]
        public void ExtractClasses_ValidContent_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithDefaultSeparator);

            var content = "some-content with [classes] and prefix-class";
            var expectedClasses = new List<string> { "prefix-class" }; // Add expected class strings here

            var actualClasses = extractor.ExtractClasses(content);

            CollectionAssert.IsSubsetOf(expectedClasses, actualClasses);
        }

        [Test]
        public void ExtractClasses_ValidUXMLContentWithDefaultSeparator_ReturnsExpectedClasses()
        {
            DefaultClassExtractor defaultExtractor = new DefaultClassExtractor(configWithDefaultSeparator);
            DefaultClassExtractor extractorWithClassTagFilter = new DefaultClassExtractor(configWithClassTagFilter);


            string xmlContent = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""True"">
                                <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />
                                <ui:VisualElement>
                                    <ui:VisualElement name=""InputHolder"" class=""rounded space-y-4 mt-minus-3 hover:test"">
                                        <ui:Label tabindex=""-1"" text=""Label"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""hover_text-red-800"" />
                                        <ui:Button text=""Button"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""bg-blue-300"" />
                                    </ui:VisualElement>
                                </ui:VisualElement>
                            </ui:UXML>";

            var expectedClassesForDefault = new List<string> { "space-y-4", "mt-minus-3", "hover_text-red-800", "bg-blue-300", "rounded" };

            var excludedValuesForDefault = new List<string> { "xmlns:", "<ui:UXML", @" <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />" };

            var actualClasses = defaultExtractor.ExtractClasses(xmlContent).ToList();




            foreach (var item in expectedClassesForDefault)
            {
                CollectionAssert.Contains(actualClasses, item);
            }

            foreach (var item in excludedValuesForDefault)
            {
                CollectionAssert.DoesNotContain(actualClasses, item);
            }


        }

        [Test]
        public void ExtractClasses_ValidUXMLContentWithClassTagFilter_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractorWithClassTagFilter = new DefaultClassExtractor(configWithClassTagFilter);



            string xmlContent = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""True"">
                                <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />
                                <ui:VisualElement>
                                    <ui:VisualElement name=""InputHolder"" class=""rounded-full space-y-4 mt-minus-3 hover:test"">
                                        <ui:Label tabindex=""-1"" text=""Label"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""hover_text-red-800"" />
                                        <ui:Button text=""Button"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""bg-blue-300 rounded"" />
                                    </ui:VisualElement>
                                </ui:VisualElement>
                            </ui:UXML>";

            var expectedClassesForClassTagFilter = new List<string> { "bg-blue-300", "hover_text-red-800", "rounded-full", "rounded" };

            var excludedValuesForDefault = new List<string> { "parse-escape-sequences", "tabindex", "xmlns:", "<ui:UXML", @" <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />", "space-y-4", "mt-minus-3" };

            var actualClasses = extractorWithClassTagFilter.ExtractClasses(xmlContent).ToList();




            foreach (var item in expectedClassesForClassTagFilter)
            {
                CollectionAssert.Contains(actualClasses, item);
            }

            foreach (var item in excludedValuesForDefault)
            {
                CollectionAssert.DoesNotContain(actualClasses, item);
            }


        }

        [Test]
        public void ExtractClasses_ValidUXMLContentWithModifierFilter_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractorWithModifierFilter = new DefaultClassExtractor(configWithModifierFilter);

            string xmlContent = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""True"">
                                <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />
                                <ui:VisualElement>
                                    <ui:VisualElement name=""InputHolder"" class=""rounded-full space-y-4 mt-minus-3 focused_bg_red-200"">
                                        <ui:Label tabindex=""-1"" text=""Label"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""hover_text-red-800"" />
                                        <ui:Button text=""Button"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""bg-blue-300 rounded"" />
                                    </ui:VisualElement>
                                </ui:VisualElement>
                            </ui:UXML>";

            var expectedClassesForModifierFilter = new List<string> { "focused_bg_red-200", "hover_text-red-800" };

            var excludedValuesForModifierFilter = new List<string> { "parse-escape-sequences", "tabindex", "xmlns:", "<ui:UXML", @" <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />", "space-y-4", "mt-minus-3", "bg-blue-300", "rounded-full", "rounded" };

            var extractedClasses = extractorWithModifierFilter.ExtractClasses(xmlContent).ToList();


            foreach (var item in expectedClassesForModifierFilter)
            {
                CollectionAssert.Contains(extractedClasses, item);
            }

            foreach (var item in excludedValuesForModifierFilter)
            {
                CollectionAssert.DoesNotContain(extractedClasses, item);
            }

        }

        [Test]
        public void ExtractClasses_ValidUXMLContentWithKnownKeysFilter_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractorWithKnownKeysFilter = new DefaultClassExtractor(configWithKnownKeysFilter);

            string xmlContent = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""True"">
                                    <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />
                                    <ui:VisualElement>
                                        <ui:VisualElement name=""InputHolder"" class=""rounded-sm space-y-12 mt-minus-3 focused_bg_red-200"">
                                            <ui:Label tabindex=""-1"" text=""Label"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""hover_flex-none"" />
                                            <ui:Button text=""Button"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""bg-blue-300 rounded"" />
                                        </ui:VisualElement>
                                    </ui:VisualElement>
                                </ui:UXML>";

            var expectedClassesForKnownKeysFilter = new List<string> { "rounded-sm", "space-y-12", "hover_flex-none" };

            var excludedValuesForKnownKeysFilter = new List<string> { "focused_bg_red-200", "mt-minus-3", "bg-blue-300" };

            var extractedClasses = extractorWithKnownKeysFilter.ExtractClasses(xmlContent).ToList();

            foreach (var item in expectedClassesForKnownKeysFilter)
            {
                CollectionAssert.Contains(extractedClasses, item);
            }

            foreach (var item in excludedValuesForKnownKeysFilter)
            {
                CollectionAssert.DoesNotContain(extractedClasses, item);
            }

        }

        [Test]
        public void ExtractClasses_ValidUXMLContentWithAllFilters_ReturnsExpectedClasses()
        {
            DefaultClassExtractor.Config configWithAllFilters = new DefaultClassExtractor.Config("_", "", configWithClassTagFilter.FilterByKnownClassTags, configWithModifierFilter.FilterByKnownModifiers, configWithKnownKeysFilter.FilterByKnownKeys);
            DefaultClassExtractor extractorWithKnownKeysFilter = new DefaultClassExtractor(configWithAllFilters);

            string xmlContent = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""True"">
                                    <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />
                                    <ui:VisualElement>
                                        <ui:VisualElement name=""InputHolder"" class=""rounded-sm space-y-12 mt-minus-3 focused_bg_red-200  enabled_py-14"">
                                            <ui:Label tabindex=""-1"" text=""Label"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""hover_flex-none"" />
                                            <ui:Button text=""Button"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""bg-blue-300 rounded"" />
                                        </ui:VisualElement>
                                    </ui:VisualElement>
                                </ui:UXML>";


            // The class is considered valid if it contains any of the known class tags, OR if it contains any of the known modifiers, OR if it contains any of the known keys
            var expectedClasses = new List<string> { "rounded-sm", "space-y-12", "hover_flex-none" };

            var excludedValuesForKnownKeysFilter = new List<string> { "enabled_py-14", "mt-minus-3", };

            var extractedClasses = extractorWithKnownKeysFilter.ExtractClasses(xmlContent).ToList();

            foreach (var item in expectedClasses)
            {
                CollectionAssert.Contains(extractedClasses, item);
            }

            foreach (var item in excludedValuesForKnownKeysFilter)
            {
                CollectionAssert.DoesNotContain(extractedClasses, item);
            }

        }



        [Test]
        public void ExtractClasses_ValidUXMLContentWithCustomPrefix_ReturnsExpectedClasses()
        {
            DefaultClassExtractor defaultExtractor = new DefaultClassExtractor(configWithCustomPrefix);

            string uxmlContentWithCustomPrefix = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""True"">
                                <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />
                                <ui:VisualElement>
                                    <ui:VisualElement name=""InputHolder"" class=""zb-space-y-4 zb-mt-minus-3"">
                                        <ui:Label tabindex=""-1"" text=""Label"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""hover--text-red-800"" />
                                        <ui:Button text=""Button"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""bg-blue-300 focus--text-blue-500"" />
                                    </ui:VisualElement>
                                </ui:VisualElement>
                            </ui:UXML>";
            var expectedClasses = new List<string> { "zb-space-y-4", "zb-mt-minus-3", "hover--text-red-800", "bg-blue-300", "focus--text-blue-500" };

            var actualClasses = defaultExtractor.ExtractClasses(uxmlContentWithCustomPrefix).ToList();


            CollectionAssert.IsSubsetOf(expectedClasses, actualClasses);
        }

        [Test]
        public void ExtractClasses_ValidUXMLContentWithClassTagFilterAndCustomPrefix_ReturnsExpectedClasses()
        {
            DefaultClassExtractor.Config configWithClassTagFilterAndCustomPrefix = new DefaultClassExtractor.Config("_", "zb-", configWithClassTagFilter.FilterByKnownClassTags);



            DefaultClassExtractor extractorWithClassTagFilterAndCustomPrefix = new DefaultClassExtractor(configWithClassTagFilterAndCustomPrefix);

            string xmlContent = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""True"">
                            <Style src=""project://database/Assets/Styles/generated.uss?fileID=7433441132597879392&amp;guid=2a2829b8dbf44244fbed2be91da0c3f8&amp;type=3#generated"" />
                            <ui:VisualElement>
                                <ui:VisualElement name=""InputHolder"" class=""zb-rounded-full space-y-4 mt-minus-3 hover:test"">
                                    <ui:Label tabindex=""-1"" text=""Label"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""hover_text-red-800"" />
                                    <ui:Button text=""Button"" parse-escape-sequences=""true"" display-tooltip-when-elided=""true"" class=""bg-blue-300 rounded"" />
                                </ui:VisualElement>
                            </ui:VisualElement>
                        </ui:UXML>";

            var expectedClassesForClassTagFilterAndCustomPrefix = new List<string> { "zb-rounded-full", "hover_text-red-800" };

            var excludedValuesForClassTagFilterAndCustomPrefix = new List<string> { "space-y-4", "mt-minus-3", "rounded-full" };

            var extractedClasses = extractorWithClassTagFilterAndCustomPrefix.ExtractClasses(xmlContent).ToList();

            foreach (var item in expectedClassesForClassTagFilterAndCustomPrefix)
            {
                CollectionAssert.Contains(extractedClasses, item);
            }

            foreach (var item in excludedValuesForClassTagFilterAndCustomPrefix)
            {
                CollectionAssert.DoesNotContain(extractedClasses, item);
            }
        }

        [Test]
        public void ExtractClasses_CSharpScriptContent_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithDefaultSeparator);

            string content = @"using UnityEngine;  
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
{
    public class CoreProperty : ScriptableObject
    {
        public string Name;
        public CorePropertyValueType ValueType;
        public string Value;

        public void HideVisualElement(VisualElement element){

            // Add the hidden class to the element
            element.AddToClassList(""hidden"");

        }

        public void MakeBackgroundTransparent(VisualElement element){

            // Add the bg-transparent class to the element
            element.AddToClassList(""bg-transparent"");

            }

    }";

            var expectedClasses = new List<string> { "hidden", "bg-transparent" };

            var actualClasses = extractor.ExtractClasses(content).ToList();


            foreach (var item in expectedClasses)
            {
                CollectionAssert.Contains(actualClasses, item);
            }

        }

        [Test]
        public void ExtractClasses_CSharpScriptContentWithClassTagFilter_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithClassTagFilter);

            string content = @"using UnityEngine;  
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
{
    public class CoreProperty : ScriptableObject
    {
        public string Name;
        public CorePropertyValueType ValueType;
        public string Value;

        public string GetHoveredClassName(){
            return ""text-red-500"";
        }

        public void HideVisualElement(VisualElement element){

            // Add the hidden class to the element
            element.AddToClassList(""hidden"");

        }

        public void MakeBackgroundTransparent(VisualElement element){

            // Add the bg-transparent class to the element
            element.AddToClassList(""bg-transparent"");

            }

    }";

            var expectedClasses = new List<string> { "bg-transparent", "text-red-500" };

            var excludedValues = new List<string> { "hidden" };

            var extractedClasses = extractor.ExtractClasses(content).ToList();

            foreach (var item in expectedClasses)
            {
                CollectionAssert.Contains(extractedClasses, item);
            }

            foreach (var item in excludedValues)
            {
                CollectionAssert.DoesNotContain(extractedClasses, item);
            }



        }

        [Test]
        public void ExtractClasses_CSharpScriptContentWithModifierFilter_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithModifierFilter);
            string content = @"using UnityEngine;  
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
{
    public class CoreProperty : ScriptableObject
    {
        public string Name;
        public CorePropertyValueType ValueType;
        public string Value;

        public string GetHoveredClassName(){
            return ""hover_text-red-500"";
        }

         public string GetFocusedClassName(){
            return ""focus_bg-blue-800"";
        }

        public void HideVisualElement(VisualElement element){

            // Add the hidden class to the element
            element.AddToClassList(""hidden"");

        }

        public void MakeBackgroundTransparent(VisualElement element){

            // Add the bg-transparent class to the element
            element.AddToClassList(""bg-transparent"");

            }

    }";

            var expectedClasses = new List<string> { "focus_bg-blue-800", "hover_text-red-500" };

            var excludedValues = new List<string> { "hidden", "bg-transparent" };

            var extractedClasses = extractor.ExtractClasses(content).ToList();

            foreach (var item in expectedClasses)
            {
                CollectionAssert.Contains(extractedClasses, item);
            }

            foreach (var item in excludedValues)
            {
                CollectionAssert.DoesNotContain(extractedClasses, item);
            }


        }


        [Test]
        public void ExtractClasses_CSharpScriptContentWithKnownKeyFilter_ReturnsExpectedClasses()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithKnownKeysFilter);
            string content = @"using UnityEngine;  
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
{
    public class CoreProperty : ScriptableObject
    {
        public string Name;
        public CorePropertyValueType ValueType;
        public string Value;

        public string GetHoveredClassName(){
            return ""hover_flex-none"";
        }

         public string GetClassName(){
            return ""rounded-sm"";
        }

        public void HideVisualElement(VisualElement element){

            // Add the hidden class to the element
            element.AddToClassList(""hidden space-y-12"");

        }

        public void MakeBackgroundTransparent(VisualElement element){

            // Add the bg-transparent class to the element
            element.AddToClassList(""bg-transparent"");

            }

    }";

            var expectedClasses = new List<string> { "rounded-sm", "space-y-12", "hover_flex-none" };

            var excludedValues = new List<string> { "hidden", "bg-transparent" };

            var extractedClasses = extractor.ExtractClasses(content).ToList();

            foreach (var item in expectedClasses)
            {
                CollectionAssert.Contains(extractedClasses, item);
            }

            foreach (var item in excludedValues)
            {
                CollectionAssert.DoesNotContain(extractedClasses, item);
            }

        }



        [Test]
        public void ExtractClasses_WithSpecialCharacters_ReturnsCorrectClasses()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithDefaultSeparator);

            string contentWithSpecialChars = @"<ui:VisualElement class='bg-blue-300 focus--text-blue-500 hover:text-red-500/50'></ui:VisualElement>";
            var expectedClasses = new List<string> { "bg-blue-300", "focus--text-blue-500", "hover:text-red-500/50" };

            var actualClasses = extractor.ExtractClasses(contentWithSpecialChars).ToList();

            foreach (var item in expectedClasses)
            {
                CollectionAssert.Contains(actualClasses, item);
            }
        }

        [Test]
        public void ExtractClasses_InMarkdownCodeFences()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithDefaultSeparator);


            string content = "<!-- markdown test: `.font-sm`, `.font-inter` -->";

            var actualClasses = extractor.ExtractClasses(content).ToList();

            CollectionAssert.Contains(actualClasses, "font-sm");
            CollectionAssert.Contains(actualClasses, "font-inter");
            CollectionAssert.DoesNotContain(actualClasses, ".font-sm");
            CollectionAssert.DoesNotContain(actualClasses, ".font-inter");
        }

        [Test]
        public void ExtractClasses_WithAngleBrackets()
        {
            DefaultClassExtractor extractor = new DefaultClassExtractor(configWithDefaultSeparator);


            string htmlContent = @"<ui:VisualElement class=""text-red-400 <% if (useFlex) { %>flex-1<% } %>"">test</ui:VisualElement>";
            var extractedClasses = extractor.ExtractClasses(htmlContent).ToList();

            CollectionAssert.Contains(extractedClasses, "text-red-400");
            CollectionAssert.Contains(extractedClasses, "flex-1");
            CollectionAssert.DoesNotContain(extractedClasses, ">flex-1");
            CollectionAssert.DoesNotContain(extractedClasses, "flex-1<");
        }




    }
}