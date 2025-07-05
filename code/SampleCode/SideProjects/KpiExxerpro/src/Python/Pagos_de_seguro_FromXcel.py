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

# Configuración de rutas para OCR
pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"
POPPLER_PATH = r"C:\Program Files\poppler\Library\bin"

# Configuración de archivos y carpetas
EXCEL_INPUT = "resumen_pagos_seguros.xlsx"
EXCEL_OUTPUT = "resumen_pagos_seguros_completado.xlsx"
LOG_PATH = "log_completado.txt"
EXTRACTED_DATA_DIR = "extracted_data"

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
        r"REGISTRO PATRONAL:\s*RFC:\s*(\S+)",
        r"RFC:\s*(\S+)",
        r"REGISTRO\s+PATRONAL[:\s]*(\S+)"
    ],
    "periodo_imss": [
        r"PERÍODO QUE COMPRENDE\s+EL PAGO DE SEGUROS IMSS\s*(.+?)\s",
        r"PERIODO[:\s]*([A-Za-z]+\s+\d{4})",
        r"PAGO[:\s]*([A-Za-z]+\s+\d{4})"
    ],
    "periodo_rcv": [
        r"BIMESTRE QUE COMPRENDE\s+EL PAGO RCV E INFONAVIT\s*(.+?)\s",
        r"BIMESTRE[:\s]*([A-Za-z]+\s+\d{4})"
    ],
    "dias_cotizar": [
        r"No\. DE DÍAS A COTIZAR:\s*(\d+)",
        r"DÍAS A COTIZAR[:\s]*(\d+)",
        r"DIAS[:\s]*(\d+)"
    ],
    "num_cotizantes": [
        r"No\. DE COTIZANTES:\s*(\d+)",
        r"COTIZANTES[:\s]*(\d+)",
        r"TRABAJADORES[:\s]*(\d+)"
    ],
    "valor_uma": [
        r"Valor UMA\s*(\d+\.\d+)",
        r"UMA[:\s]*(\d+\.\d+)"
    ],
    "cuota_fija": [
        r"CUOTA FIJA[:\s]*\$?\s*([\d,]+\.\d{2})",
        r"CUOTA[:\s]*\$?\s*([\d,]+\.\d{2})"
    ],
    "riesgos_trabajo": [
        r"RIESGOS DE TRABAJO[:\s]*\$?\s*([\d,]+\.\d{2})",
        r"RIESGOS[:\s]*\$?\s*([\d,]+\.\d{2})"
    ],
    "guarderias": [
        r"GUARDERÍAS Y PRESTACIONES SOCIALES[:\s]*\$?\s*([\d,]+\.\d{2})",
        r"GUARDERIAS[:\s]*\$?\s*([\d,]+\.\d{2})"
    ],
    "subtotal_imss": [
        r"SUBTOTAL SEGUROS IMSS[:\s]*\$?\s*([\d,]+\.\d{2})",
        r"SUBTOTAL IMSS[:\s]*\$?\s*([\d,]+\.\d{2})"
    ],
    "rcv": [
        r"SUBTOTAL RCV[:\s]*\$?\s*([\d,]+\.\d{2})",
        r"RCV[:\s]*\$?\s*([\d,]+\.\d{2})"
    ],
    "total_pagar": [
        r"TOTAL A PAGAR[:\s]*\$?\s*([\d,]+\.\d{2})",
        r"TOTAL[:\s]*\$?\s*([\d,]+\.\d{2})",
        r"\$\s*([\d,]+\.\d{2})\s*$"
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

# Función para procesar una fila del Excel
def process_excel_row(row, base_folder):
    """
    Procesa una fila del Excel para completar campos faltantes.
    Retorna: diccionario con datos actualizados y metadatos
    """
    archivo = row['archivo']
    
    # Usa la ruta completa si está disponible, sino construye la ruta
    if 'ruta_completa' in row and not is_field_empty(row['ruta_completa']):
        pdf_path = row['ruta_completa']
    else:
        # Fallback: construye la ruta usando mes y archivo (método anterior)
        mes = row.get('mes', '')
        pdf_path = os.path.join(base_folder, mes, archivo)
    
    # Verifica si el archivo existe
    if not os.path.exists(pdf_path):
        return {
            'status': 'error',
            'nota': f'Archivo no encontrado: {pdf_path}',
            'metodo_extraccion': 'no_aplicable'
        }
    
    # Identifica campos faltantes
    campos_faltantes = []
    for field in FIELD_PATTERNS.keys():
        if is_field_empty(row.get(field, '')):
            campos_faltantes.append(field)
    
    if not campos_faltantes:
        return {
            'status': 'completo',
            'nota': 'Todos los campos ya están completos',
            'metodo_extraccion': 'no_aplicable'
        }
    
    # Extrae texto del PDF
    text, metodo, error = extract_text_from_pdf(pdf_path)
    
    if error:
        return {
            'status': 'error',
            'nota': f'Error al extraer texto: {error}',
            'metodo_extraccion': metodo
        }
    
    # Busca campos faltantes
    datos_extraidos = {}
    for campo in campos_faltantes:
        if campo in FIELD_PATTERNS:
            valor = search_field_in_text(text, campo, FIELD_PATTERNS[campo])
            if valor:
                datos_extraidos[campo] = valor
    
    # Extrae conceptos si es necesario
    if any(campo in campos_faltantes for campo in ['cuota_fija', 'riesgos_trabajo', 'guarderias', 'subtotal_imss', 'rcv']):
        concepts = extract_concepts_from_text(text)
        for campo, valor in concepts.items():
            if campo in campos_faltantes:
                datos_extraidos[campo] = valor
    
    # Guarda el texto extraído en archivo JSON
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
    
    return {
        'status': 'completado' if datos_extraidos else 'sin_datos',
        'datos_extraidos': datos_extraidos,
        'metodo_extraccion': metodo,
        'nota': f"Extraídos {len(datos_extraidos)} de {len(campos_faltantes)} campos faltantes"
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
    
    # Ruta base donde están los PDF
    base_folder = r"C:\Kpi\KpiExxerpro\Fixture"
    
    # Procesa cada fila
    resultados = []
    for index, row in df.iterrows():
        log(f"[PROCESANDO] Fila {index + 1}: {row['archivo']}")
        
        resultado = process_excel_row(row, base_folder)
        resultados.append(resultado)
        
        # Actualiza la fila con los datos extraídos
        if resultado['status'] == 'completado' and 'datos_extraidos' in resultado:
            for campo, valor in resultado['datos_extraidos'].items():
                df.at[index, campo] = valor
        
        # Agrega columnas de metadatos
        df.at[index, 'metodo_extraccion'] = resultado.get('metodo_extraccion', '')
        df.at[index, 'status'] = resultado.get('status', '')
        df.at[index, 'nota'] = resultado.get('nota', '')
        df.at[index, 'fecha_procesamiento'] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    # Guarda el Excel actualizado
    try:
        df.to_excel(EXCEL_OUTPUT, index=False)
        log(f"[EXITO] Excel actualizado guardado como: {EXCEL_OUTPUT}")
    except Exception as e:
        log(f"[ERROR] Error al guardar el Excel: {e}")
        return
    
    # Resumen final
    completados = sum(1 for r in resultados if r['status'] == 'completado')
    errores = sum(1 for r in resultados if r['status'] == 'error')
    sin_datos = sum(1 for r in resultados if r['status'] == 'sin_datos')
    
    log(f"[RESUMEN] Proceso completado:")
    log(f"  - Filas procesadas: {len(resultados)}")
    log(f"  - Completadas exitosamente: {completados}")
    log(f"  - Con errores: {errores}")
    log(f"  - Sin datos encontrados: {sin_datos}")
    log("=== PROCESO FINALIZADO ===")

if __name__ == "__main__":
    main()


#como se sube al repositorio de github
#el gato se usa para hacer un comentario en el codigo
#ayer les pide que anotaron todo en su libreta
# que anotaron los comando que uso
#empezando con git add .
# despues git commit -m "un mensaje descriptivo"
# despues git push origin main
# despues git status
# despues git log
