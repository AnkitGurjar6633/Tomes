using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomes.Models.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Product> ProductList { get; set; }
        public IEnumerable<Product> RecommendedProductList { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public FavoriteVM FavoriteVM { get; set; }

        public string searchString { get; set; } 
        public int categoryId { get; set; }
        public int favoriteProductId { get; set; }
    }
}
