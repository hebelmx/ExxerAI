import nest_asyncio

nest_asyncio.apply()

hf_token = "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"

import os

os.environ["GROQ_API_KEY"] = "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"

from llama_index.llms.groq import Groq

llm = Groq(model="llama3-8b-8192")
llm_70b = Groq(model="llama3-70b-8192")

from llama_index.embeddings.huggingface import HuggingFaceEmbedding

embed_model = HuggingFaceEmbedding(model_name="BAAI/bge-small-en-v1.5")

from llama_index.core import Settings

Settings.llm = llm
Settings.embed_model = embed_model

from llama_parse import LlamaParse

docs_kendrick = LlamaParse(result_type="text").load_data("./rags/data/kendrick.pdf")
docs_drake = LlamaParse(result_type="text").load_data("./rags/data/drake.pdf")
docs_both = LlamaParse(result_type="text").load_data("./rags/data/drake_kendrick_beef.pdf"
)


# from llama_index.core import SimpleDirectoryReader

# docs_kendrick = SimpleDirectoryReader(input_files=["data/kendrick.pdf"]).load_data()
# docs_drake = SimpleDirectoryReader(input_files=["data/drake.pdf"]).load_data()
# docs_both = SimpleDirectoryReader(input_files=["data/drake_kendrick_beef.pdf"]).load_data()

stream_response = llm.stream_complete(
    "you're a drake fan. tell me why you like drake more than kendrick"
)

for t in stream_response:
    print(t.delta, end="")
