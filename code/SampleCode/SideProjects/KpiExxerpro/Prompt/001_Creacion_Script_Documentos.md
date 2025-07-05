
# 001 - Plantilla para Solicitar la Creación de un Script Python

## ✨ Instrucciones para Estudiantes

### 🧠 ¿Para qué sirve?
Esta plantilla te ayudará a pedir a ChatGPT que te cree un script desde cero para automatizar la extracción de información desde documentos de negocio (como facturas, IMSS, cotizaciones, etc.).

---

## 📌 ¿Cómo usar esta plantilla?

Debes **copiar y pegar solo las secciones en negritas**, sin copiar estas instrucciones internas. Asegúrate de escribir claramente tus necesidades.

---

## 📋 Plantilla de Comunicación con GPT

### 🧾 **INSTRUCCIÓN PARA GPT:**

> Hola, necesito un script en Python que automatice la extracción de datos desde archivos PDF u otros formatos comunes en negocios (como `.xlsx`, `.docx`, etc.).
>
> Mis archivos están organizados en una estructura de carpetas como esta:
>
> ```
> ./Documentos/
> ├── 2020/
> │   ├── Cliente1/
> │   │   └── Factura_enero.pdf
> ├── 2021/
> │   └── Cliente2/
> │       └── IMSS_agosto.pdf
> ```
>
> ### Requerimientos:
>
> 1. El script debe recorrer todas las subcarpetas desde una ruta base y procesar los archivos del tipo que yo indique (`.pdf` por defecto).
> 2. Si indico un tipo no soportado, debe mostrar un mensaje empático y sugerir bibliotecas para que un experto las agregue.
> 3. Extraer metadatos clave desde el path del archivo (como el año, cliente, tipo de documento).
> 4. Detectar si el documento contiene texto o está escaneado.
> 5. Determinar el tipo de documento: Factura, Cotización, Pago de IMSS, etc.
> 6. Extraer campos clave: Fecha, RFC, Total, Subtotal, Fecha de timbrado (si aplica), entre otros usando expresiones regulares flexibles.
> 7. Manejo de errores: si falta un campo crítico, registrar advertencia pero no detener el proceso.
> 8. Generar una columna de “Notas” para registrar advertencias.
> 9. Consolidar la información extraída en un archivo Excel llamado `documentos_clasificados.xlsx`, con una hoja por cada año detectado.
>
> ### Adicional:
>
> - Todo el código debe estar comentado paso a paso para principiantes.
> - Debe indicar cómo instalar las librerías necesarias: `pdfplumber`, `pandas`, `openpyxl`.
> - Sugerir cómo adaptar el script a otros documentos similares (por ejemplo, órdenes de compra).
>
> Gracias. Estoy aprendiendo, así que necesito que sea claro y fácil de seguir.

---

## 🧠 Tips Rápidos

- ✅ Usa ejemplos si puedes (puedes decir: "como en el PDF adjunto").
- ✅ No olvides pedir que el código esté comentado.
- ✅ Siempre revisa que entiendes el resultado y pide explicaciones si no.

