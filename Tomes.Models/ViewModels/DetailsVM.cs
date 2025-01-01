using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomes.Models.ViewModels
{
    public class DetailsVM
    {
        public ShoppingCart ShoppingCart { get; set; }

        public bool IsFavorite { get; set; }

        public int RatingSubmitted { get; set; }

        public string? ReviewSubmitted { get; set; }

        public int ReviewedProductId { get; set; }

        public IEnumerable<RatingAndReview> RatingAndReviews { get; set; }

        public RatingAndReview? UserSubmittedRatingAndReview { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalReviews { get; set; }
    }
}
