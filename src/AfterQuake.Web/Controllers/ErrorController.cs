using Microsoft.AspNetCore.Mvc;
using AfterQuake.Web.Models;

namespace AfterQuake.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/error/{statusCode}")]
    public IActionResult StatusCodeError(int statusCode)
    {
        var model = new ErrorViewModel { StatusCode = statusCode };
        switch (statusCode)
        {
            case 404:
                model.Title = "Página no encontrada";
                model.Message = "La página que buscas no existe o fue movida.";
                break;
            case 403:
                model.Title = "Acceso denegado";
                model.Message = "No tienes permisos para acceder a esta página.";
                break;
            case 500:
                model.Title = "Error interno";
                model.Message = "Ha ocurrido un error inesperado. Intenta nuevamente.";
                break;
        }
        return View("Error", model);
    }
}
