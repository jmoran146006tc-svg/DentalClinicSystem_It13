"""One-time conversion of the original embedded artwork; never runs in the app."""
import base64
import io
from pathlib import Path
import xml.etree.ElementTree as ET
from PIL import Image

root = Path(__file__).resolve().parents[1]
resx = ET.parse(root / "DentalClinicSystem/Forms/frmLogin.resx")
entry = resx.find(".//data[@name='pictureBox1.Image']/value")
image = Image.open(io.BytesIO(base64.b64decode(entry.text))).convert("RGB")
# The mark is turquoise and the lettering black. Chroma isolates the mark,
# including its sparkles, without including the lettering or white cut-outs.
mask = Image.new("L", image.size)
pixels = mask.load()
for y in range(image.height):
    for x in range(image.width):
        r, g, b = image.getpixel((x, y))
        if g > r and b > r:
            # Original flat ink is RGB(62,196,201); recover alpha from the
            # difference of channels, which cancels the white matte.
            pixels[x, y] = min(255, round((max(g, b) - r) * 255 / (201 - 62)))
bounds = mask.getbbox()
mark = Image.new("RGBA", image.size, (255, 255, 255, 0))
mark.putalpha(mask)
target = root / "DentalClinicSystem/Resources/LoginMark.png"
target.parent.mkdir(exist_ok=True)
mark.crop(bounds).save(target)
print(f"Created {target.name}: {mark.crop(bounds).size}")
