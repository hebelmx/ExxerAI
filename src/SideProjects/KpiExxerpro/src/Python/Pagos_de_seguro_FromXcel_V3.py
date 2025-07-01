import os
import fitz  # PyMuPDF
import re
import pandas as pd
import json
from datetime import datetime
# Importaciones para OCR
import pytesseract
from pdf2image import convert_from_path
from PIL import Image
import cv2  # OpenCV para procesamiento de imágenes
import numpy as np

# Configuración de rutas para OCR
pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"
POPPLER_PATH = r"C:\Program Files\Release-24.08.0-0\poppler-24.08.0\Library\bin"

# Configuración de archivos y carpetas
EXCEL_INPUT = r"resumen_pagos_seguros.xlsx"  # Ahora toma el Excel de salida del otro script
EXCEL_OUTPUT = "PRUEBA_resumen_pagos_seguros_completado.xlsx"
LOG_PATH = "log_completado.txt"
EXTRACTED_DATA_DIR = "extracted_data"
DESCARTADOS_PATH = "archivos_descartados.txt"
CARPETAS_SIN_VALIDOS_PATH = "carpetas_sin_archivos_validos.txt"

# Función para registrar mensajes en log y pantalla
def log(message):
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    log_message = f"[{timestamp}] {message}"
    with open(LOG_PATH, "a", encoding="utf-8") as f:
        f.write(log_message + "\n")
    print(log_message)

# Función para verificar si un campo está vacío o incompleto
def is_field_empty(value):
    """Verifica si un campo está vacío o contiene valores que indican datos faltantes"""
    if pd.isna(value) or value == "" or value == "nan":
        return True
    if isinstance(value, str) and value.strip() == "":
        return True
    return False

# Función para extraer texto de PDF (digital u OCR)
def extract_text_from_pdf(pdf_path):
    """
    Extrae texto de un PDF usando el método más apropiado.
    Retorna: (texto, método_usado, error_si_hay)
    """
    try:
        # Intenta extraer texto digital primero
        doc = fitz.open(pdf_path)
        text = "\n".join(page.get_text("text") for page in doc)
        doc.close()
        
        if text.strip():
            return text, "texto_digital", None
        
        # Si no hay texto digital, usa OCR
        log(f"[INFO] Aplicando OCR a: {pdf_path}")
        images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
        ocr_text = ""
        for i, image in enumerate(images):
            ocr_text += pytesseract.image_to_string(image, lang="spa") + "\n"
        
        if ocr_text.strip():
            return ocr_text, "ocr", None
        else:
            return "", "fallido", "No se pudo extraer texto ni con OCR"
            
    except Exception as e:
        return "", "error", str(e)

