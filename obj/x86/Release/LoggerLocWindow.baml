   M S B A M L   `   `   `  ����   Styx    Styx.LoggerLocWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Logger Location Tool��$	��542��$	��244��$��Normal"�$��13(� � Icon$% /Styx;component/Icons/water.png�� � DataContext ��  �� RelativeSource �� *Self+.��� 
����d� � RowDefinitions �� $
��237* ��� $
��247* �.+��� -   $��button_loadProhibited��$��Load prohibited locations��$��31��$
��Left=�$��	12,10,0,0q�$	��Top=�$	��199��$��13(� � ToolTip$R LLoad from Excel file locations (name of nodes) where loggers can't be placed��{�.    RoundedButton#��[  ���	#FF933737���	#FFC54743
�������$
��Bold"��� -   $��button_suggestLocation��$��Suggest placement��$��31��$
��Left=�$��	12,47,0,0q�$	��Top=�$	��199��$0 *Automatically suggest placement of loggers��$��13(�{�. #��[  $
��Bold"�����  �� ElementName$ button_loadProhibited�� �� Path$ 
Background����� $ button_loadProhibited��$ BorderBrush����� $ button_loadProhibited��$ 
Foreground��� -   $��button_calcLogConnections��$��Show logger connections��$��31��$
��Left=�$��	12,82,0,0q�$	��Top=�$	��199��$��13(�$r lCreate loggers at the specified nodes, calculate and show how loggers connect to each other in terms of flow��#��[  $
��Bold"�$��1������ $ button_loadProhibited��$ 
Background����� $ button_loadProhibited��$ BorderBrush����� $ button_loadProhibited��$ 
Foreground��� -   $��button_saveLogLocation��$��Save logger locations��$��31��$��
12,180,0,0q�$	��Top=�$��13(�$1 +Save current logger locations to Excel file��$
��Left=�$	��199��{�.  #��[  $
��Bold"�$��1������ $ button_loadProhibited��$ 
Background����� $ button_loadProhibited��$ BorderBrush����� $ button_loadProhibited��$ 
Foreground��� -   $��button_close��$ ��Close logger location tool��$��31��$
��Left=�$��
12,219,0,0q�$	��Top=�$	��199��$��13(�#��[  $
��Bold"�$��1������ $ button_loadProhibited��$ 
Background����� $ button_loadProhibited��$ BorderBrush����� $ button_loadProhibited��$ 
Foreground��� -   $��
textBlock1��$��32��$
��Left=�$��
12,154,0,0q�$!��Number of allocated loggers��$	��Top=�$
��Wrap=�$��97��$��12(��� -   $��
textBlock2��$��12(�$��32��$
��Left=�$��122,154,0,0q�$��Desired number of loggers��$
��Wrap=�$	��Top=�$��89���� -	   $��textBox_nAllocatedLog��$��23��$
��Left=�$��
12,189,0,0q�$	��Top=�$��90��$��13(� �� 
IsReadOnly . �� -
   $��textBox_nDesiredLog��$��23��$
��Left=�$��122,189,0,0q�$	��Top=�$��89��$��13(�	 �� TextAlignment$	 Right=����� *desiredNLoggers+�� -   $��textBox_nNeighbWeight��$��13(�$��23��$
��Left=�$��
12,120,0,0q�$	��Top=�$��62��$	 Right=�$9 3Importance of how many neighbours a given node has.������ *nNeighboursWeight+�� -   $��textBox_flowWeight��$��13(�$��23��$
��Left=�$��
81,120,0,0q�$	��Top=�$��62��$	 Right=�$6 0Importance of average flow through a given node.������ *
flowWeight+�� -   $��textBox_spreadWeight��$��13(�$��23��$
��Left=�$��149,120,0,0q�$	��Top=�$��62��$	 Right=�$= 7Importance of selected nodes being far from each other.������ *spreadWeight+�� -   $��
textBlock3��$��11(�$��32��$
��Left=�$��	12,89,0,0q�$��# neighbours weight��$
��Wrap=�$	��Top=�$��70��
 �� TextAlignment$
 Center=��� -   $��
textBlock4��$��11(�$��32��$
��Left=�$��	84,89,0,0q�$��flow weight��$
 Center=�$
��Wrap=�$	��Top=�$��55���� -   $��
textBlock5��$��11(�$��32��$
��Left=�$��
149,89,0,0q�$��spread weight��$
 Center=�$
��Wrap=�$	��Top=�$��55���� -   $��
textBlock6��$��12(�$��32��$
��Left=�$��	12,11,0,0q�$#��Head difference tolerance [m]��$
��Wrap=�$	��Top=�$��97��$��1���� -   $��
textBlock7��$��12(�$��32��$
��Left=�$��
122,11,0,0q�$��Logger neighbourhood��$
��Wrap=�$	��Top=�$��89��$��1���� -   $��textBox_headDiffTolerance��$��13(�$��23��$
��Left=�$��	12,48,0,0q�$	��Top=�$��90��$	 Right=�$��1��$~ xStop at a logger's neighbour if absolute head difference between the neighbour and the logger is smaller than this value������ *headDiffTolerance+�� -   $!��textBox_loggerNeighbourhood��$��13(�$��23��$
��Left=�$��
121,48,0,0q�$	 Right=�$	��Top=�$��89��$� �0=stop only at a logger itself, 1=stop at a direct neighbour of a logger, 2=stop at 2nd level neighbour (i.e. separated by 1 node) etc.��$��1������ *loggerNeighbourhoodLevel+�� -   $��
rectangle1��$��2��$
��Left=�
���   �$	��Top=�$	��222��$��1��$��0,5,0,0q�$��2��
���   ��� -   $��button_RemoveLoggers��$��Remove all loggers��$��13(�$
��Bold"�$��31��$
��Left=�{�. $��
12,142,0,0q�#��[  $, &Remove all currently allocated loggers��$	��Top=�$	��199��$��1������ $ button_loadProhibited��$ 
Background����� $ button_loadProhibited��$ BorderBrush����� $ button_loadProhibited��$ 
Foreground��� -   $#��checkBox_AllowLoggersAnywhere��$��Allow loggers anywhere��$��24��$
��Left=�$��
26,222,0,0q�$	��Top=�$	��170��