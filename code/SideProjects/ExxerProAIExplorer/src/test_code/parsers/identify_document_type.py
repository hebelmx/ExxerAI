import os

def identify_document_type(file_path):
    file_extension = os.path.splitext(file_path)[1].lower()
    
    if file_extension in ['.txt']:
        return 'text'
    elif file_extension in ['.pdf']:
        return 'pdf'
    elif file_extension in ['.doc', '.docx']:
        return 'word'
    elif file_extension in ['.xlsx']:
        return 'excel'
    elif file_extension in ['.png', '.jpg', '.jpeg', '.tiff']:
        return 'image'
    elif file_extension in ['.xml']:
        return 'xml'
    elif file_extension in ['.dwg']:
        return 'dwg'
    elif file_extension in ['.bin']:  # Assuming semi-binary formats
        return 'binary'
    # Add more extensions and corresponding types as needed
    else:
        return 'unknown'
