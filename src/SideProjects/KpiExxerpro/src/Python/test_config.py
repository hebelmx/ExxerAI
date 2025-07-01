import os
import sys

print("=== PRUEBA DE CONFIGURACIÓN ===")

# Verificar Tesseract
try:
    import pytesseract
    print(f"✓ pytesseract importado correctamente")
    print(f"  Ruta configurada: {pytesseract.pytesseract.tesseract_cmd}")
    
    # Verificar si Tesseract está instalado
    try:
        version = pytesseract.get_tesseract_version()
        print(f"✓ Tesseract encontrado, versión: {version}")
    except Exception as e:
        print(f"✗ Error al obtener versión de Tesseract: {e}")
        print(f"  Verificando si el archivo existe...")
        if os.path.exists(pytesseract.pytesseract.tesseract_cmd):
            print(f"  ✓ El archivo tesseract.exe existe")
        else:
            print(f"  ✗ El archivo tesseract.exe NO existe en la ruta especificada")
        
except ImportError as e:
    print(f"✗ Error al importar pytesseract: {e}")

# Verificar Poppler
POPPLER_PATH = r"C:\Program Files\poppler\Library\bin"
if os.path.exists(POPPLER_PATH):
    print(f"✓ Poppler encontrado en: {POPPLER_PATH}")
    # Verificar archivos específicos de Poppler
    poppler_files = ["pdftoppm.exe", "pdftotext.exe"]
    for file in poppler_files:
        file_path = os.path.join(POPPLER_PATH, file)
        if os.path.exists(file_path):
            print(f"  ✓ {file} encontrado")
        else:
            print(f"  ✗ {file} NO encontrado")
else:
    print(f"✗ Poppler no encontrado en: {POPPLER_PATH}")

# Verificar PyMuPDF
try:
    import fitz
    print(f"✓ PyMuPDF (fitz) importado correctamente")
except ImportError as e:
    print(f"✗ Error al importar PyMuPDF: {e}")

# Verificar pdf2image
try:
    from pdf2image import convert_from_path
    print(f"✓ pdf2image importado correctamente")
except ImportError as e:
    print(f"✗ Error al importar pdf2image: {e}")

# Verificar PIL
try:
    from PIL import Image
    print(f"✓ PIL (Pillow) importado correctamente")
except ImportError as e:
    print(f"✗ Error al importar PIL: {e}")

# Verificar archivo Excel de entrada
EXCEL_INPUT = "resumen_pagos_seguros.xlsx"
if os.path.exists(EXCEL_INPUT):
    print(f"✓ Archivo Excel de entrada encontrado: {EXCEL_INPUT}")
else:
    print(f"✗ Archivo Excel de entrada NO encontrado: {EXCEL_INPUT}")

# Verificar carpeta Fixture
FIXTURE_PATH = r"C:\Users\moren\Documents\GitHub\KpiExxerpro\Fixture"
if os.path.exists(FIXTURE_PATH):
    print(f"✓ Carpeta Fixture encontrada: {FIXTURE_PATH}")
else:
    print(f"✗ Carpeta Fixture NO encontrada: {FIXTURE_PATH}")

print("\n=== FIN DE PRUEBA ===") 