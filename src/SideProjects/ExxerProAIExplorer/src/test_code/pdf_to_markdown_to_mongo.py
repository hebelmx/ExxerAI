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
from datetime import datetime
import marker

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

def download_file(service, file_id, file_name, dest_folder):
    request = service.files().get_media(fileId=file_id)
    file_path = os.path.join(dest_folder, file_name)
    fh = io.FileIO(file_path, 'wb')
    downloader = MediaIoBaseDownload(fh, request)
    done = False
    while not done:
        status, done = downloader.next_chunk()
        print(f"Downloading {file_name}: {int(status.progress() * 100)}% complete.")
    return file_path

def convert_pdf_to_markdown(file_path):
    with open(file_path, 'rb') as pdf_file:
        pdf_content = pdf_file.read()
    markdown_content = marker.convert(pdf_content)
    return markdown_content

def store_markdown_in_mongodb(file_id, markdown_content, db):
    collection = db['markdown_files']
    collection.insert_one({
        "file_id": file_id,
        "markdown_content": markdown_content,
        "timestamp": datetime.utcnow()
    })

def update_file_tracking(file_id, db):
    collection = db[COLLECTION_NAME]
    collection.update_one(
        {"file_id": file_id},
        {"$set": {"FileWasConvertedToMarkdown": True, "conversion_timestamp": datetime.utcnow()}},
        upsert=True
    )

def process_files(service, folder_id, dest_folder, db):
    query = f"'{folder_id}' in parents and trashed=false and mimeType='application/pdf'"
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
        db_entry = db[COLLECTION_NAME].find_one({"file_id": file_id})
        if db_entry and db_entry.get('FileWasConvertedToMarkdown'):
            print(f"File {file_name} already converted to Markdown.")
            continue

        # Download the file
        file_path = download_file(service, file_id, file_name, dest_folder)

        # Convert the file to Markdown
        markdown_content = convert_pdf_to_markdown(file_path)

        # Store the Markdown content in MongoDB
        store_markdown_in_mongodb(file_id, markdown_content, db)

        # Update the file tracking in MongoDB
        update_file_tracking(file_id, db)

def main():
    print(f"Current working directory: {os.getcwd()}")
    print(f"Credentials file path: {CREDENTIALS_FILE}")

    creds = authenticate()
    service = build('drive', 'v3', credentials=creds)

    # Folder ID of the Google Drive folder you want to download from
    folder_id = '1IkFNMb3Qbmrkz_cZA9qDYBnjOu48f7Mx'
    # Destination folder on your local machine
    dest_folder = 'downloaded_files'

    # Connect to MongoDB
    client = MongoClient(MONGO_URI)
    db = client[DB_NAME]

    process_files(service, folder_id, dest_folder, db)

if __name__ == '__main__':
    main()
