using System;

namespace ExxerAi.MCPServer.Application;

/// <summary>
/// Marks a method as an MCP server tool that can be called by MCP clients.
/// This is our internal implementation replacing the external ModelContextProtocol dependency.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class McpServerToolAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the tool.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the McpServerToolAttribute class.
    /// </summary>
    public McpServerToolAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the McpServerToolAttribute class with a name.
    /// </summary>
    /// <param name="name">The name of the tool</param>
    public McpServerToolAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the McpServerToolAttribute class with a name and description.
    /// </summary>
    /// <param name="name">The name of the tool</param>
    /// <param name="description">The description of the tool</param>
    public McpServerToolAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}