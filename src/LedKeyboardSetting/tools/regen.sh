#!/bin/sh
# regenerate all axaml from the decompiled XAML (hand edits to *.axaml are overwritten! use tools/overrides instead)
cd "$(dirname "$0")/.."
D=../../reference/decompiled/Keyboard
for p in "Page_LED_Device Page_LED_Device" "MainWindow MainWindow" "loading loading"; do set -- $p; python3 tools/xaml2axaml.py $D/CC/$1.xaml $2.axaml --app $D/CC/App.xaml 2>&1 | grep -v WARN; done
python3 tools/xaml2axaml.py $D/CC.UControl/UserControl_DialogBox.xaml UserControl_DialogBox.axaml 2>&1 | tail -1
