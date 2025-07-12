using Microsoft.Extensions.Configuration;

namespace ExxerAI.IntegrationTests.TestData;

/// <summary>
/// Helper class for managing test data and Google Drive document/folder IDs.
/// Provides utilities for extracting IDs from Google Drive URLs and validating test setup.
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// Extracts document ID from various Google Drive URL formats
    /// </summary>
    /// <param name="googleDriveUrl">Google Drive URL</param>
    /// <returns>Document ID or null if invalid URL</returns>
    public static string? ExtractDocumentIdFromUrl(string googleDriveUrl)
    {
        if (string.IsNullOrEmpty(googleDriveUrl))
            return null;

        // Handle different Google Drive URL formats:
        // https://drive.google.com/file/d/DOCUMENT_ID/view
        // https://drive.google.com/open?id=DOCUMENT_ID
        // https://docs.google.com/document/d/DOCUMENT_ID/edit

        var patterns = new[]
        {
            @"/file/d/([a-zA-Z0-9-_]+)",
            @"/document/d/([a-zA-Z0-9-_]+)",
            @"/spreadsheets/d/([a-zA-Z0-9-_]+)",
            @"/presentation/d/([a-zA-Z0-9-_]+)",
            @"[?&]id=([a-zA-Z0-9-_]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(googleDriveUrl, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        // If no pattern matches, assume the input is already a document ID
        if (System.Text.RegularExpressions.Regex.IsMatch(googleDriveUrl, @"^[a-zA-Z0-9-_]+$"))
        {
            return googleDriveUrl;
        }

        return null;
    }

    /// <summary>
    /// Extracts folder ID from Google Drive URL
    /// </summary>
    /// <param name="googleDriveFolderUrl">Google Drive folder URL</param>
    /// <returns>Folder ID or null if invalid URL</returns>
    public static string? ExtractFolderIdFromUrl(string googleDriveFolderUrl)
    {
        if (string.IsNullOrEmpty(googleDriveFolderUrl))
            return null;

        // Handle folder URL format:
        // https://drive.google.com/drive/folders/FOLDER_ID

        var match = System.Text.RegularExpressions.Regex.Match(
            googleDriveFolderUrl, 
            @"/folders/([a-zA-Z0-9-_]+)");

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // If no pattern matches, assume the input is already a folder ID
        if (System.Text.RegularExpressions.Regex.IsMatch(googleDriveFolderUrl, @"^[a-zA-Z0-9-_]+$"))
        {
            return googleDriveFolderUrl;
        }

        return null;
    }

    /// <summary>
    /// Validates Google Drive configuration and provides helpful error messages
    /// </summary>
    /// <param name="configuration">Test configuration</param>
    /// <returns>Validation result with specific issues</returns>
    public static TestConfigurationValidation ValidateConfiguration(IConfiguration configuration)
    {
        var validation = new TestConfigurationValidation();

        // Check credentials path
        var credentialsPath = configuration["GoogleDrive:CredentialsPath"];
        if (string.IsNullOrEmpty(credentialsPath))
        {
            validation.Issues.Add("GoogleDrive:CredentialsPath is not configured");
        }
        else if (!File.Exists(credentialsPath))
        {
            validation.Issues.Add($"Credentials file not found: {credentialsPath}");
        }

        // Check test document ID
        var testDocumentId = configuration["GoogleDrive:TestDocumentId"];
        if (string.IsNullOrEmpty(testDocumentId) || testDocumentId.Contains("YOUR_TEST_"))
        {
            validation.Issues.Add("GoogleDrive:TestDocumentId is not configured with a real document ID");
            validation.SetupInstructions.Add("1. Share a test document in Google Drive");
            validation.SetupInstructions.Add("2. Copy the document ID from the URL");
            validation.SetupInstructions.Add("3. Update GoogleDrive:TestDocumentId in appsettings.test.json");
        }

        // Check test folder ID
        var testFolderId = configuration["GoogleDrive:TestFolderId"];
        if (string.IsNullOrEmpty(testFolderId) || testFolderId.Contains("YOUR_TEST_"))
        {
            validation.Issues.Add("GoogleDrive:TestFolderId is not configured with a real folder ID");
            validation.SetupInstructions.Add("1. Create or use an existing folder in Google Drive");
            validation.SetupInstructions.Add("2. Copy the folder ID from the URL");
            validation.SetupInstructions.Add("3. Update GoogleDrive:TestFolderId in appsettings.test.json");
        }

        // Check application name
        var appName = configuration["GoogleDrive:ApplicationName"];
        if (string.IsNullOrEmpty(appName))
        {
            validation.Issues.Add("GoogleDrive:ApplicationName should be configured");
        }

        validation.IsValid = validation.Issues.Count == 0;
        return validation;
    }

    /// <summary>
    /// Creates sample test documents for testing (as byte arrays)
    /// </summary>
    public static class SampleDocuments
    {
        public static byte[] CreateSamplePdf(string content = "Sample PDF content for testing")
        {
            // Create a minimal PDF structure
            var pdfContent = $@"%PDF-1.4
1 0 obj
<<
/Type /Catalog
/Pages 2 0 R
>>
endobj

2 0 obj
<<
/Type /Pages
/Kids [3 0 R]
/Count 1
>>
endobj

3 0 obj
<<
/Type /Page
/Parent 2 0 R
/MediaBox [0 0 612 792]
/Contents 4 0 R
>>
endobj

4 0 obj
<<
/Length {content.Length + 20}
>>
stream
BT
/F1 12 Tf
100 700 Td
({content}) Tj
ET
endstream
endobj

xref
0 5
0000000000 65535 f 
0000000010 00000 n 
0000000079 00000 n 
0000000173 00000 n 
0000000253 00000 n 
trailer
<<
/Size 5
/Root 1 0 R
>>
startxref
{400 + content.Length}
%%EOF";

            return System.Text.Encoding.UTF8.GetBytes(pdfContent);
        }

        public static byte[] CreateLargeSamplePdf(int sizeMB = 5)
        {
            var baseContent = CreateSamplePdf("Large document content for performance testing");
            var targetSize = sizeMB * 1024 * 1024;
            
            if (baseContent.Length >= targetSize)
                return baseContent;

            // Pad the content to reach target size
            var paddingSize = targetSize - baseContent.Length;
            var padding = new byte[paddingSize];
            Array.Fill<byte>(padding, 0x20); // Fill with space characters

            var result = new byte[targetSize];
            baseContent.CopyTo(result, 0);
            padding.CopyTo(result, baseContent.Length);

            return result;
        }
    }
}

/// <summary>
/// Result of test configuration validation
/// </summary>
public class TestConfigurationValidation
{
    public bool IsValid { get; set; }
    public List<string> Issues { get; } = [];
    public List<string> SetupInstructions { get; } = [];

    public string GetErrorMessage()
    {
        if (IsValid) return string.Empty;

        var message = "Google Drive test configuration issues:\n";
        message += string.Join("\n", Issues.Select(i => $"❌ {i}"));
        
        if (SetupInstructions.Count > 0)
        {
            message += "\n\nSetup instructions:\n";
            message += string.Join("\n", SetupInstructions);
        }

        return message;
    }
}