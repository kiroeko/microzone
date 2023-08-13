using Microsoft.OpenApi.Models;
using MicrozoneService;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:22333");

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Formatting = Formatting.Indented;
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    })
    .AddControllersAsServices();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Microzone Service APIs", Version = "v1" });
});

builder.Services.AddStatusService();
builder.Services.AddValorantService();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Microzone");
});

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
});

await Task.Run(async () => {
    IServiceScope scope = app.Services.CreateScope();
    try
    {
        var retryCount = 5;
        while (retryCount > 0)
        {
            try
            {
                StatusService ss = scope.ServiceProvider.GetRequiredService<StatusService>();
                ss.IsReady = true;
                break;
            }
            catch
            {
                await Task.Delay(1000);
                --retryCount;
            }
        }
    }
    finally
    {
        scope.Dispose();
    }
});

app.Run();