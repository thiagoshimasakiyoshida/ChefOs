using CozinhaApp.Models;

namespace CozinhaApp.Strategies;

/// <summary>STRATEGY 1 – como converter cada unidade informada pelo usuário para a unidade base (g ou ml).</summary>
public interface IConversaoUnidadeStrategy
{
    UnidadeMedida Unidade { get; }
    TipoMedida Tipo { get; }
    decimal ParaBase(decimal valor);
}

public abstract class ConversaoPorFator : IConversaoUnidadeStrategy
{
    private readonly decimal _fator;
    protected ConversaoPorFator(UnidadeMedida unidade, TipoMedida tipo, decimal fator)
    {
        Unidade = unidade; Tipo = tipo; _fator = fator;
    }
    public UnidadeMedida Unidade { get; }
    public TipoMedida Tipo { get; }
    public decimal ParaBase(decimal valor) => valor * _fator;
}

public class GramaStrategy : ConversaoPorFator
{ public GramaStrategy() : base(UnidadeMedida.Grama, TipoMedida.Massa, 1m) { } }

public class QuilogramaStrategy : ConversaoPorFator
{ public QuilogramaStrategy() : base(UnidadeMedida.Quilograma, TipoMedida.Massa, 1000m) { } }

public class MililitroStrategy : ConversaoPorFator
{ public MililitroStrategy() : base(UnidadeMedida.Mililitro, TipoMedida.Volume, 1m) { } }

public class LitroStrategy : ConversaoPorFator
{ public LitroStrategy() : base(UnidadeMedida.Litro, TipoMedida.Volume, 1000m) { } }
