using CozinhaApp.Models;
using CozinhaApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CozinhaApp.Controllers;

public class RankingController : Controller
{
    private readonly RankingService _ranking;
    public RankingController(RankingService ranking) => _ranking = ranking;

    public async Task<IActionResult> Index(PeriodoRanking periodo = PeriodoRanking.Todo)
    {
        var r = await _ranking.ObterAsync(periodo);
        ViewBag.Periodos = _ranking.Periodos.ToList();
        ViewBag.PeriodoAtual = r.Periodo;
        ViewBag.Inicio = r.Inicio;
        return View(r.Itens);
    }
}
