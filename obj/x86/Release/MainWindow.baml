   M S B A M L   `   `   `  ����   Styx    Styx.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Z VWPFToolkit.Extended, Version=1.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  :d2http://schemas.microsoft.com/expression/blend/2008  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  -   $��
mainWindow��  � Title$&   WSS Burst Detector - v. 1.0 Beta�� � SizeToContent$ Manual=�$
��Left=�$��Stretch=� � Icon$% /Styx;component/Icons/water.png�� � WindowStyle$ ThreeDBorderWindow=� � DataContext ��  �� RelativeSource �� *Self+ J� CommandBindings )  #System.Windows.Input.CommandBinding  -      Command$ Close��  -   $
 Open��  -   $
 Help��.��]� $
��Auto��$
��Auto����. .+��� $��Bottom=� �� BitmapScalingMode$ NearestNeighbor=����	#FF9CAAC1.@��� .��Coordinates�� -   $��statusBarCoordinateX���� -   $��statusBarCoordinateY���� �� .��Selected element�� -   $��statusBarSelectedItem���� -   $��	menuPanel��$	��Top=�	 �� Orientation$	 Vertical=����	#FFBFC9D9.+�o� -	   $��menu1��$
��Left=����� 
 �� ElementName$
 	menuPanel�� �� Path$ 
Background�.@�m� $
��File������ $
 	menuPanel��$ 
Background�.@�m�  m� Command$ ApplicationCommands.Open������ $
 	menuPanel��$ 
Background�m� $ ApplicationCommands.Close������ $
 	menuPanel��$ 
Background�m� $��Tools������ $
 	menuPanel��$ 
Background�.@�m� -
   $��optionsMenuItem��$��Options������ $
 	menuPanel��$ 
Background�m� $
��Help������ $
 	menuPanel��$ 
Background�.@�m� $
 Help������ $
 	menuPanel��$ 
Background�`�  `� 
Background �	#FF9CAAC1 J� 	AllowDrop .  .�c� $ NearestNeighbor=�$��32�����	#FF9CAAC1.@��� $��ApplicationCommands.Open�� � ToolTip$ Open inp file��.���� $&�� /Styx;component/Icons/folder.png���� {�.  .���� $+��%/Styx;component/Icons/floppy-disk.png���� -   $��printButton��$ Print��.���� $'��!/Styx;component/Icons/printer.png���� d� -   $��singleSelection��$ Select network element��.���� $+��%/Styx;component/Icons/select_node.png��d� -   $��multipleSelection��$&  Select multiple network elements��{�. .���� $2��,/Styx;component/Icons/multiple_selection.png���� �� -   $��zoomInButton��$ Zoom in��.���� $(��"/Styx;component/Icons/zoom_in2.png���� -   $��fullExtentButton��$ Full extent��.���� $&�� /Styx;component/Icons/extent.png���� -   $��zoomOutButton��$ Zoom out��.���� $)��#/Styx;component/Icons/zoom_out2.png��� -   $��
parentGrid�� � IsSharedSizeScope . .+�� -   $��layer0�� � UseLayoutRounding .  � ColumnDefinitions �� $��3* �$	��100���� $��* �$	��100���� $��* �$	��100��.+�� -   $��
canvasGrid��}�. .+��� -    ��Name$ chartBorder��
�������$��1q�$��5,5,0,5q�._��� -   $��scrollViewerCanvas1��}�. $
��Auto=�$��Stretch=���.  .���� -   $��canvas1��$��Center=�$��Center=�$
��Auto��$
��Auto��$��1��}�. 
�������$	��100��$	��100�� � ContextMenu �� -   $��canvas1ContextMenu�� �� PlacementTarget J� .@�m� -   $��showPathsContextMenu��$��
Show paths��{�.  m� -   $!��selectPathsNodesContextMenu��$��Select paths' nodes��{�.  m� $��Select element��.@��� -   $��selectItemComboBox��m� -   $��selectAllNodesContextMenu��$��Select all nodes��m� -   $��unselectAllContextMenu��$��Unselect all��� -   $��gridToolbox��$��1�����	#FF9CAAC1 � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* ��� $��2* �.+��� $��0��$��0��
����d�
�������$��Center=�.��	Toolbox�� -   $��button_LoggerLocTool��$��Logger Location Tool��$��1��$��26��$��Center=�$��7,5,5,5q�$	��Top=�$	��147��$. (Launch tool to plan placement of loggers��{�.     RoundedButton#��[  ���	#FF933737���	#FFC54743
�������$
��Bold"��� $��2��$��Labels�����	#FF293955.���� .+��� -   $��nodesLabelsVisibility��$��Node labels��$
��Left=�$��5q�$
��Auto��$��Stretch=�{�.  �� -    $��linksLabelsVisibility��$��Link labels��$
��Left=�$��5q�$��Stretch=�$
��Auto��$��2��{�.  �� -!   $��mainNetworkVisibility��$��Show water network��$
��Left=�$��5q�$��Stretch=�$
��Auto��$��2��{�.  �� $
��Auto��$��2��$��Bottom=�$��5��$��Stretch=� �� ResizeDirection$
 Rows=�1  +Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid  -"   $��propertyGridForElement��$��3��   AutoGenerateProperties . $��Stretch=����	#FF9CAAC1{�.  �� $��4��$��1��$
��Left=�� $��2��$��3�����	#FF9CAAC1 �� $
��Auto ��� $��3* ��� $
��0.9* ��� $��3* ��� $
��1.1* �.+��� $��0��$��0��
����+��
�������$��Center=�.��Burst detection�� $��1��$��0��$��E-Favor�����	#FF293955��Z� .���� .+��� -#   $��Button_loadEfavor��$��Load E-Favor Data��$��26��$��Center=�$��5q�$	��Top=�$	��120��$F @Load pressure stepping experiment (E-Favor) data from Excel file��{�.  #��[  ���	#FF933737���	#FFC54743
�������$
��Bold"��� -$   $��Button_suspectedLoggers��$��Suspected loggers��$��26��$��Center=�$��5q�$	��Top=�$	��120��$M GDisplays ranking or loggers by headloss change to define suspected area��{�.  #��[  ���	#FF933737���	#FFC54743
�������$
��Bold"��� -%   $��label1��$��Max number of bursts��$��28��$	��122���� -&   $��textBox_maxNBursts��$��23��$	��120�� �� TextAlignment$ Center=� �� 
IsReadOnly .  ���� *max_n_bursts+�� -'   $��button_LocalizeBurst��$��Localise Burst��$��26��$��Center=�$��5q�$	��Top=�$	��120��{�.  $z tPerform series of simulations with burst allocated to selected nodes and compared headloss changes with measurements��#��[  
����������	#FFC54743���	#FF933737$
��Bold"��� -(   $&�� checkbox_loggerNetworkVisibility��$��Show loggers��$��16��$��Center=�$��5q�$	��Top=�$��96��{�. �� $��2��$��0��$��Inlet set selection�����	#FF2A3A56.���� }�. $	��Top=�.+��� -)   $��inletSetSelectionComboBox��$��Stretch=�$��Stretch=�{�.   �� IsTextSearchEnabled . $2 ,Select set of inlets to use for calculations���� $��3��$��0��$��Burst flow estimation�����	#FF2A3A56.��� -*   $��burstFlowEstimationGrid��$	��Top=�  � $��4�� �� �� .+��� -+   $��estimateButton��$��0��$��ESTIMATE��$��Center=�$	��Top=�{�.  #��[  ���	#FF933737���	#FFC54743
�������$
��Bold"�$	��120��$��30���� $��3��$��2��$
��Left=�$��3�����	#FF293955