using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers;

public class DonationController : Controller
{
    private readonly IUnitOfWork _uow;

    public DonationController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index()
    {
        var repo = _uow.Repository<Donation>();
        var donations = await repo.FindAsync(d => d.Status != Domain.Enumerations.DonationStatus.Cancelled);
        return View(donations.OrderByDescending(d => d.DonatedAt).ToList());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Create()
    {
        ViewBag.DonationPoints = await _uow.Repository<DonationPoint>().FindAsync(d => d.IsActive);
        return View(new CreateDonationDto());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDonationDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
        var donation = new Donation
        {
            DonationType = dto.DonationType,
            MonetaryAmount = dto.MonetaryAmount,
            ItemName = dto.ItemName,
            ItemQuantity = dto.ItemQuantity,
            ItemUnit = dto.ItemUnit,
            Description = dto.Description,
            DonorName = dto.DonorName,
            IsAnonymous = dto.IsAnonymous,
            DonationPointId = dto.DonationPointId,
            UserId = userId
        };
        var repo = _uow.Repository<Donation>();
        await repo.AddAsync(donation);
        await _uow.SaveChangesAsync();
        TempData["SuccessMessage"] = "Gracias por tu donación. Cada aporte salva vidas.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Transparency()
    {
        var repo = _uow.Repository<Donation>();
        var donations = await repo.FindAsync(d => d.Status != Domain.Enumerations.DonationStatus.Cancelled);
        var totalMonetary = donations.Where(d => d.MonetaryAmount.HasValue).Sum(d => d.MonetaryAmount ?? 0);
        var totalItems = donations.Where(d => d.ItemQuantity.HasValue).Sum(d => d.ItemQuantity ?? 0);
        ViewBag.TotalMonetary = totalMonetary;
        ViewBag.TotalItems = totalItems;
        return View(donations.OrderByDescending(d => d.DonatedAt).ToList());
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDistributed(Guid id)
    {
        var repo = _uow.Repository<Donation>();
        var donation = await repo.GetByIdAsync(id);
        if (donation is not null)
        {
            donation.Status = Domain.Enumerations.DonationStatus.Distributed;
            donation.DistributedAt = DateTime.UtcNow;
            await repo.UpdateAsync(donation);
            await _uow.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Transparency));
    }
}
