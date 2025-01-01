using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomes.Models;

namespace Tomes.DataAccess.Repository.IRepository
{
    public interface IFavoriteRepository: IRepository<Favorite>
    {
        void Update(Favorite favorite);
    }
}
