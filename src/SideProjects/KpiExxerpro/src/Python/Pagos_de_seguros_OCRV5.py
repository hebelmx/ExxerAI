import os
import fitz  # PyMuPDF
import re
import pandas as pd
# Nuevas importaciones para OCR
import pytesseract
from pdf2image import convert_from_path
from PIL import Image

# Configura la ruta de Tesseract-OCR (ajusta si es necesario)
pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"

# Configura la ruta de Poppler (necesario para pdf2image)
POPPLER_PATH = r"C:\Program Files\Release-24.08.0-0\poppler-24.08.0\Library\bin"

# Imprime información sobre la librería fitz (PyMuPDF), útil para depuración
print(fitz.__doc__)

# Ruta del archivo de log donde se guardan mensajes de proceso y errores
LOG_PATH = "log_contextual.txt"
# Carpeta donde se guardarán muestras de texto extraído de los primeros PDF
EXTRACTED_TEXT_DIR = "extracted_text_samples"
OCR_RESULTADOS_PATH = "ocr_resultados.txt"

# Función para registrar mensajes en un archivo de log y en pantalla
def log(message):
    with open(LOG_PATH, "a", encoding="utf-8") as f:
        f.write(message + "\n")
    print(message)

# Función principal para extraer datos de un PDF (ahora con OCR)
def extract_data_from_pdf(pdf_path, save_text_sample=False, sample_index=0):
    """
    Extrae información relevante de un archivo PDF.
    Si no se puede extraer texto digital, intenta con OCR.
    Si save_text_sample es True, guarda el texto extraído en un archivo .txt para análisis.
    Devuelve una tupla (data, ocr_status) donde ocr_status puede ser 'ocr_exitoso', 'ocr_fallido', 'no_ocr'.
    """
    try:
        # Abre el PDF y extrae el texto de todas las páginas (digital)
        doc = fitz.open(pdf_path)
        text = "\n".join(page.get_text("text") for page in doc)
        doc.close()
        ocr_status = 'no_ocr'
        # Si no se extrajo texto, intenta con OCR
        if not text.strip():
            log(f"[INFO] Intentando OCR en: {pdf_path}")
            try:
                images = convert_from_path(pdf_path, poppler_path=POPPLER_PATH)
                ocr_text = ""
                for i, image in enumerate(images):
                    ocr_text += pytesseract.image_to_string(image, lang="spa") + "\n"
                text = ocr_text
                if text.strip():
                    ocr_status = 'ocr_exitoso'
                else:
                    ocr_status = 'ocr_fallido'
            except Exception as ocr_e:
                log(f"[ERROR] OCR falló en {pdf_path}: {ocr_e}")
                ocr_status = 'ocr_fallido'
                return None, ocr_status
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
            return None, ocr_status
        lines = text.splitlines()
        concepts = extract_concepts(lines)
        total_pagar_val = extract_total(text)
        total_pagar_str = f"${total_pagar_val:,.2f}"
        data = {
            "archivo": os.path.basename(pdf_path),
            "registro_patronal": extract_regex(text, r"REGISTRO PATRONAL:\s*RFC:\s*(\S+)"),
            "periodo_imss": extract_regex(text, r"PERÍODO QUE COMPRENDE\s+EL PAGO DE SEGUROS IMSS\s*(.+?)\s"),
            "periodo_rcv": extract_regex(text, r"BIMESTRE QUE COMPRENDE\s+EL PAGO RCV E INFONAVIT\s*(.+?)\s"),
            "dias_cotizar": extract_regex(text, r"No\\. DE DÍAS A COTIZAR:\s*(\d+)"),
            "num_cotizantes": extract_regex(text, r"No\\. DE COTIZANTES:\s*(\d+)"),
            "valor_uma": extract_regex(text, r"Valor UMA\s*(\d+\.\d+)"),
            "cuota_fija": concepts.get("CUOTA FIJA", 0),
            "riesgos_trabajo": concepts.get("RIESGOS DE TRABAJO", 0),
            "guarderias": concepts.get("GUARDERÍAS Y PRESTACIONES SOCIALES", 0),
            "subtotal_imss": concepts.get("SUBTOTAL SEGUROS IMSS", 0),
            "rcv": concepts.get("SUBTOTAL RCV", 0),
            "total_pagar": total_pagar_str,
            "ruta_completa": pdf_path
        }
        return data, ocr_status
    except Exception as e:
        log(f"[ERROR] {pdf_path}: {e}")
        return None, 'ocr_fallido'

# Función para buscar un patrón de texto usando expresiones regulares
def extract_regex(text, pattern):
    match = re.search(pattern, text, re.IGNORECASE)
    return match.group(1).strip() if match else ""

