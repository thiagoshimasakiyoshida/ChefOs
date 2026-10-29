using CozinhaApp.Data;
using CozinhaApp.Models;
using CozinhaApp.Services;
using CozinhaApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CozinhaApp.Controllers;

public class ReceitasController : Controller
{
    private readonly CozinhaContext _db;
    private readonly EstoqueService _estoque;
    private readonly PreparoService _preparos;
    private readonly ConversorUnidades _conversor;

    public ReceitasController(CozinhaContext db, EstoqueService estoque, PreparoService preparos, ConversorUnidades conversor)
    {
        _db = db; _estoque = estoque; _preparos = preparos; _conversor = conversor;
    }

    private IQueryable<Receita> ReceitasCompletas() =>
        _db.Receitas
           .Include(r => r.Etapas).ThenInclude(e => e.Ingredientes).ThenInclude(i => i.Ingrediente)
           .AsSplitQuery();

    public async Task<IActionResult> Index()
    {
        var receitas = await ReceitasCompletas().OrderBy(r => r.Nome).ToListAsync();
        var estoque = await _estoque.EstoqueUtilizavelAsync();
        var model = receitas.Select(r => new ReceitaResumo
        {
            Receita = r,
            Porcoes = EstoqueService.CalcularPorcoes(r, estoque)
        }).ToList();
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new ReceitaFormViewModel();
        await PrepararFormAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReceitaFormViewModel vm)
    {
        var ingredientes = await _db.Ingredientes.ToDictionaryAsync(i => i.Id);

        if (vm.Etapas.Count == 0)
            ModelState.AddModelError("", "Adicione ao menos uma etapa.");

        foreach (var etapa in vm.Etapas)
        foreach (var item in etapa.Ingredientes)
        {
            if (!ingredientes.TryGetValue(item.IngredienteId, out var ing))
                ModelState.AddModelError("", "Selecione um ingrediente válido em todas as linhas.");
            else if (item.Quantidade <= 0)
                ModelState.AddModelError("", $"Informe a quantidade de {ing.Nome}.");
            else if (_conversor.TipoDe(item.Unidade) != ing.Medida)
                ModelState.AddModelError("", $"\"{ing.Nome}\" deve ser medido em {(ing.Medida == TipoMedida.Massa ? "g ou kg" : "ml ou L")}.");
        }

        if (!vm.Etapas.SelectMany(e => e.Ingredientes).Any())
            ModelState.AddModelError("", "A receita precisa de ao menos um ingrediente.");

        if (!ModelState.IsValid)
        {
            await PrepararFormAsync();
            return View(vm);
        }

        var receita = new Receita { Nome = vm.Nome.Trim(), Descricao = vm.Descricao?.Trim() };
        var ordem = 1;
        foreach (var e in vm.Etapas)
        {
            var etapa = new EtapaReceita { Ordem = ordem++, Descricao = e.Descricao.Trim() };
            foreach (var i in e.Ingredientes)
                etapa.Ingredientes.Add(new EtapaIngrediente
                {
                    IngredienteId = i.IngredienteId,
                    Quantidade = _conversor.ParaBase(i.Quantidade, i.Unidade)
                });
            receita.Etapas.Add(etapa);
        }
        _db.Receitas.Add(receita);
        await _db.SaveChangesAsync();

        TempData["Ok"] = $"Receita \"{receita.Nome}\" cadastrada.";
        return RedirectToAction(nameof(Index));
    }

    // Passo a passo antes de iniciar o preparo
    public async Task<IActionResult> Preparar(int id)
    {
        var receita = await ReceitasCompletas().FirstOrDefaultAsync(r => r.Id == id);
        if (receita == null) return NotFound();
        var estoque = await _estoque.EstoqueUtilizavelAsync();
        ViewBag.Estoque = estoque;
        return View(new ReceitaResumo { Receita = receita, Porcoes = EstoqueService.CalcularPorcoes(receita, estoque) });
    }

    [HttpPost, ValidateAntiForgeryToken, ActionName("Preparar")]
    public async Task<IActionResult> PrepararPost(int id)
    {
        var r = await _preparos.IniciarAsync(id);
        if (!r.Ok)
        {
            TempData["Erro"] = r.Erro;
            return RedirectToAction(nameof(Preparar), new { id });
        }
        TempData["Ok"] = "Prato em produção.";
        return RedirectToAction("Index", "Home");
    }

    private async Task PrepararFormAsync()
    {
        ViewBag.Ingredientes = await _db.Ingredientes.OrderBy(i => i.Nome)
            .Select(i => new { id = i.Id, nome = i.Nome, tipo = i.Medida == TipoMedida.Massa ? "massa" : "volume" })
            .ToListAsync();
    }
}
