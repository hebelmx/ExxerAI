using Microsoft.AspNetCore.Identity;

namespace ExxerAI.MCPServer.Data
{
    /// <summary>
    /// Application user entity extending IdentityUser for MCP server authentication
    /// Add profile data for application users by adding properties to the ApplicationUser class
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
    }

}
