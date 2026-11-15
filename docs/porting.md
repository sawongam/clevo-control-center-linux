# Porting guide: Clevo Control Center 3.0 (WPF) to Avalonia 12 on Linux

This guide is for contributors who port or fix an app. It records the rules used for the existing nine apps.

## Goal
An exact clone of each Windows app's UI and behaviour on Linux (.NET 10, Avalonia 12): same window size,
layout, images, strings, page flow, button states, animations and logic.

This is a port, not a redesign. Do not simplify, modernise, restyle, rename or reorder anything. When in
doubt, do what the original decompiled code does.

## Where things are
| path | what |
|---|---|
| `reference/decompiled/<App>/` | ILSpy output of the Windows app (`.xaml` decompiled from BAML, `.cs`, images). The spec. Not part of the repository (gitignored); you need your own copy of the original software to produce it. |
| `reference/decompiled/_pkg/<App>/` | the original installed files (mtlanguage.ini, oem.ini, native DLLs, helper exes). Same caveat. |
| [`protocol.md`](protocol.md) | decoded firmware protocol, verified on the real laptop |
| `src/CCenter.Platform/` | shared library (hardware bridge, Win32 shims, app host) |
| `src/<App>/` | one project per app; per-app notes live in [`apps/<App>.md`](README.md#per-app-notes) |

Target machine: Clevo NH5x_NH7xHP, i7-11800H, RTX 3060 (keyboard type byte KBTP=0x06), 2 fans. The original
code has many branches for other models; port all branches faithfully, model detection decides at runtime.

## Project skeleton
Copy `csproj`, `Program.cs` and `App.axaml(.cs)` from an existing small app (for example `src/EnergySave`). Set `AssemblyName` and
`RootNamespace` to your project name. Keep the **original C# namespaces and class names** in
the ported files. Entry point must be `AppHost.Run<App>(args)` (gives you `--screenshot`).
Include every original image as `<AvaloniaResource Include="..."/>` at the **same relative
path and same file-name case** the XAML/code refers to (Linux is case-sensitive: if the XAML
says `Image/x.png` and the folder is `image/`, fix the folder name you copy to, not 500 XAML
lines; if both casings are used, pick one and fix the minority).
Data files the app reads at run time (mtlanguage.ini, oem.ini, …) go in as
`<Content Include="..." CopyToOutputDirectory="PreserveNewest"/>` and are read relative to
`AppContext.BaseDirectory`.

Build: `dotnet build src/<App>/<App>.csproj`

## Reading the decompiled XAML
ilspy prints BAML verbosely. Normalise while porting:
- `FrameworkElement.Width="80"` → `Width="80"`; same for `Control.`, `UIElement.`, `Panel.`,
  `Shape.`, `TextBlock.`, `Window.`, `ContentControl.` … prefixes on the element's own properties.
- `<x:TypeExtension><Ctor xmlns="https://github.com/icsharpcode/ILSpy">Button</Ctor></x:TypeExtension>`
  → `TargetType="Button"`.
- `<Control.Background><NullExtension/></Control.Background>` → `Background="{x:Null}"`.
- Root xmlns → `xmlns="https://github.com/avaloniaui"`; file extension `.axaml`.
- Drop `mc:`, `d:`, WindowsFormsIntegration namespaces.

## WPF → Avalonia 12 mapping
| WPF | Avalonia |
|---|---|
| `Visibility="Collapsed"` / `.Visibility = Visibility.Collapsed` | `IsVisible="False"` / `.IsVisible = false` |
| `Visibility="Hidden"` | `IsVisible="False"` when the parent is a Canvas or the element is absolutely placed by Margin in a Grid (almost always here). Where it sits in a StackPanel/auto row and must keep its space, use `Opacity="0" IsHitTestVisible="False"`. |
| `Window WindowStyle="None"` | `WindowDecorations="None"` |
| `AllowsTransparency="True"` + transparent bg | `TransparencyLevelHint="Transparent" Background="Transparent"` |
| `ResizeMode="NoResize"` | `CanResize="False"` |
| `Topmost` | `Topmost` |
| `DragMove()` in MouseLeftButtonDown | `BeginMoveDrag(e)` in PointerPressed (see an existing app) |
| `MouseLeftButtonDown/Up`, `MouseEnter/Leave`, `MouseMove`, `MouseWheel` | `PointerPressed/Released`, `PointerEntered/Exited`, `PointerMoved`, `PointerWheelChanged` (check `e.GetCurrentPoint(x).Properties.IsLeftButtonPressed`) |
| `PreviewMouseDown` etc. (tunnel) | `AddHandler(PointerPressedEvent, h, RoutingStrategies.Tunnel)` |
| `e.GetPosition(el)` | same |
| `Mouse.Capture(el)` | `e.Pointer.Capture(el)` |
| `Style` with `x:Key` + `ControlTemplate` + `Triggers` | `ControlTheme x:Key=… TargetType=…` with `<Setter Property="Template">` and nested `<Style Selector="^:pointerover /template/ Type#name">` (see `BTN_CLOSE` in an existing app). Apply with `Theme="{StaticResource key}"`. |
| `Trigger IsMouseOver` / `IsPressed` / `IsEnabled=False` / `IsChecked=True` / `IsFocused` | `:pointerover` / `:pressed` / `:disabled` / `:checked` / `:focus` |
| Trigger order: later trigger wins | put the later trigger's Style later |
| `DataTrigger`, `MultiTrigger` | binding + converter, or set a class from code (`el.Classes.Set("x", cond)`) and style on `.x` |
| `Style` without key (implicit) | `<Style Selector="Button">` in `<Window.Styles>` |
| `{DynamicResource {x:Static SystemColors.XxxBrushKey}}` | the literal Windows default colour (ControlText #000000, Control #F0F0F0, Highlight #0078D7, GrayText #6D6D6D, Window #FFFFFF) |
| `OverridesDefaultStyle`, `SnapsToDevicePixels`, `FocusVisualStyle` | drop (set `FocusAdorner="{x:Null}"` so no focus ring appears that WPF didn't show) |
| `UseLayoutRounding` | `UseLayoutRounding` |
| `LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1"` | `StartPoint="50%,0%" EndPoint="50%,100%"` |
| `RadialGradientBrush` | exists; `Center`/`GradientOrigin` as `50%,50%`, `RadiusX/Y` as `50%` |
| `ImageBrush ImageSource="x.png"` | `ImageBrush Source="/x.png"` |
| `Image Source="Image/x.png"` | `Source="/Image/x.png"` (leading slash = this assembly's resources) |
| `new BitmapImage(new Uri("Image/x.png", UriKind.Relative))` or `pack://application:,,,/Image/x.png` | `new Bitmap(AssetLoader.Open(new Uri("avares://<AssemblyName>/Image/x.png")))` — write one static helper `Img.Load("Image/x.png")` with a cache and use it everywhere |
| `Label Content=` | `Label Content=` exists; padding default differs: WPF Label has `Padding="5"`. Set it explicitly so text lands in the same place. |
| `TextBlock`, `TextBox`, `Canvas`, `Grid`, `StackPanel`, `Border`, `Viewbox`, `ScrollViewer`, `Slider`, `ComboBox`, `CheckBox`, `RadioButton`, `ProgressBar`, `ListBox`, `Path`, `Rectangle`, `Ellipse`, `Line`, `Polyline` | same names. For templated ones (Slider, ComboBox, CheckBox, ScrollBar) the WPF template part names differ (`PART_Track` etc.): rebuild the template with Avalonia part names so it **looks** identical. |
| `Canvas.Left/Top`, `Grid.Row/Column`, `Panel.ZIndex` | same (`ZIndex`) |
| `RenderTransform` + `RenderTransformOrigin="0.5,0.5"` | same; origin `50%,50%`; note Avalonia default origin is centre, WPF default is `0,0` → **set `RenderTransformOrigin="0,0"` explicitly whenever WPF did not set one** |
| `Storyboard`/`DoubleAnimation`/`BeginStoryboard` | Avalonia `Animation` with `KeyFrame`s (`animation.RunAsync(control)`), or `Transitions`. Keep duration, easing, from/to, repeat, autoreverse identical. |
| `DispatcherTimer` | `Avalonia.Threading.DispatcherTimer` |
| `Dispatcher.Invoke/BeginInvoke` | `Dispatcher.UIThread.Invoke/Post` |
| `Application.Current.Shutdown()` | `(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown()` |
| `MessageBox.Show` | a small modal Window you write that looks like the Windows message box, unless the app has its own dialog control (most do) |
| `Frame` + `Page` navigation | `ContentControl` + `UserControl` (keep class names) |
| `Window.Loaded` / `Closing` / `Closed` / `StateChanged` | `Opened`/`Loaded` / `Closing` / `Closed` / property-changed on `WindowState` |
| `SystemParameters.PrimaryScreenWidth/WorkArea` | `Screens.Primary.Bounds / WorkingArea` (divide by `Scaling`) |
| `Brushes`, `Colors`, `SolidColorBrush`, `Color.FromArgb`, `Thickness`, `Point`, `Rect` | same names in `Avalonia` / `Avalonia.Media` |
| `ToolTip="x"` | `ToolTip.Tip="x"` |
| `Cursor="Hand"` | same |
| `FontFamily="Microsoft Sans Serif"` etc. | **keep the name as is**; `AppHost` maps Windows font names to metric-compatible Linux fonts. Fonts embedded in the app (`/X;component/Fonts/#Name`) → copy the ttf as AvaloniaResource and use `avares://<Asm>/Fonts#Name`. |
| `System.Windows.Forms.*` (NotifyIcon, Screen, Timer), WinForms Charting | replace with Avalonia equivalents (`TrayIcon`, `Screens`, `DispatcherTimer`); draw charts by hand with `Canvas`/`Polyline`/custom `Control.Render` so they look the same |
| WPF default text: Segoe UI 12px, black | `AppHost` sets the default family; if the original relies on defaults make sure `FontSize="12"` is effective (Avalonia default is 14 → set `FontSize="12"` on the Window) |
| WPF default `Button`/`CheckBox`/… look when no template is given | Avalonia's Fluent look is different. If the original shows an untemplated control, restyle it to the classic WPF (Aero2) look. |

Avalonia is stricter than WPF in a few places — let the compiler tell you and fix:
`Name=` vs `x:Name=` both fine; a `ControlTemplate` child lookup from code is
`e.NameScope.Find<T>("name")` in `OnApplyTemplate`/`TemplateApplied`; `FindName("x")` →
`this.FindControl<T>("x")`.

## Hardware and OS access
Never P/Invoke Windows DLLs. Replace each `[DllImport]`:

| original | use |
|---|---|
| `InsydeDCHU.dll` / `InsydeDCHU_x86.dll`: `SetDCHU_Data`, `SetDCHU_DataEx`, `GetDCHU_Data_Integer`, `GetDCHU_Data_Buffer`, `ReadAppSettings`, `WriteAppSettings` | `CCenter.Platform.InsydeDCHU.<same name>` — identical signatures. Keep the app's own `DCHU`/`InsydeDriver` wrapper class and just redirect its extern calls. **Do not change command numbers, sub-commands, byte layouts or call order.** |
| `kernel32` `GetPrivateProfileString` / `WritePrivateProfileString` | `CCenter.Platform.Ini.<same name>` (handles UTF-16 ini files and Windows-style paths) |
| `Microsoft.Win32.Registry` | `CCenter.Platform.Reg.GetValue/SetValue/GetInt/GetString(keyPath, name, …)` |
| Windows paths (`C:\ProgramData\…`, `Environment.SpecialFolder.*`, backslashes) | `CCenter.Platform.CcPaths.Map(path)`; app-relative data via `AppContext.BaseDirectory` |
| `user32` window messages between the CC apps (`FindWindow`, `SendMessage`, `RegisterWindowMessage`, `WndProc` hooks), named pipes, EventLog IPC to DCHUService | The Windows service does not exist. Where the app asked the service to do a hardware call, do that DCHU call directly. App-to-app notifications: implement with a small file-watcher or unix-socket helper inside your project if the feature needs it, otherwise make it a no-op and note it. |
| UWP `Launcher.LaunchUriAsync`, `Process.Start` of another CC app | `AppHost.LaunchSibling("<ProjectName>")`, which starts `../<ProjectName>/<ProjectName>` next to the running app. Project names: `ControlCenter30`, `FanSpeedSetting`, `LedKeyboardSetting`, `Flexikey`, `CPUOC`, `GPUOverclocking`, `EnergySave`, `BatteryUtility`, `FnKey`. |
| power plans (`powrprof`), brightness, Wi-Fi/Bluetooth/camera/touchpad toggles, audio, battery info, CPU/GPU usage & temps, RAM | Linux equivalents where there is a clean unprivileged one: `/sys/class/power_supply/BAT*`, `/sys/class/backlight/*`, `/proc/stat`, `/proc/meminfo`, `/sys/class/hwmon`, `/sys/class/thermal`, `nvidia-smi --query-gpu=… --format=csv,noheader,nounits` (read-only), `rfkill`, `powerprofilesctl`, `/sys/class/leds/rgb:kbd_backlight`. Put each in a small adapter class named after the DLL it replaces. If there is no sane equivalent, stub it with a plausible fixed value and list it in the app notes. |
| CPU/GPU overclock writes (voltages, ratios, power limits, GPU clock/voltage offsets, memory timings/XMP) | route **every** such write through `HardwareGate.GuardOcWrite(() => …)`. When it returns false the UI must behave exactly as normal but skip the hardware write. Reads are fine. Fan mode / fan curve / performance mode / keyboard LED are **not** OC writes. |

Apps must run without root and without the daemon: `Backend` then uses an in-process
simulator (`MockBackend`) seeded with values read from the reference laptop. Set
`CCENTER_MOCK_LOG=/path/file` to get a line per hardware call the UI makes — use it to check
that a click sends the same command the Windows code would.

UI development needs no root: do not load kernel modules, open `/dev/ccenter_dchu` or write to `/sys` from an app
under development. Keep NuGet dependencies to Avalonia-family packages.

## Checking your work
There are no Windows reference screenshots. The XAML is the spec: sizes, margins, colours, image names.
1. `dotnet build` with 0 errors.
2. `bin/Debug/net10.0/<App> --screenshot out.png [--after-ms 1500] [--page NAME]`
   renders the main window off-screen (no display needed) and writes a PNG. Implement `--page`
   (read `AppHost.PageArg` at startup) so every page / tab / dialog / notable state can be
   captured, and `--lang` if the app has languages.
3. Open each PNG and compare it with the XAML: window size, every
   element present, positions, images not missing (a missing image is silent), text not
   clipped, right colours, right initial state. Fix and repeat. Do this for every page.
4. Run it for real for 5 s to catch runtime exceptions:
   `timeout 5 bin/Debug/net10.0/<App>; echo $?` (124 means it was still running, which is what you want).
   Unhandled exceptions in timers/handlers must not be swallowed silently during development.
5. With `CCENTER_MOCK_LOG`, confirm start-up hardware calls match what the original would send.

The screenshot shown in the README for each app lives in `docs/screenshots/`; update it when the main window changes.

## Per-app notes
Each app has a notes file in [`apps/`](README.md#per-app-notes), kept up to date as you work:
- pages done / not done;
- every deviation from the original and why;
- every stub;
- the hardware commands the app sends (func, sub, data, when);
- anything that could not be verified without the real hardware;
- anything missing from `CCenter.Platform` that you worked around inside the app.

State plainly what is incomplete, stubbed or unverified.
