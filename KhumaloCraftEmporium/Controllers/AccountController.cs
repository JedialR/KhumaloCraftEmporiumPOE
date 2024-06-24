using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KhumaloCraftEmporium.Data;
using KhumaloCraftEmporium.Models;

namespace KhumaloCraftEmporium.Controllers
{
    public class AccountController : Controller
    {
        private readonly KhumaloCraftDbContext _context;

        public AccountController(KhumaloCraftDbContext context)
        {
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new Customer());
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            // Check if the customer exists
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customer.Email && c.Phone == customer.Phone);

            if (existingCustomer == null)
            {
                ModelState.AddModelError("", "Customer not found.");
                return View(customer);
            }

            // Redirect to Orders page with the customer ID
            return RedirectToAction("Orders", new { customerId = existingCustomer.ID });
        }

        // GET: Orders
        [HttpGet]
        public async Task<IActionResult> Orders(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerID == customerId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.Customer = customer;
            return View(orders);
        }
    }
}
