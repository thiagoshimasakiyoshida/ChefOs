using CozinhaApp.Models;

namespace CozinhaApp.Strategies;

/// <summary>STRATEGY 3 – o que volta ao estoque quando o preparo é cancelado, conforme o estágio.</summary>
public interface ICancelamentoStrategy
{
    bool Aplica(Preparo preparo);
    string Descricao(Preparo preparo);
    IEnumerable<PreparoConsumo> ConsumosADevolver(Preparo preparo);
}

/// <summary>Preparo ainda não começou (etapa 0): tudo volta.</summary>
public class CancelamentoTotalStrategy : ICancelamentoStrategy
{
    public bool Aplica(Preparo p) => p.EtapaAtual == 0;
    public string Descricao(Preparo p) => "Nenhuma etapa foi iniciada: todos os ingredientes voltam ao estoque.";
    public IEnumerable<PreparoConsumo> ConsumosADevolver(Preparo p) => p.Consumos.Where(c => !c.Devolvido);
}

/// <summary>Preparo em andamento: ingredientes das etapas já iniciadas são considerados usados; os demais voltam.</summary>
public class CancelamentoPorEtapaStrategy : ICancelamentoStrategy
{
    public bool Aplica(Preparo p) => p.EtapaAtual > 0;

    public string Descricao(Preparo p) =>
        $"Etapas 1 a {p.EtapaAtual} já foram iniciadas e seus ingredientes não voltam. " +
        "Os ingredientes das etapas seguintes voltam ao estoque.";

    public IEnumerable<PreparoConsumo> ConsumosADevolver(Preparo p) =>
        p.Consumos.Where(c => !c.Devolvido && c.EtapaOrdem > p.EtapaAtual);
}
