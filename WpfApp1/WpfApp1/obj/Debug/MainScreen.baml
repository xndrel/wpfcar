   M S B A M L   `   `   `  ����B  >WpfApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:WpfApp1WpfApp1  5   	       WpfApp1.MainScreen   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:WpfApp1     5   	     � Title$  Главный экран��5   	   $	��600��6   $	��800��6,   .��5	      � .+�5   
   �� $��Photo/tyumen.jpg��6   $
��Fill=�6*   �� 5   
   -   5        $��MarkersCanvas��5      
������ .+�5      �� -   5        $��Car 1��5      $��60��6%   $��30��60    �� Left$	 200��6<   
 �� Top$	 150��6N   6_   5      �� 5   
   ���	#80000000$��50��6(   $	��Top=�64   ._�5      � .+�5      ��  �� Orientation$ 
Horizontal=�6   $��Center=�66   $
��Left=�6Q   $��15,0,0,0q�6l   .+�5      �� $.��(Фильтр по типу кузова:��6    
�������$��Center=�6Q   $��0,0,10,0q�6l   �� 5      -   5        $��BodyTypeComboBox��5      $	��120��67   $	��5,0q�6C   .@�5      �� $��Все��6'    �� 
IsSelected . 65   �� 5      $��
Седан��6'   �� 5      $��Внедорожник��6'   �� 5      $��Кабриолет��6'   5      5      �� 5"      $ 
Horizontal=�6   $��Center=�66   $��Right=�6Q   $��0,0,15,0q�6m   .+�5#      �� -   5         ��Name$ SupportChatButton��5#      $��Поддержка��68   $	��5,0q�5$      $
��10,5q�6*   ���	#FF007ACC
�������6P   �� 5%      -   5        $ ProfileButton��5%      $��Профиль��64   $	��5,0q�5&      $
��10,5q�6*   ���	#FF007ACC
�������6P   �� 5'      -   5        $ AdminPanelButton��5'      $��Админ-панель��67   $	��5,0q�5(      $
��10,5q�6*   ���	#FF007ACC
�������y�5)      �� *AdminVisibility+�� 5*      -   5        $ LogoutButton��5*      $��
Выйти��63   $	��5,0q�5+      $
��10,5q�6*   ���	#FFCC0000
�������6P   5,      5-      5.      5/      50      