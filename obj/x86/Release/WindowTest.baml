   M S B A M L   `   `   `  ����   Styx    Styx.WindowTest   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Z VWPFToolkit.Extended, Version=1.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  -   $��burstLocationWindow��  � Title$  Burst location��$	��690��$	��180�� � Icon$% /Styx;component/Icons/water.png��.��� -   $��mainGrid�����	#FF9CAAC1 � RowDefinitions �� $
��Auto ��� $��* �.+��� -   $��0��$��Print displayed results��$��Right=�$��Center=�   RoundedButton#��[  ���	#FFC54743���	#FF933737
�������$
��Bold"�$��26��$	��135���� $��1��$��2q�._�&   System.Windows.Controls.DataGrid  -   $��	dataGrid1��   AutoGenerateColumns .  $
��Auto��$��Stretch=�$��Stretch=�$
��Auto��$	��100��$	��200��   
IsReadOnly . ����  �� ElementName$ mainGrid�� �� Path$ 
Background�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$
 Node��1  +System.Windows.Controls.DataGridBoundColumn	   Binding	 �� $ Item1�  $ 	Fit index��	 �� $ Item2�
 �� StringFormat$
 f3��