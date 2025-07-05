# IMPORTANTE: Instalar requerimientos con
# pip install pymupdf pandas openpyxl pytesseract pillow
# Adicionalmente, instalar Tesseract-OCR desde https://github.com/tesseract-ocr/tesseract

import os
import re
import fitz  # PyMuPDF
import pandas as pd
from datetime import datetime
from PIL import Image
import pytesseract
import io

pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"

# === CONFIGURACIÓN GENERAL ===
ROOT_DIR = r"C:\Users\Latitude5500\Documents\PROYECTO KPI\PROYECTO KPI\1. Ordenes de Compra"
OUTPUT_FILE = f"documentos_clasificados_orden_de_compra.xlsx"
DOCUMENT_TYPE = "orden de compra"

# === EXPRESIONES REGULARES MEJORADAS ===
REGEX_PATTERNS = {
    "numero_orden": [
        r"P\d{4}_\d{2}_\d{3,4}",
        r"fr-adm-cm-02_ordendecompra\s+([A-Z0-9\-_]+)",
        r"OC\s+P\d{4}_\d{2}_\d{3,4}",
        r"ORDEN\s+DE\s+COMPRA\s+N°\s*:?\s*([A-Z0-9\-_]+)",
        r"ORDEN\s+DE\s+COMPRA\s+NO\.?\s*:?\s*([A-Z0-9\-_]+)",
        r"NO\.?\s+DE\s+ORDEN:\s*([A-Z0-9\-_]+)",
        r"ORDEN\s+NO\.?\s*:?\s*([A-Z0-9\-_]+)",
        r"PURCHASE\s+ORDER\s+NO\.?\s*:?\s*([A-Z0-9\-_]+)",
        r"PURCHASE\s+ORDER\s+#\s*([A-Z0-9\-_]+)",
        r"ORDER\s+#\s*([A-Z0-9\-_]+)",
        r"PO\s*#\s*([A-Z0-9\-_]+)",
        r"OC\s*#\s*([A-Z0-9\-_]+)",
        r"NUMERO\s+DE\s+ORDEN:\s*([A-Z0-9\-_]+)",
        r"FOLIO:\s*([A-Z0-9\-_]+)",
        r"REFERENCIA:\s*([A-Z0-9\-_]+)"
    ],
    "fecha": [
        r"FECHA\s*:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"FECHA\s+DE\s+ORDEN:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"FECHA:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"ORDEN\s+FECHA:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"DATE:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"FECHA\s+DE\s+EMISIÓN:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"FECHA\s+DE\s+COMPRA:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"FECHA\s+DE\s+EXPEDICIÓN:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"(\d{4}[/-]\d{1,2}[/-]\d{1,2})"
    ],
    "proveedor": [
        r"PROVEEDOR:\s*([A-ZÁÉÍÓÚÑ\s]+)",
        r"SUPPLIER:\s*([A-ZÁÉÍÓÚÑ\s]+)",
        r"VENDOR:\s*([A-ZÁÉÍÓÚÑ\s]+)",
        r"EMPRESA:\s*([A-ZÁÉÍÓÚÑ\s]+)",
        r"COMPAÑÍA:\s*([A-ZÁÉÍÓÚÑ\s]+)",
        r"RAZÓN\s+SOCIAL:\s*([A-ZÁÉÍÓÚÑ\s]+)",
        r"NOMBRE\s+DEL\s+PROVEEDOR:\s*([A-ZÁÉÍÓÚÑ\s]+)",
        r"PROVEEDOR\s+SERVICIO:\s*([A-ZÁÉÍÓÚÑ\s]+)"
    ],
    "rfc": [
        r"R\.F\.C\.\s*([A-Z]{3,4}\d{6}[A-Z\d]{3})",
        r"RFC\s*([A-Z]{3,4}\d{6}[A-Z\d]{3})",
        r"R\.F\.C\.\s*[:\s]+([A-Z]{3,4}\d{6}[A-Z\d]{3})",
        r"RFC[:\s]+([A-Z]{3,4}\d{6}[A-Z\d]{3})",
        r"REGISTRO\s+FEDERAL\s+DE\s+CONTRIBUYENTES[:\s]+([A-Z]{3,4}\d{6}[A-Z\d]{3})"
    ],
    "subtotal": [
        r"SUBTOTAL\s*\$?\s*([\d,]+\.?\d*)",
        r"SUB-TOTAL\s*\$?\s*([\d,]+\.?\d*)",
        r"SUBTOTAL\s+[A-Z\s]*:\s*\$?\s*([\d,]+\.?\d*)",
        r"SUB\s+TOTAL\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+PARCIAL\s*\$?\s*([\d,]+\.?\d*)",
        r"SUMA\s+PARCIAL\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+SIN\s+IVA\s*\$?\s*([\d,]+\.?\d*)"
    ],
    "iva": [
        r"I\.V\.A\.\s*\$?\s*([\d,]+\.?\d*)",
        r"IVA\s*\$?\s*([\d,]+\.?\d*)",
        r"IMPUESTO\s*\$?\s*([\d,]+\.?\d*)",
        r"TAX\s*\$?\s*([\d,]+\.?\d*)",
        r"IMPUESTO\s+AL\s+VALOR\s+AGREGADO\s*\$?\s*([\d,]+\.?\d*)",
        r"16%\s*\$?\s*([\d,]+\.?\d*)",
        r"16\s*%\s*\$?\s*([\d,]+\.?\d*)",
        r"IVA\s+16%\s*\$?\s*([\d,]+\.?\d*)"
    ],
    "total": [
        r"TOTAL\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+A\s+PAGAR\s*\$?\s*([\d,]+\.?\d*)",
        r"IMPORTE\s+TOTAL\s*\$?\s*([\d,]+\.?\d*)",
        r"GRAND\s+TOTAL\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+AMOUNT\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+FINAL\s*\$?\s*([\d,]+\.?\d*)",
        r"AMOUNT\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+GENERAL\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+DE\s+LA\s+ORDEN\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+DE\s+COMPRA\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+A\s+PAGAR\s+[A-Z\s]*:\s*\$?\s*([\d,]+\.?\d*)"
    ]
}

