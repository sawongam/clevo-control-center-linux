#!/usr/bin/env python3
"""
ILSpy-decompiled WPF XAML -> Avalonia 12 AXAML converter (specific to this app, but fairly general).

usage: xaml2axaml.py <in.xaml> <out.axaml> [--app App.xaml]

* keyed Style / ControlTemplate resources that are actually referenced (transitively) are converted to ControlThemes
* Triggers are converted into nested :pseudoclass / [Prop=Value] styles
* resources that cannot be converted automatically (Slider/ScrollBar/ComboBox templates ...) are taken from
  tools/overrides/<key>.axaml if it exists, otherwise a warning is printed and a placeholder is emitted.
The output is a starting point; the generated .axaml files are then hand-fixed and are the source of truth.
"""
import sys, re, os, json
import xml.etree.ElementTree as ET
from xml.sax.saxutils import quoteattr

HERE = os.path.dirname(os.path.abspath(__file__))
IMAGEMAP = json.load(open(os.path.join(HERE, 'imagemap.json')))
XNS = 'http://schemas.microsoft.com/winfx/2006/xaml'
WARN = []

def warn(msg):
    if msg not in WARN:
        WARN.append(msg)
        print('WARN:', msg, file=sys.stderr)

def lname(tag):
    return tag.split('}')[-1]

SYSCOLORS = {
    'ControlTextBrushKey': '#FF000000', 'ControlBrushKey': '#FFF0F0F0', 'HighlightBrushKey': '#FF0078D7',
    'GrayTextBrushKey': '#FF6D6D6D', 'WindowBrushKey': '#FFFFFFFF', 'WindowTextBrushKey': '#FF000000',
    'ControlDarkBrushKey': '#FFA0A0A0', 'ControlLightBrushKey': '#FFE3E3E3', 'HighlightTextBrushKey': '#FFFFFFFF',
}

def res_image(path):
    p = path.strip()
    p = re.sub(r'^pack://application:,,,/', '', p)
    p = re.sub(r'^/?(?:[A-Za-z0-9_.]+;component/)?', '', p)
    key = p.lower()
    if key in IMAGEMAP:
        return '/' + IMAGEMAP[key]
    warn('image not found: %s' % path)
    return '/' + p

KEEP_OWNERS = {'Grid', 'Canvas', 'DockPanel', 'ScrollViewer', 'ToolTip', 'AutomationProperties'}
DROP_PROPS = {'SnapsToDevicePixels', 'FocusVisualStyle', 'OverridesDefaultStyle', 'Command', 'ClearTypeHint',
              'DirectionalNavigation', 'IsPressAndHoldEnabled', 'BitmapScalingMode', 'EdgeMode', 'Uid', 'Language',
              'ContentTemplateSelector', 'ContentStringFormat', 'RecognizesAccessKey', 'IsDefaulted', 'ContentSource',
              'Mode', 'TabNavigation', 'ControlTabNavigation', 'AllowsTransparency', 'WindowStyle', 'ResizeMode',
              'ShowInTaskbar', 'MappingMode', 'HasContent', 'IsMoveToPointEnabled', 'CanContentScroll', 'PanningMode', 'IsFlicksEnabled'}

def prop_name(raw):
    if '.' in raw:
        owner, p = raw.split('.', 1)
        if p in ('CanContentScroll', 'PanningMode', 'IsFlicksEnabled'):
            return None
        if owner in KEEP_OWNERS:
            return raw
        if owner == 'Panel' and p == 'ZIndex':
            return 'ZIndex'
        if owner in ('Storyboard', 'Timeline', 'DoubleKeyFrame', 'KeyboardNavigation', 'RenderOptions', 'Stylus', 'VisualStateManager',
                     'Trigger', 'Condition', 'Setter', 'ControlTemplate', 'Style', 'EventTrigger'):
            return None
        return p
    return raw

