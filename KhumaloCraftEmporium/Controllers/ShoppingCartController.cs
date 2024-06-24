using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KhumaloCraftEmporium.Data;
using KhumaloCraftEmporium.Models;

namespace KhumaloCraftEmporium.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly KhumaloCraftDbContext _context;

        public ShoppingCartController(KhumaloCraftDbContext context)
        {
            _context = context;
        }

        // ShoppingCart
        public async Task<IActionResult> Index()
        {
            var shoppingCartItems = await GetShoppingCartItemsAsync();
            return View(shoppingCartItems);
        }

        // Helper method to retrieve shopping cart items
        private async Task<List<OrderItem>> GetShoppingCartItemsAsync()
        {
            var shoppingCartItems = await _context.OrderItems
                .Where(oi => oi.Order.CustomerID == 1)
                .Include(oi => oi.Product)
                .ToListAsync();

            return shoppingCartItems;
        }

        // Add Function
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync();
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.CustomerID == customer.ID && o.OrderDate == null);

            if (order == null)
            {
                order = new Order
                {
                    CustomerID = customer.ID,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                _context.Orders.Add(order);
            }

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductID == id);

            if (orderItem != null)
            {
                orderItem.Quantity++;
            }
            else
            {
                orderItem = new OrderItem
                {
                    ProductID = id,
                    Quantity = 1,
                    Price = product.Price
                };

                order.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Remove Function
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            return View(new Customer());
        }

        // POST: Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customer.Email && c.Phone == customer.Phone);

            if (existingCustomer == null)
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                existingCustomer = customer;
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.CustomerID == existingCustomer.ID && o.OrderDate == null);

            if (order == null)
            {
                order = new Order
                {
                    CustomerID = existingCustomer.ID,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = await GetShoppingCartItemsAsync() // Get items from cart to attach to order
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }

            TempData["OrderPlacedMessage"] = "Order has been placed successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}

