   M S B A M L   `   `   `  ����  WpfApp1!    WpfApp1.RentalHistoryWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$!  История аренды��5   	   $	��450��6    $	��650��6-    � WindowStartupLocation$ CenterScreen=�5   	   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $��* �6   �� 5
      $
��Auto �6   .+�5   
   �� $��0��6   $8��2История аренды автомобилей��6!   $��18(�6C   $
��Bold"�6Q   $��0,0,0,10q�6c   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ RentalHistoryDataGrid��5       $��1��6      AutoGenerateColumns .     
IsReadOnly . $	��0,5q�5      $��1q�6   ���	#FFD0D0D0   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ Дата��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$	 100 6R   1  +System.Windows.Controls.DataGridBoundColumn	   Binding	 63   �� *
RentalDate+6R     5      $ Автомобиль��6%   $	 180 6U   	 69   �� *	CarInfo+6U     5      $ Гос. номер��6%   $	 100 6Z   	 69   �� *LicensePlate+6Z     5      $ Длительность��6%   $	 100 6X   	 6;   �� *
Duration+6X     5      $ Стоимость��6%   $	 120 6Q   	 68   �� *Cost+6Q   
   RowStyle
 5      �� 5       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   .�5      ��  �� Property � �� �� Value �	#FFF5F5F563   �� 5       � ��$	 0,2q�6/   5      5       �� 5#   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��0,10,0,0q�6W   .+�5$      �� -   5         ��Name$ CloseButton��5$      $��Закрыть��6*   $	��100��6<   $��30��6H   6T   5%      5&      5'      