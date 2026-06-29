using Microsoft.AspNetCore.Mvc;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers;

public class GuideController : Controller
{
    private readonly IUnitOfWork _uow;

    public GuideController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index(string? category)
    {
        var repo = _uow.Repository<GuideContent>();
        var guides = string.IsNullOrEmpty(category)
            ? await repo.FindAsync(g => g.IsPublished)
            : await repo.FindAsync(g => g.IsPublished && g.Category == category);
        ViewBag.Categories = (await repo.FindAsync(g => g.IsPublished))
            .Select(g => g.Category).Where(c => c != null).Distinct().ToList();
        return View(guides.OrderBy(g => g.DisplayOrder).ToList());
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var repo = _uow.Repository<GuideContent>();
        var guide = await repo.GetByIdAsync(id);
        if (guide is null) return NotFound();
        return View(guide);
    }
}
