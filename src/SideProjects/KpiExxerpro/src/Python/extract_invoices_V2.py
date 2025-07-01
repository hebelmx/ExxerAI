import os
import re
import fitz  # PyMuPDF
import pandas as pd
from pathlib import Path

# Este es un script de pyhon que trata de obtener los datos
# De una factura PDF de Exxerpro y llevarlos a una hoja de excel
# De manera estructurada para hacer registros y analisis
# y semi automatizar la captura
# Inicializa el diccionario por año
data_by_year = {} #este es un campo para almacenar el año

# Puedes cambiar esto según donde esté tu carpeta raiz
# Directorio base que contiene carpetas por año
# Aqui debemos poner la ruta donde buscara los archivos
# Es muy importante poner la r para que no marque errores con los "/"
BASE_DIR = Path(r"C:/Users/natal/OneDrive/Documentos/KPI_ABEL/2020") 

 
# Recorrer todos los archivos PDF esta es  una rutina para recorrer el directorio y buscar los archivos
for root, dirs, files in os.walk(BASE_DIR):
    for file in files:
        if file.endswith(".pdf"):
            file_path = Path(root) / file
            parts = file_path.parts

            # Extraer metadatos de ruta
            try:
                year, client, folio = parts[-4], parts[-3], parts[-2]
            except ValueError:
                continue  # Saltar si no sigue la estructura esperada

            # Leer PDF aqui lee los archivos pdf
            try:
                doc = fitz.open(file_path)
                text = "\n".join(page.get_text() for page in doc)
            except Exception as e:
                print(f"Error leyendo {file_path}: {e}")
                continue

            # Detectar tipo de documento
            tipo_doc = "Factura" # de acuerdo a las palabras que encuentre clasifica el documento
            if "comprobante de pago" in text.lower():
                tipo_doc = "Comprobante de Pago"
            elif "cancelada" in text.lower():
                tipo_doc = "Cancelada"

            # Buscar datos claves con regex más tolerantes
            # Utiliza un algoritmo de busqueda conocido como Regex para buscar los datos
            # Este lo calculo de acuerdo a la información que le presentamos 
            # Cuando estuvimos interacuanto con el
            status = "Cancelada" if "cancelada" in text.lower() else "Confirmada"
            date = re.search(r"Fecha\s+(\d{4}-\d{2}-\d{2})", text)
            date_stamp = re.search(r"Fecha timbrado\s+(\d{4}-\d{2}-\d{2})", text)
            order = re.search(r"Orden De Compra\s+(\d+)", text)
            subtotal = re.search(r"Subtotal \$([\d,\.]+)", text)
            iva = re.search(r"IVA.*?\$([\d,\.]+)", text)
            total = re.search(r"Total \$([\d,\.]+)", text)
            uso_cfdi = re.search(r"Uso de CFDI\s+(\w+)", text)
            metodo_pago = re.search(r"M[eé]todo de pago:?\s+(.*?)\n", text, re.IGNORECASE)
            forma_pago = re.search(r"Forma de pago:?\s+(.*?)\n", text, re.IGNORECASE)
            rfc = re.search(r"RFC\s+([A-Z0-9]{12,13})", text)

            # Buscar descripciones
            # Hace mas busquedas
            desc_match = re.search(r"(?:Descripcion|Descripción)[\s:]*\n?(.+?)(?:\n[A-Z0-9]{4,}|\n\$|\nTotal|\nIVA|\nForma|\nCondiciones|$)", text, re.IGNORECASE | re.DOTALL)
            description = desc_match.group(1).strip() if desc_match else ""
            
            #Antes de eso solo hizo las definiciones en este momento es cuando va a capturar los datos
            record = {
                "File Path": str(file_path),
                "Tipo Documento": tipo_doc,
                "Cliente": client,
                "Folio": folio,
                "Fecha": date.group(1) if date else "",
                "Fecha Timbrado": date_stamp.group(1) if date_stamp else "",
                "Orden de Compra": order.group(1) if order else "",
                "Descripcion": description,
                "Subtotal (MXN)": subtotal.group(1).replace(",", "") if subtotal else "",
                "IVA (MXN)": iva.group(1).replace(",", "") if iva else "",
                "Total (MXN)": total.group(1).replace(",", "") if total else "",
                "Forma de Pago": forma_pago.group(1).strip() if forma_pago else "",
                "Metodo de Pago": metodo_pago.group(1).strip() if metodo_pago else "",
                "Uso CFDI": uso_cfdi.group(1) if uso_cfdi else "",
                "RFC Receptor": rfc.group(1) if rfc else "",
                "Estatus": status
            }

            if year not in data_by_year:
                data_by_year[year] = []
            data_by_year[year].append(record)

# Crear un archivo Excel con una hoja por año
# Todo lo que encontro lo tiene guardado en memoria
# Aqui se prepara para escribirlo en excel
# Lo va a escribir todo en un ciclo
with pd.ExcelWriter("facturas_clasificadas.xlsx") as writer:
    for year, records in data_by_year.items():
        df = pd.DataFrame(records)
        df.to_excel(writer, sheet_name=str(year), index=False)
#nos manda un mensaje cuando ya termino de escribir y termina el cuclo
print("Procesamiento completo. Archivo 'facturas_clasificadas.xlsx' generado.")
