using WebApplication.Middlewares;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.RequestLogMiddleware();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