# === FUNCIONES DE UTILIDAD ===
def extract_text_from_pdf(path):
    """Extrae texto del PDF usando PyMuPDF y OCR si es necesario. Devuelve el texto y el método usado."""
    try:
        doc = fitz.open(path)
        full_text = ""
        metodo = "fitz"
        for page in doc:
            text = ""
            try:
                text = page.get_text("text").strip()
            except Exception as e:
                text = ""
            if len(text) >= 50:
                full_text += text + "\n"
            else:
                # Si el texto es corto, usar OCR
                pix = page.get_pixmap(dpi=300)
                img = Image.open(io.BytesIO(pix.tobytes("png")))
                ocr_text = pytesseract.image_to_string(img, lang='spa+eng')
                full_text += ocr_text + "\n"
                metodo = "ocr" if metodo == "fitz" else "mixto"
        doc.close()
        return full_text, metodo
    except Exception as e:
        return f"ERROR: {e}", "error"

def is_orden_compra(filename):
    """Verifica si el archivo es una orden de compra basándose en el nombre"""
    filename_lower = filename.lower()
    orden_indicators = [
        "orden de compra",
        "purchase order",
        "oc p",
        "p20",
        "fr-adm-cm-02_ordendecompra",
        "solicitud de compra"
    ]
    return any(indicator in filename_lower for indicator in orden_indicators)

def extract_field_with_patterns(text, patterns):
    """Extrae un campo usando múltiples patrones"""
    for pattern in patterns:
        matches = re.findall(pattern, text, re.IGNORECASE)
        if matches:
            return matches[0].strip()
    return ""

def extract_monetary_values(text):
    """Extrae valores monetarios de manera más robusta"""
    monetary_patterns = [
        r'\$?\s*([\d,]+\.?\d*)',
        r'([\d,]+\.?\d*)\s*MXP',
        r'([\d,]+\.?\d*)\s*USD',
        r'([\d,]+\.?\d*)\s*PESOS'
    ]
    values = []
    for pattern in monetary_patterns:
        matches = re.findall(pattern, text, re.IGNORECASE)
        for match in matches:
            try:
                clean_value = match.replace(',', '')
                value = float(clean_value)
                values.append(value)
            except ValueError:
                continue
    return values

def extract_numero_orden_from_filename(filename):
    patterns = [
        r"P\d{4}_\d{2}_\d{3,4}",
        r"OC\s+P\d{4}_\d{2}_\d{3,4}",
        r"fr-adm-cm-02_ordendecompra\s+([A-Z0-9\-_]+)",
        r"P\d{4}_\d{2}_\d{3,4}"
    ]
    for pattern in patterns:
        match = re.search(pattern, filename, re.IGNORECASE)
        if match:
            return match.group(0)
    return ""

