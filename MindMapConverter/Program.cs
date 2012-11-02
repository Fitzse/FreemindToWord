using System;
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
                    var actors = root.Elements("node").Select(CreateActor);
                    using(var wordDoc = WordprocessingDocument.Create("UserStories.docx", WordprocessingDocumentType.Document))
                    {
                        var styles = ExtractStylesPart("Template.dotx");
                        var theme = ExtractThemesPart("Template.dotx");
                        var mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.AddNewPart<StyleDefinitionsPart>();
                        mainPart.AddNewPart<ThemePart>();
                        mainPart.Document = new Document();
                        styles.Save(new StreamWriter(mainPart.StyleDefinitionsPart.GetStream(FileMode.Create, FileAccess.Write)));
                        theme.Save(new StreamWriter(mainPart.ThemePart.GetStream(FileMode.Create, FileAccess.Write)));
                        var body = mainPart.Document.AppendChild(new Body());
                        var paragraph = body.AppendChild(new Paragraph());
                        paragraph.ParagraphProperties = new ParagraphProperties(){ParagraphStyleId =  new ParagraphStyleId(){Val = "Heading4"}};
                        var run = paragraph.AppendChild(new Run());
                        run.AppendChild(new Text("Test STUFF"));
                        wordDoc.Close();
                    }
                }
            }
            else
            {
                Console.WriteLine("Expects a single argument of the freemind map file path");
            }
        }
        // Extract the styles or stylesWithEffects part from a 
        // word processing document as an XDocument instance.
        public static XDocument ExtractStylesPart(
          string fileName)
        {
            // Declare a variable to hold the XDocument.
            XDocument styles = null;

            // Open the document for read access and get a reference.
            using (var document =
                WordprocessingDocument.Open(fileName, false))
            {
                // Get a reference to the main document part.
                var docPart = document.MainDocumentPart;

                var stylesPart = docPart.StyleDefinitionsPart;

                // If the part exists, read it into the XDocument.
                if (stylesPart != null)
                {
                    using (var reader = XmlNodeReader.Create(
                      stylesPart.GetStream(FileMode.Open, FileAccess.Read)))
                    {
                        // Create the XDocument.
                        styles = XDocument.Load(reader);
                    }
                }
            }
            // Return the XDocument instance.
            return styles;
        }

        public static XDocument ExtractThemesPart(
          string fileName)
        {
            // Declare a variable to hold the XDocument.
            XDocument styles = null;

            // Open the document for read access and get a reference.
            using (var document =WordprocessingDocument.Open(fileName, false))
            {
                // Get a reference to the main document part.
                var docPart = document.MainDocumentPart;

                var themePart = docPart.ThemePart;

                // If the part exists, read it into the XDocument.
                if (themePart != null)
                {
                    using (var reader = XmlNodeReader.Create(
                      themePart.GetStream(FileMode.Open, FileAccess.Read)))
                    {
                        // Create the XDocument.
                        styles = XDocument.Load(reader);
                    }
                }
            }
            // Return the XDocument instance.
            return styles;
        }

        // Return styleid that matches the styleName, or null when there's no match.
        public static string GetStyleIdFromStyleName(WordprocessingDocument doc, string styleName)
        {
            StyleDefinitionsPart stylePart = doc.MainDocumentPart.StyleDefinitionsPart;
            string styleId = stylePart.Styles.Descendants<StyleName>()
                .Where(s => s.Val.Value.Equals(styleName) && 
                    (((Style)s.Parent).Type == StyleValues.Paragraph))
                .Select(n => ((Style)n.Parent).StyleId).FirstOrDefault();
            return styleId;
        }

        static Story CreateStory(XElement element)
        {
            var text = "";
            var textAttribute = element.Attribute("TEXT");
            if(textAttribute != null)
            {
                text = textAttribute.Value;
            }
            var story = new Story(text);
            story.Children = element.Elements("node").Select(CreateStory);
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
            var actor = new Actor(name);
            actor.Stories = element.Elements("node").Select(CreateStory);
            return actor;
        }
    }
}