# Función para buscar un campo específico en el texto
def search_field_in_text(text, field_name, patterns):
    """
    Busca un campo específico usando múltiples patrones.
    Retorna el primer valor encontrado o cadena vacía.
    """
    for pattern in patterns:
        match = re.search(pattern, text, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    return ""

# Patrones de búsqueda para cada campo
FIELD_PATTERNS = {
    "registro_patronal": [
        r"REGISTRO\s+PATRONAL:\s*([^\s\n]+)"
    ],
    "periodo_imss": [
        r"PER[ÍI]ODO\s+(QUE\s+)?COMPRENDE\s+EL\s+PAGO\s+DE\s+SEGUROS\s+IMSS[:\s]*([\w\s/]+)",
        r"PER[ÍI]ODO[:\s]*([\w\s/]+)",
        r"PAGO[:\s]*([\w\s/]+)"
    ],
    "periodo_rcv": [
        r"BIMESTRE\s+(QUE\s+)?COMPRENDE\s+EL\s+PAGO\s+RCV.[:\s]([\w\s/]+)",
        r"BIMESTRE[:\s]*([\w\s/]+)"
    ],
    "dias_cotizar": [
        r"N[oº\.]\s*DE\s*D[ÍI]AS\s*A\s*COTIZAR[:\s]([0-9]{1,3})",
        r"D[ÍI]AS\s*A\s*COTIZAR[:\s]*([0-9]{1,3})"
    ],
    "num_cotizantes": [
        r"No\.\s*DE\s*COTIZANTES:\s*([0-9]{1,5})"
    ],
    "valor_uma": [
        r"VALOR\s+UMA[:\s]\$?\s([\d,]+\.\d{2})",
        r"UMA[:\s]\$?\s([\d,]+\.\d{2})"
    ]
}


# Función para extraer conceptos específicos del texto
def extract_concepts_from_text(text):
    """Extrae conceptos monetarios del texto usando búsqueda por líneas"""
    concepts = {}
    lines = text.splitlines()
    
    concept_keywords = {
        "cuota_fija": ["CUOTA FIJA"],
        "riesgos_trabajo": ["RIESGOS DE TRABAJO", "RIESGOS"],
        "guarderias": ["GUARDERÍAS Y PRESTACIONES SOCIALES", "GUARDERIAS"],
        "subtotal_imss": ["SUBTOTAL SEGUROS IMSS", "SUBTOTAL IMSS"],
        "rcv": ["SUBTOTAL RCV", "RCV"]
    }
    
    for concept_name, keywords in concept_keywords.items():
        for line in lines:
            line_upper = line.upper()
            for keyword in keywords:
                if keyword in line_upper:
                    # Busca números en la línea
                    numbers = re.findall(r"[\d,]+\.\d{2}", line)
                    if numbers:
                        concepts[concept_name] = float(numbers[0].replace(",", ""))
                        break
            if concept_name in concepts:
                break
    
    return concepts

def extract_periodo_imss_by_ocr_region(pdf_path):
    """
    Extrae el periodo_imss usando OCR solo en la región del cuadro donde aparece el texto de referencia.
    Retorna el texto extraído o cadena vacía.
    """
    try:
        # Convertir la primera página del PDF a imagen
        images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
        if not images:
            return ""
        image = images[0]
        # Convertir a formato OpenCV
        open_cv_image = np.array(image)
        if open_cv_image.shape[2] == 4:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGBA2BGR)
        else:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGB2BGR)
        gray = cv2.cvtColor(open_cv_image, cv2.COLOR_BGR2GRAY)
        # Binarizar para encontrar contornos
        _, thresh = cv2.threshold(gray, 180, 255, cv2.THRESH_BINARY_INV)
        # Encontrar contornos
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        # Buscar el cuadro que contenga el texto de referencia
        for cnt in contours:
            x, y, w, h = cv2.boundingRect(cnt)
            if w < 100 or h < 20:
                continue  # Ignora cuadros pequeños
            roi = open_cv_image[y:y+h, x:x+w]
            roi_pil = Image.fromarray(cv2.cvtColor(roi, cv2.COLOR_BGR2RGB))
            ocr_text = pytesseract.image_to_string(roi_pil, lang="spa")
            if "PERÍODO QUE COMPRENDE" in ocr_text and "SEGUROS IMSS" in ocr_text:
                # Buscar la(s) línea(s) después del texto de referencia
                lines = ocr_text.splitlines()
                for i, line in enumerate(lines):
                    if "PERÍODO QUE COMPRENDE" in line and "SEGUROS IMSS" in line:
                        # Tomar la siguiente línea no vacía
                        for next_line in lines[i+1:]:
                            if next_line.strip():
                                return next_line.strip()
        return ""
    except Exception as e:
        return ""

def extract_periodo_rcv_by_ocr_region(pdf_path):
    """
    Extrae el periodo_rcv usando OCR solo en la región del cuadro donde aparece el texto de referencia.
    Retorna el texto extraído o cadena vacía.
    """
    try:
        images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
        if not images:
            return ""
        image = images[0]
        open_cv_image = np.array(image)
        if open_cv_image.shape[2] == 4:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGBA2BGR)
        else:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGB2BGR)
        gray = cv2.cvtColor(open_cv_image, cv2.COLOR_BGR2GRAY)
        _, thresh = cv2.threshold(gray, 180, 255, cv2.THRESH_BINARY_INV)
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        for cnt in contours:
            x, y, w, h = cv2.boundingRect(cnt)
            if w < 100 or h < 20:
                continue
            roi = open_cv_image[y:y+h, x:x+w]
            roi_pil = Image.fromarray(cv2.cvtColor(roi, cv2.COLOR_BGR2RGB))
            ocr_text = pytesseract.image_to_string(roi_pil, lang="spa")
            if "BIMESTRE QUE COMPRENDE" in ocr_text and "RCV E INFONAVIT" in ocr_text:
                # Devuelve todo el texto del recuadro (puedes afinarlo después)
                return ocr_text.strip()
        return ""
    except Exception as e:
        return ""

