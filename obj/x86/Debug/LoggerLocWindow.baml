   M S B A M L   `   `   `  ����  Styx    Styx.LoggerLocWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Logger Location Tool��5   	   $	��542��6&   $	��244��63   $��Normal"�6?   $��13(�6S    � Icon$% /Styx;component/Icons/water.png��6z    � DataContext 6�   ��  �� RelativeSource �� *Self+.��5      � 
����d� � RowDefinitions 5   
   �� 5      $
��237* �6   �� 5      $
��247* �6   .+�5
   
   �� -   5        $��button_loadProhibited��5
   o   $��Load prohibited locations��6   $��31��65   $
��Left=�6A   $��	12,10,0,0q�6\   $	��Top=�6�   $	��199��6�   $��13(�6�    � ToolTip$R LLoad from Excel file locations (name of nodes) where loggers can't be placed��6�   {�.    RoundedButton#��[  6&  ���	#FF933737���	#FFC54743
�������$
��Bold"�6�  6�  �� 5   
   -   5        $��button_suggestLocation��5   g   $��Suggest placement��6   $��31��6-   $
��Left=�69   $��	12,47,0,0q�6T   $	��Top=�6�   $	��199��6�   $0 *Automatically suggest placement of loggers��6�   $��13(�6�   {�. #��[  6�   $
��Bold"�6  ��6$  ��  �� ElementName$ button_loadProhibited�� �� Path$ 
Background���6n  �� $ button_loadProhibited��$ BorderBrush���6�  �� $ button_loadProhibited��$ 
Foreground�6  �� 5   
   -   5        $��button_calcLogConnections��5   m   $��Show logger connections��6   $��31��63   $
��Left=�6?   $��	12,82,0,0q�6Z   $	��Top=�6�   $	��199��6�   $��13(�6�   $r lCreate loggers at the specified nodes, calculate and show how loggers connect to each other in terms of flow��6�   #��[  6_  $
��Bold"�6f  $��1��6x  ��6�  �� $ button_loadProhibited��$ 
Background���6�  �� $ button_loadProhibited��$ BorderBrush���6  �� $ button_loadProhibited��$ 
Foreground�6x  �� 5   
   -   5        $��button_saveLogLocation��5   Q   $��Save logger locations��6   $��31��61   $��
12,180,0,0q�6=   $	��Top=�6o   $��13(�6�   $1 +Save current logger locations to Excel file��6�   $
��Left=�6�   $	��199��6�   {�.  #��[  6)  $
��Bold"�60  $��1��6B  ��6P  �� $ button_loadProhibited��$ 
Background���6�  �� $ button_loadProhibited��$ BorderBrush���6�  �� $ button_loadProhibited��$ 
Foreground�6B  �� 5   
   -   5        $��button_close��5   q   $ ��Close logger location tool��6   $��31��66   $
��Left=�6B   $��
12,219,0,0q�6]   $	��Top=�6�   $	��199��6�   $��13(�6�   #��[  6�   $
��Bold"�6�  $��1��6�  ��6�   �� $ button_loadProhibited��$ 
Background���6C  �� $ button_loadProhibited��$ BorderBrush���6�  �� $ button_loadProhibited��$ 
Foreground�6�  �� 5   
   -   5        $��
textBlock1��5   O   $��32��6   $
��Left=�6    $��
12,154,0,0q�6;   $!��Number of allocated loggers��6a   $	��Top=�6�   $
��Wrap=�6�   $��97��6�   $��12(�6�   �� 5   
   -   5        $��
textBlock2��5   ^   $��12(�6   $��32��6"   $
��Left=�6.   $��122,154,0,0q�6I   $��Desired number of loggers��6p   $
��Wrap=�6�   $	��Top=�6�   $��89��6�   �� 5   
   -	   5        $��textBox_nAllocatedLog��5   M   $��23��6   $
��Left=�6   $��
12,189,0,0q�69   $	��Top=�6j   $��90��6�   $��13(�6�    �� 
IsReadOnly . 6�   �� 5   
   -
   5        $��textBox_nDesiredLog��5   N   $��23��6   $
��Left=�6   $��122,189,0,0q�69   $	��Top=�6i   $��89��6�   $��13(�6�   	 �� TextAlignment$	 Right=�6�   ��6�   �� *desiredNLoggers+�� 5   
   -   5        $��textBox_nNeighbWeight��5   [   $��13(�6   $��23��6    $
��Left=�6,   $��
12,120,0,0q�6G   $	��Top=�6x   $��62��6�   $	 Right=�6�   $9 3Importance of how many neighbours a given node has.��6�   ��6�   �� *nNeighboursWeight+6�   �� 5   
   -   5        $��textBox_flowWeight��5   [   $��13(�6   $��23��6    $
��Left=�6,   $��
81,120,0,0q�6G   $	��Top=�6u   $��62��6�   $	 Right=�6�   $6 0Importance of average flow through a given node.��6�   ��6�   �� *
flowWeight+6�   �� 5   
   -   5        $��textBox_spreadWeight��5   \   $��13(�6   $��23��6    $
��Left=�6,   $��149,120,0,0q�6G   $	��Top=�6x   $��62��6�   $	 Right=�6�   $= 7Importance of selected nodes being far from each other.��6�   ��6�   �� *spreadWeight+6�   �� 5   
   -   5        $��
textBlock3��5   \   $��11(�6   $��32��6"   $
��Left=�6.   $��	12,89,0,0q�6I   $��# neighbours weight��6n   $
��Wrap=�6�   $	��Top=�6�   $��70��6�   
 �� TextAlignment$
 Center=�6�   �� 5   
   -   5        $��
textBlock4��5   \   $��11(�6   $��32��6"   $
��Left=�6.   $��	84,89,0,0q�6I   $��flow weight��6n   $
 Center=�6�   $
��Wrap=�6�   $	��Top=�6�   $��55��6�   �� 5   
   -   5        $��
textBlock5��5   ]   $��11(�6   $��32��6"   $
��Left=�6.   $��
149,89,0,0q�6I   $��spread weight��6o   $
 Center=�6�   $
��Wrap=�6�   $	��Top=�6�   $��55��6�   �� 5   
   -   5        $��
textBlock6��5   \   $��12(�6   $��32��6"   $
��Left=�6.   $��	12,11,0,0q�6I   $#��Head difference tolerance [m]��6n   $
��Wrap=�6�   $	��Top=�6�   $��97��6�   $��1��6�   �� 5   
   -   5        $��
textBlock7��5   ]   $��12(�6   $��32��6"   $
��Left=�6.   $��
122,11,0,0q�6I   $��Logger neighbourhood��6o   $
��Wrap=�6�   $	��Top=�6�   $��89��6�   $��1��6�   �� 5   
   -   5        $��textBox_headDiffTolerance��5   Z   $��13(�6   $��23��6    $
��Left=�6,   $��	12,48,0,0q�6G   $	��Top=�6�   $��90��6�   $	 Right=�6�   $��1��6�   $~ xStop at a logger's neighbour if absolute head difference between the neighbour and the logger is smaller than this value��6�   ��6{   �� *headDiffTolerance+6�   �� 5   
   -   5        $!��textBox_loggerNeighbourhood��5   [   $��13(�6   $��23��6    $
��Left=�6,   $��
121,48,0,0q�6G   $	 Right=�6�   $	��Top=�6�   $��89��6�   $� �0=stop only at a logger itself, 1=stop at a direct neighbour of a logger, 2=stop at 2nd level neighbour (i.e. separated by 1 node) etc.��6�   $��1��6s  ��6~   �� *loggerNeighbourhoodLevel+6s  �� 5   
   -   5        $��
rectangle1��5   :   $��2��6   $
��Left=�6   
���   �$	��Top=�6[   $	��222��6s   $��1��6   $��0,5,0,0q�6�   $��2��6�   
���   �6�   �� 5   
   -   5        $��button_RemoveLoggers��5   z  $��Remove all loggers��6�   $��13(�6�   $
��Bold"�6�   $��31��6.  $
��Left=�6:  {�. $��
12,142,0,0q�6f  #��[  6�  $, &Remove all currently allocated loggers��6�  $	��Top=�6�  $	��199��6  $��1��6  ��6   �� $ button_loadProhibited��$ 
Background���6[   �� $ button_loadProhibited��$ BorderBrush���6�   �� $ button_loadProhibited��$ 
Foreground�6  �� 5   
   -   5        $#��checkBox_AllowLoggersAnywhere��5   o   $��Allow loggers anywhere��6   $��24��64   $
��Left=�6@   $��
26,222,0,0q�6[   $	��Top=�6�   $	��170��6�   5       5!      