def conv_point(v):
    out = []
    for part in v.split(','):
        part = part.strip()
        try:
            out.append(('%g' % (float(part) * 100)) + '%')
        except ValueError:
            out.append(part)
    return ','.join(out)

def conv_markup(v):
    v = re.sub(r'\{(?:Dynamic|Static)Resource \{(?:x:)?Static SystemColors\.(\w+)\}\}', lambda m: SYSCOLORS.get(m.group(1), '#FF000000'), v)
    v = re.sub(r'\{TemplateBinding ([A-Za-z0-9_.]+)\}', lambda m: '{TemplateBinding %s}' % m.group(1).split('.')[-1], v)
    return v

def conv_attr(owner, raw, value):
    p = prop_name(raw)
    if p is None:
        return None
    if '.' not in p and p in DROP_PROPS:
        return None
    if p == 'Visibility':
        if value.startswith('{'):
            return ('IsVisible', value.replace('.Visibility}', '.IsVisible}'))
        return ('IsVisible', 'True' if value == 'Visible' else 'False')
    if p == 'ToolTip':
        return ('ToolTip.Tip', value)
    if p in ('Style', 'Template') and value.startswith('{'):
        return ('Theme', conv_markup(value))
    if p == 'ImageSource':
        return ('Source', res_image(value) if not value.startswith('{') else value)
    if p == 'Source' and not value.startswith('{') and (owner == 'Image' or owner == 'Setter'):
        return ('Source', res_image(value))
    if p in ('RenderTransformOrigin', 'StartPoint', 'EndPoint', 'Center', 'GradientOrigin'):
        return (p, conv_point(value))
    if p in ('RadiusX', 'RadiusY') and owner == 'RadialGradientBrush':
        return (p, '%g%%' % (float(value) * 100))
    if p in ('Width', 'Height', 'MinWidth', 'MinHeight', 'MaxWidth', 'MaxHeight') and raw.startswith('FrameworkElement.') and value == 'Auto':
        return None
    if p == 'StrokeDashArray':
        return (p, re.sub(r'[\s,]+', ',', value.strip()))
    if p == 'IsChecked' and value.lower() == 'true':
        return ('IsChecked', 'True')
    return (p, conv_markup(value))

class Ctx:
    def __init__(self, resources, overrides_dir):
        self.resources = resources
        self.overrides_dir = overrides_dir
        self.emitted = set()

def esc(v):
    return quoteattr(v)

def text_of_ctor(el):
    for c in el:
        if lname(c.tag) == 'Ctor':
            return (c.text or '').strip()
    return None

def extension_value(el):
    n = lname(el.tag)
    if n == 'NullExtension':
        return '{x:Null}'
    if n in ('StaticResourceExtension', 'DynamicResourceExtension'):
        key = text_of_ctor(el)
        return '{%s %s}' % ('DynamicResource' if n.startswith('Dynamic') else 'StaticResource', key)
    if n == 'Binding':
        en = el.get('ElementName')
        path = text_of_ctor(el)
        rs = None
        for c in el:
            if lname(c.tag) == 'Binding.RelativeSource':
                rs = c[0]
        if en:
            return '{Binding #%s.%s}' % (en, path) if path else '{Binding #%s}' % en
        if rs is not None:
            mode = rs.get('Mode')
            at = None
            for c in rs:
                if lname(c.tag) == 'RelativeSource.AncestorType':
                    at = text_of_ctor(c[0]) if len(c) else None
            if mode == 'TemplatedParent':
                return '{TemplateBinding %s}' % (path or '')
            if mode == 'FindAncestor' and at:
                return '{Binding %s, RelativeSource={RelativeSource AncestorType=%s}}' % (path, at)
            return '{Binding %s, RelativeSource={RelativeSource %s}}' % (path, mode)
        return '{Binding %s}' % (path or '')
    if n == 'SolidColorBrush' and len(el) == 0 and el.get('Color') and len(el.attrib) == 1:
        return el.get('Color')
    return None

