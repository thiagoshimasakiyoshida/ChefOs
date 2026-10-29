using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CozinhaApp.Models;

/// <summary>Ingrediente do catálogo. Os lotes (quantidade + validade) ficam em LoteIngrediente.</summary>
public class Ingrediente
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = "";
    /// <summary>Massa (guardada em gramas) ou Volume (guardado em mililitros).</summary>
    public TipoMedida Medida { get; set; }
    public List<LoteIngrediente> Lotes { get; set; } = new();

    [NotMapped]
    public decimal EstoqueUtilizavel => Lotes.Where(l => l.Utilizavel).Sum(l => l.QuantidadeAtual);
}

public class LoteIngrediente
{
    public int Id { get; set; }
    public int IngredienteId { get; set; }
    public Ingrediente Ingrediente { get; set; } = null!;
    /// <summary>Sempre na unidade base (g ou ml).</summary>
    public decimal QuantidadeInicial { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public DateTime Validade { get; set; }
    public DateTime DataEntrada { get; set; }
    public bool Bloqueado { get; set; }
    [StringLength(200)] public string? MotivoBloqueio { get; set; }

    [NotMapped] public bool Vencido => Validade.Date < DateTime.Today;
    [NotMapped] public bool Utilizavel => !Bloqueado && !Vencido && QuantidadeAtual > 0;
}

public class Receita
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Nome { get; set; } = "";
    [StringLength(500)] public string? Descricao { get; set; }
    public List<EtapaReceita> Etapas { get; set; } = new();

    /// <summary>Total necessário de cada ingrediente (unidade base), somando todas as etapas.</summary>
    public Dictionary<int, decimal> NecessidadePorIngrediente() =>
        Etapas.SelectMany(e => e.Ingredientes)
              .GroupBy(i => i.IngredienteId)
              .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));
}

public class EtapaReceita
{
    public int Id { get; set; }
    public int ReceitaId { get; set; }
    public Receita Receita { get; set; } = null!;
    public int Ordem { get; set; }
    [Required] public string Descricao { get; set; } = "";
    public List<EtapaIngrediente> Ingredientes { get; set; } = new();
}

public class EtapaIngrediente
{
    public int Id { get; set; }
    public int EtapaReceitaId { get; set; }
    public EtapaReceita EtapaReceita { get; set; } = null!;
    public int IngredienteId { get; set; }
    public Ingrediente Ingrediente { get; set; } = null!;
    /// <summary>Unidade base (g ou ml).</summary>
    public decimal Quantidade { get; set; }
}

public class Preparo
{
    public int Id { get; set; }
    public int ReceitaId { get; set; }
    public Receita Receita { get; set; } = null!;
    public DateTime Inicio { get; set; }
    public DateTime? Fim { get; set; }
    public StatusPreparo Status { get; set; } = StatusPreparo.EmAndamento;
    /// <summary>Quantas etapas já foram iniciadas. 0 = nada foi usado ainda.</summary>
    public int EtapaAtual { get; set; }
    public List<PreparoConsumo> Consumos { get; set; } = new();
}

/// <summary>Registro de quanto de cada lote foi reservado para um preparo (permite devolver ao cancelar).</summary>
public class PreparoConsumo
{
    public int Id { get; set; }
    public int PreparoId { get; set; }
    public Preparo Preparo { get; set; } = null!;
    public int LoteId { get; set; }
    public LoteIngrediente Lote { get; set; } = null!;
    public int IngredienteId { get; set; }
    public Ingrediente Ingrediente { get; set; } = null!;
    /// <summary>Ordem da etapa que usa este ingrediente.</summary>
    public int EtapaOrdem { get; set; }
    public decimal Quantidade { get; set; }
    public bool Devolvido { get; set; }
}
