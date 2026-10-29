using CozinhaApp.Models;

namespace CozinhaApp.Strategies;

/// <summary>STRATEGY 4 – como calcular o início do período usado no filtro de "mais preparados".</summary>
public interface IPeriodoStrategy
{
    PeriodoRanking Periodo { get; }
    string Rotulo { get; }
    DateTime? Inicio(DateTime agora);
}

public class PeriodoTodoStrategy : IPeriodoStrategy
{
    public PeriodoRanking Periodo => PeriodoRanking.Todo;
    public string Rotulo => "Todo o período";
    public DateTime? Inicio(DateTime agora) => null;
}

public class PeriodoAnoStrategy : IPeriodoStrategy
{
    public PeriodoRanking Periodo => PeriodoRanking.Ano;
    public string Rotulo => "Este ano";
    public DateTime? Inicio(DateTime agora) => new DateTime(agora.Year, 1, 1);
}

public class PeriodoMesStrategy : IPeriodoStrategy
{
    public PeriodoRanking Periodo => PeriodoRanking.Mes;
    public string Rotulo => "Este mês";
    public DateTime? Inicio(DateTime agora) => new DateTime(agora.Year, agora.Month, 1);
}

/// <summary>Semana começando na segunda-feira.</summary>
public class PeriodoSemanaStrategy : IPeriodoStrategy
{
    public PeriodoRanking Periodo => PeriodoRanking.Semana;
    public string Rotulo => "Esta semana";
    public DateTime? Inicio(DateTime agora)
    {
        var dias = ((int)agora.DayOfWeek + 6) % 7;
        return agora.Date.AddDays(-dias);
    }
}
