using KeyboardRebind;

var builder = WebApplication.CreateBuilder(args);

// Loading all keyboards in KeyboardsData folder
var keyboard_data_path = Path.Combine(builder.Environment.ContentRootPath,"KeyboardsData");
var keyboards = new List<Keyboard>();
foreach (string keyboard_folder_path in Directory.EnumerateDirectories(keyboard_data_path))
{
    var keyboard = new Keyboard(keyboard_folder_path);
    keyboards.Add(keyboard);
}

builder.Services.AddSingleton(keyboards);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// For HTML Visual representation
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
