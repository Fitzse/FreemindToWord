using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MindMapConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var mapPath = args[0];
                var outputFile = mapPath + ".docx";
                using (var mapFile = File.OpenRead(mapPath))
                {
                    var xDoc = XDocument.Load(mapFile);
                    var root = xDoc.Root;
                    var actors = root.Element("node").Elements("node").Select(CreateActor);
                    using(var wordDoc = CreateNewDocument(outputFile))
                    {
                        var body = wordDoc.MainDocumentPart.Document.AppendChild(new Body());
                        foreach (var actor in actors)
                        {
                            AddText(body, actor.Name, "Heading1");
                            foreach (var story in actor.Stories)
                            {
                                AppendStory(body, actor, story, 0);
                            }
                        }
                        wordDoc.Close();
                    }
                }
            }
            else
            {
                Console.WriteLine("Expects a single argument of the freemind map file path");
            }
        }

        private static void AppendStory(Body body, Actor actor, Story story, int level)
        {
            AddText(body, story.GetNarrative(actor), GetStyleId(level));
            foreach (var s in story.Children)
            {
                AppendStory(body, actor, s, ++level);
            }
        }

        private static string GetStyleId(int level)
        {
            switch (level)
            {
                case 0:
                    return "Heading2";
                case 1:
                    return "Heading3";
                case 2:
                    return "Heading4";
                default:
                    return "Heading5";
            }
        }

        private static void AddText(Body body, string text, string styleId)
        {
            var paragraph = body.AppendChild(new Paragraph());
            paragraph.ParagraphProperties = new ParagraphProperties()
                                                {ParagraphStyleId = new ParagraphStyleId() {Val = styleId}};
            var run = paragraph.AppendChild(new Run());
            run.AppendChild(new Text(text));
        }

        private static WordprocessingDocument CreateNewDocument(string filePath)
        {
            var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document); 
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            ApplyStylesFromTemplate(doc, "Template.dotx");
            ApplyThemeFromTemplate(doc, "Templat.dotx");
            return doc;
        }

        private static void ApplyStylesFromTemplate(WordprocessingDocument doc, string templatePath)
        {
            doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            var styleDefinition = doc.MainDocumentPart.StyleDefinitionsPart;
            var styles = ExtractStyleDefinition("Template.dotx");
            styles.Save(new StreamWriter(styleDefinition.GetStream(FileMode.Create, FileAccess.Write)));
        }

        private static void ApplyThemeFromTemplate(WordprocessingDocument doc, string templatePath)
        {
            doc.MainDocumentPart.AddNewPart<ThemePart>();
            var themePart = doc.MainDocumentPart.ThemePart;
            var theme = ExtractTheme("Template.dotx");
            theme.Save(new StreamWriter(themePart.GetStream(FileMode.Create, FileAccess.Write)));
        }

        public static XDocument ExtractStyleDefinition(string fileName)
        {
            XDocument styles = null;

            using (var document = WordprocessingDocument.Open(fileName, false))
            {
                var docPart = document.MainDocumentPart;
                var stylesPart = docPart.StyleDefinitionsPart;

                if (stylesPart != null)
                {
                    using (var reader = XmlNodeReader.Create(stylesPart.GetStream(FileMode.Open, FileAccess.Read)))
                    {
                        styles = XDocument.Load(reader);
                    }
                }
            }
            return styles;
        }

        public static XDocument ExtractTheme(string fileName)
        {
            XDocument theme = null;

            using (var document =WordprocessingDocument.Open(fileName, false))
            {
                var docPart = document.MainDocumentPart;
                var themePart = docPart.ThemePart;

                if (themePart != null)
                {
                    using (var reader = XmlNodeReader.Create(themePart.GetStream(FileMode.Open, FileAccess.Read)))
                    {
                        theme = XDocument.Load(reader);
                    }
                }
            }
            return theme;
        }

        static Story CreateStory(XElement element)
        {
            var text = "";
            var textAttribute = element.Attribute("TEXT");
            if(textAttribute != null)
            {
                text = textAttribute.Value;
            }
            var story = new Story(text)
                            {
                                Children = element.Elements("node").Select(CreateStory)
                            };
            return story;
        }

        static Actor CreateActor(XElement element)
        {
            var name = "";
            var nameAttribute = element.Attribute("TEXT");
            if(nameAttribute != null)
            {
                name = nameAttribute.Value;
            }
            var actor = new Actor(name)
                            {
                                Stories = element.Elements("node").Select(CreateStory)
                            };
            return actor;
        }
    }
}