class Writer:
    def __init__(self):
        self.lines = []
    def w(self, ind, s):
        self.lines.append('  ' * ind + s)

def is_prop_elem(el):
    return '.' in lname(el.tag)

def attrs_of(el, owner):
    out = []
    for k, v in el.attrib.items():
        if k.startswith('{'):
            if k[1:].split('}')[0] == XNS:
                out.append(('x:' + lname(k), v))
            continue
        r = conv_attr(owner, k, v)
        if r:
            out.append(r)
    return out

# ---- identity transform cleanup --------------------------------------------------------------
def fnum(v, d):
    try:
        return float(v)
    except Exception:
        return d

def transform_identity(t):
    n = lname(t.tag)
    if n == 'ScaleTransform':
        return fnum(t.get('ScaleX'), 1) == 1 and fnum(t.get('ScaleY'), 1) == 1
    if n == 'SkewTransform':
        return fnum(t.get('AngleX'), 0) == 0 and fnum(t.get('AngleY'), 0) == 0
    if n == 'RotateTransform':
        return fnum(t.get('Angle'), 0) == 0
    if n == 'TranslateTransform':
        return fnum(t.get('X'), 0) == 0 and fnum(t.get('Y'), 0) == 0
    return False

def sort_gradient_stops(root):
    for e in root.iter():
        if lname(e.tag) in ('LinearGradientBrush', 'RadialGradientBrush'):
            stops = [c for c in e if lname(c.tag) == 'GradientStop']
            others = [c for c in e if lname(c.tag) != 'GradientStop']
            stops.sort(key=lambda c: fnum(c.get('Offset'), 0.0))   # WPF sorts stops by offset; Avalonia does not
            for c in list(e):
                e.remove(c)
            for c in others + stops:
                e.append(c)

def simplify_transforms(root):
    sort_gradient_stops(root)
    for parent in list(root.iter()):
        for ch in list(parent):
            if lname(ch.tag) == 'TransformGroup':
                for t in list(ch):
                    if transform_identity(t):
                        ch.remove(t)
        for ch in list(parent):
            if '.' in lname(ch.tag) and lname(ch.tag).endswith('.RenderTransform') and (len(ch) == 0 or (lname(ch[0].tag) == 'TransformGroup' and len(ch[0]) == 0)):
                parent.remove(ch)
        # Setter.Value with empty TransformGroup
        if lname(parent.tag) == 'Setter' and parent.get('Property', '').endswith('RenderTransform'):
            for ch in list(parent):
                if lname(ch.tag) == 'Setter.Value' and len(ch) and lname(ch[0].tag) == 'TransformGroup' and len(ch[0]) == 0:
                    # becomes a Setter resetting the transform
                    parent.remove(ch)
                    parent.set('Value', 'none')

