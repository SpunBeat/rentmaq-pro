using RentMaq.Application;
using RentMaq.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RentMaq.Infrastructure.Persistence.RentMaqDbContext>();
    await RentMaq.Infrastructure.Persistence.SeedData.DevelopmentSeedData.SeedAsync(db);
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
