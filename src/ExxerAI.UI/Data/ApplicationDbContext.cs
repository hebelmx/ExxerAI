using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExxerAI.UI.Data;

/// <summary>
/// Database context for the ExxerAI application, extending Identity framework
/// </summary>
/// <param name="options">The database context options</param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
}
