using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using project.Data;
using project.Models;

namespace project.Controllers
{
    public class usersaccountsController : Controller
    {
        private readonly projectContext _context;

        public usersaccountsController(projectContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Name"))
                return View();
            else
            {
                string na = HttpContext.Request.Cookies["Name"].ToString();
                string ro = HttpContext.Request.Cookies["Role"].ToString();
                HttpContext.Session.SetString("Name", na);
                HttpContext.Session.SetString("Role", ro);
                if (ro == "customer")
                    return RedirectToAction(nameof(customer_home));
                else
                    return RedirectToAction("admin_home", "usersaccounts");
            }
        }

        [HttpPost, ActionName("login")]
        public async Task<IActionResult> login(string na, string pa, string auto)
        {
            var ur = await _context.usersaccounts.FromSqlRaw("SELECT * FROM usersaccounts where name ='" + na + "' and  pass ='" + pa + "' ").FirstOrDefaultAsync();

            if (ur != null)
            {

                int id = ur.Id;
                string na1 = ur.name;
                string ro = ur.role;
                HttpContext.Session.SetString("userid", Convert.ToString(id));
                HttpContext.Session.SetString("Name", na1);
                HttpContext.Session.SetString("Role", ro);

                if (auto == "on")
                {
                    HttpContext.Response.Cookies.Append("Name", na1);
                    HttpContext.Response.Cookies.Append("Role", ro);

                }

                if (ro == "customer")
                    return RedirectToAction("customer_home", "usersaccounts");
                else if (ro == "admin")
                    return RedirectToAction("admin_home", "usersaccounts");
                else
                    return View();
            }
            else
            {
                ViewData["Message"] = "wrong user name password";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("Name");
            HttpContext.Response.Cookies.Delete("Role");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "usersaccounts");
        }


        public IActionResult registration()
        {
          
                return View();
        
        }

        [HttpPost]
        public IActionResult registration([Bind("name,email,job,gender,married, location")] customer cust, [Bind("name,pass,role")] usersaccounts use)
        {
            SqlConnection conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\PC\\Documents\\asiah.mdf;Integrated Security=True;Connect Timeout=30");
            // MD5 md5 = new MD5CryptoServiceProvider(); string paa = Encoding.ASCII.GetString(md5.ComputeHash(ASCIIEncoding.Default.GetBytes(use.pass)));

            conn.Open(); string sql = "select * from usersaccounts  where name = '" + use.name + "' and pass = '" + use.pass + "'";
            SqlCommand comm = new SqlCommand(sql, conn);
            SqlDataReader reader = comm.ExecuteReader();
            if (reader.Read())
            {
                ViewData["message"] = "name and password already exists";
                reader.Close();
            }
            else
            {
                reader.Close();
                sql = "insert into customer (name, email,job,married,gender,location)  values  ('" + cust.name + "','" + cust.email + "','" + cust.job + "','" + cust.married + "' ,'" + cust.gender + "','" + cust.location + "')"; comm = new SqlCommand(sql, conn);
                comm.ExecuteNonQuery();
                use.role = "customer";
                sql = "insert into usersaccounts (name,pass,role)  values  ('" + use.name + "','" + use.pass + "','" + use.role + "')";
                comm = new SqlCommand(sql, conn); 
                comm.ExecuteNonQuery();
                ViewData["message"] = "Sucessfully added";
                HttpContext.Session.SetString("Name", use.name);
                HttpContext.Session.SetString("Role", use.role);
                return RedirectToAction("customer_home", "usersaccounts");
            }
            conn.Close(); 
            return View();
        }

        public IActionResult email()
        {
            var v = HttpContext.Session.GetString("Role");

            if (v == "admin")
            {  
                 return View();
            }
            else
            {
                return RedirectToAction("login", "usersaccounts");
            }

        }
        [HttpPost, ActionName("email")]
        [ValidateAntiForgeryToken]
        public IActionResult email(string address, string subject, string body)
        {
            HttpContext.Session.LoadAsync();
            string role = HttpContext.Session.GetString("Role");
            if (role != "admin")
            {
                return RedirectToAction("login", "usersaccounts");
            }

            SmtpClient SmtpServer = new SmtpClient(" smtp.gmail.com ");
            var mail = new MailMessage();
            mail.From = new MailAddress("asaih1899@gmail.com");
            mail.To.Add(address); // receiver email address
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = body;
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("asiah1899@gmail.com",
           "zifg stqs uukh jgen");
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
            ViewData["Message"] = "Email sent.";
            return View();


        }