def extract_proveedor_from_filename(filename):
    filename_clean = filename.replace('.pdf', '').replace('.PDF', '')
    patterns = [
        r"([A-ZÁÉÍÓÚÑ\s]+)\.pdf$",
        r"([A-ZÁÉÍÓÚÑ\s]+)\s*$",
        r"([A-ZÁÉÍÓÚÑ\s]+)\s*CANCELADA?$",
        r"([A-ZÁÉÍÓÚÑ\s]+)\s*CANCELADO?$"
    ]
    for pattern in patterns:
        match = re.search(pattern, filename_clean, re.IGNORECASE)
        if match:
            proveedor = match.group(1).strip()
            if len(proveedor) > 2 and proveedor not in ['PDF', 'ORDEN', 'COMPRA', 'PURCHASE', 'ORDER']:
                return proveedor.title()
    return ""

def extract_fecha_from_filename(filename):
    date_patterns = [
        r"(\d{4}_\d{2}_\d{2})",
        r"(\d{2}_\d{2}_\d{4})",
        r"(\d{4}-\d{2}-\d{2})",
        r"(\d{2}-\d{2}-\d{4})",
    ]
    for pattern in date_patterns:
        match = re.search(pattern, filename)
        if match:
            date_str = match.group(1)
            try:
                if len(date_str.split('_')[0]) == 4:
                    year, month, day = date_str.split('_')
                else:
                    day, month, year = date_str.split('_')
                return f"{day}/{month}/{year}"
            except:
                continue
    return ""

def parse_document_data(file_path, text, metodo):
    data = {
        "Archivo": os.path.basename(file_path),
        "Ruta": file_path,
        "Tipo": DOCUMENT_TYPE,
        "Numero_Orden": "",
        "Fecha": "",
        "Proveedor": "",
        "RFC": "",
        "Subtotal": "",
        "IVA": "",
        "Total": "",
        "Año": "",
        "Mes": "",
        "Estatus": "",
        "Notas": "",
        "Método_Extracción": metodo,
        "Fecha_Procesamiento": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    }
    path_parts = file_path.split(os.sep)
    for i, part in enumerate(path_parts):
        if part.isdigit() and len(part) == 4:
            data["Año"] = part
            if i + 1 < len(path_parts):
                mes_part = path_parts[i + 1]
                mes_match = re.search(r"(\d+).\s*([A-Z]+)", mes_part, re.IGNORECASE)
                if mes_match:
                    data["Mes"] = mes_match.group(2).title()
                else:
                    data["Mes"] = mes_part
            break
    for field, patterns in REGEX_PATTERNS.items():
        value = extract_field_with_patterns(text, patterns)
        if value:
            if field == "numero_orden":
                data["Numero_Orden"] = value
            elif field == "fecha":
                data["Fecha"] = value
            elif field == "proveedor":
                data["Proveedor"] = value
            elif field == "rfc":
                data["RFC"] = value
            elif field == "subtotal":
                try:
                    data["Subtotal"] = float(value.replace(",", ""))
                except:
                    data["Subtotal"] = value
            elif field == "iva":
                try:
                    data["IVA"] = float(value.replace(",", ""))
                except:
                    data["IVA"] = value
            elif field == "total":
                try:
                    data["Total"] = float(value.replace(",", ""))
                except:
                    data["Total"] = value
    if not data["Numero_Orden"]:
        data["Numero_Orden"] = extract_numero_orden_from_filename(data["Archivo"])
        if data["Numero_Orden"]:
            data["Notas"] = "Número de orden extraído del nombre del archivo. "
    if not data["Proveedor"]:
        data["Proveedor"] = extract_proveedor_from_filename(data["Archivo"])
        if data["Proveedor"]:
            data["Notas"] += "Proveedor extraído del nombre del archivo. "
    if not data["Fecha"]:
        data["Fecha"] = extract_fecha_from_filename(data["Archivo"])
        if data["Fecha"]:
            data["Notas"] += "Fecha extraída del nombre del archivo. "
    if not data["Subtotal"] or not data["Total"]:
        monetary_values = extract_monetary_values(text)
        if monetary_values:
            monetary_values.sort()
            if not data["Subtotal"] and len(monetary_values) > 0:
                data["Subtotal"] = monetary_values[0]
                data["Notas"] += "Subtotal estimado del texto. "
            if not data["Total"] and len(monetary_values) > 0:
                data["Total"] = monetary_values[-1]
                data["Notas"] += "Total estimado del texto. "
    campos_importantes = ["Numero_Orden", "Fecha", "Total"]
    campos_faltantes = [campo for campo in campos_importantes if not data.get(campo)]
    if campos_faltantes:
        data["Notas"] += f"Campos faltantes: {', '.join(campos_faltantes)}"
    if len(text.strip()) < 50:
        data["Estatus"] = "Sin texto suficiente"
    elif not data.get("Total"):
        data["Estatus"] = "Total no encontrado"
    else:
        data["Estatus"] = "Completo"
    return data

