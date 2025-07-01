from pymongo import MongoClient
from datetime import datetime

# Connect to MongoDB
client = MongoClient('mongodb://localhost:27017/')
db = client['document_db']
collection = db['documents']


# Example document
document = {
    "document_id": "doc_001",
    "file_name": "example.pdf",
    "file_path": "/path/to/example.pdf",
    "text_content": "",
    "vector": [],
    "status": {
        "FileWasConvertedToText": False,
        "FileWasStoredOnVectorDB": False,
        "FileIsUsedOnRag": False,
        "FileWasUsedOnTraining": False
    },
    "timestamps": {
        "created_at": datetime.now(),
        "updated_at": datetime.now()
    }
}

# Insert document
collection.insert_one(document)


# Update document status
collection.update_one(
    {"document_id": "doc_001"},
    {
        "$set": {
            "text_content": "Extracted text content here...",
            "status.FileWasConvertedToText": True,
            "timestamps.updated_at": datetime.now()
        }
    }
)


# Find all documents that have been converted to text but not stored in the vector database
documents = collection.find({
    "status.FileWasConvertedToText": True,
    "status.FileWasStoredOnVectorDB": False
})

for doc in documents:
    print(doc)
