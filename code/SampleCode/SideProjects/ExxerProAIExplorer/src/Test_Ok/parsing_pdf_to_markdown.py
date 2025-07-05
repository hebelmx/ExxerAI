#This test works

import os
import pypdfium2
import marker
import marker.models
from marker.convert import convert_single_pdf
from marker.logger import configure_logging
from marker.models import load_all_models
from marker.output import save_markdown
from concurrent.futures import ProcessPoolExecutor, as_completed

def process_pdf(file_path):
    """
    Process a single PDF file to convert it to Markdown.

    Args:
        file_path (str): Path to the PDF file.

    Returns:
        tuple: Full text, images, and metadata from the converted PDF.
    """
      # langs is optional list of languages to prune from recognition MoE model
    # detection = setup_detection_model(device, dtype)
    # layout = setup_layout_model(device, dtype)
    # order = setup_order_model(device, dtype)
    # edit = load_editing_model(device, dtype)

    # # Only load recognition model if we'll need it for all pdfs
    # ocr = setup_recognition_model(langs, device, dtype)
    # texify = setup_texify_model(device, dtype)
    # model_lst = [texify, layout, order, edit, detection, ocr]

    model_lst = load_all_models()    
    full_text, images, out_meta = convert_single_pdf(file_path, model_lst)
    return full_text, images, out_meta


if __name__ == '__main__':
    #freeze_support()

    # For some reason, transformers decided to use .isin for a simple op, which is not supported on MPS
    os.environ["PYTORCH_ENABLE_MPS_FALLBACK"] = "1"

   # Example usage with a single file
    pdf_files = "/home/abel/projects/ExxerProAIExplorer/src/data/kendrick.pdf"
    

 
    full_text, images, out_meta = process_pdf(pdf_files)

    print("Full text: ", full_text)
    print("Images: ", images)
    print("Out meta: ", out_meta)
    
  