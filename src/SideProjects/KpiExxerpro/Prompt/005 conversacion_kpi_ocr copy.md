Reglas Optimizadas para el Agente de Extracción de KPIs desde PDFs
1. Propósito General del Agente
Identificar y extraer automáticamente campos clave desde documentos PDF (digitales o escaneados) para generar series históricas de indicadores (KPIs).

Adaptarse dinámicamente a distintos formatos, inconsistencias estructurales o escaneos de baja calidad.

Mejorar progresivamente mediante aprendizaje de patrones nuevos identificados.

2. Clasificación y Manejo de Archivos
PDFs digitales → Priorizar extracción con fitz (PyMuPDF) u otras APIs OCR-aware.

PDFs escaneados → Fallback automático a OCR (pdf2image + pytesseract).

Incluir lógica de clasificación automática con heurísticas de contenido/textura.

3. Extracción de Texto con Fallback Inteligente
Etapa 1: fitz.page.get_text("text").

Si el texto es sospechosamente corto o vacío → activar OCR.

Etapa 2: OCR con configuración multilenguaje (spa, eng).

Registro del método usado (texto, ocr, mixto) por documento.

4. Entorno de Ejecución y Tolerancia a Errores
Validación automática de dependencias al inicio.

Si faltan binarios (Tesseract, Poppler) o librerías, ofrecer mensajes precisos de instalación.

Incluir logs de entorno (Python, SO, paths relevantes).

5. Detección y Adaptabilidad de Campos Clave
Mantener una lista modular y ampliable de campos con alias y expresiones regulares tolerantes.

Soportar expresiones como:

regex
Copiar
Editar
(registro\s*patronal|rfc|registro\s+IMSS)\s*[:\-]?\s*(\w+)
Autoaprendizaje: guardar patrones nuevos detectados para revisión manual.

6. Exploración de Archivos
Uso recursivo de os.walk() optimizado con filtros por extensión y tamaño mínimo.

Soporte de reanudación por checkpoint (processed_files.log).

7. Generación de Resultados
Escribir resumen_pagos_seguros.csv incrementalmente tras cada archivo.

Exportar resumen_pagos_seguros.xlsx al cierre.

Guardar texto completo y campos extraídos por archivo (.json o .pickle) en extracted_data/.

8. Manejo de Errores
Logs detallados en log_contextual.txt con timestamp, archivo, error y stacktrace simplificado.

Añadir columna status y nota al DataFrame para cada entrada procesada.

Retener errores críticos para análisis posterior sin interrumpir ejecución.

9. Modularidad del Código
Separar lógica en funciones especializadas por tarea: clasificación, OCR, extracción por regex, logging, etc.

Facilitar mantenimiento y evolución.

10. Soporte, Comentarios y Usabilidad
Comentarios detallados en español en cada bloque del script.

Funciones autocontenidas con nombres significativos.

Incluir resumen general del flujo al inicio del archivo Python.

Facilitar activación/desactivación de módulos experimentales mediante config.py.

11. Buenas Prácticas de Ingeniería
Validar campos antes de insertar al DataFrame (normalización de valores, padding de ceros, tipos).

Priorizar claridad sobre concisión.

Reutilizar módulos y patrones comprobados.