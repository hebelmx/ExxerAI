# Introducción a Git y GitHub (para principiantes)

**¿Qué es Git?**
Git es un sistema de control de versiones. Te permite guardar cambios en tu código y regresar a versiones anteriores si algo sale mal. Es como un historial del trabajo hecho.

**¿Qué es GitHub?**
GitHub es una plataforma en línea donde puedes subir (almacenar) tu proyecto que usa Git. También puedes colaborar con otros desarrolladores.

---

## ¿Qué es una rama (branch)?
- Una **rama** es una copia del código donde puedes hacer cambios sin afectar el original.
- Por defecto trabajamos en una rama llamada `main`.
- También puedes tener ramas como `dev`, `feature-login`, etc.
- Se usan para probar ideas nuevas o corregir errores sin dañar el código principal.

---

## ¿Cómo subir tus cambios a GitHub?

1. Guardar los cambios para subir:
   ```bash
   git add .
   ```

2. Guardar el cambio en el historial de Git con un mensaje:
   ```bash
   git commit -m "Un mensaje descriptivo"
   ```

3. Subir los cambios a GitHub:
   ```bash
   git push origin main
   ```

4. Revisar el estado del proyecto:
   ```bash
   git status
   ```

5. Ver el historial de cambios:
   ```bash
   git log
   ```


5. Ver los cambios desde una interfaz gráfica:
   ```bash
    gitk --all
   ```
  

---

## ¿Cómo leer cambios desde GitHub?

1. Descargar los cambios sin aplicarlos:
   ```bash
   git fetch origin main
   ```

2. Descargar y aplicar los cambios:
   ```bash
   git pull origin main
   ```

---

## ¿Cómo crear una nueva rama?

```bash
git branch nombre_de_la_nueva_rama        # Crea la rama
git checkout nombre_de_la_nueva_rama      # Te cambia a esa rama
git push origin nombre_de_la_nueva_rama   # La sube a GitHub
```

---

## ¿Cómo combinar cambios de una rama a otra?

1. Cambiar a `main`:
   ```bash
   git checkout main
   ```

2. Combinar los cambios:
   ```bash
   git merge nombre_de_la_nueva_rama
   ```

3. Subir a GitHub:
   ```bash
   git push origin main
   ```

---

## ¿Cómo cambiar de rama?

```bash
git checkout nombre_de_la_rama   # Cambia a la rama deseada
git checkout -b nueva_rama       # Crea y cambia a una nueva rama
```

---

> 💡 Recuerda: usa `#` para comentarios en el código, no afecta la ejecución.
