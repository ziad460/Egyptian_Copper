using CopperFactory.Interfaces;
using CopperFactory.Models;
using CopperFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CopperFactory.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUnityOfWork unityOfWork;
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(IUnityOfWork unityOfWork, ApplicationDbContext context, UserManager<ApplicationUser> _userManager)
        {
            this.unityOfWork = unityOfWork;
            this.context = context;
            userManager = _userManager;
        }
        public void asd()
        {
            throw new Exception ();
        }
        public IActionResult Admin()
        {
            asd();
            return View();
        }

        #region Set Language
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddMonths(1) });
            return LocalRedirect(returnUrl);
        }
        #endregion
        public async Task<IActionResult> Index()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);

            if (User.IsInRole("Inventory"))
            {
                return RedirectToAction("Index", "Inventory", new { id = user.FactoryID });
            }
            else if (User.IsInRole("Forcasting"))
            {
                return RedirectToAction("Index", "Forcasting", new { id = user.FactoryID });
            }
            else if (User.IsInRole("Production"))
            {
                return RedirectToAction("Index", "Production", new { id = user.FactoryID });
            }
            else if (User.IsInRole("Sales"))
            {
                return RedirectToAction("Index", "WorkOrder", new { id = user.FactoryID });
            }

            StaticFunctions functions = new StaticFunctions(unityOfWork);
             
            var factories = await unityOfWork.Factory.FindAllAsync(x => x.IsDeleted != true , new[] { "Products" , "Orders" });

            ViewBag.Orders_Percentage = await functions.AllOrders_Percentages();
            ViewBag.ProductionPerDay_Percentage = await functions.AllProduction_Percentages_PerDay();
            ViewBag.Production_Percentage = await functions.AllProduction_Percentages();
            ViewBag.Sales_Percentage = await functions.AllSales_Percentages();

            ViewBag.StartDate = DateTime.Now.AddMonths(-1);
            ViewBag.EndDate = DateTime.Now;
            foreach (var item in factories)
            {
                item.StaticFunctions = new Statistics()
                {
                    FactoryID = item.ID,
                    Factory_Orders_Percentage = await functions.Orders_Percentages(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), DateTime.Now, item.ID),
                    Factory_Production_Percentage = await functions.Production_Percentages(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), DateTime.Now, item.ID),
                    Factory_Sales_Percentage = await functions.Sales_Percentages(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), DateTime.Now, item.ID),
                };
            }
            return View(factories);
        }
        [HttpPost, Authorize(Roles = "Admin,PowerUser")]
        public async Task<IActionResult> GetPercentages(int id, DateTime FromDate, DateTime ToDate)
        {
            StaticFunctions functions = new StaticFunctions(unityOfWork);

            var factories = await unityOfWork.Factory.FindAllAsync(x => x.IsDeleted != true, new[] { "Products", "Orders" });

            ViewBag.Orders_Percentage = await functions.AllOrders_Percentages();
            ViewBag.ProductionPerDay_Percentage = await functions.AllProduction_Percentages_PerDay();
            ViewBag.Production_Percentage = await functions.AllProduction_Percentages();
            ViewBag.Sales_Percentage = await functions.AllSales_Percentages();

            ViewBag.StartDate = FromDate;
            ViewBag.EndDate = ToDate;
            foreach (var item in factories)
            {
                item.StaticFunctions = new Statistics()
                {
                    FactoryID = item.ID,
                    Factory_Orders_Percentage = await functions.Orders_Percentages(FromDate, ToDate, item.ID),
                    Factory_Production_Percentage = await functions.Production_Percentages(FromDate, ToDate, item.ID),
                    Factory_Sales_Percentage = await functions.Sales_Percentages(FromDate, ToDate, item.ID),
                };
            }
            return View("Index" , factories);
        }
        [Authorize(Roles = "Admin,PowerUser")]
        public IActionResult FactoryArrangement()
        {
            List<FactoriesArrangeVM> factoriesArrange = new();
            var factories = context.Factories.Include(x => x.Products)
                .ThenInclude(x => x.Productions).Where(x => x.IsDeleted != true).ToList();
            foreach (var item in factories)
            {
                factoriesArrange.Add(new FactoriesArrangeVM
                {
                    Factory = item,
                    ProductionVolume = Math.Round(item.Products.Where(x => x.IsDeleted != true).Sum(x => x.Productions.Where(x => x.IsDeleted != true && x.Production_Date.ToString("MMM-yyyy") == DateTime.Now.ToString("MMM-yyyy")).Sum(x => x.Quantity_Producted)), 3)
                });
            }
            return View(factoriesArrange.OrderByDescending(x => x.ProductionVolume).ToList());
        }

        [HttpPost]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotFound()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public double CalculateSum(List<Production> _production)
        {
            double sum = 0;
            foreach (var item in _production)
            {
                sum += item.Quantity_Producted;
            }
            return sum;
        }
    }
}