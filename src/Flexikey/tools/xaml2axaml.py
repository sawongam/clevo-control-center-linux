#!/usr/bin/env python3
"""Mechanical WPF(ilspy BAML dump) -> Avalonia converter used for the Flexikey port.
Usage: xaml2axaml.py in.xaml out.axaml [--events file.cs]
Only resources (styles / templates / brushes) that are really referenced are emitted.
"""
import sys, re, collections
import os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from overrides import OVR, LISTBOXITEM_STYLES
import xml.etree.ElementTree as ET
from xml.sax.saxutils import quoteattr

WPF = 'http://schemas.microsoft.com/winfx/2006/xaml/presentation'
XNS = 'http://schemas.microsoft.com/winfx/2006/xaml'
ILS = 'https://github.com/icsharpcode/ILSpy'

def split(tag):
    if tag.startswith('{'):
        ns, n = tag[1:].split('}')
        return ns, n
    return '', tag

# ---------------------------------------------------------------- value helpers
SYS_COLORS = {
    'ControlTextBrushKey': '#FF000000', 'ControlBrushKey': '#FFF0F0F0', 'HighlightBrushKey': '#FF0078D7',
    'GrayTextBrushKey': '#FF6D6D6D', 'WindowBrushKey': '#FFFFFFFF', 'WindowTextBrushKey': '#FF000000',
    'HighlightTextBrushKey': '#FFFFFFFF', 'ControlDarkBrushKey': '#FFA0A0A0', 'ControlLightBrushKey': '#FFE3E3E3',
    'ControlDarkDarkBrushKey': '#FF696969', 'ControlLightLightBrushKey': '#FFFFFFFF',
    'MenuBrushKey': '#FFF0F0F0', 'ScrollBarBrushKey': '#FFC8C8C8',
    'ActiveBorderBrushKey': '#FFB4B4B4', 'InactiveBorderBrushKey': '#FFF4F7FC',
    'ControlColorKey': '#FFF0F0F0', 'ControlTextColorKey': '#FF000000', 'WindowColorKey': '#FFFFFFFF',
}

def rel(v):
    """WPF relative point 'a,b' -> Avalonia 'a*100%,b*100%'"""
    parts = [p.strip() for p in v.split(',')]
    out = []
    for p in parts:
        if p.endswith('%'):
            out.append(p)
        else:
            f = float(p) * 100
            out.append(('%g' % round(f, 4)) + '%')
    return ','.join(out)

def relsingle(v):
    if v.endswith('%'): return v
    return ('%g' % round(float(v) * 100, 4)) + '%'

def fix_image(v):
    v = v.strip()
    v = re.sub(r'^/?[A-Za-z_0-9]+;component/', '', v)
    v = v.lstrip('/')
    v = v.replace('\\', '/')
    return '/' + v.lower()

DROP_PROPS = {
    'SnapsToDevicePixels', 'OverridesDefaultStyle', 'FocusVisualStyle', 'IsPressAndHoldEnabled', 'IsFlicksEnabled',
    'AllowsTransparency', 'WindowStyle', 'ResizeMode', 'ClipToBounds_', 'RecognizesAccessKey', 'IsTabStop_',
    'IsManipulationEnabled', 'Stylus', 'IsHitTestVisible_', 'CanContentScroll', 'IsDeferredScrollingEnabled',
    'PanningMode', 'Command', 'CommandParameter', 'IsDefault_', 'AllowDrop', 'Topmost_', 'ShowInTaskbar_',
    'IsSynchronizedWithCurrentItem', 'TickPlacement', 'Placement', 'IsSnapToTickEnabled', 'AutoToolTipPlacement',
    'SelectionStart', 'SelectionEnd', 'HorizontalScrollBarVisibility_', 'ScrollUnit', 'IsVirtualizing', 'VirtualizationMode',
    'IsTextSearchEnabled', 'UseLayoutRounding_', 'ClickMode', 'Icon', 'WindowStartupLocation', 'Delay', 'Interval',
    'TickFrequency', 'Uid', 'IsItemsHost', 'FlowDirection', 'MappingMode', 'PlacementTarget', 'StaysOpen', 'PopupAnimation', 'AllowsTransparency', 'IsLocked', 'TextEditor',
}
PROP_RENAME = {'Visibility': 'IsVisible', 'ImageSource': 'Source', 'Panel.ZIndex': 'ZIndex', 'ZIndex': 'ZIndex',
               'ToolTip': 'ToolTip.Tip', 'IsPressed': 'IsPressed'}
ATTACHED_OWNERS = {'Grid', 'Canvas', 'DockPanel', 'ScrollViewer', 'ToolTip', 'TextOptions', 'RenderOptions', 'ToolTipService',
                   'Validation', 'Path_'}