def conv_element(el, ind, w, ctx, in_template=False):
    tag = lname(el.tag)
    if el.tag.startswith('{clr-namespace:CC}'):
        tag = 'local:' + tag
    ext = extension_value(el)
    if ext is not None:
        w.w(ind, '<!-- inline ext %s -->' % ext)
        return
    attrs = attrs_of(el, tag)
    prop_children = [c for c in el if is_prop_elem(c)]
    content_children = [c for c in el if not is_prop_elem(c)]
    extra_attrs = []
    complex_props = []
    for pc in prop_children:
        pname = lname(pc.tag)
        owner, p = pname.split('.', 1)
        if owner in ('Storyboard', 'Timeline') or pname == 'VisualStateManager.VisualStateGroups':
            continue
        if p == 'Triggers':
            continue
        if p == 'Resources':
            complex_props.append(pc); continue
        if p in ('ColumnDefinitions', 'RowDefinitions', 'Children', 'Items', 'ItemsPanel', 'Data', 'GradientStops', 'Content', 'Child'):
            complex_props.append(pc); continue
        if len(pc) == 1:
            val = extension_value(pc[0])
            if val is not None:
                r = conv_attr(tag, pname, val)
                if r:
                    extra_attrs.append(r)
                continue
        complex_props.append(pc)
    if tag == 'Label' and not any(a[0] == 'Padding' for a in attrs + extra_attrs):
        extra_attrs.append(('Padding', '5'))
    if tag == 'ContentPresenter' and in_template:
        names = [a[0] for a in attrs + extra_attrs]
        if 'Content' not in names:
            extra_attrs.append(('Content', '{TemplateBinding Content}'))
            extra_attrs.append(('ContentTemplate', '{TemplateBinding ContentTemplate}'))
    has_rt = any(a[0] == 'RenderTransform' for a in attrs + extra_attrs) or any(lname(c.tag).endswith('.RenderTransform') for c in prop_children)
    if has_rt and not any(a[0] == 'RenderTransformOrigin' for a in attrs + extra_attrs):
        extra_attrs.append(('RenderTransformOrigin', '0,0'))
    seen = {}
    for k, v in attrs + extra_attrs:
        seen[k] = v
    allattrs = list(seen.items())
    kids = [('prop', pc) for pc in complex_props] + [('content', c) for c in content_children]
    if not kids:
        w.w(ind, '<%s%s />' % (tag, ''.join(' %s=%s' % (k, esc(v)) for k, v in allattrs)))
        return
    if len(allattrs) > 3:
        w.w(ind, '<%s' % tag)
        for k, v in allattrs:
            w.w(ind + 2, '%s=%s' % (k, esc(v)))
        w.w(ind + 1, '>')
    else:
        w.w(ind, '<%s%s>' % (tag, ''.join(' %s=%s' % (k, esc(v)) for k, v in allattrs)))
    for kind, c in kids:
        if kind == 'prop':
            conv_prop_element(c, tag, ind + 1, w, ctx, in_template)
        else:
            conv_element(c, ind + 1, w, ctx, in_template)
    w.w(ind, '</%s>' % tag)

def conv_prop_element(pc, owner_tag, ind, w, ctx, in_template):
    pname = lname(pc.tag)
    owner, p = pname.split('.', 1)
    if p == 'Resources':
        return
    outname = '%s.%s' % (owner_tag, p)
    if p in ('ColumnDefinitions', 'RowDefinitions'):
        outname = 'Grid.' + p
    if prop_name(pname) is None:
        return
    w.w(ind, '<%s>' % outname)
    for c in pc:
        conv_element(c, ind + 1, w, ctx, in_template)
    w.w(ind, '</%s>' % outname)

MANUAL_TAGS = {'Track', 'Popup', 'ScrollContentPresenter', 'ItemsPresenter', 'TickBar', 'ScrollBar', 'Thumb', 'RepeatButton'}

def subtree_tags(el):
    return {lname(e.tag) for e in el.iter()}

def used_keys(el):
    s = ET.tostring(el, encoding='unicode')
    keys = set(re.findall(r'\{(?:Dynamic|Static)Resource ([A-Za-z0-9_.]+)\}', s))
    keys = {k for k in keys if 'FocusVisual' not in k}
    for e in el.iter():
        if lname(e.tag) in ('StaticResourceExtension', 'DynamicResourceExtension'):
            k = text_of_ctor(e)
            if k and 'FocusVisual' not in k:
                keys.add(k)
    return keys

def key_of(el):
    return el.get('{%s}Key' % XNS)

def type_of_style(el):
    tt = el.get('TargetType')
    if tt:
        return tt
    for c in el:
        if lname(c.tag) in ('Style.TargetType', 'ControlTemplate.TargetType'):
            return text_of_ctor(c[0])
    return None

