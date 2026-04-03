using Onyx.Oms.Core;
using Onyx.Oms.Infrastructure;
using Onyx.Oms.Web.Extensions;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddCore(typeof(Program).Assembly)
    .AddInfrastructure(builder.Configuration);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<Onyx.Oms.Web.Common.Settings.DefaultTenantProfileSettings>(
    builder.Configuration.GetSection(Onyx.Oms.Web.Common.Settings.DefaultTenantProfileSettings.SectionName));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<Onyx.Oms.Web.Middleware.GlobalExceptionHandler>();

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
        new Asp.Versioning.UrlSegmentApiVersionReader(),
        new Asp.Versioning.HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// Run Seeder
using (var scope = app.Services.CreateScope())
{
    var databaseSeeder = scope.ServiceProvider.GetRequiredService<Onyx.Oms.Infrastructure.Persistence.Seeding.DatabaseSeeder>();
    await databaseSeeder.SeedAsync();

    var subscriptionPlanSeeder = scope.ServiceProvider.GetRequiredService<Onyx.Oms.Infrastructure.Persistence.Seeding.SubscriptionPlanSeeder>();
    await subscriptionPlanSeeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("ONYX OMS API")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

//app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<Onyx.Oms.Infrastructure.Middleware.UserMirrorMiddleware>();

app.MapEndpoints();

app.Run();
