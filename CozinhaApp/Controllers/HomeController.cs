using CozinhaApp.Data;
using CozinhaApp.Models;
using CozinhaApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CozinhaApp.Controllers;

public class HomeController : Controller
{
    private readonly CozinhaContext _db;
    private readonly PreparoService _preparos;

    public HomeController(CozinhaContext db, PreparoService preparos)
    {
        _db = db; _preparos = preparos;
    }

    // Tela inicial: pratos em produção
    public async Task<IActionResult> Index()
    {
        var emProducao = await _db.Preparos
            .Include(p => p.Receita).ThenInclude(r => r.Etapas)
            .Where(p => p.Status == StatusPreparo.EmAndamento)
            .OrderBy(p => p.Inicio)
            .ToListAsync();
        return View(emProducao);
    }

    // Detalhes do prato em produção
    public async Task<IActionResult> Preparo(int id)
    {
        var preparo = await _preparos.CarregarAsync(id);
        if (preparo == null) return NotFound();
        ViewBag.InfoCancelamento = _preparos.DescricaoCancelamento(preparo);
        return View(preparo);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Avancar(int id)
    {
        var r = await _preparos.AvancarAsync(id);
        if (!r.Ok) TempData["Erro"] = r.Erro;
        return RedirectToAction(nameof(Preparo), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizar(int id)
    {
        var r = await _preparos.FinalizarAsync(id);
        if (!r.Ok) { TempData["Erro"] = r.Erro; return RedirectToAction(nameof(Preparo), new { id }); }
        TempData["Ok"] = "Prato finalizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var r = await _preparos.CancelarAsync(id);
        TempData[r.Ok ? "Ok" : "Erro"] = r.Mensagem;
        return RedirectToAction(nameof(Index));
    }
}
