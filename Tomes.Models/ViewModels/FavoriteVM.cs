using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomes.Models.ViewModels
{
    public class FavoriteVM
    {
        public IEnumerable<Favorite> FavoritesList { get; set; }
    }
}
