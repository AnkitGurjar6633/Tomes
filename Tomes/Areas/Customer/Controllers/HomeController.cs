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
using Tomes.Services;

namespace Tomes.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRecommendationService _recommendationService;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork, IRecommendationService recommendationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _recommendationService = recommendationService;
        }

        public IActionResult Index(string searchString, int categoryId = 0)
        {
            List<Category> categories = _unitOfWork.Category.GetAll().ToList();
            Category all = new Category()
            {
                Name = "All",
                Id = 0,
            };
            categories.Insert(0,all);

            IEnumerable<Product> products = null;
            try
            {
                Regex regex = new Regex(searchString == null ? "" : searchString, RegexOptions.IgnoreCase);

                products = _unitOfWork.Product.GetAll(includeProperties: "Category").Where(p => (regex.IsMatch(p.Title) || regex.IsMatch(p.Author) || regex.IsMatch(p.Description)) && (categoryId == 0 || p.CategoryId == categoryId));
            }
            catch (Exception ex)
            {
                products = [];
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            IEnumerable<Favorite> favorites = _unitOfWork.Favorite.GetAll(f => f.ApplicationUserId == userId);
            var allProducts = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            var ratings = _unitOfWork.RatingAndReview.GetAll().ToList();
            var orders = _unitOfWork.OrderDetail.GetAll(includeProperties: "OrderHeader").Where(od => od.OrderHeader.ApplicationUserId == userId).ToList();
            var visitHistory = _unitOfWork.ApplicationUser.Get(u => u.Id == userId)?.ProductVisitList?.Split(',').Select(int.Parse).ToList();

            IEnumerable<Product> recommendedProducts = null;

            if (userId != null)
            {
                //var mlContext = new MLContext();
                //var productDataView = _recommendationService.CreateProductDataView(allProducts, mlContext);

                //var recommendation = _recommendationService.GetRecommendations(_model, productDataView, allProducts, mlContext, userId, orders, visitHistory);

                //recommendedProducts = recommendation;
                if(visitHistory?.Count > 0 || favorites?.Count() > 0)
                {
                    recommendedProducts = SortProducts(allProducts, favorites.ToList(), ratings, orders, visitHistory, userId);
                }
                else
                {
                    recommendedProducts = [];
                }
            }
            else
            {
                recommendedProducts = [];
            }

            HomeVM homeVM = new HomeVM()
            {
                ProductList = products,
                RecommendedProductList = recommendedProducts,
                CategoryList = categories.Select(cl => new SelectListItem
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString(),
                    Selected = categoryId == cl.Id ? true : false

                }),
                FavoriteVM = new FavoriteVM() { FavoritesList = favorites},
                searchString = searchString,
                categoryId = categoryId
            };
            return View(homeVM);
        }

        public static List<Product> SortProducts(
        List<Product> allProducts,
        List<Favorite> favorites,
        List<RatingAndReview> ratings,
        List<OrderDetail> orders,
        List<int> visitHistory,
        string userId
    )
        {
            var productScores = new Dictionary<int, double>();

            foreach (var product in allProducts)
            {
                double score = 0;
                // Favorite Score
                if (favorites.Any(f => f.ProductId == product.Id && f.ApplicationUserId == userId))
                {
                    score += 1000;
                }

                // Order Score
                if (orders.Any(o => o.ProductId == product.Id))
                {
                    score += 500;
                }

                // Rating Score
                var productRatings = ratings.Where(r => r.ProductId == product.Id).ToList();
                if (productRatings.Any())
                {
                    double avgRating = productRatings.Average(r => r.Rating);
                    score += avgRating * 100; // You can adjust 100 to control the weight of ratings
                }

                // Visit History Score (Decaying Weight)
                for (int i = 0; i < visitHistory.Count; i++)
                {
                    if (visitHistory[i] == product.Id)
                    {
                        // Give higher scores to more recent visits, decaying by index
                        score += 100 / (i + 1.0); // or any other decay factor
                    }

                }
                productScores[product.Id] = score;
            }
            return allProducts.OrderByDescending(p => productScores.ContainsKey(p.Id) ? productScores[p.Id] : 0).ToList();
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

                homeVM.ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").Where(p => (regex.IsMatch(p.Title) || regex.IsMatch(p.Author) || regex.IsMatch(p.Description) ) && (homeVM.categoryId == 0 || p.CategoryId == homeVM.categoryId));
            }
            catch(Exception ex)
            {
                homeVM.ProductList = null;
            }
            homeVM.CategoryList = categories.Select(cl => new SelectListItem
            {
                Text = cl.Name,
                Value = cl.Id.ToString(),
                Selected = homeVM.categoryId == cl.Id ? true : false
            });

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            IEnumerable<Favorite> favorites = _unitOfWork.Favorite.GetAll(f => f.ApplicationUserId == userId);

            homeVM.FavoriteVM = new FavoriteVM() { FavoritesList = favorites };
            homeVM.RecommendedProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");

            return View(homeVM);
        }

        public IActionResult Details(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, tracked: true);
            if (user != null)
            {
                if (user.ProductVisitList != null)
                {
                    var productList = user.ProductVisitList.Split(',')
                            .Select(int.Parse).ToList();

                    productList.Remove(productId);
                    productList.Insert(0, productId);
                    if (productList.Count > 15)
                        productList.RemoveAt(productList.Count - 1);

                    user.ProductVisitList = string.Join(",", productList);
                }
                else
                {
                    user.ProductVisitList = productId.ToString();
                }
                _unitOfWork.Save();
            }

            var allProductReviews = _unitOfWork.RatingAndReview.GetAll(r => r.ProductId == productId && r.ApplicationUserId != userId);
            var userProductReview = _unitOfWork.RatingAndReview.Get(r => r.ProductId == productId && r.ApplicationUserId == userId);
            int totalReviewCount = allProductReviews.Count();
            decimal totalRatings = allProductReviews.Aggregate(0, (total, ratingAndReview) => total + ratingAndReview.Rating);
            if(userProductReview != null)
            {
                totalRatings += userProductReview.Rating;
                totalReviewCount += 1;
            }
            
            decimal averageReview = totalReviewCount > 0 ? totalRatings / totalReviewCount : 0;
            averageReview = Math.Round(averageReview, 1);

            DetailsVM detailsVM = new DetailsVM()
            {
                ShoppingCart = new()
                {
                    Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                    ProductId = productId,
                    Count = 1

                },
                IsFavorite = _unitOfWork.Favorite.GetAll(f => f.ApplicationUserId == userId).Any(f => f.ProductId == productId),
                RatingAndReviews = allProductReviews,
                UserSubmittedRatingAndReview = userProductReview,
                AverageRating = averageReview,
                TotalReviews = totalReviewCount
            };
            return View(detailsVM);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(DetailsVM detailsVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            detailsVM.ShoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ProductId == detailsVM.ShoppingCart.ProductId && 
            u.ApplicationUserId == userId);
            
            if(cartFromDb != null)
            {
                // update shopping cart
                cartFromDb.Count += detailsVM.ShoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                //add shopping cart
                _unitOfWork.ShoppingCart.Add(detailsVM.ShoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart Updated Successfully";

            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public IActionResult Favorite(HomeVM homeVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var favoriteFromDb = _unitOfWork.Favorite.Get(f => f.ProductId == homeVM.favoriteProductId && f.ApplicationUserId == userId);

            if(favoriteFromDb != null)
            {
                _unitOfWork.Favorite.Remove(favoriteFromDb);
            }
            else
            {
                _unitOfWork.Favorite.Add(new Favorite { ProductId = homeVM.favoriteProductId, ApplicationUserId = userId });
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index), new { searchString = homeVM.searchString, categoryId = homeVM.categoryId });
        }

        [Authorize]
        public IActionResult FavoriteDetails(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var favoriteFromDb = _unitOfWork.Favorite.Get(f => f.ProductId == productId && f.ApplicationUserId == userId);

            if (favoriteFromDb != null)
            {
                _unitOfWork.Favorite.Remove(favoriteFromDb);
            }
            else
            {
                _unitOfWork.Favorite.Add(new Favorite { ProductId = productId, ApplicationUserId = userId });
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Details), new {productId = productId});
        }

        [HttpPost]
        [Authorize]
        public IActionResult Ratings(DetailsVM detailsVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            var ratingFromDb = _unitOfWork.RatingAndReview.Get(r => r.ProductId == detailsVM.ReviewedProductId && r.ApplicationUserId == userId);

            if (ratingFromDb != null)
            {
                _unitOfWork.RatingAndReview.Update(ratingFromDb);
            }
            else
            {
                _unitOfWork.RatingAndReview.Add(new RatingAndReview { ApplicationUserId = userId, ProductId = detailsVM.ReviewedProductId, Review = detailsVM.ReviewSubmitted, Rating = detailsVM.RatingSubmitted, UserName = userFromDb.Name});
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Details), new { productId = detailsVM.ReviewedProductId});
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
