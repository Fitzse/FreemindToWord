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
            var article = IsVowel(actor.Name[0]) && actor.Name.ToLower() != "user" ? "an" : "a";
            var text = Char.ToLower(Text[0]) + Text.Substring(1);
            return String.Format("As {0} {1} I {2}", article, actor.Name, text);
        }

        public static bool IsVowel(char c)
        {
            return Vowels.Contains(Char.ToLower(c));
        }
    }
}