SIZE_PROPS = {'Width', 'Height', 'MinWidth', 'MinHeight', 'MaxWidth', 'MaxHeight'}
EVENT_MAP = {'Click': 'Click', 'Checked': 'IsCheckedChanged', 'Unchecked': 'IsCheckedChanged', 'MouseLeftButtonDown': 'PointerPressed',
             'MouseLeftButtonUp': 'PointerReleased', 'MouseDown': 'PointerPressed', 'MouseUp': 'PointerReleased',
             'MouseEnter': 'PointerEntered', 'MouseLeave': 'PointerExited', 'MouseMove': 'PointerMoved',
             'MouseWheel': 'PointerWheelChanged', 'ValueChanged': 'ValueChanged', 'SelectionChanged': 'SelectionChanged',
             'KeyUp': 'KeyUp', 'KeyDown': 'KeyDown', 'Loaded': 'Loaded', 'Closed': 'Closed', 'TextChanged': 'TextChanged',
             'GotFocus': 'GotFocus', 'LostFocus': 'LostFocus',
             'MouseDoubleClick': 'DoubleTapped'}

def conv_markup(v, prop=None):
    """Convert a markup-extension string."""
    s = v.strip()
    if not s.startswith('{'):
        return v
    m = re.match(r'^\{DynamicResource \{Static SystemColors\.(\w+)\}\}$', s)
    if m:
        return SYS_COLORS.get(m.group(1), '#FF000000')
    if re.match(r'^\{DynamicResource \{Static SystemParameters\.\w+\}\}$', s): return '17'
    m = re.match(r'^\{(?:Static|x:Static) SystemColors\.(\w+)\}$', s)
    if m:
        return SYS_COLORS.get(m.group(1), '#FF000000')
    m = re.match(r'^\{TemplateBinding ([\w.]+)\}$', s)
    if m:
        p = m.group(1).split('.')[-1]
        if p == 'Visibility': p = 'IsVisible'
        if p == 'IsMouseOver': p = 'IsPointerOver'
        return '{TemplateBinding %s}' % p
    m = re.match(r'^\{(Dynamic|Static)Resource ([\w.\-]+)\}$', s)
    if m:
        return '{DynamicResource %s}' % m.group(2) if m.group(1) == 'Dynamic' else '{StaticResource %s}' % m.group(2)
    if s.startswith('{x:Null}') or s == '{x:Null}':
        return s
    if s.startswith('{Binding'):
        s2 = re.sub(r'(Path=)?([A-Za-z]+)\.(\w+)', lambda mm: mm.group(0), s)
        return s2
    return v


class Node:
    def __init__(self, name):
        self.name = name
        self.attrs = collections.OrderedDict()
        self.props = []      # (propname, list[Node] | str)
        self.children = []
        self.extra_comment = None

    def prop_get(self, n):
        for i, (k, v) in enumerate(self.props):
            if k == n: return v
        return None