def extract_dias_cotizar_by_ocr_region(pdf_path):
    """
    Extrae dias_cotizar usando OCR solo en la región del cuadro donde aparece el texto de referencia.
    Retorna el segundo número encontrado o cadena vacía.
    """
    try:
        images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
        if not images:
            return ""
        image = images[0]
        open_cv_image = np.array(image)
        if open_cv_image.shape[2] == 4:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGBA2BGR)
        else:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGB2BGR)
        gray = cv2.cvtColor(open_cv_image, cv2.COLOR_BGR2GRAY)
        _, thresh = cv2.threshold(gray, 180, 255, cv2.THRESH_BINARY_INV)
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        for cnt in contours:
            x, y, w, h = cv2.boundingRect(cnt)
            if w < 100 or h < 20:
                continue
            roi = open_cv_image[y:y+h, x:x+w]
            roi_pil = Image.fromarray(cv2.cvtColor(roi, cv2.COLOR_BGR2RGB))
            ocr_text = pytesseract.image_to_string(roi_pil, lang="spa")
            if "No. DE DÍAS A COTIZAR" in ocr_text:
                # Extraer todos los números del recuadro
                numeros = re.findall(r"\d+", ocr_text)
                if len(numeros) >= 2:
                    return numeros[1]  # Segundo número
        return ""
    except Exception as e:
        return ""

def extract_valor_uma_by_ocr_region(pdf_path):
    """
    Extrae valor_uma usando OCR solo en la región del cuadro donde aparece el texto de referencia.
    Retorna el primer número decimal encontrado o cadena vacía.
    """
    try:
        images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
        if not images:
            return ""
        image = images[0]
        open_cv_image = np.array(image)
        if open_cv_image.shape[2] == 4:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGBA2BGR)
        else:
            open_cv_image = cv2.cvtColor(open_cv_image, cv2.COLOR_RGB2BGR)
        gray = cv2.cvtColor(open_cv_image, cv2.COLOR_BGR2GRAY)
        _, thresh = cv2.threshold(gray, 180, 255, cv2.THRESH_BINARY_INV)
        contours, _ = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        for cnt in contours:
            x, y, w, h = cv2.boundingRect(cnt)
            if w < 100 or h < 20:
                continue
            roi = open_cv_image[y:y+h, x:x+w]
            roi_pil = Image.fromarray(cv2.cvtColor(roi, cv2.COLOR_BGR2RGB))
            ocr_text = pytesseract.image_to_string(roi_pil, lang="spa")
            if "VALOR UMA" in ocr_text:
                # Extraer todos los números con decimales del recuadro
                numeros = re.findall(r"\d+[\.,]\d{2}", ocr_text)
                if numeros:
                    return numeros[0].replace(",", ".")  # Devuelve el primero, normalizando decimal
        return ""
    except Exception as e:
        return ""

# NUEVO: Función OCR dedicada para extraer registro patronal, periodo IMSS y periodo RCV

