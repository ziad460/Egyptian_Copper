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
using System.Text.RegularExpressions;
using System.Configuration;
using CopperFactory.Interfaces;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Admin,PowerUser")]
    public class CustomersController : Controller
    {
        private readonly IUnityOfWork unityOfWork;

        public CustomersController(IUnityOfWork _unityOfWork)
        {
            unityOfWork = _unityOfWork;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await unityOfWork.Customer.FindAllAsync(x => x.IsDeleted != true));
        }
        public async Task<IActionResult> CustomersHistory()
        {
            return View(await unityOfWork.Customer.GetAllAsync());
        }
        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await unityOfWork.Customer.FindAsync(x => x.ID == id && x.IsDeleted != true);
            if (customer == null || customer.IsDeleted == true)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,PhoneNumber")] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }
            customer.CreatedBy = User.Identity.Name;
            customer.CreatedDate = DateTime.Now;
            unityOfWork.Customer.AddOne(customer);
            await unityOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await unityOfWork.Customer.FindAsync(x => x.ID == id);
            if (customer == null || customer.IsDeleted == true)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Address,PhoneNumber,CreatedBy,CreatedDate")] Customer customer)
        {
            if (id != customer.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    customer.ModifiedDate = DateTime.Now;
                    customer.ModifiedBy = User.Identity.Name;
                    unityOfWork.Customer.UpdateOne(customer);
                    await unityOfWork.CompleteAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.ID))
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
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await unityOfWork.Customer.FindAsync(x => x.ID == id);

            if (customer == null || customer.IsDeleted == true)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await unityOfWork.Customer.GetAllAsync() == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Customers'  is null.");
            }
            var customer = await unityOfWork.Customer.FindAsync(x => x.ID == id);
            if (customer != null)
            {
                customer.DeletedDate = DateTime.Now;
                customer.IsDeleted = true;
            }

            await unityOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
          return unityOfWork.Customer.GetAll().Any(e => e.ID == id);
        }
        private CultureInfo Culture()
        {
            var currentCulture = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            return currentCulture.RequestCulture.Culture;
        }
    }
}
