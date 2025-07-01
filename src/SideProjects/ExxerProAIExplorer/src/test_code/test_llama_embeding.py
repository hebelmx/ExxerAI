import torch
import numpy as np
import os
from transformers import AutoTokenizer, AutoModelForCausalLM
from accelerate import Accelerator

hf_token = "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"
model_id = "meta-llama/Meta-Llama-3-8B-Instruct"
model_id = "meta-llama/Meta-Llama-3B-Instruct"
model_id = "distilgpt2"  # Use a smaller, lightweight model


# Initialize the Accelerator
accelerator = Accelerator()


# Load the tokenizer and model
tokenizer = AutoTokenizer.from_pretrained(model_id, token=hf_token)

# Check if CUDA is available and move model to GPU
device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")
#model.to(device)

# Add a padding token if not already present
if tokenizer.pad_token is None:
    tokenizer.add_special_tokens({'pad_token': '[PAD]'})


# Load the model with automatic device placement and without FlashAttention
model = AutoModelForCausalLM.from_pretrained(
    model_id,
    torch_dtype=torch.float16,  # Use float16 to save memory
    device_map={"": 0},  # Ensure model uses the first GPU (RTX 3060)
    low_cpu_mem_usage=True,  # This might help to manage the memory better
    output_hidden_states=True  # Enable output of hidden states
)

# Ensure model and tokenizer are on the same page regarding padding token
model.resize_token_embeddings(len(tokenizer))
model.config.pad_token_id = tokenizer.pad_token_id



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

# Tokenize the text
inputs = tokenizer(text, return_tensors='pt', truncation=True, padding=True, max_length=512)
inputs = {key: value.to(accelerator.device) for key, value in inputs.items()}  # Ensure inputs are on the right device

# Generate embeddings
#cache = None
#with torch.no_grad():
#    outputs = model(**inputs, use_cache=True, past_key_values=cache)
#    cache = outputs.past_key_values
#    embeddings = outputs.last_hidden_state  # or another relevant output

# Generate embeddings
# Generate embeddings
cache = None
with torch.no_grad():
    outputs = model(**inputs, use_cache=True, past_key_values=cache)
    cache = outputs.past_key_values
    embeddings = outputs.last_hidden_state  # or another relevant output
    
# Convert to numpy and save
embeddings = embeddings.cpu().numpy()
np.save(output_file, embeddings)

# Print a portion of the embeddings to inspect
print("Shape of embeddings:", embeddings.shape)
print("A portion of the embeddings:", embeddings[0][:5])  # Printing first 5 token embeddings of the first sequence

print(f"Embeddings saved to {output_file}")
