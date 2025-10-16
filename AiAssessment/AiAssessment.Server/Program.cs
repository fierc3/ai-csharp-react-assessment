using AiAssessment.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapPost("/askChat", async (HttpRequest request) =>
{
    var body = await request.ReadFromJsonAsync<ChatRequest>();
    var askChat = new AskChat();
    return await askChat.GenerateMessage(body?.History ?? []);
});

app.MapFallbackToFile("/index.html");

app.Run();

