using System;
using System.Collections.Generic;

namespace FreeMind.Models
{
    public class Actor
    {
        public String Name { get; set; }
        public IEnumerable<Story> Stories { get; set; }

        public Actor(String name)
        {
            Name = name;
        }
    }
}
