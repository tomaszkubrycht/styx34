   M S B A M L   `   `   `  ����  Styx    Styx.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Z VWPFToolkit.Extended, Version=1.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	   :d2http://schemas.microsoft.com/expression/blend/2008  5   C   Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  6�   -   5        $��
mainWindow��5   F    � Title$&   WSS Burst Detector - v. 1.0 Beta��6	    � SizeToContent$ Manual=�6�   $
��Left=�6  $��Stretch=�6*   � Icon$% /Styx;component/Icons/water.png��6X   � WindowStyle$ ThreeDBorderWindow=�6   � DataContext 6�  ��  �� RelativeSource �� *Self+ J� CommandBindings 5      )  #System.Windows.Input.CommandBinding  5   
   -   5           Command$ Close��5      6F     5   
   -   5        $
 Open��5      6D     5	   
   -   5        $
 Help��5	      6D   .��5      ]� $
��Auto��6   $
��Auto��6   ��. .+�5   
   �� $��Bottom=�6    �� BitmapScalingMode$ NearestNeighbor=�6,   ���	#FF9CAAC1.@�5      �� .��6   Coordinates6!   �� 5      -   5        $��statusBarCoordinateX��5      62   �� 5      -   5        $��statusBarCoordinateY��5      62   �� 5      6   �� 5      .��6   Selected element6&   �� 5      -   5        $��statusBarSelectedItem��5      63   5      �� 5   
   -   5        $��	menuPanel��5   X   $	��Top=�6   	 �� Orientation$	 Vertical=�6*   ���	#FFBFC9D9.+�5      o� -	   5        $��menu1��5      $
��Left=�6!   ��6<   �� 
 �� ElementName$
 	menuPanel�� �� Path$ 
Background�.@�5      m� $
��File��6   ��6)   �� $
 	menuPanel��$ 
Background�.@�5      m�  m� Command$ ApplicationCommands.Open��6   ��6B   �� $
 	menuPanel��$ 
Background�m� 5       $ ApplicationCommands.Close��6   ��6C   �� $
 	menuPanel��$ 
Background�5!      m� 5"      $��Tools��6   ��6*   �� $
 	menuPanel��$ 
Background�.@�5#      m� -
   5        $��optionsMenuItem��5#   n   $��Options��6   ��60   �� $
 	menuPanel��$ 
Background�6�   5$      m� 5%      $
��Help��6   ��6)   �� $
 	menuPanel��$ 
Background�.@�5&      m� $
 Help��6   ��6.   �� $
 	menuPanel��$ 
Background�5(      5)      `� 5*       `� 
Background �	#FF9CAAC1 J� 	AllowDrop .  .�5+      c� $ NearestNeighbor=�6   $��32��6L   ���	#FF9CAAC1.@�5,      �� $��ApplicationCommands.Open��6    � ToolTip$ Open inp file��6@   .��5-      �� $&�� /Styx;component/Icons/folder.png��6    5.      �� 5/      {�.  .��50      �� $+��%/Styx;component/Icons/floppy-disk.png��6    51      �� 52      -   5        $��printButton��52      $ Print��6J   .��53      �� $'��!/Styx;component/Icons/printer.png��6    54      �� 55      d� 59      -   5        $��singleSelection��59   #   $ Select network element��6:   .��5:      �� $+��%/Styx;component/Icons/select_node.png��6    5;      d� 5<      -   5        $��multipleSelection��5<   #   $&  Select multiple network elements��6<   {�. .��5=      �� $2��,/Styx;component/Icons/multiple_selection.png��6    5>      �� 5?      �� 5O      -   5        $��zoomInButton��5O      $ Zoom in��6N   .��5P      �� $(��"/Styx;component/Icons/zoom_in2.png��6    5Q      �� 5R      -   5        $��fullExtentButton��5R      $ Full extent��65   .��5S      �� $&�� /Styx;component/Icons/extent.png��6    5T      �� 5U      -   5        $��zoomOutButton��5U      $ Zoom out��6P   .��5V      �� $)��#/Styx;component/Icons/zoom_out2.png��6    5W      5X      5Y      5Z      � 5]   
   -   5        $��
parentGrid��5]       � IsSharedSizeScope . .+�5^      � -   5        $��layer0��5^       � UseLayoutRounding .  � ColumnDefinitions 5_      �� 5`      $��3* �6'   $	��100��62   �� 5a      $��* �6'   $	��100��61   �� 5b      $��* �6'   $	��100��61   .+�5d      � -   5        $��
canvasGrid��5d      }�. .+�5e      �� -   5         ��Name$ chartBorder��5e      
�������$��1q�6E   $��5,5,0,5q�6Y   ._�5f      �� -   5        $��scrollViewerCanvas1��5f   ~   }�. $
��Auto=�6;   $��Stretch=�6`   ��.  .��5g      �� -   5        $��canvas1��5g   B   $��Center=�6%   $��Center=�6Q   $
��Auto��6�   $
��Auto��6�   $��1��6�   }�. 
�������$	��100��6  $	��100��6#   � ContextMenu 5p   "   �� 5q   &   -   5        $��canvas1ContextMenu��5q   2    �� PlacementTarget 5r   *   J� 5s   .   .@�5u   *   m� -   5        $��showPathsContextMenu��5u   Y   $��
Show paths��63   {�.  6�   m� 5v   *   -   5        $!��selectPathsNodesContextMenu��5v   P   $��Select paths' nodes��63   {�.  6�   m� 5w   *   $��Select element��63   .@�5x   .   �� -   5        $��selectItemComboBox��5x   7   5z   /   5{   +   m� 5|   *   -   5        $��selectAllNodesContextMenu��5|   M   $��Select all nodes��63   6�   m� 5}   *   -   5        $��unselectAllContextMenu��5}   I   $��Unselect all��63   6�   5   '   5�      5�      5�      5�      � 5�      -   5        $��gridToolbox��5�   >   $��1��6   ���	#FF9CAAC1 � RowDefinitions 5�      �� 5�      $
��Auto �6(   �� 5�      $
��Auto �6(   �� 5�      $��* �6(   �� 5�      $��2* �6(   .+�5�      �� $��0��6   $��0��6*   
����d�
�������$��Center=�5�      .��6A   	Toolbox6J   �� 5�      -   5        $��button_LoggerLocTool��5�   �   $��Logger Location Tool��6   $��1��6<   $��26��6I   $��Center=�6U   $��7,5,5,5q�6r   $	��Top=�6�   $	��147��6�   $. (Launch tool to plan placement of loggers��6�   {�.     RoundedButton#��[  6+  ���	#FF933737���	#FFC54743
�������$
��Bold"�6�  �� 5�      $��2��6   $��Labels��6,   ���	#FF293955.��5�      �� .+�5�      �� -   5        $��nodesLabelsVisibility��5�   c   $��Node labels��6'   $
��Left=�6=   $��5q�6X   $
��Auto��6�   $��Stretch=�6�   {�.  6�   �� 5�      -    5        $��linksLabelsVisibility��5�   c   $��Link labels��6'   $
��Left=�6=   $��5q�6X   $��Stretch=�6�   $
��Auto��6�   $��2��6�   {�.  6
  �� 5�      -!   5        $��mainNetworkVisibility��5�   j   $��Show water network��6'   $
��Left=�6D   $��5q�6_   $��Stretch=�6�   $
��Auto��6�   $��2��6�   {�.  6  5�      5�      �� 5�      $
��Auto��6#   $��2��60   $��Bottom=�6=   $��5��6X   $��Stretch=�6c    �� ResizeDirection$
 Rows=�6�   1  +Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid  5�      -"   5        $��propertyGridForElement��5�   5   $��3��6(      AutoGenerateProperties . $��Stretch=�6q   ���	#FF9CAAC1{�.  5�      5�      �� 5�      $��4��6   $��1��6)   $
��Left=�69   � 5�      $��2��6   $��3��6'   ���	#FF9CAAC1 5�      �� 5�      $
��Auto �6(   �� 5�      $��3* �6(   �� 5�      $
��0.9* �6(   �� 5�      $��3* �6(   �� 5�      $
��1.1* �6(   .+�5�      �� $��0��6   $��0��6*   
����+��
�������$��Center=�5�      .��6A   Burst detection6R   �� 5�      $��1��6   $��0��6,   $��E-Favor��5�      ���	#FF293955��6H   Z� .��5�      �� .+�5�      �� -#   5        $��Button_loadEfavor��5�   u   $��Load E-Favor Data��6%   $��26��6A   $��Center=�6M   $��5q�6j   $	��Top=�6�   $	��120��6�   $F @Load pressure stepping experiment (E-Favor) data from Excel file��6�   {�.  #��[  6/  ���	#FF933737���	#FFC54743
�������$
��Bold"�6�  �� 5�      -$   5        $��Button_suspectedLoggers��5�   u   $��Suspected loggers��6%   $��26��6A   $��Center=�6M   $��5q�6j   $	��Top=�6�   $	��120��6�   $M GDisplays ranking or loggers by headloss change to define suspected area��6�   {�.  #��[  6  ���	#FF933737���	#FFC54743
�������$
��Bold"�6�  6�  �� 5�      -%   5        $��label1��5�   O   $��Max number of bursts��6$   $��28��6C   $	��122��6]   �� 5�      -&   5        $��textBox_maxNBursts��5�   2   $��23��6&   $	��120��6L    �� TextAlignment$ Center=�6v    �� 
IsReadOnly .  ��6X   �� *max_n_bursts+6�   �� 5�      -'   5        $��button_LocalizeBurst��5�   r   $��Localise Burst��6%   $��26��6>   $��Center=�6J   $��5q�6g   $	��Top=�6�   $	��120��6�   {�.  $z tPerform series of simulations with burst allocated to selected nodes and compared headloss changes with measurements��6�   #��[  6f  
����������	#FFC54743���	#FF933737$
��Bold"�6�  �� 5�      -(   5        $&�� checkbox_loggerNetworkVisibility��5�   r   $��Show loggers��6'   $��16��6>   $��Center=�6J   $��5q�6g   $	��Top=�6�   $��96��6�   {�. 6�   5�      5�      �� 5�      $��2��6   $��0��6,   $��Inlet set selection��5�      ���	#FF2A3A56.��5�      �� }�. $	��Top=�69   .+�5�      �� -)   5        $��inletSetSelectionComboBox��5�   C   $��Stretch=�6'   $��Stretch=�6d   {�.   �� IsTextSearchEnabled . $2 ,Select set of inlets to use for calculations��6�   5�      5�      5�      �� 5�      $��3��6    $��0��6-   $��Burst flow estimation��6=   ���	#FF2A3A56.��5�      � -*   5         �Name$ burstFlowEstimationGrid��5�      $
��Left=�6@   $	��138��6[   $��	10,10,0,0q�6h   $	��Top=�6{   $	��157��6�   5�      � 5�      $��4��6    5�      �� 5�      �� 5�      .+�5�      �� -+   5        $��estimateButton��5�   ^   $��0��6!   $��ESTIMATE��6.   $��Center=�6A   $	��Top=�6�   {�.  #��[  6�   ���	#FF933737���	#FFC54743
�������$
��Bold"�6$  $	��120��66  $��30��6B  5�      5�      �� 5�      -,   5        $��Button��5�      $
��Left=�6.   $��56,178.333,0,0q�6I   $��3��6a   $	��Top=�6n   $��75��6�   6�   5�      �� 5�      $��3��6   $��2��6)   $
��Left=�69   $��3��6T   ���	#FF2939556e   5�      5�      5�      5�      