using CozinhaApp.Data;
using CozinhaApp.Models;
using CozinhaApp.Strategies;
using Microsoft.EntityFrameworkCore;

namespace CozinhaApp.Services;

public class PreparoService
{
    private readonly CozinhaContext _db;
    private readonly IConsumoEstoqueStrategy _consumo;
    private readonly IEnumerable<ICancelamentoStrategy> _cancelamentos;

    public PreparoService(CozinhaContext db, IConsumoEstoqueStrategy consumo,
                          IEnumerable<ICancelamentoStrategy> cancelamentos)
    {
        _db = db; _consumo = consumo; _cancelamentos = cancelamentos;
    }

    public Task<Preparo?> CarregarAsync(int id) =>
        _db.Preparos
           .Include(p => p.Consumos)
           .Include(p => p.Receita).ThenInclude(r => r.Etapas).ThenInclude(e => e.Ingredientes).ThenInclude(i => i.Ingrediente)
           .AsSplitQuery()
           .FirstOrDefaultAsync(p => p.Id == id);

    public string DescricaoCancelamento(Preparo p) => EscolherCancelamento(p).Descricao(p);

    private ICancelamentoStrategy EscolherCancelamento(Preparo p) => _cancelamentos.First(s => s.Aplica(p));

    /// <summary>Reserva (baixa do estoque) todos os ingredientes da receita e cria o preparo.</summary>
    public async Task<(bool Ok, string? Erro, int Id)> IniciarAsync(int receitaId)
    {
        var receita = await _db.Receitas
            .Include(r => r.Etapas).ThenInclude(e => e.Ingredientes).ThenInclude(i => i.Ingrediente)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == receitaId);
        if (receita == null) return (false, "Receita não encontrada.", 0);

        var necessidade = receita.NecessidadePorIngrediente();
        if (necessidade.Count == 0) return (false, "A receita não tem ingredientes.", 0);

        var hoje = DateTime.Today;
        var ids = necessidade.Keys.ToList();
        var lotes = (await _db.Lotes
                .Where(l => ids.Contains(l.IngredienteId) && !l.Bloqueado && l.Validade >= hoje)
                .ToListAsync())
            .Where(l => l.QuantidadeAtual > 0).ToList();

        var faltas = new List<string>();
        foreach (var (ingId, precisa) in necessidade)
        {
            var disponivel = lotes.Where(l => l.IngredienteId == ingId).Sum(l => l.QuantidadeAtual);
            if (disponivel < precisa)
            {
                var ing = receita.Etapas.SelectMany(e => e.Ingredientes).First(i => i.IngredienteId == ingId).Ingrediente;
                faltas.Add($"{ing.Nome} (precisa {Formato.Quantidade(precisa, ing.Medida)}, tem {Formato.Quantidade(disponivel, ing.Medida)})");
            }
        }
        if (faltas.Count > 0)
            return (false, "Estoque insuficiente: " + string.Join("; ", faltas) + ".", 0);

        var preparo = new Preparo { Receita = receita, Inicio = DateTime.Now, Status = StatusPreparo.EmAndamento, EtapaAtual = 0 };

        foreach (var etapa in receita.Etapas.OrderBy(e => e.Ordem))
        foreach (var item in etapa.Ingredientes)
        {
            var restante = item.Quantidade;
            var candidatos = _consumo.Ordenar(lotes.Where(l => l.IngredienteId == item.IngredienteId && l.QuantidadeAtual > 0)).ToList();
            foreach (var lote in candidatos)
            {
                if (restante <= 0) break;
                var usar = Math.Min(lote.QuantidadeAtual, restante);
                lote.QuantidadeAtual -= usar;
                restante -= usar;
                preparo.Consumos.Add(new PreparoConsumo
                {
                    Lote = lote,
                    IngredienteId = item.IngredienteId,
                    EtapaOrdem = etapa.Ordem,
                    Quantidade = usar
                });
            }
        }

        _db.Preparos.Add(preparo);
        await _db.SaveChangesAsync();
        return (true, null, preparo.Id);
    }

    public async Task<(bool Ok, string? Erro)> AvancarAsync(int id)
    {
        var p = await CarregarAsync(id);
        if (p == null || p.Status != StatusPreparo.EmAndamento) return (false, "Preparo não está em andamento.");
        if (p.EtapaAtual >= p.Receita.Etapas.Count) return (false, "Todas as etapas já foram iniciadas. Finalize o prato.");
        p.EtapaAtual++;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Erro)> FinalizarAsync(int id)
    {
        var p = await CarregarAsync(id);
        if (p == null || p.Status != StatusPreparo.EmAndamento) return (false, "Preparo não está em andamento.");
        if (p.EtapaAtual < p.Receita.Etapas.Count) return (false, "Ainda há etapas a cumprir.");
        p.Status = StatusPreparo.Finalizado;
        p.Fim = DateTime.Now;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string Mensagem)> CancelarAsync(int id)
    {
        var p = await CarregarAsync(id);
        if (p == null || p.Status != StatusPreparo.EmAndamento)
            return (false, "Só é possível cancelar preparos em andamento.");

        var estrategia = EscolherCancelamento(p);
        var devolver = estrategia.ConsumosADevolver(p).ToList();

        foreach (var consumo in devolver)
        {
            var lote = await _db.Lotes.FindAsync(consumo.LoteId);
            if (lote != null) lote.QuantidadeAtual += consumo.Quantidade;
            consumo.Devolvido = true;
        }

        p.Status = StatusPreparo.Cancelado;
        p.Fim = DateTime.Now;
        await _db.SaveChangesAsync();

        var qtd = devolver.Select(c => c.IngredienteId).Distinct().Count();
        return (true, qtd == 0
            ? "Preparo cancelado. Nenhum ingrediente voltou ao estoque (todos já tinham sido usados)."
            : $"Preparo cancelado. {qtd} ingrediente(s) não utilizado(s) voltaram ao estoque.");
    }
}
