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
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        public string searchString { get; set; } 
        public string categoryId { get; set; }
    }
}
