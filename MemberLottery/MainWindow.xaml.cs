using MemberLottery.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MemberLottery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BilibiliService bilibili = new();
        private readonly Random random = new();

        private delegate void LotteryEndHandler(string username);
        private event LotteryEndHandler OnLotteryEnd;
        private bool lotteryAvailable = true;

        public MainWindow()
        {
            InitializeComponent();

            textUid.Text = "698438232";

            OnLotteryEnd += s =>
            {
                lotteryAvailable = true;

                textLastUser.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textLastUser.Text = $"{textItem.Text}: {s}";
                    MessageBox.Show($"恭喜 {s} 抽中了 {textItem.Text}", "抽选结果");
                }));
            };

            buttonUserAdd.Click += (sender, e) =>
            {
                if (!lotteryAvailable) return;

                var input = new InputDialog("请输入用户名")
                {
                    Owner = this
                };

                if (input.ShowDialog() ?? false)
                {
                    listUsers.Items.Add(input.Result);
                }
                UpdateCount();
            };

            buttonUserRemove.Click += (sender, e) =>
            {
                if (!lotteryAvailable) return;

                if (listUsers.SelectedItems?.Count > 0)
                {
                    while (listUsers.SelectedIndex != -1)
                    {
                        listUsers.Items.RemoveAt(listUsers.SelectedIndex);
                    }
                }
                UpdateCount();
            };

            buttonUserClear.Click += (sender, e) =>
            {
                if (!lotteryAvailable) return;

                listUsers.Items.Clear();

                UpdateCount();
            };

            buttonUserUp.Click += (sender, e) =>
            {
                if (!lotteryAvailable) return;

                if (listUsers.SelectedItems?.Count > 0)
                {
                    var selectedIndex = listUsers.SelectedIndex;

                    if (selectedIndex > 0)
                    {
                        var item = listUsers.Items[selectedIndex];
                        listUsers.Items.RemoveAt(selectedIndex);
                        listUsers.Items.Insert(selectedIndex - 1, item);
                        listUsers.SelectedIndex = selectedIndex - 1;
                    }
                }
            };

            buttonUserDown.Click += (sender, e) =>
            {
                if (!lotteryAvailable) return;

                if (listUsers.SelectedItems?.Count > 0)
                {
                    var selectedIndex = listUsers.SelectedIndex;

                    if (selectedIndex != -1 && selectedIndex < listUsers.Items.Count - 1)
                    {
                        var item = listUsers.Items[selectedIndex];
                        listUsers.Items.RemoveAt(selectedIndex);
                        listUsers.Items.Insert(selectedIndex + 1, item);
                        listUsers.SelectedIndex = selectedIndex + 1;
                    }
                }
            };

            buttonUserPick.Click += (sender, e) =>
            {
                if (!lotteryAvailable) return;

                if (listUsers.Items.Count < 2)
                    return;

                int i = random.Next(0, listUsers.Items.Count);
                //底下是一些激动人心的小特效

                Task.Run(() =>
                {
                    lotteryAvailable = false;
                    
                    for (int k = 1; k <= 3; k++)
                    {
                        int lag = listUsers.Items.Count >= 30 * k ? 1 : (int)Math.Floor(30.0 * k / listUsers.Items.Count);
                        int gap = listUsers.Items.Count >= 30 * k ? (int)Math.Ceiling(listUsers.Items.Count / 30.0 / k) : 1;
                        //MessageBox.Show($"{lag}, {gap}");
                        for (int now = 0; now < Math.Min(k < 3 ? listUsers.Items.Count : i + 1, listUsers.Items.Count - gap); now += gap)
                        {
                            if (lotteryAvailable)
                            {
                                return;
                            }

                            listUsers.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                listUsers.SelectedItem = listUsers.Items.GetItemAt(now);
                                listUsers.ScrollIntoView(listUsers.SelectedItem);
                                listUsers.Focus();
                            }));

                            Thread.Sleep(lag);
                        }
                    }

                    listUsers.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        listUsers.SelectedItem = listUsers.Items.GetItemAt(i);
                        listUsers.ScrollIntoView(listUsers.SelectedItem);
                        listUsers.Focus();
                    }));

                    OnLotteryEnd?.Invoke(listUsers.Items.GetItemAt(i).ToString());
                });
            };

            buttonGetMember.Click += async (sender, e) =>
            {
                if (!lotteryAvailable) return;

                if (string.IsNullOrEmpty(textUid.Text))
                {
                    MessageBox.Show("请输入UID", "UID无效");
                }

                try
                {
                    listUsers.Items.Clear();
                    listUsers.Items.Add("正在查找用户...");

                    var info = await bilibili.GetUserInfoAsync(textUid.Text);

                    listUsers.Items.Clear();
                    listUsers.Items.Add($"正在拉取 {info?.Name} 的舰长...");

                    var memberList = await bilibili.GetAllMembersAsync(textUid.Text);

                    var selectedList = memberList.Select(m => $"{m.Username}");

                    listUsers.Items.Clear();
                    foreach (var i in selectedList)
                    {
                        listUsers.Items.Add(i);
                    }
                    UpdateCount();
                    MessageBox.Show($"共获得 {info?.Name} 的舰长 {selectedList.Count()} 个", "拉取成功");
                }
                catch
                {
                    listUsers.Items.Clear();
                    MessageBox.Show("未找到指定用户, 请检查UID", "UID无效");
                }
            };
        }


        private void UpdateCount()
        {
            labelCount.Content = $"({listUsers.Items.Count})";
        }
    }
}
