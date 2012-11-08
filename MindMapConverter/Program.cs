using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FreeMind.Models;

namespace MindMapConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var mapPath = args[0];
                var docPath = mapPath + ".docx";
                var csvPath = mapPath + ".csv";
                var actors = FreeMind.Converter.GetActorsFromFile(mapPath).ToList();
                using(var wordDoc = CreateNewDocument(docPath))
                {
                    var body = wordDoc.MainDocumentPart.Document.AppendChild(new Body());
                    foreach (var actor in actors)
                    {
                        AddText(body, actor.Name, 0);
                        AddTextWithHeading(body, "Description", actor.Description, false);
                        var sectionNumber = 0;
                        foreach (var story in actor.Stories)
                        {
                            AppendStory(body, actor, story, new List<int>{sectionNumber});
                            sectionNumber++;
                        }
                    }
                    wordDoc.Close();
                }
                using(var csvFile = File.Create(csvPath))
                {
                    var line = String.Format("Title,Story,Requirments Met\n");
                    var bytes = Encoding.UTF8.GetBytes(line);
                    csvFile.Write(bytes, 0, bytes.Length);
                    foreach (var actor in actors)
                    {
                        foreach (var s in actor.Stories)
                        {
                            AddStoryToFile(csvFile, s, actor);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Expects a single argument of the freemind map file path");
            }
        }

        private static void AddStoryToFile(FileStream file, Story story, Actor actor)
        {
            var line = String.Format("\"{0}\",\"{1}\",\"{2}\"\n", story.Title, story.GetNarrative(actor),
                                     String.Join(",", story.RelatedRequirements));
            var bytes = Encoding.UTF8.GetBytes(line);
            file.Write(bytes, 0, bytes.Length);
            foreach (var s in story.Children)
            {
                AddStoryToFile(file, s, actor);
            }
        }

        private static void AppendStory(Body body, Actor actor, Story story, List<int> level)
        {
            var sectionNumber = level.Last() + 1;
            var sectionList = level.Take(level.Count - 1).ToList();
            sectionList.Add(sectionNumber);
            var sectionDisplay = String.Join(".", sectionList);
            AddText(body, sectionDisplay + " - " + story.Title, level.Count);
            AddTextWithHeading(body, "Story", story.GetNarrative(actor));
            AddTextWithHeading(body, "Requirements Met", String.Join(", ", story.RelatedRequirements));
            if (story.Children.Any())
            {
                var sectionCount = 0;
                sectionList.Add(sectionCount);
                foreach (var s in story.Children)
                {
                    AppendStory(body, actor, s, sectionList);
                    sectionCount++;
                    sectionList.RemoveAt(sectionList.Count-1);
                    sectionList.Add(sectionCount);
                }
            }
        }

        private static string GetStyleId(int level)
        {
            if (level > 8)
                return "Normal";
            return "Heading" + (level + 2);
        }

        private static void AddText(Body body, string text, int level)
        {
            var paragraph = body.AppendChild(new Paragraph());
            paragraph.ParagraphProperties = new ParagraphProperties()
                                                {
                                                    OutlineLevel = new OutlineLevel { Val = level },
                                                    ParagraphStyleId = new ParagraphStyleId() { Val = GetStyleId(level) }
                                                };
            var run = paragraph.AppendChild(new Run());
            if (level > 1)
            {
                run.AppendChild(new TabChar());
            }
            run.AppendChild(new Text(text));
        }

        private static void AddTextWithHeading(Body body, string heading, string text, bool indent=true)
        {
            var paragraph = body.AppendChild(new Paragraph());
            if (indent)
            {
                paragraph.ParagraphProperties = new ParagraphProperties
                                                    {Indentation = new Indentation() {Start = "720"}};
            }
            var run = paragraph.AppendChild(new Run());
            run.RunProperties = new RunProperties(){Bold = new Bold()};
            run.AppendChild(new Text(heading));
            run = paragraph.AppendChild(new Run());
            run.AppendChild(new Text(": " + text));
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

            using (var document = WordprocessingDocument.Open(fileName, false))
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
    }
}
