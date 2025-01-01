using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomes.Models.MLModels
{
    public class UserData
    {
        public string ApplicationUserId { get; set; }
        public int ProductId { get; set; }
        public float Rating { get; set; }
        public int CategoryId { get; set; }
    }
}
