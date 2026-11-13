# Hand-written Avalonia replacements for WPF templates whose part structure differs (Setter Template + extra styles).
OVR = {}

OVR['ListBoxStyle1_ExpressKey'] = '''
<Setter Property="Template">
  <ControlTemplate>
    <Border x:Name="Bd" BorderThickness="{TemplateBinding BorderThickness}" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}" Padding="1">
      <ScrollViewer x:Name="PART_ScrollViewer" Focusable="False" Padding="{TemplateBinding Padding}" Theme="{StaticResource ScrollViewerControlTemplate_ExperessKey}" BorderBrush="{TemplateBinding BorderBrush}" HorizontalScrollBarVisibility="{TemplateBinding (ScrollViewer.HorizontalScrollBarVisibility)}" VerticalScrollBarVisibility="{TemplateBinding (ScrollViewer.VerticalScrollBarVisibility)}">
        <ItemsPresenter x:Name="PART_ItemsPresenter" ItemsPanel="{TemplateBinding ItemsPanel}" Margin="0"/>
      </ScrollViewer>
    </Border>
  </ControlTemplate>
</Setter>
'''

OVR['ScrollViewerControlTemplate_ExperessKey'] = '''
<Setter Property="Template">
  <ControlTemplate>
    <Grid x:Name="Grid" Background="{TemplateBinding Background}" ColumnDefinitions="*,Auto" RowDefinitions="*,Auto">
      <Rectangle x:Name="Corner" Grid.Column="1" Fill="#FFF0F0F0" Grid.Row="1"/>
      <ScrollContentPresenter x:Name="PART_ContentPresenter" Grid.Column="0" Grid.Row="0" Margin="0" Padding="{TemplateBinding Padding}"
          HorizontalSnapPointsType="{TemplateBinding HorizontalSnapPointsType}" VerticalSnapPointsType="{TemplateBinding VerticalSnapPointsType}"/>
      <ScrollBar x:Name="PART_VerticalScrollBar" Theme="{StaticResource ScrollBarStyle_ExpressKey}" Orientation="Vertical" Grid.Column="1" Grid.Row="0" Cursor="Arrow" AllowAutoHide="False"/>
      <ScrollBar x:Name="PART_HorizontalScrollBar" Theme="{StaticResource ScrollBarStyle_ExpressKey}" Orientation="Horizontal" Grid.Column="0" Grid.Row="1" Cursor="Arrow" AllowAutoHide="False"/>
    </Grid>
  </ControlTemplate>
</Setter>
'''