class Conv:
    def __init__(self, tree, events=None):
        self.root = tree
        self.events = events or {}
        self.res = collections.OrderedDict()       # key -> ET element (top-level resources)
        self.used = set()
        self.warn = []
        self.template_res = {}                      # key -> ControlTemplate ET
        self.style_res = {}
        self.sb_res = {}
        self.name_types = {}
        self.cur_key = None

    # -------- resource indexing
    def index_resources(self, rd_container):
        if rd_container is None: return
        for rd in rd_container:
            if split(rd.tag)[1] != 'ResourceDictionary': continue
            for c in rd:
                k = c.attrib.get('{%s}Key' % XNS)
                if k is not None:
                    self.res[k] = c
                    nm = split(c.tag)[1]
                    if nm == 'ControlTemplate': self.template_res[k] = c
                    if nm == 'Style': self.style_res[k] = c

    def keys_in(self, el, acc):
        for e in el.iter():
            for k, v in e.attrib.items():
                for m in re.finditer(r'\{(?:Dynamic|Static)Resource ([\w.\-]+)\}', v):
                    acc.add(m.group(1))
            ln = split(e.tag)[1]
            if ln in ('StaticResourceExtension', 'DynamicResourceExtension'):
                c = e.find('{%s}Ctor' % ILS)
                if c is not None and c.text: acc.add(c.text.strip())

    def compute_used(self, roots):
        acc = set()
        for r in roots: self.keys_in(r, acc)
        work = list(acc)
        seen = set()
        while work:
            k = work.pop()
            if k in seen: continue
            seen.add(k)
            if k in self.res:
                n = set()
                self.keys_in(self.res[k], n)
                for x in n:
                    if x not in seen: work.append(x)
        self.used = seen

    # -------- helpers
    def typeext(self, el):
        """read <X.TargetType><x:TypeExtension><Ctor>T</Ctor>"""
        for c in el:
            if split(c.tag)[1].endswith('.TargetType'):
                for t in c.iter('{%s}Ctor' % ILS):
                    return t.text.strip()
        a = el.attrib.get('TargetType')
        return a

    def ctor_text(self, el):
        c = el.find('{%s}Ctor' % ILS)
        return c.text.strip() if c is not None and c.text else ''

    def value_of_ext(self, el):
        """For an extension element returns attribute string or None if not an extension."""
        ln = split(el.tag)[1]
        if ln == 'NullExtension': return '{x:Null}'
        if ln == 'StaticResourceExtension': return '{StaticResource %s}' % self.ctor_text(el)
        if ln == 'DynamicResourceExtension': return '{DynamicResource %s}' % self.ctor_text(el)
        if ln == 'Binding':
            path = self.ctor_text(el)
            en = el.attrib.get('ElementName')
            rs = el.find('{%s}RelativeSource' % WPF)
            parts = []
            if path: parts.append(path)
            if en: parts.append('ElementName=' + en)
            if rs is not None or el.find('{%s}Binding.RelativeSource' % WPF) is not None:
                parts.append('RelativeSource={RelativeSource Self}')
            if not parts: return '{Binding}'
            if not path: parts.insert(0, '.')
            return '{Binding ' + ', '.join(parts) + '}'
        if ln == 'TypeExtension':
            return self.ctor_text(el)
        return None

    # -------- attribute conversion
    def attr(self, owner_tag, k, v):
        """returns (name, value) or None"""
        if k.startswith('{'):
            ns, n = split(k)
            if ns == XNS:
                if n == 'Name': return ('x:Name', v)
                if n == 'Key': return ('x:Key', v)
                if n == 'Class': return ('x:Class', v)
                return None
            return None
        if k in ('xmlns',) or k.startswith('xmlns'): return None
        if k == 'Name': return ('x:Name', v)
        if k.endswith('.Name'): return ('x:Name', v)
        owner, _, prop = k.rpartition('.')
        if not owner: prop = k
        if prop in DROP_PROPS: return None
        if owner == 'Storyboard' or owner == 'Timeline': return None
        if owner == 'Stylus' or owner == 'KeyboardNavigation': return None
        if owner == 'Panel' and prop == 'ZIndex': return ('ZIndex', v)
        if owner == 'VirtualizingPanel' or owner == 'VirtualizingStackPanel': return None
        v = conv_markup(v, prop)
        if v is None: return None
        if prop in ('Style', 'Template'):
            m = re.match(r'\{(?:Dynamic|Static)Resource ([\w.\-]+)\}$', v)
            if m: return ('Theme', '{StaticResource %s}' % m.group(1))
            return None
        if prop == 'Visibility' and not v.startswith('{'):
            return ('IsVisible', 'True' if v == 'Visible' else 'False')
        if prop in ('Source', 'ImageSource') and not v.startswith('{'):
            return ('Source', fix_image(v))
        if prop in ('RenderTransformOrigin', 'StartPoint', 'EndPoint', 'GradientOrigin', 'Center') and not v.startswith('{'):
            if owner_tag in ('Ellipse', 'EllipseGeometry'): return (prop, v)
            return (prop, rel(v))
        if prop in ('RadiusX', 'RadiusY') and owner_tag == 'RadialGradientBrush':
            return (prop, relsingle(v))
        if prop in SIZE_PROPS and v == 'Auto' and owner_tag not in ('ColumnDefinition','RowDefinition'): v = 'NaN'
        if prop == 'ToolTip': return ('ToolTip.Tip', v)
        if prop == 'StrokeDashArray': v = re.sub(r'\s+', ',', v.strip())
        if prop in ('HorizontalAlignment', 'VerticalAlignment', 'HorizontalContentAlignment', 'VerticalContentAlignment') and v and not v.startswith('{'):
            v = v[0].upper() + v[1:]
        if owner in ATTACHED_OWNERS and owner not in ('ToolTip',):
            if owner == 'ToolTipService': return None
            return ('%s.%s' % (owner, prop), v)
        if prop == 'CornerRadius' and owner_tag == 'Rectangle': return None
        if prop == 'IsDefault': return None
        return (prop, v)

    # -------- element conversion
    def tagname(self, el):
        ns, n = split(el.tag)
        if ns == 'clr-namespace:CC': return 'local:' + n
        if ns == 'clr-namespace:CC.UControl': return 'UControl:' + n
        if ns.startswith('clr-namespace:Microsoft.Windows.Themes'): return 'Border'
        return n

    def conv(self, el, parent=None):
        tn = self.tagname(el)
        n = Node(tn)
        bare = split(el.tag)[1]
        for k, v in el.attrib.items():
            r = self.attr(tn, k, v)
            if r: n.attrs[r[0]] = r[1]
        # WPF Slider.TickFrequency defaults to 1, Avalonia's to 0 (snap-to-tick then only reaches Minimum/Maximum).
        if tn == 'Slider' and 'IsSnapToTickEnabled' in n.attrs and 'TickFrequency' not in n.attrs:
            n.attrs['TickFrequency'] = '1'
        for c in el:
            cns, cn = split(c.tag)
            if '.' in cn and cns == WPF:
                owner, _, prop = cn.partition('.')
                self.conv_propelem(n, bare, owner, prop, c)
            elif cn in ('Ctor',): continue
            else:
                self.conv_child(n, bare, c)
        self.post(n, el)
        return n

    def conv_child(self, n, bare, c):
        cn = split(c.tag)[1]
        if cn in ('VisualStateManager.VisualStateGroups',): return
        ch = self.conv(c, n)
        if ch is not None: n.children.append(ch)

    def conv_propelem(self, n, bare, owner, prop, c):
        # property element: <Owner.Prop> ... </Owner.Prop>
        if owner in ('VisualStateManager',): return
        if owner in ('Storyboard', 'Timeline'): return
        if prop in ('Triggers', 'EnterActions', 'ExitActions'): return
        if prop == 'Resources' and owner in ('FrameworkElement', 'Window', 'UserControl', 'FrameworkTemplate'):
            return    # resources handled globally
        if prop in DROP_PROPS or prop in ('FocusVisualStyle', 'ContextMenu'): return
        if bare == 'Track':
            m = {'Thumb': 'Thumb', 'IncreaseRepeatButton': 'IncreaseButton', 'DecreaseRepeatButton': 'DecreaseButton'}
            if prop in m:
                kids = [self.conv(x) for x in c]
                n.props.append((m[prop], kids)); return
        kids = list(c)
        pname = prop
        if prop == 'Visibility': pname = 'IsVisible'
        if owner in ATTACHED_OWNERS and owner not in ('ToolTip',) and owner != bare:
            pname = owner + '.' + prop
        if prop == 'ImageSource': pname = 'Source'
        if len(kids) == 1:
            ext = self.value_of_ext(kids[0])
            if ext is not None:
                if pname == 'Template' or pname == 'Style':
                    key = re.match(r'\{StaticResource (.+)\}', ext)
                    if key:
                        n.attrs['Theme'] = '{StaticResource %s}' % key.group(1); return
                n.attrs[pname] = ext; return
        if prop == 'Template' or prop == 'Style':
            return
        nodes = []
        for k in kids:
            nodes.append(self.conv(k))
        if bare in ('Border', 'Grid') and prop in ('Background', 'BorderBrush') and False:
            pass
        n.props.append((pname, nodes))

    def post(self, n, el):
        nm = n.attrs.get('x:Name')
        if n.name == 'Image' or n.name == 'ImageBrush':
            pass
        # events
        if nm and nm in self.events and n.name not in ('Style',):
            for ev, h in self.events[nm]:
                av = EVENT_MAP.get(ev)
                if av: n.attrs[av] = h
        # transform origin default (WPF 0,0)
        if n.prop_get('RenderTransform') is not None and 'RenderTransformOrigin' not in n.attrs:
            n.attrs['RenderTransformOrigin'] = '0%,0%'
        if 'RenderTransform' in n.attrs and 'RenderTransformOrigin' not in n.attrs:
            n.attrs['RenderTransformOrigin'] = '0%,0%'
        if n.name == 'ContentPresenter':
            n.attrs.setdefault('Content', '{TemplateBinding Content}')
            n.attrs.setdefault('ContentTemplate', '{TemplateBinding ContentTemplate}')
            if 'x:Name' not in n.attrs: n.attrs['x:Name'] = 'PART_ContentPresenter'
            if 'HorizontalContentAlignment' in n.attrs: n.attrs.pop('HorizontalContentAlignment')
        if n.name == 'Track':
            n.attrs.setdefault('Minimum', '{TemplateBinding Minimum}')
            n.attrs.setdefault('Maximum', '{TemplateBinding Maximum}')
            n.attrs.setdefault('Value', '{TemplateBinding Value, Mode=TwoWay}')
            n.attrs.setdefault('Orientation', '{TemplateBinding Orientation}')
            for pn, v in n.props:
                if pn == 'DecreaseButton' and v: v[0].attrs.setdefault('x:Name', 'PART_DecreaseButton')
                if pn == 'IncreaseButton' and v: v[0].attrs.setdefault('x:Name', 'PART_IncreaseButton')
        if n.name == 'Label' and 'Padding' not in n.attrs: n.attrs['Padding'] = '5'
        if n.name == 'ImageBrush':
            pass
        if n.name == 'Image' and 'Stretch' not in n.attrs: n.attrs['Stretch'] = 'Uniform'
        if n.name == 'TextBlock' and False: pass

    # -------- serialization
    def ser(self, n, ind, out, extra_ns=False):
        pad = '  ' * ind
        attrs = ''.join(' %s=%s' % (k, quoteattr(v)) for k, v in n.attrs.items())
        if not n.props and not n.children:
            out.append('%s<%s%s/>' % (pad, n.name, attrs)); return
        out.append('%s<%s%s>' % (pad, n.name, attrs))
        for pn, val in n.props:
            if isinstance(val, str):
                out.append('%s  <%s.%s>%s</%s.%s>' % (pad, n.name, pn, val, n.name, pn)); continue
            out.append('%s  <%s.%s>' % (pad, n.name, pn))
            for ch in val: self.ser(ch, ind + 2, out)
            out.append('%s  </%s.%s>' % (pad, n.name, pn))
        for ch in n.children: self.ser(ch, ind + 1, out)
        out.append('%s</%s>' % (pad, n.name))

    # ================================================================ styles / templates
    PSEUDO = {'IsMouseOver': ':pointerover', 'IsPressed': ':pressed', 'IsEnabled': ':disabled',
              'IsChecked': ':checked', 'IsFocused': ':focus', 'IsKeyboardFocused': ':focus', 'IsSelected': ':selected',
              'IsKeyboardFocusWithin': ':focus-within', 'IsDefaulted': None, 'IsDefault': None,
              'IsMouseDirectlyOver': ':pointerover', 'IsDragging': ':pressed', 'IsOpen': ':open_'}

    def cond_sel(self, prop, val, nullval=False):
        p = prop.split('.')[-1]
        if nullval:
            if p == 'IsChecked': return ':indeterminate'
            return None
        vl = str(val).lower()
        if p in ('IsDefaulted', 'IsDefault', 'IsKeyboardFocusWithin', 'HasItems_', 'IsGrouping', 'IsVirtualizingWhenGrouping', 'IsSelectionActive', 'IsHitTestVisible'):
            return None
        if p in self.PSEUDO and self.PSEUDO[p]:
            ps = self.PSEUDO[p]
            if p == 'IsEnabled':
                return ':disabled' if vl == 'false' else ':not(:disabled)'
            return ps if vl == 'true' else ':not(%s)' % ps
        if p == 'Orientation': return '[Orientation=%s]' % val
        if p == 'HasContent': return None
        if p == 'Content': return '[Content=%s]' % val
        if p == 'Tag': return '[Tag=%s]' % val
        if p == 'TickPlacement': return None
        if p == 'IsSelectionRangeEnabled': return None
        return '[%s=%s]' % (p, val)

    def conv_setter_value(self, st, tname=None, prop=None):
        """returns (kind, value): kind 'attr' -> str, 'elem' -> list[Node]"""
        if 'Value' in st.attrib:
            v = st.attrib['Value']
            return 'attr', v
        sv = st.find('{%s}Setter.Value' % WPF)
        if sv is None:
            return 'attr', ''
        kids = list(sv)
        if len(kids) == 1:
            ext = self.value_of_ext(kids[0])
            if ext is not None: return 'attr', ext
        return 'elem', [self.conv(k) for k in kids]

    def setter_xml(self, prop_full, kind, val, indent, out, target_type=None):
        p = prop_full.split('.')[-1]
        owner = prop_full.rpartition('.')[0]
        pad = '  ' * indent
        if p in DROP_PROPS or p in ('FocusVisualStyle',): return
        if owner in ('Stylus', 'KeyboardNavigation', 'VirtualizingPanel', 'VirtualizingStackPanel'): return
        if p == 'Template' or p == 'Style': return
        if getattr(self, 'cur_target', '') == 'ListBox' and p == 'VerticalContentAlignment': return
        name = p
        if p == 'Visibility':
            name = 'IsVisible'
            if kind == 'attr' and not val.startswith('{'): val = 'True' if val == 'Visible' else 'False'
        elif p in ('Source', 'ImageSource') and kind == 'attr' and not val.startswith('{'):
            val = fix_image(val); name = 'Source'
        elif owner in ATTACHED_OWNERS and owner != 'ToolTip':
            name = owner + '.' + p
        elif p == 'ToolTip': name = 'ToolTip.Tip'
        if p == 'Panel.ZIndex': name = 'ZIndex'
        if kind == 'attr':
            val = conv_markup(val)
            if p in SIZE_PROPS and val == 'Auto': val = 'NaN'
            if p in ('RenderTransformOrigin',): val = rel(val)
            out.append('%s<Setter Property=%s Value=%s/>' % (pad, quoteattr(name), quoteattr(val)))
        else:
            out.append('%s<Setter Property=%s>' % (pad, quoteattr(name)))
            for nd in val: self.ser(nd, indent + 1, out)
            out.append('%s</Setter>' % pad)

    def find_named(self, node, name, acc=None):
        if node.attrs.get('x:Name') == name: return node
        for ch in node.children:
            r = self.find_named(ch, name)
            if r: return r
        for pn, v in node.props:
            if isinstance(v, list):
                for ch in v:
                    r = self.find_named(ch, name)
                    if r: return r
        return None

    def storyboards_of(self, tmpl):
        d = {}
        for c in tmpl:
            if split(c.tag)[1] == 'FrameworkTemplate.Resources':
                for rd in c:
                    for s in rd:
                        k = s.attrib.get('{%s}Key' % XNS)
                        if k: d[k] = s
        return d

    def storyboard_anim(self, sb, indent, out):
        """Convert a looping storyboard to Avalonia <Style.Animations> content. Returns list of (targetName, xml lines)"""
        res = []
        pad = '  ' * indent
        dur_max = 0.0
        blocks = []
        for an in sb:
            ln = split(an.tag)[1]
            tprop = an.attrib.get('{%s}TargetProperty' % WPF) or an.attrib.get('Storyboard.TargetProperty')
            tname = an.attrib.get('Storyboard.TargetName')
            frames = []
            for kf in an:
                kt = kf.attrib.get('DoubleKeyFrame.KeyTime') or kf.attrib.get('ColorKeyFrame.KeyTime') or '0'
                kv = kf.attrib.get('DoubleKeyFrame.Value') or kf.attrib.get('ColorKeyFrame.Value')
                frames.append((self.parse_time(kt), kv))
            if frames: dur_max = max(dur_max, max(f[0] for f in frames))
            blocks.append((tname, tprop, frames))
        return blocks, dur_max

    @staticmethod
    def parse_time(t):
        parts = t.split(':')
        parts = [float(p) for p in parts]
        sec = 0.0
        for p in parts: sec = sec * 60 + p
        return sec

    def conv_triggers(self, trig_el, tmpl_root, indent, out, sbs=None, base_sel='^'):
        """trig_el: element holding Trigger/MultiTrigger children. tmpl_root: Node of template root (to resolve target names), may be None."""
        for t in trig_el:
            ln = split(t.tag)[1]
            if ln not in ('Trigger', 'MultiTrigger', 'MultiDataTrigger', 'DataTrigger'): continue
            sels = []
            ok = True
            if ln == 'Trigger':
                prop = t.attrib.get('Property', '')
                nullv = t.find('{%s}Trigger.Value' % WPF) is not None
                val = t.attrib.get('Value', '')
                s = self.cond_sel(prop, val, nullv)
                src = t.attrib.get('SourceName')
                if s is None: ok = False
                else: sels.append(s)
            elif ln == 'MultiTrigger':
                conds = t.find('{%s}MultiTrigger.Conditions' % WPF)
                for c in conds:
                    s = self.cond_sel(c.attrib.get('Property', ''), c.attrib.get('Value', ''))
                    if s is None: ok = False
                    else: sels.append(s)
            elif ln == 'MultiDataTrigger':
                conds = t.find('{%s}MultiDataTrigger.Conditions' % WPF)
                for c in conds:
                    b = c.find('{%s}Condition.Binding' % WPF)
                    bind = b[0] if b is not None else None
                    path = self.ctor_text(bind) if bind is not None else ''
                    en = bind.attrib.get('ElementName') if bind is not None else None
                    if en:
                        # cross-element condition: cannot be expressed; skip
                        ok = False
                    else:
                        s = self.cond_sel(path, c.attrib.get('Value', ''))
                        if s is None: ok = False
                        else: sels.append(s)
            else:
                ok = False
            if not ok:
                self.warn.append('skipped trigger %s %s' % (ln, t.attrib))
                continue
            sel = ''.join(sels)
            setters = [s for s in t if split(s.tag)[1] == 'Setter']
            by_target = collections.OrderedDict()
            for s in setters:
                by_target.setdefault(s.attrib.get('TargetName'), []).append(s)
            # enter actions
            enter = t.find('{%s}TriggerBase.EnterActions' % WPF)
            if enter is not None and sbs is not None:
                for ba in enter:
                    if split(ba.tag)[1] != 'BeginStoryboard': continue
                    sbe = ba.find('{%s}BeginStoryboard.Storyboard' % WPF)
                    key = self.ctor_text(sbe[0]) if sbe is not None and len(sbe) else None
                    if key and key in sbs:
                        blocks, dur = self.storyboard_anim(sbs[key], indent, out)
                        for tname, tprop, frames in blocks:
                            tgt = ('/template/ ' + self.sel_of(tmpl_root, tname)) if tname else ''
                            by_target.setdefault('__anim__' + (tname or ''), []).append((tname, tprop, frames, dur, sbs[key]))
            for tname, sts in by_target.items():
                if tname and tname.startswith('__anim__'):
                    for (tn, tprop, frames, dur, sbel) in sts:
                        self.emit_anim(base_sel, sel, tn, tprop, frames, dur, sbel, tmpl_root, indent, out)
                    continue
                if tname:
                    tgt = ' /template/ ' + self.sel_of(tmpl_root, tname)
                else:
                    tgt = ''
                out.append('%s<Style Selector=%s>' % ('  ' * indent, quoteattr(base_sel + sel + tgt)))
                for s in sts:
                    prop = s.attrib.get('Property', '')
                    kind, val = self.conv_setter_value(s)
                    self.setter_xml(prop, kind, val, indent + 1, out)
                out.append('%s</Style>' % ('  ' * indent))

    def emit_anim(self, base_sel, sel, tname, tprop, frames, dur, sbel, tmpl_root, indent, out):
        pad = '  ' * indent
        tgt = (' /template/ ' + self.sel_of(tmpl_root, tname)) if tname else ''
        prop = None
        m = re.match(r'\(UIElement\.Opacity\)', tprop or '')
        if m: prop = 'Opacity'
        m = re.match(r'\(TextElement\.Foreground\)\.\(SolidColorBrush\.Color\)', tprop or '')
        if m: prop = 'Foreground'
        m = re.match(r'\(UIElement\.RenderTransform\)\.\(TransformGroup\.Children\)\[(\d)\]\.\(RotateTransform\.Angle\)', tprop or '')
        if m: prop = 'RotateTransform.Angle'
        if prop is None:
            self.warn.append('animation prop unsupported ' + str(tprop)); return
        out.append('%s<Style Selector=%s>' % (pad, quoteattr(base_sel + sel + tgt)))
        out.append('%s  <Style.Animations>' % pad)
        out.append('%s    <Animation Duration="%s" IterationCount="Infinite">' % (pad, self.fmt_dur(dur)))
        for t, v in frames:
            cue = '%g%%' % round(100.0 * t / dur, 3) if dur else '0%'
            out.append('%s      <KeyFrame Cue=%s>' % (pad, quoteattr(cue)))
            out.append('%s        <Setter Property=%s Value=%s/>' % (pad, quoteattr(prop), quoteattr(v)))
            out.append('%s      </KeyFrame>' % pad)
        out.append('%s    </Animation>' % pad)
        out.append('%s  </Style.Animations>' % pad)
        out.append('%s</Style>' % pad)

    @staticmethod
    def fmt_dur(d):
        h = int(d // 3600); m = int((d % 3600) // 60); s = d - h * 3600 - m * 60
        return '%02d:%02d:%06.3f' % (h, m, s)

    def sel_of(self, tmpl_root, name):
        node = self.find_named(tmpl_root, name) if tmpl_root is not None else None
        tn = node.name if node is not None else self.name_types.get(name, 'Control')
        return '%s#%s' % (tn, name)

    def migrate_base_values(self, tmpl_root, trig_parent, indent, out):
        """Move attrs of template parts that triggers modify into base styles (triggers beat template-local values in WPF)."""
        targets = collections.OrderedDict()
        if trig_parent is None: return
        for t in trig_parent.iter():
            if split(t.tag)[1] != 'Setter': continue
            tn = t.attrib.get('TargetName')
            if not tn: continue
            p = t.attrib.get('Property', '').split('.')[-1]
            if p == 'Visibility': p = 'IsVisible'
            if p == 'ImageSource': p = 'Source'
            targets.setdefault(tn, set()).add(p)
        # animations
        for t in trig_parent.iter():
            for k, v in t.attrib.items():
                if k.endswith('TargetName') and 'Storyboard' in k:
                    pass
        res = []
        for tn, props in targets.items():
            node = self.find_named(tmpl_root, tn)
            if node is None: continue
            moved = []
            for p in sorted(props):
                if p in node.attrs:
                    moved.append((p, node.attrs.pop(p)))
            elems = []
            for p in list(props):
                for i, (pn, v) in enumerate(list(node.props)):
                    if pn == p:
                        elems.append((pn, v)); node.props.remove((pn, v))
            if not moved and not elems: continue
            res.append((tn, node, moved, elems))
        return res

    def emit_base(self, res, indent, out):
        for tn, node, moved, elems in res:
            pad = '  ' * indent
            out.append('%s<Style Selector=%s>' % (pad, quoteattr('^ /template/ %s#%s' % (node.name, tn))))
            for p, v in moved:
                out.append('%s  <Setter Property=%s Value=%s/>' % (pad, quoteattr(p), quoteattr(v)))
            for p, v in elems:
                out.append('%s  <Setter Property=%s>' % (pad, quoteattr(p)))
                for nd in v: self.ser(nd, indent + 2, out)
                out.append('%s  </Setter>' % pad)
            out.append('%s</Style>' % pad)

    def template_to_theme_body(self, tmpl, indent, out, trig_extra=None):
        """Write <Setter Property="Template"><ControlTemplate>…</ControlTemplate></Setter> and trigger styles for a WPF ControlTemplate element."""
        pad = '  ' * indent
        body = [c for c in tmpl if not split(c.tag)[1].startswith('ControlTemplate.') and not split(c.tag)[1].startswith('FrameworkTemplate.')]
        trig = tmpl.find('{%s}ControlTemplate.Triggers' % WPF)
        sbs = self.storyboards_of(tmpl)
        nodes = [self.conv(b) for b in body]
        root = nodes[0] if nodes else None
        # make trigger names resolvable
        base = self.migrate_base_values(root, trig, indent, out) if (trig is not None and root is not None) else []
        # animation targets: also need base for animated props? (opacity animated from keyframes; fine)
        out.append('%s<Setter Property="Template">' % pad)
        out.append('%s  <ControlTemplate>' % pad)
        for nd in nodes: self.ser(nd, indent + 2, out)
        out.append('%s  </ControlTemplate>' % pad)
        out.append('%s</Setter>' % pad)
        self.emit_base(base, indent, out)
        if trig is not None:
            self.conv_triggers(trig, root, indent, out, sbs)

    def emit_override(self, key, tmpl, indent, out):
        txt = OVR[key]
        for m in re.finditer(r'<(\w+)\s+x:Name="(\w+)"', txt):
            self.name_types[m.group(2)] = m.group(1)
        for ln in txt.strip().split('\n'):
            out.append('  ' * indent + ln)
        trg = tmpl.find('{%s}ControlTemplate.Triggers' % WPF) if tmpl is not None else None
        if trg is not None:
            self.conv_triggers(trg, None, indent, out, self.storyboards_of(tmpl))

    def style_to_theme(self, key, st, indent, out):
        tt = self.typeext(st) or 'Control'
        self.cur_key = key; self.cur_target = tt
        pad = '  ' * indent
        keyattr = ' x:Key=%s' % quoteattr(key) if key else ''
        out.append('%s<ControlTheme%s TargetType=%s>' % (pad, keyattr, quoteattr(tt)))
        tmpl_done = False
        for c in st:
            cn = split(c.tag)[1]
            if cn == 'Setter':
                prop = c.attrib.get('Property', '')
                if prop.split('.')[-1] == 'Template':
                    sv = c.find('{%s}Setter.Value' % WPF)
                    kid = sv[0] if sv is not None and len(sv) else None
                    tmpl = None
                    if kid is not None and split(kid.tag)[1] == 'ControlTemplate': tmpl = kid
                    elif kid is not None:
                        tk = self.ctor_text(kid)
                        tmpl = self.template_res.get(tk)
                    elif 'Value' in c.attrib:
                        m = re.match(r'\{(?:Dynamic|Static)Resource (.+)\}', c.attrib['Value'])
                        if m: tmpl = self.template_res.get(m.group(1))
                    if key in OVR:
                        self.emit_override(key, tmpl, indent + 1, out)
                        tmpl_done = True
                    elif tmpl is not None:
                        self.template_to_theme_body(tmpl, indent + 1, out)
                        tmpl_done = True
                    continue
                kind, val = self.conv_setter_value(c)
                self.setter_xml(prop, kind, val, indent + 1, out)
        trg = st.find('{%s}Style.Triggers' % WPF)
        if trg is not None:
            self.conv_triggers(trg, None, indent + 1, out)
        out.append('%s</ControlTheme>' % pad)

    def emit_resource(self, key, el, indent, out):
        pad = '  ' * indent
        ln = split(el.tag)[1]
        if ln == 'ControlTemplate':
            tt = self.typeext(el) or 'Control'
            out.append('%s<ControlTheme x:Key=%s TargetType=%s>' % (pad, quoteattr(key), quoteattr(tt)))
            if key in OVR: self.emit_override(key, el, indent + 1, out)
            else: self.template_to_theme_body(el, indent + 1, out)
            out.append('%s</ControlTheme>' % pad)
        elif ln == 'Style':
            self.style_to_theme(key, el, indent, out)
        elif ln == 'Storyboard':
            return
        else:
            n = self.conv(el)
            self.ser(n, indent, out)

    def resources_xml(self, indent, out):
        for k, el in self.res.items():
            if k not in self.used or 'FocusVisual' in k: continue
            self.emit_resource(k, el, indent, out)


NS_DECL = ('xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" '
           'xmlns:local="using:CC" xmlns:UControl="using:CC.UControl"')


def load_events(csfile):
    ev = collections.defaultdict(list)
    if not csfile: return ev
    txt = open(csfile, encoding='utf-8-sig').read()
    for m in re.finditer(r'\(\((?:\w+)\)(\w+)\)\.(\w+) \+= (\w+);', txt):
        ev[m.group(1)].append((m.group(2), m.group(3)))
    # window itself: "window = (MainWindow)target;" then ((FrameworkElement)window).Loaded
    return ev


def main():
    src, dst = sys.argv[1], sys.argv[2]
    evfile = None
    if '--events' in sys.argv: evfile = sys.argv[sys.argv.index('--events') + 1]
    tree = ET.parse(src)
    root = tree.getroot()
    c = Conv(root, load_events(evfile))
    # resources
    res_prop = None
    for ch in root:
        if split(ch.tag)[1] in ('FrameworkElement.Resources', 'Application.Resources', 'UserControl.Resources'):
            c.index_resources(ch)
    if '--app' in sys.argv:
        for p in sys.argv[sys.argv.index('--app') + 1:]:
            if p.startswith('--'): break
            r2 = ET.parse(p).getroot()
            for ch in r2:
                if split(ch.tag)[1] in ('Application.Resources',): c.index_resources(ch)
    vis = [ch for ch in root if '.' not in split(ch.tag)[1]]
    c.compute_used(vis + [ch for ch in root if split(ch.tag)[1].endswith('.Triggers')])
    rn, rname = split(root.tag)
    out = []
    rootnode = c.conv(root)
    # root attrs/props
    rtag = rname
    attrs = collections.OrderedDict(rootnode.attrs)
    attrs.pop('x:Name', None) if rtag == 'Window' else None
    if rtag == 'Window':
        attrs['WindowDecorations'] = 'None'; attrs['CanResize'] = 'False'; attrs['FontSize'] = '12'
        attrs['TransparencyLevelHint'] = 'Transparent'
    out.append('<%s %s' % (rtag, NS_DECL))
    for k, v in attrs.items():
        out.append('    %s=%s' % (k, quoteattr(v)))
    out[-1] += '>'
    # resources
    res_lines = []
    c.resources_xml(2, res_lines)
    if res_lines:
        out.append('  <%s.Resources>' % rtag)
        out.extend(res_lines)
        out.append('  </%s.Resources>' % rtag)
    if 'ListBoxStyle1_ExpressKey' in c.used:
        out.append('  <%s.Styles>' % rtag)
        out.extend('    ' + l for l in LISTBOXITEM_STYLES.strip().split('\n'))
        out.append('  </%s.Styles>' % rtag)
    for pn, v in rootnode.props:
        if isinstance(v, list):
            out.append('  <%s.%s>' % (rtag, pn))
            for nd in v: c.ser(nd, 2, out)
            out.append('  </%s.%s>' % (rtag, pn))
    for ch in rootnode.children: c.ser(ch, 1, out)
    out.append('</%s>' % rtag)
    open(dst, 'w', encoding='utf-8').write('<?xml version="1.0" encoding="utf-8"?>\n' + '\n'.join(out) + '\n')
    for w in c.warn: print('WARN', w, file=sys.stderr)

if __name__ == '__main__':
    main()
