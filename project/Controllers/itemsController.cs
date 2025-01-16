using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Controllers
{
    public class itemsController : Controller
    {
        private readonly projectContext _context;

        public itemsController(projectContext context)
        {
            _context = context;
        }

       


        public async Task<IActionResult> dashboard()
        {
            {
                string sql = "";

                //var builder = WebApplication.CreateBuilder();
                //string conStr = builder.Configuration.GetConnectionString("projectContext");
                //SqlConnection conn = new SqlConnection(conStr);
                SqlConnection conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\PC\\Documents\\asiah.mdf;Integrated Security=True;Connect Timeout=30");
                SqlCommand comm;
                conn.Open();
                sql = "SELECT COUNT( Id ) FROM items where category =1";
                comm = new SqlCommand(sql, conn);
                ViewData["d1"] = (int)comm.ExecuteScalar();
                sql = "SELECT COUNT( Id) FROM items where category =2";
                comm = new SqlCommand(sql, conn);
                ViewData["d2"] = (int)comm.ExecuteScalar();
                ViewData["total"] = (int)ViewData["d1"] + (int)ViewData["d2"];
                return View();
            }
        }

        public IActionResult listitem()
        {
            var v = HttpContext.Session.GetString("Role");

            if (v == "admin")
            {
                 var items = _context.items.FromSqlRaw("select * from items order by category").ToList(); 
            return View(items);
            }
            else
            {
                return RedirectToAction("login", "usersaccounts");
            }
          
        }

        // GET: items
        public async Task<IActionResult> Index()
        {
            ViewData["role"] = HttpContext.Session.GetString("Role");

            return View(await _context.items.ToListAsync());
        }

        // GET: items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["role"] = HttpContext.Session.GetString("Role");

            if (id == null)
            {
                return NotFound();
            }

            var items = await _context.items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (items == null)
            {
                return NotFound();
            }

            return View(items);
        }

        // GET: items/Create  
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "admin")
            {
                return RedirectToAction("Login", "UsersAccounts"); // Redirect if not admin
            }

            ViewData["Title"] = "Create"; // Set the title for the page
            return View();
        }

        // POST: items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, [Bind("Id,name,description,price,discount,pubdate,category,itemquantity,imgfile")] items items)
        {
            {
                if (file != null)
                {
                    string filename = file.FileName;
                    //  string  ext = Path.GetExtension(file.FileName);
                    string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images"));
                    using (var filestream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                    { await file.CopyToAsync(filestream); }

                    items.imgfile = filename;
                }


                _context.Add(items);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "admin")
            {
                return RedirectToAction("login", "usersaccounts");
            }

            if (id == null)
            {
                return NotFound();
            }

            var items = await _context.items.FindAsync(id);
            if (items == null)
            {
                return NotFound();
            }
            return View(items);
        }

        // POST: items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598. //317598
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile file, int id, [Bind("Id,name,description,price,discount,pubdate,category,itemquantity,imgfile")] items items)
        {
            if (id != items.Id)
            {
                return NotFound();
            }

            if (file != null)
            {
                string filename = file.FileName;
                // string ext = Path.GetExtension(file.FileName);
                string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images"));
                using (var filestream = new FileStream(Path.Combine(path, filename), FileMode.Create))
                { await file.CopyToAsync(filestream); }
                items.imgfile = filename;
            }
            _context.Update(items);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            return View(items);
        }

            // GET: items/Delete/5
            public async Task<IActionResult> Delete(int? id)
        {
            // users checks
            var role = HttpContext.Session.GetString("Role");
            if (role != "admin")
            {
                return RedirectToAction("login", "usersaccounts");
            }

            if (id == null)
            {
                return NotFound();
            }

            var items = await _context.items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (items == null)
            {
                return NotFound();
            }

            return View(items);
        }

        // POST: items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var items = await _context.items.FindAsync(id);
            if (items != null)
            {
                _context.items.Remove(items);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool itemsExists(int id)
        {
            return _context.items.Any(e => e.Id == id);
        }
    }
}
