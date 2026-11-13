#!/usr/bin/env python3
# One-shot helper: extracts selected methods verbatim from the decompiled Features.cs / Form1.cs
# so the logic is ported 1:1; Windows-only calls are patched afterwards.
import re, sys
import os
SRC = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', 'reference', 'decompiled', 'FnKey', 'FnKey') + '/'
def load(f): return open(SRC + f, encoding='utf-8-sig').read().replace('\r', '')
def extract(text, sig_regex):
    m = re.search(sig_regex, text)
    if not m: raise SystemExit('not found: ' + sig_regex)
    # start at beginning of line
    s = text.rfind('\n', 0, m.start()) + 1
    i = text.index('{', m.end() - 1 if text[m.end()-1] == '{' else m.end())
    d = 0
    j = i
    while True:
        c = text[j]
        if c == '{': d += 1
        elif c == '}':
            d -= 1
            if d == 0: break
        j += 1
    out = text[s:j+1]
    out = re.sub(r'[ \t]*//IL_[^\n]*\n', '', out)
    return out
if __name__ == '__main__':
    which, names = sys.argv[1], sys.argv[2:]
    t = load(which)
    for n in names:
        print(extract(t, r'\n\t(?:public|private|internal)[^\n=;(]*\b' + re.escape(n) + r'\([^\n]*\)\s*\n\t\{') + '\n')
