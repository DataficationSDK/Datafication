# AuthenticatedServer Sample

This sample demonstrates how to secure a Datafication.Server.Core API with JWT (JSON Web Token) authentication and role-based access control.

## Overview

- JWT Bearer token authentication
- Role-based authorization (read vs admin)
- Token endpoint for obtaining JWTs
- Separate policies for read and write access

## How to Run

```bash
cd AuthenticatedServer
dotnet restore
dotnet run
```

The server will start at `http://localhost:5000`.

## Demo Users

| Username | Password | Role | Access |
|----------|----------|------|--------|
| admin | admin123 | admin | Read + Write |
| reader | reader123 | read | Read only |

## Authentication Flow

### 1. Get a Token

```bash
curl -X POST http://localhost:5000/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "role": "admin"
}
```

### 2. Use the Token

Include the token in the `Authorization` header:

```bash
curl http://localhost:5000/api/data/employees \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Access Control

### Read Operations (DataBlockAccess policy)

Requires `read` or `admin` role:

```bash
# Get data (requires read or admin)
curl http://localhost:5000/api/data/datablocks/employees \
  -H "Authorization: Bearer <token>"

# Query data (requires read or admin)
curl -X POST http://localhost:5000/api/data/datablocks/employees/query \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"where": [{"column": "Department", "operator": "eq", "value": "Engineering"}]}'
```

### Write Operations (DataBlockAdmin policy)

Requires `admin` role only:

```bash
# Add row (requires admin)
curl -X POST http://localhost:5000/api/data/datablocks/employees/rows \
  -H "Authorization: Bearer <admin-token>" \
  -H "Content-Type: application/json" \
  -d '{"values": [4, "New Employee", "Sales", 60000]}'

# Update row at index 0 (requires admin)
curl -X PUT http://localhost:5000/api/data/datablocks/employees/rows/0 \
  -H "Authorization: Bearer <admin-token>" \
  -H "Content-Type: application/json" \
  -d '{"values": [1, "Alice Johnson-Smith", "Engineering", 100000]}'
```

## JWT Configuration

Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "DataficationServer",
    "Audience": "DataficationClients",
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!"
  }
}
```

**Important**: In production, store the secret key securely (e.g., environment variables, Azure Key Vault).

## C# Client Example

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// 1. Get token
var tokenRequest = new { username = "admin", password = "admin123" };
var tokenResponse = await client.PostAsJsonAsync("/auth/token", tokenRequest);
var token = (await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>())!.Token;

// 2. Create authenticated request
var request = new HttpRequestMessage(HttpMethod.Get, "/api/data/datablocks/employees");
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

// 3. Send request
var response = await client.SendAsync(request);
var data = await response.Content.ReadAsStringAsync();
Console.WriteLine(data);

record TokenResponse(string Token, int ExpiresIn, string Role);
```

## Server Configuration

```csharp
// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "DataficationServer",
            ValidAudience = "DataficationClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Configure access policies
builder.Services.AddDataficationServer(
    options => options.AllowAnonymousAccess = false,
    authOptions =>
    {
        authOptions.AddPolicy("DataBlockAccess", policy =>
            policy.RequireRole("read", "admin"));

        authOptions.AddPolicy("DataBlockAdmin", policy =>
            policy.RequireRole("admin"));
    });
```

## Error Responses

### 401 Unauthorized

No token or invalid token:

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 403 Forbidden

Valid token but insufficient permissions:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

## Production Considerations

1. **Use HTTPS** - Always use HTTPS in production to protect tokens in transit
2. **Secure Secret Key** - Store JWT secret key in secure configuration (not in code)
3. **Token Expiration** - Use short-lived tokens with refresh token mechanism
4. **Real User Store** - Replace demo users with a real authentication system
5. **Password Hashing** - Never store plain-text passwords

## Related Samples

- **BasicServer** - Start here if you're new to Datafication.Server.Core
- **QueryOperations** - Learn how to query data
- **RowOperations** - Learn about row-level operations
