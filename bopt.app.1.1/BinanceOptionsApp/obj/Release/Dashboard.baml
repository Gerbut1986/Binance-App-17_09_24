   M S B A M L   `   `   `  ����   WesternpipsPrivate7!    BinanceOptionsApp.Dashboard   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -   .���   � RowDefinitions  �� $��2* ��� $��3* �.+��� 
�������   locProcessors#���   $��5q�$��0��
�������.��� 
�������.+�&   System.Windows.Controls.DataGrid     AutoGenerateColumns .     
IsReadOnly .    CanUserReorderColumns .     CanUserResizeRows .  
�������$��0q����� *Tabs+   Columns 4  .System.Windows.Controls.DataGridTemplateColumn  ,  &System.Windows.Controls.DataGridColumn   Header$  ��   CellTemplate �� .R��   $  �� �� .R�� $
��Auto��$��3,0,3,1q�.+���  ��Name$ buStart��-   $��80��  locStart#���  y��� *	Started+ 	 bToVi	 �� 	Converter#	 [ �� $ buStop��-   $��80��  locStop#���  y��� *	Started+  bToV#	 [     locName# �   �� .R��� ���#006dc5���� *Title+    
locFastBid# �   �� .R��� 
 �� TextAlignment$
 Center=����#002344��`�   	priceConv `� 	Converter# [ .1���  �� Path$
 Fast��� $ Fast.Bid�    
locFastAsk# �   �� .R��� $
 Center=����#002344��`� # [ .1��� $
 Fast��� $ Fast.Ask�   	 locFastTime# � 	  �� .R��� $
 Center=����#002344���� *	Fast.Time+ 

 dtToTS#	 [
     
locFastLot# �   �� .R��� $
 Center=����#002344���� *Fast.Volume+  volConv#	 [     
locSlowBid# �   �� .R��� $
 Center=����#002344��`� # [ .1��� $
 Slow��� $ Slow.Bid�    
locSlowAsk# �   �� .R��� $
 Center=����#002344��`� # [ .1��� $
 Slow��� $ Slow.Ask�    locSlowTime# �   ��     
locSlowLot# �   ��     	locGapBuy# �   ��     
locGapSell# �   ��     locTickChart# �   ��     locLastLogMessage# �   �� .R��� ���� *	LastLog+���� *LastLogBrush+�� 
�������$��2��  locConnections#���  $��5q�
���   �.��� 
�������  �� $��42 ��� .+���  �� Orientation$ 
Horizontal=�$��Center=�$��5q�.+��� -    ��Name$ comboAddConnection��$	��130���� -   $ buAddConnection��$��5,0,0,0q�$	��140��  locAddConnection#���  �� -   $ buRemoveConnection��$��2,0,0,0q�$��80��  locRemoveConnection#���  �� -   $ buEditConnection��$��5,0,0,0q�$��80��  locEditConnection#���  �� $ 
Horizontal=�$��Center=�$��Right=�.+��� -    ��Name$ 	eaVersion��$��Center=�
�������  -	     Name$ dgConnections��$��1�� .   .  .   .     SelectionMode$ Single=�   SelectionUnit$ FullRow=� 0  *System.Windows.Controls.DataGridTextColumn  # �  1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Name+    locType# �   �� *Type+    locLogin# �   �� *Login+    
locAccount# �   �� *	Account+