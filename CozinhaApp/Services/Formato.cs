using System.Globalization;
using CozinhaApp.Models;

namespace CozinhaApp.Services;

public static class Formato
{
    private static readonly CultureInfo Br = new("pt-BR");

    public static string Quantidade(decimal valorBase, TipoMedida tipo)
    {
        if (valorBase >= 1000)
            return (valorBase / 1000).ToString("0.###", Br) + (tipo == TipoMedida.Massa ? " kg" : " L");
        return valorBase.ToString("0.##", Br) + (tipo == TipoMedida.Massa ? " g" : " ml");
    }

    public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", Br);
    public static string DataHora(DateTime d) => d.ToString("dd/MM HH:mm", Br);
}
