using CozinhaApp.Data;
using CozinhaApp.Services;
using CozinhaApp.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CozinhaContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Cozinha") ?? "Data Source=cozinha.db"));

// ---- Strategy 1: conversão de unidades
builder.Services.AddSingleton<IConversaoUnidadeStrategy, GramaStrategy>();
builder.Services.AddSingleton<IConversaoUnidadeStrategy, QuilogramaStrategy>();
builder.Services.AddSingleton<IConversaoUnidadeStrategy, MililitroStrategy>();
builder.Services.AddSingleton<IConversaoUnidadeStrategy, LitroStrategy>();

// ---- Strategy 2: ordem de consumo do estoque (troque por outra implementação se quiser, ex.: PEPS)
builder.Services.AddSingleton<IConsumoEstoqueStrategy, FefoStrategy>();

// ---- Strategy 3: devolução de ingredientes no cancelamento
builder.Services.AddSingleton<ICancelamentoStrategy, CancelamentoTotalStrategy>();
builder.Services.AddSingleton<ICancelamentoStrategy, CancelamentoPorEtapaStrategy>();

// ---- Strategy 4: períodos do ranking
builder.Services.AddSingleton<IPeriodoStrategy, PeriodoTodoStrategy>();
builder.Services.AddSingleton<IPeriodoStrategy, PeriodoAnoStrategy>();
builder.Services.AddSingleton<IPeriodoStrategy, PeriodoMesStrategy>();
builder.Services.AddSingleton<IPeriodoStrategy, PeriodoSemanaStrategy>();

builder.Services.AddScoped<ConversorUnidades>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<PreparoService>();
builder.Services.AddScoped<RankingService>();

var app = builder.Build();

// Cultura invariante-like (en-US) para o model binding de decimais vindos de <input type="number">.
// A exibição em pt-BR é feita pela classe Formato.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US")
    .AddSupportedUICultures("en-US"));

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CozinhaContext>().Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
