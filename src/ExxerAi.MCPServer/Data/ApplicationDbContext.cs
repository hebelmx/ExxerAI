using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExxerAi.MCPServer.Data
{
    /// <summary>
    /// Application database context for Identity and MCP server data
    /// </summary>
    /// <param name="options">The database context options</param>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
    }
}
