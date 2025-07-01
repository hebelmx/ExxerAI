import torch
import numpy as np
import os
from transformers import AutoTokenizer, AutoModelForCausalLM
from accelerate import Accelerator
import chromadb
from chromadb.config import Settings
#import logging

# Set up logging
#logging.basicConfig(level=logging.WARNING)

hf_token = "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"
model_id = "distilgpt2"  # Use a smaller, lightweight model
model_id = "sentence-transformers/all-MiniLM-L6-v2"

# Initialize the Accelerator
accelerator = Accelerator()

# Load the tokenizer and model
tokenizer = AutoTokenizer.from_pretrained(model_id, token=hf_token)

# Add a padding token if not already present
if tokenizer.pad_token is None:
    tokenizer.add_special_tokens({'pad_token': '[PAD]'})

# Load the model with automatic device placement
model = AutoModelForCausalLM.from_pretrained(
    model_id,
    torch_dtype=torch.float16,  # Use float16 to save memory
    output_attentions=False,  # Disable attention outputs
    output_hidden_states=True,  # Enable hidden states outputs
)

# Ensure model and tokenizer are on the same page regarding padding token
model.resize_token_embeddings(len(tokenizer))
model.config.pad_token_id = tokenizer.pad_token_id

# Prepare the model with the accelerator
model = accelerator.prepare(model)

# Check the platform and assign the appropriate data directory
if os.name == 'nt':  # 'nt' indicates a Windows environment
    data_dir = r'D:\Projects\ExxerProAIExplorer\ExxerProAIExplorer\src\data'
else:  # Assume any non-Windows OS is Linux or similar (like MacOS)
    data_dir = r'/home/abel/projects/ExxerProAIExplorer/src/data'
    
text_file = os.path.join(data_dir, 'donquijote.txt')
output_file = os.path.join(data_dir, 'donquijote_embeddings.npy')

# Read the text file
with open(text_file, 'r', encoding='utf-8') as file:
    text = file.read()

chunk_size = 1500
chunk_overlap = 100

# Tokenize the text
inputs = tokenizer(text, return_tensors='pt', truncation=True, padding=True, max_length=512)
inputs = {key: value.to(accelerator.device) for key, value in inputs.items()}  # Ensure inputs are on the right device

# Generate embeddings
with torch.no_grad():
    outputs = model(**inputs)
    hidden_states = outputs.hidden_states  # Retrieve the hidden states

# Select the last hidden state (assuming this is what you need)
embeddings = hidden_states[-1]

# Convert to numpy and save (optional)
embeddings = embeddings.cpu().numpy()
np.save(output_file, embeddings)

# Print a portion of the embeddings to inspect (optional)
print("Shape of embeddings:", embeddings.shape)
print("A portion of the embeddings:", embeddings[0][:5])  # Printing first 5 token embeddings of the first sequence

print(f"Embeddings saved to {output_file}")

# Connect to the local ChromaDB instance (default in-memory settings)
client = chromadb.Client(Settings())
collection = client.create_collection("embeddings_collection")


# Prepare embeddings for insertion
documents = text.split('.')  # Assuming each sentence as a document for example
batch_size = 100
for i in range(0, len(documents), batch_size):
    batch_docs = documents[i:i+batch_size]
    batch_embeddings = embeddings[i:i+batch_size]
    #logging.info(f"Inserting batch {i//batch_size + 1}")
    collection.add(
        embeddings=batch_embeddings.tolist(),
        metadatas=[{"text": doc.strip()} for doc in batch_docs],
        ids=[f"doc_{i+j}" for j in range(len(batch_docs))]
    )

print("Embeddings saved to ChromaDB")

# results = collection.query(
#     query_texts=["Quijote"],  # Chroma will embed this for you
#     n_results=2  # how many results to return
# )
#print(results)
