using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FreeMind.Models;

namespace FreeMind
{
    public class Converter
    {
        public static IEnumerable<Actor> GetActorsFromFile(String filePath)
        {
            using (var mapFile = File.OpenRead(filePath))
            {
                var xDoc = XDocument.Load(mapFile);
                var root = xDoc.Root;
                if (root != null )
                {
                    var actorElement = root.Element("node");
                    if(actorElement != null)
                    {
                        return actorElement.Elements("node").Select(CreateActor);
                    }
                }
            }
            return Enumerable.Empty<Actor>();
        }

        private static Story CreateStory(XElement element)
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

        private static Actor CreateActor(XElement element)
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
