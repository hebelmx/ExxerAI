# Adding a new document
add_document("example.pdf", "/path/to/example.pdf")

# Updating document status
update_document_status("example.pdf", "updated")

# Marking document as tokenized
mark_vectorized("example.pdf")

# Marking document as fine-tuned
mark_fine_tuned("example.pdf")

# Checking if a document exists
exists = document_exists("example.pdf")
print(f"Document exists: {exists}")

# Deleting a document
delete_document("example.pdf")
