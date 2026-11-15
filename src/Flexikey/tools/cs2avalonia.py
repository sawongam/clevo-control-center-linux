#!/usr/bin/env python3
"""First-pass mechanical WPF code-behind -> Avalonia code-behind. Result is then hand-fixed."""
import sys, re

CASTS = ['UIElement', 'FrameworkElement', 'ContentControl', 'ToggleButton', 'Control', 'Panel', 'Shape', 'Selector',
         'ItemsControl', 'RangeBase', 'ButtonBase', 'TextBoxBase', 'Window', 'TextBlock', 'CheckBox', 'Image', 'ListBox',
         'TextBox', 'Button', 'RadioButton', 'Slider', 'Grid', 'Canvas', 'StackPanel', 'Rectangle', 'Path', 'Label',
         'ScrollViewer', 'DependencyObject', 'Border', 'Ellipse', 'Viewbox', 'UserControl', 'Page_FlexiKey', 'FileDialog',
         'CommonDialog', 'TextElement', 'Visual', 'ItemsPresenter']

def conv(src):
    s = src
    # drop IL comments
    s = re.sub(r'^\s*//IL_[^\n]*\n', '', s, flags=re.M)
    # remove generated tail: from [DebuggerNonUserCode] ... Connect ... end of class
    i = s.find('\t[DebuggerNonUserCode]')
    if i >= 0:
        # keep the closing of class/namespace
        s = s[:i].rstrip() + '\n}\n'
    # remove internal field declarations (generated)
    s = re.sub(r'^\s*internal [\w<>\.]+ \w+;\n', '', s, flags=re.M)
    s = re.sub(r'^\s*private bool _contentLoaded;\n', '', s, flags=re.M)
    s = re.sub(r'^\s*\[GeneratedCode[^\n]*\n', '', s, flags=re.M)
    s = s.replace(', IComponentConnector', '')
    # usings
    s = re.sub(r'^using (System\.Windows[\w\.]*|System\.CodeDom[\w\.]*|Microsoft\.Win32|System\.Drawing[\w\.]*|System\.Windows\.Forms);\n', '', s, flags=re.M)
    hdr = ('using Avalonia;\nusing Avalonia.Controls;\nusing Avalonia.Controls.Primitives;\nusing Avalonia.Controls.Shapes;\n'
           'using Avalonia.Input;\nusing Avalonia.Interactivity;\nusing Avalonia.Layout;\nusing Avalonia.Media;\n'
           'using Avalonia.Media.Imaging;\nusing Avalonia.Threading;\nusing CCenter.Platform;\nusing CC.Compat;\n')
    s = hdr + s
    # casts
    for _ in range(3):
        for c in CASTS:
            s = re.sub(r'\(\(%s\)([\w\.\[\]]+)\)' % c, r'\1', s)
    s = re.sub(r'\(\(CollectionView\)([\w\.]+)\)', r'\1', s)
    s = re.sub(r'\(\((?:ItemsControl|Selector)\)([\w\.]+)\)', r'\1', s)
    s = re.sub(r'\(\(SolidColorBrush\)([\w\.]+)\)', r'((SolidColorBrush)\1)', s)
    # (UIElement)x etc as cast prefix (non-parenthesised)
    for c in ('UIElement', 'FrameworkElement', 'Control'):
        s = re.sub(r'\((%s)\)(\w+)\.' % c, r'\2.', s)
    # visibility
    s = re.sub(r'\.Visibility = \(Visibility\)0;', '.IsVisible = true;', s)
    s = re.sub(r'\.Visibility = \(Visibility\)[12];', '.IsVisible = false;', s)
    s = re.sub(r'\(int\)([\w\.]+)\.Visibility == 0', r'\1.IsVisible', s)
    s = re.sub(r'\(int\)([\w\.]+)\.Visibility == 1', r'!\1.IsVisible', s)
    s = s.replace('Visibility.Visible', 'true')
    s = re.sub(r'\bBrush\b', 'IBrush', s)
    s = s.replace('(IBrush)new', 'new')
    s = re.sub(r'\(Delegate\)\(Action\)\(', '(', s)
    s = re.sub(r'\(\(DispatcherObject\)this\)\.Dispatcher\.BeginInvoke\(', 'Dispatcher.UIThread.Post(', s)
    s = re.sub(r'\(\(DispatcherObject\)this\)\.Dispatcher\.Invoke\(\(DispatcherPriority\)\d+, ', 'Dispatcher.UIThread.Invoke(', s)
    s = re.sub(r', Array\.Empty<object>\(\)\)', ')', s)
    s = re.sub(r'\(\(DispatcherObject\)(\w+)\)\.Dispatcher', r'Dispatcher.UIThread', s)
    s = s.replace('((ToggleButton)', '(').replace('MouseButtonEventArgs', 'PointerPressedEventArgs')
    s = s.replace('RoutedPropertyChangedEventArgs<double>', 'RangeBaseValueChangedEventArgs')
    s = s.replace('DependencyPropertyChangedEventArgs', 'AvaloniaPropertyChangedEventArgs')
    s = s.replace('KeyEventArgs', 'KeyEventArgs')
    return s

if __name__ == '__main__':
    t = open(sys.argv[1], encoding='utf-8-sig').read()
    open(sys.argv[2], 'w', encoding='utf-8').write(conv(t))
