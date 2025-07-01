# Configuración para el script de extracción de datos de PDFs
# Archivo: config_mejorado.py

# =============================================================================
# CONFIGURACIÓN DE RUTAS
# =============================================================================

# Ruta del archivo Excel de entrada (con los datos a completar)
EXCEL_INPUT = r"C:\Users\moren\Documents\GitHub\KpiExxerpro\resumen_pagos_seguros.xlsx"

# Ruta del archivo Excel de salida (resultado final)
EXCEL_OUTPUT = "RESUMEN_PAGOS_SEGUROS_COMPLETADO.xlsx"

# Ruta base donde están los archivos PDF
BASE_FOLDER = r"C:\Users\moren\Documents\GitHub\KpiExxerpro\Fixture"

# =============================================================================
# CONFIGURACIÓN DE OCR
# =============================================================================

# Ruta al ejecutable de Tesseract OCR
TESSERACT_PATH = r"C:\Program Files\Tesseract-OCR\tesseract.exe"

# Ruta a Poppler (necesario para pdf2image)
POPPLER_PATH = r"C:\Program Files\poppler\Library\bin"

# =============================================================================
# CONFIGURACIÓN DE ARCHIVOS DE LOG Y DATOS
# =============================================================================

# Archivo de log principal
LOG_PATH = "log_mejorado.txt"

# Carpeta donde se guardan los datos extraídos (JSON)
EXTRACTED_DATA_DIR = "extracted_data"

# =============================================================================
# CONFIGURACIÓN DE FILTROS
# =============================================================================

# Año específico a procesar (None = procesar todos los años)
# Puedes cambiar este valor directamente aquí o usar la opción interactiva
AÑO_FILTRO = None  # Ejemplo: 2023, 2024, etc. o None para todos

# Filtrar solo documentos de cédulas (True/False)
FILTRAR_CEDULAS = True

# =============================================================================
# CONFIGURACIÓN DE CAMPOS A EXTRAER
# =============================================================================

# Lista de campos básicos que se extraen
CAMPOS_BASICOS = [
    "registro_patronal",
    "periodo_imss", 
    "periodo_rcv",
    "dias_cotizar",
    "num_cotizantes",
    "valor_uma"
]

# Lista de conceptos monetarios que se extraen
CONCEPTOS_MONETARIOS = [
    "cuota_fija",
    "riesgos_trabajo", 
    "guarderias",
    "subtotal_imss",
    "rcv",
    "total_pagar"
]

# =============================================================================
# CONFIGURACIÓN DE PATRONES DE BÚSQUEDA
# =============================================================================

# Patrones para extraer el registro patronal
PATRONES_REGISTRO_PATRONAL = [
    r"REGISTRO\s+PATRONAL[:\s]RFC[:\s]([A-Z0-9\-]+)",
    r"RFC[:\s]*([A-Z0-9\-]{10,15})",
    r"REGISTRO\s+PATRONAL[:\s]*([A-Z0-9\-]+)",
    r"REGISTRO\s+IMSS[:\s]*([A-Z0-9\-]+)"
]

# Patrones para extraer el período IMSS
PATRONES_PERIODO_IMSS = [
    r"PER[ÍI]ODO\s+(QUE\s+)?COMPRENDE\s+EL\s+PAGO\s+DE\s+SEGUROS\s+IMSS[:\s]*([\w\s/]+)",
    r"PER[ÍI]ODO[:\s]*([\w\s/]+)",
    r"PAGO[:\s]*([\w\s/]+)",
    r"PER[ÍI]ODO\s+DE\s+PAGO[:\s]*([\w\s/]+)"
]

# Patrones para extraer el período RCV
PATRONES_PERIODO_RCV = [
    r"BIMESTRE\s+(QUE\s+)?COMPRENDE\s+EL\s+PAGO\s+RCV.[:\s]([\w\s/]+)",
    r"BIMESTRE[:\s]*([\w\s/]+)",
    r"PER[ÍI]ODO\s+RCV[:\s]*([\w\s/]+)"
]

# Patrones para extraer días a cotizar
PATRONES_DIAS_COTIZAR = [
    r"N[oº\.]\s*DE\s*D[ÍI]AS\s*A\s*COTIZAR[:\s]([0-9]{1,3})",
    r"D[ÍI]AS\s+A\s*COTIZAR[:\s]*([0-9]{1,3})",
    r"D[ÍI]AS\s+COTIZADOS[:\s]*([0-9]{1,3})"
]

# Patrones para extraer número de cotizantes
PATRONES_NUM_COTIZANTES = [
    r"N[oº\.]\s*DE\s*COTIZANTES[:\s]([0-9]{1,5})",
    r"COTIZANTES[:\s]*([0-9]{1,5})",
    r"TRABAJADORES[:\s]*([0-9]{1,5})",
    r"N[oº\.]\s*DE\s+TRABAJADORES[:\s]*([0-9]{1,5})"
]

