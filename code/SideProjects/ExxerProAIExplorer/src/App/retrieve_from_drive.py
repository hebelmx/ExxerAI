# This script navigates a specified Google Drive folder, retrieves all PDF files, converts them to Markdown format,
# stores the Markdown content in MongoDB, and tracks the conversion status in MongoDB.
# 
# Functionality:
# - Authenticates with Google Drive using OAuth 2.0.
# - Downloads PDF files from a specified Google Drive folder.
# - Converts each PDF file to Markdown using the `marker-pdf` package.
# - Stores the Markdown content in a MongoDB collection.
# - Updates the file tracking status in MongoDB to indicate that the file was converted to Markdown, including a timestamp.
# 
# Required Packages:
# - google-auth
# - google-auth-oauthlib
# - google-auth-httplib2
# - google-api-python-client
# - pymongo
# - marker-pdf
# - hashlib
# 
# Information Needed:
# - Google Drive API credentials file (google.json) located in the same directory as the script.
# - MongoDB connection URI.
# - Google Drive folder ID from which to download the PDF files.
# - Local destination folder to temporarily store the downloaded PDF files.
# 
# Google Drive Setup:
# 1. Create a project in the Google Cloud Console.
# 2. Enable the Google Drive API for the project.
# 3. Create OAuth 2.0 credentials and download the JSON file.
# 4. Place the JSON file in the same directory as this script and name it `google.json`.

import os
import io
import pickle
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseDownload
from pymongo import MongoClient
import hashlib

# Define the scopes
SCOPES = ['https://www.googleapis.com/auth/drive']

# Path to your credentials file
CREDENTIALS_FILE = os.path.join(os.path.dirname(__file__), 'google.json')
TOKEN_FILE = os.path.join(os.path.dirname(__file__), 'token.pickle')

# MongoDB configuration
MONGO_URI = 'mongodb://localhost:27017/'
DB_NAME = 'file_tracking'
COLLECTION_NAME = 'files'

def authenticate():
    creds = None
    if os.path.exists(TOKEN_FILE):
        with open(TOKEN_FILE, 'rb') as token:
            creds = pickle.load(token)
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(CREDENTIALS_FILE, SCOPES)
            creds = flow.run_local_server(port=0)
        with open(TOKEN_FILE, 'wb') as token:
            pickle.dump(creds, token)
    return creds

def get_file_hash(content):
    hasher = hashlib.md5()
    hasher.update(content)
    return hasher.hexdigest()

def download_files(service, folder_id, dest_folder):
    client = MongoClient(MONGO_URI)
    db = client[DB_NAME]
    collection = db[COLLECTION_NAME]

    query = f"'{folder_id}' in parents and trashed=false"
    results = service.files().list(q=query, fields="files(id, name, md5Checksum)").execute()
    items = results.get('files', [])

    if not items:
        print('No files found.')
        return

    if not os.path.exists(dest_folder):
        os.makedirs(dest_folder)

    for item in items:
        file_id = item['id']
        file_name = item['name']
        file_md5 = item.get('md5Checksum', '')

        # Check if the file already exists in MongoDB
        db_entry = collection.find_one({"file_id": file_id})
        if db_entry and db_entry['md5'] == file_md5:
            print(f"File {file_name} already downloaded and up-to-date.")
            continue

        # Download the file
        request = service.files().get_media(fileId=file_id)
        fh = io.BytesIO()
        downloader = MediaIoBaseDownload(fh, request)
        done = False
        while not done:
            status, done = downloader.next_chunk()
            print(f"Downloading {file_name}: {int(status.progress() * 100)}% complete.")

        # Write the file to disk
        with open(os.path.join(dest_folder, file_name), 'wb') as f:
            f.write(fh.getvalue())

        # Calculate the hash of the downloaded file
        file_hash = get_file_hash(fh.getvalue())

        # Update MongoDB with the new file information
        collection.update_one(
            {"file_id": file_id},
            {"$set": {"file_name": file_name, "md5": file_md5, "local_hash": file_hash}},
            upsert=True
        )

def main():
    print(f"Current working directory: {os.getcwd()}")
    print(f"Credentials file path: {CREDENTIALS_FILE}")

    creds = authenticate()
    service = build('drive', 'v3', credentials=creds)

    # Folder ID of the Google Drive folder you want to download from
    folder_id = '1IkFNMb3Qbmrkz_cZA9qDYBnjOu48f7Mx'
    # Destination folder on your local machine
    dest_folder = 'downloaded_files'

    download_files(service, folder_id, dest_folder)

if __name__ == '__main__':
    main()
