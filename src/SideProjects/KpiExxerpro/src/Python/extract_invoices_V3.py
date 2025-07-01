# === LIBRERIAS NECESARIAS ===
import os  # Para navegar por carpetas
import re  # Para buscar texto con patrones
import fitz  # Librería PyMuPDF para leer archivos PDF
import pandas as pd  # Para crear hojas de cálculo Excel
from pathlib import Path  # Para manejar rutas de archivos de forma más cómoda

# === CONFIGURACION ===
# Aquí indicamos la carpeta principal donde están guardados los archivos PDF
# Cambia esta ruta si tus archivos están en otra carpeta
BASE_DIR = Path(r"C:/Users/natal/OneDrive/Documentos/KPI_ABEL/2020")
OUTPUT_FILE = "facturas_clasificadas.xlsx"  # Nombre del archivo Excel de salida

# Diccionario para guardar la información separada por año
data_by_year = {}

# === FUNCIONES AUXILIARES ===
# Esta función busca una palabra clave y devuelve el texto que aparece justo en la siguiente línea
# Por ejemplo: si el PDF tiene "Forma de pago\nTransferencia", devuelve "Transferencia"
def find_line_value(text, key):
    pattern = rf"{key}\s*\n(.*)"
    match = re.search(pattern, text, re.IGNORECASE)
    return match.group(1).strip() if match else ""

# Esta función intenta varios patrones de búsqueda hasta encontrar el que funcione
# Sirve para casos donde el formato puede variar
def get_field_by_regex(text, patterns):
    for pat in patterns:
        match = re.search(pat, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# === LECTURA DE ARCHIVOS PDF ===
# Este bloque recorre todas las carpetas y subcarpetas para encontrar archivos PDF
for root, dirs, files in os.walk(BASE_DIR):
    for file in files:
        if file.endswith(".pdf"):
            file_path = Path(root) / file  # Ruta completa del archivo PDF
            parts = file_path.parts  # Divide la ruta en partes (año, cliente, folio, etc.)

            try:
                year, client, folio = parts[-4], parts[-3], parts[-2]  # Extrae información desde la ruta
            except ValueError:
                continue  # Si la ruta no tiene el formato esperado, la salta

            # Abre el archivo PDF y convierte todo el texto en una sola cadena
            try:
                doc = fitz.open(file_path)
                text = "\n".join(page.get_text() for page in doc)
            except Exception as e:
                print(f"Error leyendo {file_path}: {e}")
                continue

            # Clasifica el tipo de documento según palabras clave
            tipo_doc = "Factura"
            if "comprobante de pago" in text.lower():
                tipo_doc = "Comprobante de Pago"
            elif "cancelada" in text.lower():
                tipo_doc = "Cancelada"

            # Determina si está cancelada o confirmada
            status = "Cancelada" if "cancelada" in text.lower() else "Confirmada"

            # Extrae datos usando funciones que buscan con diferentes patrones posibles
            date = get_field_by_regex(text, [r"Fecha\s+(\d{4}-\d{2}-\d{2})", r"Fecha\s+(\d{4}-\d{2}-\d{2})T"])
            date_stamp = get_field_by_regex(text, [r"Fecha timbrado\s+(\d{4}-\d{2}-\d{2})", r"Fecha timbrado\s+(\d{4}-\d{2}-\d{2})T"])
            order = get_field_by_regex(text, [r"Orden De Compra\s+(\d+)"])
            subtotal = get_field_by_regex(text, [r"Subtotal\s*\$([\d,\.]+)"])
            iva = get_field_by_regex(text, [r"IVA.*?\$([\d,\.]+)"])
            total_matches = re.findall(r"Total\s*\$([\d,\.]+)", text)  # Busca todos los "Total $..." y usa el último
            total = total_matches[-1] if total_matches else ""
            uso_cfdi = get_field_by_regex(text, [r"Uso (?:del )?CFDI\s*[:\-]?\s*(\w+)"])
            metodo_pago = find_line_value(text, "Método de pago")
            forma_pago = find_line_value(text, "Forma de pago")
            rfc = get_field_by_regex(text, [r"RFC\s+([A-Z0-9]{12,13})", r"\b([A-Z&Ñ]{3,4}\d{6}[A-Z0-9]{3})\b"])

            # Captura la descripción del servicio aunque esté en varias líneas
            desc_match = re.search(r"SOPORTE.*?(?=\n\s*\d{4}|Subtotal|\nIVA|\nTotal|\nForma|\nCondiciones|$)", text, re.IGNORECASE | re.DOTALL)
            description = desc_match.group(0).strip() if desc_match else ""

            # Guarda toda la información como un registro (fila) de Excel
            record = {
                "File Path": str(file_path),
                "Tipo Documento": tipo_doc,
                "Cliente": client,
                "Folio": folio,
                "Fecha": date,
                "Fecha Timbrado": date_stamp,
                "Orden de Compra": order,
                "Descripcion": description,
                "Subtotal (MXN)": subtotal.replace(",", "") if subtotal else "",
                "IVA (MXN)": iva.replace(",", "") if iva else "",
                "Total (MXN)": total.replace(",", "") if total else "",
                "Forma de Pago": forma_pago,
                "Metodo de Pago": metodo_pago,
                "Uso CFDI": uso_cfdi,
                "RFC Receptor": rfc,
                "Estatus": status
            }

            # Agrega el registro al año correspondiente en el diccionario
            if year not in data_by_year:
                data_by_year[year] = []
            data_by_year[year].append(record)

# === CREACION DE EXCEL FINAL ===
# Crea el archivo Excel con una hoja por cada año
with pd.ExcelWriter(OUTPUT_FILE) as writer:
    for year, records in data_by_year.items():
        df = pd.DataFrame(records)
        df.to_excel(writer, sheet_name=str(year), index=False)

# Mensaje final cuando termina todo el proceso
print(f"Procesamiento completo. Archivo '{OUTPUT_FILE}' generado.")
