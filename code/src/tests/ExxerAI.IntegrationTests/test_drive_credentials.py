#!/usr/bin/env python3
"""
Test Google Drive Service Account Credentials
This script tests the service account from GDrive.Api.json
"""

import json
import os
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError

def test_api_key():
    """Test if API key works (spoiler: it won't for Drive API)"""
    print("🔑 Testing API Key...")
    
    try:
        # This will fail - Drive API doesn't support API keys
        from googleapiclient.discovery import build
        service = build('drive', 'v3', developerKey='AIzaSyAzVBpYY8XxzLlJra-vSkez0xiNH3J5oBI')
        results = service.files().list(pageSize=10).execute()
        print("✅ API Key works!")
        return True
    except Exception as e:
        print(f"❌ API Key failed (expected): {e}")
        return False

def test_service_account():
    """Test service account credentials"""
    print("\n🔐 Testing Service Account...")
    
    try:
        # Load the service account info from exxerai.gdrive.json
        with open('exxerai.gdrive.json', 'r') as f:
            service_info = json.load(f)
        
        if not service_info:
            print("❌ No service account data found in exxerai.gdrive.json")
            return False
            
        print(f"📧 Service Account: {service_info.get('client_email', 'Not found')}")
        print(f"🆔 Project ID: {service_info.get('project_id', 'Not found')}")
        
        # Create credentials from service account info
        credentials = service_account.Credentials.from_service_account_info(
            service_info,
            scopes=['https://www.googleapis.com/auth/drive.readonly']
        )
        
        # Build the service
        service = build('drive', 'v3', credentials=credentials)
        
        # Test basic access
        print("🔍 Testing basic Drive access...")
        results = service.files().list(pageSize=5).execute()
        files = results.get('files', [])
        
        print(f"✅ Service Account works! Found {len(files)} files")
        
        if files:
            print("\n📁 Sample files:")
            for file in files[:3]:
                print(f"  - {file.get('name', 'Unknown')} ({file.get('id', 'No ID')})")
        else:
            print("📭 No files found (service account might not have access to any files)")
            
        return True
        
    except FileNotFoundError:
        print("❌ exxerai.gdrive.json file not found")
        return False
    except KeyError as e:
        print(f"❌ Missing required field in service account: {e}")
        return False
    except HttpError as e:
        print(f"❌ Google API error: {e}")
        return False
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        return False

def main():
    print("🧪 Testing Google Drive Credentials\n")
    print("=" * 50)
    
    # Test API Key (will fail)
    api_success = test_api_key()
    
    # Test Service Account (should work)
    service_success = test_service_account()
    
    print("\n" + "=" * 50)
    print("📊 Summary:")
    print(f"  API Key: {'✅ Success' if api_success else '❌ Failed (expected)'}")
    print(f"  Service Account: {'✅ Success' if service_success else '❌ Failed'}")
    
    if service_success:
        print("\n🎉 Your service account credentials are working!")
        print("💡 Update your .NET app to use service account instead of API key")
    else:
        print("\n⚠️  Service account needs attention - check credentials and permissions")

if __name__ == "__main__":
    main() 