def extract_key_fields_by_ocr(pdf_path):
    """
    Extrae registro patronal, periodo_imss, periodo_rcv, valor_uma, num_cotizantes y dias_cotizar usando OCR de la primera página.
    Retorna un diccionario con los valores extraídos (vacío si no encuentra).
    """
    result = {
        "registro_patronal": "",
        "periodo_imss": "",
        "periodo_rcv": "",
        "valor_uma": "",
        "num_cotizantes": "",
        "dias_cotizar": ""
    }
    try:
        images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
        if not images:
            return result
        image = images[0]
        ocr_text = pytesseract.image_to_string(image, lang="spa")
        # Log del texto OCR completo para depuración
        log("[DEPURACION][OCR] Texto completo extraído:")
        log(ocr_text)
        # Limpiar texto para facilitar búsqueda
        ocr_text = ocr_text.replace("\r", " ").replace("\n", " ").replace("  ", " ")
        # REGISTRO PATRONAL (flexible)
        match_reg = re.search(r"REGISTRO\s*PATRONAL:?\s*([A-Z0-9\-]+)", ocr_text, re.IGNORECASE)
        if match_reg:
            result["registro_patronal"] = match_reg.group(1).strip()
        # PERIODO IMSS (MM-AAAA)
        lines = ocr_text.splitlines()
        periodo = ""
        for i, line in enumerate(lines):
            if "PERIODO QUE COMPRENDE EL PAGO DE SEGUROS IMSS" in line.upper():
                log(f"[DEPURACION][OCR] Línea PERIODO_IMSS encontrada: {line}")
                # Busca en la misma línea
                match = re.search(r"([0-9O]{2}[-/][0-9O]{4})", line)
                if match:
                    periodo = match.group(1).replace("O", "0")
                # Si no, busca en la siguiente línea no vacía
                else:
                    for next_line in lines[i+1:]:
                        log(f"[DEPURACION][OCR] Línea siguiente PERIODO_IMSS: {next_line}")
                        match = re.search(r"([0-9O]{2}[-/][0-9O]{4})", next_line)
                        if match:
                            periodo = match.group(1).replace("O", "0")
                            break
                break
        result["periodo_imss"] = periodo
        # BIMESTRE RCV (MM-AAAA)
        periodo_rcv = ""
        for i, line in enumerate(lines):
            if "BIMESTRE QUE COMPRENDE EL PAGO RCV" in line.upper():
                log(f"[DEPURACION][OCR] Línea PERIODO_RCV encontrada: {line}")
                match = re.search(r"([0-9O]{2}[-/][0-9O]{4})", line)
                if match:
                    periodo_rcv = match.group(1).replace("O", "0")
                else:
                    for next_line in lines[i+1:]:
                        log(f"[DEPURACION][OCR] Línea siguiente PERIODO_RCV: {next_line}")
                        match = re.search(r"([0-9O]{2}[-/][0-9O]{4})", next_line)
                        if match:
                            periodo_rcv = match.group(1).replace("O", "0")
                            break
                break
        result["periodo_rcv"] = periodo_rcv
        # DIAS COTIZAR (entero)
        dias_cotizar = ""
        for i, line in enumerate(lines):
            if "DIAS A COTIZAR" in line.upper():
                log(f"[DEPURACION][OCR] Línea DIAS_COTIZAR encontrada: {line}")
                match = re.search(r"([0-9O]{1,3})", line)
                if match:
                    dias_cotizar = match.group(1).replace("O", "0")
                else:
                    for next_line in lines[i+1:]:
                        log(f"[DEPURACION][OCR] Línea siguiente DIAS_COTIZAR: {next_line}")
                        match = re.search(r"([0-9O]{1,3})", next_line)
                        if match:
                            dias_cotizar = match.group(1).replace("O", "0")
                            break
                break
        result["dias_cotizar"] = dias_cotizar
        # VALOR UMA (decimal, puede estar en la misma línea o debajo de 'Valor UMA')
        match_uma = re.search(r"Valor UMA[\s:]*([0-9]+[\.,][0-9]{1,2})", ocr_text, re.IGNORECASE)
        if not match_uma:
            # Buscar en la línea siguiente a 'Valor UMA'
            match_uma_block = re.search(r"Valor UMA[\s:]*([A-Za-z]*)\s*([0-9]+[\.,][0-9]{1,2})", ocr_text, re.IGNORECASE)
            if match_uma_block:
                result["valor_uma"] = match_uma_block.group(2).replace(",", ".").strip()
        else:
            result["valor_uma"] = match_uma.group(1).replace(",", ".").strip()
        # No. DE COTIZANTES (entero)
        match_cot = re.search(r"No\. DE COTIZANTES:?\s*([0-9]+)", ocr_text, re.IGNORECASE)
        if match_cot:
            result["num_cotizantes"] = match_cot.group(1).strip()
        return result
    except Exception as e:
        return result

