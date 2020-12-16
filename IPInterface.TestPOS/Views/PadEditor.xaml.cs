using PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    /// <summary>
    /// Interaction logic for PadEditor.xaml
    /// </summary>
    public partial class PadEditor : Window
    {
        public PadViewModel ViewModel = null;

        public PadEditor(string filename, string title = "PAD Tag", bool dataButtonVisible = true)
        {
            ViewModel = new PadViewModel(filename);
            DataContext = ViewModel;

            Title = $"{title} Collection Editor";
            ViewModel.CollectionName = $"{title} Collection";
            ViewModel.DataButtonVisible = dataButtonVisible;

            InitializeComponent();
            txtMName.Focus();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
            DialogResult = true;
            Close();
        }

        private void lstPadContent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (lstPadContent == null)
                    return;

                if (lstPadContent.SelectedIndex < 0)
                {
                    btnMUp.IsEnabled = false;
                    btnMDown.IsEnabled = false;

                    btnMAdd.Content = "Add";
                    txtMName.Focus();
                    return;
                }

                btnMUp.IsEnabled = lstPadContent.SelectedIndex > 0;
                btnMDown.IsEnabled = lstPadContent.SelectedIndex < lstPadContent.Items.Count - 1;

                if (lstPadContent.SelectedItem is ExternalData selected)
                {
                    ViewModel.PadName = selected.Name;
                    ViewModel.PadValueDisplay = selected.Value;
                }

                btnMAdd.Content = "New";
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void lstPadEditor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (lstPadEditor == null)
                    return;

                if (lstPadEditor.SelectedIndex < 0)
                {
                    btnAdd.Content = "Add";
                    txtName.Focus();
                    return;
                }


                var pt = (PadTagViewModel)lstPadEditor.SelectedItem;
                ViewModel.PadTagName = pt?.Name;
                ViewModel.PadTagValue = pt?.Data;

                btnAdd.Content = "New";
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditMode = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnAdd.Content.Equals("New"))
                {
                    ViewModel.PadTagName = string.Empty;
                    ViewModel.PadTagValue = string.Empty;

                    btnAdd.Content = "Add";
                    lstPadEditor.SelectedIndex = -1;
                }
                else
                {
                    ViewModel.AddPadTagFunc();
                    lstPadEditor.SelectedIndex = -1;
                }

                txtName.Focus();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnMAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnMAdd.Content.Equals("New"))
                {
                    ViewModel.PadName = "[Name your PAD Tag collection]";
                    ViewModel.PadValueDisplay = "TAG010HelloWorld";
                    btnMAdd.Content = "Add";
                    lstPadContent.SelectedIndex = -1;
                }
                else
                {
                    ViewModel.AddPadContentFunc();
                    lstPadContent.SelectedIndex = -1;
                }

                txtMName.Focus();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnMUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.UpdatePadContentFunc(lstPadContent.SelectedIndex);
                lstPadContent.Items.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.UpdatePadTagFunc(lstPadEditor.SelectedIndex);
                lstPadEditor.Items.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.SavePadFieldFunc();
                lstPadContent.Items.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public static ExternalDataList GetData(string filename) => new PadEditor(filename).ViewModel.UpdatedExternalData;

        public static ExternalDataList AddData(string filename, ExternalData newData)
        {
            PadEditor padEditor = new PadEditor(filename);

            ExternalData found = padEditor.ViewModel.UpdatedExternalData.Find(x => x.Name.Equals(newData.Name));
            if (found != null)
                found.Value = newData.Value;
            else
                padEditor.ViewModel.PadContentList.Add(newData);

            padEditor.ViewModel.Save();

            return padEditor.ViewModel.UpdatedExternalData;
        }

        private void ReorderItem(bool moveDown)
        {
            int index = lstPadContent.SelectedIndex;
            int newIndex = -1;
            if (moveDown && (index < (ViewModel.PadContentList.Count - 1)))
                newIndex = index + 1;
            else if (!moveDown && index > 0)
                newIndex = index - 1;

            if (newIndex >= 0)
            {
                var item = ViewModel.PadContentList[index];
                ViewModel.PadContentList.Remove(item);
                ViewModel.PadContentList.Insert(newIndex, item);
                lstPadContent.SelectedIndex = newIndex;
            }
        }

        private void btnMUp_Click(object sender, RoutedEventArgs e)   => ReorderItem(false);

        private void btnMDown_Click(object sender, RoutedEventArgs e) => ReorderItem(true);
    }

}
