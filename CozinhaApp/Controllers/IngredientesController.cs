using CozinhaApp.Data;
using CozinhaApp.Services;
using CozinhaApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CozinhaApp.Controllers;

public class IngredientesController : Controller
{
    private readonly CozinhaContext _db;
    private readonly EstoqueService _estoque;

    public IngredientesController(CozinhaContext db, EstoqueService estoque)
    {
        _db = db; _estoque = estoque;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _db.Ingredientes
            .Include(i => i.Lotes.OrderBy(l => l.Validade))
            .OrderBy(i => i.Nome)
            .ToListAsync();
        return View(lista);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarNomesAsync();
        return View(new EstoqueFormViewModel { Validade = DateTime.Today.AddDays(7) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EstoqueFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var r = await _estoque.AdicionarEstoqueAsync(vm.Nome, vm.Quantidade!.Value, vm.Unidade, vm.Validade!.Value);
            if (r.Ok)
            {
                TempData["Ok"] = $"{vm.Nome.Trim()} adicionado ao estoque.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", r.Erro!);
        }
        await CarregarNomesAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Bloquear(int loteId, string? motivo)
    {
        if (await _estoque.BloquearAsync(loteId, motivo)) TempData["Ok"] = "Lote bloqueado. Ele não será usado em nenhum preparo.";
        else TempData["Erro"] = "Lote não encontrado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Desbloquear(int loteId)
    {
        if (await _estoque.DesbloquearAsync(loteId)) TempData["Ok"] = "Lote liberado para uso.";
        else TempData["Erro"] = "Lote não encontrado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarNomesAsync() =>
        ViewBag.Nomes = await _db.Ingredientes.OrderBy(i => i.Nome).Select(i => i.Nome).ToListAsync();
}