# Patrones para extraer valor UMA
PATRONES_VALOR_UMA = [
    r"VALOR\s+UMA[:\s]\$?\s([\d,]+\.\d{2})",
    r"UMA[:\s]\$?\s([\d,]+\.\d{2})",
    r"VALOR\s+DE\s+LA\s+UMA[:\s]*\$?\s*([\d,]+\.\d{2})"
]

# =============================================================================
# CONFIGURACIÓN DE CONCEPTOS MONETARIOS
# =============================================================================

# Patrones para cuota fija
PATRONES_CUOTA_FIJA = [
    r"CUOTA\s+FIJA[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"CUOTA\s+FIJA\s+IMSS[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"CUOTA\s+FIJA\s+DE\s+IMSS[:\s]*\$?\s*([\d,]+\.\d{2})"
]

# Patrones para riesgos de trabajo
PATRONES_RIESGOS_TRABAJO = [
    r"RIESGOS\s+DE\s+TRABAJO[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"RIESGOS\s+TRABAJO[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"PRIMA\s+DE\s+RIESGO[:\s]*\$?\s*([\d,]+\.\d{2})"
]

# Patrones para guarderías
PATRONES_GUARDERIAS = [
    r"GUARDER[ÍI]AS\s+Y\s+PRESTACIONES\s+SOCIALES[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"GUARDER[ÍI]AS[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"PRESTACIONES\s+SOCIALES[:\s]*\$?\s*([\d,]+\.\d{2})"
]

# Patrones para subtotal IMSS
PATRONES_SUBTOTAL_IMSS = [
    r"SUBTOTAL\s+SEGUROS\s+IMSS[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"SUBTOTAL\s+IMSS[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"TOTAL\s+IMSS[:\s]*\$?\s*([\d,]+\.\d{2})"
]

# Patrones para RCV
PATRONES_RCV = [
    r"SUBTOTAL\s+RCV[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"RCV[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"RETIRO[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"CESANT[ÍI]A[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"VEJEZ[:\s]*\$?\s*([\d,]+\.\d{2})"
]

# Patrones para total a pagar
PATRONES_TOTAL_PAGAR = [
    r"TOTAL\s+A\s+PAGAR[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"TOTAL\s+PAGAR[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"IMPORTE\s+TOTAL[:\s]*\$?\s*([\d,]+\.\d{2})",
    r"TOTAL\s+GENERAL[:\s]*\$?\s*([\d,]+\.\d{2})"
]

# =============================================================================
# CONFIGURACIÓN DE MENSAJES
# =============================================================================

# Mensajes informativos
MENSAJES = {
    "inicio": "=== INICIANDO PROCESO DE COMPLETADO DE DATOS MEJORADO ===",
    "configuracion": "CONFIGURACIÓN DEL PROCESO",
    "opciones_año": "¿Qué año de documentos quieres procesar?",
    "opcion_todos": "1. Procesar TODOS los años (escribe: TODOS)",
    "opcion_especifico": "2. Procesar un año específico (escribe el año, ejemplo: 2023)",
    "resumen": "RESUMEN DEL PROCESO",
    "fin": "=== PROCESO FINALIZADO ==="
}

# =============================================================================
# INSTRUCCIONES DE USO
# =============================================================================

"""
INSTRUCCIONES DE USO:

1. CONFIGURACIÓN INICIAL:
   - Asegúrate de tener instalado Tesseract OCR y Poppler
   - Verifica que las rutas en este archivo sean correctas para tu sistema
   - Coloca tu archivo Excel de entrada en la ruta especificada en EXCEL_INPUT

2. EJECUCIÓN:
   - Ejecuta el script: python Pagos_de_seguro_Mejorado.py
   - El script te preguntará qué año quieres procesar
   - Puedes escribir "TODOS" para procesar todos los años
   - O escribir un año específico como "2023"

3. RESULTADOS:
   - Se generará un archivo Excel con todos los datos extraídos
   - Se creará un archivo de log con detalles del proceso
   - Se guardarán archivos JSON con el texto extraído de cada PDF

4. CAMPOS EXTRAÍDOS:
   - Registro patronal
   - Períodos (IMSS y RCV)
   - Días a cotizar
   - Número de cotizantes
   - Valor UMA
   - Conceptos monetarios (cuota fija, riesgos, guarderías, etc.)
   - Totales

5. SOLUCIÓN DE PROBLEMAS:
   - Revisa el archivo de log para ver errores específicos
   - Verifica que los PDFs estén en las rutas correctas
   - Asegúrate de que Tesseract OCR esté instalado correctamente
""" 