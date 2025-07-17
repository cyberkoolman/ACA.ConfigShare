using SharedConfigApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ConfigService>();

// Add configuration for shared config path
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["SharedConfigPath"] = Path.Combine(Directory.GetCurrentDirectory(), "SharedData", "config.xml")
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

// Initialize the ConfigService
var configService = app.Services.GetRequiredService<ConfigService>();

app.Run();