# Función para extraer los conceptos principales de las líneas del PDF
def extract_concepts(lines):
    # Diccionario con los conceptos a buscar y su valor inicial
    concepts_to_find = {
        "CUOTA FIJA": 0,
        "RIESGOS DE TRABAJO": 0,
        "GUARDERÍAS Y PRESTACIONES SOCIALES": 0,
        "SUBTOTAL SEGUROS IMSS": 0,
        "SUBTOTAL RCV": 0,
    }

    buffer = []  # Almacena líneas para buscar montos
    last_concept = None  # Concepto que se está buscando actualmente
    for i, line in enumerate(lines):
        normalized = line.upper()
        # Detecta si la línea contiene uno de los conceptos buscados
        for concept in concepts_to_find:
            if concept in normalized:
                last_concept = concept
                buffer = []
                continue
        # Si estamos buscando el monto de un concepto
        if last_concept:
            buffer.append(line)
            if len(buffer) <= 3:
                combined = " ".join(buffer)
                # Busca números con formato de cantidad (ej: 1,234.56)
                numbers = [float(n.replace(",", "")) for n in re.findall(r"\d{1,3}(?:,\d{3})*\.\d{2}", combined)]
                if numbers:
                    concepts_to_find[last_concept] = sum(numbers)
                    last_concept = None  # Reinicia la búsqueda

    return concepts_to_find

# Función para extraer el total a pagar del texto del PDF
def extract_total(text):
    # Busca patrones de montos en formato de tabla
    matches = re.findall(r"\$\s?([\d,]+\.\d{2})\s?\$\s?([\d,]+\.\d{2})\s?\$\s?([\d,]+\.\d{2})", text)
    if matches:
        last = matches[-1]
        return float(last[2].replace(",", ""))
    return 0.0

# Función para procesar todos los PDF en una carpeta y subcarpetas
def process_folder(base_path):
    all_data = []
    sample_count = 0
    ocr_exitosos = []  # Lista de rutas completas
    ocr_fallidos = []  # Lista de rutas completas
    cedulas_pattern = re.compile(r"^(0?1\.? ?C(e|é|É)dula(s)?)$|^C(E|É)DULAS$", re.IGNORECASE)
    log(f"[DIAG] Recorriendo recursivamente desde: {base_path}")
    for root, dirs, files in os.walk(base_path):
        folder_name = os.path.basename(root)
        if cedulas_pattern.match(folder_name):
            log(f"[DIAG] Carpeta de cédulas encontrada: {root}")
            for cedulas_root, _, cedulas_files in os.walk(root):
                log(f"[DIAG] Buscando PDFs en: {cedulas_root}")
                log(f"[DIAG] Archivos encontrados en la carpeta: {cedulas_files}")
                pdfs = [f for f in cedulas_files if f.lower().endswith('.pdf')]
                log(f"[DIAG] Archivos PDF filtrados: {pdfs}")
                log(f"[DIAG] Cantidad de archivos PDF encontrados: {len(pdfs)}")
                for file in pdfs:
                    pdf_path = os.path.join(cedulas_root, file)
                    log(f"Procesando: {pdf_path}")
                    save_text_sample = sample_count < 3
                    data, ocr_status = extract_data_from_pdf(pdf_path, save_text_sample=save_text_sample, sample_index=sample_count)
                    if save_text_sample:
                        sample_count += 1
                    if ocr_status == 'ocr_exitoso':
                        ocr_exitosos.append(pdf_path)
                    elif ocr_status == 'ocr_fallido':
                        ocr_fallidos.append(pdf_path)
                    if data:
                        path_parts = cedulas_root.replace(base_path, '').strip(os.sep).split(os.sep)
                        data["año"] = path_parts[0] if len(path_parts) > 1 else ''
                        data.pop("mes", None)
                        data.pop("carpeta_cedulas", None)
                        data["ruta_completa"] = pdf_path
                        all_data.append(data)
            # dirs.clear()  # No limpiar para permitir subcarpetas
    # Guardar resultados de OCR en un TXT
    try:
        with open(OCR_RESULTADOS_PATH, "w", encoding="utf-8") as f:
            f.write("Archivos procesados por OCR exitosamente:\n")
            for archivo in ocr_exitosos:
                f.write(f"[OK] {archivo}\n")
            f.write("\nArchivos procesados por OCR que fallaron:\n")
            for archivo in ocr_fallidos:
                f.write(f"[FALLÓ] {archivo}\n")
    except Exception as e:
        log(f"[ERROR] No se pudo guardar el archivo de resultados OCR: {e}")
    df = pd.DataFrame(all_data)
    if not df.empty:
        cols = [c for c in df.columns if c not in ["ruta_completa", "mes", "carpeta_cedulas"]] + ["ruta_completa"]
        df = df[cols]
    return df

# Bloque principal que se ejecuta al correr el script directamente
if __name__ == "__main__":
    # Si existe el archivo de log, lo borra para empezar limpio
    if os.path.exists(LOG_PATH):
        os.remove(LOG_PATH)

    # Ruta de la carpeta donde están los PDF (corregida)
    folder_path = r"C:\Users\moren\Documents\GitHub\KpiExxerpro\Fixture"
    # Procesa todos los PDF y obtiene un DataFrame con los resultados
    df = process_folder(folder_path)

    # Si se extrajeron datos, los guarda en un archivo Excel
    if not df.empty:
        df.to_excel("resumen_pagos_seguros.xlsx", index=False)
        log(f"[FINALIZADO] {len(df)} registros guardados.")
    else:
        log("[FINALIZADO] Sin datos útiles.")
