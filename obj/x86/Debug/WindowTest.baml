   M S B A M L   `   `   `  ����  Styx    Styx.WindowTest   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Z VWPFToolkit.Extended, Version=1.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   
   -   5        $��burstLocationWindow��5   9     � Title$  Burst location��6	   $	��690��6    $	��180��6-    � Icon$% /Styx;component/Icons/water.png��6T   .��5      � -   5        $��mainGrid��5   "   ���	#FF9CAAC1 � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $��* �6   .+�5   
   �� -   5        $��0��5      $��Print displayed results��6   $��Right=�6@   $��Center=�6\      RoundedButton#��[  6�   ���	#FFC54743���	#FF933737
�������$
��Bold"�6�   $��26��6  $	��135��6  6"  �� 5   
   $��1��6   $��2q�6   ._�5      &   System.Windows.Controls.DataGrid  -   5        $��	dataGrid1��5   _      AutoGenerateColumns .  $
��Auto��63   $��Stretch=�6A   $��Stretch=�6p   $
��Auto��6�   $	��100��6  $	��200��6     
IsReadOnly . ��6�   ��  �� ElementName$ mainGrid�� �� Path$ 
Background�   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$
 Node��6)   1  +System.Windows.Controls.DataGridBoundColumn	   Binding	 67   �� $ Item1�  5      $ 	Fit index��6)   	 6<   �� $ Item2�
 �� StringFormat$
 f3��5      5      5      5      