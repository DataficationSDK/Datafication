using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Datafication.Core.Data;
using Datafication.Server.Core.Models;
using Datafication.Server.Core.Registry;
using Datafication.Server.Core.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "DataficationServer";
var audience = jwtSettings["Audience"] ?? "DataficationClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// 2. Add Datafication Server with role-based access policies
builder.Services.AddDataficationServer(
    options =>
    {
        options.AllowAnonymousAccess = false; // Require authentication
        options.RoutePrefix = "api/data";
    },
    authOptions =>
    {
        // Read access - requires "read" or "admin" role
        authOptions.AddPolicy("DataBlockAccess", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == ClaimTypes.Role &&
                    (c.Value == "read" || c.Value == "admin"))));

        // Write access - requires "admin" role
        authOptions.AddPolicy("DataBlockAdmin", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "admin")));
    });

var app = builder.Build();

// 3. Register sample DataBlock
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();

    var employees = new DataBlock();
    employees.AddColumn(new DataColumn("Id", typeof(int)));
    employees.AddColumn(new DataColumn("Name", typeof(string)));
    employees.AddColumn(new DataColumn("Department", typeof(string)));
    employees.AddColumn(new DataColumn("Salary", typeof(decimal)));

    employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m });
    employees.AddRow(new object[] { 2, "Bob Smith", "Marketing", 75000m });
    employees.AddRow(new object[] { 3, "Carol Williams", "Engineering", 105000m });

    var metadata = new DataBlockMetadata
    {
        Name = "Employees",
        Description = "Employee directory (requires authentication)",
        Tags = new[] { "employees", "secure" },
        RegisteredAt = DateTime.UtcNow
    };

    registry.RegisterDataBlock("employees", employees, metadata);
    Console.WriteLine("Registered 'employees' DataBlock");
}

// 4. Add a token endpoint for demo purposes
app.MapPost("/auth/token", (TokenRequest request) =>
{
    // In production, validate credentials against a real user store
    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
    {
        return Results.BadRequest("Username and password required");
    }

    // Demo users (in production, use a real authentication system)
    var role = request.Username.ToLower() switch
    {
        "admin" when request.Password == "admin123" => "admin",
        "reader" when request.Password == "reader123" => "read",
        _ => null
    };

    if (role == null)
    {
        return Results.Unauthorized();
    }

    var token = GenerateToken(request.Username, role, secretKey, issuer, audience);
    return Results.Ok(new { token, expiresIn = 3600, role });
});

// 5. Configure HTTP pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine();
Console.WriteLine("AuthenticatedServer is running at http://localhost:5000");
Console.WriteLine();
Console.WriteLine("Demo Users:");
Console.WriteLine("  admin/admin123 - Full access (read + write)");
Console.WriteLine("  reader/reader123 - Read-only access");
Console.WriteLine();
Console.WriteLine("Get a token:");
Console.WriteLine(@"  curl -X POST http://localhost:5000/auth/token -H ""Content-Type: application/json"" -d '{""username"":""admin"",""password"":""admin123""}'");
Console.WriteLine();
Console.WriteLine("Use the token:");
Console.WriteLine(@"  curl http://localhost:5000/api/data/datablocks/employees -H ""Authorization: Bearer <token>""");
Console.WriteLine();

app.Run();

// Helper to generate JWT tokens
static string GenerateToken(string username, string role, string secretKey, string issuer, string audience)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

record TokenRequest(string Username, string Password);
