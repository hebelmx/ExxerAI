import os
import re
import fitz  # PyMuPDF
import pandas as pd
from pathlib import Path

# Inicializa el diccionario por año
data_by_year = {}

# Directorio base que contiene carpetas por año
BASE_DIR = Path(r"C:/Users/natal/OneDrive/Documentos/KPI_ABEL/2020")  # Puedes cambiar esto según donde esté tu carpeta raiz
# Recorrer todos los archivos PDF
for root, dirs, files in os.walk(BASE_DIR):
    for file in files:
        if file.endswith(".pdf"):
            file_path = Path(root) / file
            parts = file_path.parts

            # Extraer metadatos de ruta
            try:
                year, client, folio = parts[-4], parts[-3], parts[-2]
            except ValueError:
                continue  # Saltar si no sigue la estructura esperada

            # Leer PDF
            try:
                doc = fitz.open(file_path)
                text = "\n".join(page.get_text() for page in doc)
            except Exception as e:
                print(f"Error leyendo {file_path}: {e}")
                continue

            # Buscar datos claves
            status = "Cancelada" if "cancelada" in text.lower() else "Confirmada"
            date_match = re.search(r"Fecha\s+([\d\-T:]+)", text)
            date = date_match.group(1).split("T")[0] if date_match else ""
            order_match = re.search(r"Orden De Compra\s+(\d+)", text)
            order = order_match.group(1) if order_match else ""
            desc_match = re.search(r"Descripcion\s+(.*?)\n", text, re.IGNORECASE)
            description = desc_match.group(1).strip() if desc_match else ""
            amount_match = re.search(r"Total \$(\d{1,3}(,\d{3})*(\.\d{2})?)", text)
            amount = amount_match.group(1).replace(",", "") if amount_match else ""

            record = {
                "File Path": str(file_path),
                "Client": client,
                "Folio": folio,
                "Date": date,
                "Order": order,
                "Description": description,
                "Amount (MXN)": amount,
                "Status": status
            }

            if year not in data_by_year:
                data_by_year[year] = []
            data_by_year[year].append(record)

# Crear un archivo Excel con una hoja por año
with pd.ExcelWriter("facturas_clasificadas.xlsx") as writer:
    for year, records in data_by_year.items():
        df = pd.DataFrame(records)
        df.to_excel(writer, sheet_name=str(year), index=False)

print("Procesamiento completo. Archivo 'facturas_clasificadas.xlsx' generado.")
