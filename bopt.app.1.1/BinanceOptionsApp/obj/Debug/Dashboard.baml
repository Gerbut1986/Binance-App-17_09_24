   M S B A M L   `   `   `  ����  BinanceOptionsApp!    BinanceOptionsApp.Dashboard   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -   5        .��5	      �   � RowDefinitions  5
   
   �� 5      $��2* �6   6*   �� 5      $��3* �6   6*   .+�5   
   �� 
�������   locProcessors#���   6&   $��5q�6O   $��0��6Z   
�������.��5      � 
�������.+�5      &   System.Windows.Controls.DataGrid     AutoGenerateColumns .     
IsReadOnly .    CanUserReorderColumns .     CanUserResizeRows .  
�������$��0q�5   J   ��6   �� *Tabs+   Columns 5      4  .System.Windows.Controls.DataGridTemplateColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$  ��61      CellTemplate 5      �� 5   "   .R�5   &   � 5   '   5   #   5        5      $  ��61    5      �� 5    "   .R�5!   &   � $
��Auto��6+   $��3,0,3,1q�69   .+�5"   *   ��  ��Name$ buStart��61   -   5        $��80��5"   B     locStart#���  6�   y�6c   �� *	Started+ 	 bToVi	 �� 	Converter#	 [ 6�   �� 5#   *   $ buStop��61   -   5        $��80��5#   A     locStop#���  6�   y�6a   �� *	Started+  bToV#	 [ 6�   5$   '   5%   #   5'        5(        locName# �  61    5)      �� 5*   "   .R�5+   &   �� ���#006dc5��60   �� *Title+6^   5,   #   50        51        
locFastBid# �  61    52      �� 53   "   .R�54   &   �� 
 �� TextAlignment$
 Center=�60   ���#002344��55   *   `� 56   .     	priceConv `� 	Converter# [ 6;   .1�57   2   ��  �� Path$
 Fast�6:   6H   �� 58   2   $ Fast.Bid�6:   6L   59   /   5;   '   5<   #   5?        5@        
locFastAsk# �  61    5A      �� 5B   "   .R�5C   &   �� $
 Center=�60   ���#002344��5D   *   `� 5E   .   # [ 6;   .1�5F   2   �� $
 Fast�6:   6H   �� 5G   2   $ Fast.Ask�6:   6L   5H   /   5J   '   5K   #   5N        5O       	 locFastTime# � 	 61    5P      �� 5Q   "   .R�5R   &   �� $
 Center=�60   ���#002344��5S   >   �� *	Fast.Time+ 

 dtToTS#	 [
 6~   5T   #   5V        5W        
locFastLot# �  61    5X      �� 5Y   "   .R�5Z   &   �� $
 Center=�60   ���#002344��5\   )   �� *Fast.Volume+  volConv#	 [ 6l   5]   #   5`        5a        
locSlowBid# �  61    5b      �� 5c   "   .R�5d   &   �� $
 Center=�60   ���#002344��5e   *   `� 5f   .   # [ 6;   .1�5g   2   �� $
 Slow�6:   6H   �� 5h   2   $ Slow.Bid�6:   6L   5i   /   5k   '   5l   #   5o        5p        
locSlowAsk# �  61    5q      �� 5r   "   .R�5s   &   �� $
 Center=�60   ���#002344��5t   *   `� 5u   .   # [ 6;   .1�5v   2   �� $
 Slow�6:   6H   �� 5w   2   $ Slow.Ask�6:   6L   5x   /   5z   '   5{   #   5}        5~        locSlowTime# �  61    5      �� 5�   "   5�   #   5�        5�        
locSlowLot# �  61    5�      �� 5�   "   5�   #   5�        5�        	locGapBuy# �  61    5�      �� 5�   "   5�   #   5�        5�        
locGapSell# �  61    5�      �� 5�   "   5�   #   5�        5�        locTickChart# �  61    5�      �� 5�   "   5�   #   5�        5�        locLastLogMessage# �  61    5�      �� 5�   "   .R�5�   &   �� ��60   �� *	LastLog+��5�   0   �� *LastLogBrush+6V   5�   #   5�      5�      5�      5�      �� 5�   
   
�������$��2��6&     locConnections#���  63   $��5q�6]   
���   �.��5�      � 
�������  5�      �� 5�      $��42 �6$   62   �� 5�      6&   .+�5�      ��  �� Orientation$ 
Horizontal=�6   $��Center=�66   $��5q�6Q   .+�5�      �� -   5         ��Name$ comboAddConnection��5�      $	��130��6;   6L   �� 5�      -   5        $ buAddConnection��5�      $��5,0,0,0q�66   $	��140��6G     locAddConnection#���  6t   6�   �� 5�      -   5        $ buRemoveConnection��5�      $��2,0,0,0q�69   $��80��6J     locRemoveConnection#���  6y   6�   �� 5�      -   5        $ buEditConnection��5�      $��5,0,0,0q�67   $��80��6H     locEditConnection#���  6u   6�   5�      �� 5�      $ 
Horizontal=�6   $��Center=�66   $��Right=�6Q   .+�5�      �� -   5         ��Name$ 	eaVersion��5�       $��Center=�63   
�������6c   5�        5�      -	   5          Name$ dgConnections��5�      $��1��5�       .   .  .   .     SelectionMode$ Single=�5�   M      SelectionUnit$ FullRow=�6d    5�      0  *System.Windows.Controls.DataGridTextColumn  5�      # �  6-   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6P   �� *Name+6k     5�        locType# �  6-    6P   �� *Type+6k     5�        locLogin# �  6-    6Q   �� *Login+6m     5�        
locAccount# �  6-    6S   �� *	Account+6q   5�      5�      5�      5�      5�      