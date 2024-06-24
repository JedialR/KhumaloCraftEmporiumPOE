using Microsoft.AspNetCore.Mvc;
using KhumaloCraftEmporium.Data;

public class ContactController : Controller
{
    private readonly KhumaloCraftDbContext _context;

    public ContactController(KhumaloCraftDbContext context)
    {
        _context = context;
    }

    public IActionResult Index() => View();
}
