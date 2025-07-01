import os
import fitz  # PyMuPDF
import re
import pandas as pd
from datetime import datetime
# Nuevas importaciones para OCR
import pytesseract
from pdf2image import convert_from_path
from PIL import Image

# Configura la ruta de Tesseract-OCR (ajusta si es necesario)
pytesseract.pytesseract.tesseract_cmd = r"C:\Users\Latitude5500\Documents\PROYECTO KPI\PROYECTO KPI\PS Script\Release-24.08.0-0"

# Configura la ruta de Poppler (necesario para pdf2image)
POPPLER_PATH = r"C:\Users\Latitude5500\Documents\PROYECTO KPI\PROYECTO KPI\PS Script\poppler-24.08.0\Library\bin"

# Imprime información sobre la librería fitz (PyMuPDF), útil para depuración
print(fitz.__doc__)

# Ruta del archivo de log donde se guardan mensajes de proceso y errores
LOG_PATH = "log_contextual.txt"
# Carpeta donde se guardarán muestras de texto extraído de los primeros PDF
EXTRACTED_TEXT_DIR = "extracted_text_samples"

# Función para registrar mensajes en un archivo de log y en pantalla
def log(message):
    with open(LOG_PATH, "a", encoding="utf-8") as f:
        f.write(message + "\n")
    print(message)

# Función principal para extraer datos de un PDF (ahora con OCR)
def extract_data_from_pdf(pdf_path, save_text_sample=False, sample_index=0):
    """
    Extrae información relevante de un archivo PDF de orden de compra.
    Si no se puede extraer texto digital, intenta con OCR.
    Si save_text_sample es True, guarda el texto extraído en un archivo .txt para análisis.
    """
    try:
        # Abre el PDF y extrae el texto de todas las páginas (digital)
        doc = fitz.open(pdf_path)
        text = ""
        for page in doc:
            text += page.get_text("text") + "\n"
        doc.close()

        # Si no se extrajo texto, intenta con OCR
        if not text.strip():
            log(f"[INFO] Intentando OCR en: {pdf_path}")
            try:
                # Convierte las páginas del PDF a imágenes usando Poppler
                images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
                ocr_text = ""
                for i, image in enumerate(images):
                    # Aplica OCR a cada imagen
                    ocr_text += pytesseract.image_to_string(image, lang="spa") + "\n"
                text = ocr_text
            except Exception as ocr_e:
                log(f"[ERROR] OCR falló en {pdf_path}: {ocr_e}")
                return None

        # Guarda el texto extraído si es uno de los primeros 3 PDF
        if save_text_sample:
            if not os.path.exists(EXTRACTED_TEXT_DIR):
                os.makedirs(EXTRACTED_TEXT_DIR)
            base_name = os.path.splitext(os.path.basename(pdf_path))[0]
            sample_file = os.path.join(EXTRACTED_TEXT_DIR, f"sample_{sample_index}_{base_name}.txt")
            with open(sample_file, "w", encoding="utf-8") as f:
                f.write(text)

        # Si después de todo sigue sin texto, registra un error y termina
        if not text.strip():
            log(f"[ERROR] Texto vacío incluso con OCR: {pdf_path}")
            return None

        # Divide el texto en líneas para facilitar la búsqueda
        lines = text.splitlines()

        # Diccionario con los datos extraídos del PDF de orden de compra
        data = {
            "archivo": os.path.basename(pdf_path),  # Nombre del archivo PDF
            
            # Información del proveedor
            "proveedor_nombre": extract_proveedor_nombre(text),
            "proveedor_rfc": extract_proveedor_rfc(text),
            "proveedor_direccion": extract_proveedor_direccion(text),
            "proveedor_telefono": extract_proveedor_telefono(text),
            "proveedor_email": extract_proveedor_email(text),
            
            # Información de la orden
            "numero_orden": extract_numero_orden(text),
            "fecha_orden": extract_fecha_orden(text),
            "fecha_entrega": extract_fecha_entrega(text),
            "fecha_vencimiento": extract_fecha_vencimiento(text),
            
            # Información del solicitante
            "solicitante": extract_solicitante(text),
            "departamento": extract_departamento(text),
            
            # Información de productos y precios
            "productos": extract_productos(text),
            "cantidad_total": extract_cantidad_total(text),
            "precio_unitario_promedio": extract_precio_unitario_promedio(text),
            "subtotal": extract_subtotal(text),
            "iva": extract_iva(text),
            "total": extract_total(text),
            
            # Información de moneda y condiciones
            "moneda": extract_moneda(text),
            "condiciones_pago": extract_condiciones_pago(text),
            "forma_pago": extract_forma_pago(text),
            
            # Agrega la ruta completa del PDF para facilitar la búsqueda posterior
            "ruta_completa": pdf_path
        }

        return data
    except Exception as e:
        # Si ocurre un error, lo registra en el log
        log(f"[ERROR] {pdf_path}: {e}")
        return None

