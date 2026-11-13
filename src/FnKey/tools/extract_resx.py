#!/usr/bin/env python3
# Extract embedded images / fonts from the decompiled FnKey resx (BinaryFormatter Bitmap blobs) into PNG files.
import sys, re, base64, io, xml.etree.ElementTree as ET
src, out = sys.argv[1], sys.argv[2]
import os
os.makedirs(out, exist_ok=True)
root = ET.parse(src).getroot()
PNG = b'\x89PNG\r\n\x1a\n'
for d in root.iter('data'):
    name = d.get('name'); mt = d.get('mimetype') or ''; ty = d.get('type') or ''
    v = d.find('value')
    if v is None or not v.text: continue
    try: raw = base64.b64decode(v.text)
    except Exception: continue
    if 'Byte[]' in ty:
        ext = 'ttf' if name.lower().startswith('bank') else 'bin'
        # System.Byte[] in BinaryFormatter: raw array after header? resx type=Byte[] stores raw bytes
        open(f'{out}/{name}.{ext}', 'wb').write(raw); print(name, 'bytes', len(raw)); continue
    if 'binary.base64' not in mt: continue
    i = raw.find(PNG)
    if i >= 0:
        j = raw.find(b'IEND', i)
        if j >= 0:
            open(f'{out}/{name}.png', 'wb').write(raw[i:j+8]); continue
    # BMP / ICO / other
    for sig, ext in ((b'BM','bmp'),(b'\x00\x00\x01\x00','ico')):
        k = raw.find(sig, 40)
        if k >= 0:
            open(f'{out}/{name}.{ext}', 'wb').write(raw[k:]); print(name, ext); break
    else: print('skip', name)
