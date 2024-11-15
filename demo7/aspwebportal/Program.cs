using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/hello", () => "Hello World!");

var items = new List<Item>();

app.MapGet("/items", () => items);

app.MapGet("/items/{id}", Results<Ok<Item>, NotFound<int>> (int id) =>
{
    var item = items.FirstOrDefault(t => t.Id == id);
    return item is null ? TypedResults.NotFound(id) : TypedResults.Ok(item);
});

app.MapPost("/items", (Item item) =>
{
    items.Add(item);
    return TypedResults.Created("/items/{id}", item);
});

app.MapDelete("/items/{id}", (int id) =>
{
    items.RemoveAll(t => t.Id == id);
    return TypedResults.NoContent();
});


app.Run();


public record Item(int Id, string Name, DateTime DueDate, bool IsCompleted);