using Tomes.DataAccess.Repository;
using Tomes.DataAccess.Repository.IRepository;
using Tomes.Models;
using Tomes.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Tomes.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace Tomes.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Category> categories = _unitOfWork.Category.GetAll().ToList();
            Category all = new Category()
            {
                Name = "All",
                Id = 0,
            };
            categories.Insert(0,all);

            HomeVM homeVM = new HomeVM()
            {
                ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category"),
                CategoryList = categories.Select(cl => new SelectListItem
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString(),
                    Selected = cl.Id == 0 ? true : false

                })
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult Index(HomeVM homeVM)
        {
            List<Category> categories = _unitOfWork.Category.GetAll().ToList();
            Category all = new Category()
            {
                Name = "All",
                Id = 0,
            };
            categories.Insert(0, all);

            try
            {
                Regex regex = new Regex(homeVM.searchString == null ? "" : homeVM.searchString, RegexOptions.IgnoreCase);

                homeVM.ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").Where(p => (regex.IsMatch(p.Title) || regex.IsMatch(p.Author) || regex.IsMatch(p.Description) ) && (homeVM.categoryId == "0" || p.CategoryId.ToString() == homeVM.categoryId));
            }
            catch(Exception ex)
            {
                homeVM.ProductList = null;
            }
            homeVM.CategoryList = categories.Select(cl => new SelectListItem
            {
                Text = cl.Name,
                Value = cl.Id.ToString(),
                Selected = homeVM.categoryId == cl.Id.ToString() ? true : false
            });
            

            return View(homeVM);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                ProductId = productId,
                Count = 1

            };
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ProductId == shoppingCart.ProductId && 
            u.ApplicationUserId == userId);
            
            if(cartFromDb != null)
            {
                // update shopping cart
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                //add shopping cart
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart Updated Successfully";

            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
