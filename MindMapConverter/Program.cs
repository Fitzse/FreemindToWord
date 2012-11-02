using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;

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
                }
            }
            else
            {
                Console.WriteLine("Expects a single argument of the freemind map file path");
            }
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
