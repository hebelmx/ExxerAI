import os
import fitz  # PyMuPDF
import re
import pandas as pd

LOG_PATH = "log_extraccion_refinado.txt"

def log(message):
    with open(LOG_PATH, "a", encoding="utf-8") as f:
        f.write(message + "\n")
    print(message)

def extract_data_from_pdf(pdf_path):
    try:
        doc = fitz.open(pdf_path)
        text = "\n".join(page.get_text() for page in doc)
        doc.close()
        if not text.strip():
            log(f"[ERROR] Vacío: {pdf_path}")
            return None

        data = {
            "archivo": os.path.basename(pdf_path),
            "registro_patronal": extract_regex(text, r"REGISTRO PATRONAL:\s*RFC:\s*(\S+)"),
            "periodo_imss": extract_regex(text, r"PERÍODO QUE COMPRENDE\s+EL PAGO DE SEGUROS IMSS\s*(.+?)\s"),
            "periodo_rcv": extract_regex(text, r"BIMESTRE QUE COMPRENDE\s+EL PAGO RCV E INFONAVIT\s*(.+?)\s"),
            "dias_cotizar": extract_regex(text, r"No\. DE DÍAS A COTIZAR:\s*(\d+)"),
            "num_cotizantes": extract_regex(text, r"No\. DE COTIZANTES:\s*(\d+)"),
            "valor_uma": extract_regex(text, r"Valor UMA\s*(\d+\.\d+)"),
            "cuota_fija": extract_concept_value(text, "CUOTA FIJA"),
            "riesgos_trabajo": extract_concept_value(text, "RIESGOS DE TRABAJO"),
            "guarderias": extract_concept_value(text, "GUARDERÍAS Y PRESTACIONES SOCIALES"),
            "subtotal_imss": extract_concept_value(text, "SUBTOTAL SEGUROS IMSS"),
            "rcv": extract_concept_value(text, "SUBTOTAL RCV"),
            "total_pagar": extract_total(text)
        }

        log(f"[OK] {pdf_path}")
        return data
    except Exception as e:
        log(f"[ERROR] {pdf_path}: {e}")
        return None

def extract_regex(text, pattern):
    match = re.search(pattern, text, re.IGNORECASE)
    return match.group(1).strip() if match else ""

def extract_concept_value(text, concept):
    for line in text.splitlines():
        if concept.upper() in line.upper():
            numbers = [float(n.replace(",", "")) for n in re.findall(r"\d{1,3}(?:,\d{3})*\.\d{2}", line)]
            return sum(numbers)
    return 0.0

def extract_total(text):
    # Busca tres cantidades seguidas en una misma línea o líneas consecutivas
    matches = re.findall(r"\$\s?([\d,]+\.\d{2})\s?\$\s?([\d,]+\.\d{2})\s?\$\s?([\d,]+\.\d{2})", text)
    if matches:
        last_match = matches[-1]
        return float(last_match[2].replace(",", ""))
    return 0.0

def process_folder(base_path):
    all_data = []
    for root, dirs, files in os.walk(base_path):
        for file in files:
            if file.lower().endswith(".pdf"):
                pdf_path = os.path.join(root, file)
                log(f"Procesando: {pdf_path}")
                data = extract_data_from_pdf(pdf_path)
                if data:
                    data["mes"] = os.path.basename(root)
                    all_data.append(data)
    return pd.DataFrame(all_data)

if __name__ == "__main__":
    if os.path.exists(LOG_PATH):
        os.remove(LOG_PATH)

    folder_path = r"E:/Dynamic/PAGO DE SEGURO 2017"  # Ajusta según sea necesario
    df = process_folder(folder_path)

    if not df.empty:
        df.to_excel("resumen_pagos_seguros_refinado.xlsx", index=False)
        log(f"[FINALIZADO] {len(df)} archivos procesados.")
    else:
        log("[FINALIZADO] Sin resultados válidos. Verifica logs.")
