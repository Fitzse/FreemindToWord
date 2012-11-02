using System;
using System.Collections.Generic;

namespace MindMapConverter
{
    public class Story
    {
        private static readonly List<char> Vowels = new List<char> {'a', 'e', 'i', 'o', 'u'};
        public String Text { get; set; }
        public IEnumerable<Story> Children { get; set; }

        public Story(String text)
        {
            Text = text;
        }

        public String GetNarrative(Actor actor)
        {
            var article = IsVowel(Text[0]) ? "an" : "a";
            return String.Format("As {0} {1} I {2}", article, actor.Name, Text);
        }

        public static bool IsVowel(char c)
        {
            return Vowels.Contains(c);
        }
    }
}
