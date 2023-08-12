var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc(a => a.EnableEndpointRouting = false);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseMvcWithDefaultRoute();

app.Run();