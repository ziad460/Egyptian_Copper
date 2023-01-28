using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CopperFactory.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CopperFactory.Interfaces;
using Microsoft.Extensions.Localization;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Admin,PowerUser")]
    public class FactoryController : Controller
    {
        private readonly IUnityOfWork unityOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public FactoryController(IUnityOfWork _unityOfWork)
        {
            unityOfWork = _unityOfWork;
        }

        // GET: Factory
        public async Task<IActionResult> Index()
        {
            return View(await unityOfWork.Factory.FindAllAsync(x => x.IsDeleted != true , new[] { "Zone" }));
        }
        public IActionResult FactoriesHistory()
        {
            return View(unityOfWork.Factory.GetAll(new[] { "Zone" } ));
        }
        // GET: Factory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            var factory =  await unityOfWork.Factory.FindAsync(x => x.ID == id && x.IsDeleted != true , new[] { "Zone" });
            if (factory == null || factory.IsDeleted == true)
            {
                return NotFound();
            }

            return View(factory);
        }
        [Authorize(Roles = "PowerUser")]
        // GET: Factory/Create
        public IActionResult Create()
        {
            if (Culture().Name == "en-US")
            {
                ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "English_Name");
            }
            else
            {
                ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name");
            }
            return View();
        }

        // POST: Factory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "PowerUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("English_Name,Arabic_Name,Max_Capacity,Zone_ID")] Factory factory)
        {
            if (!ModelState.IsValid)
            {
                if (Culture().Name == "en-US")
                {
                    ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "English_Name" , factory.Zone_ID);
                }
                else
                {
                    ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name" , factory.Zone_ID);
                }
                return View(factory);
            }
            factory.CreatedBy = User.Identity.Name;
            factory.CreatedDate = DateTime.Now;
            unityOfWork.Factory.AddOne(factory);
            await unityOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "PowerUser")]
        // GET: Factory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factory = await unityOfWork.Factory.FindAsync(x => x.ID == id);
            if (factory == null || factory.IsDeleted == true)
            {
                return NotFound();
            }
            if (Culture().Name == "en-US")
            {
                ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "English_Name", factory.Zone_ID);
            }
            else
            {
                ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name", factory.Zone_ID);
            }
            return View(factory);
        }

        [HttpPost]
        [Authorize(Roles = "PowerUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,English_Name,Arabic_Name,Max_Capacity,Zone_ID,CreatedBy,CreatedDate")] Factory factory)
        {
            if (!ModelState.IsValid)
            {
                if (Culture().Name == "en-US")
                {
                    ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "English_Name", factory.Zone_ID);
                }
                else
                {
                    ViewData["Zone_ID"] = new SelectList(unityOfWork.Zone.FindAll(x => x.IsDeleted != true), "ID", "Arabic_Name", factory.Zone_ID);
                }
                return View(factory);
            }
            factory.ModifiedDate = DateTime.Now;
            factory.ModifiedBy = User.Identity.Name;
            unityOfWork.Factory.UpdateOne(factory);
            await unityOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "PowerUser")]
        // GET: Factory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factory = await unityOfWork.Factory.FindAsync(x => x.ID == id, new[] { "Zone" });
            if (factory == null || factory.IsDeleted == true)
            {
                return NotFound();
            }

            return View(factory);
        }

        // POST: Factory/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "PowerUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await unityOfWork.Factory.GetAllAsync() == null)
            {
                return Problem("Entity set 'ApplicationDbContext.factories'  is null.");
            }
            var factory = await unityOfWork.Factory.FindAsync(x => x.ID == id, new[] { "Zone" });
            if (factory == null || factory.IsDeleted == true)
            {
                return NotFound();
            }
            factory.IsDeleted = true;
            factory.DeletedDate = DateTime.Now;
            await unityOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FactoryExists(int id)
        {
          return unityOfWork.Factory.GetAll().Any(e => e.ID == id);
        }
        public CultureInfo Culture()
        {
            var currentCulture = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            return currentCulture.RequestCulture.Culture;
        }
    }
}
