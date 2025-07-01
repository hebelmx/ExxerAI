import os
import fitz  # PyMuPDF
import re
import pandas as pd
from datetime import datetime

# === CONFIGURACIÓN INICIAL ===
ROOT_DIR = ""  # Cambia según sea necesario
OUTPUT_FILE = "documentos_clasificados.xlsx"

# Expresiones regulares para datos clave
REGEX_PATTERNS = {
    "Fecha": r"\b(?:Fecha[:\s]*)(\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2})",
    "Total": r"\b(?:Total[:\s]*\$?\s*)([\d,]+\.\d{2})",
    "TipoDocumento": r"\b(Factura|Cotización|Orden de Compra)\b",
    "FechaTimbrado": r"\b(?:Fecha de timbrado[:\s]*)(\d{2}/\d{2}/\d{4})"
}

# === FUNCIONES ===

def extract_metadata_from_path(filepath):
    """
    Extrae metadatos como año y cliente desde el path.
    Asume estructura tipo ./2024/ClienteA/...
    """
    parts = filepath.split(os.sep)
    try:
        year = next(p for p in parts if p.isdigit() and len(p) == 4)
    except StopIteration:
        year = "Desconocido"
    cliente = parts[-2] if len(parts) >= 2 else "Desconocido"
    return year, cliente

def extract_text_from_pdf(pdf_path):
    """
    Intenta extraer texto del PDF. Retorna el texto y si contiene texto extraíble.
    """
    try:
        with fitz.open(pdf_path) as doc:
            text = ""
            for page in doc:
                text += page.get_text()
        return text.strip(), bool(text.strip())
    except Exception as e:
        return "", False

def parse_fields(text):
    """
    Usa expresiones regulares para extraer los campos definidos.
    """
    result = {}
    notes = []
    for field, pattern in REGEX_PATTERNS.items():
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            result[field] = match.group(1)
        else:
            result[field] = ""
            if field in ["Fecha", "Total"]:
                notes.append(f"⚠ Campo crítico no encontrado: {field}")
    result["Notas"] = "; ".join(notes)
    return result

def scan_pdfs(root_dir):
    """
    Recorre todas las carpetas desde root_dir y procesa PDFs.
    """
    data = []
    for dirpath, _, filenames in os.walk(root_dir):
        for fname in filenames:
            if fname.lower().endswith(".pdf"):
                fpath = os.path.join(dirpath, fname)
                year, cliente = extract_metadata_from_path(fpath)
                text, has_text = extract_text_from_pdf(fpath)
                fields = parse_fields(text)
                fields.update({
                    "Archivo": os.path.basename(fpath),
                    "Ruta": fpath,
                    "Año": year,
                    "Cliente": cliente,
                    "Estatus": "Texto" if has_text else "Escaneado"
                })
                data.append(fields)
    return pd.DataFrame(data)

def export_to_excel(df, output_file):
    """
    Exporta los datos a un archivo Excel con una hoja por año.
    """
    with pd.ExcelWriter(output_file, engine='openpyxl') as writer:
        for year, group in df.groupby("Año"):
            group.to_excel(writer, sheet_name=str(year), index=False)

# === EJECUCIÓN PRINCIPAL ===
if __name__ == "__main__":
    print("Escaneando archivos PDF...")
    df = scan_pdfs(ROOT_DIR)
    export_to_excel(df, OUTPUT_FILE)
    print(f"Exportación completada: {OUTPUT_FILE}")
