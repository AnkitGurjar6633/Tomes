using Microsoft.ML;
using Microsoft.ML.Trainers;
using Tomes.Models.MLModels;
using Tomes.Models;

namespace Tomes.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly float _favoriteBaseRating;
        private readonly float _orderBaseRating;
        private readonly float _orderIncrementScaleFactor;
        private readonly float _visitIncrementFactor;
        public RecommendationService(float FavoriteBaseRating = 5f, float OrderBaseRating = 4f, float OrderIncrementScaleFactor = 0.25f, float VisitIncrementFactor = 0.2f)
        {
            _favoriteBaseRating = FavoriteBaseRating;
            _orderBaseRating = OrderBaseRating;
            _orderIncrementScaleFactor = OrderIncrementScaleFactor;
            _visitIncrementFactor = VisitIncrementFactor;
        }

        public ITransformer TrainRecommendationModel(List<RatingAndReview> reviews, List<Favorite> favorites,
          List<OrderDetail> orderDetails, List<int> visitHistory, List<Product> products, MLContext mlContext, string currentUserId)
        {
            var userDataList = new List<UserData>();
            var visitedProducts = new Dictionary<int, float>();

            foreach (var review in reviews)
            {
                userDataList.Add(new UserData
                {
                    ApplicationUserId = review.ApplicationUserId,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    CategoryId = products.First(x => x.Id == review.ProductId).CategoryId
                });
            }
            foreach (var favorite in favorites)
            {
                userDataList.Add(new UserData
                {
                    ApplicationUserId = favorite.ApplicationUserId,
                    ProductId = favorite.ProductId,
                    Rating = _favoriteBaseRating,
                    CategoryId = products.First(x => x.Id == favorite.ProductId).CategoryId
                });
            }

            foreach (var orderDetail in orderDetails)
            {
                float rating = _orderBaseRating;
                if (orderDetails.Where(x => x.ProductId == orderDetail.ProductId).Count() > 1)
                {
                    rating = _orderBaseRating + (orderDetails.Where(x => x.ProductId == orderDetail.ProductId).Count() * _orderIncrementScaleFactor);
                }

                userDataList.Add(new UserData
                {
                    ApplicationUserId = orderDetail.OrderHeader.ApplicationUserId,
                    ProductId = orderDetail.ProductId,
                    Rating = rating,
                    CategoryId = products.First(x => x.Id == orderDetail.ProductId).CategoryId
                });
            }
            foreach (var productId in visitHistory)
            {
                if (visitedProducts.ContainsKey(productId))
                {
                    visitedProducts[productId] += _visitIncrementFactor;
                }
                else
                {
                    visitedProducts[productId] = 1;
                }
            }
            foreach (var product in visitedProducts)
            {
                userDataList.Add(new UserData
                {
                    ApplicationUserId = currentUserId,  // fixed this
                    ProductId = product.Key,
                    Rating = product.Value,
                    CategoryId = products.First(x => x.Id == product.Key).CategoryId
                });
            }

            var dataView = mlContext.Data.LoadFromEnumerable(userDataList.Select(x => new UserData { ApplicationUserId = x.ApplicationUserId, ProductId = x.ProductId, Rating = (float)x.Rating, CategoryId = x.CategoryId }));
            var idMap = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "ApplicationUserId", outputColumnName: "UserIdMapped").Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "ProductId", outputColumnName: "ProductIdMapped"));

            var matrixFactorizationOptions = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserIdMapped",
                MatrixRowIndexColumnName = "ProductIdMapped",
                LabelColumnName = nameof(UserData.Rating)
            };
            var pipeline = idMap
                      .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CategoryIdEncoded", inputColumnName: nameof(UserData.CategoryId)))
                      .Append(mlContext.Recommendation().Trainers.MatrixFactorization(matrixFactorizationOptions));
            return pipeline.Fit(dataView);
        }
        public IDataView CreateDataView(List<RatingAndReview> reviews, List<Favorite> favorites,
            List<OrderDetail> orderDetails, List<int> visitHistory, List<Product> products, MLContext mlContext, string currentUserId)
        {
            var userDataList = new List<UserData>();
            var visitedProducts = new Dictionary<int, float>();

            foreach (var review in reviews.Where(x => x.ApplicationUserId == currentUserId))
            {
                userDataList.Add(new UserData
                {
                    ApplicationUserId = review.ApplicationUserId,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    CategoryId = products.First(x => x.Id == review.ProductId).CategoryId
                });
            }
            foreach (var favorite in favorites.Where(x => x.ApplicationUserId == currentUserId))
            {
                userDataList.Add(new UserData
                {
                    ApplicationUserId = favorite.ApplicationUserId,
                    ProductId = favorite.ProductId,
                    Rating = _favoriteBaseRating, // configuration with float types for models  that used parameter type correctly
                    CategoryId = products.First(x => x.Id == favorite.ProductId).CategoryId
                });
            }

            foreach (var orderDetail in orderDetails.Where(x => x.OrderHeader.ApplicationUserId == currentUserId))
            {
                float rating = _orderBaseRating;
                if (orderDetails.Where(x => x.OrderHeader.ApplicationUserId == currentUserId && x.ProductId == orderDetail.ProductId).Count() > 1)
                {
                    rating = _orderBaseRating + (orderDetails.Where(x => x.OrderHeader.ApplicationUserId == currentUserId && x.ProductId == orderDetail.ProductId).Count() * _orderIncrementScaleFactor);
                }

                userDataList.Add(new UserData
                {
                    ApplicationUserId = orderDetail.OrderHeader.ApplicationUserId,
                    ProductId = orderDetail.ProductId,
                    Rating = rating,
                    CategoryId = products.First(x => x.Id == orderDetail.ProductId).CategoryId
                });
            }
            foreach (var productId in visitHistory)
            {
                if (visitedProducts.ContainsKey(productId))
                {
                    visitedProducts[productId] += _visitIncrementFactor;
                }
                else
                {
                    visitedProducts[productId] = 1;
                }
            }
            foreach (var product in visitedProducts)
            {
                userDataList.Add(new UserData
                {
                    ApplicationUserId = currentUserId, // we are now adding `currentUserId` variable correctly to load type at data for that method with correct model scope.

                    ProductId = product.Key,
                    Rating = product.Value,
                    CategoryId = products.First(x => x.Id == product.Key).CategoryId
                });
            }

            return mlContext.Data.LoadFromEnumerable(userDataList.Select(x => new UserData { ApplicationUserId = x.ApplicationUserId, ProductId = x.ProductId, Rating = (float)x.Rating, CategoryId = x.CategoryId }));
        }

        public IDataView CreateProductDataView(List<Product> products, MLContext mlContext)
        {
            var productDataList = products.Select(x => new ProductData { Id = x.Id, CategoryId = x.CategoryId }).ToList();
            return mlContext.Data.LoadFromEnumerable(productDataList);
        }
        public ITransformer TrainModel(IDataView trainingDataView, IDataView productDataView, MLContext mlContext)
        {
            return null; // implemented as  TrainRecommendationModel with right implementations now

        }
        public IEnumerable<Product> GetRecommendations(ITransformer model, IDataView productDataView, IEnumerable<Product> allProducts, MLContext mlContext, string currentUserId, List<OrderDetail> orderDetails, List<int> visitedHistory, int topN = 10)
        {
            var predictionEngine = mlContext.Model.CreatePredictionEngine<UserData, Prediction>(model);
            var allProductIds = allProducts.Select(p => p.Id).ToList();
            var userOrderDetailProductIds = orderDetails.Where(x => x.OrderHeader.ApplicationUserId == currentUserId).Select(x => x.ProductId).ToList();

            var predictionResults = new List<(int ProductId, float Score)>();
            foreach (var productId in allProductIds)
            {
                if (userOrderDetailProductIds.Contains(productId) || visitedHistory.Contains(productId))
                {
                    continue;
                }
                var prediction = predictionEngine.Predict(new UserData { ApplicationUserId = currentUserId, ProductId = productId, CategoryId = allProducts.First(x => x.Id == productId).CategoryId });
                predictionResults.Add((productId, prediction.Score));
            }
            return predictionResults
               .OrderByDescending(result => result.Score)
               .Take(topN)
                 .Select(result => allProducts.First(p => p.Id == result.ProductId))
                .ToList();
        }
    }
}