# Función para buscar un patrón de texto usando expresiones regulares
def extract_regex(text, pattern):
    match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
    return match.group(1).strip() if match else ""

# Función para extraer nombre del proveedor
def extract_proveedor_nombre(text):
    """Extrae el nombre del proveedor"""
    patterns = [
        r"PROVEEDOR:\s*(.+?)(?:\n|$)",
        r"RAZÓN\s+SOCIAL:\s*(.+?)(?:\n|$)",
        r"NOMBRE\s+DEL\s+PROVEEDOR:\s*(.+?)(?:\n|$)",
        r"EMPRESA:\s*(.+?)(?:\n|$)",
        r"SUPPLIER:\s*(.+?)(?:\n|$)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer RFC del proveedor
def extract_proveedor_rfc(text):
    """Extrae el RFC del proveedor"""
    patterns = [
        r"RFC:\s*([A-Z]{3,4}\d{6}[A-Z0-9]{3})",
        r"R\.F\.C\.:\s*([A-Z]{3,4}\d{6}[A-Z0-9]{3})",
        r"REGISTRO\s+FEDERAL\s+DE\s+CONTRIBUYENTES:\s*([A-Z]{3,4}\d{6}[A-Z0-9]{3})"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer dirección del proveedor
def extract_proveedor_direccion(text):
    """Extrae la dirección del proveedor"""
    patterns = [
        r"DIRECCIÓN:\s*(.+?)(?:\n|TEL|EMAIL|RFC)",
        r"DOMICILIO:\s*(.+?)(?:\n|TEL|EMAIL|RFC)",
        r"ADDRESS:\s*(.+?)(?:\n|TEL|EMAIL|RFC)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer teléfono del proveedor
def extract_proveedor_telefono(text):
    """Extrae el teléfono del proveedor"""
    patterns = [
        r"TEL[ÉÉ]FONO:\s*([\d\-\+\(\)\s]+)",
        r"TEL:\s*([\d\-\+\(\)\s]+)",
        r"PHONE:\s*([\d\-\+\(\)\s]+)",
        r"(\d{2,4}[\-\s]?\d{3,4}[\-\s]?\d{3,4})"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer email del proveedor
def extract_proveedor_email(text):
    """Extrae el email del proveedor"""
    patterns = [
        r"EMAIL:\s*([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})",
        r"E-MAIL:\s*([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})",
        r"CORREO:\s*([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer número de orden
def extract_numero_orden(text):
    """Extrae el número de orden de compra"""
    patterns = [
        r"ORDEN\s+DE\s+COMPRA\s+NO\.?\s*:?\s*([A-Z0-9\-]+)",
        r"NO\.?\s+DE\s+ORDEN:\s*([A-Z0-9\-]+)",
        r"ORDEN\s+NO\.?\s*:?\s*([A-Z0-9\-]+)",
        r"PURCHASE\s+ORDER\s+NO\.?\s*:?\s*([A-Z0-9\-]+)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer fecha de la orden de compra
def extract_fecha_orden(text):
    """Extrae la fecha de la orden de compra"""
    patterns = [
        r"FECHA\s+DE\s+ORDEN:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"FECHA:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"ORDEN\s+FECHA:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"DATE:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer fecha de entrega
def extract_fecha_entrega(text):
    """Extrae la fecha de entrega"""
    patterns = [
        r"FECHA\s+DE\s+ENTREGA:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"ENTREGA:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"DELIVERY\s+DATE:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer fecha de vencimiento
def extract_fecha_vencimiento(text):
    """Extrae la fecha de vencimiento de la orden"""
    patterns = [
        r"VENCIMIENTO:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"FECHA\s+DE\s+VENCIMIENTO:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
        r"VENCE:\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer solicitante
def extract_solicitante(text):
    """Extrae el nombre del solicitante"""
    patterns = [
        r"SOLICITANTE:\s*(.+?)(?:\n|$)",
        r"REQUISITOR:\s*(.+?)(?:\n|$)",
        r"REQUESTED\s+BY:\s*(.+?)(?:\n|$)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer departamento
def extract_departamento(text):
    """Extrae el departamento"""
    patterns = [
        r"DEPARTAMENTO:\s*(.+?)(?:\n|$)",
        r"DEPT\.?:\s*(.+?)(?:\n|$)",
        r"DEPARTMENT:\s*(.+?)(?:\n|$)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer productos (lista de productos)
def extract_productos(text):
    """Extrae la lista de productos"""
    # Busca secciones que contengan productos
    product_sections = re.findall(r"(?:PRODUCTO|ITEM|ARTÍCULO|DESCRIPCIÓN).*?(?:\n|$)", text, re.IGNORECASE | re.MULTILINE)
    if product_sections:
        return "; ".join([s.strip() for s in product_sections[:5]])  # Máximo 5 productos
    return ""

# Función para extraer cantidad total
def extract_cantidad_total(text):
    """Extrae la cantidad total de productos"""
    patterns = [
        r"CANTIDAD\s+TOTAL:\s*([\d,]+\.?\d*)",
        r"TOTAL\s+QTY:\s*([\d,]+\.?\d*)",
        r"QTY\s+TOTAL:\s*([\d,]+\.?\d*)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            try:
                return float(match.group(1).replace(",", ""))
            except ValueError:
                continue
    return 0.0

# Función para extraer precio unitario promedio
def extract_precio_unitario_promedio(text):
    """Extrae el precio unitario promedio"""
    patterns = [
        r"PRECIO\s+UNITARIO:\s*\$?\s*([\d,]+\.?\d*)",
        r"UNIT\s+PRICE:\s*\$?\s*([\d,]+\.?\d*)",
        r"PRECIO:\s*\$?\s*([\d,]+\.?\d*)"
    ]
    
    for pattern in patterns:
        matches = re.findall(pattern, text, re.IGNORECASE)
        if matches:
            try:
                prices = [float(p.replace(",", "")) for p in matches]
                return sum(prices) / len(prices)
            except ValueError:
                continue
    return 0.0

# Función para extraer subtotal
def extract_subtotal(text):
    """Extrae el subtotal"""
    patterns = [
        r"SUBTOTAL:\s*\$?\s*([\d,]+\.?\d*)",
        r"SUB\s+TOTAL:\s*\$?\s*([\d,]+\.?\d*)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            try:
                return float(match.group(1).replace(",", ""))
            except ValueError:
                continue
    return 0.0

# Función para extraer IVA
def extract_iva(text):
    """Extrae el IVA"""
    patterns = [
        r"IVA:\s*\$?\s*([\d,]+\.?\d*)",
        r"IMPUESTO:\s*\$?\s*([\d,]+\.?\d*)",
        r"TAX:\s*\$?\s*([\d,]+\.?\d*)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            try:
                return float(match.group(1).replace(",", ""))
            except ValueError:
                continue
    return 0.0

# Función para extraer el total a pagar del texto del PDF
def extract_total(text):
    """Extrae el total a pagar con patrones más robustos"""
    patterns = [
        r"TOTAL\s+A\s+PAGAR:\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL:\s*\$?\s*([\d,]+\.?\d*)",
        r"IMPORTE\s+TOTAL:\s*\$?\s*([\d,]+\.?\d*)",
        r"GRAND\s+TOTAL:\s*\$?\s*([\d,]+\.?\d*)",
        r"TOTAL\s+AMOUNT:\s*\$?\s*([\d,]+\.?\d*)"
    ]
    
    for pattern in patterns:
        matches = re.findall(pattern, text, re.IGNORECASE)
        if matches:
            try:
                return float(matches[-1].replace(",", ""))
            except ValueError:
                continue
    
    return 0.0

# Función para extraer moneda
def extract_moneda(text):
    """Extrae la moneda utilizada"""
    patterns = [
        r"MONEDA:\s*([A-Z]{3})",
        r"CURRENCY:\s*([A-Z]{3})",
        r"(\$|USD|MXN|EUR|PESOS|DÓLARES)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return "MXN"  # Por defecto

# Función para extraer condiciones de pago
def extract_condiciones_pago(text):
    """Extrae las condiciones de pago"""
    patterns = [
        r"CONDICIONES\s+DE\s+PAGO:\s*(.+?)(?:\n|$)",
        r"PAYMENT\s+TERMS:\s*(.+?)(?:\n|$)",
        r"TÉRMINOS:\s*(.+?)(?:\n|$)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
        if match:
            return match.group(1).strip()
    return ""

# Función para extraer forma de pago
def extract_forma_pago(text):
    """Extrae la forma de pago"""
    patterns = [
        r"FORMA\s+DE\s+PAGO:\s*(.+?)(?:\n|$)",
        r"PAYMENT\s+METHOD:\s*(.+?)(?:\n|$)",
        r"MÉTODO\s+DE\s+PAGO:\s*(.+?)(?:\n|$)"
    ]
    
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
        if match:
            return match.group(1).strip()
    return ""

# Función para procesar todos los PDF en una carpeta y subcarpetas
def process_folder(base_path):
    all_data = []  # Lista para almacenar los datos de todos los PDF
    sample_count = 0  # Contador para guardar muestras de texto
    
    for root, _, files in os.walk(base_path):
        for file in files:
            if file.lower().endswith(".pdf"):
                pdf_path = os.path.join(root, file)
                log(f"Procesando: {pdf_path}")
                save_text_sample = sample_count < 3
                data = extract_data_from_pdf(pdf_path, save_text_sample=save_text_sample, sample_index=sample_count)
                if save_text_sample:
                    sample_count += 1
                if data:
                    # Agrega información de la carpeta y fecha de procesamiento
                    data["mes"] = os.path.basename(root)
                    data["carpeta_padre"] = os.path.basename(os.path.dirname(root))
                    data["fecha_procesamiento"] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                    all_data.append(data)
    
    # Convierte la lista de datos en un DataFrame de pandas
    return pd.DataFrame(all_data)

# Bloque principal que se ejecuta al correr el script directamente
if __name__ == "__main__":
    # Si existe el archivo de log, lo borra para empezar limpio
    if os.path.exists(LOG_PATH):
        os.remove(LOG_PATH)

    # Ruta de la carpeta donde están los PDF (ajustar si es necesario)
    folder_path = r"C:\Users\Latitude5500\Documents\PROYECTO KPI\PROYECTO KPI\1. Ordenes de Compra"
    
    # Procesa todos los PDF y obtiene un DataFrame con los resultados
    df = process_folder(folder_path)

    # Si se extrajeron datos, los guarda en un archivo Excel
    if not df.empty:
        # Ordena por fecha de procesamiento para mejor visualización
        df = df.sort_values('fecha_procesamiento', ascending=False)
        
        # Guarda en Excel con formato mejorado
        with pd.ExcelWriter("resumen_ordenes_de_compra.xlsx", engine='openpyxl') as writer:
            df.to_excel(writer, sheet_name='Datos Extraídos', index=False)
            
            # Obtiene el workbook para aplicar formato
            workbook = writer.book
            worksheet = writer.sheets['Datos Extraídos']
            
            # Ajusta el ancho de las columnas
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
        
        log(f"[FINALIZADO] {len(df)} registros guardados en 'resumen_ordenes_de_compra.xlsx'")
        
        # Muestra un resumen de los datos extraídos
        log(f"[RESUMEN] Total de archivos procesados: {len(df)}")
        if 'total' in df.columns:
            total_general = df['total'].sum()
            log(f"[RESUMEN] Total general de órdenes: ${total_general:,.2f}")
    else:
        log("[FINALIZADO] Sin datos útiles.")