# === PROCESAMIENTO PRINCIPAL ===
print("Iniciando procesamiento de órdenes de compra...")
registros = []
archivos_procesados = 0
archivos_orden_compra = 0

for root, dirs, files in os.walk(ROOT_DIR):
    for file in files:
        if file.lower().endswith(".pdf"):
            archivos_procesados += 1
            path = os.path.join(root, file)
            if is_orden_compra(file):
                archivos_orden_compra += 1
                print(f"Procesando orden de compra: {file}")
                contenido, metodo = extract_text_from_pdf(path)
                if not str(contenido).startswith("ERROR"):
                    datos = parse_document_data(path, contenido, metodo)
                    registros.append(datos)
                else:
                    registros.append({
                        "Archivo": file,
                        "Ruta": path,
                        "Notas": contenido,
                        "Estatus": "Error de lectura",
                        "Método_Extracción": metodo,
                        "Fecha_Procesamiento": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                    })

# === GENERACIÓN DEL ARCHIVO EXCEL ===
if registros:
    df = pd.DataFrame(registros)
    if "Año" in df.columns and "Mes" in df.columns:
        df["Año"] = pd.to_numeric(df["Año"], errors='coerce')
        df = df.sort_values(["Año", "Mes"], ascending=[True, True])
        df["Año"] = df["Año"].astype(str)
    with pd.ExcelWriter(OUTPUT_FILE, engine='openpyxl') as writer:
        df.to_excel(writer, sheet_name='Órdenes de Compra', index=False)
        workbook = writer.book
        worksheet = writer.sheets['Órdenes de Compra']
        for column in worksheet.columns:
            max_length = 0
            column_letter = column[0].column_letter
            for cell in column:
                try:
                    if len(str(cell.value)) > max_length:
                        max_length = len(str(cell.value))
                except:
                    pass
            adjusted_width = min(max_length + 2, 50)
            worksheet.column_dimensions[column_letter].width = adjusted_width
    print(f"\n=== RESUMEN ===")
    print(f"Archivos PDF encontrados: {archivos_procesados}")
    print(f"Órdenes de compra identificadas: {archivos_orden_compra}")
    print(f"Registros procesados exitosamente: {len(registros)}")
    if "Total" in df.columns:
        totales = df["Total"].apply(lambda x: float(x) if isinstance(x, (int, float)) else 0)
        total_general = totales.sum()
        print(f"Total general de órdenes: ${total_general:,.2f}")
    if "Subtotal" in df.columns:
        subtotales = df["Subtotal"].apply(lambda x: float(x) if isinstance(x, (int, float)) else 0)
        subtotal_general = subtotales.sum()
        print(f"Subtotal general: ${subtotal_general:,.2f}")
    print(f"\nArchivo generado: {OUTPUT_FILE}")
else:
    print("No se encontraron órdenes de compra para procesar.")

# === INSTRUCCIONES PARA EL USUARIO ===
print(f"\n=== INSTRUCCIONES ===")
print("Para ejecutar este script asegúrate de tener:")
print("pip install pymupdf pandas openpyxl pytesseract pillow")
print("Además, instala Tesseract-OCR desde https://github.com/tesseract-ocr/tesseract")
print("\nEl archivo Excel generado contiene todas las órdenes de compra en una sola hoja.")
print("Los campos incluyen: número de orden, fecha, proveedor, RFC, subtotales, IVA, totales, método de extracción y estatus.")
print("Ordenado por año ascendente (2011, 2012, 2013, etc.)")
print(f"\nArchivo generado: {OUTPUT_FILE}")
