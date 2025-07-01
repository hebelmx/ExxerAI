from llama_index.embeddings.instructor import InstructorEmbedding

embed_model = InstructorEmbedding(model_name="hkunlp/instructor-base")

embeddings = embed_model.get_text_embedding("Hello World!")
print(len(embeddings))
print(embeddings[:5])