using QuizPortalAPI.Data;
using QuizPortalAPI.Services;
using QuizPortalAPI.Models;
using QuizPortalAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS - Allow credentials for cookie-based auth
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")  // Vite default, CRA default
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();  // Important for cookies
    });
});

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// JWT Authentication 
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Enable reading JWT from cookies in addition to Authorization header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check for token in cookies if not in Authorization header
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies["accessToken"];
            }
            return Task.CompletedTask;
        }
    };
});

// Register Services (Dependency Injection)
builder.Services.AddScoped<AuthHelper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IStudentResponseService, StudentResponseService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IGradingService, GradingService>();

// Configure HTTP context for cookie access
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// Enable CORS before auth
app.UseCors("AllowFrontend");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Apply migrations instead of EnsureCreated
        context.Database.Migrate();
        
        // Check if admin user exists
        var adminExists = context.Users.Any(u => u.Role == UserRole.Admin);
        logger.LogInformation($"Admin user exists: {adminExists}");
        
        // Seed admin user if not exists
        if (!adminExists)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            logger.LogInformation($"Creating admin user with hashed password: {hashedPassword}");
            
            var adminUser = new User
            {
                FullName = "System Administrator",
                Email = "admin@quizportal.com",
                Password = hashedPassword,
                Role = UserRole.Admin,
                IsDefaultPassword = false,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(adminUser);
            context.SaveChanges();
            
            logger.LogInformation("Admin user seeded successfully: admin@quizportal.com / Admin123!");
            
            // Verify the user was created
            var createdUser = context.Users.FirstOrDefault(u => u.Email == "admin@quizportal.com");
            if (createdUser != null)
            {
                logger.LogInformation($"Admin user verification: ID={createdUser.UserID}, Role={createdUser.Role}");
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists");
            
            // Check if the existing admin has the correct email and password
            var existingAdmin = context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
            if (existingAdmin != null)
            {
                logger.LogInformation($"Existing admin: {existingAdmin.Email}, Role={existingAdmin.Role}");

                var needsUpdate = false;

                // If the existing admin doesn't have the expected email, update it
                if (existingAdmin.Email != "admin@quizportal.com")
                {
                    logger.LogWarning($"Existing admin email ({existingAdmin.Email}) doesn't match expected email (admin@quizportal.com). Updating...");
                    existingAdmin.Email = "admin@quizportal.com";
                    needsUpdate = true;
                }

                // For reliability in local/testing environments, reset the admin password
                // to the known default so login works. Mark IsDefaultPassword = true so
                // callers can force a password change later.
                existingAdmin.Password = BCrypt.Net.BCrypt.HashPassword("Admin123!");
                existingAdmin.IsDefaultPassword = true;
                if (needsUpdate || existingAdmin.Password != null)
                {
                    context.Users.Update(existingAdmin);
                    context.SaveChanges();
                    logger.LogInformation("Existing admin record updated (email/password/isDefaultPassword).");
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
        logger.LogError($"Exception details: {ex.Message}");
        if (ex.InnerException != null)
        {
            logger.LogError($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

app.MapControllers();

app.Run();