        public IActionResult addadmin()
        {
            var v = HttpContext.Session.GetString("Role");

            if (v == "admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("login", "usersaccounts");
            }
        }

        [HttpPost]
        public IActionResult addadmin(string na, int nu)
        {
            SqlConnection conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\PC\\Documents\\asiah.mdf;Integrated Security=True;Connect Timeout=30");

            conn.Open(); string sql = "select * from usersaccounts  where name = '" + na + "' and pass = '" + nu + "'";
            SqlCommand comm = new SqlCommand(sql, conn); SqlDataReader reader = comm.ExecuteReader();
            if (reader.Read())
            {
                ViewData["message"] = "name and password already exists";
                return View();
                reader.Close();
            }
            else
            {
                reader.Close(); sql = "INSERT INTO usersaccounts (name, pass, role) VALUES ('" + na + "', '" + nu + "', 'admin')";
                comm = new SqlCommand(sql, conn);
                comm.ExecuteNonQuery(); 
                ViewData["message"] = "Sucessfully added";
            }
            conn.Close();
            return RedirectToAction("Index","items");

        }


        public async Task<IActionResult> customer_search()
        {
            var v = HttpContext.Session.GetString("Role");
            if (v == "admin")
            {
                usersaccounts brItem = new usersaccounts();
                return View(brItem);
            }
            else
                return RedirectToAction("login", "usersaccounts");

        }
        [HttpPost]
        public async Task<IActionResult> customer_search(string tit)
        {

            var v = HttpContext.Session.GetString("Role");
            if (v == "admin")
            {
                var bkItems = await _context.usersaccounts.FromSqlRaw("select * from usersaccounts where name = '" + tit + "' ").FirstOrDefaultAsync();
                return View(bkItems);
            }
            else
                return RedirectToAction("login", "usersaccounts");
        }

         public async Task<IActionResult> customer_home()
        {
            var v = HttpContext.Session.GetString("Role");

            if (v == "customer")
            {
                HttpContext.Session.LoadAsync();
                ViewData["name"] = HttpContext.Session.GetString("Name");
                return View(await _context.items.ToListAsync());
            }
            else
            {
                return RedirectToAction("login", "usersaccounts");
            }

        }




        public async Task<IActionResult> admin_home()
        {
            HttpContext.Session.LoadAsync();

            String v = HttpContext.Session.GetString("Role");

            if (v == "admin")
            {
                ViewData["name"] = HttpContext.Session.GetString("Name");
                return View();
            }
            else
            {
                return RedirectToAction("login", "usersaccounts");
            }


        }

        // GET: usersaccounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.usersaccounts.ToListAsync());
        }

        // GET: usersaccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usersaccounts = await _context.usersaccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usersaccounts == null)
            {
                return NotFound();
            }

            return View(usersaccounts);
        }

        // GET: usersaccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: usersaccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,pass,role")] usersaccounts usersaccounts)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usersaccounts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usersaccounts);
        }

        // GET: usersaccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usersaccounts = await _context.usersaccounts.FindAsync(id);
            if (usersaccounts == null)
            {
                return NotFound();
            }
            return View(usersaccounts);
        }

        // POST: usersaccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,pass,role")] usersaccounts usersaccounts)
        {
            if (id != usersaccounts.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usersaccounts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!usersaccountsExists(usersaccounts.Id))
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
            return View(usersaccounts);
        }

        // GET: usersaccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usersaccounts = await _context.usersaccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usersaccounts == null)
            {
                return NotFound();
            }

            return View(usersaccounts);
        }

        // POST: usersaccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usersaccounts = await _context.usersaccounts.FindAsync(id);
            if (usersaccounts != null)
            {
                _context.usersaccounts.Remove(usersaccounts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool usersaccountsExists(int id)
        {
            return _context.usersaccounts.Any(e => e.Id == id);
        }
    }
}
