# Secure Google Drive Integration Testing with .NET and Workload Identity Federation

This document explains how to securely integrate Google Drive with your .NET integration tests using **Workload Identity Federation (WIF)** or **Application Default Credentials (ADC)**, avoiding service account JSON files.

## 🔐 Why Not Use JSON Keys Anymore?

Google discourages using downloaded service account keys due to the high security risk. Keys can be leaked and are now subject to automatic revocation when detected in public repos.

## 🧩 Authentication Options

### 🔸 Local Development (CLI)

**Method**: Application Default Credentials (ADC)

#### 🔧 One-time Setup

```bash
gcloud auth application-default login
```

- Stores secure OAuth2 tokens locally.
- No need to download service account keys.

#### 🔄 Usage in Integration Tests

In your `.NET` test project:

```csharp
var credential = await GoogleCredential.GetApplicationDefaultAsync();
if (credential.IsCreateScopedRequired)
    credential = credential.CreateScoped(DriveService.Scope.Drive);
```

Run tests normally:

```bash
dotnet test --filter Category=Integration
```

### 🔸 CI/CD (e.g., GitHub Actions)

**Method**: Workload Identity Federation (OIDC from GitHub)

#### 🛠️ GCP Setup Steps

1. **Create Workload Identity Pool**:

```bash
gcloud iam workload-identity-pools create "my-pool" \
  --location="global" \
  --display-name="My Pool"
```

2. **Create OIDC Provider (GitHub)**:

```bash
gcloud iam workload-identity-pools providers create-oidc "github-provider" \
  --location="global" \
  --workload-identity-pool="my-pool" \
  --display-name="GitHub Provider" \
  --issuer-uri="https://token.actions.githubusercontent.com" \
  --attribute-mapping="google.subject=assertion.sub"
```

3. **Grant Access to Service Account**:

```bash
gcloud iam service-accounts add-iam-policy-binding \
  "exxerai@exxerai.iam.gserviceaccount.com" \
  --role="roles/iam.workloadIdentityUser" \
  --member="principalSet://iam.googleapis.com/projects/YOUR_PROJECT_NUMBER/locations/global/workloadIdentityPools/my-pool/attribute.subject/YOUR_GITHUB_SUBJECT"
```

4. **Generate Credential Config**:

```bash
gcloud iam workload-identity-pools create-cred-config \
  --workload-identity-pool-provider="projects/YOUR_PROJECT_NUMBER/locations/global/workloadIdentityPools/my-pool/providers/github-provider" \
  --service-account="exxerai@exxerai.iam.gserviceaccount.com" \
  --output-file="wif-cred.json"
```

#### 🧪 GitHub Workflow Sample

```yaml
jobs:
  integration:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Set up .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Authenticate with WIF
      run: |
        export GOOGLE_APPLICATION_CREDENTIALS=wif-cred.json
        dotnet test --filter Category=Integration
```

## 📈 Flow Diagram

```
[.NET App] ---> [GoogleCredential.GetApplicationDefault()] --->
    [1] local: ~/.config/gcloud/application_default_credentials.json
    [2] CI: federated credential from WIF config

    ---> [DriveService] ---> [Google Drive API Access]
```

## 🧪 XUnit Integration Test Example

```csharp
public class DriveIntegrationTests
{
    [Fact, Trait("Category", "Integration")]
    public async Task ShouldListDriveFiles()
    {
        var credential = await GoogleCredential.GetApplicationDefaultAsync();
        if (credential.IsCreateScopedRequired)
            credential = credential.CreateScoped(DriveService.Scope.Drive);

        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "ExxerAI Integration",
        });

        var files = await service.Files.List().ExecuteAsync();
        files.Files.ShouldNotBeEmpty();
    }
}
```

## ✅ Summary

| Scenario       | Method           | Keys Used     |
| -------------- | ---------------- | ------------- |
| Local Dev      | ADC via `gcloud` | ❌             |
| CI/CD (GitHub) | WIF + OIDC token | ❌             |
| Deprecated     | JSON key files   | ⚠️ Not Secure |

### 🔐 Service Account Info (Used in WIF setup)

```json
{
  "type": "service_account",
  "project_id": "exxerai",
  "private_key_id": "d7408e6b83c380e064acae1124698b83a8ab6791",
  "client_email": "exxerai@exxerai.iam.gserviceaccount.com",
  "client_id": "109050742539773248446",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/exxerai%40exxerai.iam.gserviceaccount.com",
  "universe_domain": "googleapis.com"
}
```

> 🔒 **Important**: Add any service account or federated credential files to your `.gitignore`:

```gitignore
# Credential-related files
*.json
!appsettings*.json
**/wif-cred.json
**/service-account.json
**/credentials/**
```

Follow these practices for secure, compliant, and automatable integration with Google APIs in .NET projects.

