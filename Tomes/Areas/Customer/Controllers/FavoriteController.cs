using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tomes.DataAccess.Repository.IRepository;
using Tomes.Models.ViewModels;

namespace Tomes.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public FavoriteVM FavoriteVM { get; set; }
        public FavoriteController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            FavoriteVM = new()
            {
                FavoritesList = _unitOfWork.Favorite.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product")
            };

            return View(FavoriteVM);
        }

        public IActionResult Remove(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var favoriteFromDb = _unitOfWork.Favorite.Get(f => f.ApplicationUserId == userId && f.ProductId == productId);

            if(favoriteFromDb != null)
            {
                _unitOfWork.Favorite.Remove(favoriteFromDb);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
