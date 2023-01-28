using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CopperFactory.Models;
using System.Xml.Linq;
using CopperFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using CopperFactory.Interfaces;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Production,Admin,PowerUser")]
    public class ProductionController : Controller
    {
        private readonly IUnityOfWork unityOfWork;

        public ProductionController(IUnityOfWork unityOfWork)
        {
            this.unityOfWork = unityOfWork;
        }
        public async Task<IActionResult> ProductionHistory()
        {
            return View(await unityOfWork.Production.GetAllAsync(new[] { "Product" }));
        }
        public async Task<IActionResult> Index(int? id)
        {
            if(id == null)
                return NotFound();
            else
                return View(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "Productions" }));
        }
        public async Task<IActionResult> ShowProductData(int id , [Bind("Day,Product_Id")] ProductDataVM _Date) //Product ID
        {
            var product = await unityOfWork.Product.FindAsync(x => x.ID == id, new[] { "Productions" });

            if (product == null)
            {
                return NotFound();
            }
            List<DateTime> DaysInMonth = new List<DateTime>();
            List<ProductDataVM> PdataVM = new List<ProductDataVM>();

            var date = _Date.Day;
            if (date == DateTime.MinValue)
            {
                date = DateTime.Now;
            }
            int DaysCount = DateTime.DaysInMonth(date.Year, date.Month);

            for (int i = 1; i <= DaysCount; i++)
            {
                DaysInMonth.Add(new DateTime(date.Year, date.Month, i));
            }
            var productProductions = product.Productions
                .Where(p => p.Production_Date.Month == date.Month && p.Production_Date.Year == date.Year).ToList();

            if (productProductions.Count != 0)
            {
                foreach (var bigListItem in DaysInMonth)
                {
                    if (productProductions.Any(p => p.Production_Date.Day == bigListItem.Day && p.IsDeleted != true))
                    {
                        PdataVM.Add(new ProductDataVM
                        {
                            Product_Id = product.ID,
                            Model_ID = productProductions.FirstOrDefault(p => p.Production_Date.Day == bigListItem.Day).ID,
                            Day = bigListItem,
                            DayStatus = true,
                            Value = productProductions.FirstOrDefault(p => p.Production_Date.Day == bigListItem.Day).Quantity_Producted,
                        });
                    }
                    else
                    {
                        PdataVM.Add(new ProductDataVM { Product_Id = id, Model_ID = null, Day = bigListItem, DayStatus = false, Value = 0 });
                    }
                }
                ViewBag.TotalProductionValue = Math.Round(productProductions.Where(p => p.IsDeleted != true).Sum(p => p.Quantity_Producted) , 3);
            }
            else
            {
                foreach (var item in DaysInMonth)
                {
                    PdataVM.Add(new ProductDataVM {Product_Id = id , Model_ID = null , Day = item, DayStatus = false, Value = 0});
                }
                ViewBag.TotalProductionValue = 0;
            }
            ViewBag.Product = product;
            ViewBag.URLID = product.Factory_ID;
            return View(PdataVM);
        }
        [HttpPost]
        public async Task<IActionResult> AddProdValue(ProductDataVM dataVM, string returnUrl , string Value) //Product ID
        {
            dataVM.Value = double.Parse(Value, CultureInfo.InvariantCulture);

            var product = await unityOfWork.Product.FindAsync(x => x.ID == dataVM.Product_Id, new[] { "Productions" });
            
            if (product == null)
            {
                return NotFound();
            }

            var production = product.Productions.FirstOrDefault(p => p.Production_Date.Day == dataVM.Day.Day 
                                                        && p.Production_Date.Month == dataVM.Day.Month
                                                        && p.Production_Date.Year == dataVM.Day.Year);
            if (production != null)
            {
                production.Quantity_Producted = dataVM.Value;
                production.ModifiedDate = DateTime.Now;
                production.ModifiedBy = User.Identity.Name;
                production.IsDeleted = false;

                await unityOfWork.CompleteAsync();
                return LocalRedirect(returnUrl);
            }

            Production newProduction = new Production()
            {
                Production_Date = dataVM.Day,
                Product_ID = dataVM.Product_Id,
                Quantity_Producted = dataVM.Value,
                CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now, 
            };
            unityOfWork.Production.AddOne(newProduction);
            await unityOfWork.CompleteAsync();
            return LocalRedirect(returnUrl);
        }
        public async Task<IActionResult> DeleteProduction(int id, string returnUrl)
        {
            var production = await unityOfWork.Production.FindAsync(x => x.ID == id);
            if (production == null)
            {
                return NotFound();
            }
            production.IsDeleted = true;
            production.DeletedDate = DateTime.Now;
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
