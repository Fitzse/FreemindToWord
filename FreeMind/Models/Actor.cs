using System;
using System.Collections.Generic;

namespace FreeMind.Models
{
    public class Actor
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public IEnumerable<Story> Stories { get; set; }

        public Actor(String text)
        {
            Name = ParseName(text);
            Description = ParseDescription(text);
        }

        private String ParseName(String text)
        {
            var colonIndex = text.IndexOf(':');
            if(colonIndex > 0)
            {
                return text.Substring(0, colonIndex - 1).Trim();
            }
            return text;
        }

        private String ParseDescription(String text)
        {
            var colonIndex = text.IndexOf(':');
            if(colonIndex > 0)
            {
                return text.Substring(colonIndex + 1, text.Length - colonIndex - 1).Trim();
            }
            return "N/A";
        }
    }
}
