#!/usr/bin/env python3
"""First-pass mechanical conversion of ILSpy C# (WPF) to Avalonia C#. Output then hand-fixed (source of truth)."""
import re, sys
src, dst = sys.argv[1], sys.argv[2]
t = open(src, encoding='utf-8-sig').read()
t = t.replace('\r\n', '\n')
lines = [l for l in t.split('\n') if '//IL_' not in l and '// IL_' not in l]
t = '\n'.join(lines)
# remove generated connector methods
def cut_method(t, sig):
    i = t.find(sig)
    if i < 0: return t
    # find start of attribute lines above
    start = t.rfind('\n\n', 0, i)
    j = t.find('{', i)
    depth = 0; k = j
    while True:
        if t[k] == '{': depth += 1
        elif t[k] == '}':
            depth -= 1
            if depth == 0: break
        k += 1
    return t[:start] + t[k+1:]
events = []
m = re.search(r'void IComponentConnector\.Connect.*?\n\t\}\n', t, re.S)
if m:
    for ev in re.findall(r'\(\(\w+\)(\w+)\)\.(\w+) \+= (\w+);', m.group(0)):
        events.append(ev)
    for ev in re.findall(r'^\t\t\t(\w+)\.(\w+) \+= (\w+);', m.group(0), re.M):
        events.append(ev)
for sig in ['void IComponentConnector.Connect', 'void IStyleConnector.Connect', 'internal Delegate _CreateDelegate']:
    t = cut_method(t, sig)
t = re.sub(r'\n\t\[(DebuggerNonUserCode|GeneratedCode[^\]]*|EditorBrowsable[^\]]*)\]', '', t)
t = re.sub(r'\n\tprivate bool _contentLoaded;', '', t)
t = re.sub(r'\n\tinternal [\w.<>]+ \w+;\n', '\n', t)
t = re.sub(r'\n\tinternal [\w.<>]+ \w+;\n', '\n', t)
t = re.sub(r'\n\tinternal [\w.<>]+ \w+;\n', '\n', t)
t = re.sub(r', IComponentConnector(, IStyleConnector)?', '', t)
# usings
t = re.sub(r'using System\.(Windows[\w.]*|CodeDom\.Compiler|ComponentModel|Diagnostics);\n', '', t)
t = t.replace('using System;', 'using System;\nusing Avalonia;\nusing Avalonia.Controls;\nusing Avalonia.Controls.Primitives;\nusing Avalonia.Controls.Shapes;\nusing Avalonia.Input;\nusing Avalonia.Interactivity;\nusing Avalonia.Media;\nusing Avalonia.Media.Imaging;\nusing Avalonia.Threading;\nusing Avalonia.Layout;\nusing RippleCC.Platform;', 1)
# visibility
def vis(v): return 'true' if v in ('0',) else 'false'
t = re.sub(r'\(\((?:UIElement|FrameworkElement|Control)\)([\w.\[\]]+)\)\.Visibility = \(Visibility\)(\d)', lambda m: '%s.IsVisible = %s' % (m.group(1), vis(m.group(2))), t)
t = re.sub(r'\(\((?:UIElement|FrameworkElement|Control)\)([\w.\[\]]+)\)\.Visibility = Visibility\.(\w+)', lambda m: '%s.IsVisible = %s' % (m.group(1), 'true' if m.group(2)=='Visible' else 'false'), t)
t = re.sub(r'\(\((?:UIElement|FrameworkElement|Control)\)([\w.\[\]]+)\)\.Visibility ([!=]=) \(Visibility\)(\d)', lambda m: '%s.IsVisible %s %s' % (m.group(1), m.group(2), vis(m.group(3))), t)
t = re.sub(r'\(\((?:UIElement|FrameworkElement|Control)\)([\w.\[\]]+)\)\.Visibility ([!=]=) Visibility\.(\w+)', lambda m: '%s.IsVisible %s %s' % (m.group(1), m.group(2), 'true' if m.group(3)=='Visible' else 'false'), t)
t = re.sub(r'\(Visibility\)(\d)', lambda m: 'true' if m.group(1)=='0' else 'false', t)
# casts
t = re.sub(r'\(\((?:UIElement|FrameworkElement|DispatcherObject)\)([\w.\[\]]+)\)', r'\1', t)
t = t.replace('(UIElement)', '(Control)').replace('(FrameworkElement)', '(Control)').replace('(ButtonBase)', '(Button)')
t = t.replace('(Brush)new SolidColorBrush', 'new SolidColorBrush')
t = re.sub(r'\(\((?:Control|Shape|Panel|ToggleButton|ContentControl|Button|Border|Image|TextBlock|Label|Canvas|Grid|Path|Rectangle)\)([\w.\[\]]+)\)', r'\1', t)
t = t.replace('Visibility.Visible', 'true').replace('Visibility.Hidden', 'false').replace('Visibility.Collapsed', 'false')
t = t.replace('Dispatcher.BeginInvoke((Delegate)(Action)(', 'Dispatcher.UIThread.Post((Action)(')
t = t.replace('Array.Empty<object>());', ');')
t = t.replace('MouseEventArgs', 'PointerEventArgs').replace('MouseButtonEventArgs', 'PointerPressedEventArgs')
t = t.replace('RoutedPropertyChangedEventArgs<double>', 'RangeBaseValueChangedEventArgs')
t = t.replace('KeyboardFocusChangedEventArgs', 'GotFocusEventArgs')
t = re.sub(r'\(byte\)(\d+)', r'(byte)\1', t)
open(dst, 'w', encoding='utf-8').write(t)
open(dst + '.events', 'w').write('\n'.join('%s.%s += %s;' % e for e in events))