_vert = '''<ControlTemplate>
  <Grid x:Name="Bg" RowDefinitions="Auto,*,Auto">
    <Border BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" Background="{TemplateBinding Background}" Grid.Row="1"/>
    <RepeatButton x:Name="PART_LineUpButton" Theme="{StaticResource RepeatButtonStyle_ExpressKey}" Background="{TemplateBinding BorderBrush}" BorderThickness="0" Height="17" Focusable="False">
      <Path x:Name="ArrowTop" Data="M0,4 C0,4 0,6 0,6 C0,6 3.5,2.5 3.5,2.5 C3.5,2.5 7,6 7,6 C7,6 7,4 7,4 C7,4 3.5,0.5 3.5,0.5 C3.5,0.5 0,4 0,4" Margin="0" Stretch="Uniform" Fill="#FF000000"/>
    </RepeatButton>
    <Track x:Name="PART_Track" Grid.Row="1" Minimum="{TemplateBinding Minimum}" Maximum="{TemplateBinding Maximum}" Value="{TemplateBinding Value, Mode=TwoWay}" ViewportSize="{TemplateBinding ViewportSize}" Orientation="{TemplateBinding Orientation}" IsDirectionReversed="False">
      <Track.DecreaseButton><RepeatButton x:Name="PART_PageUpButton" Theme="{StaticResource RepeatButtonTransparent}" Focusable="False"/></Track.DecreaseButton>
      <Track.IncreaseButton><RepeatButton x:Name="PART_PageDownButton" Theme="{StaticResource RepeatButtonTransparent}" Focusable="False"/></Track.IncreaseButton>
      <Thumb Theme="{StaticResource ThumbStyle_ExpressText}" BorderBrush="{TemplateBinding BorderBrush}"/>
    </Track>
    <RepeatButton x:Name="PART_LineDownButton" Grid.Row="2" Theme="{StaticResource RepeatButtonStyle_ExpressKey}" Background="{TemplateBinding BorderBrush}" Height="17" Focusable="False">
      <Path x:Name="ArrowBottom" Data="M0,2.5 C0,2.5 0,0.5 0,0.5 C0,0.5 3.5,4 3.5,4 C3.5,4 7,0.5 7,0.5 C7,0.5 7,2.5 7,2.5 C7,2.5 3.5,6 3.5,6 C3.5,6 0,2.5 0,2.5" Margin="0" Stretch="Uniform" Fill="#FF000000"/>
    </RepeatButton>
  </Grid>
</ControlTemplate>'''
_horz = '''<ControlTemplate>
  <Grid x:Name="Bg" ColumnDefinitions="Auto,*,Auto">
    <Border BorderBrush="{TemplateBinding BorderBrush}" BorderThickness="{TemplateBinding BorderThickness}" Background="{TemplateBinding Background}" Grid.Column="1"/>
    <RepeatButton x:Name="PART_LineLeftButton" Theme="{StaticResource RepeatButtonStyle_ExpressKey}" Background="{TemplateBinding BorderBrush}" Width="17" Focusable="False">
      <Path x:Name="ArrowLeft" Data="M3.18,7 C3.18,7 5,7 5,7 C5,7 1.81,3.5 1.81,3.5 C1.81,3.5 5,0 5,0 C5,0 3.18,0 3.18,0 C3.18,0 0,3.5 0,3.5 C0,3.5 3.18,7 3.18,7" Fill="#FF000000" Margin="0" Stretch="Uniform"/>
    </RepeatButton>
    <Track x:Name="PART_Track" Grid.Column="1" Minimum="{TemplateBinding Minimum}" Maximum="{TemplateBinding Maximum}" Value="{TemplateBinding Value, Mode=TwoWay}" ViewportSize="{TemplateBinding ViewportSize}" Orientation="{TemplateBinding Orientation}" IsDirectionReversed="False">
      <Track.DecreaseButton><RepeatButton x:Name="PART_PageLeftButton" Theme="{StaticResource RepeatButtonTransparent}" Focusable="False"/></Track.DecreaseButton>
      <Track.IncreaseButton><RepeatButton x:Name="PART_PageRightButton" Theme="{StaticResource RepeatButtonTransparent}" Focusable="False"/></Track.IncreaseButton>
      <Thumb Theme="{StaticResource ScrollBarThumbHorizontal}" BorderBrush="{TemplateBinding BorderBrush}"/>
    </Track>
    <RepeatButton x:Name="PART_LineRightButton" Grid.Column="2" Theme="{StaticResource RepeatButtonStyle_ExpressKey}" Background="{TemplateBinding BorderBrush}" Width="17" Focusable="False">
      <Path x:Name="ArrowRight" Data="M1.81,7 C1.81,7 0,7 0,7 C0,7 3.19,3.5 3.19,3.5 C3.19,3.5 0,0 0,0 C0,0 1.81,0 1.81,0 C1.81,0 5,3.5 5,3.5 C5,3.5 1.81,7 1.81,7" Fill="#FF000000" Margin="0" Stretch="Uniform"/>
    </RepeatButton>
  </Grid>
</ControlTemplate>'''
OVR['ScrollBarStyle_ExpressKey'] = '''
<Setter Property="Template">
  %s
</Setter>
<Style Selector="^[Orientation=Horizontal]">
  <Setter Property="Template">
    %s
  </Setter>
</Style>
''' % (_vert, _horz)

OVR['TextBoxStyle_ExpressText'] = '''
<Setter Property="Template">
  <ControlTemplate>
    <Border x:Name="border" BorderThickness="{TemplateBinding BorderThickness}" Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}">
      <ScrollViewer x:Name="PART_ScrollViewer" Focusable="False" HorizontalScrollBarVisibility="Hidden" VerticalScrollBarVisibility="Hidden">
        <TextPresenter x:Name="PART_TextPresenter" Text="{TemplateBinding Text, Mode=TwoWay}" CaretIndex="{TemplateBinding CaretIndex}" SelectionStart="{TemplateBinding SelectionStart}" SelectionEnd="{TemplateBinding SelectionEnd}" CaretBrush="{TemplateBinding CaretBrush}" SelectionBrush="{TemplateBinding SelectionBrush}" SelectionForegroundBrush="{TemplateBinding SelectionForegroundBrush}" TextAlignment="{TemplateBinding TextAlignment}" TextWrapping="{TemplateBinding TextWrapping}" LineHeight="{TemplateBinding LineHeight}" LetterSpacing="{TemplateBinding LetterSpacing}"/>
      </ScrollViewer>
    </Border>
  </ControlTemplate>
</Setter>
'''

LISTBOXITEM_STYLES = '''<Style Selector="ListBoxItem">
  <Setter Property="Padding" Value="0"/>
  <Setter Property="Margin" Value="0"/>
  <Setter Property="MinHeight" Value="0"/>
  <Setter Property="Background" Value="Transparent"/>
</Style>
<Style Selector="ListBoxItem:pointerover /template/ ContentPresenter#PART_ContentPresenter"><Setter Property="Background" Value="Transparent"/></Style>
<Style Selector="ListBoxItem:selected /template/ ContentPresenter#PART_ContentPresenter"><Setter Property="Background" Value="Transparent"/></Style>
<Style Selector="ListBoxItem:selected:pointerover /template/ ContentPresenter#PART_ContentPresenter"><Setter Property="Background" Value="Transparent"/></Style>
<Style Selector="ListBoxItem:pressed /template/ ContentPresenter#PART_ContentPresenter"><Setter Property="Background" Value="Transparent"/></Style>
'''
