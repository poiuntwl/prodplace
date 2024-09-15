var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;
s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/hello", () => "Hello World!");

app.Run();