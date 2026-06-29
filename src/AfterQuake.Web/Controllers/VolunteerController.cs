using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers;

[Authorize]
public class VolunteerController : Controller
{
    private readonly IUnitOfWork _uow;

    public VolunteerController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index()
    {
        var repo = _uow.Repository<Volunteer>();
        var volunteers = await repo.FindAsync(v => v.IsAvailable);
        return View(volunteers);
    }

    public async Task<IActionResult> Register()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var repo = _uow.Repository<Volunteer>();
        var existing = await repo.FirstOrDefaultAsync(v => v.UserId == userId);
        if (existing is not null)
            return RedirectToAction(nameof(MyTasks));
        return View(new RegisterVolunteerDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVolunteerDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var volunteer = new Volunteer
        {
            UserId = userId,
            Skills = dto.Skills,
            Certifications = dto.Certifications,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ZoneCode = dto.ZoneCode,
            MaxHoursPerDay = dto.MaxHoursPerDay,
            Notes = dto.Notes
        };
        var repo = _uow.Repository<Volunteer>();
        await repo.AddAsync(volunteer);
        await _uow.SaveChangesAsync();
        TempData["SuccessMessage"] = "Te has registrado como voluntario. Te notificaremos cuando te necesiten.";
        return RedirectToAction(nameof(MyTasks));
    }

    public async Task<IActionResult> MyTasks()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var repo = _uow.Repository<Volunteer>();
        var volunteer = await repo.Query().Include(v => v.AssignedTasks).FirstOrDefaultAsync(v => v.UserId == userId);
        return View(volunteer?.AssignedTasks.ToList() ?? new List<VolunteerTask>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(Guid taskId)
    {
        var repo = _uow.Repository<VolunteerTask>();
        var task = await repo.GetByIdAsync(taskId);
        if (task is not null)
        {
            task.StartedAt = DateTime.UtcNow;
            await repo.UpdateAsync(task);
            await _uow.SaveChangesAsync();
        }
        return RedirectToAction(nameof(MyTasks));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(Guid taskId)
    {
        var repo = _uow.Repository<VolunteerTask>();
        var task = await repo.GetByIdAsync(taskId);
        if (task is not null)
        {
            task.EndedAt = DateTime.UtcNow;
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
            await repo.UpdateAsync(task);
            await _uow.SaveChangesAsync();
        }
        return RedirectToAction(nameof(MyTasks));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var repo = _uow.Repository<Volunteer>();
        var volunteer = await repo.FirstOrDefaultAsync(v => v.UserId == userId);
        if (volunteer is not null)
        {
            volunteer.IsAvailable = !volunteer.IsAvailable;
            volunteer.Status = volunteer.IsAvailable ? VolunteerStatus.Available : VolunteerStatus.Unavailable;
            await repo.UpdateAsync(volunteer);
            await _uow.SaveChangesAsync();
        }
        return RedirectToAction(nameof(MyTasks));
    }
}
