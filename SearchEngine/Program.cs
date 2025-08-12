using SearchEngine.Data;
using SearchEngine.Repositories;
using SearchEngine.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddHttpContextAccessor();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SearchEngineDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SearchEngineConnectionString")));

// Register repositories
builder.Services.AddScoped<IDocumentRepository, SQLDocumentRepository>();
builder.Services.AddScoped<IDocumentKeywordRepository, SQLDocumentKeywordRepository>();

// Register services
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles(
    new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
        RequestPath = new PathString("/Uploads")
    }
);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();