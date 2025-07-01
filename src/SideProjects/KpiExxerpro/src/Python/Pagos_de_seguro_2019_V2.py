import os
import fitz  # PyMuPDF
import pandas as pd

LOG_PATH = "log_extraccion.txt"

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
            log(f"[ERROR] Sin texto legible: {pdf_path}")
            return None

        data = {
            "archivo": os.path.basename(pdf_path),
            "rfc": extract_between(text, "RFC:", "\n"),
            "periodo_imss": extract_between(text, "PERÍODO QUE COMPRENDE\nEL PAGO DE SEGUROS IMSS", "\n").strip(),
            "periodo_rcv": extract_between(text, "BIMESTRE QUE COMPRENDE\nEL PAGO RCV E INFONAVIT", "\n").strip(),
            "dias_cotizar": extract_between(text, "No. DE DÍAS A COTIZAR:", "\n").strip(),
            "num_cotizantes": extract_between(text, "No. DE COTIZANTES:", "\n").strip(),
            "valor_uma": extract_between(text, "Valor UMA", "\n").strip(),
            "cuota_fija": extract_concept(text, "CUOTA FIJA"),
            "riesgos_trabajo": extract_concept(text, "RIESGOS DE TRABAJO"),
            "guarderias": extract_concept(text, "GUARDERÍAS Y PRESTACIONES SOCIALES"),
            "subtotal_imss": extract_concept(text, "SUBTOTAL SEGUROS IMSS"),
            "rcv": extract_concept(text, "SUBTOTAL RCV"),
            "total_pagar": extract_concept(text, "TOTAL A PAGAR")
        }

        log(f"[OK] Datos extraídos de {pdf_path}")
        for k, v in data.items():
            log(f"    {k}: {v}")

        return data
    except Exception as e:
        log(f"[ERROR] Excepción en {pdf_path}: {e}")
        return None

def extract_between(text, start, end):
    try:
        return text.split(start)[1].split(end)[0]
    except IndexError:
        return ""

def extract_concept(text, concept):
    try:
        for line in text.splitlines():
            if concept in line:
                numbers = [float(n.replace(",", "")) for n in line.split() if n.replace(",", "").replace(".", "").isdigit()]
                return sum(numbers)
    except:
        return None
    return None

def process_folder(base_path):
    all_data = []
    for root, dirs, files in os.walk(base_path):
        for file in files:
            if file.lower().endswith(".pdf"):
                pdf_path = os.path.join(root, file)
                log(f"Procesando archivo: {pdf_path}")
                data = extract_data_from_pdf(pdf_path)
                if data:
                    data["mes"] = os.path.basename(root)
                    all_data.append(data)
    return pd.DataFrame(all_data)

if __name__ == "__main__":
    if os.path.exists(LOG_PATH):
        os.remove(LOG_PATH)

    folder_path = r"C:/Users/natal/OneDrive/Documentos/PAGOS 2019"  # Ajusta según sea necesario
    df = process_folder(folder_path)

    if df.empty:
        log("[FINALIZADO] Carpeta vacia. Revisa la ruta.")
    else:
        df.to_excel("resumen_pagos_seguros.xlsx", index=False)
        df.to_csv("resumen_debug.csv", index=False)
        log(f"[FINALIZADO] Datos guardados: {len(df)} registros")
