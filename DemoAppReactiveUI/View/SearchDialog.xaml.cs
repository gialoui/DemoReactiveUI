using DemoAppReactiveUI.Model;
using System;
using System.Windows;

namespace DemoAppReactiveUI.View
{
    /// <summary>
    /// Interaction logic for SearchDialog.xaml
    /// </summary>
    public partial class SearchDialog : Window
    {
        public Product SelectedProduct;

        public SearchDialog()
        {
            InitializeComponent();

            SearchControl.ViewModel.ExecuteSelectProduct.Subscribe(_ =>
            {
                SelectedProduct = SearchControl.ViewModel.SelectedProduct;

                // Close the current dialog
                DialogResult = true;
            });
        }
    }
}