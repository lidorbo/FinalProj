   M S B A M L   `   `   `  ���� I  ERafelFinalProj, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:RafelFinalProjRafelFinalProj      RafelFinalProj.MainScreen   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:RafelFinalProj       � Title$  WireShark Analyzer��$��689.135��$	��956��$��689.135��$	��800�� � Icon$ animals-shark-icon-2.png�� � 	Resources    brushWatermarkBackground �� Color  brushWatermarkForeground  brushWatermarkBorder   BooleanToVisibilityConverter3  -RafelFinalProj.TextInputToVisibilityConverter " TextInputToVisibilityConverter  EntryFieldStyle �� 
TargetType �� Property �� Value��%�   &	        &	      &	 +     &	 <     &	 A     &	 F     �� $ White���� $ LightSteelBlue���� $ Black����   �� " �.���  � ��$ Stretch=���  � ��$ Center=�.���  � RowDefinitions �� $��2* ��� $��3* ��� $��* � � ColumnDefinitions �� $��2* ��� $��* �.+��� $��
Load Files��$��0��$��10q�$��10 10q�$
��Auto��$��Stretch=�$	��Top=�.���� .+��  �� $��* ��� $��* ��� $��* ��� $��* � �� $��4* ��� $��* �.+�� $��0��#��[  #��[ .+��� $	��5,2q�$��Wireshark Log File��#��[ y��� 	 �� ElementName$	 wiresharkLogTB��
 �� Path$
 Text.IsEmpty� �� 	Converter# [ �� -   $��wiresharkLogTB��{�.  #��[   RoundedTextbox#���  �� -   $��wiresharkLogBrowseBTN��$��0��$��1��$
��5 10q�$��Browse��  RoundedButton#���  
�������$
��Bold"�� $��1��#��[  #��[ .+��� $	��5,2q�$��XML File��#��[ y��� $	 	xmlLoadTB��$
 Text.IsEmpty�# [ �� -   $��	xmlLoadTB��{�.  #��[ #���  �� -   $��
xmlLoadBTN��$��1��$��1��$
��5 10q�$��Browse��#���  
�������$
��Bold"�� $��2��#��[  #��[ .+��� $	��5,2q�$��Save ini File To��#��[ y��� $	 	iniSaveTB��$
 Text.IsEmpty�# [ �� -   $��	iniSaveTB��{�.  #��[ #���  �� -   $��
iniSaveBTN��$��2��$��1��$
��5 10q�$��Browse��#���  
�������$
��Bold"�� $��3��#��[  #��[ .+��� $	��5,2q�$��Save Log File To��#��[ y��� $	 	logSaveTB��$
 Text.IsEmpty�# [ �� -   $��	logSaveTB��{�.  #��[ #���  �� -   $��
logSaveBTN��$��3��$��1��$
��5 10q�$��Browse��#���  
�������$
��Bold"��� $��Filters��$��1��$��10q�$��10 10q�$
��Auto��$��Stretch=�$	��Top=�.���� .+��  � IsSharedSizeScope .  ��  z� SharedSizeGroup$ A���� $ A���� $ A���� $ A���� $ A���� $ A���� $ A�� �� $��* ��� $��* ��� $��* �.+��� $��0��$��0��$��Port Range:��$
��Left=�$
��Bold"�� $��0��$��1��$��5q�#��[  #��[ .+��� $	��5,2q�$��	Port From��#��[ y��� $	 
portFromTB��$
 Text.IsEmpty�# [ �� -	   $��
portFromTB��#��[ #���  � $��0��$��2��$��5q�#��[  #��[ .+��� $	��5,2q�$��Port To��#��[ y��� $	 portToTB��$
 Text.IsEmpty�# [ �� -
   $��portToTB��#��[ #���  �� $��1��$��0��$��Packet Size Range(Bytes):��$
��Left=�$
��Bold"�� $��1��$��1��$��5q�#��[  #��[ .+��� $	��5,2q�$��Packet Size From��#��[ y��� $	 packetSizeFromTB��$
 Text.IsEmpty�# [ �� -   $��packetSizeFromTB��#��[ #���  � $��1��$��2��$��5q�#��[  #��[ .+��� $	��5,2q�$��Packet Size To��#��[ y��� $	 packetSizeToTB��$
 Text.IsEmpty�# [ �� -   $��packetSizeToTB��#��[ #���  �� $��2��$��0��$��	IP Range:��$
��Left=�$
��Bold"�� $��2��$��1��$��5q�#��[  #��[ .+��� $	��5,2q�$��IP From��#��[ y��� $	 ipFromTB��$
 Text.IsEmpty�# [ �� -   $��ipFromTB��#��[ #���  � $��2��$��2��$��5q�#��[  #��[ .+��� $	��5,2q�$��IP To��#��[ y��� $	 ipToTB��$
 Text.IsEmpty�# [ �� -   $��ipToTB��#��[ #���  �� $��3��$��0��$��Offset:��$
��Left=�$
��Bold"�� $��3��$��1��$��5q�#��[  #��[ .+��� $	��5,2q�$��Offset��#��[ y��� $	 offsetTB��$
 Text.IsEmpty�# [ �� -   $��offsetTB��#��[ #���  � $��4��$��0��.+��� $	��3,5q�$��	Protocol:�� �� TextAlignment$
 Left=�$
��Bold"�� $��4��$��1��.+��� -    ��Name$ tcpRB��$	��TCP��$��Center=�$
��Left=� �� 	ClickMode$ Press=� d� 	IsChecked$
 TrueY�� $��4��$��2��.+��� -   $ udpRB��$	��UDP��$��Center=�$
��Left=�$ Press=�� $��5��$��0��.+��� $	��3,5q�$��Endian Format:��$
 Left=�$
��Bold"�� $��5��$��1��.+�	� -    	�Name$ littleRB�� 	� 	GroupName$ endian��$��Little Endian��$��Center=�$
��Left=�$ Press=�� $��5��$��2��.+�	� -   $ bigRB��$ endian��$��
Big Endian��$��Center=�$
��Left=�$ Press=�$
 TrueY��� $��2��.+�� $
��Auto�� �� $��* ��� $��* ��� $��* ��� $��* � �� $��3* �.+��� -   $��startScanBTN��$��1��$��	4,15,10,5q�$	��100q�$��
Start Scan��#���  
�������$
��Bold"��� -   $��stopScanBTN��$��2��$��	10,15,4,5q�$��	Stop Scan��#���  
�������$
��Bold"�� -   $��scanProgressBar��$��2��$
��40px��$��Center=�$��10 10q� � Minimum$ 0Q� � Maximum$	 100Q� � Value$ 0Q�$y�Hidden=��� -   $��progressValue��$��Center=�$	��Top=�$	��-38q�$y�Hidden=����� $	 scanProgressBar��$
 Value� �� StringFormat$ {0:0}%��� $��0��$��1�� �� $��* ��� $��3* �.+��� $��Notifications��$��0��$��Center=�$��Center=�$
��Bold"��� -   $��sysNotificationsLV��
�������
���   �$
��Left=�$��Stretch=�$��Stretch=�$��0��$	��440��$��1��$��2��$��5, 20q��� -   $��clearNotificationsBTN��$	��Top=�$��2��$��1��$��50 10q�$��Clear Notifications��#���  
�������$
��Bold"