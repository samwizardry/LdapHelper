# LdapHelper

[![NuGet Version](https://img.shields.io/nuget/v/Samwizardry.LdapHelper.svg)](https://www.nuget.org/packages/Samwizardry.LdapHelper/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

A modern, robust, and high-performance .NET library for interacting with **Active Directory** and LDAP directories. Built directly on top of `System.DirectoryServices.Protocols` with native `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` integration.

---

## Features

- **LDAP Injection Protection**: RFC 4515-compliant escaping built into `FilterBuilder`.
- **Automatic Pagination**: Seamlessly bypasses Active Directory's 1000-entry `MaxPageSize` limit using `PageResultRequestControl`.
- **Full Async/Await & Streaming**: Non-blocking asynchronous operations and streaming with `IAsyncEnumerable<T>`.
- **Dependency Injection Ready**: Register services in `IServiceCollection` with a single call to `services.AddLdapHelper(...)`.
- **Password Management**: Change passwords with verification or administratively reset passwords over LDAPS.
- **Flexible User Modeling**: Use `DefaultUser` out of the box or define custom models via `IUser<TSelf>` using C# static abstract members.
- **Safe DateTime & UTC Parsing**: Reliable parsing of Active Directory `whenCreated`, `whenChanged`, and `FileTime` (`lastLogon`, `lastLogonTimestamp`) directly into UTC `DateTimeOffset?`.

---

## Installation

Install the package via the .NET CLI:

```bash
dotnet add package Samwizardry.LdapHelper
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package Samwizardry.LdapHelper
```

---

## Quickstart

### 1. Dependency Injection (ASP.NET Core / Generic Host)

Register `LdapHelper` in your application's `Program.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register LdapHelper services
builder.Services.AddLdapHelper(options =>
{
    options.Host = "192.168.0.137";
    options.Domain = "lorien";
    options.Zone = "local";
    options.Username = "Administrator";
    options.Password = "P@ssw0rd";
    options.DefaultQueryBase = "OU=Company Users,DC=lorien,DC=local";
    options.SecureSocketLayer = false; // Set to true (LDAPS port 636) for password operations
});

var app = builder.Build();
```

Inject `IActiveDirectoryService` into your controllers or services:

```csharp
public class UserController
{
    private readonly IActiveDirectoryService _adService;

    public UserController(IActiveDirectoryService adService)
    {
        _adService = adService;
    }

    public async Task<IActionResult> GetUser(string upn)
    {
        DefaultUser? user = await _adService.GetUserByUpnAsync<DefaultUser>(upn);
        if (user is null)
            return NotFound();

        return Ok(new { user.Name, user.Email, user.SamAccountName });
    }
}
```

---

### 2. Standalone / Direct Usage

You can also instantiate the service directly without a DI container:

```csharp
using LdapHelper;

var options = new LdapOptions
{
    Host = "192.168.0.137",
    Domain = "lorien",
    Zone = "local",
    Username = "Administrator",
    Password = "P@ssw0rd",
    DefaultQueryBase = "OU=Company Users,DC=lorien,DC=local"
};

var connectionFactory = new LdapConnectionFactory(options);
using var service = new ActiveDirectoryService(connectionFactory);

// Query users
DefaultUser? user = service.GetUserByUpn<DefaultUser>("john@lorien.local");
```

---

## Common Scenarios

### Finding Users by UPN or SAM Account Name

```csharp
// Find user by UserPrincipalName
DefaultUser? userByUpn = await service.GetUserByUpnAsync<DefaultUser>("john@lorien.local");

// Find user by sAMAccountName (Windows logon name)
DefaultUser? userBySam = await service.GetUserBySamAsync<DefaultUser>("john");

if (userBySam is not null)
{
    Console.WriteLine($"Display Name: {userBySam.Name}");
    Console.WriteLine($"Email: {userBySam.Email}");
    Console.WriteLine($"Created At (UTC): {userBySam.CreatedAt}");
    Console.WriteLine($"Last Logon (UTC): {userBySam.LastLogonTime}");
    Console.WriteLine($"Is Disabled: {userBySam.IsAccountDisabled()}");
}
```

---

### Querying Group Members

Retrieve all members of an Active Directory security group:

```csharp
const string groupDn = "CN=Company Managers,OU=Company Groups,DC=lorien,DC=local";

// Synchronous enumeration
IEnumerable<DefaultUser> managers = service.GetUsersByGroup<DefaultUser>(groupDn);

// Asynchronous streaming with IAsyncEnumerable
await foreach (DefaultUser manager in service.GetUsersByGroupAsync<DefaultUser>(groupDn))
{
    Console.WriteLine($"Manager: {manager.SamAccountName} ({manager.Email})");
}
```

---

### Advanced Queries with `FilterBuilder`

`FilterBuilder` provides a fluent, injection-safe API (RFC 4515) for building LDAP filters:

```csharp
string filter = new FilterBuilder()
    .AddObjectCategory("person")
    .AddObjectClass("user")
    .AddAccountEnabledFilter()
    .AddMemberOf("CN=Engineers,OU=Groups,DC=lorien,DC=local")
    .AddEquals("company", "Contoso Ltd.")
    .Build();

// Automatically handles pagination (>1000 items) behind the scenes
IEnumerable<DefaultUser> engineers = service.GetUsers<DefaultUser>(
    DefaultUser.Attributes,
    queryBase: "DC=lorien,DC=local",
    filter: filter,
    pageSize: 1000);
```

---

### User Authentication (Credential Verification)

Validate user credentials against Active Directory:

```csharp
// Check credentials safely
if (service.TryAuthenticate("john", "UserPassword123!", out string? errorMessage))
{
    Console.WriteLine("Authentication successful!");
}
else
{
    Console.WriteLine($"Authentication failed: {errorMessage}");
}

// Or asynchronously
bool isValid = await service.TryAuthenticateAsync("john@lorien.local", "UserPassword123!");
```

---

### Password Management

> **Note**: Active Directory requires a secure connection (**LDAPS**, port 636) for all password modification operations (`SecureSocketLayer = true`).

```csharp
// Change password (verifying old password)
await service.ChangePasswordAsync(
    user.DistinguishedName,
    oldPassword: "OldPassword123!",
    newPassword: "NewSecretPassword456!");

// Administrative password reset (without knowing the old password)
await service.ResetPasswordAsync(
    user.DistinguishedName,
    newPassword: "TemporaryPassword789!");
```

---

### Custom User Models

To define a lightweight or specialized user model, implement `IUser<TSelf>`:

```csharp
using System.DirectoryServices.Protocols;
using LdapHelper;

public class Employee : IUser<Employee>
{
    public Guid? ObjectGuid { get; init; }
    public string? SamAccountName { get; init; }
    public string? UserPrincipalName { get; init; }
    public string? DistinguishedName { get; init; }
    public string? Email { get; init; }
    public string? Department { get; init; }

    public static Employee? ParseUser(SearchResultEntry entry)
    {
        return new Employee
        {
            ObjectGuid = entry.GetGuid("objectGUID"),
            SamAccountName = entry.GetString("sAMAccountName"),
            UserPrincipalName = entry.GetString("userPrincipalName"),
            DistinguishedName = entry.DistinguishedName,
            Email = entry.GetString("mail"),
            Department = entry.GetString("department")
        };
    }
}

// Usage:
Employee? emp = await service.GetUserBySamAsync<Employee>("john");
```

---

## Configuration Reference (`LdapOptions`)

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Host` | `string` | `""` | LDAP server hostname or IP address (e.g. `192.168.0.137` or `dc1.domain.local`). |
| `Port` | `int` | `389` | Port number (`389` for plain LDAP, `636` for LDAPS). |
| `Domain` | `string` | `""` | Domain name (e.g. `lorien` or `lorien.local`). |
| `Username` | `string` | `""` | Service account username (`user`, `DOMAIN\user`, or `user@domain.local`). |
| `Password` | `string` | `""` | Service account password. |
| `DefaultQueryBase` | `string?` | `null` | Base DN for searches (e.g. `DC=lorien,DC=local`). Auto-calculated if not set. |
| `SecureSocketLayer` | `bool` | `false` | Enable SSL/TLS encryption (LDAPS). Required for password operations. |
| `TrustAllCertificates`| `bool` | `false` | Skip SSL certificate verification (for local development/testing only). |
| `Timeout` | `TimeSpan` | `30s` | LDAP connection timeout. |
| `ProtocolVersion` | `int` | `3` | LDAP protocol version (LDAPv3). |

---

## License

This project is licensed under the [MIT License](LICENSE).
