import os
import pandas as pd
import json

# Configuración de rutas
EXCEL_PATH = "PRUEBA_resumen_pagos_seguros_completado.xlsx"
EXTRACTED_DATA_DIR = "extracted_data"

# Leer el Excel de salida
try:
    df = pd.read_excel(EXCEL_PATH)
except Exception as e:
    print(f"[ERROR] No se pudo leer el Excel: {e}")
    exit(1)

errores = []  # Definir la variable errores antes de su uso

# Resultados de verificación y generación de correctos/errores
filas_correctas = []
for idx, row in df.iterrows():
    archivo = row.get('archivo', '')
    registro_excel = str(row.get('registro_patronal', '')).strip()
    if not archivo:
        continue
    base = os.path.splitext(os.path.basename(archivo))[0]
    json_file = None
    for fname in os.listdir(EXTRACTED_DATA_DIR):
        if fname.startswith(base) and fname.endswith('.json'):
            json_file = os.path.join(EXTRACTED_DATA_DIR, fname)
            break
    if not json_file or not os.path.exists(json_file):
        errores.append({
            'fila': idx+1,
            'archivo': archivo,
            'motivo': 'No se encontró JSON correspondiente',
            'registro_excel': registro_excel,
            'registro_json': '',
            'json_path': json_file if json_file else ''
        })
        continue
    try:
        with open(json_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        registro_json = str(data.get('datos_extraidos', {}).get('registro_patronal', '')).strip()
    except Exception as e:
        errores.append({
            'fila': idx+1,
            'archivo': archivo,
            'motivo': f'Error al leer JSON: {e}',
            'registro_excel': registro_excel,
            'registro_json': '',
            'json_path': json_file
        })
        continue
    # Si el JSON tiene un registro patronal válido, lo usamos para corregir
    row_copy = row.copy()
    row_copy['json_path'] = json_file
    if not registro_excel.startswith('E23') and registro_json.startswith('E23'):
        print(f"Corrigiendo fila {idx+1}: Excel='{registro_excel}' -> JSON='{registro_json}'")
        row_copy['registro_patronal'] = registro_json
    filas_correctas.append(row_copy)

# Exportar errores a Excel
if errores:
    df_errores = pd.DataFrame(errores)
    df_errores.to_excel("verificacion_errores.xlsx", index=False)
    print("[INFO] Archivo de errores guardado como verificacion_errores.xlsx")
else:
    print("[INFO] No se encontraron errores para exportar.")

# Exportar datos correctos a Excel y corregir registro_patronal si es necesario
if filas_correctas:
    df_correctos = pd.DataFrame(filas_correctas)
    df_correctos.to_excel("verificacion_correctos.xlsx", index=False)
    print("[INFO] Archivo de datos correctos guardado como verificacion_correctos.xlsx")
else:
    print("[INFO] No se encontraron filas correctas para exportar.")
