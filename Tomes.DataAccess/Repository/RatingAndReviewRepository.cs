using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomes.DataAccess.Data;
using Tomes.DataAccess.Repository.IRepository;
using Tomes.Models;

namespace Tomes.DataAccess.Repository
{
    public class RatingAndReviewRepository : Repository<RatingAndReview>, IRatingAndReviewRepository
    {
        private ApplicationDbContext _db;
        public RatingAndReviewRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(RatingAndReview ratingAndReview)
        {
            _db.RatingAndReviews.Update(ratingAndReview);
        }
    }
}
