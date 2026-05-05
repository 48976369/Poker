using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Poker
{
    public partial class frmPoker : Form
    {
        PictureBox[] pic = new PictureBox[5]; 
        int[] allPoker = new int[52];         
        int[] playerPoker = new int[5];       
        int cardIndex = 5;                   
        int totalMoney = 1000000;             
        int currentBet = 0;                   

        public frmPoker()
        {
            InitializeComponent();
            InitializePoker();
        }

        private void InitializePoker()
        {
            for (int i = 0; i < 5; i++)
            {
                pic[i] = new PictureBox();
                pic[i].Image = Properties.Resources.ResourceManager.GetObject("back") as Image;
                pic[i].Name = "pic" + i;
                pic[i].SizeMode = PictureBoxSizeMode.AutoSize;
                pic[i].Top = 30;
                pic[i].Left = 10 + ((85 + 10) * i); // 假設寬度85
                pic[i].Visible = true;
                pic[i].Enabled = false;
                pic[i].Tag = "back";
                pic[i].MouseClick += new MouseEventHandler(pic_Click);
                this.grpPoker.Controls.Add(pic[i]);
            }
        }

        // 讀取圖片的副程式
        private Image GetImage(string name)
        {
            return Properties.Resources.ResourceManager.GetObject(name) as Image;
        }

        // 1. 押注按鈕事件
        private void btnBet_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtBetMoney.Text, out currentBet) && currentBet > 0 && currentBet <= totalMoney)
            {
                totalMoney -= currentBet; // 扣除押注金
                txtTotalMoney.Text = totalMoney.ToString();

                btnBet.Enabled = false;      // 鎖定押注
                txtBetMoney.Enabled = false; // 鎖定金額輸入
                btnDealCard.Enabled = true;  // 開放發牌
                lblResult.Text = "請點擊發牌";
            }
            else
            {
                MessageBox.Show("請輸入有效的押注金額！(不能超過總資金)");
            }
        }

        // 洗牌副程式
        private void Shuffle()
        {
            Random rand = new Random();
            for (int i = 0; i < allPoker.Length; i++)
            {
                int r = rand.Next(allPoker.Length);
                int temp = allPoker[r];
                allPoker[r] = allPoker[0];
                allPoker[0] = temp;
            }
        }

        // 2. 發牌按鈕事件
        private async void btnDealCard_Click(object sender, EventArgs e)
        {
            btnDealCard.Enabled = false; // 避免連按

            // 初始化52張牌
            for (int i = 0; i < 52; i++) allPoker[i] = i;
            Shuffle();

            // 蓋牌效果
            for (int i = 0; i < 5; i++) pic[i].Image = GetImage("back");
            await Task.Delay(500); // 暫停0.5秒

            // 發前5張牌
            for (int i = 0; i < 5; i++)
            {
                playerPoker[i] = allPoker[i];
                pic[i].Image = GetImage("pic" + (playerPoker[i] + 1));
                pic[i].Enabled = true;
                pic[i].Tag = "front";
            }

            cardIndex = 5; // 重置發牌索引
            btnChangeCard.Enabled = true;
            btnCheck.Enabled = true;
            lblResult.Text = "點擊牌面可選擇換牌，或直接判斷牌型";
        }

        // 點擊撲克牌 (選擇是否換牌)
        private void pic_Click(object sender, MouseEventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            int index = int.Parse(p.Name.Replace("pic", ""));

            if (p.Tag.ToString() == "front")
            {
                p.Tag = "back";
                p.Image = GetImage("back"); // 標記為要換掉
            }
            else
            {
                p.Tag = "front";
                p.Image = GetImage("pic" + (playerPoker[index] + 1)); // 取消換牌
            }
        }

        // 3. 換牌按鈕事件
        private void btnChangeCard_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                if (pic[i].Tag.ToString() == "back") // 把蓋起來的牌換掉
                {
                    playerPoker[i] = allPoker[cardIndex];
                    pic[i].Image = GetImage("pic" + (playerPoker[i] + 1));
                    pic[i].Tag = "front";
                    cardIndex++;
                }
                pic[i].Enabled = false; // 換完不能再點
            }
            btnChangeCard.Enabled = false; // 只能換一次
            lblResult.Text = "請點擊判斷牌型";
        }

        // 4. 判斷牌型與計算賠率事件
        private void btnCheck_Click(object sender, EventArgs e)
        {
            // 將牌面轉為花色與點數
            int[] pokerColor = new int[5];
            int[] pokerPoint = new int[5];
            for (int i = 0; i < 5; i++)
            {
                pokerColor[i] = playerPoker[i] % 4;
                pokerPoint[i] = playerPoker[i] / 4;
            }

            // 統計出現次數
            int[] colorCount = new int[4];
            int[] pointCount = new int[13];
            for (int i = 0; i < 5; i++)
            {
                colorCount[pokerColor[i]]++;
                pointCount[pokerPoint[i]]++;
            }

            // 排序找最大特徵 (為了判斷牌型方便)
            Array.Sort(colorCount); Array.Reverse(colorCount);
            Array.Sort(pointCount); Array.Reverse(pointCount);

            // 判斷邏輯
            bool isFlush = (colorCount[0] == 5);
            bool isSingle = (pointCount[0] == 1 && pointCount[4] == 1);
            bool isDiffFour = (pokerPoint.Max() - pokerPoint.Min() == 4);
            bool isRoyal = pokerPoint.Contains(0) && pokerPoint.Contains(9) && pokerPoint.Contains(10) && pokerPoint.Contains(11) && pokerPoint.Contains(12); // A, 10, J, Q, K

            bool isRoyalFlush = isFlush && isRoyal;
            bool isStraightFlush = isFlush && isSingle && isDiffFour;
            bool isFourOfAKind = (pointCount[0] == 4);
            bool isFullHouse = (pointCount[0] == 3 && pointCount[1] == 2);
            bool isStraight = isSingle && (isDiffFour || isRoyal);
            bool isThreeOfAKind = (pointCount[0] == 3 && pointCount[1] == 1);
            bool isTwoPair = (pointCount[0] == 2 && pointCount[1] == 2);
            bool isOnePair = (pointCount[0] == 2 && pointCount[1] == 1);

            // 計算賠率與結果
            int multiplier = 0;
            string resultStr = "";

            if (isRoyalFlush) { resultStr = "皇家同花順"; multiplier = 250; }
            else if (isStraightFlush) { resultStr = "同花順"; multiplier = 50; }
            else if (isFourOfAKind) { resultStr = "四條"; multiplier = 25; }
            else if (isFullHouse) { resultStr = "葫蘆"; multiplier = 9; }
            else if (isFlush) { resultStr = "同花"; multiplier = 6; }
            else if (isStraight) { resultStr = "順子"; multiplier = 4; }
            else if (isThreeOfAKind) { resultStr = "三條"; multiplier = 3; }
            else if (isTwoPair) { resultStr = "兩對"; multiplier = 2; }
            else if (isOnePair) { resultStr = "一對"; multiplier = 1; }
            else { resultStr = "雜牌 (未中獎)"; multiplier = 0; }

            // 結算獎金
            int winMoney = currentBet * multiplier;
            totalMoney += winMoney;
            txtTotalMoney.Text = totalMoney.ToString();

            lblResult.Text = $"{resultStr}！ 賠率 {multiplier} 倍，贏得 {winMoney} 元。";

            // 回復下一局的狀態
            for (int i = 0; i < 5; i++) pic[i].Enabled = false;
            btnChangeCard.Enabled = false;
            btnCheck.Enabled = false;
            btnBet.Enabled = true;       // 開放下一局押注
            txtBetMoney.Enabled = true;
        }

        private void frmPoker_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (btnDealCard.Enabled == false)
            {
                switch (e.KeyChar)
                {
                    case 'q': // q鍵
                              // 同花大順
                        playerPoker[0] = 51;
                        playerPoker[1] = 47;
                        playerPoker[2] = 43;
                        playerPoker[3] = 39;
                        playerPoker[4] = 3;
                        break;
                    case 'w': // w鍵
                              // 同花順
                        playerPoker[0] = 37;
                        playerPoker[1] = 33;
                        playerPoker[2] = 29;
                        playerPoker[3] = 25;
                        playerPoker[4] = 21;
                        break;
                    case 'e': // e鍵
                              // 同花
                        playerPoker[0] = 50;
                        playerPoker[1] = 38;
                        playerPoker[2] = 34;
                        playerPoker[3] = 22;
                        playerPoker[4] = 18;
                        break;
                    case 'r': // r鍵
                              // 鐵支
                        playerPoker[0] = 48;
                        playerPoker[1] = 39;
                        playerPoker[2] = 38;
                        playerPoker[3] = 37;
                        playerPoker[4] = 36;
                        break;
                    case 't': // t鍵
                              // 葫蘆
                        playerPoker[0] = 30;
                        playerPoker[1] = 29;
                        playerPoker[2] = 6;
                        playerPoker[3] = 5;
                        playerPoker[4] = 4;
                        break;
                    case 'y': // y鍵
                              // 三條
                        playerPoker[0] = 48;
                        playerPoker[1] = 39;
                        playerPoker[2] = 15;
                        playerPoker[3] = 14;
                        playerPoker[4] = 13;
                        break;
                }
                // 顯示五張撲克牌到桌面上
                ShowCards();
            }
        }
        private void ShowCards()
        {
            for (int i = 0; i < 5; i++)
            {
                // 將陣列記錄的牌號轉成圖片顯示
                pic[i].Image = GetImage("pic" + (playerPoker[i] + 1));

                // 讓這五張牌顯示為翻開的狀態
                pic[i].Tag = "front";
            }
        }
    }
}