# Función para procesar una fila del Excel
def process_excel_row(row, pdf_path, anio):
    """
    Procesa una fila del Excel para completar campos faltantes.
    Retorna: diccionario con datos actualizados y metadatos
    """
    archivo = row['archivo']
    log(f"[DEBUG] Procesando archivo: {archivo}")
    log(f"[DEBUG] Columnas disponibles en la fila: {list(row.index)}")
    if not os.path.exists(pdf_path):
        log(f"[ERROR] Archivo no encontrado: {pdf_path}")
        return {
            'status': 'error',
            'nota': f'Archivo no encontrado: {pdf_path}',
            'metodo_extraccion': 'no_aplicable',
            'ruta_completa': pdf_path,
            'año': anio
        }
    log(f"[DEBUG] Archivo encontrado: {pdf_path}")
    campos_faltantes = []
    for field in FIELD_PATTERNS.keys():
        if is_field_empty(row.get(field, '')):
            campos_faltantes.append(field)
    # NUEVO: Extraer SIEMPRE por OCR los 6 campos clave
    log(f"[DEBUG] Extrayendo campos clave por OCR dedicado")
    key_fields = extract_key_fields_by_ocr(pdf_path)
    datos_extraidos = {}
    for campo in ["registro_patronal", "periodo_imss", "periodo_rcv", "valor_uma", "num_cotizantes", "dias_cotizar"]:
        if campo in campos_faltantes:
            valor = key_fields.get(campo, "")
            if campo in ["periodo_imss", "periodo_rcv"]:
                # Validar formato MM-AAAA
                if re.match(r"^[0-9]{2}-[0-9]{4}$", valor):
                    datos_extraidos[campo] = valor
                else:
                    datos_extraidos[campo] = ""
            elif campo == "valor_uma":
                # Validar decimal
                if re.match(r"^[0-9]+\.[0-9]{1,2}$", valor):
                    datos_extraidos[campo] = valor
                else:
                    datos_extraidos[campo] = ""
            elif campo in ["num_cotizantes", "dias_cotizar"]:
                # Validar entero
                if re.match(r"^[0-9]+$", valor):
                    datos_extraidos[campo] = valor
                else:
                    datos_extraidos[campo] = ""
            else:
                datos_extraidos[campo] = valor
            log(f"[DEBUG] Campo '{campo}' extraído por OCR dedicado: {datos_extraidos[campo]}")
    # Para los demás campos, seguir lógica anterior
    text, metodo, error = extract_text_from_pdf(pdf_path)
    if error:
        log(f"[ERROR] Error al extraer texto: {error}")
        return {
            'status': 'error',
            'nota': f'Error al extraer texto: {error}',
            'metodo_extraccion': metodo,
            'ruta_completa': pdf_path,
            'año': anio
        }
    for campo in campos_faltantes:
        if campo in ["registro_patronal", "periodo_imss", "periodo_rcv", "valor_uma", "num_cotizantes", "dias_cotizar"]:
            continue  # Ya extraídos por OCR dedicado
        if campo in FIELD_PATTERNS:
            valor = search_field_in_text(text, campo, FIELD_PATTERNS[campo])
            if valor:
                datos_extraidos[campo] = valor
                log(f"[DEBUG] Campo '{campo}' extraído: {valor}")
            else:
                # Si es periodo_imss y no se encontró, intenta por OCR de región
                if campo == "periodo_imss":
                    log(f"[INFO] Intentando extraer '{campo}' por OCR de región...")
                    valor_ocr = extract_periodo_imss_by_ocr_region(pdf_path)
                    if valor_ocr:
                        datos_extraidos[campo] = valor_ocr
                        log(f"[DEBUG] Campo '{campo}' extraído por OCR de región: {valor_ocr}")
                    else:
                        log(f"[DEBUG] Campo '{campo}' no encontrado ni por OCR de región")
                elif campo == "periodo_rcv":
                    log(f"[INFO] Intentando extraer '{campo}' por OCR de región...")
                    valor_ocr = extract_periodo_rcv_by_ocr_region(pdf_path)
                    if valor_ocr:
                        datos_extraidos[campo] = valor_ocr
                        log(f"[DEBUG] Campo '{campo}' extraído por OCR de región: {valor_ocr}")
                    else:
                        log(f"[DEBUG] Campo '{campo}' no encontrado ni por OCR de región")
                elif campo == "dias_cotizar":
                    log(f"[INFO] Intentando extraer '{campo}' por OCR de región...")
                    valor_ocr = extract_dias_cotizar_by_ocr_region(pdf_path)
                    if valor_ocr:
                        datos_extraidos[campo] = valor_ocr
                        log(f"[DEBUG] Campo '{campo}' extraído por OCR de región: {valor_ocr}")
                    else:
                        log(f"[DEBUG] Campo '{campo}' no encontrado ni por OCR de región")
                elif campo == "valor_uma":
                    log(f"[INFO] Intentando extraer '{campo}' por OCR de región...")
                    valor_ocr = extract_valor_uma_by_ocr_region(pdf_path)
                    if valor_ocr:
                        datos_extraidos[campo] = valor_ocr
                        log(f"[DEBUG] Campo '{campo}' extraído por OCR de región: {valor_ocr}")
                    else:
                        log(f"[DEBUG] Campo '{campo}' no encontrado ni por OCR de región")
                else:
                    log(f"[DEBUG] Campo '{campo}' no encontrado en el texto")
    # Extrae conceptos si es necesario
    if any(campo in campos_faltantes for campo in ['cuota_fija', 'riesgos_trabajo', 'guarderias', 'subtotal_imss', 'rcv']):
        log(f"[DEBUG] Extrayendo conceptos monetarios")
        concepts = extract_concepts_from_text(text)
        for campo, valor in concepts.items():
            if campo in campos_faltantes:
                datos_extraidos[campo] = valor
                log(f"[DEBUG] Concepto '{campo}' extraído: {valor}")
    log(f"[DEBUG] Total de datos extraídos: {len(datos_extraidos)}")
    if not os.path.exists(EXTRACTED_DATA_DIR):
        os.makedirs(EXTRACTED_DATA_DIR)
    json_filename = f"{os.path.splitext(archivo)[0]}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
    json_path = os.path.join(EXTRACTED_DATA_DIR, json_filename)
    json_data = {
        'archivo_original': archivo,
        'fecha_procesamiento': datetime.now().isoformat(),
        'metodo_extraccion': metodo,
        'texto_completo': text,
        'campos_faltantes_originales': campos_faltantes,
        'datos_extraidos': datos_extraidos
    }
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(json_data, f, ensure_ascii=False, indent=2)
    log(f"[DEBUG] Datos guardados en JSON: {json_path}")
    return {
        'status': 'completado' if datos_extraidos else 'sin_datos',
        'datos_extraidos': datos_extraidos,
        'metodo_extraccion': metodo,
        'nota': f"Extraídos {len(datos_extraidos)} de {len(campos_faltantes)} campos faltantes",
        'ruta_completa': pdf_path,
        'año': anio
    }

