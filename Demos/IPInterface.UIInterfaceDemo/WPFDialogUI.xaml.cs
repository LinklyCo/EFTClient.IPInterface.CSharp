using System.Threading.Tasks;
using System.Windows;

namespace PCEFTPOS.EFTClient.IPInterface.DialogUI
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class WPFDialogUI : Window
	{
		public WPFDialogUI()
		{
			DataContext = this;
			InitializeComponent();
		}

	}
}
