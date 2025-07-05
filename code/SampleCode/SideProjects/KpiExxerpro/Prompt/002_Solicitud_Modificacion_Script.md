
# 002 - Solicitud de Modificación o Expansión de un Script Python

## 🧠 Propósito
Este documento guía a los residentes para crear un prompt eficaz que solicite a la IA la generación de un script Python adaptado a necesidades reales de digitalización de documentos empresariales.

---

## 🧾 Prompt Enriquecido para ChatGPT

Hola, necesito un script en Python que automatice la extracción de datos desde archivos PDF de una categoría específica de documentos (por ejemplo, facturas, cotizaciones, órdenes de compra, etc.).

---

## 📁 Estructura de Archivos Esperada

Mis archivos están organizados en una estructura de carpetas así:

```
./Documentos/
├── 2020/
│   ├── Cliente1/
│   │   └── Factura_enero.pdf
│   ├── Cliente2/
│   │   └── Cotizacion_abril.pdf
├── 2021/
│   └── Cliente3/
│       └── IMSS_julio.pdf
```

---

## 🧠 Requerimientos del Script

1. **Recorrido de carpetas**:
   - Debe recorrer todas las subcarpetas desde una raíz configurable (por ejemplo, `./`).

2. **Tipo de archivo configurable**:
   - Por defecto solo procesa `.pdf`, pero el usuario debe poder cambiarlo a `.docx`, `.xlsx`, etc.

3. **Validación de tipo de archivo**:
   - Si el usuario indica un tipo no soportado, debe mostrarse un mensaje empático y sugerir bibliotecas que un programador puede considerar.

4. **Extracción de metadatos desde la ruta del archivo**:
   - Año, cliente, tipo de documento, etc.

5. **Análisis del contenido**:
   - Determinar si el archivo tiene texto (estatus: digitalizado o escaneado).
   - Identificar el tipo de documento: "Factura", "Cotización", "Pago de Impuesto", etc.
   - Extraer campos clave como: Fecha, Fecha de timbrado, RFC, Total, Subtotal, etc.

6. **Manejo de excepciones**:
   - Si falta un dato crítico, registrar una advertencia en una columna “Notas” pero continuar el procesamiento.

7. **Salida Excel**:
   - Consolidar los resultados en `documentos_clasificados.xlsx`.
   - Una hoja por año detectado.

---

## 🎓 Consideraciones para Principiantes

- El script debe estar **comentado paso a paso**.
- Debe incluir instrucciones claras para instalar las librerías necesarias:
  ```bash
  pip install pdfplumber pandas openpyxl
  ```
- El diseño debe permitir personalizar fácilmente los campos y tipos de documento.
- La solución debe ser **robusta, reutilizable y clara** para futuros proyectos similares.

---

## 🎯 Objetivo Final

Un script Python que pueda adaptarse rápidamente a diferentes tipos de documentos empresariales, organizado por carpetas, y que genere un reporte tabular limpio y procesable.

Gracias.
