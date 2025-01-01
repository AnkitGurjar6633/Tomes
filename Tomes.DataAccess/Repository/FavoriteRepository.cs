using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomes.DataAccess.Data;
using Tomes.DataAccess.Repository.IRepository;
using Tomes.Models;
using Tomes.Models.ViewModels;

namespace Tomes.DataAccess.Repository
{
    public class FavoriteRepository : Repository<Favorite>, IFavoriteRepository
    {
        private ApplicationDbContext _db;
        public FavoriteRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Favorite favorite)
        {
            _db.Favorites.Update(favorite);
        }
    }
}
