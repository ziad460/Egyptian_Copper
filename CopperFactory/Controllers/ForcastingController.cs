using CopperFactory.Interfaces;
using CopperFactory.Models;
using CopperFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Forcasting,Admin,PowerUser")]
    public class ForcastingController : Controller
    {
        private readonly IUnityOfWork unityOfWork;

        public ForcastingController(IUnityOfWork unityOfWork,UserManager<ApplicationUser> _userManager)
        {
            this.unityOfWork = unityOfWork;
        }
        public async Task<IActionResult> Index(int? id)
        {
            if(id == null)
                return NotFound();
            else
                return View(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "Forcastings" }));
        }
        public async Task<IActionResult> ForcastingsHistory()
        {
            return View(await unityOfWork.Forcasting.GetAllAsync(new[] { "Product" }));
        }
        public async Task<IActionResult> ShowProductData(int id,int year, [Bind("Day,Product_Id")] ProductDataVM _Date) //Product ID
        {
            var product = await unityOfWork.Product.FindAsync(x => x.ID == id , new[] { "Forcastings" });
            if ( product == null || product.IsDeleted == true)
            {
                return NotFound();
            }
            List<DateTime> MonthsInYear = new List<DateTime>();

            List<ProductDataVM> PdataVM = new List<ProductDataVM>();

            if (year == 0)
            {
                year = DateTime.Now.Year;
                _Date.Day = DateTime.Now;
            }
            int DaysCount = 12;

            var date = _Date.Day;
            for (int i = 1; i <= DaysCount; i++)
            {
                MonthsInYear.Add(new DateTime(year, i ,1));
            }

            //getting all forcastings for product with id = id
            var forcasting_logs = product.Forcastings.Where(p => p.Forcasting_Date.Year == year).ToList();

            if (forcasting_logs.Count != 0)
            {
                foreach (var bigListItems in MonthsInYear)
                {
                    if (forcasting_logs.Any(p => p.Forcasting_Date.Month == bigListItems.Month && p.IsDeleted != true))
                    {
                        PdataVM.Add(new ProductDataVM
                        {
                            Product_Id = product.ID,
                            Model_ID = forcasting_logs.FirstOrDefault(p => p.Forcasting_Date.Month == bigListItems.Month).ID,
                            Day = bigListItems,
                            DayStatus = true,
                            Value = forcasting_logs.FirstOrDefault(p => p.Forcasting_Date.Month == bigListItems.Month).Quantity_Forcasted,
                        });
                    }
                    else
                    {
                        PdataVM.Add(new ProductDataVM { Product_Id = product.ID , Model_ID = null, Day = bigListItems, DayStatus = false, Value = 0 });
                    }
                }
                List<int> Years = new List<int>() { DateTime.Now.Year , DateTime.Now.AddYears(-1).Year};
                ViewBag.Years = new SelectList(Years, Years.Where(p => p == year).First());
            }
            else
            {
                foreach (var item in MonthsInYear)
                {
                    PdataVM.Add(new ProductDataVM { Product_Id = product.ID , Model_ID = null, Day = item, DayStatus = false, Value = 0 });
                }
                List<int> Years = new List<int>() { DateTime.Now.Year, DateTime.Now.AddYears(-1).Year };
                ViewBag.Years = new SelectList(Years, Years.Where(p => p == year).First());
            }
            ViewBag.Product = product;
            ViewBag.URLID = product.Factory_ID;
            return View(PdataVM);
        }
        [HttpPost]
        public async Task<IActionResult> AddForcastingValue(ProductDataVM dataVM, string returnUrl ,string Value) //Product ID
        {
            dataVM.Value = double.Parse(Value , CultureInfo.InvariantCulture);
            
            var product = await unityOfWork.Product.FindAsync(x => x.ID == dataVM.Product_Id, new[] { "Forcastings" });
            if (product == null || product.IsDeleted == true) return NotFound();

            var forcasting = product.Forcastings.FirstOrDefault(p => p.Forcasting_Date.Month == dataVM.Day.Month &&
                                p.Forcasting_Date.Year == dataVM.Day.Year);

            if (forcasting != null)
            {
                forcasting.Forcasting_Date = dataVM.Day;
                forcasting.Quantity_Forcasted = dataVM.Value;
                forcasting.Quantity_Forcasted_PerDay = Math.Round(dataVM.Value / DateTime.DaysInMonth(dataVM.Day.Year, dataVM.Day.Month), 3);
                forcasting.ModifiedDate = DateTime.Now;
                forcasting.ModifiedBy = User.Identity.Name;
                forcasting.IsDeleted = false;

                await unityOfWork.CompleteAsync();
                return LocalRedirect(returnUrl);
            }

            Forcasting newForcasting = new Forcasting()
            {
                Forcasting_Date = dataVM.Day,
                Product_ID = dataVM.Product_Id,
                Quantity_Forcasted = dataVM.Value,
                Quantity_Forcasted_PerDay = Math.Round(dataVM.Value / DateTime.DaysInMonth(dataVM.Day.Year, dataVM.Day.Month) , 3),
                CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now
            };
            unityOfWork.Forcasting.AddOne(newForcasting);
            await unityOfWork.CompleteAsync();
            return LocalRedirect(returnUrl);
        }
        
        public async Task<IActionResult> DeleteForcast(int id , string returnUrl)
        {
            var forcasting = await unityOfWork.Forcasting.FindAsync(x => x.ID == id);

            if (forcasting == null || forcasting.IsDeleted == true)
            {
                return NotFound();
            }

            forcasting.IsDeleted = true;
            forcasting.DeletedDate = DateTime.Now;
            await unityOfWork.CompleteAsync();

            return LocalRedirect(returnUrl);
        }
        private CultureInfo Culture()
        {
            var currentCulture = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            return currentCulture.RequestCulture.Culture;
        }
    }
}
