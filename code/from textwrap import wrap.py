from textwrap import wrap

# Define the output SVG file path
output_path = r"f:/Dynamic/ExxerAi/ExxerAI/code/Politica_Calidad_Lona_FontReduced.svg"

# Define paragraph blocks with metadata
blocks = [
    {"text": "Política de Calidad", "font": "Calibri", "size": 75, "bold": True},
    {"gap": 60},
    {"text": (
        "Exxerpro Solutions, empresa dedicada a Industria 5.0, mediante el Diseño y desarrollo de "
        "soluciones, y proyectos a la medida, enfocándonos en la optimización, sustentabilidad y "
        "automatización para la industria de manufactura comprometidos con la calidad de sus "
        "productos y servicios y tiene como parte de su misión ser la mejor opción para sus "
        "clientes por lo que establece su compromiso por medio de la siguiente política basada "
        "en la norma ISO 9001:2015:"
    ), "font": "Helvetica", "size": 38},
    {"gap": 50},
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
    {"gap": 50},
    {"text": (
        "En Exxerpro Solutions todos los colaboradores y directivos compartimos la responsabilidad "
        "de aplicar esta política a todas las actividades, programas, y proyectos abriendo una "
        "comunicación bidireccional de manera horizontal y vertical asegurando la calidad de nuestras soluciones."
    ), "font": "Helvetica", "size": 38},
    {"gap": 80},
    {"text": "Director de planeación estratégica", "font": "Calibri", "size": 38},
    {"gap": 50},
    {"text": "Santiago de Querétaro a 11 de Marzo del 2025", "font": "Calibri", "size": 38},
    {"gap": 40},
    {"text": "Creación: 06-Sep-2022", "font": "Arial", "size": 27, "fill": "#555"},
    {"text": "Revisión: 11-Mar-2025   REV. 01", "font": "Arial", "size": 27, "fill": "#555"}
]

# Begin SVG
svg = '''<?xml version="1.0" encoding="UTF-8"?>
<svg width="1500mm" height="2000mm" viewBox="0 0 1500 2000"
     xmlns="http://www.w3.org/2000/svg" version="1.1">
  <rect width="100%" height="100%" fill="white"/>
'''

# Text layout logic
x, y = 100, 150
for block in blocks:
    if "gap" in block:
        y += block["gap"]
        continue
    for line in wrap(block["text"], 95):
        svg += f'<text x="{x}" y="{y}" font-family="{block["font"]}" font-size="{block["size"]}" '
        svg += f'fill="{block.get("fill", "#000")}" font-weight="{"bold" if block.get("bold") else "normal"}">{line}</text>\n'
        y += block["size"] * 1.5

# End SVG
svg += '</svg>'

# Write to file
with open(output_path, "w", encoding="utf-8") as f:
    f.write(svg)

print(f"SVG saved to {output_path}")