def trig_selector(prop, value):
    p = prop.split('.')[-1]
    v = (value or '').lower()
    if p == 'IsMouseOver': return ':pointerover' if v == 'true' else ':not(:pointerover)'
    if p == 'IsPressed' and v == 'true': return ':pressed'
    if p == 'IsEnabled': return ':disabled' if v == 'false' else ':not(:disabled)'
    if p == 'IsChecked':
        if v == 'true': return ':checked'
        if v == 'false': return ':not(:checked)'
        if v == 'null': return ':indeterminate'
    if p in ('IsKeyboardFocused', 'IsFocused') and v == 'true': return ':focus'
    if p == 'IsSelected' and v == 'true': return ':selected'
    if p == 'IsDropDownOpen' and v == 'true': return ':dropdownopen'
    if p in ('IsDefaulted', 'HasContent', 'IsHighlighted', 'IsKeyboardFocusWithin', 'IsPressed', 'IsFocused'):
        return None
    if p in ('Content', 'Tag', 'Orientation', 'Text'):
        return '[%s=%s]' % (p, value)
    warn('unsupported trigger property %s=%s' % (prop, value))
    return None

def trigger_value(tr):
    v = tr.get('Value')
    if v is None:
        for c in tr:
            if lname(c.tag) == 'Trigger.Value':
                v = 'null' if lname(c[0].tag) == 'NullExtension' else (c[0].text or '')
    return v

def names_types(root):
    d = {}
    for e in root.iter():
        n = e.get('Name') or e.get('{%s}Name' % XNS)
        if n:
            d[n] = lname(e.tag)
    return d

def setter_to_xml(st, ind, w, ctx, nt):
    prop = st.get('Property')
    pn = prop_name(prop)
    if pn is None or ('.' not in pn and pn in DROP_PROPS):
        return
    val = st.get('Value')
    child = None
    for c in st:
        if lname(c.tag) == 'Setter.Value':
            child = c[0]
    if val is not None:
        r = conv_attr('Setter', prop, val)
        if not r:
            return
        n, v = r
        if v.startswith('{TemplateBinding'):
            return
        if v == 'none':
            w.w(ind, '<Setter Property=%s Value="{x:Null}" />' % esc(n))
        else:
            w.w(ind, '<Setter Property=%s Value=%s />' % (esc(n), esc(v)))
        return
    if child is not None:
        ev = extension_value(child)
        r = conv_attr('Setter', prop, ev if ev is not None else '')
        if not r:
            return
        if ev is not None:
            w.w(ind, '<Setter Property=%s Value=%s />' % (esc(r[0]), esc(r[1])))
            return
        w.w(ind, '<Setter Property=%s>' % esc(r[0]))
        conv_element(child, ind + 1, w, ctx, True)
        w.w(ind, '</Setter>')

def conv_triggers(trs, ind, w, ctx, nt):
    for tr in trs:
        n = lname(tr.tag)
        conds = []
        if n == 'Trigger':
            conds = [(tr.get('Property'), trigger_value(tr))]
        elif n == 'MultiTrigger':
            for cs in tr:
                if lname(cs.tag) == 'MultiTrigger.Conditions':
                    for c in cs:
                        conds.append((c.get('Property'), c.get('Value')))
        elif n in ('DataTrigger', 'MultiDataTrigger'):
            warn('%s skipped (handle manually)' % n)
            continue
        elif n == 'EventTrigger':
            warn('EventTrigger skipped (handle manually)')
            continue
        else:
            continue
        pieces = []
        ok = True
        for p, v in conds:
            s = trig_selector(p, v) if p else None
            if s is None:
                ok = False; break
            pieces.append(s)
        setters = [c for c in tr if lname(c.tag) == 'Setter']
        if not setters or not ok:
            continue
        by_target = {}
        order = []
        for st in setters:
            t = st.get('TargetName')
            if t not in by_target:
                by_target[t] = []
                order.append(t)
            by_target[t].append(st)
        for t in order:
            sel = '^' + ''.join(pieces)
            if t:
                sel += ' /template/ %s#%s' % (nt.get(t, 'Control'), t)
            w.w(ind, '<Style Selector=%s>' % esc(sel))
            for st in by_target[t]:
                setter_to_xml(st, ind + 1, w, ctx, nt)
            w.w(ind, '</Style>')

