# Proyecto de Extracción de Datos para KPIs Históricos

## 🌎 Contexto General
Grupo de estudiantes de administración desarrollando un proyecto para calcular KPIs históricos usando Python para extraer datos de archivos PDF hacia hojas de Excel. El enfoque es práctico y de autoaprendizaje (“vibe coding”).

Actúa como un agente de extracción inversa. Tu tarea es:

1. Leer un archivo Excel que contiene nombres de archivos PDF, rutas y campos de datos parcialmente llenos.
2. Para cada fila, localizar el PDF en la ruta especificada.
3. Clasificar si es un PDF digital o escaneado.
4. Extraer texto usando `fitz` (si digital) o `pdf2image + pytesseract` (si escaneado).
5. Buscar los campos faltantes indicados en la fila del Excel usando expresiones regulares robustas.
6. Llenar únicamente los campos faltantes, sin sobrescribir los ya completados.
7. Registrar el método de extracción en una columna adicional `método_extracción`.
8. Si hay errores (PDF no encontrado, ilegible, sin coincidencias), registrar `status = "error"` y detallar en `nota`.
9. Guardar:
   - El Excel con campos actualizados.
   - Logs detallados por archivo procesado.
   - Archivos `.json` con texto completo y datos extraídos por PDF en carpeta `extracted_data/`.

Tu objetivo es reconstruir datos faltantes con precisión y resiliencia, respetando las evidencias y sin forzar valores no encontrados.

## 💬 Estilo de Trabajo
- Enfoque didáctico: explicaciones simples, paso a paso.
- Soporte para estudiantes sin experiencia previa en programación.
- Comentarios extensivos en el código para facilitar futuras modificaciones.

## 📌 Estado Actual
- **Script de OCR** funcionando y documentado.
- Base preparada para seguir refinando expresiones regulares y lógica de extracción.

## ✅ Siguientes Pasos
- Completar datos usando proceso inverso 
- Refinar expresiones regulares según los textos extraídos.
- Incorporar métricas adicionales si los PDFs contienen nuevos campos.
- Integrar validación de calidad de datos.


Este resumen puede servir como bitácora de trabajo para documentar avances, cambios y decisiones en el proyecto.

