﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace XAlerts
{
    public partial class InputItemControl : UserControl
    {
        public InputItemControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
        }

        private string m_strInputLabel = "Enter Value:";

        public string InputLabel
        {
            get { return m_strInputLabel; }
            set { m_strInputLabel = value; }
        }
        private string m_strInputValue = "";

        public string InputValue
        {
            get { return m_strInputValue; }
            set { m_strInputValue = value; }
        }

        public void ShowAndGetItems()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.DataContext = null;
            this.DataContext = this;
            this.TextBoxInput.Focus();
            this.TextBoxInput.SelectAll();
        }

        public event EventHandler OnInputSaved = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            if (OnInputSaved != null)
                OnInputSaved(this, e);
        }
    }
}
