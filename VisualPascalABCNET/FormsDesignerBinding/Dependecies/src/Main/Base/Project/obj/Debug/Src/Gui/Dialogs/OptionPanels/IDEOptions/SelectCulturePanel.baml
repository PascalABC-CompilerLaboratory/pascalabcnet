   M S B A M L   `   `   `  ����S  OICSharpCode.SharpDevelop, Version=4.2.1.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:ICSharpCode.SharpDevelop.GuiICSharpCode.SharpDevelop.Gui  5      e7clr-namespace:ICSharpCode.SharpDevelop.Gui.OptionPanels)ICSharpCode.SharpDevelop.Gui.OptionPanels  5      B    <ICSharpCode.SharpDevelop.Gui.OptionPanels.SelectCulturePanel   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     X TICSharpCode.Core.Presentation, Version=4.2.1.0, Culture=neutral, PublicKeyToken=null1sd(http://icsharpcode.net/sharpdevelop/core  5      4gui*clr-namespace:ICSharpCode.SharpDevelop.Gui  5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Ilocal7clr-namespace:ICSharpCode.SharpDevelop.Gui.OptionPanels     5      -   5        .   (ICSharpCode.SharpDevelop.Gui.OptionPanel   Name$  window��5       � 	Resources 5       �� 
TargetType �� BasedOn �� Property �� Value � 	ItemWidth � 
ItemHeight K� IsItemsHost	 �� RelativeSource
 �� AncestorType � RowDefinitions �� DecodePixelWidth �� DecodePixelHeight �� 	UriSource �� TextAlignment �� TypeInTargetAssembly9   3ICSharpCode.SharpDevelop.Gui.OptionPanels.ImageView �� 
ResourceId��%�  '��       7��'�� �    7��(�� �    5.   
   "  $ 	ImageView��)(��     5/   
   "  $ ImageViewItem��)�� 5	      " ��6
   8   .�5
      ��  � ��
 �   �6#   �� 5       � ��$	 0.5q�6'   �� 5       � �� 5      �� 5      .R�5      �� $��bd��6   #��z��6   #��z��6E   #��z��6y   #��z��6�   ._�5   	   �� #��z��6   .��5   
   � $ 96��6   $ 64��6#    . $��50��6F   ��6T   �� *ActualWidth+	 �� $
 ScrollContentPresenterL�6�   5   
   5   	   5      5      5      �� 5      " ��6
   8   .�5      ��  � ��$ 3q�6   �� 5       � ��$ Center=�62   �� 5       � �� 5      �� 5      .R�5      � $��Center=�6   $	��Top=�6*    5   	   �� 5    
   $
��Auto �6   �� 5!   
   $
��Auto �6   .+�5#   	   �� $��Center=�6   $��32��6,   ��5$   
   �� 5%      $ 32��67   $ 24��6M    6   �� *	ImagePath+6M   5'   
   �� 5(   	   $��1��6   $��Center=�6    $ Center=�6=   $
��Wrap=�6T   ��6h   �� *Name+5)   	   5*      5,      5-      �� 5.      6
   �� 5/      6
   50      .��51      �  52      �� 53      $
��Auto �6   �� 54      $
��Auto �6   �� 55      $
��Auto �6   �� 56      $
��Auto �6   .+�58      �� -   5         ��Name$ listView��58      $	��5,0q�6   $	��200��6,    �� SelectedItem 69   1  +ICSharpCode.Core.Presentation.OptionBinding  **(local:SelectCulturePanel.CurrentLanguage+ �� View 59        5:      5<      �� 5=      $��1��6   $	��5,0q�6   ��5>      `� 5?       �� StringFormat$ {0} {1}��6   .1�5@      5  /ICSharpCode.Core.Presentation.LocalizeExtension  
   Key$D >Dialog.Options.IDEOptions.SelectCulture.CurrentUILanguageLabel��6   �� 5A       �� Path$ SelectedItem.Name�6    �� ElementName$ listView��6(   5B      5D      �� 5M      $��3��6   $
��Wrap=�6   $��5q�6|   ��6/     *97Dialog.Options.IDEOptions.SelectCulture.DescriptionText+6|   5N      5O      