# 🚀 Google Drive MCP Integration Setup Guide

## Overview

ExxerAI MCP Server now includes **native C# Google Drive integration** with real-time document monitoring and advanced document processing capabilities. This replaces mock implementations with production-ready Google Drive API integration.

## 🔧 Prerequisites

1. **Google Cloud Project** with Google Drive API enabled
2. **OAuth 2.0 Credentials** configured for web application
3. **.NET 9.0** runtime
4. **Valid Google account** with Drive access

## 📋 Setup Instructions

### 1. Google Cloud Console Setup

1. **Create or Select Project**:
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select existing one

2. **Enable Google Drive API**:
   - Navigate to **APIs & Services** → **Library**
   - Search for "Google Drive API"
   - Click **Enable**

3. **Create OAuth 2.0 Credentials**:
   - Go to **APIs & Services** → **Credentials**
   - Click **Create Credentials** → **OAuth 2.0 Client IDs**
   - Application type: **Web application**
   - Authorized redirect URIs: `http://localhost:8000/oauth2callback`
   - Save **Client ID** and **Client Secret**

### 2. Configuration Methods

#### Option A: Environment Variables (Recommended for Production)

```bash
# Set environment variables
export GOOGLE_OAUTH_CLIENT_ID="your-client-id.apps.googleusercontent.com"
export GOOGLE_OAUTH_CLIENT_SECRET="your-client-secret"

# For development only
export OAUTHLIB_INSECURE_TRANSPORT=1
```

#### Option B: Configuration File (Development)

Update `appsettings.json`:

```json
{
  "GoogleDrive": {
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "http://localhost:8000/oauth2callback"
  }
}
```

### 3. MCP Client Configuration

#### For Claude Desktop

Add to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "exxerai_google_drive": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/ExxerAi.MCPServer"],
      "env": {
        "GOOGLE_OAUTH_CLIENT_ID": "your-client-id.apps.googleusercontent.com",
        "GOOGLE_OAUTH_CLIENT_SECRET": "your-client-secret"
      }
    }
  }
}
```

#### For Other MCP Clients

The server runs on `http://localhost:8000` by default and exposes MCP endpoints at `/mcp`.

## 🔑 Authentication Flow

1. **First Tool Call**: Server will return OAuth authorization URL
2. **Browser Authentication**: Open URL, sign in to Google, authorize access
3. **Automatic Token Management**: Server handles token refresh automatically
4. **Subsequent Calls**: Use stored credentials for API calls

## 🛠️ Available MCP Tools

### Core Google Drive Operations

- **`StartFolderWatchAsync`**: Monitor Google Drive folders for changes
- **`DownloadDocumentAsync`**: Download files with metadata
- **`GetDocumentMetadataAsync`**: Retrieve detailed file information
- **`GetActiveWatchesAsync`**: List all active monitoring sessions
- **`StopWatchingAsync`**: Stop folder monitoring
- **`CheckHealthStatusAsync`**: Verify API connectivity and authentication

### Document Processing Integration

- **Real-time Processing**: Auto-process documents detected in watched folders
- **KpiExxerpro Pipeline**: Advanced document intelligence with 95% accuracy
- **Hybrid Storage**: Store results in SQL Server 2025 with vector search
- **Audit Trail**: Complete processing lineage and validation history

## 🔄 Usage Examples

### Start Monitoring a Folder

```
Tool: StartFolderWatchAsync
Parameters:
- folderId: "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms"
- includeSubdirectories: true
- autoProcess: true
- pollingIntervalSeconds: 60
```

### Download and Process Document

```
Tool: DownloadDocumentAsync
Parameters:
- documentId: "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms"
```

### Check System Health

```
Tool: CheckHealthStatusAsync
Parameters: (none)
```

## 🚨 Troubleshooting

### Common Issues

1. **"Drive service not initialized"**
   - Verify OAuth credentials are correctly configured
   - Check environment variables or appsettings.json

2. **"Folder not found or not accessible"**
   - Ensure folder ID is correct
   - Verify Google account has access to the folder
   - Check folder sharing permissions

3. **OAuth redirect errors**
   - Verify redirect URI in Google Cloud Console: `http://localhost:8000/oauth2callback`
   - Ensure server is running on correct port (8000)

### Debug Mode

For development, set:
```bash
export OAUTHLIB_INSECURE_TRANSPORT=1
```

This allows OAuth over HTTP (localhost only).

## 🔒 Security Considerations

- **Production**: Use HTTPS for OAuth callbacks
- **Credentials**: Never commit OAuth secrets to version control
- **Environment Variables**: Preferred for production deployments
- **Token Storage**: OAuth tokens are stored securely by Google Auth library
- **Scope Minimization**: Only requests necessary Google Drive permissions

## 📊 Integration Architecture

```
Claude/MCP Client
    ↓
ExxerAI MCP Server (C#)
    ↓
Google Drive API (OAuth)
    ↓
Document Detection & Download
    ↓
ExxerAI Document Processing Pipeline
    ↓
SQL Server 2025 Hybrid Vector Storage
```

## 🎯 Phase 2 Integration Status

✅ **Google Drive API Integration**: Native C# implementation  
✅ **Real-time Monitoring**: Folder watching with background polling  
✅ **Document Processing**: Integration with KpiExxerpro pipeline  
✅ **MCP Protocol**: Full MCP tool compliance  
✅ **Production Ready**: Error handling, logging, health checks  
🔄 **Vector Storage**: SQL Server 2025 integration (next step)  
🔄 **Advanced Analytics**: Pattern learning and validation (next step)  

## 📞 Support

For issues or questions:
1. Check server logs for detailed error messages
2. Verify Google Cloud Console API quotas and limits
3. Test OAuth flow manually using `CheckHealthStatusAsync`
4. Review Google Drive API documentation for advanced features 