def conv_template(tpl, ind, w, ctx):
    root = None
    trig = []
    for c in tpl:
        n = lname(c.tag)
        if n == 'ControlTemplate.TargetType' or n == 'FrameworkTemplate.Resources':
            continue
        if n == 'ControlTemplate.Triggers':
            trig = list(c)
            continue
        root = c
    w.w(ind, '<ControlTemplate>')
    conv_element(root, ind + 1, w, ctx, True)
    w.w(ind, '</ControlTemplate>')
    return names_types(tpl), trig

def conv_style_as_theme(el, key, ind, w, ctx):
    target = type_of_style(el) or 'Control'
    w.w(ind, '<ControlTheme x:Key=%s TargetType=%s>' % (esc(key), esc(target)))
    nt = {}
    trig_t = []
    style_trig = []
    for c in el:
        n = lname(c.tag)
        if n == 'Setter':
            prop = c.get('Property')
            if prop == 'Control.Template':
                tpl = None
                for cc in c:
                    if lname(cc.tag) == 'Setter.Value':
                        tpl = cc[0]
                if tpl is not None and lname(tpl.tag) == 'ControlTemplate':
                    w.w(ind + 1, '<Setter Property="Template">')
                    nt, trig_t = conv_template(tpl, ind + 2, w, ctx)
                    w.w(ind + 1, '</Setter>')
                else:
                    warn('Template setter by reference in style %s' % key)
                continue
            setter_to_xml(c, ind + 1, w, ctx, nt)
        elif n == 'Style.Triggers':
            style_trig = list(c)
    conv_triggers(trig_t, ind + 1, w, ctx, nt)
    conv_triggers(style_trig, ind + 1, w, ctx, nt)
    w.w(ind, '</ControlTheme>')

def conv_template_as_theme(el, key, ind, w, ctx):
    target = type_of_style(el) or 'Control'
    w.w(ind, '<ControlTheme x:Key=%s TargetType=%s>' % (esc(key), esc(target)))
    w.w(ind + 1, '<Setter Property="Template">')
    nt, trig = conv_template(el, ind + 2, w, ctx)
    w.w(ind + 1, '</Setter>')
    conv_triggers(trig, ind + 1, w, ctx, nt)
    w.w(ind, '</ControlTheme>')

def emit_override(key, ind, w, ctx):
    p = os.path.join(ctx.overrides_dir, key + '.xamlfrag')
    if os.path.exists(p):
        for line in open(p, encoding='utf-8').read().rstrip().split('\n'):
            w.w(ind, line)
        return True
    return False

def conv_resource_item(el, key, ind, w, ctx):
    n = lname(el.tag)
    if emit_override(key, ind, w, ctx):
        return
    if n in ('Style', 'ControlTemplate'):
        tags = subtree_tags(el)
        if tags & MANUAL_TAGS:
            warn('MANUAL needed: %s %s (contains %s)' % (n, key, sorted(tags & MANUAL_TAGS)))
            w.w(ind, '<!-- MANUAL: %s -->' % key)
            return
        (conv_style_as_theme if n == 'Style' else conv_template_as_theme)(el, key, ind, w, ctx)
    elif n == 'Storyboard':
        w.w(ind, '<!-- Storyboard %s: handle in code -->' % key)
    else:
        conv_element(el, ind, w, ctx)

def collect_resources(resprop):
    d = {}
    order = []
    for rd in resprop:
        for item in (rd if lname(rd.tag) == 'ResourceDictionary' else [rd]):
            k = key_of(item)
            if k:
                d[k] = item
                order.append(k)
    return d, order

def find_resprop(root):
    for c in root:
        if lname(c.tag).endswith('.Resources'):
            return c
    return None