# Función principal
def main():
    """Función principal que ejecuta todo el proceso"""
    log("=== INICIANDO PROCESO DE COMPLETADO DE DATOS ===")
    
    # Verifica que el archivo Excel existe
    if not os.path.exists(EXCEL_INPUT):
        log(f"[ERROR] No se encontró el archivo: {EXCEL_INPUT}")
        return
    
    # Lee el Excel
    try:
        df = pd.read_excel(EXCEL_INPUT)
        log(f"[INFO] Excel leído correctamente. {len(df)} filas encontradas.")
    except Exception as e:
        log(f"[ERROR] Error al leer el Excel: {e}")
        return
    
    filas_filtradas = []
    resultados = []
    archivos_descartados = []
    carpetas_con_validos = set()
    carpetas_todas = set()
    for index, row in df.iterrows():
        log(f"[PROCESANDO] Fila {index + 1}: {row['archivo']}")
        pdf_path = row['ruta_completa']
        anio = row['año'] if 'año' in row else ''
        carpeta = os.path.dirname(pdf_path)
        carpetas_todas.add(carpeta)
        text, metodo, error = extract_text_from_pdf(pdf_path)
        if error:
            log(f"[ERROR] Error al extraer texto: {error}")
            resultado = {
                'status': 'error',
                'nota': f'Error al extraer texto: {error}',
                'metodo_extraccion': metodo,
                'ruta_completa': pdf_path,
                'año': anio
            }
            resultados.append(resultado)
            continue
        titulo = "FORMATO PARA PAGO DE CUOTAS OBRERO PATRONALES, APORTACIONES Y AMORTIZACIONES"
        if titulo in text:
            resultado = process_excel_row(row, pdf_path, anio)
            resultados.append(resultado)
            if resultado['status'] == 'completado' and 'datos_extraidos' in resultado:
                for campo, valor in resultado['datos_extraidos'].items():
                    df.at[index, campo] = valor
            df.at[index, 'metodo_extraccion'] = resultado.get('metodo_extraccion', '')
            df.at[index, 'status'] = resultado.get('status', '')
            df.at[index, 'nota'] = resultado.get('nota', '')
            df.at[index, 'fecha_procesamiento'] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            df.at[index, 'ruta_completa'] = resultado.get('ruta_completa', '')
            filas_filtradas.append(df.loc[index])
            carpetas_con_validos.add(carpeta)
        else:
            log(f"[INFO] El PDF no contiene el título requerido. Fila omitida.")
            archivos_descartados.append(row['archivo'])
            resultado = {
                'status': 'omitido',
                'nota': 'No contiene el título requerido',
                'metodo_extraccion': metodo,
                'ruta_completa': pdf_path,
                'año': anio
            }
            resultados.append(resultado)
    if filas_filtradas:
        df_filtrado = pd.DataFrame(filas_filtradas)
    else:
        df_filtrado = pd.DataFrame(columns=df.columns)
    try:
        df_filtrado.to_excel(EXCEL_OUTPUT, index=False)
        log(f"[EXITO] Excel actualizado guardado como: {EXCEL_OUTPUT}")
    except Exception as e:
        log(f"[ERROR] Error al guardar el Excel: {e}")
        return
    # Guardar los archivos descartados en un TXT
    try:
        with open(DESCARTADOS_PATH, "w", encoding="utf-8") as f:
            for archivo in archivos_descartados:
                f.write(str(archivo) + "\n")
        log(f"[INFO] Lista de archivos descartados guardada en: {DESCARTADOS_PATH}")
    except Exception as e:
        log(f"[ERROR] No se pudo guardar el archivo de descartados: {e}")
    # Guardar las carpetas donde no se seleccionó ningún archivo válido
    carpetas_sin_validos = sorted(list(carpetas_todas - carpetas_con_validos))
    try:
        with open(CARPETAS_SIN_VALIDOS_PATH, "w", encoding="utf-8") as f:
            for idx, carpeta in enumerate(carpetas_sin_validos, 1):
                f.write(f"{idx}. {carpeta}\n")
        log(f"[INFO] Lista de carpetas sin archivos válidos guardada en: {CARPETAS_SIN_VALIDOS_PATH}")
    except Exception as e:
        log(f"[ERROR] No se pudo guardar el archivo de carpetas sin válidos: {e}")
    completados = sum(1 for r in resultados if r['status'] == 'completado')
    errores = sum(1 for r in resultados if r['status'] == 'error')
    omitidos = sum(1 for r in resultados if r['status'] == 'omitido')
    sin_datos = sum(1 for r in resultados if r['status'] == 'sin_datos')
    log(f"[RESUMEN] Proceso completado:")
    log(f"  - Fila procesada: {len(resultados)}")
    log(f"  - Completadas exitosamente: {completados}")
    log(f"  - Con errores: {errores}")
    log(f"  - Sin datos encontrados: {sin_datos}")
    log(f"  - Omitidas (sin título): {omitidos}")
    # Advertencia si hay muchos errores de extracción
    UMBRAL_ADVERTENCIA = 10
    if errores > UMBRAL_ADVERTENCIA:
        log(f"[ADVERTENCIA] Se detectaron {errores} archivos con error al extraer texto (posible fallo de OCR o PDF dañado). Revisa la configuración de OCR o la calidad de los archivos.")
    log("=== PROCESO FINALIZADO ===")

if __name__ == "__main__":
    main()
