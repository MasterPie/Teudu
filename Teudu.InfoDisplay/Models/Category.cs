using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class Category
    {
        private string name;
        public Category(string catName)
        {
            name = catName;
        }

        public string Name
        {
            get { return name; }
        }
    }
}
