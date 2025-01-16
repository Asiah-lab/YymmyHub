using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;
using System.Text.Json;

namespace project.Controllers
{
    public class ordersController : Controller
    {
        private readonly projectContext _context;

        public ordersController(projectContext context)
        {
            _context = context;
        }

        List<buyitems> Bbks = new List<buyitems>();

        [HttpPost]
        public async Task<IActionResult> cartadd(int iditem, int quantity)
        {
            await HttpContext.Session.LoadAsync();
            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
            {
                Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
            }

            var itemm = await _context.items.FromSqlRaw("select * from items  where Id= '" + iditem + "'  ").FirstOrDefaultAsync();
            if (itemm == null)
            {
                ViewData["Error"] = "items not Found";
                return View("itemBuyDetail", itemm);
            }
            if (quantity > itemm.itemquantity)
            {
                ViewData["Error"] = "Requested quantity exceeds available stock";
                return View("itemBuyDetail", itemm);
            }
            Bbks.Add(new buyitems
            {
                name = itemm.name,
                Price = itemm.price,
                quant = quantity
            });

            itemm.itemquantity -= quantity;
            _context.Update(itemm);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(Bbks));
            return RedirectToAction("CartBuy");
        }


        public async Task<IActionResult> CartBuy()
        {

            await HttpContext.Session.LoadAsync();
            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
            {
                Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
            }
            return View(Bbks);
        }

        public async Task<IActionResult> purchase_Report()

        {
            HttpContext.Session.LoadAsync();
            string ss = HttpContext.Session.GetString("Role");
            if (ss == "admin")
            {
                ViewData["name"] = HttpContext.Session.GetString("Name");


                List<report> orItems = await _context.report.FromSqlRaw("SELECT custname, SUM(total) AS total FROM orders GROUP BY custname").ToListAsync();

                return View(orItems);

            }
            else
                return RedirectToAction("login", "usersaccounts");
        }





        public async Task<IActionResult> ordersdetail(string? custname)
        {
            var orItems = await _context.orders.FromSqlRaw("select * from orders where custname = '" + custname + "' ").ToListAsync();
            return View(orItems);
        } 



        public async Task<IActionResult> CatalogueBuy()
        {
            var v = HttpContext.Session.GetString("Role");

            if (v == "customer")
            {
                 return View(await _context.items.ToListAsync());
            }
            else
            {
                return RedirectToAction("login", "usersaccounts");
            }

           
        }

        public async Task<IActionResult> itemBuyDetail(int? id)
        {
            HttpContext.Session.LoadAsync();
            string role = HttpContext.Session.GetString("Role");
            if (role != "customer")
            {
                return RedirectToAction("login", "usersaccounts");

            }
            var item = await _context.items.FindAsync(id);
            return View(item);
        }

        public async Task<IActionResult> Buy()
        {
            await HttpContext.Session.LoadAsync();
            var sessionString = HttpContext.Session.GetString("Cart");
            if (sessionString is not null)
            {
                Bbks = JsonSerializer.Deserialize<List<buyitems>>(sessionString);
            }
            string ctname = HttpContext.Session.GetString("Name");
            orders bkorder = new orders();
            bkorder.total = 0;
            bkorder.custname = ctname;
            bkorder.orderdate = DateTime.Today;
            _context.orders.Add(bkorder);
            await _context.SaveChangesAsync();
            var bord = await _context.orders.FromSqlRaw("select * from orders  where custname= '" + ctname + "' ").OrderByDescending(e => e.Id).FirstOrDefaultAsync();
            int ordid = bord.Id;
            decimal tot = 0;
            foreach (var bk in Bbks.ToList())
            {
                orderline oline = new orderline();
                oline.orderid = ordid;
                oline.itemname = bk.name;
                oline.itemquant = bk.quant;
                oline.itemprice = bk.Price;
                _context.orderline.Add(oline);
                await _context.SaveChangesAsync();
                var bkk = await _context.items.FromSqlRaw("select * from items  where name= '" + bk.name + "' ").FirstOrDefaultAsync();
                bkk.itemquantity = bkk.itemquantity - bk.quant;
                _context.Update(bkk);
                await _context.SaveChangesAsync();
                tot = tot + (bk.quant * bk.Price);
            }
            bord.total = Convert.ToInt16(tot);
            _context.Update(bord);
            await _context.SaveChangesAsync();

            ViewData["Message"] = "Thank you See you again";
            Bbks = new List<buyitems>();
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(Bbks));
            return RedirectToAction("MyOrder");
        }

        public async Task<IActionResult> MyOrder()
        {
            await HttpContext.Session.LoadAsync();

            ViewData["role"] = HttpContext.Session.GetString("Role");
            string ctname = HttpContext.Session.GetString("Name");
            return View(await _context.orders.FromSqlRaw("select * from orders  where custname = '" + ctname + "' ").ToListAsync());
        }

        public async Task<IActionResult> Orderline(int? orid)

        {
            await HttpContext.Session.LoadAsync();

            ViewData["role"] = HttpContext.Session.GetString("Role");
            var buybk = await _context.orderline.FromSqlRaw("select * from orderline  where orderid = '" + orid + "' ").ToListAsync();
            return View(buybk);
        }



        // GET: orders
        public async Task<IActionResult> Index()
        {
            return View(await _context.orders.ToListAsync());
        }

        // GET: orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            await HttpContext.Session.LoadAsync();
            ViewData["role"] = HttpContext.Session.GetString("Role");
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,custname,orderdate,total")] orders orders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,custname,orderdate,total")] orders orders)
        {
            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ordersExists(orders.Id))
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
            return View(orders);
        }

        // GET: orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.orders.FindAsync(id);
            if (orders != null)
            {
                _context.orders.Remove(orders);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ordersExists(int id)
        {
            return _context.orders.Any(e => e.Id == id);
        }
    }
}
