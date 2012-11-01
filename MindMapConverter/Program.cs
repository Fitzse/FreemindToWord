using System;
using System.IO;
using System.Xml.Linq;
using Novacode;

namespace MindMapConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {

                var mapPath = args[0];
                    var outputPath = args[1];
                using (var mapFile = File.OpenRead(mapPath))
                {
                    var xDoc = XDocument.Load(mapFile);
                    var root = xDoc.Root;
                    using (var document = DocX.Create(outputPath))
                    {
                        foreach (var node in root.Descendants())
                        {
                            var text = node.Attribute("TEXT").Value;
                            document.InsertParagraph(text);
                        }
                        document.Save();
                    }
                }
            }
            else
            {
                Console.WriteLine("2 arguments expected: 1 - freemind map filepath and 2 - output file path");
            }
        }
    }
}
