using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genometric.TVQ.API.Model
{
    //-------------------------------
    // This is an experimental model.
    //-------------------------------

    public class ToolCategoryDistribution
    {
        public int Count { set; get; }

        public double Percentage { set; get; }

        private List<Category> Categories { set; get; }

        public HashSet<int> CategoriesID { set; get; }

        public ToolCategoryDistribution()
        {
            Categories = new List<Category>();
            CategoriesID = new HashSet<int>();
        }

        public void Add(Category category)
        {
            if (category == null)
                return;

            Categories.Add(category);
            CategoriesID.Add(category.ID);
        }

        public void Add(IEnumerable<Category> categories)
        {
            if (categories == null)
                return;

            foreach (var category in categories)
                Add(category);
        }
    }
}
