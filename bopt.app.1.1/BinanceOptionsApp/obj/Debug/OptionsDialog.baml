   M S B A M L   `   `   `  ����L  HBinanceOptionsApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:BinanceOptionsApp.ControlsBinanceOptionsApp.Controls  5   	   O,clr-namespace:BinanceOptionsApp.CustomStylesBinanceOptionsApp.CustomStyles  5	   	   G(clr-namespace:BinanceOptionsApp.ControlsBinanceOptionsApp.Controls  5   	   %    BinanceOptionsApp.OptionsDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   9ctrl(clr-namespace:BinanceOptionsApp.Controls     5   	   ;cs,clr-namespace:BinanceOptionsApp.CustomStyles     5	   	   =controls(clr-namespace:BinanceOptionsApp.Controls     5   	   -   5        $	��520��5   	   $	��460��6     � WindowStartupLocation$  CenterOwner=�6"    � 
ResizeMode$ NoResize=�5
   	   
�������$��14(�5   	   .��5      � 
������� � 	Resources 5   
    �� MergedDictionaries �� Source �� 
TargetType   EditorTextBox �� BasedOn  LightCheckBox/  )BinanceOptionsApp.Controls.IntegerTextBox  myGreenThinButton  StandardGroupBox�� 5       5      �� 5      $&  CustomStyles/GroupBoxStyles.xaml?�6)   6U   �� 5      $% CustomStyles/TextBoxStyles.xaml?�6)   6T   �� 5      $&  CustomStyles/CheckBoxStyles.xaml?�6)   6U   �� 5      $$ CustomStyles/ButtonStyles.xaml?�6)   6S   %�   '��       7   '�� "     7  '  D     7   '�� f     7  '�� �     7  �� 5      " ��6   8   6X   �� 5      " ��6   8   6Y   �� 5      "  6   8   6h   �� 5      " ��6   8   6[   �� 5      " ��6   8   6\   5       � RowDefinitions 5   
   �� 5       6   �� 5!      $��48 �6   6*   .+�5#   
   ��  �� Orientation$ Vertical=�6   $��5q�6,   .+�5$      ��   locSmtpServer#���  6   6A   �� 5%      ��6   �� *Smtp.Server+	 �� UpdateSourceTrigger$	 PropertyChanged=�
 �� Mode$
 TwoWay=�6e   �� 5&        locSmtpPort#���  6   6?     5'         Value 6"   �� *	Smtp.Port+$
 TwoWay=�$	 PropertyChanged=�6p   �� 5(        locSmtpSender#���  6   6A   �� 5)      ��6   �� *Smtp.Sender+$
 TwoWay=�$	 PropertyChanged=�6e   �� 5*        locSmtpPassword#���  6   6C   �� 5+      ��6   �� *Smtp.Password+$
 TwoWay=�$	 PropertyChanged=�6g   �� 5,      
�������$	��0,3q�6*    d� 	IsChecked 67   �� *
Smtp.SSL+$
 TwoWay=�$	 PropertyChanged=�.��6�   
SMTP SSL6�   �� 5-      $��0,10,0,0q�6     locSmtpRecipients#���  6*   6W   �� 5.      ��6   �� *Smtp.Recipients+$
 TwoWay=�$	 PropertyChanged=�6i   �� 5/      $��0,15,0,0q�6   
������� 6)   �� *Clicker.UseClickerForOpen+$
 TwoWay=�$	 PropertyChanged=�.��6�   Use Clicker For Open6�   �� 50      $��0,0,0,0q�6   
������� 6(   �� *Clicker.UseClickerForClose+$
 TwoWay=�$	 PropertyChanged=�.��6�   Use Clicker For Close6�   � 51       � ColumnDefinitions 52      �� 53      6)   �� 54      6)   �� 55      6)   .+�57      �� $	��Buy��6   
��� � �{�6;   �� *	Started+ 		 bToBi �� 	Converter# [	 .��58      �� $��20,10q�6!   .+�59      �� -   5         ��Name$ buSetBuyCoord��59   !   .��6T   Set6Y   �� 5:      -   5        $ buShowBuyCoord��5:   !   .��6V   Show6\   �� 5;      -   5        $ buClickBuyCoord��5;   !   .��6X   Click6_   5<      5=      �� 5>      $
��Sell��6   
���  ��$��1��6:   {�6J   �� *	Started+# [	 .��5?      �� $��20,10q�6!   .+�5@      �� -   5        $ buSetSellCoord��5@   !   .��6V   Set6[   �� 5A      -   5        $ buShowSellCoord��5A   !   .��6X   Show6^   �� 5B      -   5        $ buClickSellCoord��5B   !   .��6Z   Click6a   5C      5D      �� 5E      $��Close��6   
����ح�$��2��6A   {�6Q   �� *	Started+# [	 .��5F      �� $��20,10q�6!   .+�5G      �� -   5        $ buSetCloseCoord��5G   !   .��6X   Set6]   �� 5H      -	   5        $ buShowCloseCoord��5H   !   .��6Z   Show6`   �� 5I      -
   5        $ buClickCloseCoord��5I   !   .��6\   Click6c   5J      5K      5L      5N      �� 5O   
   $ 
Horizontal=�6   $��1��6.   $��Center=�6;   $��Center=�6V   .+�5P      �� -   5        $ buAccept��5P       
 myBlueButton#��[
 6'   $��80��6M    �� 	IsDefault .   locDlgAccept#���  6�   6�   �� 5Q      -   5        $ buCancel��5Q        myRedButton#��[ 6'   $��80��6L   $��5,0,0,0q�6Z    �� IsCancel .   locDlgCancel#���  6�   6�   5R      �� 5S   
   -   5        $ buTest��5S      $��1��6   #��[
 6.   $��80��6T     
locDlgTest#���  6w   $��Center=�6�   $
��Left=�6�   $��5q�6�   6�   5T      5U      