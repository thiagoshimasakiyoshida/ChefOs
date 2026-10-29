using CozinhaApp.Models;
using CozinhaApp.Strategies;

namespace CozinhaApp.Services;

/// <summary>Contexto do Strategy 1: escolhe a estratégia de conversão pela unidade.</summary>
public class ConversorUnidades
{
    private readonly Dictionary<UnidadeMedida, IConversaoUnidadeStrategy> _estrategias;

    public ConversorUnidades(IEnumerable<IConversaoUnidadeStrategy> estrategias) =>
        _estrategias = estrategias.ToDictionary(e => e.Unidade);

    public decimal ParaBase(decimal valor, UnidadeMedida unidade) => _estrategias[unidade].ParaBase(valor);
    public TipoMedida TipoDe(UnidadeMedida unidade) => _estrategias[unidade].Tipo;
}
