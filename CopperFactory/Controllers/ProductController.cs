using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CopperFactory.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using CopperFactory.Interfaces;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Admin,PowerUser")]
    public class ProductController : Controller
    {
        private readonly IUnityOfWork unityOfWork;

        public ProductController(IUnityOfWork unityOfWork)
        {
            this.unityOfWork = unityOfWork;
        }
        public async Task<IActionResult> ProductHistory()
        {
            return View(await unityOfWork.Product.GetAllAsync(new[] { "Factory" }));
        }
        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await unityOfWork.Product.FindAllAsync(x => x.IsDeleted != true , new[] { "Factory" }));
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            return View(await unityOfWork.Product.FindAsync(x => x.ID == id && x.IsDeleted !=true , new[] { "Factory" }));
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            if (Culture().Name == "en-US")
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "English_Name");
            }
            else
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name");
            }
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("English_Name,Arabic_Name,Factory_ID")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;
                product.CreatedBy = User.Identity.Name;
                unityOfWork.Product.AddOne(product);
                await unityOfWork.CompleteAsync();

                return RedirectToAction(nameof(Index));
            }
            if (Culture().Name == "en-US")
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "English_Name", product.Factory_ID);
            }
            else
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name", product.Factory_ID);
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await unityOfWork.Product.FindAsync(x => x.ID == id);
            if (product == null && product.IsDeleted == true)
            {
                return NotFound();
            }

            if (Culture().Name == "en-US")
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "English_Name", product.Factory_ID);
            }
            else
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name", product.Factory_ID);
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,English_Name,Arabic_Name,Factory_ID,CreatedBy,CreatedDate")] Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    product.ModifiedDate = DateTime.Now;
                    product.ModifiedBy = User.Identity.Name;
                    unityOfWork.Product.UpdateOne(product);
                    await unityOfWork.CompleteAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            if (Culture().Name == "en-US")
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "English_Name", product.Factory_ID);
            }
            else
            {
                ViewBag.factories = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name", product.Factory_ID);
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product product = await unityOfWork.Product.FindAsync(x => x.ID == id , new[] {"Factory"});
            if (product == null && product.IsDeleted == true)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Product product = await unityOfWork.Product.FindAsync(x => x.ID == id);
            if (product != null)
            {
                product.IsDeleted = true;
                product.DeletedDate = DateTime.Now;
            }
            await unityOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return unityOfWork.Product.GetAll().Any(e => e.ID == id);
        }
        private CultureInfo Culture()
        {
            var currentCulture = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            return currentCulture.RequestCulture.Culture;
        }
    }
}
