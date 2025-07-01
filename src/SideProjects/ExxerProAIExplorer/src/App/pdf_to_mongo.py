import os
import hashlib
from datetime import datetime
from pymongo import MongoClient
from marker.convert import convert_single_pdf
from marker.models import load_all_models
from PIL import Image

# MongoDB configuration
MONGO_URI = 'mongodb://localhost:27017/'
DB_NAME = 'file_tracking'
COLLECTION_NAME = 'files'

def get_file_hash(content):
    """
    Compute and return the MD5 hash of the given file content.
    """
    hasher = hashlib.md5()
    hasher.update(content)
    return hasher.hexdigest()

def convert_pdf_to_markdown(file_path, model_lst):
    """
    Convert a PDF file to Markdown format using the marker library.
    """
    full_text, images, out_meta = convert_single_pdf(file_path, model_lst)
    return full_text, images, out_meta

def save_images(images, output_dir, file_id):
    """
    Save images to the output directory and return the paths.
    """
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    
    image_paths = []
    for i, (image_name, image) in enumerate(images.items()):
        image_path = os.path.join(output_dir, f"{file_id}_image_{i}.png")
        image.save(image_path)
        image_paths.append(image_path)
    return image_paths

def store_in_mongodb(file_id, file_name, markdown_content, image_paths, metadata, db):
    """
    Store the converted Markdown, image paths, and metadata in MongoDB.
    """
    collection = db['markdown_files']
    collection.insert_one({
        "file_id": file_id,
        "file_name": file_name,
        "markdown_content": markdown_content,
        "image_paths": image_paths,
        "metadata": metadata,
        "timestamp": datetime.utcnow()
    })

def update_file_status(file_id, db):
    """
    Update the status of the file in MongoDB to indicate Markdown storage.
    """
    collection = db[COLLECTION_NAME]
    collection.update_one(
        {"file_id": file_id},
        {"$set": {"StoredMarkDownOnMongoDB": True, "storage_timestamp": datetime.utcnow()}},
        upsert=True
    )

def process_local_pdfs(directory, db, image_output_dir):
    """
    Traverse a local directory, find all PDF documents, convert them to Markdown, 
    and store the results in MongoDB.
    """
    model_lst = load_all_models()
    
    for root, dirs, files in os.walk(directory):
        for file_name in files:
            if file_name.lower().endswith('.pdf'):
                file_path = os.path.join(root, file_name)
                
                # Compute local file hash
                with open(file_path, 'rb') as f:
                    file_content = f.read()
                local_hash = get_file_hash(file_content)
                
                # Check if file is already processed or has changed
                db_entry = db[COLLECTION_NAME].find_one({"local_hash": local_hash})
                if db_entry and db_entry.get('StoredMarkDownOnMongoDB'):
                    print(f"File {file_name} already processed and stored in MongoDB.")
                    continue
                
                # Convert PDF to Markdown
                markdown_content, images, metadata = convert_pdf_to_markdown(file_path, model_lst)
                
                # Save images locally
                file_id = db_entry['_id'] if db_entry else local_hash
                image_paths = save_images(images, image_output_dir, file_id)
                
                # Store results in MongoDB
                store_in_mongodb(file_id, file_name, markdown_content, image_paths, metadata, db)
                
                # Update file status in MongoDB
                update_file_status(file_id, db)
                print(f"Processed and stored file {file_name}.")

def main():
    # Connect to MongoDB
    client = MongoClient(MONGO_URI)
    db = client[DB_NAME]

    # Local directory containing PDF files
    directory = '/home/abel/projects/ExxerProAIExplorer/src/data/'
    
    # Directory to save images
    image_output_dir = '/home/abel/projects/ExxerProAIExplorer/src/data/'

    # Process local PDFs
    process_local_pdfs(directory, db, image_output_dir)

if __name__ == '__main__':
    main()
