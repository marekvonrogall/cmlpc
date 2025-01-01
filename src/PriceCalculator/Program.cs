using PriceCalculator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton(new DistanceService(new HttpClient(), new GeocodeService("/app/data/plz_geocoord.csv")));
//plz_geocoord.csv from: https://github.com/WZBSocialScienceCenter/plz_geocoord

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
