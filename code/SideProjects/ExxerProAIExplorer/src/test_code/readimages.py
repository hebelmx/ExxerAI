import fitz  
from PIL import Image

def extract_pdf_section(pdf_path, page_number, bbox, output_image_path, page_bbox):
    document = fitz.open(pdf_path)
    page = document[page_number]
    print(page.rect.width, page_bbox[2])
    for i, coord in enumerate(bbox):
        if i % 2 == 0:
            # y coord
            bbox[i] *= page.rect.height / page_bbox[3]
        else:
            # x coord
            bbox[i] *= page.rect.width / page_bbox[2]
    rect = fitz.Rect(bbox[0],bbox[1],bbox[2],bbox[3])
    pix = page.get_pixmap(clip=rect)
    image = Image.frombytes("RGB", [pix.width, pix.height], pix.samples)
    image.save(output_image_path)