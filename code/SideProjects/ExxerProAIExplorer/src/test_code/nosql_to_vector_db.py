import numpy as np
from pymongo import MongoClient
from transformers import LlamaTokenizer, LlamaModel
import torch
import pinecone

# Initialize the tokenizer and model from Hugging Face
tokenizer = LlamaTokenizer.from_pretrained('facebook/llama-3b')
model = LlamaModel.from_pretrained('facebook/llama-3b')

class VectorDatabase:
    """Facade for vector database operations."""
    def insert_vectors(self, index_name, vectors):
        raise NotImplementedError

    def query_vectors(self, index_name, query_vector, top_k):
        raise NotImplementedError

class PineconeService(VectorDatabase):
    def __init__(self, api_key):
        pinecone.init(api_key=api_key, environment='us-west1-gcp')
        self.indexes = {}

    def create_index(self, index_name, dimension):
        if index_name not in pinecone.list_indexes():
            pinecone.create_index(index_name, dimension=dimension)
        self.indexes[index_name] = pinecone.Index(index_name)

    def insert_vectors(self, index_name, vectors):
        self.indexes[index_name].upsert(vectors=vectors)

    def query_vectors(self, index_name, query_vector, top_k):
        return self.indexes[index_name].query(queries=[query_vector], top_k=top_k)

def tokenize_text(text):
    """Tokenize text and return the vector."""
    inputs = tokenizer(text, return_tensors="pt", truncation=True, max_length=512)
    with torch.no_grad():
        outputs = model(**inputs)
    return outputs.last_hidden_state.mean(dim=1).numpy().flatten()

# MongoDB setup
mongo_client = MongoClient('mongodb://localhost:27017/')
db = mongo_client['your_database']
collection = db['your_collection']

# Vector database setup
vector_db = PineconeService(api_key='your_pinecone_api_key')
vector_db.create_index('your_index_name', dimension=1024)

# Fetch and process documents
documents = collection.find({})

for doc in documents:
    if doc.get('StoredTokenizedDocumentOnVectorDB'):
        continue  # Skip already processed documents

    text = doc['text_field']
    vector = tokenize_text(text)
    vector_id = str(doc['_id'])

    # Insert the vector into the vector database
    vector_db.insert_vectors('your_index_name', [(vector_id, vector.tolist())])

    # Update the MongoDB document to reflect the status
    collection.update_one({'_id': doc['_id']}, {'$set': {'StoredTokenizedDocumentOnVectorDB': True}})

print("Data has been processed and vectors are stored with status updated in MongoDB.")
