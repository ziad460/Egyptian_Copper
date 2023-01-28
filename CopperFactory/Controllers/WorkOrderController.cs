using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CopperFactory.Models;
using CopperFactory.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mime;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Collections;
using CopperFactory.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Sales,Admin,PowerUser")]
    public class WorkOrderController : Controller
    {
        private readonly IUnityOfWork unityOfWork;
        
        public WorkOrderController(IUnityOfWork unityOfWork)
        {
            this.unityOfWork = unityOfWork;
        }
        public async Task<IActionResult> WorkOrdersHistory()
        {
            return View(await unityOfWork.Order.GetAllAsync(new[] { "Customer" }));
        }
        public async Task<IActionResult> ProductsToOrderHistory()
        {
            return View(await unityOfWork.OrderDetails.GetAllAsync(new[] { "Order", "Product" }));
        }

        public double TotalOrderQuantity(int? id)
        {
            double sum = 0;
            Order order = unityOfWork.Order.Find(x => x.ID == id , new[] { "OrderDetails" } );
            foreach (var item in order.OrderDetails.Where(x => x.IsDeleted != true))
            {
                sum += item.value;
            }
            return sum;
        }
        public ActionResult CheckEndDate(DateTime End_Date, DateTime Start_Date)
        {
            var result = (End_Date < Start_Date);
            if (result)
                return Json(false);
            else
                return Json(true);
        }
        // GET: Orders/id
        public async Task<IActionResult> Index(int? id)  // Factory ID
        {
            ViewBag.FactoryID = id;
            if(id == null)
                return NotFound();
            else
                return View(await unityOfWork.Order.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == id, new[] { "OrderDetails" , "Customer" }));
        }
        public async Task<IActionResult> Details(int? id)  //Order ID
        {
            List<Product> products = new();

            var order = unityOfWork.Order.QueryableFind(x => x.ID == id && x.IsDeleted != true)
                    .Include(x => x.OrderDetails.Where(x => x.IsDeleted != true)).ThenInclude(x => x.Product)
                    .Include(x => x.Customer).FirstOrDefault();

            if (order == null) 
                return NotFound();

            var factoryProducts = await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == order.Factory_ID);
            foreach (var item in factoryProducts)
            {
                if (order.OrderDetails.Any(x => x.Product_ID == item.ID))
                    continue;
                else
                    products.Add(item);
            }

            if (Culture().Name == "en-US")
            {
                ViewBag.Products = new SelectList(products, "ID", "English_Name");
            }
            else
            {
                ViewBag.Products = new SelectList(products, "ID", "Arabic_Name");
            }

            if (products.Count != 0)
                ViewBag.Status = true;
            else
                ViewBag.Status = false;

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create(int id)
        {
            ViewBag.FactoryID = id;
            ViewData["Customer_ID"] = new SelectList(unityOfWork.Customer.FindAll(x => x.IsDeleted != true), "ID", "Name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id ,[Bind("Factory_ID,Customer_ID,Start_Date,End_Date")]Order order)   // Factory ID
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FactoryID = id;
                ViewData["Customer_ID"] = new SelectList(unityOfWork.Customer.FindAll(x => x.IsDeleted != true), "ID", "Name",order.Customer_ID);
                return View(order);
            }

            order.OrderStatus = false;
            order.Total_Products_Quantity = 0;
            order.CreatedBy = User.Identity.Name;
            order.CreatedDate = DateTime.Now;

            unityOfWork.Order.AddOne(order);
            await unityOfWork.CompleteAsync();
            
            return RedirectToAction(nameof(Index) , new { id = id });
        }

        public async Task<IActionResult> AddProductToOrder(int? id) // Order ID
        {
            var order = await unityOfWork.Order.FindAsync(x => x.ID == id && x.IsDeleted != true , new[] { "OrderDetails" });

            if (order == null)
            {
                return NotFound();
            }
            
            if (Culture().Name == "en-US")
            {
                ViewBag.Products = new SelectList(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == order.Factory_ID && order.OrderDetails.Any(p => p.Product_ID != x.ID)), "ID", "English_Name");
            }
            else
            {
                ViewBag.Products = new SelectList(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true && x.Factory_ID == order.Factory_ID && order.OrderDetails.Any(p => p.Product_ID != x.ID)), "ID", "Arabic_Name");
            }
            
            return View(new OrderDetails { Order_ID = order.ID});
        }
        [HttpPost]
        public async Task<IActionResult> AddProductToOrder([Bind("Order_ID,Product_ID")]OrderDetails orderDetails , string value , string returnUrl)
        {
            orderDetails.value = double.Parse(value, CultureInfo.InvariantCulture);
            if (!ModelState.IsValid)
            {
                return View(orderDetails);
            }
            var order = await unityOfWork.Order.FindAsync(x => x.ID == orderDetails.Order_ID && x.IsDeleted != true , new[] { "OrderDetails" } );
            // Check if product added before to this order or not
            var check = unityOfWork.OrderDetails.GetAll().Any(p => p.Order_ID == orderDetails.Order_ID && p.Product_ID == orderDetails.Product_ID);
            if (check)
            {
                OrderDetails orderdet = await unityOfWork.OrderDetails.FindAsync(p => p.Order_ID == orderDetails.Order_ID && p.Product_ID == orderDetails.Product_ID);
                orderdet.value = orderDetails.value;
                orderdet.IsDeleted = false;
                await unityOfWork.CompleteAsync();

                order.Total_Products_Quantity = order.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value);
                await unityOfWork.CompleteAsync();
            }
            else
            {
                orderDetails.CreatedBy = User.Identity.Name;
                orderDetails.CreatedDate = DateTime.Now;
                unityOfWork.OrderDetails.AddOne(orderDetails);
                await unityOfWork.CompleteAsync();

                order.Total_Products_Quantity = order.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value);
                await unityOfWork.CompleteAsync();
            }
            return LocalRedirect(returnUrl);
        }
        public async Task<IActionResult> EditProductToOrder(int? id) // OrderDetails ID
        {
            var orderDetails = await unityOfWork.OrderDetails.FindAsync(x => x.ID == id && x.IsDeleted != true);
            if (id == null || orderDetails == null)
            {
                return NotFound();
            }
            
            return View(orderDetails);
        }
        [HttpPost]
        public async Task<IActionResult> EditProductToOrder([Bind("ID,Order_ID,value")] OrderDetails orderDetails , string value , string returnUrl) // OrderDetails ID
        {
            orderDetails.value = double.Parse(value, CultureInfo.InvariantCulture);

            var details = await unityOfWork.OrderDetails.FindAsync(x => x.ID == orderDetails.ID && x.IsDeleted != true);
            var order = await unityOfWork.Order.FindAsync(x => x.ID == orderDetails.Order_ID && x.IsDeleted != true , new[] { "OrderDetails"});
            
            if (details == null || order == null) return NotFound();

            details.value = orderDetails.value;
            details.ModifiedDate = DateTime.Now;
            details.ModifiedBy = User.Identity.Name;
            await unityOfWork.CompleteAsync();

            order.Total_Products_Quantity = order.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value);
            await unityOfWork.CompleteAsync();

            return LocalRedirect(returnUrl);
        }
        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)  // Order ID
        {
            var order = await unityOfWork.Order.FindAsync(x => x.ID == id && x.IsDeleted != true);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.FactoryID = order.Factory_ID;
            ViewData["Customer_ID"] = new SelectList(unityOfWork.Customer.FindAll(x => x.IsDeleted != true), "ID", "Name", order.Customer_ID);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order orderVM)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Customer_ID"] = new SelectList(unityOfWork.Customer.FindAll(x => x.IsDeleted != true), "ID", "Name", orderVM.Customer_ID);
                return View(orderVM);
            }
            var order = await unityOfWork.Order.FindAsync(x => x.ID == orderVM.ID && x.IsDeleted != true);
            if (order == null)
            {
                return NotFound();
            }
            order.Start_Date = orderVM.Start_Date;
            order.End_Date = orderVM.End_Date;
            order.Customer_ID = orderVM.Customer_ID;
            order.ModifiedDate = DateTime.Now;
            order.ModifiedBy = User.Identity.Name;

            await unityOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index) , new { id = order.Factory_ID});
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var order = await unityOfWork.Order.FindAsync(x => x.ID == id && x.IsDeleted != true , new[] { "Customer" });
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        public async Task<IActionResult> DeleteProductToOrder(int? id)
        {
            var orderDetails = await unityOfWork.OrderDetails.FindAsync(x => x.ID == id && x.IsDeleted != true);
            if (orderDetails == null)
            {
                return NotFound();
            }
            var order = await unityOfWork.Order.FindAsync(x => x.ID == orderDetails.Order_ID && x.IsDeleted != true, new[] { "OrderDetails" });

            orderDetails.IsDeleted = true;
            orderDetails.DeletedDate = DateTime.Now;
            await unityOfWork.CompleteAsync();

            order.Total_Products_Quantity = order.OrderDetails.Where(x => x.IsDeleted != true).Sum(x => x.value);
            await unityOfWork.CompleteAsync();

            return RedirectToAction(nameof(Details), new { id = orderDetails.Order_ID });
        }
        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id , string returnUrl)
        {
            var order = await unityOfWork.Order.FindAsync(x => x.ID == id && x.IsDeleted != true);

            if (order == null) return NotFound();

            order.IsDeleted = true;
            order.DeletedDate = DateTime.Now;
            await unityOfWork.CompleteAsync();
            return LocalRedirect(returnUrl);
        }

        private bool OrderExists(int id)
        {
          return unityOfWork.Order.GetAll().Any(e => e.ID == id);
        }
        private CultureInfo Culture()
        {
            var currentCulture = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            return currentCulture.RequestCulture.Culture;
        }
    }
}
