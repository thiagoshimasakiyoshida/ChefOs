# CozinhaApp – gerenciamento de cozinha (ASP.NET Core MVC + EF Core + Strategy)

## Como rodar
Requer o .NET 8 SDK.

    cd CozinhaApp
    dotnet restore
    dotnet run

Abra a URL exibida no terminal. O banco SQLite (`cozinha.db`) é criado automaticamente.

## Fluxo
1. **Ingredientes** → "Adicionar ao estoque": nome, quantidade (g, kg, ml ou L) e validade. Cada entrada é um lote.
2. **Pratos e receitas** → "Nova receita": etapas, com os ingredientes de cada etapa. A lista mostra quantas unidades dá para fazer.
3. **Preparar**: mostra o passo a passo, "Iniciar preparo" baixa os ingredientes do estoque e vai para a tela inicial.
4. **Em produção** (tela inicial) → clique no prato: receita, ingredientes, avançar etapas, finalizar ou cancelar.
5. **Mais preparados**: ranking por todo o período, ano, mês ou semana.

## Onde está o Strategy (pasta Strategies/)
| Interface | Implementações | Uso |
|---|---|---|
| IConversaoUnidadeStrategy | Grama, Quilograma, Mililitro, Litro | converte a unidade digitada para g/ml |
| IConsumoEstoqueStrategy | Fefo | ordem de consumo dos lotes (vence primeiro, sai primeiro) |
| ICancelamentoStrategy | CancelamentoTotal, CancelamentoPorEtapa | o que volta ao estoque conforme o estágio |
| IPeriodoStrategy | Todo, Ano, Mes, Semana | início do período do ranking |

Para criar uma nova regra, implemente a interface e registre em `Program.cs`.

## Regras
- Lote bloqueado ou vencido nunca é usado nem entra no cálculo de "unidades possíveis".
- Ao iniciar o preparo, os ingredientes de todas as etapas são reservados.
- Cancelar com o preparo ainda na etapa 0 devolve tudo; depois, devolve só as etapas que ainda não foram iniciadas.
