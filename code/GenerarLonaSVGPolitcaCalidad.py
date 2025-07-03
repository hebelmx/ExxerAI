from textwrap import wrap


def justify_text_line(words, line_width, start_x, y, font, size, fill, weight):
    """
    Genera un <text> con múltiples <tspan> que simulan justificación.
    """
    n = len(words)
    if n == 1:
        # Una sola palabra: centrado o alineado sin justificar
        return f'<text x="{start_x}" y="{y}" font-family="{font}" font-size="{size}" fill="{fill}" font-weight="{weight}">{words[0]}</text>\n'

    total_word_len = sum(len(w) for w in words)
    space_count = n - 1
    space_width = (line_width - (total_word_len * size * 0.52)) / space_count

    x_cursor = start_x
    svg_line = f'<text y="{y}" font-family="{font}" font-size="{size}" fill="{fill}" font-weight="{weight}">\n'

    for word in words:
        svg_line += f'  <tspan x="{x_cursor:.2f}" dy="0">{word}</tspan>\n'
        x_cursor += len(word) * size * 0.52 + space_width

    svg_line += '</text>\n'
    return svg_line

# SVG parameters
output_path = r"f:/Dynamic/ExxerAi/ExxerAI/code/Politica_Calidad_Lona_FontReduced.svg"
viewbox_width = 1500
padding = 10

blocks = [
    {"text": "Política de Calidad", "font": "Calibri", "size": 75, "bold": True, "align": "center"},
    {"gap": 30},
    {"text": (
        "Exxerpro Solutions, empresa dedicada a Industria 5.0, mediante el Diseño y desarrollo de "
        "soluciones, y proyectos a la medida, enfocándonos en la optimización, sustentabilidad y "
        "automatización para la industria de manufactura comprometidos con la calidad de sus "
        "productos y servicios y tiene como parte de su misión ser la mejor opción para sus "
        "clientes por lo que establece su compromiso por medio de la siguiente política basada "
        "en la norma ISO 9001:2015:"
    ), "font": "Helvetica", "size": 38},
    {"gap": 20},
    {"text": (
        "Como director estratégico de Exxerpro Solutions enfoco el sistema de gestión de la "
        "calidad como una herramienta para determinar, satisfacer y exceder las necesidades y "
        "expectativas de nuestros clientes, partes interesadas y los requisitos legales y "
        "reglamentarios aplicables a nuestros procesos, a través de la mejora continua del "
        "sistema de gestión de la calidad y el desarrollo de nuevas soluciones, productos y "
        "servicios; incorporando para tal fin tecnologías novedosas y de vanguardia, apoyándonos "
        "de nuestras actividades de investigación, desarrollo e innovación, convirtiéndonos así "
        "en la mejor opción para nuestros clientes."
    ), "font": "Helvetica", "size": 38},
    {"gap": 20},
    {"text": (
        "En Exxerpro Solutions todos los colaboradores y directivos compartimos la responsabilidad "
        "de aplicar esta política a todas las actividades, programas, y proyectos abriendo una "
        "comunicación bidireccional de manera horizontal y vertical asegurando la calidad de nuestras soluciones."
    ), "font": "Helvetica", "size": 38},
    {"gap": 40},
    {"text": "Director de planeación", "font": "Calibri", "size": 38, "align": "center"},
    {"text": "estratégica", "font": "Calibri", "size": 38, "align": "center"},
    {"gap": 100},  # espacio para firma
    {"text": "Santiago de Querétaro a 11 de Marzo del 2025", "font": "Calibri", "size": 38, "align": "center"},
    {"gap": 20},
    {"text": "Creación: 06-Sep-2022", "font": "Arial", "size": 27, "fill": "#555"},
    {"text": "Revisión: 11-Mar-2025", "font": "Arial", "size": 27, "fill": "#555"},
    {"text": "REV. 01", "font": "Arial", "size": 27, "fill": "#555", "align": "center"},
]

# SVG init
svg = '''<?xml version="1.0" encoding="UTF-8"?>
<svg width="1500mm" height="2000mm" viewBox="0 0 1500 2000"
     xmlns="http://www.w3.org/2000/svg" version="1.1">
  <rect width="100%" height="100%" fill="white"/>
'''

x, y = 100, 350
for block in blocks:
    if "gap" in block:
        y += block["gap"]
        continue

    font = block["font"]
    size = block["size"]
    fill = block.get("fill", "#000")
    weight = "bold" if block.get("bold") else "normal"

    # Auto-wrap based on character width estimate
    char_width = size * 0.52
    max_chars = int((viewbox_width - padding) / char_width)
    lines = wrap(block["text"], max_chars)

    for line in lines:
        text_x = x
        anchor = ""

        if block.get("align") == "center":
            text_x = viewbox_width / 2
            anchor = ' text-anchor="middle"'

        svg += f'<text x="{text_x}" y="{y}" font-family="{font}" font-size="{size}" fill="{fill}" font-weight="{weight}"{anchor}>{line}</text>\n'
        y += size * 1.3  # línea más compacta

# Close SVG
svg += '</svg>'

with open(output_path, "w", encoding="utf-8") as f:
    f.write(svg)

print(f"SVG saved to {output_path}")
