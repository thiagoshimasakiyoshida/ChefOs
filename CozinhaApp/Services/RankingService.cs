using CozinhaApp.Data;
using CozinhaApp.Models;
using CozinhaApp.Strategies;
using Microsoft.EntityFrameworkCore;

namespace CozinhaApp.Services;

public record RankingItem(string Prato, int Total);

public class RankingService
{
    private readonly CozinhaContext _db;
    private readonly Dictionary<PeriodoRanking, IPeriodoStrategy> _periodos;

    public RankingService(CozinhaContext db, IEnumerable<IPeriodoStrategy> periodos)
    {
        _db = db;
        _periodos = periodos.ToDictionary(p => p.Periodo);
    }

    public IEnumerable<IPeriodoStrategy> Periodos => _periodos.Values.OrderBy(p => p.Periodo);

    public async Task<(List<RankingItem> Itens, IPeriodoStrategy Periodo, DateTime? Inicio)> ObterAsync(PeriodoRanking periodo)
    {
        var estrategia = _periodos[periodo];
        var inicio = estrategia.Inicio(DateTime.Now);

        var query = _db.Preparos.Where(p => p.Status == StatusPreparo.Finalizado);
        if (inicio.HasValue) query = query.Where(p => p.Fim >= inicio.Value);

        var dados = await query
            .GroupBy(p => new { p.ReceitaId, p.Receita.Nome })
            .Select(g => new { g.Key.Nome, Total = g.Count() })
            .OrderByDescending(x => x.Total).ThenBy(x => x.Nome)
            .ToListAsync();

        return (dados.Select(d => new RankingItem(d.Nome, d.Total)).ToList(), estrategia, inicio);
    }
}
