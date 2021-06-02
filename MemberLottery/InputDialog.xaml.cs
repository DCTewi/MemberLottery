using System.Windows;

namespace MemberLottery
{
    public partial class InputDialog : Window
    {
        public InputDialog(string label)
        {
            InitializeComponent();

            labelInput.Content = label;

            okButton.Click += (sender, e) =>
            {
                DialogResult = true;
            };
        }

        public string Result
        {
            get => textInput.Text;
            set => textInput.Text = value;
        }
    }
}
