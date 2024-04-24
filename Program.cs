using System.Reflection;
using CmApi.Classes;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSingleton<IDatabase, Database>();
builder.Services.AddSingleton<IByond, CmApi.Classes.Byond>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors", policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

var info = new OpenApiInfo()
{
    Title = "CM-API",
    Version = "v1",
};

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", info);

// Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UsePathBase("/api");
}

app.UseHttpsRedirection();
app.UseCors("cors");
app.UseAuthorization();
app.MapControllers();

app.Run();