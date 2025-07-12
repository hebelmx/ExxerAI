# 🔧 Google Drive Testing Setup Guide

## Step 1: Place Your Credentials

1. **Copy your Google Drive credentials JSON file** to these locations:
   ```
   /code/src/tests/ExxerAI.IntegrationTests/google-credentials.json
   /code/src/tests/ExxerAI.Infrastructure.Tests/google-credentials.json
   ```

## Step 2: Create Test Configuration Files

### For Integration Tests:
1. Copy the template:
   ```bash
   cd /code/src/tests/ExxerAI.IntegrationTests/
   cp appsettings.test.json.template appsettings.test.json
   ```

2. Edit `appsettings.test.json` and replace:
   - `YOUR_TEST_DOCUMENT_ID_HERE` - A test PDF/Word document in your Google Drive
   - `YOUR_TEST_FOLDER_ID_HERE` - A test folder in your Google Drive for watching
   - `YOUR_LARGE_DOCUMENT_ID_HERE` - A large document (>1MB) for performance testing

### For Infrastructure Tests:
```bash
cd /code/src/tests/ExxerAI.Infrastructure.Tests/
cp appsettings.test.json.template appsettings.test.json
```

## Step 3: Get Google Drive IDs

### For Document IDs:
1. Open Google Drive in browser
2. Click on a document
3. Copy the ID from URL: `https://drive.google.com/file/d/[DOCUMENT_ID]/view`

### For Folder IDs:
1. Open a folder in Google Drive
2. Copy the ID from URL: `https://drive.google.com/drive/folders/[FOLDER_ID]`

## Step 4: Test Your Setup

Run the tests to verify everything works:

```bash
# Test infrastructure (unit tests with mocks)
dotnet test /code/src/tests/ExxerAI.Infrastructure.Tests/ExxerAI.Infrastructure.Tests.csproj

# Test integration (real Google Drive API calls)
dotnet test /code/src/tests/ExxerAI.IntegrationTests/ExxerAI.IntegrationTests.csproj
```

## 🛡️ Security Notes

- ✅ All credential files are in `.gitignore` - they won't be committed
- ✅ Use environment variables for CI/CD: `EXXERAI_TEST_GoogleDrive__CredentialsPath`
- ✅ Keep your `google-credentials.json` file secure and never share it

## 🚀 Ready to Run!

Once setup is complete, you can run:
- Infrastructure tests (fast, mocked)
- Integration tests (real API calls)
- End-to-end chain tests (complete workflows)
- Edge case and error scenario tests