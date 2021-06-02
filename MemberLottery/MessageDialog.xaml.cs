using System.Windows;

namespace MemberLottery
{
    public partial class MessageDialog : Window
    {
        public MessageDialog()
        {
            InitializeComponent();

            okButton.Click += (sender, e) =>
            {
                DialogResult = true;
            };
        }

        public bool? ShowDialog(string message)
        {
            labelMessage.Content = message;

            return ShowDialog();
        }
    }
}
