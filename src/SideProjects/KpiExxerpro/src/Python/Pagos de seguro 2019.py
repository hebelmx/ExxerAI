import os
import fitz  # PyMuPDF
import pandas as pd

def extract_data_from_pdf(pdf_path):
    doc = fitz.open(pdf_path)
    text = "\n".join(page.get_text() for page in doc)
    doc.close()

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
    return data

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
                return sum(numbers)  # total column value
    except:
        return None
    return None

def process_folder(base_path):
    all_data = []
    for root, dirs, files in os.walk(base_path):
        for file in files:
            if file.lower().endswith(".pdf"):
                pdf_path = os.path.join(root, file)
                print(f"Procesando: {pdf_path}")
                data = extract_data_from_pdf(pdf_path)
                data["mes"] = os.path.basename(root)
                all_data.append(data)
    return pd.DataFrame(all_data)

if __name__ == "__main__":
    folder_path = "C:r/Users/natal/OneDrive/Documentos/PAGOS 2019"  # Cambia esto al path real
    df = process_folder(folder_path)
    df.to_excel("resumen_pagos_seguros.xlsx", index=False)
    print("Extracción completada. Archivo generado: resumen_pagos_seguros.xlsx")