def main():
    args = sys.argv[1:]
    inp, outp = args[0], args[1]
    app = args[args.index('--app') + 1] if '--app' in args else None
    ovd = os.path.join(HERE, 'overrides')
    root = ET.parse(inp).getroot()
    simplify_transforms(root)
    rname = lname(root.tag)
    local_res, local_order = ({}, [])
    rp = find_resprop(root)
    if rp is not None:
        local_res, local_order = collect_resources(rp)
    app_res, app_order = ({}, [])
    if app:
        ar = ET.parse(app).getroot()
        simplify_transforms(ar)
        arp = find_resprop(ar)
        if arp is not None:
            app_res, app_order = collect_resources(arp)
    allres = dict(app_res); allres.update(local_res)
    order = [k for k in app_order if k not in local_res] + local_order
    ctx = Ctx(allres, ovd)
    body_probe = ET.Element('probe')
    for c in root:
        if not lname(c.tag).endswith('.Resources') and not lname(c.tag).endswith('.Triggers'):
            body_probe.append(c)
    needed = set()
    extra_keys = []
    todo = list(used_keys(body_probe))
    while todo:
        k = todo.pop()
        if k in needed:
            continue
        if k not in allres:
            if os.path.exists(os.path.join(ovd, k + '.xamlfrag')):
                needed.add(k); extra_keys.append(k)
                todo.extend(re.findall(r'\{(?:Dynamic|Static)Resource ([A-Za-z0-9_.]+)\}', open(os.path.join(ovd, k + '.xamlfrag')).read()))
            continue
        needed.add(k)
        todo.extend(used_keys(allres[k]))
        p = os.path.join(ovd, k + '.xamlfrag')
        if os.path.exists(p):
            todo.extend(re.findall(r'\{(?:Dynamic|Static)Resource ([A-Za-z0-9_.]+)\}', open(p).read()))
    w = Writer()
    cls = root.get('{%s}Class' % XNS)
    ra = []
    for k, v in root.attrib.items():
        if k.startswith('{'):
            continue
        pn = prop_name(k)
        if pn in ('Name', 'Title', 'Height', 'Width', 'Topmost', 'WindowStartupLocation', 'Icon', 'BorderThickness', 'Background', 'BorderBrush', 'UseLayoutRounding'):
            ra.append((pn, res_image(v) if pn == 'Icon' else v))
    for c in root:
        if is_prop_elem(c) and not lname(c.tag).endswith('.Resources') and not lname(c.tag).endswith('.Triggers') and len(c) == 1:
            ev = extension_value(c[0])
            if ev:
                r = conv_attr(rname, lname(c.tag), ev)
                if r:
                    ra.append(r)
    if rname == 'Window':
        ra.append(('WindowDecorations', 'None'))
    w.lines.append('<?xml version="1.0" encoding="utf-8"?>')
    w.w(0, '<%s xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:CC" x:Class=%s %s FontSize="12">' % (rname, esc(cls), ' '.join('%s=%s' % (k, esc(v)) for k, v in ra)))
    if needed:
        w.w(1, '<%s.Resources>' % rname)
        w.w(2, '<ResourceDictionary>')
        for k in extra_keys:
            ctx.emitted.add(k)
            emit_override(k, 3, w, ctx)
        for k in order:
            if k in needed and k not in ctx.emitted:
                ctx.emitted.add(k)
                conv_resource_item(allres[k], k, 3, w, ctx)
        w.w(2, '</ResourceDictionary>')
        w.w(1, '</%s.Resources>' % rname)
    for c in root:
        if is_prop_elem(c):
            continue
        conv_element(c, 1, w, ctx, False)
    w.w(0, '</%s>' % rname)
    open(outp, 'w', encoding='utf-8').write('\n'.join(w.lines) + '\n')
    print('wrote', outp, len(w.lines), 'lines;', len(WARN), 'warnings')

main()
