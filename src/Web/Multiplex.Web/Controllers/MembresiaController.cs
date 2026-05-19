using Microsoft.AspNetCore.Mvc;
using Multiplex.Web.Auth;

namespace Multiplex.Web.Controllers;

public sealed class MembresiaController : Controller
{
    public IActionResult Index()
    {
        ViewData["NavActive"] = "membresia";
        var nivel = User?.GetNivel() ?? "Normal";
        return View(nivel);
    }
}
