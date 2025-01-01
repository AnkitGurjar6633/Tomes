using Microsoft.ML;
using Tomes.Models;

namespace Tomes.Services
{
    public interface IRecommendationService
    {
        IDataView CreateDataView(List<RatingAndReview> reviews, List<Favorite> favorites,
        List<OrderDetail> orderDetails, List<int> visitHistory, List<Product> products, MLContext mlContext, string currentUserId);
        IDataView CreateProductDataView(List<Product> products, MLContext mlContext);
        ITransformer TrainModel(IDataView trainingDataView, IDataView productDataView, MLContext mlContext);
        IEnumerable<Product> GetRecommendations(ITransformer model, IDataView productDataView, IEnumerable<Product> allProducts, MLContext mlContext,
                  string currentUserId, List<OrderDetail> orderDetails, List<int> visitedHistory, int topN = 10);
        ITransformer TrainRecommendationModel(List<RatingAndReview> reviews, List<Favorite> favorites,
        List<OrderDetail> orderDetails, List<int> visitHistory, List<Product> products, MLContext mlContext, string currentUserId);
    }
}
