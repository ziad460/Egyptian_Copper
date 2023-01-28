using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CopperFactory.Interfaces;
using CopperFactory.Models;

namespace CopperFactory.Controllers
{
    [Authorize(Roles = "Admin,PowerUser")]
    public class ZoneController : Controller
    {
        private readonly IUnityOfWork unityOfWork;
        public ZoneController(IUnityOfWork unityOfWork)
        {
            this.unityOfWork = unityOfWork;
        }
        // GET: Areas
        public async Task<IActionResult> Index()
        {
            return View(await unityOfWork.Zone.FindAllAsync(x => x.IsDeleted != true));
        }
        public async Task<IActionResult> ZoneHistory()
        {
            return View(await unityOfWork.Zone.GetAllAsync());
        }
        // GET: Areas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var area = await unityOfWork.Zone.FindAsync(x=>x.ID == id && x.IsDeleted != true);
            if (area == null || area.IsDeleted == true)
            {
                return NotFound();
            }

            return View(area);
        }
        [Authorize(Roles = "PowerUser")]
        // GET: Areas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Areas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "PowerUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Arabic_Name,English_Name")] Zone zone)
        {
            if (!ModelState.IsValid)
            {
                return View(zone);
            }
            zone.CreatedBy = User.Identity.Name;
            zone.CreatedDate = DateTime.Now;
            
            unityOfWork.Zone.AddOne(zone);
            await unityOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "PowerUser")]
        // GET: Areas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var area = await unityOfWork.Zone.FindAsync(x => x.ID == id);
            if (area == null || area.IsDeleted == true)
            {
                return NotFound();
            }
            return View(area);
        }

        // POST: Areas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "PowerUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("English_Name,Arabic_Name,ID,CreatedBy,CreatedDate")] Zone zone)
        {
            if (!ModelState.IsValid)
            {
                return View(zone);
            }
            
            try
            {
                zone.ModifiedDate = DateTime.Now;
                zone.ModifiedBy = User.Identity.Name;
                unityOfWork.Zone.UpdateOne(zone);
                await unityOfWork.CompleteAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AreaExists(zone.ID))
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

        [Authorize(Roles = "PowerUser")]
        // GET: Areas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var area = await unityOfWork.Zone.FindAsync(x => x.ID == id);
            if (area == null || area.IsDeleted == true)
            {
                return NotFound();
            }

            return View(area);
        }

        // POST: Areas/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "PowerUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await unityOfWork.Zone.GetAllAsync() == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Areas'  is null.");
            }
            var area = await unityOfWork.Zone.FindAsync(x => x.ID == id);
            if (area != null)
            {
                area.IsDeleted = true;
                area.DeletedDate = DateTime.Now;
            }
            await unityOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AreaExists(long id)
        {
            return unityOfWork.Zone.GetAll().Any(e => e.ID == id);
        }
    }
}
