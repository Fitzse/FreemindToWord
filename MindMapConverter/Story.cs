using System;
using System.Collections.Generic;
using System.Linq;

namespace MindMapConverter
{
    public class Story
    {
        private static readonly List<char> Vowels = new List<char> {'a', 'e', 'i', 'o', 'u'};
        public String Text { get; set; }
        public String Title { get; set; }

        public IEnumerable<Story> Children { get; set; }
        public IEnumerable<String> RelatedRequirements { get; set; }

        public Story(String text)
        {
            RelatedRequirements = ParseRequirements(text);
            Title = ParseTitle(text);
            Text = ParseStory(text);
        }

        private IEnumerable<String> ParseRequirements(string text)
        {
            var bracketIndex = text.IndexOf("[", System.StringComparison.Ordinal);
            if(bracketIndex > 0)
            {
                var requirements = text.Substring(bracketIndex + 1, text.Length - bracketIndex - 2);
                return requirements.Split(',').Select(x => x.Trim());
            }
            return new List<string>{"N/A"};
        }

        private String ParseStory(string text)
        {
            var colonIndex = text.IndexOf(":", System.StringComparison.Ordinal);
            var bracketIndex = text.IndexOf("[", StringComparison.Ordinal);
            if (colonIndex > 0)
            {
                return text.Substring(0, colonIndex-1).Trim();
            }

            if(colonIndex > 0)
            {
                return text.Substring(0, bracketIndex - 1).Trim();
            }

            return text;
        }
        
        private String ParseTitle(string text)
        {
            var colonIndex = text.IndexOf(":", System.StringComparison.Ordinal);
            var bracketIndex = text.IndexOf("[", StringComparison.Ordinal);
            if (colonIndex > 0 && bracketIndex > 0)
            {
                return text.Substring(colonIndex + 1, bracketIndex - colonIndex - 1).Trim();
            }

            if(colonIndex > 0)
            {
                return text.Substring(colonIndex + 1).Trim();
            }

            return ParseStory(text);
        }

        public String GetNarrative(Actor actor)
        {
            var article = IsVowel(actor.Name[0]) && actor.Name.ToLower() != "user" ? "an" : "a";
            var text = Char.ToLower(Text[0]) + Text.Substring(1);
            return String.Format("As {0} {1} I {2}.", article, actor.Name, text);
        }

        public static bool IsVowel(char c)
        {
            return Vowels.Contains(Char.ToLower(c));
        }
    }
}
