using CopperFactory.Interfaces;
using CopperFactory.Models;
using CopperFactory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Inventory,Admin,PowerUser")]
    public class InventoryController : Controller
    {
        private readonly IUnityOfWork unityOfWork;

        public InventoryController(IUnityOfWork unityOfWork)
        {
            this.unityOfWork = unityOfWork;
        }
        public async Task<IActionResult> Index(int? id)
        {
            ViewBag.FactoryID = id;

            if (id == null)
                return NotFound();
            else
                return View(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "Inventory_INs" }));
        }
        public async Task<IActionResult> InventoryInHistory()
        {
            return View(await unityOfWork.Inventory_IN.GetAllAsync(new[] { "Product" } ));
        }
        public async Task<IActionResult> InventoryOutHistory()
        {
            return View(await unityOfWork.Inventory_Out.GetAllAsync(new[] { "Product" , "Customer" }));
        }
        public async Task<IActionResult> ShowInventoryData(int? id, InventoryVM inventoryVM)
        {
            var product = await unityOfWork.Product.FindAsync(x => x.ID == id, new[] { "Inventory_INs" });

            if (product == null)
            {
                return NotFound();
            }
            List<DateTime> DaysInMonth = new List<DateTime>();
            List<InventoryVM> inventoryVMs = new List<InventoryVM>();

            var date = inventoryVM.Date;

            if (date == DateTime.MinValue)
            {
                date = DateTime.Now;
            }
            int DaysCount = DateTime.DaysInMonth(date.Year, date.Month);

            for (int i = 1; i <= DaysCount; i++)
            {
                DaysInMonth.Add(new DateTime(date.Year, date.Month, i));
            }
            var productInventory = product.Inventory_INs
                .Where(p => p.DateTime.Month == date.Month && p.DateTime.Year == date.Year && p.IsDeleted != true).ToList();

            var productionPerMonth = unityOfWork.Production.FindAll(x => x.IsDeleted != true).Where( p => p.Product_ID == id &&
                                                                p.Production_Date.Month == date.Month && 
                                                                p.Production_Date.Year == date.Year).ToList();

            if (productInventory.Count != 0)
            {
                foreach (var bigListItem in DaysInMonth)
                {
                    if (productInventory.Any(p => p.DateTime.Day == bigListItem.Day))
                    {
                        var inventory_ = productInventory.FirstOrDefault(p => p.DateTime.Day == bigListItem.Day);
                        var production = productionPerMonth.FirstOrDefault(p => p.Production_Date.Day == bigListItem.Day);
                        inventoryVMs.Add(new InventoryVM
                        {
                            Product_Id = product.ID,
                            Model_ID = inventory_.ID,
                            Date = bigListItem,
                            DayStatus = true,
                            Value_Stored = inventory_.Quantity_Received,
                            productionValue = production == null ? 0 : production.Quantity_Producted,
                        });
                    }
                    else
                    {
                        var production = productionPerMonth.FirstOrDefault(p => p.Production_Date.Day == bigListItem.Day);
                        inventoryVMs.Add(new InventoryVM
                        {
                            Product_Id = product.ID,
                            Date = bigListItem,
                            DayStatus = false,
                            Model_ID = null,
                            Value_Stored = 0,
                            DidNotStore = production == null ? 0 : production.Quantity_Producted,
                            productionValue = production == null ? 0 : production.Quantity_Producted,
                        });
                    }
                }
            }
            else
            {
                foreach (var item in DaysInMonth)
                {
                    var production = productionPerMonth.FirstOrDefault(p => p.Production_Date.Day == item.Day);
                    inventoryVMs.Add(new InventoryVM
                    {
                        Product_Id = id,
                        Date = item,
                        DayStatus = false,
                        Model_ID = null,
                        Value_Stored = 0,
                        DidNotStore = production == null ? 0 : production.Quantity_Producted,
                        productionValue = production == null ? 0 : production.Quantity_Producted,
                    });
                }
            }
            ViewBag.Product = product;
            ViewBag.FcatoryID = product.Factory_ID;
            return View(inventoryVMs);
        }
        [HttpPost]
        public async Task<IActionResult> AddToInsides(int id, DateTime Date, string Quantity_Received, string returnUrl)
        {
            double InQuantity = double.Parse(Quantity_Received, CultureInfo.InvariantCulture);

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            var product = await unityOfWork.Product.FindAsync(x => x.ID == id, new[] { "Inventory_INs" });

            if (product == null)
            {
                return NotFound();
            }

                Inventory_IN? inventory = await unityOfWork.Inventory_IN.FindAsync(x => x.IsDeleted != true
                                            && x.Product_ID == product.ID 
                                            && x.DateTime.Day == Date.Day
                                            && x.DateTime.Month == Date.Month
                                            && x.DateTime.Year == Date.Year);
            
            Production? production = await unityOfWork.Production.FindAsync(x => x.IsDeleted != true
                                            && x.Product_ID == product.ID
                                            && x.Production_Date.Day == Date.Day
                                            && x.Production_Date.Month == Date.Month
                                            && x.Production_Date.Year == Date.Year);

            if (inventory != null)
            {
                inventory.Quantity_Received = InQuantity;
                inventory.ModifiedDate = DateTime.Now;
                inventory.ModifiedBy = User.Identity.Name;
                inventory.IsDeleted = false;

                await unityOfWork.CompleteAsync();

                product.Inventory_Total_Amount = TotalInInventory(product.ID);
                product.Out_Inventory_Total_Amount = TotalOutInventory(product.ID);
                await unityOfWork.CompleteAsync();
                return LocalRedirect(returnUrl);   
            }

            Inventory_IN inventory_IN = new Inventory_IN()
            {
                DateTime = Date,
                Product_ID = product.ID,
                Quantity_Received = InQuantity,
                CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now
            };
            unityOfWork.Inventory_IN.AddOne(inventory_IN);
            await unityOfWork.CompleteAsync();

            product.Inventory_Total_Amount = TotalInInventory(product.ID);
            product.Out_Inventory_Total_Amount = TotalOutInventory(product.ID);
            await unityOfWork.CompleteAsync();
            return LocalRedirect(returnUrl);
        }
        public async Task<IActionResult> DeleteFromInside(int id, string returnUrl)
        {
            Inventory_IN? inventory = await unityOfWork.Inventory_IN.FindAsync(x => x.ID == id);
            if (inventory == null) return NotFound();

            Product? product = await unityOfWork.Product.FindAsync(x => x.ID == inventory.Product_ID);

            inventory.DeletedDate = DateTime.Now;
            inventory.IsDeleted = true;
            await unityOfWork.CompleteAsync();

            product.Inventory_Total_Amount = TotalInInventory(product.ID);
            product.Out_Inventory_Total_Amount = TotalOutInventory(product.ID);
            await unityOfWork.CompleteAsync();
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> ShowOutsides(int? id , int? factoryID)
        {
            List<Product> products = new List<Product>();

            var order = unityOfWork.Order.QueryableFind(x => x.ID == id && x.IsDeleted != true && x.Factory_ID == factoryID)
                    .Include(x => x.Inventory_Outs).ThenInclude(x => x.Product)
                    .Include(x => x.Customer).Include(x => x.OrderDetails).FirstOrDefault();

            if (order == null)
            {
                ViewData["AllOrders"] = new SelectList(await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == factoryID), "ID", "ID");
                ViewBag.FcatoryID = factoryID;
                ViewBag.Status = true;
                return View(new Order { Inventory_Outs = await unityOfWork.Inventory_Out.FindAllAsync(x => x.IsDeleted != true && x.Order_ID == null && x.Product.Factory_ID == factoryID, new[] { "Product" })});
            }

            foreach (var item in order.OrderDetails.Where(x => x.IsDeleted != true))
            {
                if (order.Inventory_Outs.Count != 0 && order.Inventory_Outs.Any(x => x.Product_ID == item.Product_ID && x.IsDeleted != true))
                {
                    continue;
                }
                else
                {
                    products.Add(await unityOfWork.Product.FindAsync(x => x.IsDeleted != true && x.ID == item.Product_ID));
                }
            }

            ViewData["AllOrders"] = new SelectList(await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == order.Factory_ID), "ID", "ID", order.ID);
            ViewBag.FcatoryID = factoryID;
            return View(order);
        }

        public async Task<IActionResult> AddToOutsides(int? id , int? factoryID)
        {
            List<Product> products = new List<Product>();
            Order? order = unityOfWork.Order.QueryableFind(x => x.ID == id && x.IsDeleted != true)
                .Include(x => x.Inventory_Outs).Include(x => x.OrderDetails).ThenInclude(x => x.Product).FirstOrDefault();

            if (order == null)
            {
                if (Culture().Name == "en-US")
                {
                    ViewBag.Product_ID = new SelectList(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == factoryID), "ID", "English_Name");
                }
                else
                {
                    ViewBag.Product_ID = new SelectList(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == factoryID), "ID", "Arabic_Name");
                }
                ViewBag.CustomerID = new SelectList(await unityOfWork.Customer.FindAllAsync(x => x.IsDeleted != true), "ID", "Name");
                ViewBag.FactoryID = factoryID;
                return View();
            }
            foreach (var item in order.OrderDetails.Where(x => x.IsDeleted != true))
            {
                products.Add(item.Product);
            }

            if (Culture().Name == "en-US")
            {
                ViewBag.Product_ID = new SelectList(products , "ID", "English_Name");
            }
            else
            {
                ViewBag.Product_ID = new SelectList(products , "ID", "Arabic_Name");
            }
            
            ViewBag.Order_ID = order.ID;
            ViewBag.Customer_ID = order.Customer_ID;
            ViewBag.FactoryID = factoryID;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToOutsides(string Quantity_Sold ,[Bind("Order_ID,Customer_ID,Product_ID,DateTime,Quantity_Sold")]Inventory_Out productTo)
        {
            productTo.Quantity_Sold = double.Parse(Quantity_Sold, CultureInfo.InvariantCulture);

            Product product = await unityOfWork.Product.FindAsync(x => x.IsDeleted != true && x.ID == productTo.Product_ID , new[] { "Inventory_INs" } );
            Order order = await unityOfWork.Order.FindAsync(x => x.ID == productTo.Order_ID && x.IsDeleted != true , new[] { "OrderDetails" , "Inventory_Outs" } );
            
            Inventory_Out inventory_Out = new Inventory_Out()
            {
                Order_ID = productTo.Order_ID,
                DateTime = productTo.DateTime,
                Product_ID = productTo.Product_ID,
                Customer_ID = productTo.Customer_ID,
                Quantity_Sold = productTo.Quantity_Sold,
                CreatedBy = User.Identity.Name,
                CreatedDate = DateTime.Now
            };
            unityOfWork.Inventory_Out.AddOne(inventory_Out);
            await unityOfWork.CompleteAsync();

            product.Inventory_Total_Amount = TotalInInventory(product.ID);
            product.Out_Inventory_Total_Amount = TotalOutInventory(product.ID);

            if (order != null)
            {
                var productInOrder = order.OrderDetails.FirstOrDefault(x => x.IsDeleted != true && x.Product_ID == productTo.Product_ID);
                var productOuts = await unityOfWork.Inventory_Out.FindAllAsync(x => x.IsDeleted != true && x.Product_ID == productTo.Product_ID);
                if (productOuts.Sum(x => x.Quantity_Sold) >= productInOrder.value)
                    productInOrder.Delivery_Status = true;
            }
            await unityOfWork.CompleteAsync();

            if (order != null)
            {
                if (!order.OrderDetails.Any(x => x.Delivery_Status == false))
                    order.OrderStatus = true;
            }
            await unityOfWork.CompleteAsync();
            return RedirectToAction("ShowOutSides", new { id = order?.ID, factoryID = product.Factory_ID });
        }

        [HttpPost]
        public async Task<IActionResult> EditOutside(int? id ,string Quantity_Sold, string returnUrl)
        {
            double QuantityOut = double.Parse(Quantity_Sold, CultureInfo.InvariantCulture);

            Inventory_Out inventory_Out = await unityOfWork.Inventory_Out.FindAsync(x => x.IsDeleted != true && x.ID == id);
            Product product = await unityOfWork.Product.FindAsync(x => x.IsDeleted != true && x.ID == inventory_Out.Product_ID);
            Order? order = await unityOfWork.Order.FindAsync(x => x.IsDeleted != true && x.ID == inventory_Out.Order_ID , new[] { "OrderDetails" });

            if (inventory_Out == null) return NotFound();

            inventory_Out.Quantity_Sold = QuantityOut;
            inventory_Out.ModifiedBy = User.Identity.Name;
            inventory_Out.ModifiedDate = DateTime.Now;
            await unityOfWork.CompleteAsync();

            product.Inventory_Total_Amount = TotalInInventory(product.ID);
            product.Out_Inventory_Total_Amount = TotalOutInventory(product.ID);

            if (order != null)
            {
                var productInOrder = order.OrderDetails.FirstOrDefault(x => x.IsDeleted != true && x.Product_ID == product.ID);
                var productOuts = await unityOfWork.Inventory_Out.FindAllAsync(x => x.IsDeleted != true && x.Product_ID == product.ID);
                if (productOuts.Sum(x => x.Quantity_Sold) >= productInOrder.value)
                    productInOrder.Delivery_Status = true;
            }

            await unityOfWork.CompleteAsync();

            if (order != null)
            {
                if (!order.OrderDetails.Any(x => x.Delivery_Status == false))
                    order.OrderStatus = true;
            }
            await unityOfWork.CompleteAsync();

            return LocalRedirect(returnUrl);
        }
        public async Task<IActionResult> DeleteOutSide(int? id , string returnUrl)
        {
            Inventory_Out inventory_Out = await unityOfWork.Inventory_Out.FindAsync(x => x.IsDeleted != true && x.ID == id);
            Product product = await unityOfWork.Product.FindAsync(x => x.IsDeleted != true && x.ID == inventory_Out.Product_ID);

            if (inventory_Out == null) return NotFound();

            inventory_Out.IsDeleted = true;
            inventory_Out.DeletedDate = DateTime.Now;
            await unityOfWork.CompleteAsync();

            product.Inventory_Total_Amount = TotalInInventory(product.ID);
            product.Out_Inventory_Total_Amount = TotalOutInventory(product.ID);
            await unityOfWork.CompleteAsync();

            return LocalRedirect(returnUrl);
        }
        private CultureInfo Culture()
        {
            var currentCulture = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            return currentCulture.RequestCulture.Culture;
        }
        public double TotalInInventory(int id)
        {
            var outs = unityOfWork.Inventory_Out.FindAll(x => x.IsDeleted != true && x.Product_ID == id);
            var Ins = unityOfWork.Inventory_IN.FindAll(x => x.IsDeleted != true && x.Product_ID == id);
            double result = Ins.Sum(x => x.Quantity_Received) - outs.Sum(x => x.Quantity_Sold);
            if (result < 0)
                return 0;
            return Math.Round(result , 3);
        }
        public double TotalOutInventory(int id)
        {
            var productions = unityOfWork.Production.FindAll(x => x.IsDeleted != true && x.Product_ID == id);
            var Ins = unityOfWork.Inventory_IN.FindAll(x => x.IsDeleted != true && x.Product_ID == id);
            double result = productions.Sum(x => x.Quantity_Producted) - Ins.Sum(x => x.Quantity_Received);
            if (result < 0)
                return 0;
            return Math.Round(result, 3);
        }
        public IActionResult CheckDateTime(DateTime DateTime , int? Order_ID)
        {
            Order order = unityOfWork.Order.Find(x => x.IsDeleted != true && x.ID == Order_ID);

            if (order == null)
            {
                return Json(true);
            }
            if (DateTime < order.Start_Date)
                return Json(false);

            return Json(true);
        }
        public IActionResult CheckQuantitySold(double Quantity_Sold , int Product_ID)
        {
            Product product = unityOfWork.Product.Find(x => x.IsDeleted != true && x.ID == Product_ID , new[] { "Inventory_INs" });
            if (product.Inventory_INs.Where(x => x.IsDeleted != true).Sum(x => x.Quantity_Received) < Quantity_Sold)
                return Json(false);

            return Json(true);
        }
    }
}
