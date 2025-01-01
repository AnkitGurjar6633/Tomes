using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomes.Models
{
    public class RatingAndReview
    {
        public int Id { get; set; }

        public required int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product? Product { get; set; }

        public required string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        public required string UserName { get; set; }

        [Range(0,5)]
        public int Rating {  get; set; }

        [Length(0,500)]
        public required string Review {  get; set; }
    }
}
