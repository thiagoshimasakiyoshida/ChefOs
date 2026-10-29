using CozinhaApp.Models;

namespace CozinhaApp.Strategies;

/// <summary>STRATEGY 2 – em que ordem os lotes são consumidos.</summary>
public interface IConsumoEstoqueStrategy
{
    IEnumerable<LoteIngrediente> Ordenar(IEnumerable<LoteIngrediente> lotesUtilizaveis);
}

/// <summary>FEFO: o que vence primeiro sai primeiro.</summary>
public class FefoStrategy : IConsumoEstoqueStrategy
{
    public IEnumerable<LoteIngrediente> Ordenar(IEnumerable<LoteIngrediente> lotes) =>
        lotes.OrderBy(l => l.Validade).ThenBy(l => l.DataEntrada);
}
