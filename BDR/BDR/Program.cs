using BillApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//  global exception filter to handle exceptions
builder.Services.AddControllers(options =>
    options.Filters.Add<GlobalExceptionFilter>()
);


// Session
builder.Services.AddSession
    (
        option =>
        {
            option.IdleTimeout = TimeSpan.FromMinutes(5);
            option.Cookie.HttpOnly = true;
            option.Cookie.IsEssential = true;

        }
    );

builder.Services.AddHttpContextAccessor();

// http client
builder.Services.AddHttpClient();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
