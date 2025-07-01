import os
import fitz  # PyMuPDF
import re
import pandas as pd

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

# Función principal para extraer datos de un PDF
def extract_data_from_pdf(pdf_path, save_text_sample=False, sample_index=0):
    """
    Extrae información relevante de un archivo PDF.
    Si save_text_sample es True, guarda el texto extraído en un archivo .txt para análisis.
    """
    try:
        # Abre el PDF y extrae el texto de todas las páginas
        doc = fitz.open(pdf_path)
        text = "\n".join(page.get_text("text") for page in doc)
        doc.close()

        # Guarda el texto extraído si es uno de los primeros 3 PDF
        if save_text_sample:
            if not os.path.exists(EXTRACTED_TEXT_DIR):
                os.makedirs(EXTRACTED_TEXT_DIR)
            base_name = os.path.splitext(os.path.basename(pdf_path))[0]
            sample_file = os.path.join(EXTRACTED_TEXT_DIR, f"sample_{sample_index}_{base_name}.txt")
            with open(sample_file, "w", encoding="utf-8") as f:
                f.write(text)

        # Si no se extrajo texto, registra un error y termina
        if not text.strip():
            log(f"[ERROR] Texto vacío: {pdf_path}")
            return None

        # Divide el texto en líneas para facilitar la búsqueda de conceptos
        lines = text.splitlines()
        concepts = extract_concepts(lines)

        # Diccionario con los datos extraídos del PDF
        data = {
            "archivo": os.path.basename(pdf_path),  # Nombre del archivo PDF
            # Busca el registro patronal usando una expresión regular
            "registro_patronal": extract_regex(text, r"REGISTRO PATRONAL:\s*RFC:\s*(\S+)"),
            # Busca el periodo IMSS
            "periodo_imss": extract_regex(text, r"PERÍODO QUE COMPRENDE\s+EL PAGO DE SEGUROS IMSS\s*(.+?)\s"),
            # Busca el periodo RCV
            "periodo_rcv": extract_regex(text, r"BIMESTRE QUE COMPRENDE\s+EL PAGO RCV E INFONAVIT\s*(.+?)\s"),
            # Busca los días a cotizar
            "dias_cotizar": extract_regex(text, r"No\\. DE DÍAS A COTIZAR:\s*(\d+)"),
            # Busca el número de cotizantes
            "num_cotizantes": extract_regex(text, r"No\\. DE COTIZANTES:\s*(\d+)"),
            # Busca el valor UMA
            "valor_uma": extract_regex(text, r"Valor UMA\s*(\d+\.\d+)"),
            # Busca los conceptos principales en el texto
            "cuota_fija": concepts.get("CUOTA FIJA", 0),
            "riesgos_trabajo": concepts.get("RIESGOS DE TRABAJO", 0),
            "guarderias": concepts.get("GUARDERÍAS Y PRESTACIONES SOCIALES", 0),
            "subtotal_imss": concepts.get("SUBTOTAL SEGUROS IMSS", 0),
            "rcv": concepts.get("SUBTOTAL RCV", 0),
            # Busca el total a pagar
            "total_pagar": extract_total(text)
        }

        return data
    except Exception as e:
        # Si ocurre un error, lo registra en el log
        log(f"[ERROR] {pdf_path}: {e}")
        return None

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
                    # Agrega el nombre de la carpeta como "mes" (puede ser útil para agrupar)
                    data["mes"] = os.path.basename(root)
                    all_data.append(data)
    # Convierte la lista de datos en un DataFrame de pandas
    return pd.DataFrame(all_data)

# Bloque principal que se ejecuta al correr el script directamente
if __name__ == "__main__":
    # Si existe el archivo de log, lo borra para empezar limpio
    if os.path.exists(LOG_PATH):
        os.remove(LOG_PATH)

    # Ruta de la carpeta donde están los PDF (ajustar si es necesario)
    folder_path = r"C:\Kpi\KpiExxerpro\Fixture"
    # Procesa todos los PDF y obtiene un DataFrame con los resultados
    df = process_folder(folder_path)

    # Si se extrajeron datos, los guarda en un archivo Excel
    if not df.empty:
        df.to_excel("resumen_pagos_seguros.xlsx", index=False)
        log(f"[FINALIZADO] {len(df)} registros guardados.")
    else:
        log("[FINALIZADO] Sin datos útiles.")
