using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers;

public class DirectoryController : Controller
{
    private readonly IUnitOfWork _uow;

    public DirectoryController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index()
    {
        var repo = _uow.Repository<ContactDirectory>();
        var contacts = await repo.FindAsync(c => c.IsActive);
        return View(contacts.OrderBy(c => c.DisplayOrder).ToList());
    }

    public async Task<IActionResult> ServiceStatus()
    {
        var repo = _uow.Repository<ServiceStatus>();
        var statuses = await repo.GetAllAsync();
        return View(statuses);
    }
}
