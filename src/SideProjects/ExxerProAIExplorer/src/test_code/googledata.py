import os
import io
import pickle
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseDownload

# Define the scopes
SCOPES = ['https://www.googleapis.com/auth/drive']

# Path to your credentials file
CREDENTIALS_FILE = os.path.join(os.path.dirname(__file__), 'google.json')
TOKEN_FILE = os.path.join(os.path.dirname(__file__), 'token.pickle')

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

def download_files(service, folder_id, dest_folder):
    query = f"'{folder_id}' in parents and trashed=false"
    results = service.files().list(q=query, fields="files(id, name)").execute()
    items = results.get('files', [])

    if not items:
        print('No files found.')
        return

    if not os.path.exists(dest_folder):
        os.makedirs(dest_folder)

    for item in items:
        request = service.files().get_media(fileId=item['id'])
        fh = io.FileIO(os.path.join(dest_folder, item['name']), 'wb')
        downloader = MediaIoBaseDownload(fh, request)
        done = False
        while not done:
            status, done = downloader.next_chunk()
            print(f"Downloading {item['name']}: {int(status.progress() * 100)}% complete.")

def main():
    print(f"Current working directory: {os.getcwd()}")
    print(f"Credentials file path: {CREDENTIALS_FILE}")

    creds = authenticate()
    service = build('drive', 'v3', credentials=creds)

    # Folder ID of the Google Drive folder you want to download from
    folder_id = '1IkFNMb3Qbmrkz_cZA9qDYBnjOu48f7Mx' # '1IkFNMb3Qbmrkz_cZA9qDYBnjOu48f7Mx' is the ID of the folder 'drives' in the shared drive
    # Destination folder on your local machine
    dest_folder = 'downloaded_files'

    download_files(service, folder_id, dest_folder)

if __name__ == '__main__':
    main()
