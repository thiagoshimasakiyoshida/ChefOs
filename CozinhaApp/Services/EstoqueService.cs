using CozinhaApp.Data;
using CozinhaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CozinhaApp.Services;

public class EstoqueService
{
    private readonly CozinhaContext _db;
    private readonly ConversorUnidades _conversor;

    public EstoqueService(CozinhaContext db, ConversorUnidades conversor)
    {
        _db = db; _conversor = conversor;
    }

    public async Task<(bool Ok, string? Erro)> AdicionarEstoqueAsync(
        string nome, decimal quantidade, UnidadeMedida unidade, DateTime validade)
    {
        nome = nome.Trim();
        if (quantidade <= 0) return (false, "A quantidade deve ser maior que zero.");

        var tipo = _conversor.TipoDe(unidade);
        var ingrediente = await _db.Ingredientes.FirstOrDefaultAsync(i => i.Nome.ToLower() == nome.ToLower());

        if (ingrediente == null)
        {
            ingrediente = new Ingrediente { Nome = nome, Medida = tipo };
            _db.Ingredientes.Add(ingrediente);
        }
        else if (ingrediente.Medida != tipo)
        {
            var esperado = ingrediente.Medida == TipoMedida.Massa ? "gramas ou quilos" : "mililitros ou litros";
            return (false, $"\"{ingrediente.Nome}\" já está cadastrado em {esperado}. Use uma dessas unidades.");
        }

        var qtd = _conversor.ParaBase(quantidade, unidade);
        _db.Lotes.Add(new LoteIngrediente
        {
            Ingrediente = ingrediente,
            QuantidadeInicial = qtd,
            QuantidadeAtual = qtd,
            Validade = validade.Date,
            DataEntrada = DateTime.Now
        });
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> BloquearAsync(int loteId, string? motivo)
    {
        var lote = await _db.Lotes.FindAsync(loteId);
        if (lote == null) return false;
        lote.Bloqueado = true;
        lote.MotivoBloqueio = string.IsNullOrWhiteSpace(motivo) ? "Sem motivo informado" : motivo.Trim();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesbloquearAsync(int loteId)
    {
        var lote = await _db.Lotes.FindAsync(loteId);
        if (lote == null) return false;
        lote.Bloqueado = false;
        lote.MotivoBloqueio = null;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Estoque utilizável (não bloqueado, não vencido) por ingrediente, em unidade base.</summary>
    public async Task<Dictionary<int, decimal>> EstoqueUtilizavelAsync()
    {
        var hoje = DateTime.Today;
        var lotes = await _db.Lotes.Where(l => !l.Bloqueado && l.Validade >= hoje).ToListAsync();
        return lotes.Where(l => l.QuantidadeAtual > 0)
                    .GroupBy(l => l.IngredienteId)
                    .ToDictionary(g => g.Key, g => g.Sum(l => l.QuantidadeAtual));
    }

    /// <summary>Quantas unidades da receita dá para fazer com o estoque atual.</summary>
    public static int CalcularPorcoes(Receita receita, Dictionary<int, decimal> estoque)
    {
        var necessidade = receita.NecessidadePorIngrediente();
        if (necessidade.Count == 0) return 0;

        var min = int.MaxValue;
        foreach (var (ingId, precisa) in necessidade)
        {
            if (precisa <= 0) continue;
            estoque.TryGetValue(ingId, out var disponivel);
            min = Math.Min(min, (int)Math.Floor(disponivel / precisa));
        }
        return min == int.MaxValue ? 0 : min;
    }
}
