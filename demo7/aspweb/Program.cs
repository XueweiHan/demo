var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

///////////////////////////////////////////////////////////////////
// Add a simple JSON API endpoint

app.MapGet("/hello", () => "Hello World! " + Environment.MachineName + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

var items = new List<Item>();

app.MapGet("/items", () => items);

app.MapGet("/items/{id}", (int id) =>
{
    var item = items.FirstOrDefault(t => t.Id == id);
    return item is null ? Results.NotFound(id) : Results.Ok(item);
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

///////////////////////////////////////////////////////////////////

app.Run();

///////////////////////////////////////////////////////////////////
// This is a simple record type to represent an item in the list
public record Item(int Id, string Name, DateTime DueDate, bool IsCompleted);
