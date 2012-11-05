using System;
using System.Collections.Generic;
using System.Linq;

namespace MindMapConverter
{
    public class Story
    {
        private static readonly List<char> Vowels = new List<char> {'a', 'e', 'i', 'o', 'u'};
        public String StoryText { get; set; }
        public IEnumerable<Story> Children { get; set; }
        public IEnumerable<String> RelatedRequirements { get; set; }

        public Story(String text)
        {
            RelatedRequirements = ParseRequirements(text);
            StoryText = ParseStory(text);
        }

        private IEnumerable<String> ParseRequirements(string text)
        {
            var colonIndex = text.IndexOf(":", System.StringComparison.Ordinal);
            if(colonIndex > 0)
            {
                var requirements = text.Substring(colonIndex + 1);
                return requirements.Split(',').Select(x => x.Trim());
            }
            return Enumerable.Empty<String>();
        }

        private String ParseStory(string text)
        {
            var colonIndex = text.IndexOf(":", System.StringComparison.Ordinal);
            if (colonIndex > 0)
            {
                return text.Substring(0, colonIndex-1);
            }
            return text;
        }

        public String GetNarrative(Actor actor)
        {
            var article = IsVowel(actor.Name[0]) && actor.Name.ToLower() != "user" ? "an" : "a";
            var text = Char.ToLower(StoryText[0]) + StoryText.Substring(1);
            return String.Format("As {0} {1} I {2}.", article, actor.Name, text);
        }

        public static bool IsVowel(char c)
        {
            return Vowels.Contains(Char.ToLower(c));
        }
    }
}
