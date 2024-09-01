var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://khalidwalid2001.github.io")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});


var app = builder.Build();

// Configure the HTTP request pipeline.

// Enable Swagger/OpenAPI
app.UseSwagger();
app.UseSwaggerUI();

// Enable HTTPS redirection and HSTS (HTTP Strict Transport Security)
app.UseHttpsRedirection();
app.UseHsts();

// Enable CORS
app.UseCors("AllowSpecificOrigin");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
