﻿#pragma checksum "..\..\AdminPanel.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "BFFC588DF5E98A45C29B0D5034CFB6F3494458686A61F4C3B6332B75F1B8EA38"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using WpfApp1;
using WpfApp1.Converters;


namespace WpfApp1 {
    
    
    /// <summary>
    /// AdminPanel
    /// </summary>
    public partial class AdminPanel : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 26 "..\..\AdminPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView UsersListView;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\AdminPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView CarsListView;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\AdminPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView ChatsListView;
        
        #line default
        #line hidden
        
        
        #line 127 "..\..\AdminPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas AdminMapCanvas;
        
        #line default
        #line hidden
        
        
        #line 136 "..\..\AdminPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox AdminMapFilterComboBox;
        
        #line default
        #line hidden
        
        
        #line 162 "..\..\AdminPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox AnnouncementTitleTextBox;
        
        #line default
        #line hidden
        
        
        #line 165 "..\..\AdminPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox AnnouncementContentTextBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/WpfApp1;component/adminpanel.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\AdminPanel.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.UsersListView = ((System.Windows.Controls.ListView)(target));
            return;
            case 2:
            
            #line 48 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BlockUser_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 49 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.UnblockUser_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 50 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ShowRentalHistory_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 51 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.EditUser_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.CarsListView = ((System.Windows.Controls.ListView)(target));
            return;
            case 7:
            
            #line 86 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.AddCar_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 87 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.EditCar_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 88 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.DeleteCar_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 89 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.TestRental_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.ChatsListView = ((System.Windows.Controls.ListView)(target));
            return;
            case 12:
            
            #line 114 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OpenChat_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 115 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CloseChat_Click);
            
            #line default
            #line hidden
            return;
            case 14:
            this.AdminMapCanvas = ((System.Windows.Controls.Canvas)(target));
            return;
            case 15:
            this.AdminMapFilterComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 136 "..\..\AdminPanel.xaml"
            this.AdminMapFilterComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AdminMapFilterComboBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 142 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.RefreshMap_Click);
            
            #line default
            #line hidden
            return;
            case 17:
            this.AnnouncementTitleTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 18:
            this.AnnouncementContentTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 19:
            
            #line 168 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.PublishAnnouncement_Click);
            
            #line default
            #line hidden
            return;
            case 20:
            
            #line 176 "..\..\AdminPanel.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.LogoutButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

