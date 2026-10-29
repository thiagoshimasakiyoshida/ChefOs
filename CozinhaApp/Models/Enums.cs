namespace CozinhaApp.Models;

public enum TipoMedida { Massa = 1, Volume = 2 }

public enum UnidadeMedida
{
    Grama = 1,
    Quilograma = 2,
    Mililitro = 3,
    Litro = 4
}

public enum StatusPreparo { EmAndamento = 1, Finalizado = 2, Cancelado = 3 }

public enum PeriodoRanking { Todo = 0, Ano = 1, Mes = 2, Semana = 3 }
