using Tomes.DataAccess.Data;
using Tomes.DataAccess.Repository;
using Tomes.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Tomes.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Tomes.DataAccess.DbInitializer;
using Microsoft.ML;
using Tomes.Services;
using Tomes.Models;
using Product = Tomes.Models.Product;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "476428931707932";
    options.AppSecret = "9ce8610ddc7d2dd1c9be148e3f49e271";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender,EmailSender>();

//add recommendation system
builder.Services.AddSingleton<MLContext>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    return new RecommendationService(
     FavoriteBaseRating: config.GetValue<float>("RecommendationSettings:FavoriteBaseRating", 5f),
            OrderBaseRating: config.GetValue<float>("RecommendationSettings:OrderBaseRating", 4f),
        OrderIncrementScaleFactor: config.GetValue<float>("RecommendationSettings:OrderIncrementScaleFactor", 0.25f),
          VisitIncrementFactor: config.GetValue<float>("RecommendationSettings:VisitIncrementFactor", 0.2f));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey=builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

//TrainAndSaveModel(app.Services);

app.Run();


void SeedDatabase()
{
    using(var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}

void TrainAndSaveModel(IServiceProvider serviceProvider)
{
    using (var scope = serviceProvider.CreateScope())
    {
        var mlContext = scope.ServiceProvider.GetRequiredService<MLContext>();
        var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var dataPath = config["DataPath"];

        var products = DataHelper.ReadJsonFile<Product>($"{dataPath}products.json");
        var reviews = DataHelper.ReadJsonFile<RatingAndReview>($"{dataPath}ratings.json");
        var favorites = DataHelper.ReadJsonFile<Favorite>($"{dataPath}favorites.json");
        var orderDetails = DataHelper.ReadJsonFile<OrderDetail>($"{dataPath}orders.json");
        var orderHeaders = DataHelper.ReadJsonFile<OrderHeader>($"{dataPath}OrderHeader.json");
        var visitHistory = new List<int>() { 1, 2, 2, 3, 3, 3, 4, 4, 4, 4, 12, 3, 3, 4 };

        foreach ( var item in orderDetails)
        {
            item.OrderHeader = orderHeaders.First(o =>  o.Id == item.OrderHeaderId);
        }

        
        var trainingDataView = recommendationService.CreateDataView(reviews, favorites, orderDetails, visitHistory, products, mlContext, "user_id_1");  

        ITransformer model = recommendationService.TrainRecommendationModel(reviews, favorites, orderDetails, visitHistory, products, mlContext, "user_id_1");  
        mlContext.Model.Save(model, trainingDataView.Schema, products.First().GetType().Name + "_recommendation_model.zip");  
        Console.WriteLine("Trained model Saved with  parameters types from data  to: " + products.First().GetType().Name + "_recommendation_model.zip");
    }
}
