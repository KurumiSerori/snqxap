﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

//待编写的部分：       
//               先不管非夜战能放非夜战技能

namespace snqxap
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 枪娘总数
        /// </summary>
        public const int GUN_NUMBER = 119; 
        /// <summary>
        /// 装备总数
        /// </summary>
        public const int EQUIP_NUMBER = 49;
        /// <summary>
        /// 0-3 外骨骼 4-7 穿甲弹 8-11 高速弹 12-15 光学瞄具 16-19 全息 20-23 红点 24-27夜视 28五星穿甲 29五星夜视
        /// AR 全息/光喵/ACOG/夜视 高速弹 外骨 smg 外骨 空 全息/光喵/ACOG/夜视
        /// RF 穿甲 全息/光喵/ACOG 空 MG 穿甲 全息/光喵/ACOG 空 HG 夜视 空 外骨
        /// </summary>
        Equip[] equip = new Equip[EQUIP_NUMBER];
        Gun[] gun = new Gun[GUN_NUMBER + 1]; //存枪娘数据
        GunGrid[] gg = new GunGrid[9];//存九宫格buff加成
        Double[] skillupdamage = new Double[9];//存九宫格技能加成
        Double[] skilldamageagain = new Double[9];
        Double[] skilluphit = new Double[9];
        Double[] skillupshotspeed = new Double[9];
        Double[] skillupdodge = new Double[9];
        int[] equipdamage = new int[9];
        int[] equiphit = new int[9];
        int[] equipshotspeed = new int[9];
        int[] equipdodge = new int[9];
        int[] equipbelt = new int[9];
        double[] equipcrit = new double[9];
        double[] equipnightsee = new double[9];
        int[] equipbreakarmor = new int[9];
        double skilldowndodge;//对敌人技能debuff
        double skilldownhit;
        double skilldowndamage;
        int[] lastgunindex = new int[9];//存上次格内枪娘
        int howmany;//计算在场枪娘数用
        bool innight;
        double[] merry = new double[9];
        List<Border>[] gridlist = new List<Border>[9];

        public static readonly float[][] arrAbilityRatio = new float[][]  //各类枪娘属性成长基础
{
    new float[] //凑数
    {
        1f,1f,1f,1f,1f,1f,1f
    },
      new float[]   //凑数
      {
        1f,1f,1f,1f,1f,1f,1f
    },
        new float[]   //2 ar 3 smg 4 hg 5 rf 6 mg    
	{
          1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f
    },
    new float[]  
	{
        1.6f,
        0.6f,
        1.2f,
        1.2f,
        0.3f,
        1.6f,
        0.75f
    },
    new float[]
	{
         0.6f,
        0.6f,
        0.8f,
        1.5f,
        1.2f,
        1.8f,
        0.75f
    
    },
    new float[]
	{
          0.8f,
        2.4f,
        0.5f,
        0.7f,
        1.6f,
        0.8f,
        1.4f
    },
    new float[]
	{
        1.5f,
        1.8f,
        1.6f,
        0.4f,
        0.6f,
        0.6f,
        0.95f
    }
};

        public MainWindow()
        {
       
            InitializeComponent();
     
            baka();

            Combo0.SelectedIndex = GUN_NUMBER;
            renewindex(0);
            Combo1.SelectedIndex = GUN_NUMBER;
            renewindex(1);
            Combo2.SelectedIndex = GUN_NUMBER;
            renewindex(2);
            Combo3.SelectedIndex = GUN_NUMBER;
            renewindex(3);
            Combo4.SelectedIndex = GUN_NUMBER;
            renewindex(4);
            Combo5.SelectedIndex = GUN_NUMBER;
            renewindex(5);
            Combo6.SelectedIndex = GUN_NUMBER;
            renewindex(6);
            Combo7.SelectedIndex = GUN_NUMBER;
            renewindex(7);
            Combo8.SelectedIndex = GUN_NUMBER;
            renewindex(8);

            rb0.IsChecked = false;
            rb1.IsChecked = false;
            rb2.IsChecked = false;
            rb3.IsChecked = false;
            rb4.IsChecked = false;
            rb5.IsChecked = false;
            rb6.IsChecked = false;
            rb7.IsChecked = false;
            rb8.IsChecked = false;

            tank.Content = 0;
            enemydodge.Text = "10";
            enemyhit.Text = "20";
            enemydamage.Text = "1";

            rbf0.IsChecked = false;
            rbf1.IsChecked = false;
            rbf2.IsChecked = false;
            rbf3.IsChecked = false;
            rbf4.IsChecked = false;
            rbf5.IsChecked = false;
            rbf6.IsChecked = false;
            rbf7.IsChecked = false;
            rbf8.IsChecked = false;

            ftank.Content = 0;

            cb0.IsChecked = false;
            cb1.IsChecked = false;
            cb2.IsChecked = false;
            cb3.IsChecked = false;
            cb4.IsChecked = false;
            cb5.IsChecked = false;
            cb6.IsChecked = false;
            cb7.IsChecked = false;
            cb8.IsChecked = false;

            Lskillrate0.Content = "0%";
            Lskillrate1.Content = "0%";
            Lskillrate2.Content = "0%";
            Lskillrate3.Content = "0%";
            Lskillrate4.Content = "0%";
            Lskillrate5.Content = "0%";
            Lskillrate6.Content = "0%";
            Lskillrate7.Content = "0%";
            Lskillrate8.Content = "0%";
        }


        /// <summary>
        /// 获取更新urll内容
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        public string Get(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            request.Timeout = 5000;

            try
            {
                Stream stream = request.GetResponse().GetResponseStream();
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                string strContent = sr.ReadToEnd();
                return strContent;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 初始化数据，别问我为什么叫这个名字
        /// </summary>
        public void baka() {

            howmany = 0;

            for (int i = 0; i < GUN_NUMBER + 1; i++)
                gun[i] = new Gun();

            gun[0].name = "汤姆森"; gun[0].what = 3;gun[0].crit = 0.05; gun[0].belt = 0; gun[0].number = 2; gun[0].effect0 = 1; gun[0].effect1 = 7; gun[0].damageup = 0.12; gun[0].hitup = 0; gun[0].shotspeedup = 0; gun[0].critup = 0; gun[0].dodgeup = 0.15; gun[0].to = 2;
            gun[1].name = "司登MkⅡ"; gun[1].what = 3; gun[1].crit = 0.05; gun[1].belt = 0; gun[1].number = 3; gun[1].effect0 = 1; gun[1].effect1 = 4; gun[1].effect2 = 7; gun[1].damageup = 0; gun[1].hitup = 0.1; gun[1].shotspeedup = 0; gun[1].critup = 0; gun[1].dodgeup = 0.3; gun[1].to = 2;
            gun[2].name = "UMP9"; gun[2].what = 3; gun[2].crit = 0.05; gun[2].belt = 0; gun[2].number = 3; gun[2].effect0 = 1; gun[2].effect1 = 4; gun[2].effect2 = 7; gun[2].damageup = 0; gun[2].hitup = 0.3; gun[2].shotspeedup = 0.12; gun[2].critup = 0; gun[2].dodgeup = 0; gun[2].to = 2;
            gun[3].name = "Vector"; gun[3].what = 3; gun[3].crit = 0.05; gun[3].belt = 0; gun[3].number = 1; gun[3].effect0 = 4; gun[3].damageup = 0; gun[3].hitup = 0; gun[3].shotspeedup = 0.25; gun[3].critup = 0; gun[3].dodgeup = 0; gun[3].to = 2;
            gun[4].name = "蝎式"; gun[4].what = 3; gun[4].crit = 0.05; gun[4].belt = 0; gun[4].number = 1; gun[4].effect0 = 4; gun[4].damageup = 0; gun[4].hitup = 0.5; gun[4].shotspeedup = 0.15; gun[4].critup = 0; gun[4].dodgeup = 0; gun[4].to = 2;
            gun[5].name = "M3"; gun[5].what = 3; gun[5].crit = 0.05; gun[5].belt = 0; gun[5].number = 1; gun[5].effect0 = 4; gun[5].damageup = 0; gun[5].hitup = 0.4; gun[5].shotspeedup = 0; gun[5].critup = 0; gun[5].dodgeup = 0.3; gun[5].to = 2;
             gun[6].name = "IDW";gun[6].what = 3; gun[6].crit = 0.05; gun[6].belt = 0; gun[6].number = 3; gun[6].effect0 = 1; gun[6].effect1 = 4; gun[6].effect2 = 7; gun[6].damageup = 0; gun[6].hitup = 0; gun[6].shotspeedup = 0; gun[6].critup = 0; gun[6].dodgeup = 0.2; gun[6].to = 2;
            gun[7].name = "微型乌兹"; gun[7].what = 3; gun[7].crit = 0.05; gun[7].belt = 0; gun[7].number = 2; gun[7].effect0 = 2; gun[7].effect1 = 8; gun[7].damageup = 0.18; gun[7].hitup = 0; gun[7].shotspeedup = 0; gun[7].critup = 0; gun[7].dodgeup = 0; gun[7].to = 2;
            gun[8].name = "FMG-9"; gun[8].what = 3;gun[8].crit = 0.05; gun[8].belt = 0; gun[8].number = 2; gun[8].effect0 = 1; gun[8].effect1 = 7; gun[8].damageup = 0.1; gun[8].hitup = 0; gun[8].shotspeedup = 0; gun[8].critup = 0; gun[8].dodgeup = 0.12; gun[8].to = 2;
            gun[9].name = "MAC-10"; gun[9].what = 3; gun[9].crit = 0.05; gun[9].belt = 0; gun[9].number = 3; gun[9].effect0 = 1; gun[9].effect1 = 4; gun[9].effect2 = 7; gun[9].damageup = 0.12; gun[9].hitup = 0; gun[9].shotspeedup = 0; gun[9].critup = 0; gun[9].dodgeup = 0; gun[9].to = 2;
            gun[10].name = "M45"; gun[10].what = 3; gun[10].crit = 0.05; gun[10].belt = 0; gun[10].number = 2; gun[10].effect0 = 1; gun[10].effect1 = 7; gun[10].damageup = 0; gun[10].hitup = 0; gun[10].shotspeedup = 0.1; gun[10].critup = 0; gun[10].dodgeup = 0.1; gun[10].to = 2;
            gun[11].name = "Spectre M4"; gun[11].what = 3;  gun[11].crit = 0.05; gun[11].belt = 0; gun[11].number = 1; gun[11].effect0 = 4; gun[11].damageup = 0.2; gun[11].hitup = 0; gun[11].shotspeedup = 0; gun[11].critup = 0; gun[11].dodgeup = 0; gun[11].to = 2;
            gun[12].name = "PPS-43"; gun[12].what = 3;  gun[12].crit = 0.05; gun[12].belt = 0; gun[12].number = 3; gun[12].effect0 = 1; gun[12].effect1 = 4; gun[12].effect2 = 7; gun[12].damageup = 0.12; gun[12].hitup = 0; gun[12].shotspeedup = 0; gun[12].critup = 0; gun[12].dodgeup = 0; gun[12].to = 2;
            gun[13].name = "PP-2000"; gun[13].what = 3;  gun[13].crit = 0.05; gun[13].belt = 0; gun[13].number = 2; gun[13].effect0 = 1; gun[13].effect1 = 7; gun[13].damageup = 0.1; gun[13].hitup = 0.25; gun[13].shotspeedup = 0; gun[13].critup = 0; gun[13].dodgeup = 0; gun[13].to = 2;
            gun[14].name = "MP5"; gun[14].what = 3; gun[14].crit = 0.05; gun[14].belt = 0; gun[14].number = 2; gun[14].effect0 = 1; gun[14].effect1 = 7; gun[14].damageup = 0; gun[14].hitup = 0.4; gun[14].shotspeedup = 0; gun[14].critup = 0.2; gun[14].dodgeup = 0; gun[14].to = 2;
            gun[15].name = "伯莱塔38型"; gun[15].what = 3;gun[15].crit = 0.05; gun[15].belt = 0; gun[15].number = 2; gun[15].effect0 = 1; gun[15].effect1 = 7; gun[15].damageup = 0.05; gun[15].hitup = 0; gun[15].shotspeedup = 0.1; gun[15].critup = 0; gun[15].dodgeup = 0; gun[15].to = 2;
            gun[16].name = "MP40"; gun[16].what = 3; gun[16].crit = 0.05; gun[16].belt = 0; gun[16].number = 2; gun[16].effect0 = 1; gun[16].effect1 = 7; gun[16].damageup = 0; gun[16].hitup = 0.25; gun[16].shotspeedup = 0; gun[16].critup = 0; gun[16].dodgeup = 0.2; gun[16].to = 2;
            gun[17].name = "PPSh-41"; gun[17].what = 3; gun[17].crit = 0.05; gun[17].belt = 0; gun[17].number = 2; gun[17].effect0 = 2; gun[17].effect1 = 8; gun[17].damageup = 0.1; gun[17].hitup = 0; gun[17].shotspeedup = 0.05; gun[17].critup = 0; gun[17].dodgeup = 0; gun[17].to = 2;
            gun[18].name = "64式"; gun[18].what = 3; gun[18].crit = 0.05; gun[18].belt = 0; gun[18].number = 1; gun[18].effect0 = 4; gun[18].damageup = 0; gun[18].hitup = 0; gun[18].shotspeedup = 0.2; gun[18].critup = 0; gun[18].dodgeup = 0; gun[18].to = 2;
            gun[19].name = "UMP45"; gun[19].what = 3;  gun[19].crit = 0.05; gun[19].belt = 0; gun[19].number = 3; gun[19].effect0 = 1; gun[19].effect1 = 4; gun[19].effect2 = 7; gun[19].damageup = 0.15; gun[19].hitup = 0.0; gun[19].shotspeedup = 0; gun[19].critup = 0.5; gun[19].dodgeup = 0; gun[19].to = 2;
            gun[20].name = "索米"; gun[20].what = 3; gun[20].crit = 0.05; gun[20].belt = 0; gun[20].number = 2; gun[20].effect0 = 1; gun[20].effect1 = 7; gun[20].damageup = 0; gun[20].hitup = 0.3; gun[20].shotspeedup = 0.15; gun[20].critup = 0; gun[20].dodgeup = 0; gun[20].to = 2;
            gun[21].name = "OTs-12"; gun[21].what = 2; gun[21].crit = 0.2; gun[21].belt = 0; gun[21].number = 2; gun[21].effect0 = 6; gun[21].effect1 = 9; gun[21].damageup = 0.15; gun[21].hitup = 0; gun[21].shotspeedup = 0.2; gun[21].critup = 0; gun[21].dodgeup = 0; gun[21].to = 3;
            gun[22].name = "G36"; gun[22].what = 2; gun[22].crit = 0.2; gun[22].belt = 0; gun[22].number = 2; gun[22].effect0 = 6; gun[22].effect1 = 3; gun[22].damageup = 0.3; gun[22].hitup = 0; gun[22].shotspeedup = 0.1; gun[22].critup = 0; gun[22].dodgeup = 0; gun[22].to = 3;
            gun[23].name = "FAL"; gun[23].what = 2;  gun[23].crit = 0.2; gun[23].belt = 0; gun[23].number = 3; gun[23].effect0 = 3; gun[23].effect1 = 6; gun[23].effect2 = 9; gun[23].damageup = 0; gun[23].hitup = 0; gun[23].shotspeedup = 0; gun[23].critup = 0; gun[23].dodgeup = 0.2; gun[23].to = 3;
            gun[24].name = "HK416"; gun[24].what = 2; gun[24].crit = 0.2; gun[24].belt = 0; gun[24].number = 1; gun[24].effect0 = 6; gun[24].damageup = 0.4; gun[24].hitup = 0; gun[24].shotspeedup = 0; gun[24].critup = 0; gun[24].dodgeup = 0; gun[24].to = 3;
            gun[25].name = "G41"; gun[25].what = 2; gun[25].crit = 0.2; gun[25].belt = 0; gun[25].number = 2; gun[25].effect0 = 3; gun[25].effect1 = 9; gun[25].damageup = 0; gun[25].hitup = 0.5; gun[25].shotspeedup = 0; gun[25].critup = 0; gun[25].dodgeup = 0.15; gun[25].to = 3;
            gun[26].name = "56-1式"; gun[26].what = 2; gun[26].crit = 0.2; gun[26].belt = 0; gun[26].number = 1; gun[26].effect0 = 6; gun[26].damageup = 0; gun[26].hitup = 0; gun[26].shotspeedup = 0; gun[26].critup = 0.1; gun[26].dodgeup = 0.15; gun[26].to = 3;
            gun[27].name = "M4A1"; gun[27].what = 2;gun[27].crit = 0.2; gun[27].belt = 0; gun[27].number = 1; gun[27].effect0 = 7; gun[27].damageup = 0; gun[27].hitup = 0; gun[27].shotspeedup = 0.18; gun[27].critup = 0; gun[27].dodgeup = 0; gun[27].to = 2;
            gun[28].name = "M16A1"; gun[28].what = 2; gun[28].crit = 0.2; gun[28].belt = 0; gun[28].number = 1; gun[28].effect0 = 1; gun[28].damageup = 0.18; gun[28].hitup = 0; gun[28].shotspeedup = 0; gun[28].critup = 0; gun[28].dodgeup = 0; gun[28].to = 2;
            gun[29].name = "ST AR-15"; gun[29].what = 2;gun[29].crit = 0.2; gun[29].belt = 0; gun[29].number = 1; gun[29].effect0 = 9; gun[29].damageup = 0; gun[29].hitup = 0; gun[29].shotspeedup = 0; gun[29].critup = 0; gun[29].dodgeup = 0.36; gun[29].to = 2;
            gun[30].name = "FAMAS"; gun[30].what = 2; gun[30].crit = 0.2; gun[30].belt = 0; gun[30].number = 1; gun[30].effect0 = 3; gun[30].damageup = 0.25; gun[30].hitup = 0.6; gun[30].shotspeedup = 0; gun[30].critup = 0; gun[30].dodgeup = 0; gun[30].to = 3;
            gun[31].name = "AK-47"; gun[31].what = 2;  gun[31].crit = 0.2; gun[31].belt = 0; gun[31].number = 1; gun[31].effect0 = 2; gun[31].damageup = 0; gun[31].hitup = 0; gun[31].shotspeedup = 0; gun[31].critup = 0; gun[31].dodgeup = 0.18; gun[31].to = 3;
            gun[32].name = "StG44"; gun[32].what = 2; gun[32].crit = 0.2; gun[32].belt = 0; gun[32].number = 1; gun[32].effect0 = 6; gun[32].damageup = 0; gun[32].hitup = 0.6; gun[32].shotspeedup = 0; gun[32].critup = 0; gun[32].dodgeup = 0.2; gun[32].to = 3;
            gun[33].name = "CZ-805"; gun[33].what = 2; gun[33].crit = 0.2; gun[33].belt = 0; gun[33].number = 2; gun[33].effect0 = 3; gun[33].effect1 = 9; gun[33].damageup = 0; gun[33].hitup = 0.5; gun[33].shotspeedup = 0.25; gun[33].critup = 0; gun[33].dodgeup = 0; gun[33].to = 3;
            gun[34].name = "M4 SOPMODⅡ"; gun[34].what = 2;  gun[34].crit = 0.2; gun[34].belt = 0; gun[34].number = 1; gun[34].effect0 = 3; gun[34].damageup = 0; gun[34].hitup = 0; gun[34].shotspeedup = 0; gun[34].critup = 0; gun[34].dodgeup = 0.36; gun[34].to = 2;
            gun[35].name = "TAR-21"; gun[35].what = 2;  gun[35].crit = 0.2; gun[35].belt = 0; gun[35].number = 2; gun[35].effect0 = 3; gun[35].effect1 = 9; gun[35].damageup = 0; gun[35].hitup = 0; gun[35].shotspeedup = 0; gun[35].critup = 0; gun[35].dodgeup = 0.18; gun[35].to = 3;
            gun[36].name = "加利尔"; gun[36].what = 2; gun[36].crit = 0.2; gun[36].belt = 0; gun[36].number = 1; gun[36].effect0 = 6; gun[36].damageup = 0; gun[36].hitup = 0.5; gun[36].shotspeedup = 0; gun[36].critup = 0; gun[36].dodgeup = 0.1; gun[36].to = 3;
            gun[37].name = "SIG-510"; gun[37].what = 2; gun[37].crit = 0.2; gun[37].belt = 0; gun[37].number = 2; gun[37].effect0 = 3; gun[37].effect1 = 9; gun[37].damageup = 0.2; gun[37].hitup = 0; gun[37].shotspeedup = 0.1; gun[37].critup = 0; gun[37].dodgeup = 0; gun[37].to = 3;
            gun[38].name = "G3"; gun[38].what = 2; gun[38].crit = 0.2; gun[38].belt = 0; gun[38].number = 1; gun[38].effect0 = 8; gun[38].damageup = 0; gun[38].hitup = 0.5; gun[38].shotspeedup = 0.2; gun[38].critup = 0; gun[38].dodgeup = 0; gun[38].to = 3;
            gun[39].name = "F2000"; gun[39].what = 2; gun[39].crit = 0.2; gun[39].belt = 0; gun[39].number = 1; gun[39].effect0 = 6; gun[39].damageup = 0.2; gun[39].hitup = 0; gun[39].shotspeedup = 0; gun[39].critup = 0; gun[39].dodgeup = 0.1; gun[39].to = 3;
            gun[40].name = "FNC"; gun[40].what = 2; gun[40].crit = 0.2; gun[40].belt = 0; gun[40].number = 1; gun[40].effect0 = 9; gun[40].damageup = 0; gun[40].hitup = 0.5; gun[40].shotspeedup = 0; gun[40].critup = 0; gun[40].dodgeup = 0.12; gun[40].to = 3;
            gun[41].name = "L85A1"; gun[41].what = 2; gun[41].crit = 0.2; gun[41].belt = 0; gun[41].number = 1; gun[41].effect0 = 2; gun[41].damageup = 0.2; gun[41].hitup = 0.5; gun[41].shotspeedup = 0; gun[41].critup = 0; gun[41].dodgeup = 0; gun[41].to = 3;
            gun[42].name = "9a91"; gun[42].what = 2;  gun[42].crit = 0.2; gun[42].belt = 0; gun[42].number = 2; gun[42].effect0 = 3; gun[42].effect1 = 9; gun[42].damageup = 0; gun[42].hitup = 0; gun[42].shotspeedup = 0.1; gun[42].critup = 0; gun[42].dodgeup = 0.15; gun[42].to = 3;
            gun[43].name = "AS Val"; gun[43].what = 2; gun[43].crit = 0.2; gun[43].belt = 0; gun[43].number = 1; gun[43].effect0 = 8; gun[43].damageup = 0.25; gun[43].hitup = 0; gun[43].shotspeedup = 0.1; gun[43].critup = 0; gun[43].dodgeup = 0; gun[43].to = 3;
            gun[44].name = "维尔德"; gun[44].what = 4; gun[44].crit = 0.2; gun[44].belt = 0; gun[44].number = 5; gun[44].effect0 = 1; gun[44].effect1 = 2; gun[44].effect2 = 4; gun[44].effect3 = 7;gun[44].effect4 = 8; gun[44].damageup = 0.18; gun[44].hitup = 0; gun[44].shotspeedup = 0.1; gun[44].critup = 0; gun[44].dodgeup = 0; gun[44].to = 1;
            gun[45].name = "纳甘左轮"; gun[45].what = 4; gun[45].crit = 0.2; gun[45].belt = 0; gun[45].number = 2; gun[45].effect0 = 2; gun[45].effect1 = 8; gun[45].damageup = 0.25; gun[45].hitup = 0; gun[45].shotspeedup = 0; gun[45].critup = 0.1; gun[45].dodgeup = 0; gun[45].to = 1;
            gun[46].name = "柯尔特左轮"; gun[46].what = 4; gun[46].crit = 0.2; gun[46].belt = 0; gun[46].number = 4; gun[46].effect0 = 2; gun[46].effect1 = 4; gun[46].effect2 = 6; gun[46].effect3 = 8; gun[46].damageup = 0.15; gun[46].hitup = 0.5; gun[46].shotspeedup = 0; gun[46].critup = 0; gun[46].dodgeup = 0; gun[46].to = 1;
            gun[47].name = "灰熊MkⅤ"; gun[47].what = 4; gun[47].crit = 0.2; gun[47].belt = 0; gun[47].number = 5; gun[47].effect0 = 1; gun[47].effect1 = 2; gun[47].effect2 = 6; gun[47].effect3 = 7; gun[47].effect4 = 8; gun[47].damageup = 0.18; gun[47].hitup = 0; gun[47].shotspeedup = 0; gun[47].critup = 0; gun[47].dodgeup = 0.2; gun[47].to = 1;
            gun[48].name = "托卡列夫"; gun[48].what = 4; gun[48].crit = 0.2; gun[48].belt = 0; gun[48].number = 4; gun[48].effect0 = 2; gun[48].effect1 = 3; gun[48].effect2 = 8; gun[48].effect3 = 9; gun[48].damageup = 0; gun[48].hitup = 0.5; gun[48].shotspeedup = 0.12; gun[48].critup = 0; gun[48].dodgeup = 0; gun[48].to = 1;
            gun[49].name = "格洛克17"; gun[49].what = 4; gun[49].crit = 0.2; gun[49].belt = 0; gun[49].number = 5; gun[49].effect0 = 1; gun[49].effect1 = 3; gun[49].effect2 = 6; gun[49].effect3 = 7; gun[49].effect4 = 9; gun[49].damageup = 0; gun[49].hitup = 0.5; gun[49].shotspeedup = 0; gun[49].critup = 0; gun[49].dodgeup = 0.25; gun[49].to = 1;
            gun[50].name = "马卡洛夫"; gun[50].what = 4;gun[50].crit = 0.2; gun[50].belt = 0; gun[50].number = 4; gun[50].effect0 = 1; gun[50].effect1 = 4; gun[50].effect2 = 6; gun[50].effect3 = 7; gun[50].damageup = 0.12; gun[50].hitup = 0; gun[50].shotspeedup = 0.1; gun[50].critup = 0; gun[50].dodgeup = 0; gun[50].to = 1;
            gun[51].name = "斯捷奇金"; gun[51].what = 4; gun[51].crit = 0.2; gun[51].belt = 0; gun[51].number = 4; gun[51].effect0 = 2; gun[51].effect1 = 3; gun[51].effect2 = 8; gun[51].effect3 = 9; gun[51].damageup = 0.1; gun[51].hitup = 0; gun[51].shotspeedup = 0.15; gun[51].critup = 0; gun[51].dodgeup = 0; gun[51].to = 1;
            gun[52].name = "阿斯特拉左轮"; gun[52].what = 4; gun[52].crit = 0.2; gun[52].belt = 0; gun[52].number = 4; gun[52].effect0 = 1; gun[52].effect1 = 3; gun[52].effect2 = 7; gun[52].effect3 = 9; gun[52].damageup = 0; gun[52].hitup = 0; gun[52].shotspeedup = 0.15; gun[52].critup = 0; gun[52].dodgeup = 0.15; gun[52].to = 1;
            gun[53].name = "P08"; gun[53].what = 4;gun[53].crit = 0.2; gun[53].belt = 0; gun[53].number = 2; gun[53].effect0 = 2; gun[53].effect1 = 8; gun[53].damageup = 0.2; gun[53].hitup = 0.6; gun[53].shotspeedup = 0; gun[53].critup = 0; gun[53].dodgeup = 0; gun[53].to = 1;
            gun[54].name = "Mk23"; gun[54].what = 4; gun[54].crit = 0.2; gun[54].belt = 0; gun[54].number = 4; gun[54].effect0 = 3; gun[54].effect1 = 4; gun[54].effect2 = 6; gun[54].effect3 = 9; gun[54].damageup = 0.25; gun[54].hitup = 0; gun[54].shotspeedup = 0; gun[54].critup = 0; gun[54].dodgeup = 0; gun[54].to = 1;
            gun[55].name = "M1911"; gun[55].what = 4; gun[55].crit = 0.2; gun[55].belt = 0; gun[55].number = 4; gun[55].effect0 = 2; gun[55].effect1 = 4; gun[55].effect2 = 6; gun[55].effect3 = 8; gun[55].damageup = 0; gun[55].hitup = 0.5; gun[55].shotspeedup = 0.1; gun[55].critup = 0; gun[55].dodgeup = 0; gun[55].to = 1;
            gun[56].name = "PPK"; gun[56].what = 4; gun[56].crit = 0.2; gun[56].belt = 0; gun[56].number = 4; gun[56].effect0 = 1; gun[56].effect1 = 4; gun[56].effect2 = 6; gun[56].effect3 = 7; gun[56].damageup = 0; gun[56].hitup = 0; gun[56].shotspeedup = 0.2; gun[56].critup = 0.1; gun[56].dodgeup = 0; gun[56].to = 1;
            gun[57].name = "C96"; gun[57].what = 4; gun[57].crit = 0.2; gun[57].belt = 0; gun[57].number = 3; gun[57].effect0 = 1; gun[57].effect1 = 6; gun[57].effect2 = 7; gun[57].damageup = 0; gun[57].hitup = 0.5; gun[57].shotspeedup = 0; gun[57].critup = 0; gun[57].dodgeup = 0.25; gun[57].to = 1;
            gun[58].name = "M950A"; gun[58].what = 4; gun[58].crit = 0.2; gun[58].belt = 0; gun[58].number = 4; gun[58].effect0 = 1; gun[58].effect1 = 3; gun[58].effect2 = 7; gun[58].effect3 = 9; gun[58].damageup = 0; gun[58].hitup = 0.5; gun[58].shotspeedup = 0.18; gun[58].critup = 0; gun[58].dodgeup = 0; gun[58].to = 1;
            gun[59].name = "P38"; gun[59].what = 4;gun[59].crit = 0.2; gun[59].belt = 0; gun[59].number = 4; gun[59].effect0 = 2; gun[59].effect1 = 3; gun[59].effect2 = 8; gun[59].effect3 = 9; gun[59].damageup = 0; gun[59].hitup = 0.5; gun[59].shotspeedup = 0.1; gun[59].critup = 0; gun[59].dodgeup = 0; gun[59].to = 1;
            gun[60].name = "M9"; gun[60].what = 4; gun[60].crit = 0.2; gun[60].belt = 0; gun[60].number = 4; gun[60].effect0 = 1; gun[60].effect1 = 2; gun[60].effect2 = 7; gun[60].effect3 = 8; gun[60].damageup = 0; gun[60].hitup = 0; gun[60].shotspeedup = 0; gun[60].critup = 0; gun[60].dodgeup = 0.4; gun[60].to = 1;
            gun[61].name = "P7"; gun[61].what = 4; gun[61].crit = 0.2; gun[61].belt = 0; gun[61].number = 6; gun[61].effect0 = 1; gun[61].effect1 = 2; gun[61].effect2 = 3; gun[61].effect3 = 7; gun[61].effect4 = 8; gun[61].effect5 = 9; gun[61].damageup = 0; gun[61].hitup = 0; gun[61].shotspeedup = 0.1; gun[61].critup = 0; gun[61].dodgeup = 0.2; gun[61].to = 1;
            gun[62].name = "92式"; gun[62].what = 4; gun[62].crit = 0.2; gun[62].belt = 0; gun[62].number = 8; gun[62].effect0 = 1; gun[62].effect1 = 2; gun[62].effect2 = 3; gun[62].effect3 = 4; gun[62].effect4 = 6; gun[62].effect5 = 7; gun[62].effect6 = 8; gun[62].effect7 = 9; gun[62].damageup = 0; gun[62].hitup = 0.35; gun[62].shotspeedup = 0; gun[62].critup = 0; gun[62].dodgeup = 0.25; gun[62].to = 1;
            gun[63].name = "FNP-9"; gun[63].what = 4; gun[63].crit = 0.2; gun[63].belt = 0; gun[63].number = 5; gun[63].effect0 = 2; gun[63].effect1 = 3; gun[63].effect2 = 6; gun[63].effect3 = 8; gun[63].effect4 = 9; gun[63].damageup = 0; gun[63].hitup = 0.5; gun[63].shotspeedup = 0.1; gun[63].critup = 0; gun[63].dodgeup = 0; gun[63].to = 1;
            gun[64].name = "MP-446"; gun[64].what = 4; gun[64].crit = 0.2; gun[64].belt = 0; gun[64].number = 5; gun[64].effect0 = 1; gun[64].effect1 = 2; gun[64].effect2 = 4; gun[64].effect3 = 7; gun[64].effect4 = 8; gun[64].damageup = 0.2; gun[64].hitup = 0; gun[64].shotspeedup = 0; gun[64].critup = 0; gun[64].dodgeup = 0; gun[64].to = 1;
            gun[65].name = "西蒙诺夫"; gun[65].what = 5; gun[65].crit = 0.4; gun[65].belt = 0; gun[65].number = 2;gun[65].effect0 = 2;gun[65].effect1 = 8; gun[65].rateup = 0.18; gun[65].damageup = 0; gun[65].hitup = 0; gun[65].shotspeedup = 0; gun[65].critup = 0; gun[65].dodgeup = 0; gun[65].to = 4;
            gun[66].name = "FN-49"; gun[66].what = 5;  gun[66].crit = 0.4; gun[66].belt = 0; gun[66].number = 2; gun[66].effect0 = 3; gun[66].effect1 = 9; gun[66].rateup = 0.18; gun[66].damageup = 0; gun[66].hitup = 0; gun[66].shotspeedup = 0; gun[66].critup = 0; gun[66].dodgeup = 0; gun[66].to = 4;
            gun[67].name = "李-恩菲尔德"; gun[67].what = 5; gun[67].crit = 0.4; gun[67].belt = 0; gun[67].number = 2; gun[67].effect0 = 2; gun[67].effect1 = 8; gun[67].rateup = 0.25; gun[67].damageup = 0; gun[67].hitup = 0; gun[67].shotspeedup = 0; gun[67].critup = 0; gun[67].dodgeup = 0; gun[67].to = 4;
            gun[68].name = "NTW-20"; gun[68].what = 5; gun[68].crit = 0.4; gun[68].belt = 0; gun[68].number = 1;gun[68].effect0 = 6;gun[68].rateup = 0.25; gun[68].damageup = 0; gun[68].hitup = 0; gun[68].shotspeedup = 0; gun[68].critup = 0; gun[68].dodgeup = 0; gun[68].to = 4;
            gun[69].name = "PTRD"; gun[69].what = 5;gun[69].crit = 0.4; gun[69].belt = 0; gun[69].number = 1; gun[69].effect0 = 6; gun[69].rateup = 0.22; gun[69].damageup = 0; gun[69].hitup = 0; gun[69].shotspeedup = 0; gun[69].critup = 0; gun[69].dodgeup = 0; gun[69].to = 4;
            gun[70].name = "SVT-38"; gun[70].what = 5;gun[70].crit = 0.4; gun[70].belt = 0; gun[70].number = 1; gun[70].effect0 = 6; gun[70].rateup = 0.18; gun[70].damageup = 0; gun[70].hitup = 0; gun[70].shotspeedup = 0; gun[70].critup = 0; gun[70].dodgeup = 0; gun[70].to = 4;
            gun[71].name = "WA2000"; gun[71].what = 5; gun[71].crit = 0.4; gun[71].belt = 0; gun[71].number = 1; gun[71].effect0 = 6; gun[71].rateup = 0.25; gun[71].damageup = 0; gun[71].hitup = 0; gun[71].shotspeedup = 0; gun[71].critup = 0; gun[71].dodgeup = 0; gun[71].to = 4;
            gun[72].name = "M14"; gun[72].what = 5; gun[72].crit = 0.4; gun[72].belt = 0; gun[72].number = 2; gun[72].effect0 = 3; gun[72].effect1 = 9; gun[72].rateup = 0.2; gun[72].damageup = 0; gun[72].hitup = 0; gun[72].shotspeedup = 0; gun[72].critup = 0; gun[72].dodgeup = 0; gun[72].to = 4;
            gun[73].name = "M21"; gun[73].what = 5; gun[73].crit = 0.4; gun[73].belt = 0; gun[73].number = 2; gun[73].effect0 = 2; gun[73].effect1 = 8; gun[73].rateup = 0.2; gun[73].damageup = 0; gun[73].hitup = 0; gun[73].shotspeedup = 0; gun[73].critup = 0; gun[73].dodgeup = 0; gun[73].to = 4;
            gun[74].name = "BM59"; gun[74].what = 5; gun[74].crit = 0.4; gun[74].belt = 0; gun[74].number = 1; gun[74].effect0 = 6; gun[74].rateup = 0.18; gun[74].damageup = 0; gun[74].hitup = 0; gun[74].shotspeedup = 0; gun[74].critup = 0; gun[74].dodgeup = 0; gun[74].to = 4;
            gun[75].name = "M1加兰德"; gun[75].what = 5; gun[75].crit = 0.4; gun[75].belt = 0; gun[75].number = 1; gun[75].effect0 = 6; gun[75].rateup = 0.2; gun[75].damageup = 0; gun[75].hitup = 0; gun[75].shotspeedup = 0; gun[75].critup = 0; gun[75].dodgeup = 0; gun[75].to = 4;
            gun[76].name = "SV-98"; gun[76].what = 5;gun[76].crit = 0.4; gun[76].belt = 0; gun[76].number = 1; gun[76].effect0 = 3; gun[76].rateup = 0.2; gun[76].damageup = 0; gun[76].hitup = 0; gun[76].shotspeedup = 0; gun[76].critup = 0; gun[76].dodgeup = 0; gun[76].to = 4;
            gun[77].name = "G43"; gun[77].what = 5; gun[77].crit = 0.4; gun[77].belt = 0; gun[77].number = 2; gun[77].effect0 = 3; gun[77].effect1 = 9; gun[77].rateup = 0.2; gun[77].damageup = 0; gun[77].hitup = 0; gun[77].shotspeedup = 0; gun[77].critup = 0; gun[77].dodgeup = 0; gun[77].to = 4;
            gun[78].name = "汉阳造88式"; gun[78].what = 5; gun[78].crit = 0.4; gun[78].belt = 0; gun[78].number = 1; gun[78].effect0 = 6; gun[78].rateup = 0.22; gun[78].damageup = 0; gun[78].hitup = 0; gun[78].shotspeedup = 0; gun[78].critup = 0; gun[78].dodgeup = 0; gun[78].to = 4;
            gun[79].name = "Kar98k"; gun[79].what = 5;  gun[79].crit = 0.4; gun[79].belt = 0; gun[79].number = 2; gun[79].effect0 = 3; gun[79].effect1 = 9; gun[79].rateup = 0.25; gun[79].damageup = 0; gun[79].hitup = 0; gun[79].shotspeedup = 0; gun[79].critup = 0; gun[79].dodgeup = 0; gun[79].to = 4;
            gun[80].name = "莫辛-纳甘"; gun[80].what = 5; gun[80].crit = 0.4; gun[80].belt = 0; gun[80].number = 1; gun[80].effect0 = 9; gun[80].rateup = 0.22; gun[80].damageup = 0; gun[80].hitup = 0; gun[80].shotspeedup = 0; gun[80].critup = 0; gun[80].dodgeup = 0; gun[80].to = 4;
            gun[81].name = "春田"; gun[81].what = 5; gun[81].crit = 0.4; gun[81].belt = 0; gun[81].number = 1; gun[81].effect0 = 3; gun[81].rateup = 0.22;  gun[81].damageup = 0; gun[81].hitup = 0; gun[81].shotspeedup = 0; gun[81].critup = 0; gun[81].dodgeup = 0; gun[81].to = 4;
            gun[82].name = "M60"; gun[82].what = 6; gun[82].crit = 0.05; gun[82].belt = 9; gun[82].number = 0; gun[82].damageup = 0; gun[82].hitup = 0; gun[82].shotspeedup = 0; gun[82].critup = 0; gun[82].dodgeup = 0; gun[82].to = 0;
            gun[83].name = "MG5"; gun[83].what = 6;gun[83].crit = 0.05; gun[83].belt = 11; gun[83].number = 0; gun[83].damageup = 0; gun[83].hitup = 0; gun[83].shotspeedup = 0; gun[83].critup = 0; gun[83].dodgeup = 0; gun[83].to = 0;
            gun[84].name = "M1918"; gun[84].what = 6;gun[84].crit = 0.05; gun[84].belt = 8; gun[84].number = 0; gun[84].damageup = 0; gun[84].hitup = 0; gun[84].shotspeedup = 0; gun[84].critup = 0; gun[84].dodgeup = 0; gun[84].to = 0;
            gun[85].name = "MG3"; gun[85].what = 6; gun[85].crit = 0.05; gun[85].belt = 10; gun[85].number = 0; gun[85].damageup = 0; gun[85].hitup = 0; gun[85].shotspeedup = 0; gun[85].critup = 0; gun[85].dodgeup = 0; gun[85].to = 0;
            gun[86].name = "M1919A4"; gun[86].what = 6; gun[86].crit = 0.05; gun[86].belt = 9; gun[86].number = 0; gun[86].damageup = 0; gun[86].hitup = 0; gun[86].shotspeedup = 0; gun[86].critup = 0; gun[86].dodgeup = 0; gun[86].to = 0;
            gun[87].name = "PK"; gun[87].what = 6;gun[87].crit = 0.05; gun[87].belt = 11; gun[87].number = 0; gun[87].damageup = 0; gun[87].hitup = 0; gun[87].shotspeedup = 0; gun[87].critup = 0; gun[87].dodgeup = 0; gun[87].to = 0;
            gun[88].name = "内格夫"; gun[88].what = 6;gun[88].crit = 0.05; gun[88].belt = 9; gun[88].number = 0; gun[88].damageup = 0; gun[88].hitup = 0; gun[88].shotspeedup = 0; gun[88].critup = 0; gun[88].dodgeup = 0; gun[88].to = 0;
            gun[89].name = "RPD"; gun[89].what = 6; gun[89].crit = 0.05; gun[89].belt = 8; gun[89].number = 0; gun[89].damageup = 0; gun[89].hitup = 0; gun[89].shotspeedup = 0; gun[89].critup = 0; gun[89].dodgeup = 0; gun[89].to = 0;
            gun[90].name = "M2HB"; gun[90].what = 6; gun[90].crit = 0.05; gun[90].belt = 9; gun[90].number = 0; gun[90].damageup = 0; gun[90].hitup = 0; gun[90].shotspeedup = 0; gun[90].critup = 0; gun[90].dodgeup = 0; gun[90].to = 0;
            gun[91].name = "LWMMG"; gun[91].what = 6; gun[91].crit = 0.05; gun[91].belt = 9; gun[91].number = 0; gun[91].damageup = 0; gun[91].hitup = 0; gun[91].shotspeedup = 0; gun[91].critup = 0; gun[91].dodgeup = 0; gun[91].to = 0;
            gun[92].name = "M249 SAW"; gun[92].what = 6;gun[92].crit = 0.05; gun[92].belt = 8; gun[92].number = 0; gun[92].damageup = 0; gun[92].hitup = 0; gun[92].shotspeedup = 0; gun[92].critup = 0; gun[92].dodgeup = 0; gun[92].to = 0;
            gun[93].name = "AAT-52"; gun[93].what = 6;gun[93].crit = 0.05; gun[93].belt = 10; gun[93].number = 0; gun[93].damageup = 0; gun[93].hitup = 0; gun[93].shotspeedup = 0; gun[93].critup = 0; gun[93].dodgeup = 0; gun[93].to = 0;
            gun[94].name = "DP28"; gun[94].what = 6; gun[94].crit = 0.05; gun[94].belt = 9; gun[94].number = 0; gun[94].damageup = 0; gun[94].hitup = 0; gun[94].shotspeedup = 0; gun[94].critup = 0; gun[94].dodgeup = 0; gun[94].to = 0;
            gun[95].name = "MG42"; gun[95].what = 6; gun[95].crit = 0.05; gun[95].belt = 10; gun[95].number = 0; gun[95].damageup = 0; gun[95].hitup = 0; gun[95].shotspeedup = 0; gun[95].critup = 0; gun[95].dodgeup = 0; gun[95].to = 0;
            gun[96].name = "MG34"; gun[96].what = 6; gun[96].crit = 0.05; gun[96].belt = 10; gun[96].number = 0; gun[96].damageup = 0; gun[96].hitup = 0; gun[96].shotspeedup = 0; gun[96].critup = 0; gun[96].dodgeup = 0; gun[96].to = 0;
            gun[97].name = "布伦"; gun[97].what = 6;  gun[97].crit = 0.05; gun[97].belt = 8; gun[97].number = 0; gun[97].damageup = 0; gun[97].hitup = 0; gun[97].shotspeedup = 0; gun[97].critup = 0; gun[97].dodgeup = 0; gun[97].to = 0;
            gun[98].name = "FG42"; gun[98].what = 6; gun[98].crit = 0.05; gun[98].belt = 8; gun[98].number = 0; gun[98].damageup = 0; gun[98].hitup = 0; gun[98].shotspeedup = 0; gun[98].critup = 0; gun[98].dodgeup = 0; gun[98].to = 0;
            gun[99].name = "MK48"; gun[99].what = 6;  gun[99].crit = 0.05; gun[99].belt = 10; gun[99].number = 0; gun[99].damageup = 0; gun[99].hitup = 0; gun[99].shotspeedup = 0; gun[99].critup = 0; gun[99].dodgeup = 0; gun[99].to = 0;
            gun[100].name = "谢尔久科夫"; gun[100].what = 4; gun[100].crit = 0.2; gun[100].belt = 0; gun[100].number = 3; gun[100].effect0 = 2; gun[100].effect1 = 4; gun[100].effect2 = 8; gun[100].damageup = 0.2; gun[100].hitup = 0.3; gun[100].to = 1;

            gun[101].name = "56式半";gun[101].what = 5; gun[101].crit = 0.4;gun[101].belt = 0;gun[101].number = 2;gun[101].effect0 = 2;gun[101].effect1 = 8;gun[101].rateup = 0.2;gun[101].to = 4;
            gun[101].eatratio = 110; gun[101].ratiododge = 115; gun[101].ratiohit = 105; gun[101].ratiohp = 105; gun[101].ratiopow = 100;gun[101].ratiorate = 110;
            gun[101].type = 101;gun[101].probability = 40; gun[101].skilleffect1 = 100; gun[101].skilleffect2 = 2;gun[101].skilleffect3 = 0; gun[101].skilleffect4 = 0; gun[101].growth = 0.6; gun[101].growth_type = 3;

            gun[102].name = "SPP-1"; gun[102].what = 4; gun[102].crit = 0.2; gun[102].belt = 0; gun[102].number = 4; gun[102].effect0 = 1; gun[102].effect1 = 2; gun[102].effect2 = 7; gun[102].effect3 = 8; gun[102].hitup = 0.8; gun[102].damageup = 0.1; gun[102].to = 1;
            gun[102].eatratio = 135; gun[102].ratiododge = 90; gun[102].ratiohit = 110; gun[102].ratiohp = 115; gun[102].ratiopow = 115; gun[102].ratiorate = 85;
            gun[102].type = 204; gun[102].probability = 40; gun[102].skilleffect1 = 20; gun[102].skilleffect2 = 8; gun[102].skilleffect3 = 0; gun[102].skilleffect4 = 0; gun[102].growth = 0.5; gun[102].growth_type = 3;

            gun[103].name = "Z-62"; gun[103].what = 3; gun[103].crit = 0.05; gun[103].belt = 0; gun[103].number = 2; gun[103].effect0 = 1; gun[103].effect1 = 4; gun[103].damageup = 0.12; gun[103].dodgeup = 0.1; gun[103].to = 2;
            gun[103].eatratio = 120; gun[103].ratiododge = 115; gun[103].ratiohit = 120; gun[103].ratiohp = 95; gun[103].ratiopow = 95; gun[103].ratiorate = 95;
            gun[103].type = 404; gun[103].probability = 28; gun[103].skilleffect1 = 1.5; gun[103].skilleffect2 = 0.5; gun[103].skilleffect3 = 2; gun[103].skilleffect4 = 1; gun[103].growth = 1; gun[103].growth_type = 4;

            gun[104].name = "PSG-1"; gun[104].what = 5; gun[104].crit = 0.4; gun[104].belt = 0; gun[104].number = 1; gun[104].effect0 = 9; gun[104].rateup = 0.22; gun[104].to = 4;
            gun[104].eatratio = 105; gun[104].ratiododge = 85; gun[104].ratiohit = 125; gun[104].ratiohp = 105; gun[104].ratiopow = 120; gun[104].ratiorate = 120;
            gun[104].type = 502; gun[104].probability = 30; gun[104].skilleffect1 = 2.4; gun[104].skilleffect2 = 2; gun[104].skilleffect3 = 0; gun[104].skilleffect4 = 0; gun[104].growth = 1.2; gun[104].growth_type = 2;

            gun[105].name = "OTs-14"; gun[105].what = 2; gun[105].crit = 0.2; gun[105].belt = 0; gun[105].number = 3; gun[105].effect0 = 3; gun[105].effect1 = 6;gun[105].effect2 = 9; gun[105].hitup = 0.65; gun[105].shotspeedup = 0.25; gun[105].to = 3;
            gun[105].eatratio = 125; gun[105].ratiododge = 125; gun[105].ratiohit = 125; gun[105].ratiohp = 100; gun[105].ratiopow = 105; gun[105].ratiorate = 110;
            gun[105].type = 131; gun[105].probability = 32; gun[105].skilleffect1 = 120; gun[105].skilleffect2 = 5; gun[105].skilleffect3 = 0; gun[105].skilleffect4 = 0; gun[105].growth = 0.6; gun[105].growth_type = 3;

            gun[106].name = "ARX-160"; gun[106].what = 2; gun[106].crit = 0.2; gun[106].belt = 0; gun[106].number = 1; gun[106].effect0 = 3; gun[106].damageup = 0.25; gun[106].hitup = 0.5; gun[106].to = 3;
            gun[106].eatratio = 115; gun[106].ratiododge = 120; gun[106].ratiohit = 120; gun[106].ratiohp = 90; gun[106].ratiopow = 110; gun[106].ratiorate = 110;
            gun[106].type = 601; gun[106].probability = 22; gun[106].skilleffect1 = 2.2; gun[106].skilleffect2 = 1; gun[106].skilleffect3 = 0; gun[106].skilleffect4 = 0; gun[106].growth = 1.5; gun[106].growth_type = 2;

            gun[107].name = "G11"; gun[107].what = 2; gun[107].crit = 0.2; gun[107].belt = 0; gun[107].number = 1; gun[107].effect0 = 6; gun[107].damageup = 0.3; gun[107].dodgeup = 0.1; gun[107].to = 3;
            gun[107].eatratio = 110; gun[107].ratiododge = 105; gun[107].ratiohit = 115; gun[107].ratiohp = 110; gun[107].ratiopow = 100; gun[107].ratiorate = 145;
            gun[107].type = 108; gun[107].probability = 32; gun[107].skilleffect1 = 2.2; gun[107].skilleffect2 = 3; gun[107].skilleffect3 = 0; gun[107].skilleffect4 = 0; gun[107].growth = 0.5; gun[107].growth_type = 3;

            gun[108].name = "Super SASS";gun[108].what = 5; gun[108].crit = 0.4; gun[108].belt = 0; gun[108].number = 1; gun[108].effect0 = 9; gun[108].rateup = 0.20; gun[108].to = 4;
            gun[108].eatratio = 105; gun[108].ratiododge = 90; gun[108].ratiohit = 110; gun[108].ratiohp = 100; gun[108].ratiopow = 115; gun[108].ratiorate = 120;
            gun[108].type = 503; gun[108].probability = 30; gun[108].skilleffect1 = 2.2; gun[108].skilleffect2 = 2; gun[108].skilleffect3 = 0; gun[108].skilleffect4 = 0; gun[108].growth = 1.2; gun[108].growth_type = 2;

            gun[109].name = "SVD"; gun[109].what = 5; gun[109].crit = 0.4; gun[109].belt = 0; gun[109].number = 2; gun[109].effect0 = 3; gun[109].effect1 = 9; gun[109].rateup = 0.22; gun[109].to = 4;
            gun[109].eatratio = 120; gun[109].ratiododge = 100; gun[109].ratiohit = 120; gun[109].ratiohp = 90; gun[109].ratiopow = 120; gun[109].ratiorate = 110;
            gun[109].type = 102; gun[109].probability = 36; gun[109].skilleffect1 = 50; gun[109].skilleffect2 = 5; gun[109].skilleffect3 = 0; gun[109].skilleffect4 = 0; gun[109].growth = 0.6; gun[109].growth_type = 3;

            gun[110].name = "P99"; gun[110].what = 4; gun[110].crit = 0.2; gun[110].belt = 0; gun[110].number = 3; gun[110].effect0 = 2; gun[110].effect1 = 6; gun[110].effect2 = 8; gun[110].shotspeedup = 0.05; gun[110].dodgeup = 0.35; gun[110].to = 1;
            gun[110].eatratio = 110; gun[110].ratiododge = 125; gun[110].ratiohit = 120; gun[110].ratiohp = 90; gun[110].ratiopow = 115; gun[110].ratiorate = 115;
            gun[110].type = 4; gun[110].probability = 32; gun[110].skilleffect1 = 30; gun[110].skilleffect2 = 5; gun[110].skilleffect3 = 0; gun[110].skilleffect4 = 0; gun[110].growth = 0.6; gun[110].growth_type = 3;

            gun[111].name = "MG4"; gun[111].what = 6; gun[111].crit = 0.05; gun[111].belt = 9; gun[111].number = 0; gun[111].to = 0;
            gun[111].eatratio = 135; gun[111].ratiododge = 120; gun[111].ratiohit = 120; gun[111].ratiohp = 110; gun[111].ratiopow = 95; gun[111].ratiorate = 125;
            gun[111].type = 101; gun[111].probability = 32; gun[111].skilleffect1 = 30; gun[111].skilleffect2 = 8; gun[111].skilleffect3 = 0; gun[111].skilleffect4 = 0; gun[111].growth = 1.2; gun[111].growth_type = 2;

            gun[112].name = "G36C"; gun[112].what = 3; gun[112].crit = 0.05; gun[112].belt = 0; gun[112].number = 3; gun[112].effect0 = 1; gun[112].effect1 = 4; gun[112].effect2 = 7; gun[112].damageup = 0.1; gun[112].shotspeedup = 0.08; gun[112].to = 2;
            gun[112].eatratio = 95; gun[112].ratiododge = 120; gun[112].ratiohit = 110; gun[112].ratiohp = 115; gun[112].ratiopow = 135; gun[112].ratiorate = 110;
            gun[112].type = 106; gun[112].probability = 20; gun[112].skilleffect1 = 2; gun[112].skilleffect2 = 0; gun[112].skilleffect3 = 0; gun[112].skilleffect4 = 0; gun[112].growth = 1; gun[112].growth_type = 2;

            gun[113].name = "NZ75"; gun[113].what = 4; gun[113].crit = 0.2; gun[113].belt = 0; gun[113].number = 6; gun[113].effect0 = 1; gun[113].effect1 = 4; gun[113].effect2 = 7; gun[113].effect3 = 3; gun[113].effect4 = 6; gun[113].effect5 = 9; gun[113].damageup = 0.1; gun[113].dodgeup = 0.25; gun[113].to = 1;
            gun[113].eatratio = 125; gun[113].ratiododge = 95; gun[113].ratiohit = 120; gun[113].ratiohp = 110; gun[113].ratiopow = 115; gun[113].ratiorate = 115;
            gun[113].type = 202; gun[113].probability = 20; gun[113].skilleffect1 = 15; gun[113].skilleffect2 = 8; gun[113].skilleffect3 = 0; gun[113].skilleffect4 = 0; gun[113].growth = 0.6; gun[113].growth_type = 3;

            gun[114].name = "79式"; gun[114].what = 3; gun[114].crit = 0.05; gun[114].belt = 0; gun[114].number = 2; gun[114].effect0 = 1; gun[114].effect1 = 7; gun[114].damageup = 0.2; gun[114].to = 2;
            gun[114].eatratio = 100; gun[114].ratiododge = 125; gun[114].ratiohit = 110; gun[114].ratiohp = 110; gun[114].ratiopow = 130; gun[114].ratiorate = 115;
            gun[114].type = 403; gun[114].probability = 30; gun[114].skilleffect1 = 2; gun[114].skilleffect2 = 2.5; gun[114].skilleffect3 = 0; gun[114].skilleffect4 = 0; gun[114].growth = 1; gun[114].growth_type = 2;

            gun[115].name = "M99"; gun[115].what = 5; gun[115].crit = 0.4; gun[115].belt = 0; gun[115].number = 2; gun[115].effect0 = 3; gun[115].effect1 = 9; gun[115].rateup = 0.25; gun[115].to = 4;
            gun[115].eatratio = 135; gun[115].ratiododge = 85; gun[115].ratiohit = 110; gun[115].ratiohp = 100; gun[115].ratiopow = 135; gun[115].ratiorate = 90;
            gun[115].type = 502; gun[115].probability = 16; gun[115].skilleffect1 = 4; gun[115].skilleffect2 = 2; gun[115].skilleffect3 = 0; gun[115].skilleffect4 = 0; gun[115].growth = 1.2; gun[115].growth_type = 2;

            gun[116].name = "95式"; gun[116].what = 2; gun[116].crit = 0.2; gun[116].belt = 0; gun[116].number = 1; gun[116].effect0 = 3; gun[116].damageup = 0.1; gun[116].dodgeup = 0.18; gun[116].to = 3;
            gun[116].eatratio = 120; gun[116].ratiododge = 110; gun[116].ratiohit = 120; gun[116].ratiohp = 105; gun[116].ratiopow = 120; gun[116].ratiorate = 105;
            gun[116].type = 101; gun[116].probability = 40; gun[116].skilleffect1 = 30; gun[116].skilleffect2 = 8; gun[116].skilleffect3 = 0; gun[116].skilleffect4 = 0; gun[116].growth = 0.6; gun[116].growth_type = 3;

            gun[117].name = "97式"; gun[117].what = 2; gun[117].crit = 0.2; gun[117].belt = 0; gun[117].number = 1; gun[117].effect0 = 9; gun[117].shotspeedup = 0.1; gun[117].dodgeup = 0.18; gun[117].to = 3;
            gun[117].eatratio = 125; gun[117].ratiododge = 105; gun[117].ratiohit = 120; gun[117].ratiohp = 105; gun[117].ratiopow = 115; gun[117].ratiorate = 105;
            gun[117].type = 102; gun[117].probability = 40; gun[117].skilleffect1 = 30; gun[117].skilleffect2 = 8; gun[117].skilleffect3 = 0; gun[117].skilleffect4 = 0; gun[117].growth = 0.6; gun[117].growth_type = 3;

            gun[118].name = "EVO 3"; gun[118].what = 3; gun[118].crit = 0.05; gun[118].belt = 0; gun[118].number = 1; gun[118].effect0 = 4; gun[118].hitup = 0.55; gun[118].to = 2;
            gun[118].eatratio = 105; gun[118].ratiododge = 115; gun[118].ratiohit = 115; gun[118].ratiohp = 110; gun[118].ratiopow = 90; gun[118].ratiorate = 120;
            gun[118].type = 401; gun[118].probability = 20; gun[118].skilleffect1 = 1.6; gun[118].skilleffect2 = 2.5; gun[118].skilleffect3 = 0; gun[118].skilleffect4 = 0; gun[118].growth = 1.5; gun[118].growth_type = 2;


            for (int i = 0; i < GUN_NUMBER + 1; i++)//加颜色
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo0.Items.Add(l);
            }

            for (int i = 0; i < GUN_NUMBER + 1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo1.Items.Add(l);
            }
            for (int i = 0; i < GUN_NUMBER + 1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo2.Items.Add(l);
            }
            for (int i = 0; i < GUN_NUMBER + 1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo3.Items.Add(l);
            }
            for (int i = 0; i < GUN_NUMBER + 1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo4.Items.Add(l);
            }

            for (int i = 0; i < GUN_NUMBER + 1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo5.Items.Add(l);
            }
            for (int i = 0; i < GUN_NUMBER + 1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo6.Items.Add(l);
            }
            for (int i = 0; i < GUN_NUMBER + 1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo7.Items.Add(l);
            }
            for (int i = 0; i < GUN_NUMBER+1; i++)
            {
                Label l = new Label();
                l.Content = gun[i].name;
                if (gun[i].what == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    l.Foreground = br;
                }
                else if (gun[i].what == 6)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                    l.Foreground = br;
                }
                Combo8.Items.Add(l);
            }
                Merry0.Content = "♡";
                Brush br2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry0.Foreground = br2;
                Merry1.Content = "♡";
                Merry1.Foreground = br2;
                Merry2.Content = "♡";
                Merry2.Foreground = br2;
                Merry3.Content = "♡";
                Merry3.Foreground = br2;
                Merry4.Content = "♡";
                Merry4.Foreground = br2;
                Merry5.Content = "♡";
                Merry5.Foreground = br2;
                Merry6.Content = "♡";
                Merry6.Foreground = br2;
                Merry0.Content = "♡";
                Merry0.Foreground = br2;
                Merry8.Content = "♡";
                Merry8.Foreground = br2;

            for (int i = 0; i < GUN_NUMBER; i++) //加图像
            {
                gun[i].image = "/assets/" + i.ToString() + ".png";
            }
            gun[GUN_NUMBER].image = "";

                for (int i = 0; i < 9; i++)
                {
                    gg[i] = new GunGrid();
                    gg[i].critup = 1.00;
                    gg[i].damageup = 1.00;
                    gg[i].dodgeup = 1.00;
                    gg[i].hitup = 1.00;
                    gg[i].shotspeedup = 1.00;
                    gg[i].rateup = 0.00;
                    lastgunindex[i] = -1;
                    skillupdamage[i] = new Double();
                    skillupdodge[i] = new Double();
                    skilluphit[i] = new Double();
                    skilldamageagain[i] = new Double();
                    skillupshotspeed[i] = new double();
                    skillupshotspeed[i] = 1;
                    skilluphit[i] = 1;
                    skillupdodge[i] = 1;
                    skillupdamage[i] = 1;
                    skilldamageagain[i] = 0;
                    equipdamage[i] = 0;
                    equiphit[i] = 0;
                    equipdodge[i] = 0;
                    equipbelt[i] = 0;
                    equipcrit[i] =0;
                    equipnightsee[i] = 0;
                    equipshotspeed[i] = 0;      
                    equipbreakarmor[i] = 0;
                    merry[i] = 1;
                    gridlist[i] = new List<Border>();

                }

            skilldowndodge = 1;
            skilldownhit = 1;
            skilldowndamage = 1;

            for(int i =1;i<=101;i++)
            {
                Level0.Items.Add(i);
                Level1.Items.Add(i);
                Level2.Items.Add(i);
                Level3.Items.Add(i);
                Level4.Items.Add(i);
                Level5.Items.Add(i);
                Level6.Items.Add(i);
                Level7.Items.Add(i);
                Level8.Items.Add(i);
            }
            for (int i = 1; i <= 10; i++)
            {
                SkillLevel0.Items.Add(i);
                SkillLevel1.Items.Add(i);
                SkillLevel2.Items.Add(i);
                SkillLevel3.Items.Add(i);
                SkillLevel4.Items.Add(i);
                SkillLevel5.Items.Add(i);
                SkillLevel6.Items.Add(i);
                SkillLevel7.Items.Add(i);
                SkillLevel8.Items.Add(i);
            }

            Level0.SelectedIndex = 99;
            Level1.SelectedIndex = 99;
            Level2.SelectedIndex = 99;
            Level3.SelectedIndex = 99;
            Level4.SelectedIndex = 99;
            Level5.SelectedIndex = 99;
            Level6.SelectedIndex = 99;
            Level7.SelectedIndex = 99;
            Level8.SelectedIndex = 99;
            SkillLevel0.SelectedIndex = 9;
            SkillLevel1.SelectedIndex = 9;
            SkillLevel2.SelectedIndex = 9;
            SkillLevel3.SelectedIndex = 9;
            SkillLevel4.SelectedIndex = 9;
            SkillLevel5.SelectedIndex = 9;
            SkillLevel6.SelectedIndex = 9;
            SkillLevel7.SelectedIndex = 9;
            SkillLevel8.SelectedIndex = 9;
            gun[0].eatratio = 105;
            gun[1].eatratio = 125;
            gun[2].eatratio = 110;
            gun[3].eatratio = 105;
            gun[4].eatratio = 115;
            gun[5].eatratio = 115;
            gun[6].eatratio = 130;
            gun[7].eatratio = 115;
            gun[8].eatratio = 115;
            gun[9].eatratio = 110;
            gun[10].eatratio = 105;
            gun[11].eatratio = 100;
            gun[12].eatratio = 105;
            gun[13].eatratio = 100;
            gun[14].eatratio = 105;
            gun[15].eatratio = 110;
            gun[16].eatratio = 115;
            gun[17].eatratio = 105;
            gun[18].eatratio = 105;
            gun[19].eatratio = 110;
            gun[20].eatratio = 105;
            gun[21].eatratio = 125;
            gun[22].eatratio = 110;
            gun[23].eatratio = 115;
            gun[24].eatratio = 115;
            gun[25].eatratio = 120;
            gun[26].eatratio = 110;
            gun[27].eatratio = 115;
            gun[28].eatratio = 110;
            gun[29].eatratio = 120;
            gun[30].eatratio = 105;
            gun[31].eatratio = 110;
            gun[32].eatratio = 120;
            gun[33].eatratio = 110;
            gun[34].eatratio = 110;
            gun[35].eatratio = 115;
            gun[36].eatratio = 130;
            gun[37].eatratio = 125;
            gun[38].eatratio = 120;
            gun[39].eatratio = 105;
            gun[40].eatratio = 115;
            gun[41].eatratio = 125;
            gun[42].eatratio = 110;
            gun[43].eatratio = 110;
            gun[44].eatratio = 140;
            gun[45].eatratio = 130;
            gun[46].eatratio = 130;
            gun[47].eatratio = 125;
            gun[48].eatratio = 105;
            gun[49].eatratio = 110;
            gun[50].eatratio = 130;
            gun[51].eatratio = 110;
            gun[52].eatratio = 120;
            gun[53].eatratio = 130;
            gun[54].eatratio = 110;
            gun[55].eatratio = 120;
            gun[56].eatratio = 125;
            gun[57].eatratio = 100;
            gun[58].eatratio = 115;
            gun[59].eatratio = 125;
            gun[60].eatratio = 110;
            gun[61].eatratio = 115;
            gun[62].eatratio = 110;
            gun[63].eatratio = 110;
            gun[64].eatratio = 115;
            gun[65].eatratio = 105;
            gun[66].eatratio = 115;
            gun[67].eatratio = 135;
            gun[68].eatratio = 130;
            gun[69].eatratio = 130;
            gun[70].eatratio = 105;
            gun[71].eatratio = 120;
            gun[72].eatratio = 110;
            gun[73].eatratio = 110;
            gun[74].eatratio = 95;
            gun[75].eatratio = 105;
            gun[76].eatratio = 115;
            gun[77].eatratio = 115;
            gun[78].eatratio = 120;
            gun[79].eatratio = 135;
            gun[80].eatratio = 130;
            gun[81].eatratio = 125;
            gun[82].eatratio = 115;
            gun[83].eatratio = 110;
            gun[84].eatratio = 125;
            gun[85].eatratio = 110;
            gun[86].eatratio = 110;
            gun[87].eatratio = 105;
            gun[88].eatratio = 135;
            gun[89].eatratio = 130;
            gun[90].eatratio = 95;
            gun[91].eatratio = 90;
            gun[92].eatratio = 135;
            gun[93].eatratio = 90;
            gun[94].eatratio = 125;
            gun[95].eatratio = 115;
            gun[96].eatratio = 110;
            gun[97].eatratio = 120;
            gun[98].eatratio = 100;
            gun[99].eatratio = 100;
            gun[100].eatratio = 115;

            gun[0].ratiohit = 100;
            gun[1].ratiohit = 115;
            gun[2].ratiohit = 120;
            gun[3].ratiohit = 90;
            gun[4].ratiohit = 100;
            gun[5].ratiohit = 105;
            gun[6].ratiohit = 110;
            gun[7].ratiohit = 85;
            gun[8].ratiohit = 105;
            gun[9].ratiohit = 90;
            gun[10].ratiohit = 100;
            gun[11].ratiohit = 105;
            gun[12].ratiohit = 110;
            gun[13].ratiohit = 100;
            gun[14].ratiohit = 115;
            gun[15].ratiohit = 80;
            gun[16].ratiohit = 100;
            gun[17].ratiohit = 85;
            gun[18].ratiohit = 90;
            gun[19].ratiohit = 110;
            gun[20].ratiohit = 130;
            gun[21].ratiohit = 125;
            gun[22].ratiohit = 115;
            gun[23].ratiohit = 100;
            gun[24].ratiohit = 115;
            gun[25].ratiohit = 115;
            gun[26].ratiohit = 90;
            gun[27].ratiohit = 120;
            gun[28].ratiohit = 120;
            gun[29].ratiohit = 120;
            gun[30].ratiohit = 130;
            gun[31].ratiohit = 90;
            gun[32].ratiohit = 110;
            gun[33].ratiohit = 125;
            gun[34].ratiohit = 125;
            gun[35].ratiohit = 120;
            gun[36].ratiohit = 100;
            gun[37].ratiohit = 95;
            gun[38].ratiohit = 110;
            gun[39].ratiohit = 120;
            gun[40].ratiohit = 115;
            gun[41].ratiohit = 100;
            gun[42].ratiohit = 125;
            gun[43].ratiohit = 120;
            gun[44].ratiohit = 125;
            gun[45].ratiohit = 85;
            gun[46].ratiohit = 90;
            gun[47].ratiohit = 100;
            gun[48].ratiohit = 105;
            gun[49].ratiohit = 125;
            gun[50].ratiohit = 115;
            gun[51].ratiohit = 95;
            gun[52].ratiohit = 90;
            gun[53].ratiohit = 85;
            gun[54].ratiohit = 115;
            gun[55].ratiohit = 100;
            gun[56].ratiohit = 115;
            gun[57].ratiohit = 95;
            gun[58].ratiohit = 115;
            gun[59].ratiohit = 95;
            gun[60].ratiohit = 120;
            gun[61].ratiohit = 130;
            gun[62].ratiohit = 100;
            gun[63].ratiohit = 115;
            gun[64].ratiohit = 105;
            gun[65].ratiohit = 100;
            gun[66].ratiohit = 95;
            gun[67].ratiohit = 105;
            gun[68].ratiohit = 105;
            gun[69].ratiohit = 105;
            gun[70].ratiohit = 100;
            gun[71].ratiohit = 125;
            gun[72].ratiohit = 115;
            gun[73].ratiohit = 120;
            gun[74].ratiohit = 95;
            gun[75].ratiohit = 105;
            gun[76].ratiohit = 115;
            gun[77].ratiohit = 90;
            gun[78].ratiohit = 90;
            gun[79].ratiohit = 105;
            gun[80].ratiohit = 120;
            gun[81].ratiohit = 105;
            gun[82].ratiohit = 105;
            gun[83].ratiohit = 115;
            gun[84].ratiohit = 120;
            gun[85].ratiohit = 110;
            gun[86].ratiohit = 110;
            gun[87].ratiohit = 90;
            gun[88].ratiohit = 125;
            gun[89].ratiohit = 125;
            gun[90].ratiohit = 85;
            gun[91].ratiohit = 115;
            gun[92].ratiohit = 125;
            gun[93].ratiohit = 110;
            gun[94].ratiohit = 105;
            gun[95].ratiohit = 95;
            gun[96].ratiohit = 95;
            gun[97].ratiohit = 115;
            gun[98].ratiohit = 115;
            gun[99].ratiohit = 110;
            gun[100].ratiohit = 120;

            gun[0].ratiopow = 120;
            gun[1].ratiopow = 90;
            gun[2].ratiopow = 95;
            gun[3].ratiopow = 115;
            gun[4].ratiopow = 85;
            gun[5].ratiopow = 105;
            gun[6].ratiopow = 90;
            gun[7].ratiopow = 90;
            gun[8].ratiopow = 95;
            gun[9].ratiopow = 105;
            gun[10].ratiopow = 115;
            gun[11].ratiopow = 100;
            gun[12].ratiopow = 130;
            gun[13].ratiopow = 115;
            gun[14].ratiopow = 115;
            gun[15].ratiopow = 125;
            gun[16].ratiopow = 105;
            gun[17].ratiopow = 100;
            gun[18].ratiopow = 105;
            gun[19].ratiopow = 105;
            gun[20].ratiopow = 110;
            gun[21].ratiopow = 90;
            gun[22].ratiopow = 110;
            gun[23].ratiopow = 130;
            gun[24].ratiopow = 115;
            gun[25].ratiopow = 110;
            gun[26].ratiopow = 120;
            gun[27].ratiopow = 105;
            gun[28].ratiopow = 110;
            gun[29].ratiopow = 105;
            gun[30].ratiopow = 105;
            gun[31].ratiopow = 120;
            gun[32].ratiopow = 115;
            gun[33].ratiopow = 100;
            gun[34].ratiopow = 110;
            gun[35].ratiopow = 110;
            gun[36].ratiopow = 105;
            gun[37].ratiopow = 115;
            gun[38].ratiopow = 120;
            gun[39].ratiopow = 100;
            gun[40].ratiopow = 115;
            gun[41].ratiopow = 100;
            gun[42].ratiopow = 95;
            gun[43].ratiopow = 90;
            gun[44].ratiopow = 90;
            gun[45].ratiopow = 110;
            gun[46].ratiopow = 125;
            gun[47].ratiopow = 135;
            gun[48].ratiopow = 120;
            gun[49].ratiopow = 110;
            gun[50].ratiopow = 90;
            gun[51].ratiopow = 105;
            gun[52].ratiopow = 120;
            gun[53].ratiopow = 105;
            gun[54].ratiopow = 110;
            gun[55].ratiopow = 95;
            gun[56].ratiopow = 85;
            gun[57].ratiopow = 115;
            gun[58].ratiopow = 110;
            gun[59].ratiopow = 100;
            gun[60].ratiopow = 110;
            gun[61].ratiopow = 115;
            gun[62].ratiopow = 115;
            gun[63].ratiopow = 105;
            gun[64].ratiopow = 105;
            gun[65].ratiopow = 100;
            gun[66].ratiopow = 105;
            gun[67].ratiopow = 115;
            gun[68].ratiopow = 145;
            gun[69].ratiopow = 140;
            gun[70].ratiopow = 110;
            gun[71].ratiopow = 120;
            gun[72].ratiopow = 105;
            gun[73].ratiopow = 115;
            gun[74].ratiopow = 110;
            gun[75].ratiopow = 120;
            gun[76].ratiopow = 115;
            gun[77].ratiopow = 105;
            gun[78].ratiopow = 100;
            gun[79].ratiopow = 115;
            gun[80].ratiopow = 115;
            gun[81].ratiopow = 115;
            gun[82].ratiopow = 115;
            gun[83].ratiopow = 110;
            gun[84].ratiopow = 115;
            gun[85].ratiopow = 110;
            gun[86].ratiopow = 125;
            gun[87].ratiopow = 125;
            gun[88].ratiopow = 95;
            gun[89].ratiopow = 95;
            gun[90].ratiopow = 145;
            gun[91].ratiopow = 135;
            gun[92].ratiopow = 90;
            gun[93].ratiopow = 115;
            gun[94].ratiopow = 105;
            gun[95].ratiopow = 110;
            gun[96].ratiopow = 110;
            gun[97].ratiopow = 110;
            gun[98].ratiopow = 110;
            gun[99].ratiopow = 125;
            gun[100].ratiopow = 125;

            gun[0].ratiododge = 90;
            gun[1].ratiododge = 110;
            gun[2].ratiododge = 125;
            gun[3].ratiododge = 120;
            gun[4].ratiododge = 130;
            gun[5].ratiododge = 105;
            gun[6].ratiododge = 120;
            gun[7].ratiododge = 125;
            gun[8].ratiododge = 140;
            gun[9].ratiododge = 110;
            gun[10].ratiododge = 105;
            gun[11].ratiododge = 115;
            gun[12].ratiododge = 110;
            gun[13].ratiododge = 130;
            gun[14].ratiododge = 115;
            gun[15].ratiododge = 85;
            gun[16].ratiododge = 90;
            gun[17].ratiododge = 95;
            gun[18].ratiododge = 100;
            gun[19].ratiododge = 120;
            gun[20].ratiododge = 95;
            gun[21].ratiododge = 125;
            gun[22].ratiododge = 105;
            gun[23].ratiododge = 95;
            gun[24].ratiododge = 105;
            gun[25].ratiododge = 95;
            gun[26].ratiododge = 90;
            gun[27].ratiododge = 120;
            gun[28].ratiododge = 115;
            gun[29].ratiododge = 120;
            gun[30].ratiododge = 105;
            gun[31].ratiododge = 85;
            gun[32].ratiododge = 85;
            gun[33].ratiododge = 105;
            gun[34].ratiododge = 115;
            gun[35].ratiododge = 110;
            gun[36].ratiododge = 95;
            gun[37].ratiododge = 85;
            gun[38].ratiododge = 90;
            gun[39].ratiododge = 105;
            gun[40].ratiododge = 90;
            gun[41].ratiododge = 100;
            gun[42].ratiododge = 130;
            gun[43].ratiododge = 125;
            gun[44].ratiododge = 105;
            gun[45].ratiododge = 115;
            gun[46].ratiododge = 95;
            gun[47].ratiododge = 85;
            gun[48].ratiododge = 100;
            gun[49].ratiododge = 125;
            gun[50].ratiododge = 120;
            gun[51].ratiododge = 95;
            gun[52].ratiododge = 90;
            gun[53].ratiododge = 100;
            gun[54].ratiododge = 95;
            gun[55].ratiododge = 100;
            gun[56].ratiododge = 130;
            gun[57].ratiododge = 95;
            gun[58].ratiododge = 95;
            gun[59].ratiododge = 105;
            gun[60].ratiododge = 95;
            gun[61].ratiododge = 115;
            gun[62].ratiododge = 115;
            gun[63].ratiododge = 120;
            gun[64].ratiododge = 100;
            gun[65].ratiododge = 115;
            gun[66].ratiododge = 100;
            gun[67].ratiododge = 105;
            gun[68].ratiododge = 80;
            gun[69].ratiododge = 80;
            gun[70].ratiododge = 115;
            gun[71].ratiododge = 90;
            gun[72].ratiododge = 85;
            gun[73].ratiododge = 85;
            gun[74].ratiododge = 85;
            gun[75].ratiododge = 95;
            gun[76].ratiododge = 85;
            gun[77].ratiododge = 85;
            gun[78].ratiododge = 110;
            gun[79].ratiododge = 110;
            gun[80].ratiododge = 105;
            gun[81].ratiododge = 115;
            gun[82].ratiododge = 105;
            gun[83].ratiododge = 105;
            gun[84].ratiododge = 125;
            gun[85].ratiododge = 90;
            gun[86].ratiododge = 95;
            gun[87].ratiododge = 100;
            gun[88].ratiododge = 130;
            gun[89].ratiododge = 125;
            gun[90].ratiododge = 75;
            gun[91].ratiododge = 105;
            gun[92].ratiododge = 130;
            gun[93].ratiododge = 110;
            gun[94].ratiododge = 120;
            gun[95].ratiododge = 105;
            gun[96].ratiododge = 105;
            gun[97].ratiododge = 110;
            gun[98].ratiododge = 135;
            gun[99].ratiododge = 120;
            gun[100].ratiododge = 95;

            gun[0].ratiohp = 135;
            gun[1].ratiohp = 105;
            gun[2].ratiohp = 100;
            gun[3].ratiohp = 105;
            gun[4].ratiohp = 90;
            gun[5].ratiohp = 105;
            gun[6].ratiohp = 85;
            gun[7].ratiohp = 90;
            gun[8].ratiohp = 80;
            gun[9].ratiohp = 100;
            gun[10].ratiohp = 105;
            gun[11].ratiohp = 100;
            gun[12].ratiohp = 100;
            gun[13].ratiohp = 90;
            gun[14].ratiohp = 95;
            gun[15].ratiohp = 115;
            gun[16].ratiohp = 105;
            gun[17].ratiohp = 105;
            gun[18].ratiohp = 100;
            gun[19].ratiohp = 105;
            gun[20].ratiohp = 125;
            gun[21].ratiohp = 95;
            gun[22].ratiohp = 115;
            gun[23].ratiohp = 120;
            gun[24].ratiohp = 110;
            gun[25].ratiohp = 115;
            gun[26].ratiohp = 125;
            gun[27].ratiohp = 100;
            gun[28].ratiohp = 105;
            gun[29].ratiohp = 95;
            gun[30].ratiohp = 110;
            gun[31].ratiohp = 120;
            gun[32].ratiohp = 115;
            gun[33].ratiohp = 105;
            gun[34].ratiohp = 100;
            gun[35].ratiohp = 95;
            gun[36].ratiohp = 95;
            gun[37].ratiohp = 105;
            gun[38].ratiohp = 100;
            gun[39].ratiohp = 95;
            gun[40].ratiohp = 100;
            gun[41].ratiohp = 85;
            gun[42].ratiohp = 105;
            gun[43].ratiohp = 120;
            gun[44].ratiohp = 120;
            gun[45].ratiohp = 105;
            gun[46].ratiohp = 120;
            gun[47].ratiohp = 130;
            gun[48].ratiohp = 130;
            gun[49].ratiohp = 95;
            gun[50].ratiohp = 95;
            gun[51].ratiohp = 125;
            gun[52].ratiohp = 120;
            gun[53].ratiohp = 105;
            gun[54].ratiohp = 120;
            gun[55].ratiohp = 110;
            gun[56].ratiohp = 85;
            gun[57].ratiohp = 125;
            gun[58].ratiohp = 115;
            gun[59].ratiohp = 100;
            gun[60].ratiohp = 115;
            gun[61].ratiohp = 95;
            gun[62].ratiohp = 95;
            gun[63].ratiohp = 90;
            gun[64].ratiohp = 100;
            gun[65].ratiohp = 105;
            gun[66].ratiohp = 105;
            gun[67].ratiohp = 90;
            gun[68].ratiohp = 105;
            gun[69].ratiohp = 105;
            gun[70].ratiohp = 95;
            gun[71].ratiohp = 100;
            gun[72].ratiohp = 95;
            gun[73].ratiohp = 105;
            gun[74].ratiohp = 110;
            gun[75].ratiohp = 100;
            gun[76].ratiohp = 95;
            gun[77].ratiohp = 90;
            gun[78].ratiohp = 115;
            gun[79].ratiohp = 95;
            gun[80].ratiohp = 100;
            gun[81].ratiohp = 95;
            gun[82].ratiohp = 110;
            gun[83].ratiohp = 120;
            gun[84].ratiohp = 95;
            gun[85].ratiohp = 120;
            gun[86].ratiohp = 105;
            gun[87].ratiohp = 115;
            gun[88].ratiohp = 105;
            gun[89].ratiohp = 100;
            gun[90].ratiohp = 130;
            gun[91].ratiohp = 105;
            gun[92].ratiohp = 95;
            gun[93].ratiohp = 110;
            gun[94].ratiohp = 85;
            gun[95].ratiohp = 100;
            gun[96].ratiohp = 95;
            gun[97].ratiohp = 105;
            gun[98].ratiohp = 90;
            gun[99].ratiohp = 105;
            gun[100].ratiohp = 105;

            gun[0].ratiorate = 105;
            gun[1].ratiorate = 95;
            gun[2].ratiorate = 110;
            gun[3].ratiorate = 130;
            gun[4].ratiorate = 120;
            gun[5].ratiorate = 85;
            gun[6].ratiorate = 90;
            gun[7].ratiorate = 130;
            gun[8].ratiorate = 115;
            gun[9].ratiorate = 115;
            gun[10].ratiorate = 95;
            gun[11].ratiorate = 115;
            gun[12].ratiorate = 95;
            gun[13].ratiorate = 105;
            gun[14].ratiorate = 115;
            gun[15].ratiorate = 95;
            gun[16].ratiorate = 95;
            gun[17].ratiorate = 120;
            gun[18].ratiorate = 120;
            gun[19].ratiorate = 105;
            gun[20].ratiorate = 120;
            gun[21].ratiorate = 105;
            gun[22].ratiorate = 110;
            gun[23].ratiorate = 105;
            gun[24].ratiorate = 115;
            gun[25].ratiorate = 115;
            gun[26].ratiorate = 105;
            gun[27].ratiorate = 120;
            gun[28].ratiorate = 115;
            gun[29].ratiorate = 115;
            gun[30].ratiorate = 125;
            gun[31].ratiorate = 100;
            gun[32].ratiorate = 90;
            gun[33].ratiorate = 115;
            gun[34].ratiorate = 120;
            gun[35].ratiorate = 120;
            gun[36].ratiorate = 95;
            gun[37].ratiorate = 85;
            gun[38].ratiorate = 90;
            gun[39].ratiorate = 125;
            gun[40].ratiorate = 110;
            gun[41].ratiorate = 115;
            gun[42].ratiorate = 120;
            gun[43].ratiorate = 115;
            gun[44].ratiorate = 90;
            gun[45].ratiorate = 80;
            gun[46].ratiorate = 85;
            gun[47].ratiorate = 100;
            gun[48].ratiorate = 100;
            gun[49].ratiorate = 115;
            gun[50].ratiorate = 110;
            gun[51].ratiorate = 125;
            gun[52].ratiorate = 95;
            gun[53].ratiorate = 100;
            gun[54].ratiorate = 120;
            gun[55].ratiorate = 105;
            gun[56].ratiorate = 115;
            gun[57].ratiorate = 120;
            gun[58].ratiorate = 135;
            gun[59].ratiorate = 105;
            gun[60].ratiorate = 115;
            gun[61].ratiorate = 115;
            gun[62].ratiorate = 115;
            gun[63].ratiorate = 115;
            gun[64].ratiorate = 110;
            gun[65].ratiorate = 105;
            gun[66].ratiorate = 95;
            gun[67].ratiorate = 100;
            gun[68].ratiorate = 85;
            gun[69].ratiorate = 80;
            gun[70].ratiorate = 105;
            gun[71].ratiorate = 115;
            gun[72].ratiorate = 130;
            gun[73].ratiorate = 105;
            gun[74].ratiorate = 120;
            gun[75].ratiorate = 115;
            gun[76].ratiorate = 110;
            gun[77].ratiorate = 120;
            gun[78].ratiorate = 90;
            gun[79].ratiorate = 95;
            gun[80].ratiorate = 85;
            gun[81].ratiorate = 90;
            gun[82].ratiorate = 105;
            gun[83].ratiorate = 115;
            gun[84].ratiorate = 105;
            gun[85].ratiorate = 125;
            gun[86].ratiorate = 95;
            gun[87].ratiorate = 80;
            gun[88].ratiorate = 125;
            gun[89].ratiorate = 110;
            gun[90].ratiorate = 100;
            gun[91].ratiorate = 90;
            gun[92].ratiorate = 125;
            gun[93].ratiorate = 120;
            gun[94].ratiorate = 105;
            gun[95].ratiorate = 125;
            gun[96].ratiorate = 115;
            gun[97].ratiorate = 95;
            gun[98].ratiorate = 115;
            gun[99].ratiorate = 110;
            gun[100].ratiorate = 110;

            gun[0].type = 106;
            gun[1].type = 401;
            gun[2].type = 403;
            gun[3].type = 404;
            gun[4].type = 404;
            gun[5].type = 401;
            gun[6].type = 305;
            gun[7].type = 404;
            gun[8].type = 104;
            gun[9].type = 402;
            gun[10].type = 402;
            gun[11].type = 104;
            gun[12].type = 401;
            gun[13].type = 401;
            gun[14].type = 106;
            gun[15].type = 403;
            gun[16].type = 404;
            gun[17].type = 401;
            gun[18].type = 403;
            gun[19].type = 402;
            gun[20].type = 104;
            gun[21].type = 102;
            gun[22].type = 101;
            gun[23].type = 601;
            gun[24].type = 601;
            gun[25].type = 101;
            gun[26].type = 601;
            gun[27].type = 304;
            gun[28].type = 403;
            gun[29].type = 102;
            gun[30].type = 601;
            gun[31].type = 301;
            gun[32].type = 601;
            gun[33].type = 601;
            gun[34].type = 601;
            gun[35].type = 132;
            gun[36].type = 133;
            gun[37].type = 405;
            gun[38].type = 601;
            gun[39].type = 101;
            gun[40].type = 101;
            gun[41].type = 302;
            gun[42].type = 131;
            gun[43].type = 601;
            gun[44].type = 203;
            gun[45].type = 231;
            gun[46].type = 1;
            gun[47].type = 1;
            gun[48].type = 4;
            gun[49].type = 201;
            gun[50].type = 233;
            gun[51].type = 2;
            gun[52].type = 2;
            gun[53].type = 34;
            gun[54].type = 31;
            gun[55].type = 402;
            gun[56].type = 1;
            gun[57].type = 405;
            gun[58].type = 2;
            gun[59].type = 405;
            gun[60].type = 403;
            gun[61].type = 4;
            gun[62].type = 2;
            gun[63].type = 204;
            gun[64].type = 202;
            gun[65].type = 102;
            gun[66].type = 101;
            gun[67].type = 101;
            gun[68].type = 501;
            gun[69].type = 501;
            gun[70].type = 503;
            gun[71].type = 304;
            gun[72].type = 101;
            gun[73].type = 503;
            gun[74].type = 102;
            gun[75].type = 501;
            gun[76].type = 501;
            gun[77].type = 132;
            gun[78].type = 333;
            gun[79].type = 501;
            gun[80].type = 502;
            gun[81].type = 502;
            gun[82].type = 131;
            gun[83].type = 101;
            gun[84].type = 101;
            gun[85].type = 101;
            gun[86].type = 103;
            gun[87].type = 101;
            gun[88].type = 103;
            gun[89].type = 101;
            gun[90].type = 103;
            gun[91].type = 103;
            gun[92].type = 133;
            gun[93].type = 101;
            gun[94].type = 303;
            gun[95].type = 104;
            gun[96].type = 301;
            gun[97].type = 302;
            gun[98].type = 104;
            gun[99].type = 103;
            gun[100].type = 1;

            gun[0].probability = 20;
            gun[1].probability = 20;
            gun[2].probability = 30;
            gun[3].probability = 30;
            gun[4].probability = 28;
            gun[5].probability = 18;
            gun[6].probability = 40;
            gun[7].probability = 28;
            gun[8].probability = 22;
            gun[9].probability = 40;
            gun[10].probability = 35;
            gun[11].probability = 20;
            gun[12].probability = 20;
            gun[13].probability = 18;
            gun[14].probability = 24;
            gun[15].probability = 30;
            gun[16].probability = 25;
            gun[17].probability = 18;
            gun[18].probability = 30;
            gun[19].probability = 35;
            gun[20].probability = 25;
            gun[21].probability = 40;
            gun[22].probability = 24;
            gun[23].probability = 18;
            gun[24].probability = 24;
            gun[25].probability = 24;
            gun[26].probability = 20;
            gun[27].probability = 36;
            gun[28].probability = 30;
            gun[29].probability = 40;
            gun[30].probability = 20;
            gun[31].probability = 40;
            gun[32].probability = 18;
            gun[33].probability = 18;
            gun[34].probability = 24;
            gun[35].probability = 55;
            gun[36].probability = 40;
            gun[37].probability = 40;
            gun[38].probability = 20;
            gun[39].probability = 36;
            gun[40].probability = 40;
            gun[41].probability = 40;
            gun[42].probability = 36;
            gun[43].probability = 24;
            gun[44].probability = 40;
            gun[45].probability = 40;
            gun[46].probability = 35;
            gun[47].probability = 35;
            gun[48].probability = 32;
            gun[49].probability = 24;
            gun[50].probability = 50;
            gun[51].probability = 35;
            gun[52].probability = 40;
            gun[53].probability = 45;
            gun[54].probability = 35;
            gun[55].probability = 35;
            gun[56].probability = 35;
            gun[57].probability = 40;
            gun[58].probability = 35;
            gun[59].probability = 40;
            gun[60].probability = 30;
            gun[61].probability = 30;
            gun[62].probability = 40;
            gun[63].probability = 40;
            gun[64].probability = 24;
            gun[65].probability = 36;
            gun[66].probability = 36;
            gun[67].probability = 40;
            gun[68].probability = 16;
            gun[69].probability = 16;
            gun[70].probability = 30;
            gun[71].probability = 40;
            gun[72].probability = 40;
            gun[73].probability = 30;
            gun[74].probability = 36;
            gun[75].probability = 30;
            gun[76].probability = 30;
            gun[77].probability = 36;
            gun[78].probability = 50;
            gun[79].probability = 16;
            gun[80].probability = 30;
            gun[81].probability = 30;
            gun[82].probability = 30;
            gun[83].probability = 32;
            gun[84].probability = 30;
            gun[85].probability = 30;
            gun[86].probability = 32;
            gun[87].probability = 30;
            gun[88].probability = 32;
            gun[89].probability = 30;
            gun[90].probability = 32;
            gun[91].probability = 30;
            gun[92].probability = 30;
            gun[93].probability = 30;
            gun[94].probability = 30;
            gun[95].probability = 32;
            gun[96].probability = 40;
            gun[97].probability = 42;
            gun[98].probability = 30;
            gun[99].probability = 30;
            gun[100].probability = 40;

            gun[0].skilleffect1 = 2;
            gun[1].skilleffect1 = 1.6;
            gun[2].skilleffect1 = 1.8;
            gun[3].skilleffect1 = 1.6;
            gun[4].skilleffect1 = 1.5;
            gun[5].skilleffect1 = 1.6;
            gun[6].skilleffect1 = 60;
            gun[7].skilleffect1 = 1.5;
            gun[8].skilleffect1 = 200;
            gun[9].skilleffect1 = 30;
            gun[10].skilleffect1 = 30;
            gun[11].skilleffect1 = 200;
            gun[12].skilleffect1 = 1.6;
            gun[13].skilleffect1 = 1.6;
            gun[14].skilleffect1 = 1.5;
            gun[15].skilleffect1 = 1.5;
            gun[16].skilleffect1 = 1.5;
            gun[17].skilleffect1 = 1.6;
            gun[18].skilleffect1 = 1.5;
            gun[19].skilleffect1 = 30;
            gun[20].skilleffect1 = 200;
            gun[21].skilleffect1 = 40;
            gun[22].skilleffect1 = 60;
            gun[23].skilleffect1 = 1;
            gun[24].skilleffect1 = 2.4;
            gun[25].skilleffect1 = 65;
            gun[26].skilleffect1 = 0.8;
            gun[27].skilleffect1 = 60;
            gun[28].skilleffect1 = 1.8;
            gun[29].skilleffect1 = 36;
            gun[30].skilleffect1 = 0.8;
            gun[31].skilleffect1 = 60;
            gun[32].skilleffect1 = 0.8;
            gun[33].skilleffect1 = 0.8;
            gun[34].skilleffect1 = 2.2;
            gun[35].skilleffect1 = 32;
            gun[36].skilleffect1 = 200;
            gun[37].skilleffect1 = 50;
            gun[38].skilleffect1 = 2.2;
            gun[39].skilleffect1 = 100;
            gun[40].skilleffect1 = 100;
            gun[41].skilleffect1 = 60;
            gun[42].skilleffect1 = 100;
            gun[43].skilleffect1 = 2.2;
            gun[44].skilleffect1 = 30;
            gun[45].skilleffect1 = 30;
            gun[46].skilleffect1 = 12;
            gun[47].skilleffect1 = 15;
            gun[48].skilleffect1 = 30;
            gun[49].skilleffect1 = 28;
            gun[50].skilleffect1 = 40;
            gun[51].skilleffect1 = 6;
            gun[52].skilleffect1 = 10;
            gun[53].skilleffect1 = 40;
            gun[54].skilleffect1 = 12;
            gun[55].skilleffect1 = 30;
            gun[56].skilleffect1 = 10;
            gun[57].skilleffect1 = 55;
            gun[58].skilleffect1 = 15;
            gun[59].skilleffect1 = 50;
            gun[60].skilleffect1 = 1.6;
            gun[61].skilleffect1 = 18;
            gun[62].skilleffect1 = 10;
            gun[63].skilleffect1 = 40;
            gun[64].skilleffect1 = 25;
            gun[65].skilleffect1 = 40;
            gun[66].skilleffect1 = 100;
            gun[67].skilleffect1 = 120;
            gun[68].skilleffect1 = 4;
            gun[69].skilleffect1 = 3.5;
            gun[70].skilleffect1 = 2;
            gun[71].skilleffect1 = 60;
            gun[72].skilleffect1 = 100;
            gun[73].skilleffect1 = 2.2;
            gun[74].skilleffect1 = 40;
            gun[75].skilleffect1 = 2.2;
            gun[76].skilleffect1 = 2.2;
            gun[77].skilleffect1 = 50;
            gun[78].skilleffect1 = 60;
            gun[79].skilleffect1 = 4;
            gun[80].skilleffect1 = 2.4;
            gun[81].skilleffect1 = 2.4;
            gun[82].skilleffect1 = 50;
            gun[83].skilleffect1 = 30;
            gun[84].skilleffect1 = 30;
            gun[85].skilleffect1 = 30;
            gun[86].skilleffect1 = 100;
            gun[87].skilleffect1 = 30;
            gun[88].skilleffect1 = 120;
            gun[89].skilleffect1 = 28;
            gun[90].skilleffect1 = 100;
            gun[91].skilleffect1 = 100;
            gun[92].skilleffect1 = 150;
            gun[93].skilleffect1 = 25;
            gun[94].skilleffect1 = 60;
            gun[95].skilleffect1 = 200;
            gun[96].skilleffect1 = 60;
            gun[97].skilleffect1 = 60;
            gun[98].skilleffect1 = 200;
            gun[99].skilleffect1 = 120;
            gun[100].skilleffect1 = 10;
            gun[0].skilleffect2 = 0;
            gun[1].skilleffect2 = 2.5;
            gun[2].skilleffect2 = 2.5;
            gun[3].skilleffect2 = 0.5;
            gun[4].skilleffect2 = 0.5;
            gun[5].skilleffect2 = 2.5;
            gun[6].skilleffect2 = 3;
            gun[7].skilleffect2 = 0.5;
            gun[8].skilleffect2 = 3;
            gun[9].skilleffect2 = 20;
            gun[10].skilleffect2 = 20;
            gun[11].skilleffect2 = 3;
            gun[12].skilleffect2 = 2.5;
            gun[13].skilleffect2 = 2.5;
            gun[14].skilleffect2 = 0;
            gun[15].skilleffect2 = 2.5;
            gun[16].skilleffect2 = 0.5;
            gun[17].skilleffect2 = 2.5;
            gun[18].skilleffect2 = 2.5;
            gun[19].skilleffect2 = 25;
            gun[20].skilleffect2 = 3;
            gun[21].skilleffect2 = 5;
            gun[22].skilleffect2 = 6;
            gun[23].skilleffect2 = 4;
            gun[24].skilleffect2 = 1;
            gun[25].skilleffect2 = 6;
            gun[26].skilleffect2 = 4;
            gun[27].skilleffect2 = 8;
            gun[28].skilleffect2 = 2.5;
            gun[29].skilleffect2 = 6;
            gun[30].skilleffect2 = 4;
            gun[31].skilleffect2 = 2.8;
            gun[32].skilleffect2 = 4;
            gun[33].skilleffect2 = 4;
            gun[34].skilleffect2 = 1;
            gun[35].skilleffect2 = 10;
            gun[36].skilleffect2 = 8;
            gun[37].skilleffect2 = 8;
            gun[38].skilleffect2 = 1;
            gun[39].skilleffect2 = 2;
            gun[40].skilleffect2 = 2;
            gun[41].skilleffect2 = 2.5;
            gun[42].skilleffect2 = 5;
            gun[43].skilleffect2 = 1;
            gun[44].skilleffect2 = 4;
            gun[45].skilleffect2 = 3;
            gun[46].skilleffect2 = 5;
            gun[47].skilleffect2 = 5;
            gun[48].skilleffect2 = 5;
            gun[49].skilleffect2 = 3;
            gun[50].skilleffect2 = 4;
            gun[51].skilleffect2 = 10;
            gun[52].skilleffect2 = 5;
            gun[53].skilleffect2 = 8;
            gun[54].skilleffect2 = 10;
            gun[55].skilleffect2 = 20;
            gun[56].skilleffect2 = 5;
            gun[57].skilleffect2 = 8;
            gun[58].skilleffect2 = 5;
            gun[59].skilleffect2 = 8;
            gun[60].skilleffect2 = 2.5;
            gun[61].skilleffect2 = 10;
            gun[62].skilleffect2 = 5;
            gun[63].skilleffect2 = 4;
            gun[64].skilleffect2 = 3;
            gun[65].skilleffect2 = 5;
            gun[66].skilleffect2 = 2;
            gun[67].skilleffect2 = 2;
            gun[68].skilleffect2 = 2;
            gun[69].skilleffect2 = 2;
            gun[70].skilleffect2 = 2;
            gun[71].skilleffect2 = 8;
            gun[72].skilleffect2 = 2;
            gun[73].skilleffect2 = 2;
            gun[74].skilleffect2 = 5;
            gun[75].skilleffect2 = 2;
            gun[76].skilleffect2 = 2;
            gun[77].skilleffect2 = 8;
            gun[78].skilleffect2 = 5;
            gun[79].skilleffect2 = 2;
            gun[80].skilleffect2 = 2;
            gun[81].skilleffect2 = 2;
            gun[82].skilleffect2 = 8;
            gun[83].skilleffect2 = 8;
            gun[84].skilleffect2 = 8;
            gun[85].skilleffect2 = 8;
            gun[86].skilleffect2 = 8;
            gun[87].skilleffect2 = 8;
            gun[88].skilleffect2 = 8;
            gun[89].skilleffect2 = 8;
            gun[90].skilleffect2 = 8;
            gun[91].skilleffect2 = 8;
            gun[92].skilleffect2 = 10;
            gun[93].skilleffect2 = 8;
            gun[94].skilleffect2 = 5;
            gun[95].skilleffect2 = 4;
            gun[96].skilleffect2 = 2.5;
            gun[97].skilleffect2 = 2.5;
            gun[98].skilleffect2 = 4;
            gun[99].skilleffect2 = 8;
            gun[100].skilleffect2 = 5;
            gun[0].skilleffect3 = 0;
            gun[1].skilleffect3 = 0;
            gun[2].skilleffect3 = 0;
            gun[3].skilleffect3 = 2;
            gun[4].skilleffect3 = 2;
            gun[5].skilleffect3 = 0;
            gun[6].skilleffect3 = 0;
            gun[7].skilleffect3 = 2;
            gun[8].skilleffect3 = 0;
            gun[9].skilleffect3 = 2.5;
            gun[10].skilleffect3 = 2.5;
            gun[11].skilleffect3 = 0;
            gun[12].skilleffect3 = 0;
            gun[13].skilleffect3 = 0;
            gun[14].skilleffect3 = 0;
            gun[15].skilleffect3 = 0;
            gun[16].skilleffect3 = 2;
            gun[17].skilleffect3 = 0;
            gun[18].skilleffect3 = 0;
            gun[19].skilleffect3 = 2.5;
            gun[20].skilleffect3 = 0;
            gun[21].skilleffect3 = 0;
            gun[22].skilleffect3 = 0;
            gun[23].skilleffect3 = 0;
            gun[24].skilleffect3 = 0;
            gun[25].skilleffect3 = 0;
            gun[26].skilleffect3 = 0;
            gun[27].skilleffect3 = 0;
            gun[28].skilleffect3 = 0;
            gun[29].skilleffect3 = 0;
            gun[30].skilleffect3 = 0;
            gun[31].skilleffect3 = 0;
            gun[32].skilleffect3 = 0;
            gun[33].skilleffect3 = 0;
            gun[34].skilleffect3 = 0;
            gun[35].skilleffect3 = 0;
            gun[36].skilleffect3 = 0;
            gun[37].skilleffect3 = 0;
            gun[38].skilleffect3 = 0;
            gun[39].skilleffect3 = 0;
            gun[40].skilleffect3 = 0;
            gun[41].skilleffect3 = 0;
            gun[42].skilleffect3 = 0;
            gun[43].skilleffect3 = 0;
            gun[44].skilleffect3 = 0;
            gun[45].skilleffect3 = 0;
            gun[46].skilleffect3 = 0;
            gun[47].skilleffect3 = 0;
            gun[48].skilleffect3 = 0;
            gun[49].skilleffect3 = 0;
            gun[50].skilleffect3 = 0;
            gun[51].skilleffect3 = 0;
            gun[52].skilleffect3 = 0;
            gun[53].skilleffect3 = 0;
            gun[54].skilleffect3 = 0;
            gun[55].skilleffect3 = 2.5;
            gun[56].skilleffect3 = 0;
            gun[57].skilleffect3 = 0;
            gun[58].skilleffect3 = 0;
            gun[59].skilleffect3 = 0;
            gun[60].skilleffect3 = 0;
            gun[61].skilleffect3 = 0;
            gun[62].skilleffect3 = 0;
            gun[63].skilleffect3 = 0;
            gun[64].skilleffect3 = 0;
            gun[65].skilleffect3 = 0;
            gun[66].skilleffect3 = 0;
            gun[67].skilleffect3 = 0;
            gun[68].skilleffect3 = 0;
            gun[69].skilleffect3 = 0;
            gun[70].skilleffect3 = 0;
            gun[71].skilleffect3 = 0;
            gun[72].skilleffect3 = 0;
            gun[73].skilleffect3 = 0;
            gun[74].skilleffect3 = 0;
            gun[75].skilleffect3 = 0;
            gun[76].skilleffect3 = 0;
            gun[77].skilleffect3 = 0;
            gun[78].skilleffect3 = 0;
            gun[79].skilleffect3 = 0;
            gun[80].skilleffect3 = 0;
            gun[81].skilleffect3 = 0;
            gun[82].skilleffect3 = 0;
            gun[83].skilleffect3 = 0;
            gun[84].skilleffect3 = 0;
            gun[85].skilleffect3 = 0;
            gun[86].skilleffect3 = 0;
            gun[87].skilleffect3 = 0;
            gun[88].skilleffect3 = 0;
            gun[89].skilleffect3 = 0;
            gun[90].skilleffect3 = 0;
            gun[91].skilleffect3 = 0;
            gun[92].skilleffect3 = 0;
            gun[93].skilleffect3 = 0;
            gun[94].skilleffect3 = 0;
            gun[95].skilleffect3 = 0;
            gun[96].skilleffect3 = 0;
            gun[97].skilleffect3 = 0;
            gun[98].skilleffect3 = 0;
            gun[99].skilleffect3 = 0;
            gun[100].skilleffect3 = 0;
            gun[0].skilleffect4 = 0;
            gun[1].skilleffect4 = 0;
            gun[2].skilleffect4 = 0;
            gun[3].skilleffect4 = 1;
            gun[4].skilleffect4 = 1;
            gun[5].skilleffect4 = 0;
            gun[6].skilleffect4 = 0;
            gun[7].skilleffect4 = 1;
            gun[8].skilleffect4 = 0;
            gun[9].skilleffect4 = 2.5;
            gun[10].skilleffect4 = 2.5;
            gun[11].skilleffect4 = 0;
            gun[12].skilleffect4 = 0;
            gun[13].skilleffect4 = 0;
            gun[14].skilleffect4 = 0;
            gun[15].skilleffect4 = 0;
            gun[16].skilleffect4 = 1;
            gun[17].skilleffect4 = 0;
            gun[18].skilleffect4 = 0;
            gun[19].skilleffect4 = 2.5;
            gun[20].skilleffect4 = 0;
            gun[21].skilleffect4 = 0;
            gun[22].skilleffect4 = 0;
            gun[23].skilleffect4 = 0;
            gun[24].skilleffect4 = 0;
            gun[25].skilleffect4 = 0;
            gun[26].skilleffect4 = 0;
            gun[27].skilleffect4 = 0;
            gun[28].skilleffect4 = 0;
            gun[29].skilleffect4 = 0;
            gun[30].skilleffect4 = 0;
            gun[31].skilleffect4 = 0;
            gun[32].skilleffect4 = 0;
            gun[33].skilleffect4 = 0;
            gun[34].skilleffect4 = 0;
            gun[35].skilleffect4 = 0;
            gun[36].skilleffect4 = 0;
            gun[37].skilleffect4 = 0;
            gun[38].skilleffect4 = 0;
            gun[39].skilleffect4 = 0;
            gun[40].skilleffect4 = 0;
            gun[41].skilleffect4 = 0;
            gun[42].skilleffect4 = 0;
            gun[43].skilleffect4 = 0;
            gun[44].skilleffect4 = 0;
            gun[45].skilleffect4 = 0;
            gun[46].skilleffect4 = 0;
            gun[47].skilleffect4 = 0;
            gun[48].skilleffect4 = 0;
            gun[49].skilleffect4 = 0;
            gun[50].skilleffect4 = 0;
            gun[51].skilleffect4 = 0;
            gun[52].skilleffect4 = 0;
            gun[53].skilleffect4 = 0;
            gun[54].skilleffect4 = 0;
            gun[55].skilleffect4 = 2.5;
            gun[56].skilleffect4 = 0;
            gun[57].skilleffect4 = 0;
            gun[58].skilleffect4 = 0;
            gun[59].skilleffect4 = 0;
            gun[60].skilleffect4 = 0;
            gun[61].skilleffect4 = 0;
            gun[62].skilleffect4 = 0;
            gun[63].skilleffect4 = 0;
            gun[64].skilleffect4 = 0;
            gun[65].skilleffect4 = 0;
            gun[66].skilleffect4 = 0;
            gun[67].skilleffect4 = 0;
            gun[68].skilleffect4 = 0;
            gun[69].skilleffect4 = 0;
            gun[70].skilleffect4 = 0;
            gun[71].skilleffect4 = 0;
            gun[72].skilleffect4 = 0;
            gun[73].skilleffect4 = 0;
            gun[74].skilleffect4 = 0;
            gun[75].skilleffect4 = 0;
            gun[76].skilleffect4 = 0;
            gun[77].skilleffect4 = 0;
            gun[78].skilleffect4 = 0;
            gun[79].skilleffect4 = 0;
            gun[80].skilleffect4 = 0;
            gun[81].skilleffect4 = 0;
            gun[82].skilleffect4 = 0;
            gun[83].skilleffect4 = 0;
            gun[84].skilleffect4 = 0;
            gun[85].skilleffect4 = 0;
            gun[86].skilleffect4 = 0;
            gun[87].skilleffect4 = 0;
            gun[88].skilleffect4 = 0;
            gun[89].skilleffect4 = 0;
            gun[90].skilleffect4 = 0;
            gun[91].skilleffect4 = 0;
            gun[92].skilleffect4 = 0;
            gun[93].skilleffect4 = 0;
            gun[94].skilleffect4 = 0;
            gun[95].skilleffect4 = 0;
            gun[96].skilleffect4 = 0;
            gun[97].skilleffect4 = 0;
            gun[98].skilleffect4 = 0;
            gun[99].skilleffect4 = 0;
            gun[100].skilleffect4 = 0;

            gun[0].growth = 1;
            gun[1].growth = 1.5;
            gun[2].growth = 1;
            gun[3].growth = 1;
            gun[4].growth = 1;
            gun[5].growth = 1.5;
            gun[6].growth = 0.5;
            gun[7].growth = 1;
            gun[8].growth = 0.8;
            gun[9].growth = 0.6;
            gun[10].growth = 0.6;
            gun[11].growth = 0.8;
            gun[12].growth = 1.5;
            gun[13].growth = 1.5;
            gun[14].growth = 1;
            gun[15].growth = 1;
            gun[16].growth = 1;
            gun[17].growth = 1.5;
            gun[18].growth = 1;
            gun[19].growth = 0.6;
            gun[20].growth = 0.8;
            gun[21].growth = 0.6;
            gun[22].growth = 0.6;
            gun[23].growth = 1.5;
            gun[24].growth = 1.5;
            gun[25].growth = 0.6;
            gun[26].growth = 1.5;
            gun[27].growth = 0.6;
            gun[28].growth = 1;
            gun[29].growth = 0.6;
            gun[30].growth = 1.5;
            gun[31].growth = 0.6;
            gun[32].growth = 1.5;
            gun[33].growth = 1.5;
            gun[34].growth = 1.5;
            gun[35].growth = 0.6;
            gun[36].growth = 1.2;
            gun[37].growth = 0.8;
            gun[38].growth = 1.5;
            gun[39].growth = 0.6;
            gun[40].growth = 0.6;
            gun[41].growth = 0.6;
            gun[42].growth = 0.6;
            gun[43].growth = 1.5;
            gun[44].growth = 0.5;
            gun[45].growth = 0.6;
            gun[46].growth = 0.6;
            gun[47].growth = 0.6;
            gun[48].growth = 0.6;
            gun[49].growth = 0.6;
            gun[50].growth = 0.5;
            gun[51].growth = 0.6;
            gun[52].growth = 0.6;
            gun[53].growth = 0.8;
            gun[54].growth = 0.6;
            gun[55].growth = 0.6;
            gun[56].growth = 0.6;
            gun[57].growth = 0.8;
            gun[58].growth = 0.6;
            gun[59].growth = 0.8;
            gun[60].growth = 1;
            gun[61].growth = 0.6;
            gun[62].growth = 0.6;
            gun[63].growth = 0.8;
            gun[64].growth = 0.6;
            gun[65].growth = 0.6;
            gun[66].growth = 0.6;
            gun[67].growth = 0.6;
            gun[68].growth = 1.2;
            gun[69].growth = 1.2;
            gun[70].growth = 1.2;
            gun[71].growth = 0.6;
            gun[72].growth = 0.6;
            gun[73].growth = 1.2;
            gun[74].growth = 0.6;
            gun[75].growth = 1.2;
            gun[76].growth = 1.2;
            gun[77].growth = 0.6;
            gun[78].growth = 0.5;
            gun[79].growth = 1.2;
            gun[80].growth = 1.2;
            gun[81].growth = 1.2;
            gun[82].growth = 1.2;
            gun[83].growth = 1.2;
            gun[84].growth = 1.2;
            gun[85].growth = 1.2;
            gun[86].growth = 2;
            gun[87].growth = 1.2;
            gun[88].growth = 2;
            gun[89].growth = 1.2;
            gun[90].growth = 1.5;
            gun[91].growth = 2;
            gun[92].growth = 2;
            gun[93].growth = 1.2;
            gun[94].growth = 0.5;
            gun[95].growth = 0.8;
            gun[96].growth = 0.6;
            gun[97].growth = 0.6;
            gun[98].growth = 0.8;
            gun[99].growth = 2;
            gun[100].growth = 0.6;

            gun[0].growth_type = 2;
            gun[1].growth_type = 2;
            gun[2].growth_type = 2;
            gun[3].growth_type = 4;
            gun[4].growth_type = 4;
            gun[5].growth_type = 2;
            gun[6].growth_type = 3;
            gun[7].growth_type = 4;
            gun[8].growth_type = 3;
            gun[9].growth_type = 4;
            gun[10].growth_type = 4;
            gun[11].growth_type = 3;
            gun[12].growth_type = 2;
            gun[13].growth_type = 2;
            gun[14].growth_type = 2;
            gun[15].growth_type = 2;
            gun[16].growth_type = 4;
            gun[17].growth_type = 2;
            gun[18].growth_type = 2;
            gun[19].growth_type = 4;
            gun[20].growth_type = 3;
            gun[21].growth_type = 3;
            gun[22].growth_type = 3;
            gun[23].growth_type = 2;
            gun[24].growth_type = 2;
            gun[25].growth_type = 3;
            gun[26].growth_type = 2;
            gun[27].growth_type = 3;
            gun[28].growth_type = 2;
            gun[29].growth_type = 3;
            gun[30].growth_type = 2;
            gun[31].growth_type = 3;
            gun[32].growth_type = 2;
            gun[33].growth_type = 2;
            gun[34].growth_type = 2;
            gun[35].growth_type = 3;
            gun[36].growth_type = 3;
            gun[37].growth_type = 3;
            gun[38].growth_type = 2;
            gun[39].growth_type = 3;
            gun[40].growth_type = 3;
            gun[41].growth_type = 3;
            gun[42].growth_type = 3;
            gun[43].growth_type = 2;
            gun[44].growth_type = 3;
            gun[45].growth_type = 3;
            gun[46].growth_type = 3;
            gun[47].growth_type = 3;
            gun[48].growth_type = 3;
            gun[49].growth_type = 3;
            gun[50].growth_type = 3;
            gun[51].growth_type = 3;
            gun[52].growth_type = 3;
            gun[53].growth_type = 3;
            gun[54].growth_type = 3;
            gun[55].growth_type = 4;
            gun[56].growth_type = 3;
            gun[57].growth_type = 3;
            gun[58].growth_type = 3;
            gun[59].growth_type = 3;
            gun[60].growth_type = 2;
            gun[61].growth_type = 3;
            gun[62].growth_type = 3;
            gun[63].growth_type = 3;
            gun[64].growth_type = 3;
            gun[65].growth_type = 3;
            gun[66].growth_type = 3;
            gun[67].growth_type = 3;
            gun[68].growth_type = 2;
            gun[69].growth_type = 2;
            gun[70].growth_type = 2;
            gun[71].growth_type = 3;
            gun[72].growth_type = 3;
            gun[73].growth_type = 2;
            gun[74].growth_type = 3;
            gun[75].growth_type = 2;
            gun[76].growth_type = 2;
            gun[77].growth_type = 3;
            gun[78].growth_type = 3;
            gun[79].growth_type = 2;
            gun[80].growth_type = 2;
            gun[81].growth_type = 2;
            gun[82].growth_type = 2;
            gun[83].growth_type = 2;
            gun[84].growth_type = 2;
            gun[85].growth_type = 2;
            gun[86].growth_type = 2;
            gun[87].growth_type = 2;
            gun[88].growth_type = 2;
            gun[89].growth_type = 2;
            gun[90].growth_type = 2;
            gun[91].growth_type = 2;
            gun[92].growth_type = 2;
            gun[93].growth_type = 2;
            gun[94].growth_type = 3;
            gun[95].growth_type = 3;
            gun[96].growth_type = 3;
            gun[97].growth_type = 3;
            gun[98].growth_type = 3;
            gun[99].growth_type = 2;
            gun[100].growth_type = 3;

            for (int i = 0; i < EQUIP_NUMBER; i++)
                equip[i] = new Equip();
            equip[0].name = "IOP T1外骨骼"; equip[0].dodge = 8; equip[0].damage = -1; equip[0].type = 10; equip[0].tooltip = "回避+8,伤害-1"; equip[0].rank = 2;
            equip[1].name = "IOP T2外骨骼"; equip[1].dodge = 12; equip[1].damage = -3; equip[1].type = 10; equip[1].tooltip = "回避+12,伤害-3"; equip[1].rank = 3;
            equip[2].name = "IOP T3外骨骼"; equip[2].dodge = 16; equip[2].damage = -5; equip[2].type = 10; equip[2].tooltip = "回避+16,伤害-5"; equip[2].rank = 4;
            equip[3].name = "（顶配）IOP T4外骨骼"; equip[3].dodge = 25; equip[3].damage = -6; equip[3].type = 10; equip[3].tooltip = "回避+25,伤害-6"; equip[3].rank = 5;
            equip[4].name = "（常值）IOP T4外骨骼"; equip[4].dodge = 21; equip[4].damage = -7; equip[4].type = 10; equip[4].tooltip = "回避+21,伤害-7"; equip[4].rank = 5;
            equip[5].name = "M61穿甲弹"; equip[5].breakarmor = 35; equip[5].type = 5; equip[5].tooltip = "穿甲+35"; equip[5].rank = 2;
            equip[6].name = "M993穿甲弹"; equip[6].breakarmor = 50; equip[6].type = 5; equip[6].tooltip = "穿甲+50"; equip[6].rank = 3;
            equip[7].name = "Mk169穿甲弹"; equip[7].breakarmor = 65; equip[7].type = 5; equip[7].tooltip = "穿甲+65"; equip[7].rank = 4;
            equip[8].name = "（顶配）Mk211高爆穿甲弹"; equip[8].breakarmor = 80; equip[8].type = 5; equip[8].tooltip = "穿甲+80"; equip[8].rank = 5;
            equip[9].name = "（常值）Mk211高爆穿甲弹"; equip[9].breakarmor = 80; equip[9].type = 5; equip[9].tooltip = "穿甲+74";equip[9].rank = 5;
            equip[10].name = "JHP高速弹"; equip[10].damage = 1; equip[10].type = 8; equip[10].tooltip = "伤害+1"; equip[10].rank = 2;
            equip[11].name = "三星FMJ高速弹"; equip[11].damage = 2; equip[11].type = 8; equip[11].tooltip = "伤害+2"; equip[11].rank = 3;
            equip[12].name = "四星FMJ高速弹"; equip[12].damage = 4; equip[12].type = 8; equip[12].tooltip = "伤害+4"; equip[12].rank = 4;
            equip[13].name = "（顶配）HVAP高速弹"; equip[13].damage = 8; equip[13].type = 8; equip[13].tooltip = "伤害+8"; equip[13].rank = 5;
            equip[14].name = "（常值）HVAP高速弹"; equip[14].damage = 6; equip[14].type = 8; equip[14].tooltip = "伤害+6"; equip[14].rank = 5;
            equip[15].name = "光瞄 - BM 3-12X40"; equip[15].critup = 0.08; equip[15].type = 1; equip[15].tooltip = "暴击率+8%"; equip[15].rank = 2;
            equip[16].name = "光瞄 - LRA 2-12X50"; equip[16].critup = 0.12; equip[16].type = 1; equip[16].tooltip = "暴击率+12%"; equip[16].rank = 3;
            equip[17].name = "光瞄 - PSO-1"; equip[17].critup = 0.16; equip[17].type = 1; equip[17].tooltip = "暴击率+16%"; equip[17].rank = 4;
            equip[18].name = "（顶配）光瞄 - VFL 6-24X56"; equip[18].critup = 0.24; equip[18].type = 1; equip[18].tooltip = "暴击率+24%"; equip[18].rank = 5;
            equip[19].name = "（常值）光瞄 - VFL 6-24X56"; equip[19].critup = 0.2; equip[19].type = 1; equip[19].tooltip = "暴击率+20%";equip[19].rank = 5;
            equip[20].name = "全息 - EOT 506"; equip[20].hit = 1; equip[20].damage = 1; equip[20].shotspeed = -1; equip[20].type = 2; equip[20].tooltip = "命中+1,伤害+1,射速-1"; equip[20].rank = 2;
            equip[21].name = "全息 - EOT 512"; equip[21].hit = 2; equip[21].damage = 2; equip[21].shotspeed = -2; equip[21].type = 2; equip[21].tooltip = "命中+2,伤害+2,射速-2"; equip[21].rank = 3;
            equip[22].name = "全息 - EOT 516"; equip[22].hit = 5; equip[22].damage = 3; equip[22].shotspeed = -3; equip[22].type = 2; equip[22].tooltip = "命中+5,伤害+3,射速-3"; equip[22].rank = 4;
            equip[23].name = "（顶配）全息 - EOT 518"; equip[23].hit = 10; equip[23].damage = 6; equip[23].shotspeed = -4; equip[23].type = 2; equip[23].tooltip = "命中+10,伤害+6,射速-4"; equip[23].rank = 5;
            equip[24].name = "（常值）全息 - EOT 518"; equip[24].hit = 8; equip[24].damage = 5; equip[24].shotspeed = -5; equip[24].type = 2; equip[24].tooltip = "命中+8,伤害+5,射速-5"; equip[24].rank = 5;
            equip[25].name = "ACOG - AMP COMPM2"; equip[25].hit = 3; equip[25].shotspeed = -1; equip[25].type = 3; equip[25].tooltip = "命中+3,射速-1"; equip[25].rank = 2;
            equip[26].name = "ACOG - AMP COMPM4"; equip[26].hit = 6; equip[26].shotspeed = -1; equip[26].type = 3; equip[26].tooltip = "命中+6,射速-1"; equip[26].rank = 3;
            equip[27].name = "ACOG - COG M150"; equip[27].hit = 10; equip[27].shotspeed = -1; equip[27].type = 3; equip[27].tooltip = "命中+10,射速-1"; equip[27].rank = 4;
            equip[28].name = "（顶配）ACOG - ITI MARS"; equip[28].hit = 15; equip[28].shotspeed = -1; equip[28].type = 3; equip[28].tooltip = "命中+15,射速-1"; equip[28].rank = 5;
            equip[29].name = "（常值）ACOG - ITI MARS"; equip[29].hit = 12; equip[29].shotspeed = -2; equip[29].type = 3; equip[29].tooltip = "命中+12,射速-2"; equip[29].rank = 5;
            equip[31].name = "夜视 - PEQ-2"; equip[31].nightsee = 55; equip[31].type = 4; equip[31].tooltip = "夜战命中抵消55%"; equip[31].rank = 2;
            equip[32].name = "夜视 - PEQ-5"; equip[32].nightsee = 70; equip[32].type = 4; equip[32].tooltip = "夜战命中抵消70%"; equip[32].rank = 3;
            equip[33].name = "夜视 - PEQ-15"; equip[33].nightsee = 85; equip[33].type = 4; equip[33].tooltip = "夜战命中抵消85%"; equip[33].rank = 4;
            equip[34].name = "（顶配）夜视 - PEQ-16A"; equip[34].nightsee = 100; equip[34].type = 4; equip[34].tooltip = "夜战命中抵消100%"; equip[34].rank = 5;
            equip[35].name = "（常值）夜视 - PEQ-16A"; equip[35].nightsee = 93; equip[35].type = 4; equip[35].tooltip = "夜战命中抵消93%"; equip[35].rank = 5;
            equip[36].name = "16Lab次口径穿甲弹"; equip[36].breakarmor = 80; equip[36].type = 5; equip[36].tooltip = "穿甲+80"; equip[36].rank = 5;
            equip[37].name = "16Lab红外指示器"; equip[37].nightsee = 100; equip[37].type = 4; equip[37].tooltip = "夜战命中抵消100%"; equip[37].rank = 5;
            equip[30].name = " "; equip[30].type = 13; equip[30].rank = 2;
            equip[38].name = "IOP X1外骨骼"; equip[38].dodge = 3; equip[38].damage = 0; equip[38].type = 10; equip[38].tooltip = "回避+3"; equip[38].rank = 2;
            equip[39].name = "IOP X2外骨骼"; equip[39].dodge = 5; equip[39].damage = 0; equip[39].type = 10; equip[39].tooltip = "回避+5"; equip[39].rank = 3;
            equip[40].name = "IOP X3外骨骼"; equip[40].dodge = 7; equip[40].damage = 0; equip[40].type = 10; equip[40].tooltip = "回避+7"; equip[40].rank = 4;
            equip[41].name = "（顶配）IOP X4外骨骼"; equip[41].dodge = 12; equip[41].damage = 0; equip[41].type = 10; equip[41].tooltip = "回避+12"; equip[41].rank = 5;
            equip[42].name = "（常值）IOP X4外骨骼"; equip[42].dodge = 10; equip[42].damage = 0; equip[42].type = 10; equip[42].tooltip = "回避+10"; equip[42].rank = 5;
            equip[43].name = "（顶配）国家竞赛穿甲弹"; equip[43].shotspeed = 8; equip[43].breakarmor = 85; equip[43].type = 5; equip[43].tooltip = "射速+8,穿甲+85"; equip[43].rank = 5; equip[43].forwhat = 81;
            equip[44].name = "（常值）国家竞赛穿甲弹"; equip[44].shotspeed = 3; equip[44].breakarmor = 80; equip[44].type = 5; equip[44].tooltip = "射速+3,穿甲+80"; equip[44].rank = 5; equip[44].forwhat = 81;
            equip[45].name = "（顶配）.300BLK高速弹"; equip[45].damage = 12; equip[45].hit = -1; equip[45].type = 8; equip[45].tooltip = "伤害+12,命中-1"; equip[45].rank = 5; equip[45].forwhat = 29;
            equip[46].name = "（常值）.300BLK高速弹"; equip[46].damage = 10; equip[46].hit = -3; equip[46].type = 8; equip[46].tooltip = "伤害+10,命中-3"; equip[46].rank = 5; equip[46].forwhat = 29;
            equip[47].name = "（顶配）Titan火控芯片"; equip[47].damage = -2; equip[47].shotspeed = -1; equip[47].belt = 2; equip[47].type = 9; equip[47].tooltip = "伤害-2,射速-1,弹链+2"; equip[47].rank = 5; equip[47].forwhat = 84;
            equip[48].name = "（常值）Titan火控芯片"; equip[48].damage = -3; equip[48].shotspeed = -3; equip[48].belt = 1; equip[48].type = 9; equip[48].tooltip = "伤害-3,射速-3,弹链+1"; equip[48].rank = 5; equip[48].forwhat = 84;
                 

            gridlist[0].Add(buffGrid01); gridlist[0].Add(buffGrid02); gridlist[0].Add(buffGrid03); gridlist[0].Add(buffGrid04); gridlist[0].Add(buffGrid05); gridlist[0].Add(buffGrid06); gridlist[0].Add(buffGrid07); gridlist[0].Add(buffGrid08); gridlist[0].Add(buffGrid09);
            gridlist[1].Add(buffGrid11); gridlist[1].Add(buffGrid12); gridlist[1].Add(buffGrid13); gridlist[1].Add(buffGrid14); gridlist[1].Add(buffGrid15); gridlist[1].Add(buffGrid16); gridlist[1].Add(buffGrid17); gridlist[1].Add(buffGrid18); gridlist[1].Add(buffGrid19);
            gridlist[2].Add(buffGrid21); gridlist[2].Add(buffGrid22); gridlist[2].Add(buffGrid23); gridlist[2].Add(buffGrid24); gridlist[2].Add(buffGrid25); gridlist[2].Add(buffGrid26); gridlist[2].Add(buffGrid27); gridlist[2].Add(buffGrid28); gridlist[2].Add(buffGrid29);
            gridlist[3].Add(buffGrid31); gridlist[3].Add(buffGrid32); gridlist[3].Add(buffGrid33); gridlist[3].Add(buffGrid34); gridlist[3].Add(buffGrid35); gridlist[3].Add(buffGrid36); gridlist[3].Add(buffGrid37); gridlist[3].Add(buffGrid38); gridlist[3].Add(buffGrid39);
            gridlist[4].Add(buffGrid41); gridlist[4].Add(buffGrid42); gridlist[4].Add(buffGrid43); gridlist[4].Add(buffGrid44); gridlist[4].Add(buffGrid45); gridlist[4].Add(buffGrid46); gridlist[4].Add(buffGrid47); gridlist[4].Add(buffGrid48); gridlist[4].Add(buffGrid49);
            gridlist[5].Add(buffGrid51); gridlist[5].Add(buffGrid52); gridlist[5].Add(buffGrid53); gridlist[5].Add(buffGrid54); gridlist[5].Add(buffGrid55); gridlist[5].Add(buffGrid56); gridlist[5].Add(buffGrid57); gridlist[5].Add(buffGrid58); gridlist[5].Add(buffGrid59);
            gridlist[6].Add(buffGrid61); gridlist[6].Add(buffGrid62); gridlist[6].Add(buffGrid63); gridlist[6].Add(buffGrid64); gridlist[6].Add(buffGrid65); gridlist[6].Add(buffGrid66); gridlist[6].Add(buffGrid67); gridlist[6].Add(buffGrid68); gridlist[6].Add(buffGrid69);
            gridlist[7].Add(buffGrid71); gridlist[7].Add(buffGrid72); gridlist[7].Add(buffGrid73); gridlist[7].Add(buffGrid74); gridlist[7].Add(buffGrid75); gridlist[7].Add(buffGrid76); gridlist[7].Add(buffGrid77); gridlist[7].Add(buffGrid78); gridlist[7].Add(buffGrid79);
            gridlist[8].Add(buffGrid81); gridlist[8].Add(buffGrid82); gridlist[8].Add(buffGrid83); gridlist[8].Add(buffGrid84); gridlist[8].Add(buffGrid85); gridlist[8].Add(buffGrid86); gridlist[8].Add(buffGrid87); gridlist[8].Add(buffGrid88); gridlist[8].Add(buffGrid89);



            //检查更新
            string strGatherJsonUrl = "http://jyying.cn/snqxap/AssemblyInfo";
            //string strGatherJsonUrl = "http://www.google.com";
            string strContent = Get(strGatherJsonUrl);
            string Pattren = @"(\d\.){3}\d";
            Regex regex = new Regex(Pattren);
            Match m = regex.Match(strContent);
            if(!m.Success||strContent.Length>10)
            {
                UpdateTb.Text = "无法获取服务器版本信息";
            }
            else if (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() == strContent)
            {
                UpdateTb.Text = "已是最新版";
            }
            else
                hyperLink.IsEnabled = true;

            innight = false;
        }
        /// <summary>
        /// 改变选取枪娘时计算光环
        /// </summary>
        /// <param name="nextselect">临近格所选枪娘index</param>
        /// <param name="select">该格所选枪娘index</param>
        /// <param name="grid">临近格在该格的哪个方向（电脑小键盘）</param>
        /// <param name="ggi">哪一格</param>
        public void othercombochange(int nextselect,int select,int grid,int ggi)
        {
            switch (gun[nextselect].number)
            {
                      
                case 1:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                            }
                        break;
                    }
                case 2:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect1 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        break;
                    }
                case 3:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect1 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect2 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        break;
                    }
                case 4:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect1 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect2 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect3 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        break;
                    }
                case 5:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect1 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect2 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect3 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect4 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        break;
                    }
                case 6:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect1 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect2 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect3 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect4 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect5 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        break;
                    }
                case 7:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect1 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect2 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect3 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect4 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect5 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect6 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        break;
                    }
                case 8:
                    {
                        if (gun[nextselect].effect0 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect1 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect2 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect3 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect4 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect5 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect6 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        else if (gun[nextselect].effect7 == grid && (gun[nextselect].to == gun[select].what || gun[nextselect].to == 1))
                        {
                            gg[ggi].critup += gun[nextselect].critup;
                            gg[ggi].damageup += gun[nextselect].damageup;
                            gg[ggi].dodgeup += gun[nextselect].dodgeup;
                            gg[ggi].shotspeedup += gun[nextselect].shotspeedup;
                            gg[ggi].hitup += gun[nextselect].hitup;
                            gg[ggi].rateup += gun[nextselect].rateup;
                        }
                        break;
                    }
                default:
                    break;
            }


        }
        /// <summary>
        /// 更新伤害指数
        /// </summary>
        /// <param name="comboi">哪一个九宫格</param>
        public void renewindex(int comboi)
        {
            int select = 0;
            switch(comboi)
            {
                case 0:
                    {
                        
                       select = Combo0.SelectedIndex;
                        int levelselect = Level0.SelectedIndex;
                        int skillselect = SkillLevel0.SelectedIndex;
                        calclevel(select, levelselect, 0,skillselect);                     
                        break;
                    }
                case 1:
                    {
                        select = Combo1.SelectedIndex;
                        int levelselect = Level1.SelectedIndex;
                        int skillselect = SkillLevel1.SelectedIndex;
                        calclevel(select, levelselect, 1,skillselect);
                        break;
                    }
                case 2:
                    {
                        select = Combo2.SelectedIndex;
                        int levelselect = Level2.SelectedIndex;
                        int skillselect = SkillLevel2.SelectedIndex;
                        calclevel(select, levelselect, 2,skillselect);
                        break;
                    }
                case 3:
                    {
                        select = Combo3.SelectedIndex;
                        int levelselect = Level3.SelectedIndex;
                        int skillselect = SkillLevel3.SelectedIndex;
                        calclevel(select, levelselect, 3,skillselect);
                        break;
                    }
                case 4:
                    {
                        select = Combo4.SelectedIndex;
                        int levelselect = Level4.SelectedIndex;
                        int skillselect = SkillLevel4.SelectedIndex;
                        calclevel(select, levelselect, 4,skillselect);
                        break;
                    }
                case 5:
                    {
                        select = Combo5.SelectedIndex;
                        int levelselect = Level5.SelectedIndex;
                        int skillselect = SkillLevel5.SelectedIndex;
                        calclevel(select, levelselect, 5,skillselect);
                        break;
                    }
                case 6:
                    {
                        select = Combo6.SelectedIndex;
                        int levelselect = Level6.SelectedIndex;
                        int skillselect = SkillLevel6.SelectedIndex;
                        calclevel(select, levelselect, 6,skillselect);
                        break;
                    }
                case 7:
                    {
                        select = Combo7.SelectedIndex;
                        int levelselect = Level7.SelectedIndex;
                        int skillselect = SkillLevel7.SelectedIndex;
                        calclevel(select, levelselect, 7,skillselect);
                        break;
                    }
                case 8:
                    {
                        select = Combo8.SelectedIndex;
                        int levelselect = Level8.SelectedIndex;
                        int skillselect = SkillLevel8.SelectedIndex;
                        calclevel(select, levelselect, 8,skillselect);
                        break;
                    }
                default:
                    break;
            }
        }
       /// <summary>
       /// 计算伤害指数
       /// </summary>
       /// <param name="shotspeed">射速</param>
       /// <param name="damage">伤害</param>
       /// <param name="crit">暴击</param>
       /// <param name="enemydodge">敌方回避</param>
       /// <param name="hit">命中</param>
       /// <param name="belt">弹链</param>
       /// <param name="combo">哪一格（目前没有用该参数）</param>
       /// <param name="damageagain">是否有二次伤害（对应突击者之眼）</param>
       /// <returns></returns>
        public double Index(double shotspeed,double damage,double crit,double enemydodge,double hit,int belt,int combo,double damageagain)
       {
            if (hit == 0)
                return 0;
            else if (belt == 0)
                return shotspeed * damage / 50 * (1 - crit + crit * 1.5) / (1 + enemydodge / hit) * damageagain;
            else
            {
                
                if(slider.Value!=0)
                {
                    double shottime = (double)belt * 0.4;
                    double roletime = (double)belt * 0.4 + 4 + 200 / shotspeed;
                    double shotdamage = belt * damage * (1 - crit + crit * 1.5) / (1 + enemydodge / hit);
                    if (slider.Value < shottime)
                        return (shotdamage / shottime);
                    else if (slider.Value < roletime)
                        return (shotdamage / slider.Value);
                    else if (slider.Value < (shottime + roletime))
                        return ((shotdamage + shotdamage / shottime * (slider.Value-roletime)) / slider.Value);
                    else if (slider.Value < 2 * roletime)
                        return 2 * shotdamage / slider.Value;
                    else if (slider.Value < 2 * roletime + 3 * shottime)
                        return ((2 * shotdamage + shotdamage / shottime * (slider.Value-2*roletime)) / slider.Value);
                    else
                        return 3 * shotdamage / slider.Value;
                }
                else
                    return 0;
            }
        }
        /// <summary>
        /// 得到该格枪娘名称
        /// </summary>
        /// <param name="combo">哪一格</param>
        /// <returns></returns>
        string getcombogunname(int combo)
        {
            switch(combo)
            {
                case 0:
                    {
                        if (Combo0.SelectedIndex != -1)
                            return gun[Combo0.SelectedIndex].name;
                        else
                            return "";
                    }
                case 1:
                    {
                        if (Combo1.SelectedIndex != -1)
                            return gun[Combo1.SelectedIndex].name;
                        else
                            return "";
                    }
                case 2:
                    {
                        if (Combo2.SelectedIndex != -1)
                            return gun[Combo2.SelectedIndex].name;
                        else
                            return "";
                    }
                case 3:
                    {
                        if (Combo3.SelectedIndex != -1)
                            return gun[Combo3.SelectedIndex].name;
                        else
                            return "";
                    }
                case 4:
                    {
                        if (Combo4.SelectedIndex != -1)
                            return gun[Combo4.SelectedIndex].name;
                        else
                            return "";
                    }
                case 5:
                    {
                        if (Combo5.SelectedIndex != -1)
                            return gun[Combo5.SelectedIndex].name;
                        else
                            return "";
                    }
                case 6:
                    {
                        if (Combo6.SelectedIndex != -1)
                            return gun[Combo6.SelectedIndex].name;
                        else
                            return "";
                    }
                case 7:
                    {
                        if (Combo7.SelectedIndex != -1)
                            return gun[Combo7.SelectedIndex].name;
                        else
                            return "";
                    }
                case 8:
                    {
                        if (Combo8.SelectedIndex != -1)
                            return gun[Combo8.SelectedIndex].name;
                        else
                            return "";
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// 计算左上格光环
        /// </summary>
        void calccombo0buff()
        {
         
            gg[0].cleargg();

            int index0 = Combo0.SelectedIndex;
            int index1 = Combo1.SelectedIndex;
            int index3 = Combo3.SelectedIndex;
            int index4 = Combo4.SelectedIndex;

            if (index0 == -1 || index0 == GUN_NUMBER)
            {
                Combo0.SelectedIndex = GUN_NUMBER;
                renewindex(0);
                return;
            }
            else
            {
                if(index1!=-1)
                {
                    othercombochange(index1, index0, 4, 0);
                }
                if(index3!=-1)
                {
                    othercombochange(index3, index0, 8, 0);
                }
                if(index4!=-1)
                {
                    othercombochange(index4, index0, 7, 0);
                }
                renewindex(0);
                if(rb0.IsChecked == true)
                    calctank(0);
                if (rbf0.IsChecked == true)
                    calcftank(0);
                return;
            }
        }
        /// <summary>
        /// 计算上格光环
        /// </summary>
        void calccombo1buff()
        {
       
            gg[1].cleargg();

            int index0 = Combo0.SelectedIndex;
            int index1 = Combo1.SelectedIndex;
            int index2 = Combo2.SelectedIndex;
            int index3 = Combo3.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index5 = Combo5.SelectedIndex;

            if (index1 == -1 || index1 == GUN_NUMBER)
            {
                Combo1.SelectedIndex = GUN_NUMBER;
                renewindex(1);
                return;
            }
            else
            {
                if (index0 != -1)
                {
                    othercombochange(index0, index1, 6, 1);
                }
                if (index2 != -1)
                {
                    othercombochange(index2, index1, 4, 1);
                }
                if (index3 != -1)
                {
                    othercombochange(index3, index1, 9, 1);
                }
                if (index4 != -1)
                {
                    othercombochange(index4, index1, 8, 1);
                }
                if (index5 != -1)
                {
                    othercombochange(index5, index1, 7, 1);
                }
                renewindex(1);
                if (rb1.IsChecked == true)
                    calctank(1);
                if (rbf1.IsChecked == true)
                    calcftank(1);
                return;
            }
        }
        /// <summary>
        /// 计算右上格光环
        /// </summary>
        void calccombo2buff()
        {
      
            gg[2].cleargg();

            int index2 = Combo2.SelectedIndex;
            int index1 = Combo1.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index5 = Combo5.SelectedIndex;

            if (index2 == -1 || index2 == GUN_NUMBER)
            {
                Combo2.SelectedIndex = GUN_NUMBER;
                renewindex(2);
                return;
            }
            else
            {
                if (index1 != -1)
                {
                    othercombochange(index1, index2, 6, 2);
                }
                if (index4 != -1)
                {
                    othercombochange(index4, index2, 9, 2);
                }
                if (index5 != -1)
                {
                    othercombochange(index5, index2, 8, 2);
                }
                renewindex(2);
                if (rb2.IsChecked == true)
                    calctank(2);
                if (rbf2.IsChecked == true)
                    calcftank(2);
                return;
            }
        }
        /// <summary>
        /// 计算左中格光环
        /// </summary>
        void calccombo3buff()
        {
   
            gg[3].cleargg();

            int index0 = Combo0.SelectedIndex;
            int index1 = Combo1.SelectedIndex;
            int index6 = Combo6.SelectedIndex;
            int index3 = Combo3.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index7 = Combo7.SelectedIndex;

            if (index3 == -1||index3 == GUN_NUMBER)
            {
                Combo3.SelectedIndex = GUN_NUMBER;
                renewindex(3);
                return;
            }
            else
            {
                if (index0 != -1)
                {
                    othercombochange(index0, index3, 2, 3);
                }
                if (index1 != -1)
                {
                    othercombochange(index1, index3, 1, 3);
                }
                if (index4 != -1)
                {
                    othercombochange(index4, index3, 4, 3);
                }
                if (index6 != -1)
                {
                    othercombochange(index6, index3, 8, 3);
                }
                if (index7 != -1)
                {
                    othercombochange(index7, index3, 7, 3);
                }
                renewindex(3);
                if (rb3.IsChecked == true)
                    calctank(3);
                if (rbf3.IsChecked == true)
                    calcftank(3);
                return;
            }
        }
        /// <summary>
        /// 计算中格光环
        /// </summary>
        void calccombo4buff()
        {

            gg[4].cleargg();

            int index0 = Combo0.SelectedIndex;
            int index1 = Combo1.SelectedIndex;
            int index2 = Combo2.SelectedIndex;
            int index3 = Combo3.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index5 = Combo5.SelectedIndex;
            int index6 = Combo6.SelectedIndex;
            int index7 = Combo7.SelectedIndex;
            int index8 = Combo8.SelectedIndex;

            if (index4 == -1 || index4 == GUN_NUMBER)
            {
                Combo4.SelectedIndex = GUN_NUMBER;
                renewindex(4);
                return;
            }
            else
            {
                if (index0 != -1)
                {
                    othercombochange(index0, index4, 3, 4);
                }
                if (index1 != -1)
                {
                    othercombochange(index1, index4, 2, 4);
                }
                if (index3 != -1)
                {
                    othercombochange(index3, index4, 6, 4);
                }
                if (index2 != -1)
                {
                    othercombochange(index2, index4, 1, 4);
                }
                if (index5 != -1)
                {
                    othercombochange(index5, index4, 4, 4);
                }
                if (index6 != -1)
                {
                    othercombochange(index6, index4, 9, 4);
                }
                if (index7 != -1)
                {
                    othercombochange(index7, index4, 8, 4);
                }
                if (index8 != -1)
                {
                    othercombochange(index8, index4, 7, 4);
                }
                renewindex(4);
                if (rb4.IsChecked == true)
                    calctank(4);
                if (rbf4.IsChecked == true)
                    calcftank(4);
                return;
            }
        }
        /// <summary>
        /// 计算右中格光环
        /// </summary>
        void calccombo5buff()
        {
            gg[5].cleargg();

            int index2 = Combo2.SelectedIndex;
            int index1 = Combo1.SelectedIndex;
            int index8 = Combo8.SelectedIndex;
            int index5 = Combo5.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index7 = Combo7.SelectedIndex;

            if (index5 == -1 || index5 == GUN_NUMBER)
            {
                Combo5.SelectedIndex = GUN_NUMBER;
                renewindex(5);
                return;
            }
            else
            {
                if (index1 != -1)
                {
                    othercombochange(index1, index5, 3, 5);
                }
                if (index2 != -1)
                {
                    othercombochange(index2, index5, 2, 5);
                }
                if (index4 != -1)
                {
                    othercombochange(index4, index5, 6, 5);
                }
                if (index7 != -1)
                {
                    othercombochange(index7, index5, 9, 5);
                }
                if (index8 != -1)
                {
                    othercombochange(index8, index5, 8, 5);
                }
                renewindex(5);
                if (rb5.IsChecked == true)
                    calctank(5);
                if (rbf5.IsChecked == true)
                    calcftank(5);
                return;
            }
        }
        /// <summary>
        /// 计算左下格光环
        /// </summary>
        void calccombo6buff()
        {
            gg[6].cleargg();

            int index6 = Combo6.SelectedIndex;
            int index7 = Combo7.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index3 = Combo3.SelectedIndex;

            if (index6 == -1 || index6 == GUN_NUMBER)
            {
                Combo6.SelectedIndex = GUN_NUMBER;
                renewindex(6);
                return;
            }
            else
            {
                if (index3 != -1)
                {
                    othercombochange(index3, index6, 2, 6);
                }
                if (index4 != -1)
                {
                    othercombochange(index4, index6, 1, 6);
                }
                if (index7 != -1)
                {
                    othercombochange(index7, index6, 4, 6);
                }
                renewindex(6);
                if (rb6.IsChecked == true)
                    calctank(6);
                if (rbf6.IsChecked == true)
                    calcftank(6);
                return;
            }
        }
        /// <summary>
        /// 计算下格光环
        /// </summary>
        void calccombo7buff()
        {
            gg[7].cleargg();

            int index3 = Combo3.SelectedIndex;
            int index6 = Combo6.SelectedIndex;
            int index8 = Combo8.SelectedIndex;
            int index5 = Combo5.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index7 = Combo7.SelectedIndex;

            if (index7 == -1 || index7 == GUN_NUMBER)
            {
                Combo7.SelectedIndex = GUN_NUMBER;
                renewindex(7);
                return;
            }
            else
            {
                if (index3 != -1)
                {
                    othercombochange(index3, index7, 3, 7);
                }
                if (index4 != -1)
                {
                    othercombochange(index4, index7, 2, 7);
                }
                if (index5 != -1)
                {
                    othercombochange(index5, index7, 1, 7);
                }
                if (index6 != -1)
                {
                    othercombochange(index6, index7, 6, 7);
                }
                if (index8 != -1)
                {
                    othercombochange(index8, index7, 4, 7);
                }
                renewindex(7);
                if (rb7.IsChecked == true)
                    calctank(7);
                if (rbf7.IsChecked == true)
                    calcftank(7);
                return;
            }
        }
        /// <summary>
        /// 计算右下格光环
        /// </summary>
        void calccombo8buff()
        {
            gg[8].cleargg();

            int index5 = Combo5.SelectedIndex;
            int index7 = Combo7.SelectedIndex;
            int index4 = Combo4.SelectedIndex;
            int index8 = Combo8.SelectedIndex;

            if (index8 == -1 || index8 == GUN_NUMBER)
            {
                Combo8.SelectedIndex = GUN_NUMBER;
                renewindex(8);
                return;
            }
            else
            {
                if (index4 != -1)
                {
                    othercombochange(index4, index8, 3, 8);
                }
                if (index5 != -1)
                {
                    othercombochange(index5, index8, 2, 8);
                }
                if (index7 != -1)
                {
                    othercombochange(index7, index8, 6, 8);
                }
                renewindex(8);
                if (rb8.IsChecked == true)
                    calctank(8);
                if (rbf8.IsChecked == true)
                    calcftank(8);
                return;
            }
        }



        /// <summary>
        /// 左上格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo0_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int select = Combo0.SelectedIndex;
            if (select == -1)
            {
                
                return;
            }
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != ""&&getcombogunname(i)!=null)
                    howmany++;
                if (getcombogunname(i) == gun[select].name&& getcombogunname(i) != "" && getcombogunname(i) != null && i != 0)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
           

                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo0.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(7) || gun[select].name == getcombogunname(8))
                {
                    Combo0.SelectedIndex = lastgunindex[0];
                    return;
                }
            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo0.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo0.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo0.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo0.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo0.Foreground = br;
            }
            if (select!=-1)
                calccombo0buff();
            calccombo1buff();
            calccombo2buff();
            calccombo3buff();
            calccombo4buff();
            calccombo5buff();
            calccombo6buff();
            calccombo7buff();
            calccombo8buff();
            lastgunindex[0] = select;

            renewskill();
            calceat();

            equipcb01.Items.Clear();
            equipcb01.ToolTip = null;
            equipcb01.IsEnabled = false;
            equipcb02.Items.Clear();
            equipcb02.ToolTip = null;
            equipcb02.IsEnabled = false;
            equipcb03.Items.Clear();
            equipcb03.ToolTip = null;
            equipcb03.IsEnabled = false;
            loadequipcb(select, Level0.SelectedIndex, 0);
        }
        /// <summary>
        /// 上格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo1.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                if (getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null && i != 1)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo1.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(7) || gun[select].name == getcombogunname(8))
                {
                    Combo1.SelectedIndex = lastgunindex[1];
                    return;
                }
            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo1.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo1.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo1.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo1.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo1.Foreground = br;
            }
            if (select != -1)
                calccombo1buff();
            calccombo0buff();
            calccombo2buff();
            calccombo3buff();
            calccombo4buff();
            calccombo5buff();
            calccombo6buff();
            calccombo7buff();
            calccombo8buff();
            lastgunindex[1] = select;
            renewskill();
            calceat();
            equipcb11.Items.Clear();
            equipcb11.IsEnabled = false;
            equipcb11.ToolTip = null;
            equipcb12.Items.Clear();
            equipcb12.IsEnabled = false;
            equipcb12.ToolTip = null;
            equipcb13.Items.Clear();
            equipcb13.IsEnabled = false;
            equipcb13.ToolTip = null;
            loadequipcb(select, Level1.SelectedIndex, 1);
        }
        /// <summary>
        /// 右上格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo2.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                if (getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null && i != 2)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo2.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(7) || gun[select].name == getcombogunname(8))
                {
                    Combo2.SelectedIndex = lastgunindex[2];
                    return;
                }
            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo2.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo2.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo2.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo2.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo2.Foreground = br;
            }
            if (select != -1)
                calccombo2buff();
            calccombo0buff();
            calccombo1buff();
            calccombo3buff();
            calccombo4buff();
            calccombo5buff();
            calccombo6buff();
            calccombo7buff();
            calccombo8buff();
            lastgunindex[2] = select;
            renewskill();
            calceat();
            equipcb21.Items.Clear();
            equipcb21.IsEnabled = false;
            equipcb21.ToolTip = null;
            equipcb22.Items.Clear();
            equipcb22.IsEnabled = false;
            equipcb22.ToolTip = null;
            equipcb23.Items.Clear();
            equipcb23.IsEnabled = false;
            equipcb23.ToolTip = null;
            loadequipcb(select, Level2.SelectedIndex, 2);
        }
        /// <summary>
        /// 左中格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo3.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                if (getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null && i != 3)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
           

                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo3.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(7) || gun[select].name == getcombogunname(8))
                {
                    Combo3.SelectedIndex = lastgunindex[3];
                    return;
                }

            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo3.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo3.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo3.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo3.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo3.Foreground = br;
            }
            if (select != -1)
                calccombo3buff();
            calccombo0buff();
            calccombo1buff();
            calccombo3buff();
            calccombo4buff();
            calccombo5buff();
            calccombo6buff();
            calccombo7buff();
            calccombo8buff();
            lastgunindex[3] = select;
/*
            cb3.IsChecked = false;
            if (isnightskill(gun[select].type) && cbIsnight.IsChecked == false)
                cb3.IsEnabled = false;
            else
                cb3.IsEnabled = true;*/

            renewskill();
            calceat();
            equipcb31.Items.Clear();
            equipcb31.IsEnabled = false;
            equipcb31.ToolTip = null;
            equipcb32.Items.Clear();
            equipcb32.IsEnabled = false;
            equipcb32.ToolTip = null;
            equipcb33.Items.Clear();
            equipcb33.IsEnabled = false;
            equipcb33.ToolTip = null;
            loadequipcb(select, Level3.SelectedIndex, 3);
        }

        /// <summary>
        /// 更新技能加成
        /// </summary>
        private void renewskill()
        {
            clearskill();
            calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
            calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
            calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
            calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
            calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
            calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
            calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
            calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
            calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            for (int i = 0; i < 9; i++)
                renewindex(i);
            renewtank();
        }
        /// <summary>
        /// 更新肉度
        /// </summary>
        private void renewtank()
        {
            if (rb0.IsChecked == true)
            {
                calctank(0);
            }
            else if (rb1.IsChecked == true)
            {
                calctank(1);
            }
            else if (rb2.IsChecked == true)
            {
                calctank(2);
            }
            else if (rb3.IsChecked == true)
            {
                calctank(3);
            }
            else if (rb4.IsChecked == true)
            {
                calctank(4);
            }
            else if (rb5.IsChecked == true)
            {
                calctank(5);
            }
            else if (rb6.IsChecked == true)
            {
                calctank(6);
            }
            else if (rb7.IsChecked == true)
            {
                calctank(7);
            }
            else if (rb7.IsChecked == true)
            {
                calctank(7);
            }
            else if (rb8.IsChecked == true)
            {
                calctank(8);
            }
            if (rbf0.IsChecked == true)
            {
                calcftank(0);
            }
            else if (rbf1.IsChecked == true)
            {
                calcftank(1);
            }
            else if (rbf2.IsChecked == true)
            {
                calcftank(2);
            }
            else if (rbf3.IsChecked == true)
            {
                calcftank(3);
            }
            else if (rbf4.IsChecked == true)
            {
                calcftank(4);
            }
            else if (rbf5.IsChecked == true)
            {
                calcftank(5);
            }
            else if (rbf6.IsChecked == true)
            {
                calcftank(6);
            }
            else if (rbf7.IsChecked == true)
            {
                calcftank(7);
            }
            else if (rbf7.IsChecked == true)
            {
                calcftank(7);
            }
            else if (rbf8.IsChecked == true)
            {
                calcftank(8);
            }
        }
        /// <summary>
        /// 中格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo4.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                if (getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null && i != 4)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
           

                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo4.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(7) || gun[select].name == getcombogunname(8))
                {
                    Combo4.SelectedIndex = lastgunindex[4];
                    return;
                }

            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo4.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo4.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo4.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo4.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo4.Foreground = br;
            }
            if (select != -1)
                calccombo4buff();
            calccombo0buff();
            calccombo1buff();
            calccombo2buff();
            calccombo3buff();
            calccombo5buff();
            calccombo6buff();
            calccombo7buff();
            calccombo8buff();

            lastgunindex[4] = select;
    /*        cb4.IsChecked = false;
            if (isnightskill(gun[select].type) && cbIsnight.IsChecked == false)
                cb4.IsEnabled = false;
            else
                cb4.IsEnabled = true;*/
            renewskill();
            calceat();
            equipcb41.Items.Clear();
            equipcb41.IsEnabled = false;
            equipcb41.ToolTip = null;
            equipcb42.Items.Clear();
            equipcb42.IsEnabled = false;
            equipcb42.ToolTip = null;
            equipcb43.Items.Clear();
            equipcb43.IsEnabled = false;
            equipcb43.ToolTip = null;
            loadequipcb(select, Level4.SelectedIndex, 4);
        }
        /// <summary>
        /// 右中格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo5.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                   if(getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null && i != 5)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo5.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(7) || gun[select].name == getcombogunname(8))
                {
                    Combo5.SelectedIndex = lastgunindex[5];
                    return;
                }

            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo5.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo5.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo5.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo5.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo5.Foreground = br;
            }
            if (select != -1)
                calccombo5buff();
            calccombo0buff();
            calccombo1buff();
            calccombo2buff();
            calccombo3buff();
            calccombo5buff();
            calccombo6buff();
            calccombo7buff();
            calccombo8buff();

            lastgunindex[5] = select;
   /*         cb5.IsChecked = false;
            if (isnightskill(gun[select].type) && cbIsnight.IsChecked == false)
                cb5.IsEnabled = false;
            else
                cb5.IsEnabled = true;*/
            renewskill();
            calceat();
            equipcb51.Items.Clear();
            equipcb51.IsEnabled = false;
            equipcb51.ToolTip = null;
            equipcb52.Items.Clear();
            equipcb52.IsEnabled = false;
            equipcb52.ToolTip = null;
            equipcb53.Items.Clear();
            equipcb53.IsEnabled = false;
            equipcb53.ToolTip = null;
            loadequipcb(select, Level5.SelectedIndex, 5);
        }
        /// <summary>
        /// 左下格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo6.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                if (getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null && i != 6)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
           

                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo6.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(7) || gun[select].name == getcombogunname(8))
                {
                    Combo6.SelectedIndex = lastgunindex[6];
                    return;
                }

            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo6.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo6.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo6.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo6.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo6.Foreground = br;
            }
            if (select != -1)
                calccombo6buff();
            calccombo0buff();
            calccombo1buff();
            calccombo2buff();
            calccombo3buff();
            calccombo4buff();
            calccombo5buff();
            calccombo7buff();
            calccombo8buff();
            lastgunindex[6] = select;
    /*        cb6.IsChecked = false;
            if (isnightskill(gun[select].type) && cbIsnight.IsChecked == false)
                cb6.IsEnabled = false;
            else
                cb6.IsEnabled = true;*/
            renewskill();
            calceat();
            equipcb61.Items.Clear();
            equipcb61.IsEnabled = false;
            equipcb61.ToolTip = null;
            equipcb62.Items.Clear();
            equipcb62.IsEnabled = false;
            equipcb62.ToolTip = null;
            equipcb63.Items.Clear();
            equipcb63.IsEnabled = false;
            equipcb63.ToolTip = null;
            loadequipcb(select, Level6.SelectedIndex, 6);
        }
        /// <summary>
        /// 下格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            int select = Combo7.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                if(getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null && i != 7)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo7.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1 && select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(8))
                {
                    Combo7.SelectedIndex = lastgunindex[7];
                    return;
                }

            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo7.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo7.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo7.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo7.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo7.Foreground = br;
            }
            if (select != -1)
                calccombo7buff();
            calccombo0buff();
            calccombo1buff();
            calccombo2buff();
            calccombo3buff();
            calccombo4buff();
            calccombo5buff();
            calccombo6buff();
            calccombo8buff();

            lastgunindex[7] = select;
       /*     cb7.IsChecked = false;
            if (isnightskill(gun[select].type) && cbIsnight.IsChecked == false)
                cb7.IsEnabled = false;
            else
                cb7.IsEnabled = true;*/
            renewskill();

            calceat();
            equipcb71.Items.Clear();
            equipcb71.IsEnabled = false;
            equipcb71.ToolTip = null;
            equipcb72.Items.Clear();
            equipcb72.IsEnabled = false;
            equipcb72.ToolTip = null;
            equipcb73.Items.Clear();
            equipcb73.IsEnabled = false;
            equipcb73.ToolTip = null;
            loadequipcb(select, Level7.SelectedIndex, 7);
        }
        /// <summary>
        /// 右下格ComboBox改变选中项事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Combo8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo8.SelectedIndex;
            if (select == -1)
                return;
            for (int i = 0; i < 9; i++)
            {
                if (getcombogunname(i) != "" && getcombogunname(i) != null)
                    howmany++;
                if (getcombogunname(i) == gun[select].name && getcombogunname(i) != "" && getcombogunname(i) != null&&i!=8)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                Combo0.SelectedIndex = GUN_NUMBER;
                                Image0.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb0.Text = "无";
                                Lskilldamage0.Content = 0;
                                Ltime0.Content = 0;
                                Lskillread0.Content = 0;
                                break;
                            }
                        case 1:
                            {
                                Combo1.SelectedIndex = GUN_NUMBER;
                                Image1.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb1.Text = "无";
                                Lskilldamage1.Content = 0;
                                Ltime1.Content = 0;
                                Lskillread1.Content = 0;
                                break;
                            }
                        case 2:
                            {
                                Combo2.SelectedIndex = GUN_NUMBER;
                                Image2.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb2.Text = "无";
                                Lskilldamage2.Content = 0;
                                Ltime2.Content = 0;
                                Lskillread2.Content = 0;
                                break;
                            }
                        case 3:
                            {
                                Combo3.SelectedIndex = GUN_NUMBER;
                                Image3.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb3.Text = "无";
                                Lskilldamage3.Content = 0;
                                Ltime3.Content = 0;
                                Lskillread3.Content = 0;
                                break;
                            }
                        case 4:
                            {
                                Combo4.SelectedIndex = GUN_NUMBER;
                                Image4.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb4.Text = "无";
                                Lskilldamage4.Content = 0;
                                Ltime4.Content = 0;
                                Lskillread4.Content = 0;
                                break;
                            }
                        case 5:
                            {
                                Combo5.SelectedIndex = GUN_NUMBER;
                                Image5.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb5.Text = "无";
                                Lskilldamage5.Content = 0;
                                Ltime5.Content = 0;
                                Lskillread5.Content = 0;
                                break;
                            }
                        case 6:
                            {
                                Combo6.SelectedIndex = GUN_NUMBER;
                                Image6.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb6.Text = "无";
                                Lskilldamage6.Content = 0;
                                Ltime6.Content = 0;
                                Lskillread6.Content = 0;
                                break;
                            }
                        case 7:
                            {
                                Combo7.SelectedIndex = GUN_NUMBER;
                                Image7.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb7.Text = "无";
                                Lskilldamage7.Content = 0;
                                Ltime7.Content = 0;
                                Lskillread7.Content = 0;
                                break;
                            }
                        case 8:
                            {
                                Combo8.SelectedIndex = GUN_NUMBER;
                                Image8.Source = new BitmapImage(new Uri(@"", UriKind.Relative));
                                tb8.Text = "无";
                                Lskilldamage8.Content = 0;
                                Ltime8.Content = 0;
                                Lskillread8.Content = 0;
                                break;
                            }
                        default:
                            break;
                    }
           

                    break;
                }
            }

            if (howmany == 6)
            {
                howmany = 0;
                Combo8.SelectedIndex = GUN_NUMBER;
                return;
            }
            else
                howmany = 0;
            if (select != -1&&select != GUN_NUMBER)
                if (gun[select].name == getcombogunname(0) || gun[select].name == getcombogunname(1) || gun[select].name == getcombogunname(2) || gun[select].name == getcombogunname(3) || gun[select].name == getcombogunname(4) || gun[select].name == getcombogunname(5) || gun[select].name == getcombogunname(6) || gun[select].name == getcombogunname(7))
                {
                    Combo8.SelectedIndex = lastgunindex[8]; 
                    return;
                }

            if (gun[select].what == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                Combo8.Foreground = br;
            }
            else if (gun[select].what == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("darkviolet"));
                Combo8.Foreground = br;
            }
            else if (gun[select].what == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                Combo8.Foreground = br;
            }
            else if (gun[select].what == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                Combo8.Foreground = br;
            }
            else if (gun[select].what == 6)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("red"));
                Combo8.Foreground = br;
            }
            if (select != -1)
                calccombo8buff();
            calccombo0buff();
            calccombo1buff();
            calccombo2buff();
            calccombo3buff();
            calccombo4buff();
            calccombo5buff();
            calccombo6buff();
            calccombo7buff();

            lastgunindex[8] = select;
   /*         cb8.IsChecked = false;
            if (isnightskill(gun[select].type) && cbIsnight.IsChecked == false)
                cb8.IsEnabled = false;
            else
                cb8.IsEnabled = true;*/
            renewskill();

            calceat();
            equipcb81.Items.Clear();
            equipcb81.IsEnabled = false;
            equipcb81.ToolTip = null;
            equipcb82.Items.Clear();
            equipcb82.IsEnabled = false;
            equipcb82.ToolTip = null;
            equipcb83.Items.Clear();
            equipcb83.IsEnabled = false;
            equipcb83.ToolTip = null;
            loadequipcb(select, Level8.SelectedIndex, 8);
        }
        /// <summary>
        /// 判断string是否为数字
        /// </summary>
        /// <param name="strNumber">string</param>
        /// <returns></returns>
        public bool IsNumber(String strNumber)
        {
            Regex objNotNumberPattern = new Regex("[^0-9.-]");
            Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
            Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
            String strValidRealPattern = "^([.]|[0-9])[0-9]*[.]*[0-9]+$";
            String strValidIntegerPattern = "^([0-9])[0-9]*$";
            Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");
            return !objNotNumberPattern.IsMatch(strNumber) &&
                   !objTwoDotPattern.IsMatch(strNumber) &&
                   !objTwoMinusPattern.IsMatch(strNumber) &&
                   objNumberPattern.IsMatch(strNumber);
        }
        /// <summary>
        /// 回避输入框数值改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsNumber(enemydodge.Text))
                enemydodge.Text = "0";
            int select = Combo0.SelectedIndex;
            if(select!=-1)
                renewindex(0);
            select = Combo1.SelectedIndex;
            if (select != -1)
                renewindex(1);
            select = Combo2.SelectedIndex;
            if (select != -1)
                renewindex(2);
            select = Combo3.SelectedIndex;
            if (select != -1)
                renewindex(3);
            select = Combo4.SelectedIndex;
            if (select != -1)
                renewindex(4);
            select = Combo5.SelectedIndex;
            if (select != -1)
                renewindex(5);
            select = Combo6.SelectedIndex;
            if (select != -1)
                renewindex(6);
            select = Combo7.SelectedIndex;
            if (select != -1)
                renewindex(7);
            select = Combo8.SelectedIndex;
            if (select != -1)
                renewindex(8);
         }

        /// <summary>
        /// 计算主T肉度
        /// </summary>
        /// <param name="combo">哪一格</param>
        void calctank(int combo)
        {
            nowhit.Content = (Double.Parse(enemyhit.Text) * skilldownhit).ToString("0");  
            nowdamage.Content = (Double.Parse(enemydamage.Text) * skilldowndamage).ToString("0");
            switch (combo)
            {
                case 0:
                    {
                        if(Combo0.SelectedIndex!=-1)
                        {
                            if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp0.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge0.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;         
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 1:
                    {
                        if (Combo1.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp1.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge1.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 2:
                    {
                        if (Combo2.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp2.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge2.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 3:
                    {
                        if (Combo3.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp3.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge3.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 4:
                    {
                        if (Combo4.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp4.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge4.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 5:
                    {
                        if (Combo5.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp5.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge5.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 6:
                    {
                        if (Combo6.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp6.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge6.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 7:
                    {
                        if (Combo7.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp7.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge7.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                case 8:
                    {
                        if (Combo8.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                tank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp8.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge8.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                tank.Content = 0;
                        }
                        else
                            tank.Content = 0;
                        break;
                    }
                default:
                    break;

            }
        }
        /// <summary>
        /// 计算副T肉度
        /// </summary>
        /// <param name="combo">哪一格</param>
        void calcftank(int combo)
        {
            nowhit.Content = (Double.Parse(enemyhit.Text) * skilldownhit).ToString("0");
            nowdamage.Content = (Double.Parse(enemydamage.Text) * skilldowndamage).ToString("0");
            switch (combo)
            {
                case 0:
                    {
                        if (Combo0.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp0.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge0.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 1:
                    {
                        if (Combo1.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp1.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge1.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 2:
                    {
                        if (Combo2.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp2.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge2.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 3:
                    {
                        if (Combo3.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp3.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge3.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 4:
                    {
                        if (Combo4.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp4.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge4.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 5:
                    {
                        if (Combo5.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp5.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge5.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 6:
                    {
                        if (Combo6.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp6.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge6.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 7:
                    {
                        if (Combo7.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp7.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge7.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                case 8:
                    {
                        if (Combo8.SelectedIndex != -1)
                        {
                               if (Double.Parse(nowhit.Content.ToString()) != 0&& Double.Parse(nowdamage.Content.ToString()) != 0)
                                ftank.Content = (Double.Parse((Math.Ceiling(Double.Parse(Lhp8.Content.ToString()) / Double.Parse(nowdamage.Content.ToString())) / (1 / (1 + Double.Parse(Ldodge8.Content.ToString()) / Double.Parse(nowhit.Content.ToString())))).ToString())).ToString("0.00");
                            else
                                ftank.Content = 0;
                        }
                        else
                            ftank.Content = 0;
                        break;
                    }
                default:
                    break;

            }
        }
        /// <summary>
        /// 命中输入框数值改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsNumber(enemyhit.Text))
                enemyhit.Text = "0";
            renewtank();
        }
        /// <summary>
        /// 选中左上格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb0_Checked(object sender, RoutedEventArgs e)
        {
            calctank(0);
        }
        /// <summary>
        /// 选中上格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb1_Checked(object sender, RoutedEventArgs e)
        {
            calctank(1);
        }
        /// <summary>
        /// 选中右上格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb2_Checked(object sender, RoutedEventArgs e)
        {
            calctank(2);
        }
        /// <summary>
        /// 选中左中格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb3_Checked(object sender, RoutedEventArgs e)
        {
            calctank(3);
        }
        /// <summary>
        /// 选中中格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb4_Checked(object sender, RoutedEventArgs e)
        {
            calctank(4);
        }
        /// <summary>
        /// 选中右中格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb5_Checked(object sender, RoutedEventArgs e)
        {
            calctank(5);
        }
        /// <summary>
        /// 选中左下格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb6_Checked(object sender, RoutedEventArgs e)
        {
            calctank(6);
        }
        /// <summary>
        /// 选中下格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb7_Checked(object sender, RoutedEventArgs e)
        {
            calctank(7);
        }
        /// <summary>
        /// 选中右下格主T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rb8_Checked(object sender, RoutedEventArgs e)
        {
            calctank(8);
        }
        /// <summary>
        /// 点击重置按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reinit_Click(object sender, RoutedEventArgs e)
        {
            bignews.Visibility = Visibility.Visible;
            for (int i = 0; i < 9; i++)
            {
                gg[i] = new GunGrid();
                gg[i].critup = 1.00;
                gg[i].damageup = 1.00;
                gg[i].dodgeup = 1.00;
                gg[i].hitup = 1.00;
                gg[i].shotspeedup = 1.00;
                lastgunindex[i] = -1;
                gg[i].rateup = 0;
                clearequip(i);
                merry[i] = 1;
            }
            howmany = 0;

            Combo0.SelectedIndex = GUN_NUMBER;
            renewindex(0);
            Combo1.SelectedIndex = GUN_NUMBER;
            renewindex(1);
            Combo2.SelectedIndex = GUN_NUMBER;
            renewindex(2);
            Combo3.SelectedIndex = GUN_NUMBER;
            renewindex(3);
            Combo4.SelectedIndex = GUN_NUMBER;
            renewindex(4);
            Combo5.SelectedIndex = GUN_NUMBER;
            renewindex(5);
            Combo6.SelectedIndex = GUN_NUMBER;
            renewindex(6);
            Combo7.SelectedIndex = GUN_NUMBER;
            renewindex(7);
            Combo8.SelectedIndex = GUN_NUMBER;
            renewindex(8);

            rb0.IsChecked = false;
            rb1.IsChecked = false;
            rb2.IsChecked = false;
            rb3.IsChecked = false;
            rb4.IsChecked = false;
            rb5.IsChecked = false;
            rb6.IsChecked = false;
            rb7.IsChecked = false;
            rb8.IsChecked = false;

            tank.Content = 0;
            enemydodge.Text = "10";
            enemyhit.Text = "20";
            enemydamage.Text = "1";

            rbf0.IsChecked = false;
            rbf1.IsChecked = false;
            rbf2.IsChecked = false;
            rbf3.IsChecked = false;
            rbf4.IsChecked = false;
            rbf5.IsChecked = false;
            rbf6.IsChecked = false;
            rbf7.IsChecked = false;
            rbf8.IsChecked = false;

            ftank.Content = 0;

            cb0.IsChecked = false;
            cb1.IsChecked = false;
            cb2.IsChecked = false;
            cb3.IsChecked = false;
            cb4.IsChecked = false;
            cb5.IsChecked = false;
            cb6.IsChecked = false;
            cb7.IsChecked = false;
            cb8.IsChecked = false;

            Lskillrate0.Content = "0%";
            Lskillrate1.Content = "0%";
            Lskillrate2.Content = "0%";
            Lskillrate3.Content = "0%";
            Lskillrate4.Content = "0%";
            Lskillrate5.Content = "0%";
            Lskillrate6.Content = "0%";
            Lskillrate7.Content = "0%";
            Lskillrate8.Content = "0%";

            Level0.SelectedIndex = 99;
            Level1.SelectedIndex = 99;
            Level2.SelectedIndex = 99;
            Level3.SelectedIndex = 99;
            Level4.SelectedIndex = 99;
            Level5.SelectedIndex = 99;
            Level6.SelectedIndex = 99;
            Level7.SelectedIndex = 99;
            Level8.SelectedIndex = 99;
            SkillLevel0.SelectedIndex = 9;
            SkillLevel1.SelectedIndex = 9;
            SkillLevel2.SelectedIndex = 9;
            SkillLevel3.SelectedIndex = 9;
            SkillLevel4.SelectedIndex = 9;
            SkillLevel5.SelectedIndex = 9;
            SkillLevel6.SelectedIndex = 9;
            SkillLevel7.SelectedIndex = 9;
            SkillLevel8.SelectedIndex = 9;

            Merry0.Content = "♡";
            Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
            Merry0.Foreground = br;
            Merry1.Content = "♡";
            Merry1.Foreground = br;
            Merry2.Content = "♡";
            Merry2.Foreground = br;
            Merry3.Content = "♡";
            Merry3.Foreground = br;
            Merry4.Content = "♡";
            Merry4.Foreground = br;
            Merry5.Content = "♡";
            Merry5.Foreground = br;
            Merry6.Content = "♡";
            Merry6.Foreground = br;
            Merry0.Content = "♡";
            Merry0.Foreground = br;
            Merry8.Content = "♡";
            Merry8.Foreground = br;
        }
        /// <summary>
        /// 选中左上格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf0_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(0);
        }
        /// <summary>
        /// 选中上格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf1_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(1);
        }
        /// <summary>
        /// 选中右上格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf2_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(2);
        }
        /// <summary>
        /// 选中左中格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf3_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(3);
        }
        /// <summary>
        /// 选中中格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf4_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(4);
        }
        /// <summary>
        /// 选中右中格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf5_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(5);
        }
        /// <summary>
        /// 选中左下格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf6_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(6);
        }
        /// <summary>
        /// 选中下格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf7_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(7);
        }
        /// <summary>
        /// 选中右下格副T
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbf8_Checked(object sender, RoutedEventArgs e)
        {
            calcftank(8);
        }
        /// <summary>
        /// 点击About按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            Summary s = new Summary();
            s.ShowDialog();
        }
        /// <summary>
        /// 点击log按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            UpdateData u = new UpdateData();
            u.ShowDialog();
        }
        /// <summary>
        /// 重置技能加成
        /// </summary>
        private void clearskill()
        {
            for(int i = 0;i<9;i++)
            {
                skillupshotspeed[i] = 1;
                skilluphit[i] = 1;
                skillupdodge[i] = 1;
                skillupdamage[i] = 1;
                skilldamageagain[i] = 1;

            }

            skilldowndodge = 1;
            skilldownhit = 1;
            skilldowndamage = 1;
        }
        /// <summary>
        /// 左上格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb0_Click(object sender, RoutedEventArgs e)
        {
            if (cb0.IsChecked == true)
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(0, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            }
            renewtank();
        }
        /// <summary>
        /// 上格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb1_Click(object sender, RoutedEventArgs e)
        {
            if (cb1.IsChecked == true)
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(1, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            }
            renewtank();
        }
        /// <summary>
        /// 右上格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb2_Click(object sender, RoutedEventArgs e)
        {
            if (cb2.IsChecked == true)
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(2, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            }
            renewtank();
        }
        /// <summary>
        /// 左中格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb3_Click(object sender, RoutedEventArgs e)
        {
            if (cb3.IsChecked == true)
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(3, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            }
            renewtank();
        }
        /// <summary>
        /// 中格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb4_Click(object sender, RoutedEventArgs e)
        {
            if (cb4.IsChecked == true)
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(4, 0);
                for (int i = 0; i < 9;i++ )
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            }
            renewtank();
        }
        /// <summary>
        /// 右中格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb5_Click(object sender, RoutedEventArgs e)
        {
            if (cb5.IsChecked == true)
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(5, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            }
            renewtank();
        }
        /// <summary>
        /// 左下格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb6_Click(object sender, RoutedEventArgs e)
        {
            if (cb6.IsChecked == true)
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(6, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
    
            }
            renewtank();
        }
        /// <summary>
        /// 下格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb7_Click(object sender, RoutedEventArgs e)
        {
            if (cb7.IsChecked == true)
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(7, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            }
            renewtank();
        }
        /// <summary>
        /// 右下格技能打勾事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb8_Click(object sender, RoutedEventArgs e)
        {
            if (cb8.IsChecked == true)
                calcskill(8, Combo8.SelectedIndex, SkillLevel8.SelectedIndex, cb8.IsChecked == true, Level8.SelectedIndex);
            else
            {
                clearskill();
                renewdamage(8, 0);
                for (int i = 0; i < 9; i++)
                {
                    renewindex(i);
                }
                calcskill(0, Combo0.SelectedIndex, SkillLevel0.SelectedIndex, cb0.IsChecked == true, Level0.SelectedIndex);
                calcskill(1, Combo1.SelectedIndex, SkillLevel1.SelectedIndex, cb1.IsChecked == true, Level1.SelectedIndex);
                calcskill(2, Combo2.SelectedIndex, SkillLevel2.SelectedIndex, cb2.IsChecked == true, Level2.SelectedIndex);
                calcskill(3, Combo3.SelectedIndex, SkillLevel3.SelectedIndex, cb3.IsChecked == true, Level3.SelectedIndex);
                calcskill(4, Combo4.SelectedIndex, SkillLevel4.SelectedIndex, cb4.IsChecked == true, Level4.SelectedIndex);
                calcskill(5, Combo5.SelectedIndex, SkillLevel5.SelectedIndex, cb5.IsChecked == true, Level5.SelectedIndex);
                calcskill(6, Combo6.SelectedIndex, SkillLevel6.SelectedIndex, cb6.IsChecked == true, Level6.SelectedIndex);
                calcskill(7, Combo7.SelectedIndex, SkillLevel7.SelectedIndex, cb7.IsChecked == true, Level7.SelectedIndex);
             }
            renewtank();
        }
        /// <summary>
        /// 计算一战消耗
        /// </summary>
        private void calceat()
        {
            double ammo = 0;
            double food = 0;
            if(Combo0.SelectedIndex!=-1&&Combo0.SelectedIndex!=GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo0.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level0.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level0.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level0.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level0.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else 
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level0.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level0.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level0.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level0.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level0.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level0.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level0.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level0.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level0.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level0.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level0.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level0.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level0.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level0.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level0.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level0.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            if (Combo1.SelectedIndex != -1 && Combo1.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo1.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level1.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level1.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level1.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level1.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level1.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level1.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level1.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level1.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level1.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level1.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level1.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level1.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level1.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level1.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level1.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level1.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level1.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level1.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level1.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level1.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (Combo2.SelectedIndex != -1 && Combo2.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo2.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level2.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level2.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level2.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level2.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level2.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level2.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level2.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level2.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level2.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level2.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level2.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level2.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level2.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level2.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level2.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level2.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level2.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level2.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level2.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level2.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (Combo3.SelectedIndex != -1 && Combo3.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo3.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level3.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level3.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level3.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level3.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level3.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level3.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level3.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level3.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level3.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level3.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level3.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level3.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level3.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level3.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level3.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level3.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level3.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level3.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level3.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level3.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (Combo4.SelectedIndex != -1 && Combo4.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo4.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level4.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level4.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level4.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level4.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level4.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level4.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level4.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level4.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level4.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level4.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level4.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level4.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level4.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level4.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level4.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level4.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level4.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level4.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level4.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level4.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (Combo5.SelectedIndex != -1 && Combo5.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo5.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level5.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level5.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level5.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level5.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level5.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level5.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level5.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level5.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level5.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level5.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level5.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level5.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level5.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level5.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level5.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level5.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level5.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level5.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level5.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level5.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (Combo6.SelectedIndex != -1 && Combo6.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo6.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level6.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level6.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level6.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level6.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level6.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level6.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level6.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level6.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level6.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level6.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level6.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level6.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level6.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level6.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level6.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level6.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level6.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level6.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level6.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level6.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (Combo7.SelectedIndex != -1 && Combo7.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo7.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level7.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level7.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level7.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level7.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level7.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level7.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level7.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level7.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level7.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level7.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level7.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level7.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level7.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level7.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level7.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level7.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level7.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level7.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level7.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level7.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (Combo8.SelectedIndex != -1 && Combo8.SelectedIndex != GUN_NUMBER)//2 ar 3 smg 4 hg 5 rf 6 mg
            {
                switch (gun[Combo8.SelectedIndex].what)
                {
                    case 2:
                        {
                            if (Level8.SelectedIndex >= 89)
                            {
                                ammo += 12;
                                food += 6;
                            }
                            else if (Level8.SelectedIndex >= 69)
                            {
                                ammo += 10;
                                food += 5;
                            }
                            else if (Level8.SelectedIndex >= 29)
                            {
                                ammo += 8;
                                food += 4;
                            }
                            else if (Level8.SelectedIndex >= 9)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else
                            {
                                ammo += 4;
                                food += 2;
                            }
                            break;
                        }
                    case 3:
                        {
                            if (Level8.SelectedIndex >= 89)
                            {
                                ammo += 17;
                                food += 6;
                            }
                            else if (Level8.SelectedIndex >= 69)
                            {
                                ammo += 14;
                                food += 5;
                            }
                            else if (Level8.SelectedIndex >= 29)
                            {
                                ammo += 11;
                                food += 4;
                            }
                            else if (Level8.SelectedIndex >= 9)
                            {
                                ammo += 8;
                                food += 3;
                            }
                            else
                            {
                                ammo += 5;
                                food += 2;
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Level8.SelectedIndex >= 89)
                            {
                                ammo += 6;
                                food += 3;
                            }
                            else if (Level8.SelectedIndex >= 69)
                            {
                                ammo += 5;
                                food += 2.5;
                            }
                            else if (Level8.SelectedIndex >= 29)
                            {
                                ammo += 4;
                                food += 2;
                            }
                            else if (Level8.SelectedIndex >= 9)
                            {
                                ammo += 3;
                                food += 1.5;
                            }
                            else
                            {
                                ammo += 2;
                                food += 1;
                            }
                            break;
                        }
                    case 5:
                        {
                            if (Level8.SelectedIndex >= 89)
                            {
                                ammo += 11;
                                food += 9;
                            }
                            else if (Level8.SelectedIndex >= 69)
                            {
                                ammo += 9;
                                food += 7.5;
                            }
                            else if (Level8.SelectedIndex >= 29)
                            {
                                ammo += 7;
                                food += 6;
                            }
                            else if (Level8.SelectedIndex >= 9)
                            {
                                ammo += 5;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 3;
                                food += 3;
                            }
                            break;
                        }
                    case 6:
                        {
                            if (Level8.SelectedIndex >= 89)
                            {
                                ammo += 28;
                                food += 9;
                            }
                            else if (Level8.SelectedIndex >= 69)
                            {
                                ammo += 23;
                                food += 7.5;
                            }
                            else if (Level8.SelectedIndex >= 29)
                            {
                                ammo += 18;
                                food += 6;
                            }
                            else if (Level8.SelectedIndex >= 9)
                            {
                                ammo += 13;
                                food += 4.5;
                            }
                            else
                            {
                                ammo += 8;
                                food += 3;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            waste.Content = ammo.ToString() + "弹药 " + food.ToString() + "口粮";
        }
        /// <summary>
        /// 滑块移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            for(int i =0;i<9;i++)
            {
                renewindex(i);
            }
        }
        /// <summary>
        /// 敌方数据按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            enemydata ed = new enemydata();
            ed.Show();
        }
        /// <summary>
        /// 计算等级属性
        /// </summary>
        /// <param name="select">该格枪娘index</param>
        /// <param name="levelselect">该格等级index</param>
        /// <param name="combo">哪一格</param>
        /// <param name="skillselect">该格技能index</param>
        private void calclevel(int select,int levelselect,int combo,int skillselect)
        {
            if (select == -1)
                return;
            if (levelselect == -1 || levelselect == 100)
                return;
            showbuff(combo, select);
            float[] array = arrAbilityRatio[gun[select].what];
            float num = 55f;
            float num2 = 0.555f;
            float num3 = 100f;
            double maxLife = Math.Ceiling((num + levelselect * num2) * array[0] * gun[select].ratiohp / num3);

            num = 16f;
            num2 = 100f;
            double basePow = Math.Ceiling(num * array[1] * gun[select].ratiopow / num2);
            num = 0.242f;
            num2 = 100f;
            num3 = 100f;
            double maxAddPow = Math.Ceiling(levelselect  * num * array[1] * gun[select].ratiopow * gun[select].eatratio / num2 / num3);
            num = 45f;
            num2 = 100f;
            double baseRate = Math.Ceiling(num * array[2] * gun[select].ratiorate / num2);
            num = 0.181f;
            num2 = 100f;
            num3 = 100f;
            double  maxAddRate= Math.Ceiling(levelselect  * num * array[2] * gun[select].ratiorate*gun[select].eatratio / num2 / num3);
            num = 5f;
            num2 = 100f;
            double basehit = Math.Ceiling(num * array[4] * gun[select].ratiohit / num2);
            num = 0.303f;
            num2 = 100f;
            num3 = 100f;
            double maxAddHit = Math.Ceiling(levelselect  * num * array[4] * gun[select].ratiohit * gun[select].eatratio / num2 / num3);
            
            num = 5f;
            num2 = 100f;
            double baseDodge = Math.Ceiling(num * array[5] * gun[select].ratiododge / num2);
            num = 0.303f;
            num2 = 100f;
            num3 = 100f;
            double maxAddDodge = Math.Ceiling(levelselect  * num * array[5] * gun[select].ratiododge *gun[select].eatratio / num2 / num3);

            switch(combo)
            {
                case 0:
                    {
                        Lhp0.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[0] > Int32.Parse(enemyarmor.Text))
                                Ldamage0.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[0])+ equipdamage[0]) * gg[0].damageup * (skillupdamage[0]) + 2).ToString("0");
                            else
                                Ldamage0.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[0])+ equipdamage[0]) * gg[0].damageup * (skillupdamage[0])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[0] > Int32.Parse(enemyarmor.Text))
                                Ldamage0.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[0])+ equipdamage[0]) * gg[0].damageup * (skillupdamage[0]) + 2).ToString("0");
                            else
                                Ldamage0.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[0])+ equipdamage[0]) * gg[0].damageup * (skillupdamage[0])) / 10, (((Math.Ceiling((basePow + maxAddPow ) * merry[0])+ equipdamage[0]) * gg[0].damageup * (skillupdamage[0])) + equipbreakarmor[0] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp0.Content.ToString()) == 0)
                            Ldamage0.Content = 0;
                        if (innight)
                            Lhit0.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[0]) + equiphit[0]) * (100 - 0.9 * (100 - equipnightsee[0])) / 100 * gg[0].hitup * (skilluphit[0])).ToString();
                        else
                            Lhit0.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[0]) + equiphit[0]) * gg[0].hitup * (skilluphit[0])).ToString("0");

                        Image0.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[0].damageup != 1)
                            tbt += "伤害+" + ((gg[0].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[0].shotspeedup != 1)
                            tbt += "射速+" + ((gg[0].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[0].hitup != 1)
                            tbt += "命中+" + ((gg[0].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[0].dodgeup != 1)
                            tbt += "回避+" + ((gg[0].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[0].critup != 1)
                            tbt += "暴击率+" + ((gg[0].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[0].rateup != 0)
                            tbt += "发动率+" + (gg[0].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb0.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[0]) * gg[0].shotspeedup * (skillupshotspeed[0]) > 120)
                            Lshotspeed0.Content = 120;
                        else
                            Lshotspeed0.Content = ((baseRate + maxAddRate + equipshotspeed[0]) * gg[0].shotspeedup * (skillupshotspeed[0])).ToString("0");
                        calcprobabiliy(0, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[0]) * gg[0].critup;
                        Lcrit0.Content = (crit * 100).ToString("0") + "%";
                        Ldodge0.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[0]) + equipdodge[0]) * gg[0].dodgeup * (skillupdodge[0])).ToString("0");
                        Lbelt0.Content = gun[select].belt + equipbelt[0];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex0.Content = Index(Double.Parse(Lshotspeed0.Content.ToString()), Double.Parse(Ldamage0.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit0.Content.ToString()), int.Parse(Lbelt0.Content.ToString()), 0, skilldamageagain[0]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb0.IsChecked == true)
                            calctank(0);
                        if (rbf0.IsChecked == true)
                            calcftank(0);
                        break;
                    }
                case 1:
                    {
                        Lhp1.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[1] > 0)
                                Ldamage1.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[1])+ equipdamage[1]) * gg[1].damageup * (skillupdamage[1]) + 2).ToString("0");
                            else
                                Ldamage1.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[1])+ equipdamage[1]) * gg[1].damageup * (skillupdamage[1])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[1] > Int32.Parse(enemyarmor.Text))
                                Ldamage1.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[1])+ equipdamage[1]) * gg[1].damageup * (skillupdamage[1]) + 2).ToString("0");
                            else
                                Ldamage1.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[1])+ equipdamage[1]) * gg[1].damageup * (skillupdamage[1])) / 10, (((Math.Ceiling((basePow + maxAddPow ) * merry[1])+ equipdamage[1]) * gg[1].damageup * (skillupdamage[1])) + equipbreakarmor[1] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp1.Content.ToString()) == 0)
                            Ldamage1.Content = 0;
                        if (innight)
                            Lhit1.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[1] )+ equiphit[1]) * (100 - 0.9 * (100 - equipnightsee[1])) / 100 * gg[1].hitup * (skilluphit[1])).ToString();
                        else
                            Lhit1.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[1] )+ equiphit[1]) * gg[1].hitup * (skilluphit[1])).ToString("0");

           
                        Image1.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[1].damageup != 1)
                            tbt += "伤害+" + ((gg[1].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[1].shotspeedup != 1)
                            tbt += "射速+" + ((gg[1].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[1].hitup != 1)
                            tbt += "命中+" + ((gg[1].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[1].dodgeup != 1)
                            tbt += "回避+" + ((gg[1].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[1].critup != 1)
                            tbt += "暴击率+" + ((gg[1].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[1].rateup != 0)
                            tbt += "发动率+" + (gg[1].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb1.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[1]) * gg[1].shotspeedup * (skillupshotspeed[1]) > 120)
                            Lshotspeed1.Content = 120;
                        else
                            Lshotspeed1.Content = ((baseRate + maxAddRate + equipshotspeed[1]) * gg[1].shotspeedup * (skillupshotspeed[1])).ToString("0");
                        calcprobabiliy(1, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[1]) * gg[1].critup;
                        Lcrit1.Content = (crit * 100).ToString("0") + "%";
                        Ldodge1.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[1]) + equipdodge[1]) * gg[1].dodgeup * (skillupdodge[1])).ToString("0");
                        Lbelt1.Content = gun[select].belt + equipbelt[1];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex1.Content = Index(Double.Parse(Lshotspeed1.Content.ToString()), Double.Parse(Ldamage1.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit1.Content.ToString()), int.Parse(Lbelt1.Content.ToString()), 1, skilldamageagain[1]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb1.IsChecked == true)
                            calctank(1);
                        if (rbf1.IsChecked == true)
                            calcftank(1);
                        break;
                    }
                case 2:
                    {
                        Lhp2.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[2] > 0)
                                Ldamage2.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[2])+ equipdamage[2]) * gg[2].damageup * (skillupdamage[2]) + 2).ToString("0");
                            else
                                Ldamage2.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[2])+ equipdamage[2]) * gg[2].damageup * (skillupdamage[2])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[2] > Int32.Parse(enemyarmor.Text))
                                Ldamage2.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[2])+ equipdamage[2]) * gg[2].damageup * (skillupdamage[2]) + 2).ToString("0");
                            else
                                Ldamage2.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[2])+ equipdamage[2]) * gg[2].damageup * (skillupdamage[2])) / 10, ((((Math.Ceiling(basePow + maxAddPow ) * merry[2])+ equipdamage[2]) * gg[2].damageup * (skillupdamage[2])) + equipbreakarmor[2] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp2.Content.ToString()) == 0)
                            Ldamage2.Content = 0;
                        if (innight)
                            Lhit2.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[2]) + equiphit[2]) * (100 - 0.9 * (100 - equipnightsee[2])) / 100 * gg[2].hitup * (skilluphit[2])).ToString();
                        else
                            Lhit2.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[2]) + equiphit[2]) * gg[2].hitup * (skilluphit[2])).ToString("0");
                        Image2.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[2].damageup != 1)
                            tbt += "伤害+" + ((gg[2].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[2].shotspeedup != 1)
                            tbt += "射速+" + ((gg[2].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[2].hitup != 1)
                            tbt += "命中+" + ((gg[2].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[2].dodgeup != 1)
                            tbt += "回避+" + ((gg[2].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[2].critup != 1)
                            tbt += "暴击率+" + ((gg[2].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[2].rateup != 0)
                            tbt += "发动率+" + (gg[2].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb2.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[2]) * gg[2].shotspeedup * (skillupshotspeed[2]) > 120)
                            Lshotspeed2.Content = 120;
                        else
                            Lshotspeed2.Content = ((baseRate + maxAddRate + equipshotspeed[2]) * gg[2].shotspeedup * (skillupshotspeed[2])).ToString("0");
                        calcprobabiliy(2, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[2]) * gg[2].critup;
                        Lcrit2.Content = (crit * 100).ToString("0") + "%";
                        Ldodge2.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[2]) + equipdodge[2]) * gg[2].dodgeup * (skillupdodge[2])).ToString("0");
                        Lbelt2.Content = gun[select].belt +equipbelt[2];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex2.Content = Index(Double.Parse(Lshotspeed2.Content.ToString()), Double.Parse(Ldamage2.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit2.Content.ToString()), int.Parse(Lbelt2.Content.ToString()), 2, skilldamageagain[2]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb2.IsChecked == true)
                            calctank(2);
                        if (rbf2.IsChecked == true)
                            calcftank(2);
                        break;
                    }
                case 3:
                    {
                        Lhp3.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[3] > 0)
                                Ldamage3.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[3])+ equipdamage[3]) * gg[3].damageup * (skillupdamage[3]) + 2).ToString("0");
                            else
                                Ldamage3.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[3])+ equipdamage[3]) * gg[3].damageup * (skillupdamage[3])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[3] > Int32.Parse(enemyarmor.Text))
                                Ldamage3.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[3])+ equipdamage[3]) * gg[3].damageup * (skillupdamage[3]) + 2).ToString("0");
                            else
                                Ldamage3.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[3])+ equipdamage[3]) * gg[3].damageup * (skillupdamage[3])) / 10, (((Math.Ceiling((basePow + maxAddPow ) * merry[3])+ equipdamage[3]) * gg[3].damageup * (skillupdamage[3])) + equipbreakarmor[3] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp3.Content.ToString()) == 0)
                            Ldamage3.Content = 0;
                        if (innight)
                            Lhit3.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[3]) + equiphit[3]) * (100 - 0.9 * (100 - equipnightsee[3])) / 100 * gg[3].hitup * (skilluphit[3])).ToString();
                        else
                            Lhit3.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[3]) + equiphit[3]) * gg[3].hitup * (skilluphit[3])).ToString("0");
                        Image3.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[3].damageup != 1)
                            tbt += "伤害+" + ((gg[3].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[3].shotspeedup != 1)
                            tbt += "射速+" + ((gg[3].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[3].hitup != 1)
                            tbt += "命中+" + ((gg[3].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[3].dodgeup != 1)
                            tbt += "回避+" + ((gg[3].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[3].critup != 1)
                            tbt += "暴击率+" + ((gg[3].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[3].rateup != 0)
                            tbt += "发动率+" + (gg[3].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb3.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[3]) * gg[3].shotspeedup * (skillupshotspeed[3]) > 120)
                            Lshotspeed3.Content = 120;
                        else
                            Lshotspeed3.Content = ((baseRate + maxAddRate + equipshotspeed[3]) * gg[3].shotspeedup * (skillupshotspeed[3])).ToString("0");
                        calcprobabiliy(3, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[3]) * gg[3].critup;
                        Lcrit3.Content = (crit * 100).ToString("0") + "%";
                        Ldodge3.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[3]) + equipdodge[3]) * gg[3].dodgeup * (skillupdodge[3])).ToString("0");
                        Lbelt3.Content = gun[select].belt +equipbelt[3];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex3.Content = Index(Double.Parse(Lshotspeed3.Content.ToString()), Double.Parse(Ldamage3.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit3.Content.ToString()), int.Parse(Lbelt3.Content.ToString()), 3, skilldamageagain[3]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb3.IsChecked == true)
                            calctank(3);
                        if (rbf3.IsChecked == true)
                            calcftank(3);
                        break;
                    }
                case 4:
                    {
                        Lhp4.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[4] > 0)
                                Ldamage4.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[4])+ equipdamage[4]) * gg[4].damageup * (skillupdamage[4]) + 2).ToString("0");
                            else
                                Ldamage4.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[4])+ equipdamage[4]) * gg[4].damageup * (skillupdamage[4])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[4] > Int32.Parse(enemyarmor.Text))
                                Ldamage4.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[4])+ equipdamage[4]) * gg[4].damageup * (skillupdamage[4]) + 2).ToString("0");
                            else
                                Ldamage4.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[4])+ equipdamage[4]) * gg[4].damageup * (skillupdamage[4])) / 10, (((Math.Ceiling(basePow + maxAddPow ) * merry[4]+ equipdamage[4]) * gg[4].damageup * (skillupdamage[4])) + equipbreakarmor[4] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp4.Content.ToString()) == 0)
                            Ldamage4.Content = 0;
                        if (innight)
                            Lhit4.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[4] )+ equiphit[4]) * (100 - 0.9 * (100 - equipnightsee[4])) / 100 * gg[4].hitup * (skilluphit[4])).ToString();
                        else
                            Lhit4.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[4] )+ equiphit[4]) * gg[4].hitup * (skilluphit[4])).ToString("0");
                        Image4.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[4].damageup != 1)
                            tbt += "伤害+" + ((gg[4].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[4].shotspeedup != 1)
                            tbt += "射速+" + ((gg[4].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[4].hitup != 1)
                            tbt += "命中+" + ((gg[4].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[4].dodgeup != 1)
                            tbt += "回避+" + ((gg[4].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[4].critup != 1)
                            tbt += "暴击率+" + ((gg[4].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[4].rateup != 0)
                            tbt += "发动率+" + (gg[4].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb4.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[4]) * gg[4].shotspeedup * (skillupshotspeed[4]) > 120)
                            Lshotspeed4.Content = 120;
                        else
                            Lshotspeed4.Content = ((baseRate + maxAddRate + equipshotspeed[4]) * gg[4].shotspeedup * (skillupshotspeed[4])).ToString("0");
                        calcprobabiliy(4, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[4]) * gg[4].critup;
                        Lcrit4.Content = (crit * 100).ToString("0") + "%";
                        Ldodge4.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[4]) + equipdodge[4]) * gg[4].dodgeup * (skillupdodge[4])).ToString("0");
                        Lbelt4.Content = gun[select].belt+equipbelt[4];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex4.Content = Index(Double.Parse(Lshotspeed4.Content.ToString()), Double.Parse(Ldamage4.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit4.Content.ToString()), int.Parse(Lbelt4.Content.ToString()), 4, skilldamageagain[4]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb4.IsChecked == true)
                            calctank(4);
                        if (rbf4.IsChecked == true)
                            calcftank(4);
                        break;
                    }
                case 5:
                    {
                        Lhp5.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[5] > 0)
                                Ldamage5.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[5])+ equipdamage[5]) * gg[5].damageup * (skillupdamage[5]) + 2).ToString("0");
                            else
                                Ldamage5.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[5])+ equipdamage[5]) * gg[5].damageup * (skillupdamage[5])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[5] > Int32.Parse(enemyarmor.Text))
                                Ldamage5.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[5])+ equipdamage[5]) * gg[5].damageup * (skillupdamage[5]) + 2).ToString("0");
                            else
                                Ldamage5.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[5])+ equipdamage[5]) * gg[5].damageup * (skillupdamage[5])) / 10, (((Math.Ceiling(basePow + maxAddPow ) * merry[5]+ equipdamage[5]) * gg[5].damageup * (skillupdamage[5])) + equipbreakarmor[5] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp5.Content.ToString()) == 0)
                            Ldamage5.Content = 0;
                        if (innight)
                            Lhit5.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[5]) + equiphit[5]) * (100 - 0.9 * (100 - equipnightsee[5])) / 100 * gg[5].hitup * (skilluphit[5])).ToString();
                        else
                            Lhit5.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[5] )+ equiphit[5]) * gg[5].hitup * (skilluphit[5])).ToString("0");
                        Image5.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[5].damageup != 1)
                            tbt += "伤害+" + ((gg[5].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[5].shotspeedup != 1)
                            tbt += "射速+" + ((gg[5].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[5].hitup != 1)
                            tbt += "命中+" + ((gg[5].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[5].dodgeup != 1)
                            tbt += "回避+" + ((gg[5].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[5].critup != 1)
                            tbt += "暴击率+" + ((gg[5].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[5].rateup != 0)
                            tbt += "发动率+" + (gg[5].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb5.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[5]) * gg[5].shotspeedup * (skillupshotspeed[5]) > 120)
                            Lshotspeed5.Content = 120;
                        else
                            Lshotspeed5.Content = ((baseRate + maxAddRate + equipshotspeed[5]) * gg[5].shotspeedup * (skillupshotspeed[5])).ToString("0");
                        calcprobabiliy(5, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[5]) * gg[5].critup;
                        Lcrit5.Content = (crit * 100).ToString("0") + "%";
                        Ldodge5.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[5]) + equipdodge[5]) * gg[5].dodgeup * (skillupdodge[5])).ToString("0");
                        Lbelt5.Content = gun[select].belt+equipbelt[5];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex5.Content = Index(Double.Parse(Lshotspeed5.Content.ToString()), Double.Parse(Ldamage5.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit5.Content.ToString()), int.Parse(Lbelt5.Content.ToString()), 5, skilldamageagain[5]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb5.IsChecked == true)
                            calctank(5);
                        if (rbf5.IsChecked == true)
                            calcftank(5);
                        break;
                    }
                case 6:
                    {
                        Lhp6.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[6] > 0)
                                Ldamage6.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[6])+ equipdamage[6]) * gg[6].damageup * (skillupdamage[6]) + 2).ToString("0");
                            else
                                Ldamage6.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[6])+ equipdamage[6]) * gg[6].damageup * (skillupdamage[6])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[6] > Int32.Parse(enemyarmor.Text))
                                Ldamage6.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[6])+ equipdamage[6]) * gg[6].damageup * (skillupdamage[6]) + 2).ToString("0");
                            else
                                Ldamage6.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[6])+ equipdamage[6]) * gg[6].damageup * (skillupdamage[6])) / 10, (((Math.Ceiling((basePow + maxAddPow ) * merry[6])+ equipdamage[6]) * gg[6].damageup * (skillupdamage[6])) + equipbreakarmor[6] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp6.Content.ToString()) == 0)
                            Ldamage6.Content = 0;
                        if (innight)
                            Lhit6.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[6]) + equiphit[6]) * (100 - 0.9 * (100 - equipnightsee[6])) / 100 * gg[6].hitup * (skilluphit[6])).ToString();
                        else
                            Lhit6.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[6] )+ equiphit[6]) * gg[6].hitup * (skilluphit[6])).ToString("0");
                        Image6.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[6].damageup != 1)
                            tbt += "伤害+" + ((gg[6].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[6].shotspeedup != 1)
                            tbt += "射速+" + ((gg[6].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[6].hitup != 1)
                            tbt += "命中+" + ((gg[6].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[6].dodgeup != 1)
                            tbt += "回避+" + ((gg[6].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[6].critup != 1)
                            tbt += "暴击率+" + ((gg[6].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[6].rateup != 0)
                            tbt += "发动率+" + (gg[6].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb6.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[6]) * gg[6].shotspeedup * (skillupshotspeed[6]) > 120)
                            Lshotspeed6.Content = 120;
                        else
                            Lshotspeed6.Content = ((baseRate + maxAddRate + equipshotspeed[6]) * gg[6].shotspeedup * (skillupshotspeed[6])).ToString("0");
                        calcprobabiliy(6, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[6]) * gg[6].critup;
                        Lcrit6.Content = (crit * 100).ToString("0") + "%";
                        Ldodge6.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[6] )+ equipdodge[6]) * gg[6].dodgeup * (skillupdodge[6])).ToString("0");
                        Lbelt6.Content = gun[select].belt+equipbelt[6];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex6.Content = Index(Double.Parse(Lshotspeed6.Content.ToString()), Double.Parse(Ldamage6.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit6.Content.ToString()), int.Parse(Lbelt6.Content.ToString()), 6, skilldamageagain[6]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb6.IsChecked == true)
                            calctank(6);
                        if (rbf6.IsChecked == true)
                            calcftank(6);
                        break;
                    }
                case 7:
                    {
                        Lhp7.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[7] > 0)
                                Ldamage7.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[7])+ equipdamage[7]) * gg[7].damageup * (skillupdamage[7]) + 2).ToString("0");
                            else
                                Ldamage7.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[7])+ equipdamage[7]) * gg[7].damageup * (skillupdamage[7])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[7] > Int32.Parse(enemyarmor.Text))
                                Ldamage7.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[7])+ equipdamage[7]) * gg[7].damageup * (skillupdamage[7]) + 2).ToString("0");
                            else
                                Ldamage7.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow ) * merry[7])+ equipdamage[7]) * gg[7].damageup * (skillupdamage[7])) / 10, (((Math.Ceiling((basePow + maxAddPow ) * merry[7])+ equipdamage[7]) * gg[7].damageup * (skillupdamage[7])) + equipbreakarmor[7] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp7.Content.ToString()) == 0)
                            Ldamage7.Content = 0;
                        if (innight)
                            Lhit7.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[7]) + equiphit[7]) * (100 - 0.9 * (100 - equipnightsee[7])) / 100 * gg[7].hitup * (skilluphit[7])).ToString();
                        else
                            Lhit7.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[7] )+ equiphit[7]) * gg[7].hitup * (skilluphit[7])).ToString("0");
                        Image7.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[7].damageup != 1)
                            tbt += "伤害+" + ((gg[7].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[7].shotspeedup != 1)
                            tbt += "射速+" + ((gg[7].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[7].hitup != 1)
                            tbt += "命中+" + ((gg[7].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[7].dodgeup != 1)
                            tbt += "回避+" + ((gg[7].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[7].critup != 1)
                            tbt += "暴击率+" + ((gg[7].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[7].rateup != 0)
                            tbt += "发动率+" + (gg[7].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb7.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[7]) * gg[7].shotspeedup * (skillupshotspeed[7]) > 120)
                            Lshotspeed7.Content = 120;
                        else
                            Lshotspeed7.Content = ((baseRate + maxAddRate + equipshotspeed[7]) * gg[7].shotspeedup * (skillupshotspeed[7])).ToString("0");
                        calcprobabiliy(7, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[7]) * gg[7].critup;
                        Lcrit7.Content = (crit * 100).ToString("0") + "%";
                        Ldodge7.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[7] )+ equipdodge[7]) * gg[7].dodgeup * (skillupdodge[7])).ToString("0");
                        Lbelt7.Content = gun[select].belt+equipbelt[7];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex7.Content = Index(Double.Parse(Lshotspeed7.Content.ToString()), Double.Parse(Ldamage7.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit7.Content.ToString()), int.Parse(Lbelt7.Content.ToString()), 7, skilldamageagain[7]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb7.IsChecked == true)
                            calctank(7);
                        if (rbf7.IsChecked == true)
                            calcftank(7);
                        break;
                    }
                case 8:
                    {
                        Lhp8.Content = maxLife;
                        if (enemyarmor.Text == "0")
                            if (equipbreakarmor[8] > 0)
                                Ldamage8.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[8])+ equipdamage[8]) * gg[8].damageup * (skillupdamage[8]) + 2).ToString("0");
                            else
                                Ldamage8.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[8])+ equipdamage[8]) * gg[8].damageup * (skillupdamage[8])).ToString("0");
                        else
                        {
                            if (equipbreakarmor[8] > Int32.Parse(enemyarmor.Text))
                                Ldamage8.Content = ((Math.Ceiling((basePow + maxAddPow ) * merry[8])+ equipdamage[8]) * gg[8].damageup * (skillupdamage[8]) + 2).ToString("0");
                            else
                                Ldamage8.Content = Math.Ceiling(Math.Max(((Math.Ceiling((basePow + maxAddPow )) * merry[8]+ equipdamage[8]) * gg[8].damageup * (skillupdamage[8])) / 10, (((Math.Ceiling((basePow + maxAddPow ) * merry[8])+ equipdamage[8]) * gg[8].damageup * (skillupdamage[8])) + equipbreakarmor[8] - Int32.Parse(enemyarmor.Text)))).ToString();
                        }
                        if (Int32.Parse(Lhp8.Content.ToString()) == 0)
                            Ldamage8.Content = 0;
                        if (innight)
                            Lhit8.Content = Math.Ceiling((Math.Ceiling((basehit + maxAddHit) * merry[8] )+ equiphit[8]) * (100 - 0.9 * (100 - equipnightsee[8])) / 100 * gg[8].hitup * (skilluphit[8])).ToString();
                        else
                            Lhit8.Content = ((Math.Ceiling((basehit + maxAddHit) * merry[8] )+ equiphit[8]) * gg[8].hitup * (skilluphit[8])).ToString("0");
                        Image8.Source = new BitmapImage(new Uri(@gun[select].image, UriKind.Relative));
                        string tbt = "";
                        if (gg[8].damageup != 1)
                            tbt += "伤害+" + ((gg[8].damageup - 1) * 100).ToString("0") + "% ";
                        if (gg[8].shotspeedup != 1)
                            tbt += "射速+" + ((gg[8].shotspeedup - 1) * 100).ToString("0") + "% ";
                        if (gg[8].hitup != 1)
                            tbt += "命中+" + ((gg[8].hitup - 1) * 100).ToString("0") + "% ";
                        if (gg[8].dodgeup != 1)
                            tbt += "回避+" + ((gg[8].dodgeup - 1) * 100).ToString("0") + "% ";
                        if (gg[8].critup != 1)
                            tbt += "暴击率+" + ((gg[8].critup - 1) * 100).ToString("0") + "% ";
                        if (gg[8].rateup != 0)
                            tbt += "发动率+" + (gg[8].rateup * 100).ToString("0") + "% ";
                        if (tbt == "")
                            tbt = "无";
                        tb8.Text = tbt;
                        if (gun[select].belt == 0 && (baseRate + maxAddRate + equipshotspeed[8]) * gg[8].shotspeedup * (skillupshotspeed[8]) > 120)
                            Lshotspeed8.Content = 120;
                        else
                            Lshotspeed8.Content = ((baseRate + maxAddRate + equipshotspeed[8]) * gg[8].shotspeedup * (skillupshotspeed[8])).ToString("0");
                        calcprobabiliy(8, select, skillselect);
                        double crit = (gun[select].crit + equipcrit[8]) * gg[8].critup;
                        Lcrit8.Content = (crit * 100).ToString("0") + "%";
                        Ldodge8.Content = ((Math.Ceiling((baseDodge + maxAddDodge) * merry[8] )+ equipdodge[8]) * gg[8].dodgeup * (skillupdodge[8])).ToString("0");
                        Lbelt8.Content = gun[select].belt+equipbelt[8];
                        nowdodge.Content = (Double.Parse(enemydodge.Text) * skilldowndodge).ToString("0");
                        Lindex8.Content = Index(Double.Parse(Lshotspeed8.Content.ToString()), Double.Parse(Ldamage8.Content.ToString()), crit, Double.Parse(nowdodge.Content.ToString()), Double.Parse(Lhit8.Content.ToString()), int.Parse(Lbelt8.Content.ToString()), 8, skilldamageagain[8]).ToString("0.00");
                        allindex.Content = (Double.Parse(Lindex0.Content.ToString()) + Double.Parse(Lindex1.Content.ToString()) + Double.Parse(Lindex2.Content.ToString()) + Double.Parse(Lindex3.Content.ToString()) + Double.Parse(Lindex4.Content.ToString()) + Double.Parse(Lindex5.Content.ToString()) + Double.Parse(Lindex6.Content.ToString()) + Double.Parse(Lindex7.Content.ToString()) + Double.Parse(Lindex8.Content.ToString())).ToString("0.00");
                        if (rb8.IsChecked == true)
                            calctank(8);
                        if (rbf8.IsChecked == true)
                            calcftank(8);
                        break;
                    }
                default: break;
            }
        }
        /// <summary>
        /// 左上格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level0_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo0.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level0.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel0.SelectedIndex;
            if (skillselect == -1)
                return;

            clearskill();
            calclevel(select, levelselect,0,skillselect);
            renewskill();

            equipcb01.Items.Clear();
            equipcb01.IsEnabled = false;
            equipcb02.Items.Clear();
            equipcb02.IsEnabled = false;
            equipcb03.Items.Clear();
            equipcb03.IsEnabled = false;
            loadequipcb(select, levelselect, 0);

            calceat();
        }
        /// <summary>
        /// 上格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo1.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level1.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel1.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 1,skillselect);
            renewskill();

            equipcb11.Items.Clear();
            equipcb11.IsEnabled = false;
            equipcb11.ToolTip = null;
            equipcb12.Items.Clear();
            equipcb12.IsEnabled = false;
            equipcb12.ToolTip = null;
            equipcb13.Items.Clear();
            equipcb13.IsEnabled = false;
            equipcb13.ToolTip = null;
            loadequipcb(select, levelselect, 1);

            calceat();
        }
        /// <summary>
        /// 右上格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo2.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level2.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel2.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 2,skillselect);
            renewskill();

            equipcb21.Items.Clear();
            equipcb21.IsEnabled = false;
            equipcb21.ToolTip = null;
            equipcb22.Items.Clear();
            equipcb22.IsEnabled = false;
            equipcb22.ToolTip = null;
            equipcb23.Items.Clear();
            equipcb23.IsEnabled = false;
            equipcb23.ToolTip = null;
            loadequipcb(select, levelselect, 2);

            calceat();
        }
        /// <summary>
        /// 左中格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo3.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level3.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel3.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 3,skillselect);
            renewskill();

            equipcb31.Items.Clear();
            equipcb31.IsEnabled = false;
            equipcb31.ToolTip = null;
            equipcb32.Items.Clear();
            equipcb32.IsEnabled = false;
            equipcb32.ToolTip = null;
            equipcb33.Items.Clear();
            equipcb33.IsEnabled = false;
            equipcb33.ToolTip = null;
            loadequipcb(select, levelselect, 3);

            calceat();
        }
        /// <summary>
        /// 中格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo4.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level4.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel4.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 4,skillselect);
            renewskill();

            equipcb41.Items.Clear();
            equipcb41.IsEnabled = false;
            equipcb41.ToolTip = null;
            equipcb42.Items.Clear();
            equipcb42.IsEnabled = false;
            equipcb42.ToolTip = null;
            equipcb43.Items.Clear();
            equipcb43.IsEnabled = false;
            equipcb43.ToolTip = null;
            loadequipcb(select, levelselect, 4);

            calceat();
            ///       calcskill(4, select, skillselect, cb4.IsChecked == true, levelselect);
        }
        /// <summary>
        /// 右中格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo5.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level5.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel5.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 5,skillselect);
            renewskill();

            equipcb51.Items.Clear();
            equipcb51.IsEnabled = false;
            equipcb51.ToolTip = null;
            equipcb52.Items.Clear();
            equipcb52.IsEnabled = false;
            equipcb52.ToolTip = null;
            equipcb53.Items.Clear();
            equipcb53.IsEnabled = false;
            equipcb53.ToolTip = null;
            loadequipcb(select, levelselect, 5);

            calceat();
        }
        /// <summary>
        /// 左下格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo6.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level6.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel6.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 6,skillselect);
            renewskill();

            equipcb61.Items.Clear();
            equipcb61.IsEnabled = false;
            equipcb61.ToolTip = null;
            equipcb62.Items.Clear();
            equipcb62.IsEnabled = false;
            equipcb62.ToolTip = null;
            equipcb63.Items.Clear();
            equipcb63.IsEnabled = false;
            equipcb63.ToolTip = null;
            loadequipcb(select, levelselect, 6);

            calceat();
        }
        /// <summary>
        /// 下格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo7.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level7.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel7.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 7,skillselect);
            renewskill();

            equipcb71.Items.Clear();
            equipcb71.IsEnabled = false;
            equipcb71.ToolTip = null;
            equipcb72.Items.Clear();
            equipcb72.IsEnabled = false;
            equipcb72.ToolTip = null;
            equipcb73.Items.Clear();
            equipcb73.IsEnabled = false;
            equipcb73.ToolTip = null;
            loadequipcb(select, levelselect, 7);

            calceat();
        }
        /// <summary>
        /// 右下格等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Level8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int select = Combo8.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level8.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel8.SelectedIndex;
            if (skillselect == -1)
                return;
            clearskill();
            calclevel(select, levelselect, 8,skillselect);
            renewskill();

            equipcb81.Items.Clear();
            equipcb81.IsEnabled = false;
            equipcb81.ToolTip = null;
            equipcb82.Items.Clear();
            equipcb82.IsEnabled = false;
            equipcb82.ToolTip = null;
            equipcb83.Items.Clear();
            equipcb83.IsEnabled = false;
            equipcb83.ToolTip = null;
            loadequipcb(select, levelselect, 8);

            calceat();
        }
        /// <summary>
        /// 计算技能发动率
        /// </summary>
        /// <param name="combo">哪一格</param>
        /// <param name="index">该格枪娘index</param>
        /// <param name="skillindex">该格技能等级index</param>
        private void calcprobabiliy(int combo,int index,int skillindex)
        {
            if (index == -1||skillindex == -1)
                return;

            int skilllevel = skillindex + 1;
            double rootrate = gun[index].probability * (1f + (float)(skilllevel - 1) * gun[index].growth / 9f) * Math.Ceiling((float)skilllevel / 10f) / 100f;

            switch(combo)
            {
                case 0:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[0].rateup) > 1)
                            Lskillrate0.Content = "100%";
                        else
                            Lskillrate0.Content = ((rootrate * (1 + gg[0].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 1:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[1].rateup) > 1)
                            Lskillrate1.Content = "100%";
                        else
                            Lskillrate1.Content = ((rootrate * (1 + gg[1].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 2:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[2].rateup) > 1)
                            Lskillrate2.Content = "100%";
                        else
                            Lskillrate2.Content = ((rootrate * (1 + gg[2].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 3:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[3].rateup) > 1)
                            Lskillrate3.Content = "100%";
                        else
                            Lskillrate3.Content = ((rootrate * (1 + gg[3].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 4:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[4].rateup) > 1)
                            Lskillrate4.Content = "100%";
                        else
                            Lskillrate4.Content = ((rootrate * (1 + gg[4].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 5:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[5].rateup) > 1)
                            Lskillrate5.Content = "100%";
                        else
                            Lskillrate5.Content = ((rootrate * (1 + gg[5].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 6:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[6].rateup) > 1)
                            Lskillrate6.Content = "100%";
                        else
                            Lskillrate6.Content = ((rootrate * (1 + gg[6].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 7:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[7].rateup) > 1)
                            Lskillrate7.Content = "100%";
                        else
                            Lskillrate7.Content = ((rootrate * (1 + gg[7].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                case 8:
                    {
                        if (gun[index].what == 4 && rootrate * (1 + gg[8].rateup) > 1)
                            Lskillrate8.Content = "100%";
                        else
                            Lskillrate8.Content = ((rootrate * (1 + gg[8].rateup)) * 100).ToString("0.0") + "%";
                        break;
                    }
                default: return;
            }
           
                  
        }
        /// <summary>
        /// 更新技能固定伤害
        /// </summary>
        /// <param name="combo">哪一格</param>
        /// <param name="damage">伤害值</param>
        private void renewdamage(int combo,double damage)
        {
            switch (combo)
            {
                case 0:
                    {
                        Lskilldamage0.Content = damage.ToString("0");
                        return;
                    }
                case 1:
                    {
                        Lskilldamage1.Content = damage.ToString("0");
                        return;
                    }
                case 2:
                    {
                        Lskilldamage2.Content = damage.ToString("0");
                        return;
                    }
                case 3:
                    {
                        Lskilldamage3.Content = damage.ToString("0");
                        return;
                    }
                case 4:
                    {
                        Lskilldamage4.Content = damage.ToString("0");
                        return;
                    }
                case 5:
                    {
                        Lskilldamage5.Content = damage.ToString("0");
                        return;
                    }
                case 6:
                    {
                        Lskilldamage6.Content = damage.ToString("0");
                        return;
                    }
                case 7:
                    {
                        Lskilldamage7.Content = damage.ToString("0");
                        return;
                    }
                case 8:
                    {
                        Lskilldamage8.Content = damage.ToString("0");
                        return;
                    }
                default:
                    return;
            }
        }
        /// <summary>
        /// 更新技能持续时间
        /// </summary>
        /// <param name="combo">哪一格</param>
        /// <param name="time">技能持续时间</param>
        private void renewtime(int combo,double time)
        {
            switch (combo)
            {
                case 0:
                    {
                        Ltime0.Content = time.ToString("0.0");
                        return;
                    }
               case 1:
                    {
                        Ltime1.Content = time.ToString("0.0");
                        return;
                    }
               case 2:
                    {
                        Ltime2.Content = time.ToString("0.0");
                        return;
                    }
               case 3:
                    {
                        Ltime3.Content = time.ToString("0.0");
                        return;
                    }
               case 4:
                    {
                        Ltime4.Content = time.ToString("0.0");
                        return;
                    }
               case 5:
                    {
                        Ltime5.Content = time.ToString("0.0");
                        return;
                    }
               case 6:
                    {
                        Ltime6.Content = time.ToString("0.0");
                        return;
                    }
               case 7:
                    {
                        Ltime7.Content = time.ToString("0.0");
                        return;
                    }
               case 8:
                    {
                        Ltime8.Content = time.ToString("0.0");
                        return;
                    }
                default:
                    return;
            }
        }

        /// <summary>
        /// 更新技能说明
        /// </summary>
        /// <param name="combo">哪一格</param>
        /// <param name="read">技能说明</param>
        private void renewread(int combo, string read)
        {
            switch (combo)
            {
                case 0:
                    {
                        Lskillread0.Content = read;
                        return;
                    }
                case 1:
                    {
                        Lskillread1.Content = read;
                        return;
                    }
                case 2:
                    {
                        Lskillread2.Content = read;
                        return;
                    }
                case 3:
                    {
                        Lskillread3.Content = read;
                        return;
                    }
                case 4:
                    {
                        Lskillread4.Content = read;
                        return;
                    }
                case 5:
                    {
                        Lskillread5.Content = read;
                        return;
                    }
                case 6:
                    {
                        Lskillread6.Content = read;
                        return;
                    }
                case 7:
                    {
                        Lskillread7.Content = read;
                        return;
                    }
                case 8:
                    {
                        Lskillread8.Content = read;
                        return;
                    }
                default:
                    return;
            }
        }

        /// <summary>
        /// 判断是否是夜战技能（暂时不用）
        /// </summary>
        /// <param name="skilltype">技能类型</param>
        /// <returns></returns>
        private bool isnightskill(int skilltype) //31 34 131 132 133 231 233 333 405
        {
            if (skilltype == 31 || skilltype == 34 || skilltype == 131 || skilltype == 132 || skilltype == 133 || skilltype == 231 || skilltype == 233 || skilltype == 333 || skilltype == 405)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 计算技能
        /// </summary>
        /// <param name="combo">哪一格</param>
        /// <param name="index">该格枪娘index</param>
        /// <param name="skillindex">该格技能等级index</param>
        /// <param name="ischecked">技能是否发动</param>
        /// <param name="levelindex">该格等级index</param>
        private void calcskill(int combo,int index,int skillindex,bool ischecked,int levelindex)
        {
            if (index == -1||skillindex == -1||levelindex == -1)
                return;
            double num1 = gun[index].skilleffect1 * (1f + (float)(skillindex) * gun[index].growth / 9f);
            double num2 = gun[index].skilleffect2 * (1f + (float)(skillindex) * gun[index].growth / 9f);
             double num3 = gun[index].skilleffect3 * (1f + (float)(skillindex) * gun[index].growth / 9f);
            double num4 = gun[index].skilleffect4 * (1f + (float)(skillindex) * gun[index].growth / 9f);
            renewdamage(combo, 0);
            switch(gun[index].type) 
            {
                case 1:
                    {
                        if (ischecked)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                skillupdamage[i] *= 1 + (Math.Round(num1) / 100);
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "提升己方" + num1.ToString("f0") + "%伤害";
                        renewread(combo, read);
                        return;
                    }
                case 2:
                    {
                        if (ischecked)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                skillupshotspeed[i] *= 1 + (Math.Round(num1) / 100);
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "提升己方" + num1.ToString("f0") + "%射速";
                        renewread(combo, read);
                        return;
                    }
                case 4:
                    {
                        if (ischecked)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                skillupdodge[i] *= 1 + (Math.Round(num1) / 100);
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "提升己方" + num1.ToString("f0") + "%回避";
                        renewread(combo, read);
                        return;
                    }
                case 31:
                    {
                        if (ischecked)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                skillupdamage[i] *= 1 + (Math.Round(num1) / 100);
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "(夜)提升己方" + num1.ToString("f0") + "%伤害";
                        renewread(combo, read);
                        return;
                    }
                case 34:
                    {
                        if (ischecked)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                skillupdodge[i] *= 1 + (Math.Round(num1) / 100);
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "(夜)提升己方" + num1.ToString("f0") + "%回避";
                        renewread(combo, read);
                        return;
                    }
                case 101:
                    {
                        if (ischecked)
                        {
                                skillupdamage[combo] *= 1 + (Math.Round(num1) / 100);
                                renewindex(combo);
                        }

                        if (gun[index].belt != 0)
                            renewtime(combo, gun[index].skilleffect2);
                        else
                            renewtime(combo, num2);
                        string read = "提升自身" + num1.ToString("f0") + "%伤害";
                        renewread(combo, read);
                        return;
                    }
                case 102:
                    {
                        if (ischecked)
                        {
                            skillupshotspeed[combo] *= 1 + (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        renewtime(combo, num2);
                        string read = "提升自身" + num1.ToString("f0") + "%射速";
                        renewread(combo, read);
                        return;
                    }
                case 103:
                    {
                        if (ischecked)
                        {
                            skilluphit[combo] *= 1 + (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        renewtime(combo, gun[index].skilleffect2);
                        string read = "提升自身" + num1.ToString("f0") + "%命中";
                       renewread(combo, read);
                        return;
                    }
                case 104:
                    {
                        if (ischecked)
                        {
                            skillupdodge[combo] *= 1 + (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        renewtime(combo, num2);
                        string read = "提升自身" + num1.ToString("f0") + "%回避";
                        renewread(combo, read);
                        return;
                    }
                case 106:
                    {
                        renewtime(combo, num1);
                        string read = "力场盾";
                        renewread(combo, read);
                        return;
                    }
                case 108:
                    {
                        if (ischecked)
                        {
                            skilldamageagain[combo] = Math.Floor(num1);
                            renewindex(combo);
                        }
                        renewtime(combo, num2);
                        string read = "每次攻击造成" + Math.Floor(num1).ToString() + "次伤害";
                     
                        renewread(combo, read);

                        return; 
                    }
                case 131:
                    {
                        if (ischecked)
                        {
                            skillupdamage[combo] *= 1 + (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        if(gun[index].what==6)
                          renewtime(combo, gun[index].skilleffect2);
                        else
                            renewtime(combo, num2);
                        string read = "(夜)提升自身" + num1.ToString("f0") + "%伤害";
                        renewread(combo, read);
                        return;
                    }
                case 132:
                    {
                        if (ischecked)
                        {
                            skillupshotspeed[combo] *=1+ (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        renewtime(combo, num2);
                        string read = "(夜)提升自身" + num1.ToString("f0") + "%射速";
                        renewread(combo, read);
                        return;
                    }
                case 133:
                    {
                        if (ischecked)
                        {
                            skilluphit[combo] *=1+ (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        if (index == 92)
                            renewtime(combo, gun[index].skilleffect2);
                        else
                            renewtime(combo, num2);
                        string read = "(夜)提升自身" + num1.ToString("f0") + "%命中";
                        renewread(combo, read);
                        return;
                    }
                case 201:
                    {
                        if (ischecked)
                        {
                            skilldowndamage *= 1 - (Math.Round(num1) / 100);
                            renewtank();
                        }
                        renewtime(combo, num2);
                        string read = "降低敌方" + num1.ToString("f0") + "%伤害";
                        renewread(combo, read);
                        return;
                    }
                case 202:
                    {
                        renewtime(combo, num2);
                        string read = "降低敌方" + num1.ToString("f0") + "%射速(不算)";
                        renewread(combo, read);
                        return;
                    }
                case 203:
                    {
                        if (ischecked)
                        {
                            skilldownhit *=1- (Math.Round(num1) / 100);
                            renewtank();
                        }
                        renewtime(combo, num2);
                        string read = "降低敌方" + num1.ToString("f0") + "%命中";
                        renewread(combo, read);
                        return;
                    }
                case 204:
                    {
                        if (ischecked)
                        {
                            skilldowndodge *= 1- (Math.Round(num1) / 100);
                            for (int i = 0; i < 9; i++)
                            {
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "降低敌方" + num1.ToString("f0") + "%回避";
                        renewread(combo, read);
                        return;
                    }
                case 231:
                    {
                        if (ischecked)
                        {
                            skilldowndamage *= 1 - (Math.Round(num1) / 100);
                            renewtank();
                        }
                        renewtime(combo, num2);
                        string read = "(夜)降低敌方" + num1.ToString("f0") + "%伤害";
                        renewread(combo, read);
                        return;
                    }
                case 233:
                    {
                        if (ischecked)
                        {
                            skilldownhit *=1- (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        renewtime(combo, num2);
                        string read = "(夜)降低敌方" + num1.ToString("f0") + "%命中";
                        renewread(combo, read);
                        return;
                    }
                case 301:
                    {
                        if (ischecked)
                        {
                            skilldowndamage *= 1 - (Math.Round(num1) / 100);
                            renewtank();
                        }
                        renewtime(combo, num2);
                        string read = "降低目标" + num1.ToString("f0") + "%伤害";
                        renewread(combo, read);
                        return;
                    }
                case 302:
                    {
                        renewtime(combo, num2);
                        string read = "降低目标" + num1.ToString("f0") + "%射速(不算)";
                        renewread(combo, read);
                        return;
                    }
                case 303:
                    {
                        if (ischecked)
                        {
                            skilldownhit *=1- (Math.Round(num1) / 100);
                            renewtank();
                        }
                        renewtime(combo, num2);
                        string read = "降低目标" + num1.ToString("f0") + "%命中";
                        renewread(combo, read);
                        return;
                    }
                case 304:
                    {
                        if (ischecked)
                        {
                            skilldowndodge *=1- (Math.Round(num1) / 100);
                            if (skilldowndodge < 0)
                                skilldowndodge = 0;
                            for (int i = 0; i < 9; i++)
                            {
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "降低目标" + num1.ToString("f0") + "%回避";
                        renewread(combo, read);
                        return;
                    }
                case 305:
                    {
                        renewtime(combo, num2);
                        string read = "降低目标" + num1.ToString("f0") + "%移速(不算)";
                        renewread(combo, read);
                        return;
                    }
                case 333:
                    {
                        if (ischecked)
                        {
                            skilldownhit *=1- (Math.Round(num1) / 100);
                            renewtank();
                        }
                        renewtime(combo, num2);
                        string read = "(夜)降低目标" + num1.ToString("f0") + "%命中";
                        renewread(combo, read);
                        return;
                    }
                case 401:
                    {
                        float[] array = arrAbilityRatio[gun[index].what];
       
                        float base1 = 16f;
                        float base2 = 100f;
                        double basePow = Math.Ceiling(base1 * array[1] * gun[index].ratiopow / base2);
                        base1 = 0.242f;
                        base2 = 100f;
                        float base3 = 100f;
                        double maxAddPow = Math.Ceiling(levelindex * base1 * array[1] * gun[index].ratiopow * gun[index].eatratio / base2 / base3);
                        if (ischecked)
                        {
                            renewdamage(combo, (Math.Ceiling((basePow + maxAddPow)*merry[combo]) + equipdamage[combo]) * num1);
                        }
                        renewtime(combo, num2);
                        string read = "手榴弹,半径2.5," + num1.ToString("0.0") + "倍";
                        renewread(combo, read);
                        return;
                    }
                case 402:
                    {
                        if (ischecked)
                        {
                            skilldownhit *= 1 - (Math.Round(num1) / 100);
                            renewindex(combo);
                        }
                        renewtime(combo, num3);
                        string read = "烟雾弹,半径2.5,降低" + num1.ToString("f0") + "%命中" + num2.ToString("f0") + "%移速";
                        renewread(combo, read);
                        return;
                    }
                case 403:
                    {
                        renewtime(combo, num1);
                        string read = "闪光弹,半径2.5";
                        renewread(combo, read);
                        return;
                    }

                case 404:
                    {

                        float[] array = arrAbilityRatio[gun[index].what];

                        float base1 = 16f;
                        float base2 = 100f;
                        double basePow = Math.Ceiling(base1 * array[1] * gun[index].ratiopow / base2);
                        base1 = 0.242f;
                        base2 = 100f;
                        float base3 = 100f;
                        double maxAddPow = Math.Ceiling(levelindex * base1 * array[1] * gun[index].ratiopow * gun[index].eatratio / base2 / base3);
                        if (ischecked)
                        {
                            renewdamage(combo, (Math.Ceiling((basePow + maxAddPow)*merry[combo]) + equipdamage[combo]) * num1);
                        }
                        renewtime(combo, num3);
                        string read = "燃烧弹" + num1.ToString("0.0") + "倍半径1.0不算DOT";
                        renewread(combo, read);
                        return;
                    }
                case 405:
                    {
                        if (ischecked)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                skilluphit[i] *=1+ (Math.Round(num1) / 100);
                                renewindex(i);
                            }
                        }
                        renewtime(combo, num2);
                        string read = "(夜)照明弹,提升己方" + num1.ToString("f0") + "%命中";
                        renewread(combo, read);
                        return;
                    }
                 case 501:
                    {
                        float[] array = arrAbilityRatio[gun[index].what];

                        float base1 = 16f;
                        float base2 = 100f;
                        double basePow = Math.Ceiling(base1 * array[1] * gun[index].ratiopow / base2);
                        base1 = 0.242f;
                        base2 = 100f;
                        float base3 = 100f;
                        double maxAddPow = Math.Ceiling(levelindex * base1 * array[1] * gun[index].ratiopow * gun[index].eatratio / base2 / base3);
                        if (ischecked)
                        {
                            if (equipbreakarmor[combo] > Int32.Parse(enemyarmor.Text))
                                renewdamage(combo, (Math.Ceiling((basePow + maxAddPow) * merry[combo]) + equipdamage[combo]) * num1);
                            else
                                renewdamage(combo, Math.Max(((Math.Ceiling((basePow + maxAddPow) * merry[combo]) + equipdamage[combo]) + equipbreakarmor[combo] - Int32.Parse(enemyarmor.Text)),0) * num1);
                        }
                        renewtime(combo, gun[index].skilleffect2);
                        string read = "瞄准射击,最左目标" + num1.ToString("0.0") + "倍";
                        renewread(combo, read);
                        return;
                    }
                case 502:
                    {
                        float[] array = arrAbilityRatio[gun[index].what];

                        float base1 = 16f;
                        float base2 = 100f;
                        double basePow = Math.Ceiling(base1 * array[1] * gun[index].ratiopow / base2);
                        base1 = 0.242f;
                        base2 = 100f;
                        float base3 = 100f;
                        double maxAddPow = Math.Ceiling(levelindex * base1 * array[1] * gun[index].ratiopow * gun[index].eatratio / base2 / base3);
                        if (ischecked)
                        {
                            if (equipbreakarmor[combo] > Int32.Parse(enemyarmor.Text))
                                renewdamage(combo, (Math.Ceiling((basePow + maxAddPow) * merry[combo]) + equipdamage[combo]) * num1);
                            else
                                renewdamage(combo, Math.Max(((Math.Ceiling((basePow + maxAddPow) * merry[combo]) + equipdamage[combo]) + equipbreakarmor[combo] - Int32.Parse(enemyarmor.Text)), 0) * num1);
                        }
                        renewtime(combo, gun[index].skilleffect2);
                        string read = "定点射击,最右目标" + num1.ToString("0.0") + "倍";
                        renewread(combo, read);
                        return;
                    }
                case 503:
                    {
                        float[] array = arrAbilityRatio[gun[index].what];

                        float base1 = 16f;
                        float base2 = 100f;
                        double basePow = Math.Ceiling(base1 * array[1] * gun[index].ratiopow / base2);
                        base1 = 0.242f;
                        base2 = 100f;
                        float base3 = 100f;
                        double maxAddPow = Math.Ceiling(levelindex * base1 * array[1] * gun[index].ratiopow * gun[index].eatratio / base2 / base3);
                        if (ischecked)
                        {
                            if (equipbreakarmor[combo] > Int32.Parse(enemyarmor.Text))
                                renewdamage(combo, (Math.Ceiling((basePow + maxAddPow) * merry[combo]) + equipdamage[combo]) * num1);
                            else
                                renewdamage(combo, Math.Max(((Math.Ceiling((basePow + maxAddPow) * merry[combo]) + equipdamage[combo]) + equipbreakarmor[combo] - Int32.Parse(enemyarmor.Text)), 0) * num1);
                        }
                        renewtime(combo, gun[index].skilleffect2);
                        string read = "阻断射击,特定目标" + num1.ToString("0.0") + "倍";
                        renewread(combo, read);
                        return;
                    }

                case 601:
                    {
                        float[] array = arrAbilityRatio[gun[index].what];

                        float base1 = 16f;
                        float base2 = 100f;
                        double basePow = Math.Ceiling(base1 * array[1] * gun[index].ratiopow / base2);
                        base1 = 0.242f;
                        base2 = 100f;
                        float base3 = 100f;
                        double maxAddPow = Math.Ceiling(levelindex * base1 * array[1] * gun[index].ratiopow * gun[index].eatratio / base2 / base3);
                        if (ischecked)
                        {
                           renewdamage(combo, (Math.Ceiling((basePow + maxAddPow)*merry[combo]) + equipdamage[combo]) * num1);
                        }
                        renewtime(combo, 0);
                        string read = "杀伤/爆破榴弹" +gun[index].skilleffect2+"半径"+num1.ToString("0.0") + "倍";
                        renewread(combo, read);
                        return;
                    }
                default:
                    return;
            }
        }
        /// <summary>
        /// 左上格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel0_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel0.SelectedIndex;
            int index = Combo0.SelectedIndex;
            if(index==-1||skillindex == -1)
                return;
            calcprobabiliy(0, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 上格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel1.SelectedIndex;
            int index = Combo1.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(1, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 右上格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel2.SelectedIndex;
            int index = Combo2.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(2, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 左中格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel3.SelectedIndex;
            int index = Combo3.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(3, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 中格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel4.SelectedIndex;
            int index = Combo4.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(4, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 右中格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel5.SelectedIndex;
            int index = Combo5.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(5, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 左下格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel6.SelectedIndex;
            int index = Combo6.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(6, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 下格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel7.SelectedIndex;
            int index = Combo7.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(7, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 右下格技能等级改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkillLevel8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int skillindex = SkillLevel8.SelectedIndex;
            int index = Combo8.SelectedIndex;
            if (index == -1 || skillindex == -1)
                return;
            calcprobabiliy(8, index, skillindex);
            renewskill();
        }
        /// <summary>
        /// 凑数用按钮点击。。。已抛弃的功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buff_Click(object sender, RoutedEventArgs e)
        {
            buffOn bo = new buffOn();
            bo.gg = gg;
            bo.select = new int[9];
            bo.select[0] = Combo0.SelectedIndex;
            bo.select[1] = Combo1.SelectedIndex;
            bo.select[2] = Combo2.SelectedIndex;
            bo.select[3] = Combo3.SelectedIndex;
            bo.select[4] = Combo4.SelectedIndex;
            bo.select[5] = Combo5.SelectedIndex;
            bo.select[6] = Combo6.SelectedIndex;
            bo.select[7] = Combo7.SelectedIndex;
            bo.select[8] = Combo8.SelectedIndex;
            bo.ShowDialog();
        }

        /// <summary>
        /// 点击更新链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("iexplore.exe", hyperLink.NavigateUri.ToString());
        }

        /// <summary>
        /// 给装备选项上色
        /// </summary>
        /// <param name="rank">星级</param>
        /// <param name="str">名称</param>
        /// <returns></returns>
        public Label BrushEquipCombobox(int rank,string str)
        {
            Label l = new Label();
            l.Content = str;
            if (rank == 2)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                l.Foreground = br;
            }
            else if (rank == 3)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                l.Foreground = br;
            }
            else if (rank == 4)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                l.Foreground = br;
            }
            else if (rank == 5)
            {
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                l.Foreground = br;
            }
            return l;
        }

        /// <summary>
        /// 导入枪娘可用装备数据
        /// </summary>
        /// <param name="index">该格枪娘index</param>
        /// <param name="levelindex">该格枪娘等级index</param>
        /// <param name="combo">哪一格</param>
        public void loadequipcb(int index, int levelindex, int combo)
        {
            if (levelindex == -1)
                return;
            if (index == -1 || index == GUN_NUMBER || levelindex < 19)
                return;
            int ranklevel = 0;
            int cardlevel = 0;
            if (levelindex < 29)
            {
                ranklevel = 2;
                cardlevel = 1;
            }
            else if (levelindex < 44)
            {
                ranklevel = 3;
                cardlevel = 1;
            }
            else if (levelindex < 49)
            {
                ranklevel = 4;
                cardlevel = 1;
            }
            else if (levelindex < 59)
            {
                ranklevel = 4;
                cardlevel = 2;
            }
            else if (levelindex < 79)
            {
                ranklevel = 5;
                cardlevel = 2;
            }
            else
            {
                ranklevel = 5;
                cardlevel = 3;
            }
                switch (combo)
                {
                    case 0:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb01.IsEnabled = true;
                                            equipcb01.Items.Clear();
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb02.IsEnabled = true;
                                            equipcb02.Items.Clear();
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb03.IsEnabled = true;
                                            equipcb03.Items.Clear();
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb01.IsEnabled = true;
                                            equipcb01.Items.Clear();
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb02.IsEnabled = true;
                                            equipcb02.Items.Clear();
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb03.IsEnabled = true;
                                            equipcb03.Items.Clear();
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb01.IsEnabled = true;
                                            equipcb01.Items.Clear();
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb02.IsEnabled = true;
                                            equipcb02.Items.Clear();
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb03.IsEnabled = true;
                                            equipcb03.Items.Clear();
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb01.IsEnabled = true;
                                            equipcb01.Items.Clear();
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb02.IsEnabled = true;
                                            equipcb02.Items.Clear();
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb03.IsEnabled = true;
                                            equipcb03.Items.Clear();
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb01.IsEnabled = true;
                                            equipcb01.Items.Clear();
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb02.IsEnabled = true;
                                            equipcb02.Items.Clear();
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb03.IsEnabled = true;
                                            equipcb03.Items.Clear();
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                    case 1:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb11.IsEnabled = true;
                                            equipcb11.Items.Clear();
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb12.IsEnabled = true;
                                            equipcb12.Items.Clear();
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb13.IsEnabled = true;
                                            equipcb13.Items.Clear();
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb11.IsEnabled = true;
                                            equipcb11.Items.Clear();
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb12.IsEnabled = true;
                                            equipcb12.Items.Clear();
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb13.IsEnabled = true;
                                            equipcb13.Items.Clear();
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb11.IsEnabled = true;
                                            equipcb11.Items.Clear();
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb12.IsEnabled = true;
                                            equipcb12.Items.Clear();
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb13.IsEnabled = true;
                                            equipcb13.Items.Clear();
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb11.IsEnabled = true;
                                            equipcb11.Items.Clear();
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb12.IsEnabled = true;
                                            equipcb12.Items.Clear();
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb13.IsEnabled = true;
                                            equipcb13.Items.Clear();
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb11.IsEnabled = true;
                                            equipcb11.Items.Clear();
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb12.IsEnabled = true;
                                            equipcb12.Items.Clear();
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb13.IsEnabled = true;
                                            equipcb13.Items.Clear();
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }

                            default:
                                break;
                        }
                            break;
                        }
                    case 2:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb21.IsEnabled = true;
                                            equipcb21.Items.Clear();
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb22.IsEnabled = true;
                                            equipcb22.Items.Clear();
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb23.IsEnabled = true;
                                            equipcb23.Items.Clear();
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb21.IsEnabled = true;
                                            equipcb21.Items.Clear();
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb22.IsEnabled = true;
                                            equipcb22.Items.Clear();
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb23.IsEnabled = true;
                                            equipcb23.Items.Clear();
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb21.IsEnabled = true;
                                            equipcb21.Items.Clear();
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb22.IsEnabled = true;
                                            equipcb22.Items.Clear();
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb23.IsEnabled = true;
                                            equipcb23.Items.Clear();
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb21.IsEnabled = true;
                                            equipcb21.Items.Clear();
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb22.IsEnabled = true;
                                            equipcb22.Items.Clear();
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb23.IsEnabled = true;
                                            equipcb23.Items.Clear();
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb21.IsEnabled = true;
                                            equipcb21.Items.Clear();
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb22.IsEnabled = true;
                                            equipcb22.Items.Clear();
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb23.IsEnabled = true;
                                            equipcb23.Items.Clear();
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                    case 3:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb31.IsEnabled = true;
                                            equipcb31.Items.Clear();
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb32.IsEnabled = true;
                                            equipcb32.Items.Clear();
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb33.IsEnabled = true;
                                            equipcb33.Items.Clear();
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb31.IsEnabled = true;
                                            equipcb31.Items.Clear();
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb32.IsEnabled = true;
                                            equipcb32.Items.Clear();
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb33.IsEnabled = true;
                                            equipcb33.Items.Clear();
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb31.IsEnabled = true;
                                            equipcb31.Items.Clear();
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb32.IsEnabled = true;
                                            equipcb32.Items.Clear();
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb33.IsEnabled = true;
                                            equipcb33.Items.Clear();
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb31.IsEnabled = true;
                                            equipcb31.Items.Clear();
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb32.IsEnabled = true;
                                            equipcb32.Items.Clear();
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb33.IsEnabled = true;
                                            equipcb33.Items.Clear();
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb31.IsEnabled = true;
                                            equipcb31.Items.Clear();
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb32.IsEnabled = true;
                                            equipcb32.Items.Clear();
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb33.IsEnabled = true;
                                            equipcb33.Items.Clear();
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                    case 4:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb41.IsEnabled = true;
                                            equipcb41.Items.Clear();
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb42.IsEnabled = true;
                                            equipcb42.Items.Clear();
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb43.IsEnabled = true;
                                            equipcb43.Items.Clear();
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb41.IsEnabled = true;
                                            equipcb41.Items.Clear();
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb42.IsEnabled = true;
                                            equipcb42.Items.Clear();
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb43.IsEnabled = true;
                                            equipcb43.Items.Clear();
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb41.IsEnabled = true;
                                            equipcb41.Items.Clear();
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb42.IsEnabled = true;
                                            equipcb42.Items.Clear();
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb43.IsEnabled = true;
                                            equipcb43.Items.Clear();
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb41.IsEnabled = true;
                                            equipcb41.Items.Clear();
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb42.IsEnabled = true;
                                            equipcb42.Items.Clear();
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb43.IsEnabled = true;
                                            equipcb43.Items.Clear();
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb41.IsEnabled = true;
                                            equipcb41.Items.Clear();
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb42.IsEnabled = true;
                                            equipcb42.Items.Clear();
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb43.IsEnabled = true;
                                            equipcb43.Items.Clear();
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                    case 5:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb51.IsEnabled = true;
                                            equipcb51.Items.Clear();
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb52.IsEnabled = true;
                                            equipcb52.Items.Clear();
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb53.IsEnabled = true;
                                            equipcb53.Items.Clear();
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb51.IsEnabled = true;
                                            equipcb51.Items.Clear();
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb52.IsEnabled = true;
                                            equipcb52.Items.Clear();
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb53.IsEnabled = true;
                                            equipcb53.Items.Clear();
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb51.IsEnabled = true;
                                            equipcb51.Items.Clear();
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb52.IsEnabled = true;
                                            equipcb52.Items.Clear();
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb53.IsEnabled = true;
                                            equipcb53.Items.Clear();
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb51.IsEnabled = true;
                                            equipcb51.Items.Clear();
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb52.IsEnabled = true;
                                            equipcb52.Items.Clear();
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb53.IsEnabled = true;
                                            equipcb53.Items.Clear();
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb51.IsEnabled = true;
                                            equipcb51.Items.Clear();
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb52.IsEnabled = true;
                                            equipcb52.Items.Clear();
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb53.IsEnabled = true;
                                            equipcb53.Items.Clear();
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                    case 6:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb61.IsEnabled = true;
                                            equipcb61.Items.Clear();
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb62.IsEnabled = true;
                                            equipcb62.Items.Clear();
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb63.IsEnabled = true;
                                            equipcb63.Items.Clear();
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb61.IsEnabled = true;
                                            equipcb61.Items.Clear();
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb62.IsEnabled = true;
                                            equipcb62.Items.Clear();
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb63.IsEnabled = true;
                                            equipcb63.Items.Clear();
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb61.IsEnabled = true;
                                            equipcb61.Items.Clear();
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb62.IsEnabled = true;
                                            equipcb62.Items.Clear();
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb63.IsEnabled = true;
                                            equipcb63.Items.Clear();
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb61.IsEnabled = true;
                                            equipcb61.Items.Clear();
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb62.IsEnabled = true;
                                            equipcb62.Items.Clear();
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb63.IsEnabled = true;
                                            equipcb63.Items.Clear();
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb61.IsEnabled = true;
                                            equipcb61.Items.Clear();
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb62.IsEnabled = true;
                                            equipcb62.Items.Clear();
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb63.IsEnabled = true;
                                            equipcb63.Items.Clear();
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                    case 7:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb71.IsEnabled = true;
                                            equipcb71.Items.Clear();
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb72.IsEnabled = true;
                                            equipcb72.Items.Clear();
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb73.IsEnabled = true;
                                            equipcb73.Items.Clear();
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb71.IsEnabled = true;
                                            equipcb71.Items.Clear();
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb72.IsEnabled = true;
                                            equipcb72.Items.Clear();
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb73.IsEnabled = true;
                                            equipcb73.Items.Clear();
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb71.IsEnabled = true;
                                            equipcb71.Items.Clear();
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb72.IsEnabled = true;
                                            equipcb72.Items.Clear();
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb73.IsEnabled = true;
                                            equipcb73.Items.Clear();
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb71.IsEnabled = true;
                                            equipcb71.Items.Clear();
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb72.IsEnabled = true;
                                            equipcb72.Items.Clear();
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb73.IsEnabled = true;
                                            equipcb73.Items.Clear();
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb71.IsEnabled = true;
                                            equipcb71.Items.Clear();
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb72.IsEnabled = true;
                                            equipcb72.Items.Clear();
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb73.IsEnabled = true;
                                            equipcb73.Items.Clear();
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                    case 8:
                        {
                            switch (gun[index].what)
                            {
                                case 2:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb81.IsEnabled = true;
                                            equipcb81.Items.Clear();
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb82.IsEnabled = true;
                                            equipcb82.Items.Clear();
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb83.IsEnabled = true;
                                            equipcb83.Items.Clear();
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb81.IsEnabled = true;
                                            equipcb81.Items.Clear();
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb82.IsEnabled = true;
                                            equipcb82.Items.Clear();
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb83.IsEnabled = true;
                                            equipcb83.Items.Clear();
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb81.IsEnabled = true;
                                            equipcb81.Items.Clear();
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 4) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb82.IsEnabled = true;
                                            equipcb82.Items.Clear();
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb83.IsEnabled = true;
                                            equipcb83.Items.Clear();
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb81.IsEnabled = true;
                                            equipcb81.Items.Clear();
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb82.IsEnabled = true;
                                            equipcb82.Items.Clear();
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb83.IsEnabled = true;
                                            equipcb83.Items.Clear();
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((false) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (cardlevel >= 1)
                                        {
                                            equipcb81.IsEnabled = true;
                                            equipcb81.Items.Clear();
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 5) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 2)
                                        {
                                            equipcb82.IsEnabled = true;
                                            equipcb82.Items.Clear();
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        if (cardlevel >= 3)
                                        {
                                            equipcb83.IsEnabled = true;
                                            equipcb83.Items.Clear();
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                            for (int i = 0; i < EQUIP_NUMBER; i++)
                                            {
                                                if ((equip[i].type == 9) && equip[i].rank <= ranklevel)
                                                {
                                                    if (equip[i].forwhat == 0)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                    else if (equip[i].forwhat == index)
                                                        equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                                }
                                            }
                                        }
                                        break;
                                    }
                            default:
                                break;
                        }
                            break;
                        }
                }
        
            if (index == 28)
            {
                switch (combo)
                {
                    case 0:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb01.IsEnabled = true;
                                equipcb01.Items.Clear();
                                equipcb01.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb02.IsEnabled = true;
                                equipcb02.Items.Clear();
                                equipcb02.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb03.IsEnabled = true;
                                equipcb03.Items.Clear();
                                equipcb03.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb11.IsEnabled = true;
                                equipcb11.Items.Clear();
                                equipcb11.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb12.IsEnabled = true;
                                equipcb12.Items.Clear();
                                equipcb12.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb13.IsEnabled = true;
                                equipcb13.Items.Clear();
                                equipcb13.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb21.IsEnabled = true;
                                equipcb21.Items.Clear();
                                equipcb21.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb22.IsEnabled = true;
                                equipcb22.Items.Clear();
                                equipcb22.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb23.IsEnabled = true;
                                equipcb23.Items.Clear();
                                equipcb23.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 3:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb31.IsEnabled = true;
                                equipcb31.Items.Clear();
                                equipcb31.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb32.IsEnabled = true;
                                equipcb32.Items.Clear();
                                equipcb32.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb33.IsEnabled = true;
                                equipcb33.Items.Clear();
                                equipcb33.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 4:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb41.IsEnabled = true;
                                equipcb41.Items.Clear();
                                equipcb41.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb42.IsEnabled = true;
                                equipcb42.Items.Clear();
                                equipcb42.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb43.IsEnabled = true;
                                equipcb43.Items.Clear();
                                equipcb43.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 5:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb51.IsEnabled = true;
                                equipcb51.Items.Clear();
                                equipcb51.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb52.IsEnabled = true;
                                equipcb52.Items.Clear();
                                equipcb52.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb53.IsEnabled = true;
                                equipcb53.Items.Clear();
                                equipcb53.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 6:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb61.IsEnabled = true;
                                equipcb61.Items.Clear();
                                equipcb61.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb62.IsEnabled = true;
                                equipcb62.Items.Clear();
                                equipcb62.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb63.IsEnabled = true;
                                equipcb63.Items.Clear();
                                equipcb63.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 7:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb71.IsEnabled = true;
                                equipcb71.Items.Clear();
                                equipcb71.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb72.IsEnabled = true;
                                equipcb72.Items.Clear();
                                equipcb72.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb73.IsEnabled = true;
                                equipcb73.Items.Clear();
                                equipcb73.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 8:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb81.IsEnabled = true;
                                equipcb81.Items.Clear();
                                equipcb81.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb82.IsEnabled = true;
                                equipcb82.Items.Clear();
                                equipcb82.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb83.IsEnabled = true;
                                equipcb83.Items.Clear();
                                equipcb83.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 10) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            if (index == 29||index == 34)
            {
                switch (combo)
                {
                    case 0:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb01.IsEnabled = true;
                                equipcb01.Items.Clear();
                                equipcb01.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb01.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb02.IsEnabled = true;
                                equipcb02.Items.Clear();
                                equipcb02.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb02.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb03.IsEnabled = true;
                                equipcb03.Items.Clear();
                                equipcb03.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb03.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb11.IsEnabled = true;
                                equipcb11.Items.Clear();
                                equipcb11.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb11.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb12.IsEnabled = true;
                                equipcb12.Items.Clear();
                                equipcb12.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb12.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb13.IsEnabled = true;
                                equipcb13.Items.Clear();
                                equipcb13.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb13.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb21.IsEnabled = true;
                                equipcb21.Items.Clear();
                                equipcb21.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb21.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb22.IsEnabled = true;
                                equipcb22.Items.Clear();
                                equipcb22.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb22.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb23.IsEnabled = true;
                                equipcb23.Items.Clear();
                                equipcb23.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb23.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 3:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb31.IsEnabled = true;
                                equipcb31.Items.Clear();
                                equipcb31.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb31.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb32.IsEnabled = true;
                                equipcb32.Items.Clear();
                                equipcb32.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb32.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb33.IsEnabled = true;
                                equipcb33.Items.Clear();
                                equipcb33.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb33.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 4:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb41.IsEnabled = true;
                                equipcb41.Items.Clear();
                                equipcb41.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb41.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb42.IsEnabled = true;
                                equipcb42.Items.Clear();
                                equipcb42.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb42.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb43.IsEnabled = true;
                                equipcb43.Items.Clear();
                                equipcb43.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb43.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 5:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb51.IsEnabled = true;
                                equipcb51.Items.Clear();
                                equipcb51.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb51.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb52.IsEnabled = true;
                                equipcb52.Items.Clear();
                                equipcb52.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb52.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb53.IsEnabled = true;
                                equipcb53.Items.Clear();
                                equipcb53.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb53.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 6:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb61.IsEnabled = true;
                                equipcb61.Items.Clear();
                                equipcb61.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb61.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb62.IsEnabled = true;
                                equipcb62.Items.Clear();
                                equipcb62.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb62.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb63.IsEnabled = true;
                                equipcb63.Items.Clear();
                                equipcb63.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb63.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 7:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb71.IsEnabled = true;
                                equipcb71.Items.Clear();
                                equipcb71.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb71.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb72.IsEnabled = true;
                                equipcb72.Items.Clear();
                                equipcb72.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb72.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb73.IsEnabled = true;
                                equipcb73.Items.Clear();
                                equipcb73.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb73.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    case 8:
                        {
                            if (cardlevel >= 1)
                            {
                                equipcb81.IsEnabled = true;
                                equipcb81.Items.Clear();
                                equipcb81.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb81.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 2)
                            {
                                equipcb82.IsEnabled = true;
                                equipcb82.Items.Clear();
                                equipcb82.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 2 || equip[i].type == 1 || equip[i].type == 3 || equip[i].type == 4) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb82.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            if (cardlevel >= 3)
                            {
                                equipcb83.IsEnabled = true;
                                equipcb83.Items.Clear();
                                equipcb83.Items.Add(BrushEquipCombobox(equip[30].rank, equip[30].name));
                                for (int i = 0; i < EQUIP_NUMBER; i++)
                                {
                                    if ((equip[i].type == 8) && equip[i].rank <= ranklevel)
                                    {
                                        if (equip[i].forwhat == 0)
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                        else if (equip[i].forwhat == index)
                                            equipcb83.Items.Add(BrushEquipCombobox(equip[i].rank, equip[i].name));
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 得到装备index
        /// </summary>
        /// <param name="name">装备名称</param>
        /// <returns></returns>
        public int getequipindex(string name)
        {
            if (name == null)
                return -1;
            int equipindex = 0;
            for (; equipindex < EQUIP_NUMBER; equipindex++)
            {
                if (name == equip[equipindex].name)
                    break;
            }
            return equipindex;
        }
        /// <summary>
        /// 清空装备栏
        /// </summary>
        /// <param name="combo">哪一格</param>
        public void clearequip(int combo)
        {
                    equipdamage[combo] = 0;
                    equiphit[combo] = 0;
                    equipdodge[combo] = 0;
                    equipbelt[combo] = 0;
                    equipcrit[combo] =0;
                    equipnightsee[combo] = 0;
                    equipshotspeed[combo] = 0;      
                    equipbreakarmor[combo] = 0;
        }
        /// <summary>
        /// 计算装备加成
        /// </summary>
        /// <param name="combo">哪一格</param>
        /// <param name="equipindex">装备index</param>
        public void calcequip(int combo,int equipindex)
        {
       

            switch (equip[equipindex].type) 
            {
                case 1:
                    {
                        equipcrit[combo] += equip[equipindex].critup;
                        break;
                    }
                case 2:
                    {
                        equiphit[combo] += equip[equipindex].hit;
                        equipdamage[combo] += equip[equipindex].damage;
                        equipshotspeed[combo] += equip[equipindex].shotspeed;
                        break;
                    }
                case 3:
                    {
                        equiphit[combo] += equip[equipindex].hit;
                        equipshotspeed[combo] += equip[equipindex].shotspeed;
                        break;
                    }
                case 4:
                    {
                        equipnightsee[combo] += equip[equipindex].nightsee;
                        break;
                    }
                case 5:
                    {
                        equipbreakarmor[combo] += equip[equipindex].breakarmor;
                        equipshotspeed[combo] += equip[equipindex].shotspeed;
                        break;
                    }
                case 8:
                    {
                        equipdamage[combo] += equip[equipindex].damage;
                        equiphit[combo] += equip[equipindex].hit;
                        break;
                    }
                case 9:
                    {
                        equipshotspeed[combo] += equip[equipindex].shotspeed;
                        equipdamage[combo] += equip[equipindex].damage;
                        equipbelt[combo] += equip[equipindex].belt;
                        break;
                    }
                case 10:
                    {
                        equipdodge[combo] += equip[equipindex].dodge;
                        equipdamage[combo] += equip[equipindex].damage;
                        break;
                    }
                case 13:
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// 设置装备tooltips
        /// </summary>
        /// <param name="combo">哪一格装备格</param>
        /// <param name="index">装备index</param>
        private void setequiptooltips(int combo,int index)
        {
            switch(combo)
            {
                case 1:
                    {
                        equipcb01.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 2:
                    {
                        equipcb02.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 3:
                    {
                        equipcb03.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 11:
                    {
                        equipcb11.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 12:
                    {
                        equipcb12.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 13:
                    {
                        equipcb13.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 21:
                    {
                        equipcb21.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 22:
                    {
                        equipcb22.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 23:
                    {
                        equipcb23.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 31:
                    {
                        equipcb31.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 32:
                    {
                        equipcb32.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 33:
                    {
                        equipcb33.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 41:
                    {
                        equipcb41.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 42:
                    {
                        equipcb42.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 43:
                    {
                        equipcb43.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 51:
                    {
                        equipcb51.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 52:
                    {
                        equipcb52.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 53:
                    {
                        equipcb53.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 61:
                    {
                        equipcb61.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 62:
                    {
                        equipcb62.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 63:
                    {
                        equipcb63.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 71:
                    {
                        equipcb71.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 72:
                    {
                        equipcb72.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 73:
                    {
                        equipcb73.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 81:
                    {
                        equipcb81.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 82:
                    {
                        equipcb82.ToolTip = equip[index].tooltip;
                        break;
                    }
                case 83:
                    {
                        equipcb83.ToolTip = equip[index].tooltip;
                        break;
                    }
            }
        }
        /// <summary>
        /// 01装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb01_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo0.SelectedIndex;
            clearequip(0);
            if (equipcb01.SelectedItem != null)
            {
                string equipselect01 = equipcb01.SelectedItem.ToString();
                int equipindex01 = getequipindex(equipselect01.Substring(31));
                if (equipindex01 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb02.SelectedItem != null && equipcb02.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb02.SelectedItem.ToString().Substring(31))].type == equip[equipindex01].type) {
                                equipcb01.SelectedIndex = 0; return; }
                    }
                if (equipcb01.SelectedIndex > -1)
                    calcequip(0, equipindex01);
                    

                if (equip[equipindex01].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb01.Foreground = br;
                }
                else if (equip[equipindex01].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb01.Foreground = br;
                }
                else if (equip[equipindex01].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb01.Foreground = br;
                }
                else if (equip[equipindex01].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb01.Foreground = br;
                }
                setequiptooltips(1, equipindex01);
            }
            if (equipcb02.SelectedItem != null)
            {
                string equipselect02 = equipcb02.SelectedItem.ToString();
                int equipindex02 = getequipindex(equipselect02.Substring(31));

                if (equipindex02 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb03.SelectedItem != null && equipcb03.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb03.SelectedItem.ToString().Substring(31))].type == equip[equipindex02].type) {
                              equipcb01.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb01.SelectedItem != null && equipcb01.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb01.SelectedItem.ToString().Substring(31))].type == equip[equipindex02].type) {
                               equipcb01.SelectedIndex = 0; return; }
                    }
                if (equipcb01.SelectedIndex > -1)
                       calcequip(0, equipindex02);
            }
            if (equipcb03.SelectedItem != null)
            {
                string equipselect03 = equipcb03.SelectedItem.ToString();
                int equipindex03 = getequipindex(equipselect03.Substring(31));
                if (equipindex03 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb02.SelectedItem != null && equipcb02.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb02.SelectedItem.ToString().Substring(31))].type == equip[equipindex03].type) {
                                equipcb01.SelectedIndex = 0; return; }
                    }

                if (equipcb01.SelectedIndex > -1)
                         calcequip(0, equipindex03);
            }
            renewskill();
        }
        /// <summary>
        /// 02装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb02_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo0.SelectedIndex;
            clearequip(0);
            if (equipcb01.SelectedItem != null)
            {
                string equipselect01 = equipcb01.SelectedItem.ToString();
                int equipindex01 = getequipindex(equipselect01.Substring(31));
                if (equipindex01 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb02.SelectedItem != null && equipcb02.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb02.SelectedItem.ToString().Substring(31))].type == equip[equipindex01].type) {
                                equipcb02.SelectedIndex = 0; return; }
                          
                    }
                if (equipcb02.SelectedIndex > -1)
                    calcequip(0, equipindex01);
            }
            if (equipcb02.SelectedItem != null)
            {
                string equipselect02 = equipcb02.SelectedItem.ToString();
                int equipindex02 = getequipindex(equipselect02.Substring(31));
                if (equipindex02 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb03.SelectedItem != null && equipcb03.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb03.SelectedItem.ToString().Substring(31))].type == equip[equipindex02].type) {
                                equipcb02.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb01.SelectedItem != null && equipcb01.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb01.SelectedItem.ToString().Substring(31))].type == equip[equipindex02].type) {
                                equipcb02.SelectedIndex = 0; return; }
                    }
                if (equipcb02.SelectedIndex > -1)
                    calcequip(0, equipindex02);
                if (equip[equipindex02].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb02.Foreground = br;
                }
                else if (equip[equipindex02].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb02.Foreground = br;
                }
                else if (equip[equipindex02].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb02.Foreground = br;
                }
                else if (equip[equipindex02].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb02.Foreground = br;
                }
                setequiptooltips(2, equipindex02);
            }
            if (equipcb03.SelectedItem != null)
            {
                string equipselect03 = equipcb03.SelectedItem.ToString();
                int equipindex03 = getequipindex(equipselect03.Substring(31));
                if (equipindex03 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb02.SelectedItem != null && equipcb02.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb02.SelectedItem.ToString().Substring(31))].type == equip[equipindex03].type) {
                                equipcb02.SelectedIndex = 0; return; }
                    }

                if (equipcb02.SelectedIndex > -1)
                    calcequip(0, equipindex03);
            }
            renewskill();
        }
        /// <summary>
        /// 03装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb03_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo0.SelectedIndex;
            clearequip(0);
            if (equipcb01.SelectedItem != null)
            {
                string equipselect01 = equipcb01.SelectedItem.ToString();
                int equipindex01 = getequipindex(equipselect01.Substring(31));
                if (equipindex01 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb02.SelectedItem != null && equipcb02.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb02.SelectedItem.ToString().Substring(31))].type == equip[equipindex01].type) {
                                equipcb03.SelectedIndex = 0; return; }
                    }
                if (equipcb03.SelectedIndex > -1)
                    calcequip(0, equipindex01);
            }
            if (equipcb02.SelectedItem != null)
            {
                string equipselect02 = equipcb02.SelectedItem.ToString();
                int equipindex02 = getequipindex(equipselect02.Substring(31));
                if (equipindex02 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb03.SelectedItem != null && equipcb03.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb03.SelectedItem.ToString().Substring(31))].type == equip[equipindex02].type) {
                                equipcb03.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb01.SelectedItem != null && equipcb01.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb01.SelectedItem.ToString().Substring(31))].type == equip[equipindex02].type) {
                                equipcb03.SelectedIndex = 0; return; }
                    }
                if (equipcb03.SelectedIndex > -1)
                    calcequip(0, equipindex02);
            }
            if (equipcb03.SelectedItem != null)
            {
                string equipselect03 = equipcb03.SelectedItem.ToString();
                int equipindex03 = getequipindex(equipselect03.Substring(31));
                if (equipindex03 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb02.SelectedItem != null && equipcb02.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb02.SelectedItem.ToString().Substring(31))].type == equip[equipindex03].type) {
                                equipcb03.SelectedIndex = 0; return; }
                    }


                if (equipcb03.SelectedIndex > -1) 
                    calcequip(0, equipindex03);
                if (equip[equipindex03].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb03.Foreground = br;
                }
                else if (equip[equipindex03].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb03.Foreground = br;
                }
                else if (equip[equipindex03].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb03.Foreground = br;
                }
                else if (equip[equipindex03].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb03.Foreground = br;
                }
                setequiptooltips(3, equipindex03);
            }
            renewskill();
        }
        /// <summary>
        /// 11装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb11_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo1.SelectedIndex;
            clearequip(1);
            if (equipcb11.SelectedItem != null)
            {
                string equipselect11 = equipcb11.SelectedItem.ToString();
                int equipindex11 = getequipindex(equipselect11.Substring(31));
                if (equipindex11 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb12.SelectedItem != null && equipcb12.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb12.SelectedItem.ToString().Substring(31))].type == equip[equipindex11].type) {
                                equipcb11.SelectedIndex = 0; return; }
                    }
                if (equipcb11.SelectedIndex > -1)
                    calcequip(1, equipindex11);
                if (equip[equipindex11].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb11.Foreground = br;
                }
                else if (equip[equipindex11].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb11.Foreground = br;
                }
                else if (equip[equipindex11].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb11.Foreground = br;
                }
                else if (equip[equipindex11].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb11.Foreground = br;
                }
                setequiptooltips(11, equipindex11);
            }
            if (equipcb12.SelectedItem != null)
            {
                string equipselect12 = equipcb12.SelectedItem.ToString();
                int equipindex12 = getequipindex(equipselect12.Substring(31));
                if (equipindex12 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb13.SelectedItem != null && equipcb13.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb13.SelectedItem.ToString().Substring(31))].type == equip[equipindex12].type) {
                                equipcb11.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb11.SelectedItem != null && equipcb11.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb11.SelectedItem.ToString().Substring(31))].type == equip[equipindex12].type) {
                                equipcb11.SelectedIndex = 0; return; }
                    }
                if (equipcb11.SelectedIndex > -1)
                    calcequip(1, equipindex12);
            }
            if (equipcb13.SelectedItem != null)
            {
                string equipselect13 = equipcb13.SelectedItem.ToString();
                int equipindex13 = getequipindex(equipselect13.Substring(31));
                if (equipindex13 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb12.SelectedItem != null && equipcb12.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb12.SelectedItem.ToString().Substring(31))].type == equip[equipindex13].type) {
                                equipcb11.SelectedIndex = 0; return; }
                    }

                if (equipcb11.SelectedIndex > -1)
                    calcequip(1, equipindex13);
            }
            renewskill();
        }
        /// <summary>
        /// 12装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb12_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo1.SelectedIndex;
            clearequip(1);
            if (equipcb11.SelectedItem != null)
            {
                string equipselect11 = equipcb11.SelectedItem.ToString();
                int equipindex11 = getequipindex(equipselect11.Substring(31));
                if (equipindex11 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb12.SelectedItem != null && equipcb12.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb12.SelectedItem.ToString().Substring(31))].type == equip[equipindex11].type) {
                                equipcb12.SelectedIndex = 0; return; }
                    }
                if (equipcb12.SelectedIndex > -1)
                    calcequip(1, equipindex11);
            }
            if (equipcb12.SelectedItem != null)
            {
                string equipselect12 = equipcb12.SelectedItem.ToString();
                int equipindex12 = getequipindex(equipselect12.Substring(31));
                if (equipindex12 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb13.SelectedItem != null && equipcb13.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb13.SelectedItem.ToString().Substring(31))].type == equip[equipindex12].type) {
                                equipcb12.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb11.SelectedItem != null && equipcb11.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb11.SelectedItem.ToString().Substring(31))].type == equip[equipindex12].type) {
                                equipcb12.SelectedIndex = 0; return; }
                    }
                if (equipcb12.SelectedIndex > -1)
                    calcequip(1, equipindex12);
                if (equip[equipindex12].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb12.Foreground = br;
                }
                else if (equip[equipindex12].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb12.Foreground = br;
                }
                else if (equip[equipindex12].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb12.Foreground = br;
                }
                else if (equip[equipindex12].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb12.Foreground = br;
                }
                setequiptooltips(12, equipindex12);
            }
            if (equipcb13.SelectedItem != null)
            {
                string equipselect13 = equipcb13.SelectedItem.ToString();
                int equipindex13 = getequipindex(equipselect13.Substring(31));
                if (equipindex13 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb12.SelectedItem != null && equipcb12.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb12.SelectedItem.ToString().Substring(31))].type == equip[equipindex13].type) {
                                equipcb12.SelectedIndex = 0; return; }
                    }

                if (equipcb12.SelectedIndex > -1)
                    calcequip(1, equipindex13);
            }
            renewskill();
        }
        /// <summary>
        ///13装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb13_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo1.SelectedIndex;
            clearequip(1);
            if (equipcb11.SelectedItem != null)
            {
                string equipselect11 = equipcb11.SelectedItem.ToString();
                int equipindex11 = getequipindex(equipselect11.Substring(31));
                if (equipindex11 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb12.SelectedItem != null && equipcb12.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb12.SelectedItem.ToString().Substring(31))].type == equip[equipindex11].type) {
                                equipcb13.SelectedIndex = 0; return; }
                    }
                if (equipcb13.SelectedIndex > -1)
                    calcequip(1, equipindex11);
            }
            if (equipcb12.SelectedItem != null)
            {
                string equipselect12 = equipcb12.SelectedItem.ToString();
                int equipindex12 = getequipindex(equipselect12.Substring(31));
                if (equipindex12 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb13.SelectedItem != null && equipcb13.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb13.SelectedItem.ToString().Substring(31))].type == equip[equipindex12].type) {
                                equipcb13.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb11.SelectedItem != null && equipcb11.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb11.SelectedItem.ToString().Substring(31))].type == equip[equipindex12].type) {
                                equipcb13.SelectedIndex = 0; return; }
                    }
                if (equipcb13.SelectedIndex > -1)
                    calcequip(1, equipindex12);
            }
            if (equipcb13.SelectedItem != null)
            {
                string equipselect13 = equipcb13.SelectedItem.ToString();
                int equipindex13 = getequipindex(equipselect13.Substring(31));
                if (equipindex13 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb12.SelectedItem != null && equipcb12.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb12.SelectedItem.ToString().Substring(31))].type == equip[equipindex13].type) {
                                equipcb13.SelectedIndex = 0; return; }
                    }

                if (equipcb13.SelectedIndex > -1)
                    calcequip(1, equipindex13);
                if (equip[equipindex13].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb13.Foreground = br;
                }
                else if (equip[equipindex13].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb13.Foreground = br;
                }
                else if (equip[equipindex13].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb13.Foreground = br;
                }
                else if (equip[equipindex13].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb13.Foreground = br;
                }
                setequiptooltips(13, equipindex13);
            }
            renewskill();
        }
        /// <summary>
        /// 21装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb21_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo2.SelectedIndex;
            clearequip(2);
            if (equipcb21.SelectedItem != null)
            {
                string equipselect21 = equipcb21.SelectedItem.ToString();
                int equipindex21 = getequipindex(equipselect21.Substring(31));
                if (equipindex21 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb22.SelectedItem != null && equipcb22.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb22.SelectedItem.ToString().Substring(31))].type == equip[equipindex21].type) {
                                equipcb21.SelectedIndex = 0; return; }
                    }
                if (equipcb21.SelectedIndex > -1)
                    calcequip(2, equipindex21);
                if (equip[equipindex21].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb21.Foreground = br;
                }
                else if (equip[equipindex21].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb21.Foreground = br;
                }
                else if (equip[equipindex21].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb21.Foreground = br;
                }
                else if (equip[equipindex21].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb21.Foreground = br;
                }
                setequiptooltips(21, equipindex21);
            }
            if (equipcb22.SelectedItem != null)
            {
                string equipselect22 = equipcb22.SelectedItem.ToString();
                int equipindex22 = getequipindex(equipselect22.Substring(31));
                if (equipindex22 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb23.SelectedItem != null && equipcb23.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb23.SelectedItem.ToString().Substring(31))].type == equip[equipindex22].type) {
                                equipcb21.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb21.SelectedItem != null && equipcb21.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb21.SelectedItem.ToString().Substring(31))].type == equip[equipindex22].type) {
                                equipcb21.SelectedIndex = 0; return; }
                    }
                if (equipcb21.SelectedIndex > -1)
                    calcequip(2, equipindex22);
            }
            if (equipcb23.SelectedItem != null)
            {
                string equipselect23 = equipcb23.SelectedItem.ToString();
                int equipindex23 = getequipindex(equipselect23.Substring(31));
                if (equipindex23 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb22.SelectedItem != null && equipcb22.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb22.SelectedItem.ToString().Substring(31))].type == equip[equipindex23].type) {
                                equipcb21.SelectedIndex = 0; return; }
                    }

                if (equipcb21.SelectedIndex > -1)
                    calcequip(2, equipindex23);
            }
            renewskill();
        }
        /// <summary>
        /// 22装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb22_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo2.SelectedIndex;
            clearequip(2);
            if (equipcb21.SelectedItem != null)
            {
                string equipselect21 = equipcb21.SelectedItem.ToString();
                int equipindex21 = getequipindex(equipselect21.Substring(31));
                if (equipindex21 != -1)
                    if (equipindex21 != -1)
                        if (comboindex == 29 || comboindex == 34)
                        {
                            if (equipcb22.SelectedItem != null && equipcb22.SelectedItem.ToString().Substring(31)!= " ")
                                if (equip[getequipindex(equipcb22.SelectedItem.ToString().Substring(31))].type == equip[equipindex21].type) {
                                    equipcb22.SelectedIndex = 0; return; }
                        }
                if (equipcb22.SelectedIndex > -1)
                    calcequip(2, equipindex21);
            }
            if (equipcb22.SelectedItem != null)
            {
                string equipselect22 = equipcb22.SelectedItem.ToString();
                int equipindex22 = getequipindex(equipselect22.Substring(31));
                if (equipindex22 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb23.SelectedItem != null && equipcb23.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb23.SelectedItem.ToString().Substring(31))].type == equip[equipindex22].type) {
                                equipcb22.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb21.SelectedItem != null && equipcb21.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb21.SelectedItem.ToString().Substring(31))].type == equip[equipindex22].type) {
                                equipcb22.SelectedIndex = 0; return; }
                    }
                if (equipcb22.SelectedIndex > -1)
                    calcequip(2, equipindex22);
                if (equip[equipindex22].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb22.Foreground = br;
                }
                else if (equip[equipindex22].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb22.Foreground = br;
                }
                else if (equip[equipindex22].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb22.Foreground = br;
                }
                else if (equip[equipindex22].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb22.Foreground = br;
                }
                setequiptooltips(22, equipindex22);
            }
            if (equipcb23.SelectedItem != null)
            {
                string equipselect23 = equipcb23.SelectedItem.ToString();
                int equipindex23 = getequipindex(equipselect23.Substring(31));
                if (equipindex23 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb22.SelectedItem != null && equipcb22.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb22.SelectedItem.ToString().Substring(31))].type == equip[equipindex23].type) {
                                equipcb22.SelectedIndex = 0; return; }
                    }

                if (equipcb22.SelectedIndex > -1)
                    calcequip(2, equipindex23);
            }
            renewskill();
        }
        /// <summary>
        /// 23装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb23_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo2.SelectedIndex;
            clearequip(2);
            if (equipcb21.SelectedItem != null)
            {
                string equipselect21 = equipcb21.SelectedItem.ToString();
                int equipindex21 = getequipindex(equipselect21.Substring(31));
                if (equipindex21 != -1)
                    if (equipindex21 != -1)
                        if (comboindex == 29 || comboindex == 34)
                        {
                            if (equipcb22.SelectedItem != null && equipcb22.SelectedItem.ToString().Substring(31)!= " ")
                                if (equip[getequipindex(equipcb22.SelectedItem.ToString().Substring(31))].type == equip[equipindex21].type) {
                                    equipcb23.SelectedIndex = 0; return; }
                        }
                if (equipcb23.SelectedIndex > -1)
                    calcequip(2, equipindex21);
            }
            if (equipcb22.SelectedItem != null)
            {
                string equipselect22 = equipcb22.SelectedItem.ToString();
                int equipindex22 = getequipindex(equipselect22.Substring(31));
                if (equipindex22 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb23.SelectedItem != null && equipcb23.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb23.SelectedItem.ToString().Substring(31))].type == equip[equipindex22].type) {
                                equipcb23.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb21.SelectedItem != null && equipcb21.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb21.SelectedItem.ToString().Substring(31))].type == equip[equipindex22].type) {
                                equipcb23.SelectedIndex = 0; return; }
                    }
                if (equipcb23.SelectedIndex > -1)
                    calcequip(2, equipindex22);
            }
            if (equipcb23.SelectedItem != null)
            {
                string equipselect23 = equipcb23.SelectedItem.ToString();
                int equipindex23 = getequipindex(equipselect23.Substring(31));
                if (equipindex23 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb22.SelectedItem != null && equipcb22.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb22.SelectedItem.ToString().Substring(31))].type == equip[equipindex23].type) {
                                equipcb23.SelectedIndex = 0; return; }
                    }

                if (equipcb23.SelectedIndex > -1)
                    calcequip(2, equipindex23);
                if (equip[equipindex23].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb23.Foreground = br;
                }
                else if (equip[equipindex23].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb23.Foreground = br;
                }
                else if (equip[equipindex23].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb23.Foreground = br;
                }
                else if (equip[equipindex23].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb23.Foreground = br;
                }
                setequiptooltips(23, equipindex23);
            }
            renewskill();
        }
        /// <summary>
        ///31装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb31_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo3.SelectedIndex;
            clearequip(3);
            if (equipcb31.SelectedItem != null)
            {
                string equipselect31 = equipcb31.SelectedItem.ToString();
                int equipindex31 = getequipindex(equipselect31.Substring(31));
                if (equipindex31 != -1)
                    if (equipindex31 != -1)
                        if (comboindex == 29 || comboindex == 34)
                        {
                            if (equipcb32.SelectedItem != null && equipcb32.SelectedItem.ToString().Substring(31)!= " ")
                                if (equip[getequipindex(equipcb32.SelectedItem.ToString().Substring(31))].type == equip[equipindex31].type) {
                                    equipcb31.SelectedIndex = 0; return; }
                        }
                if (equipcb31.SelectedIndex > -1)
                    calcequip(3, equipindex31);
                if (equip[equipindex31].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb31.Foreground = br;
                }
                else if (equip[equipindex31].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb31.Foreground = br;
                }
                else if (equip[equipindex31].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb31.Foreground = br;
                }
                else if (equip[equipindex31].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb31.Foreground = br;
                }
                setequiptooltips(31, equipindex31);
            }
            if (equipcb32.SelectedItem != null)
            {
                string equipselect32 = equipcb32.SelectedItem.ToString();
                int equipindex32 = getequipindex(equipselect32.Substring(31));
                if (equipindex32 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb33.SelectedItem != null && equipcb33.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb33.SelectedItem.ToString().Substring(31))].type == equip[equipindex32].type) {
                                equipcb31.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb31.SelectedItem != null && equipcb31.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb31.SelectedItem.ToString().Substring(31))].type == equip[equipindex32].type) {
                                equipcb31.SelectedIndex = 0; return; }
                    }
                if (equipcb31.SelectedIndex > -1)
                    calcequip(3, equipindex32);
            }
            if (equipcb33.SelectedItem != null)
            {
                string equipselect33 = equipcb33.SelectedItem.ToString();
                int equipindex33 = getequipindex(equipselect33.Substring(31));
                if (equipindex33 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb32.SelectedItem != null && equipcb32.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb32.SelectedItem.ToString().Substring(31))].type == equip[equipindex33].type) {
                                equipcb31.SelectedIndex = 0; return; }
                    }

                if (equipcb31.SelectedIndex > -1)
                    calcequip(3, equipindex33);
            }
            renewskill();
        }
        /// <summary>
        ///32装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb32_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo3.SelectedIndex;
            clearequip(3);
            if (equipcb31.SelectedItem != null)
            {
                string equipselect31 = equipcb31.SelectedItem.ToString();
                int equipindex31 = getequipindex(equipselect31.Substring(31));
                if (equipindex31 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb32.SelectedItem != null && equipcb32.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb32.SelectedItem.ToString().Substring(31))].type == equip[equipindex31].type) {
                                equipcb32.SelectedIndex = 0; return; }
                    }
                if (equipcb32.SelectedIndex > -1)
                    calcequip(3, equipindex31);
            }
            if (equipcb32.SelectedItem != null)
            {
                string equipselect32 = equipcb32.SelectedItem.ToString();
                int equipindex32 = getequipindex(equipselect32.Substring(31));
                if (equipindex32 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb33.SelectedItem != null && equipcb33.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb33.SelectedItem.ToString().Substring(31))].type == equip[equipindex32].type) {
                                equipcb32.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb31.SelectedItem != null && equipcb31.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb31.SelectedItem.ToString().Substring(31))].type == equip[equipindex32].type) {
                                equipcb32.SelectedIndex = 0; return; }
                    }
                if (equipcb32.SelectedIndex > -1)
                    calcequip(3, equipindex32);
                if (equip[equipindex32].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb32.Foreground = br;
                }
                else if (equip[equipindex32].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb32.Foreground = br;
                }
                else if (equip[equipindex32].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb32.Foreground = br;
                }
                else if (equip[equipindex32].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb32.Foreground = br;
                }
                setequiptooltips(32, equipindex32);
            }
            if (equipcb33.SelectedItem != null)
            {
                string equipselect33 = equipcb33.SelectedItem.ToString();
                int equipindex33 = getequipindex(equipselect33.Substring(31));
                if (equipindex33 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb32.SelectedItem != null && equipcb32.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb32.SelectedItem.ToString().Substring(31))].type == equip[equipindex33].type) {
                                equipcb32.SelectedIndex = 0; return; }
                    }

                if (equipcb32.SelectedIndex > -1)
                    calcequip(3, equipindex33);
            }
            renewskill();
        }
        /// <summary>
        ///33装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb33_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo3.SelectedIndex;
            clearequip(3);
            if (equipcb31.SelectedItem != null)
            {
                string equipselect31 = equipcb31.SelectedItem.ToString();
                int equipindex31 = getequipindex(equipselect31.Substring(31));
                if (equipindex31 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb32.SelectedItem != null && equipcb32.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb32.SelectedItem.ToString().Substring(31))].type == equip[equipindex31].type) {
                                equipcb33.SelectedIndex = 0; return; }
                    }
                if (equipcb33.SelectedIndex > -1)
                    calcequip(3, equipindex31);
            }
            if (equipcb32.SelectedItem != null)
            {
                string equipselect32 = equipcb32.SelectedItem.ToString();
                int equipindex32 = getequipindex(equipselect32.Substring(31));
                if (equipindex32 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb33.SelectedItem != null && equipcb33.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb33.SelectedItem.ToString().Substring(31))].type == equip[equipindex32].type) {
                                equipcb33.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb31.SelectedItem != null && equipcb31.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb31.SelectedItem.ToString().Substring(31))].type == equip[equipindex32].type) {
                                equipcb33.SelectedIndex = 0; return; }
                    }
                if (equipcb33.SelectedIndex > -1)
                    calcequip(3, equipindex32);
            }
            if (equipcb33.SelectedItem != null)
            {
                string equipselect33 = equipcb33.SelectedItem.ToString();
                int equipindex33 = getequipindex(equipselect33.Substring(31));
                if (equipindex33 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb32.SelectedItem != null && equipcb32.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb32.SelectedItem.ToString().Substring(31))].type == equip[equipindex33].type) {
                                equipcb33.SelectedIndex = 0; return; }
                    }

                if (equipcb33.SelectedIndex > -1)
                    calcequip(3, equipindex33);
                if (equip[equipindex33].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb33.Foreground = br;
                }
                else if (equip[equipindex33].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb33.Foreground = br;
                }
                else if (equip[equipindex33].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb33.Foreground = br;
                }
                else if (equip[equipindex33].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb33.Foreground = br;
                }
                setequiptooltips(33, equipindex33);
            }
            renewskill();
        }
        /// <summary>
        ///41装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb41_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo4.SelectedIndex;
            clearequip(4);
            if (equipcb41.SelectedItem != null)
            {
                string equipselect41 = equipcb41.SelectedItem.ToString();
                int equipindex41 = getequipindex(equipselect41.Substring(31));
                if (equipindex41 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb42.SelectedItem != null && equipcb42.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb42.SelectedItem.ToString().Substring(31))].type == equip[equipindex41].type) {
                                equipcb41.SelectedIndex = 0; return; }
                    }
                if (equipcb41.SelectedIndex > -1)
                    calcequip(4, equipindex41);
                if (equip[equipindex41].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb41.Foreground = br;
                }
                else if (equip[equipindex41].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb41.Foreground = br;
                }
                else if (equip[equipindex41].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb41.Foreground = br;
                }
                else if (equip[equipindex41].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb41.Foreground = br;
                }
                setequiptooltips(41, equipindex41);
            }
            if (equipcb42.SelectedItem != null)
            {
                string equipselect42 = equipcb42.SelectedItem.ToString();
                int equipindex42 = getequipindex(equipselect42.Substring(31));
                if (equipindex42 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb43.SelectedItem != null && equipcb43.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb43.SelectedItem.ToString().Substring(31))].type == equip[equipindex42].type) {
                                equipcb41.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb41.SelectedItem != null && equipcb41.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb41.SelectedItem.ToString().Substring(31))].type == equip[equipindex42].type) {
                                equipcb41.SelectedIndex = 0; return; }
                    }
                if (equipcb41.SelectedIndex > -1)
                    calcequip(4, equipindex42);
            }
            if (equipcb43.SelectedItem != null)
            {
                string equipselect43 = equipcb43.SelectedItem.ToString();
                int equipindex43 = getequipindex(equipselect43.Substring(31));
                if (equipindex43 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb42.SelectedItem != null && equipcb42.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb42.SelectedItem.ToString().Substring(31))].type == equip[equipindex43].type) {
                                equipcb41.SelectedIndex = 0; return; }
                    }

                if (equipcb41.SelectedIndex > -1)
                    calcequip(4, equipindex43);
            }
            renewskill();
        }
        /// <summary>
        ///42装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb42_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo4.SelectedIndex;
            clearequip(4);
            if (equipcb41.SelectedItem != null)
            {
                string equipselect41 = equipcb41.SelectedItem.ToString();
                int equipindex41 = getequipindex(equipselect41.Substring(31));
                if (equipindex41 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb42.SelectedItem != null && equipcb42.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb42.SelectedItem.ToString().Substring(31))].type == equip[equipindex41].type) {
                                equipcb42.SelectedIndex = 0; return; }
                    }
                if (equipcb42.SelectedIndex > -1 )
                    calcequip(4, equipindex41);
            }
            if (equipcb42.SelectedItem != null)
            {
                string equipselect42 = equipcb42.SelectedItem.ToString();
                int equipindex42 = getequipindex(equipselect42.Substring(31));
                if (equipindex42 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb43.SelectedItem != null && equipcb43.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb43.SelectedItem.ToString().Substring(31))].type == equip[equipindex42].type) {
                                equipcb42.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb41.SelectedItem != null && equipcb41.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb41.SelectedItem.ToString().Substring(31))].type == equip[equipindex42].type) {
                                equipcb42.SelectedIndex = 0; return; }
                    }

                if (equipcb42.SelectedIndex > -1)
                    calcequip(4, equipindex42);
                if (equip[equipindex42].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb42.Foreground = br;
                }
                else if (equip[equipindex42].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb42.Foreground = br;
                }
                else if (equip[equipindex42].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb42.Foreground = br;
                }
                else if (equip[equipindex42].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb42.Foreground = br;
                }
                setequiptooltips(42, equipindex42);
            }
            if (equipcb43.SelectedItem != null)
            {
                string equipselect43 = equipcb43.SelectedItem.ToString();
                int equipindex43 = getequipindex(equipselect43.Substring(31));
                if (equipindex43 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb42.SelectedItem != null && equipcb42.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb42.SelectedItem.ToString().Substring(31))].type == equip[equipindex43].type) {
                                equipcb42.SelectedIndex = 0; return; }
                    }

                if (equipcb42.SelectedIndex > -1)
                    calcequip(4, equipindex43);
            }
            renewskill();
        }
        /// <summary>
        ///43装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb43_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo4.SelectedIndex;
            clearequip(4);
            if (equipcb41.SelectedItem != null)
            {
                string equipselect41 = equipcb41.SelectedItem.ToString();
                int equipindex41 = getequipindex(equipselect41.Substring(31));
                if (equipindex41 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb42.SelectedItem != null && equipcb42.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb42.SelectedItem.ToString().Substring(31))].type == equip[equipindex41].type) {
                                equipcb43.SelectedIndex = 0; return; }
                    }
                if (equipcb43.SelectedIndex > -1)
                    calcequip(4, equipindex41);
            }
            if (equipcb42.SelectedItem != null)
            {
                string equipselect42 = equipcb42.SelectedItem.ToString();
                int equipindex42 = getequipindex(equipselect42.Substring(31));
                if (equipindex42 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb43.SelectedItem != null && equipcb43.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb43.SelectedItem.ToString().Substring(31))].type == equip[equipindex42].type) {
                                equipcb43.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb41.SelectedItem != null && equipcb41.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb41.SelectedItem.ToString().Substring(31))].type == equip[equipindex42].type) {
                                equipcb43.SelectedIndex = 0; return; }
                    }
                if (equipcb43.SelectedIndex > -1)
                    calcequip(4, equipindex42);
            }
            if (equipcb43.SelectedItem != null)
            {
                string equipselect43 = equipcb43.SelectedItem.ToString();
                int equipindex43 = getequipindex(equipselect43.Substring(31));
                if (equipindex43 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb42.SelectedItem != null && equipcb42.SelectedItem.ToString().Substring(31) != " ")
                            if (equip[getequipindex(equipcb42.SelectedItem.ToString().Substring(31))].type == equip[equipindex43].type) {
                                equipcb43.SelectedIndex = 0; return; }
                    }

                if (equipcb43.SelectedIndex > -1)
                    calcequip(4, equipindex43);
                if (equip[equipindex43].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb43.Foreground = br;
                }
                else if (equip[equipindex43].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb43.Foreground = br;
                }
                else if (equip[equipindex43].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb43.Foreground = br;
                }
                else if (equip[equipindex43].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb43.Foreground = br;
                }
                setequiptooltips(43, equipindex43);
            }
            renewskill();
        }
        /// <summary>
        ///51装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb51_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo5.SelectedIndex;
            clearequip(5);
            if (equipcb51.SelectedItem != null)
            {
                string equipselect51 = equipcb51.SelectedItem.ToString();
                int equipindex51 = getequipindex(equipselect51.Substring(31));
                if (equipindex51 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb52.SelectedItem != null && equipcb52.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb52.SelectedItem.ToString().Substring(31))].type == equip[equipindex51].type) {
                                equipcb51.SelectedIndex = 0; return; }
                    }
                if (equipcb51.SelectedIndex > -1)
                    calcequip(5, equipindex51);
                if (equip[equipindex51].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb51.Foreground = br;
                }
                else if (equip[equipindex51].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb51.Foreground = br;
                }
                else if (equip[equipindex51].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb51.Foreground = br;
                }
                else if (equip[equipindex51].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb51.Foreground = br;
                }
                setequiptooltips(51, equipindex51);
            }
            if (equipcb52.SelectedItem != null)
            {
                string equipselect52 = equipcb52.SelectedItem.ToString();
                int equipindex52 = getequipindex(equipselect52.Substring(31));
                if (equipindex52 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb53.SelectedItem != null && equipcb53.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb53.SelectedItem.ToString().Substring(31))].type == equip[equipindex52].type) {
                                equipcb51.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb51.SelectedItem != null && equipcb51.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb51.SelectedItem.ToString().Substring(31))].type == equip[equipindex52].type) {
                                equipcb51.SelectedIndex = 0; return; }
                    }
                if (equipcb51.SelectedIndex > -1)
                    calcequip(5, equipindex52);
            }
            if (equipcb53.SelectedItem != null)
            {
                string equipselect53 = equipcb53.SelectedItem.ToString();
                int equipindex53 = getequipindex(equipselect53.Substring(31));
                if (equipindex53 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb52.SelectedItem != null && equipcb52.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb52.SelectedItem.ToString().Substring(31))].type == equip[equipindex53].type) {
                                equipcb51.SelectedIndex = 0; return; }
                    }

                if (equipcb51.SelectedIndex > -1)
                    calcequip(5, equipindex53);
            }
            renewskill();
        }
        /// <summary>
        ///52装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb52_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo5.SelectedIndex;
            clearequip(5);
            if (equipcb51.SelectedItem != null)
            {
                string equipselect51 = equipcb51.SelectedItem.ToString();
                int equipindex51 = getequipindex(equipselect51.Substring(31));
                if (equipindex51 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb52.SelectedItem != null && equipcb52.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb52.SelectedItem.ToString().Substring(31))].type == equip[equipindex51].type) {
                                equipcb52.SelectedIndex = 0; return; }
                    }
                if (equipcb52.SelectedIndex > -1)
                    calcequip(5, equipindex51);
            }
            if (equipcb52.SelectedItem != null)
            {
                string equipselect52 = equipcb52.SelectedItem.ToString();
                int equipindex52 = getequipindex(equipselect52.Substring(31));
                if (equipindex52 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb53.SelectedItem != null && equipcb53.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb53.SelectedItem.ToString().Substring(31))].type == equip[equipindex52].type) {
                                equipcb52.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb51.SelectedItem != null && equipcb51.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb51.SelectedItem.ToString().Substring(31))].type == equip[equipindex52].type) {
                                equipcb52.SelectedIndex = 0; return; }
                    }
                if (equipcb52.SelectedIndex > -1)
                    calcequip(5, equipindex52);
                if (equip[equipindex52].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb52.Foreground = br;
                }
                else if (equip[equipindex52].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb52.Foreground = br;
                }
                else if (equip[equipindex52].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb52.Foreground = br;
                }
                else if (equip[equipindex52].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb52.Foreground = br;
                }
                setequiptooltips(52, equipindex52);
            }
            if (equipcb53.SelectedItem != null)
            {
                string equipselect53 = equipcb53.SelectedItem.ToString();
                int equipindex53 = getequipindex(equipselect53.Substring(31));
                if (equipindex53 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb52.SelectedItem != null && equipcb52.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb52.SelectedItem.ToString().Substring(31))].type == equip[equipindex53].type) {
                                equipcb52.SelectedIndex = 0; return; }
                    }

                if (equipcb52.SelectedIndex > -1)
                    calcequip(5, equipindex53);
            }
            renewskill();
        }
        /// <summary>
        ///53装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb53_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo5.SelectedIndex;
            clearequip(5);
            if (equipcb51.SelectedItem != null)
            {
                string equipselect51 = equipcb51.SelectedItem.ToString();
                int equipindex51 = getequipindex(equipselect51.Substring(31));
                if (equipindex51 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb52.SelectedItem != null && equipcb52.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb52.SelectedItem.ToString().Substring(31))].type == equip[equipindex51].type) {
                                equipcb53.SelectedIndex = 0; return; }
                    }
                if (equipcb53.SelectedIndex > -1)
                    calcequip(5, equipindex51);
            }
            if (equipcb52.SelectedItem != null)
            {
                string equipselect52 = equipcb52.SelectedItem.ToString();
                int equipindex52 = getequipindex(equipselect52.Substring(31));
                if (equipindex52 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb53.SelectedItem != null && equipcb53.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb53.SelectedItem.ToString().Substring(31))].type == equip[equipindex52].type) {
                                equipcb53.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb51.SelectedItem != null && equipcb51.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb51.SelectedItem.ToString().Substring(31))].type == equip[equipindex52].type) {
                                equipcb53.SelectedIndex = 0; return; }
                    }
                if (equipcb53.SelectedIndex > -1)
                    calcequip(5, equipindex52);
            }
            if (equipcb53.SelectedItem != null)
            {
                string equipselect53 = equipcb53.SelectedItem.ToString();
                int equipindex53 = getequipindex(equipselect53.Substring(31));
                if (equipindex53 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb52.SelectedItem != null && equipcb52.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb52.SelectedItem.ToString().Substring(31))].type == equip[equipindex53].type) {
                                equipcb53.SelectedIndex = 0; return; }
                    }

                if (equipcb53.SelectedIndex > -1)
                    calcequip(5, equipindex53);
                if (equip[equipindex53].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb53.Foreground = br;
                }
                else if (equip[equipindex53].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb53.Foreground = br;
                }
                else if (equip[equipindex53].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb53.Foreground = br;
                }
                else if (equip[equipindex53].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb53.Foreground = br;
                }
                setequiptooltips(53, equipindex53);
            }
            renewskill();
        }
        /// <summary>
        ///61装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb61_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo6.SelectedIndex;
            clearequip(6);
            if (equipcb61.SelectedItem != null)
            {
                string equipselect61 = equipcb61.SelectedItem.ToString();
                int equipindex61 = getequipindex(equipselect61.Substring(31));
                if (equipindex61 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb62.SelectedItem != null && equipcb62.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb62.SelectedItem.ToString().Substring(31))].type == equip[equipindex61].type) {
                                equipcb61.SelectedIndex = 0; return; }
                    }
                if (equipcb61.SelectedIndex > -1)
                    calcequip(6, equipindex61);
                if (equip[equipindex61].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb61.Foreground = br;
                }
                else if (equip[equipindex61].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb61.Foreground = br;
                }
                else if (equip[equipindex61].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb61.Foreground = br;
                }
                else if (equip[equipindex61].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb61.Foreground = br;
                }
                setequiptooltips(61, equipindex61);
            }
            if (equipcb62.SelectedItem != null)
            {
                string equipselect62 = equipcb62.SelectedItem.ToString();
                int equipindex62 = getequipindex(equipselect62.Substring(31));
                if (equipindex62 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb63.SelectedItem != null && equipcb63.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb63.SelectedItem.ToString().Substring(31))].type == equip[equipindex62].type) {
                                equipcb61.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb61.SelectedItem != null && equipcb61.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb61.SelectedItem.ToString().Substring(31))].type == equip[equipindex62].type) {
                                equipcb61.SelectedIndex = 0; return; }
                    }
                if (equipcb61.SelectedIndex > -1)
                    calcequip(6, equipindex62);
            }
            if (equipcb63.SelectedItem != null)
            {
                string equipselect63 = equipcb63.SelectedItem.ToString();
                int equipindex63 = getequipindex(equipselect63.Substring(31));
                if (equipindex63 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb62.SelectedItem != null && equipcb62.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb62.SelectedItem.ToString().Substring(31))].type == equip[equipindex63].type) {
                                equipcb61.SelectedIndex = 0; return; }
                    }

                if (equipcb61.SelectedIndex > -1)
                    calcequip(6, equipindex63);
            }
            renewskill();
        }
        /// <summary>
        ///62装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb62_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int comboindex = Combo6.SelectedIndex;
            clearequip(6);
            if (equipcb61.SelectedItem != null)
            {
                string equipselect61 = equipcb61.SelectedItem.ToString();
                int equipindex61 = getequipindex(equipselect61.Substring(31));
                if (equipindex61 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb62.SelectedItem != null && equipcb62.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb62.SelectedItem.ToString().Substring(31))].type == equip[equipindex61].type) {
                                equipcb62.SelectedIndex = 0; return; }
                    }
                if (equipcb62.SelectedIndex > -1)
                    calcequip(6, equipindex61);
            }
            if (equipcb62.SelectedItem != null)
            {
                string equipselect62 = equipcb62.SelectedItem.ToString();
                int equipindex62 = getequipindex(equipselect62.Substring(31));
                if (equipindex62 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb63.SelectedItem != null && equipcb63.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb63.SelectedItem.ToString().Substring(31))].type == equip[equipindex62].type) {
                                equipcb62.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb61.SelectedItem != null && equipcb61.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb61.SelectedItem.ToString().Substring(31))].type == equip[equipindex62].type) {
                                equipcb62.SelectedIndex = 0; return; }
                    }
                if (equipcb62.SelectedIndex > -1)
                    calcequip(6, equipindex62);
                if (equip[equipindex62].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb62.Foreground = br;
                }
                else if (equip[equipindex62].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb62.Foreground = br;
                }
                else if (equip[equipindex62].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb62.Foreground = br;
                }
                else if (equip[equipindex62].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb62.Foreground = br;
                }
                setequiptooltips(62, equipindex62);
            }
            if (equipcb63.SelectedItem != null)
            {
                string equipselect63 = equipcb63.SelectedItem.ToString();
                int equipindex63 = getequipindex(equipselect63.Substring(31));
                if (equipindex63 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb62.SelectedItem != null && equipcb62.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb62.SelectedItem.ToString().Substring(31))].type == equip[equipindex63].type) {
                                equipcb62.SelectedIndex = 0; return; }
                    }

                if (equipcb62.SelectedIndex > -1)
                    calcequip(6, equipindex63);

            }
            renewskill();
        }
        /// <summary>
        ///63装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb63_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo6.SelectedIndex;
            clearequip(6);
            if (equipcb61.SelectedItem != null)
            {
                string equipselect61 = equipcb61.SelectedItem.ToString();
                int equipindex61 = getequipindex(equipselect61.Substring(31));
                if (equipindex61 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb62.SelectedItem != null && equipcb62.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb62.SelectedItem.ToString().Substring(31))].type == equip[equipindex61].type) {
                                equipcb63.SelectedIndex = 0; return; }
                    }
                if (equipcb63.SelectedIndex > -1)
                    calcequip(6, equipindex61);
            }
            if (equipcb62.SelectedItem != null)
            {
                string equipselect62 = equipcb62.SelectedItem.ToString();
                int equipindex62 = getequipindex(equipselect62.Substring(31));
                if (equipindex62 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb63.SelectedItem != null && equipcb63.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb63.SelectedItem.ToString().Substring(31))].type == equip[equipindex62].type) {
                                equipcb63.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb61.SelectedItem != null && equipcb61.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb61.SelectedItem.ToString().Substring(31))].type == equip[equipindex62].type) {
                                equipcb63.SelectedIndex = 0; return; }
                    }
                if (equipcb63.SelectedIndex > -1)
                    calcequip(6, equipindex62);
            }
            if (equipcb63.SelectedItem != null)
            {
                string equipselect63 = equipcb63.SelectedItem.ToString();
                int equipindex63 = getequipindex(equipselect63.Substring(31));
                if (equipindex63 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb62.SelectedItem != null && equipcb62.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb62.SelectedItem.ToString().Substring(31))].type == equip[equipindex63].type) {
                                equipcb63.SelectedIndex = 0; return; }
                    }

                if (equipcb63.SelectedIndex > -1)
                    calcequip(6, equipindex63);
                if (equip[equipindex63].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb63.Foreground = br;
                }
                else if (equip[equipindex63].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb63.Foreground = br;
                }
                else if (equip[equipindex63].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb63.Foreground = br;
                }
                else if (equip[equipindex63].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb63.Foreground = br;
                }
                setequiptooltips(63, equipindex63);
            }
            renewskill();
        }
        /// <summary>
        ///71装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb71_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo7.SelectedIndex;
            clearequip(7);
            if (equipcb71.SelectedItem != null)
            {
                string equipselect71 = equipcb71.SelectedItem.ToString();
                int equipindex71 = getequipindex(equipselect71.Substring(31));
                if (equipindex71 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb72.SelectedItem != null && equipcb72.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb72.SelectedItem.ToString().Substring(31))].type == equip[equipindex71].type) {
                                equipcb71.SelectedIndex = 0; return; }
                    }
                if (equipcb71.SelectedIndex > -1)
                    calcequip(7, equipindex71);
                if (equip[equipindex71].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb71.Foreground = br;
                }
                else if (equip[equipindex71].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb71.Foreground = br;
                }
                else if (equip[equipindex71].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb71.Foreground = br;
                }
                else if (equip[equipindex71].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb71.Foreground = br;
                }
                setequiptooltips(71, equipindex71);
            }
            if (equipcb72.SelectedItem != null)
            {
                string equipselect72 = equipcb72.SelectedItem.ToString();
                int equipindex72 = getequipindex(equipselect72.Substring(31));
                if (equipindex72 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb73.SelectedItem != null && equipcb73.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb73.SelectedItem.ToString().Substring(31))].type == equip[equipindex72].type) {
                                equipcb71.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb71.SelectedItem != null && equipcb71.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb71.SelectedItem.ToString().Substring(31))].type == equip[equipindex72].type) {
                                equipcb71.SelectedIndex = 0; return; }
                    }
                if (equipcb71.SelectedIndex > -1)
                    calcequip(7, equipindex72);
            }
            if (equipcb73.SelectedItem != null)
            {
                string equipselect73 = equipcb73.SelectedItem.ToString();
                int equipindex73 = getequipindex(equipselect73.Substring(31));
                if (equipindex73 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb72.SelectedItem != null && equipcb72.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb72.SelectedItem.ToString().Substring(31))].type == equip[equipindex73].type) {
                                equipcb71.SelectedIndex = 0; return; }
                    }

                if (equipcb71.SelectedIndex > -1)
                    calcequip(7, equipindex73);
            }
            renewskill();
        }
        /// <summary>
        ///72装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb72_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo7.SelectedIndex;
            clearequip(7);
            if (equipcb71.SelectedItem != null)
            {
                string equipselect71 = equipcb71.SelectedItem.ToString();
                int equipindex71 = getequipindex(equipselect71.Substring(31));
                if (equipindex71 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb72.SelectedItem != null && equipcb72.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb72.SelectedItem.ToString().Substring(31))].type == equip[equipindex71].type) {
                                equipcb72.SelectedIndex = 0; return; }
                    }
                if (equipcb72.SelectedIndex > -1)
                    calcequip(7, equipindex71);
            }
            if (equipcb72.SelectedItem != null)
            {
                string equipselect72 = equipcb72.SelectedItem.ToString();
                int equipindex72 = getequipindex(equipselect72.Substring(31));
                if (equipindex72 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb73.SelectedItem != null && equipcb73.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb73.SelectedItem.ToString().Substring(31))].type == equip[equipindex72].type) {
                                equipcb72.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb71.SelectedItem != null && equipcb71.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb71.SelectedItem.ToString().Substring(31))].type == equip[equipindex72].type) {
                                equipcb72.SelectedIndex = 0; return; }
                    }
                if (equipcb72.SelectedIndex > -1)
                    calcequip(7, equipindex72);
                if (equip[equipindex72].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb72.Foreground = br;
                }
                else if (equip[equipindex72].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb72.Foreground = br;
                }
                else if (equip[equipindex72].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb72.Foreground = br;
                }
                else if (equip[equipindex72].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb72.Foreground = br;
                }
                setequiptooltips(72, equipindex72);
            }
            if (equipcb73.SelectedItem != null)
            {
                string equipselect73 = equipcb73.SelectedItem.ToString();
                int equipindex73 = getequipindex(equipselect73.Substring(31));
                if (equipindex73 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb72.SelectedItem != null && equipcb72.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb72.SelectedItem.ToString().Substring(31))].type == equip[equipindex73].type) {
                                equipcb72.SelectedIndex = 0; return; }
                    }

                if (equipcb72.SelectedIndex > -1)
                    calcequip(7, equipindex73);
            }
            renewskill();

        }
        /// <summary>
        ///73装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb73_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo7.SelectedIndex;
            clearequip(7);
            if (equipcb71.SelectedItem != null)
            {
                string equipselect71 = equipcb71.SelectedItem.ToString();
                int equipindex71 = getequipindex(equipselect71.Substring(31));
                if (equipindex71 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb72.SelectedItem != null && equipcb72.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb72.SelectedItem.ToString().Substring(31))].type == equip[equipindex71].type) {
                                equipcb73.SelectedIndex = 0; return; }
                    }
                if (equipcb73.SelectedIndex > -1)
                    calcequip(7, equipindex71);
            }
            if (equipcb72.SelectedItem != null)
            {
                string equipselect72 = equipcb72.SelectedItem.ToString();
                int equipindex72 = getequipindex(equipselect72.Substring(31));
                if (equipindex72 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb73.SelectedItem != null && equipcb73.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb73.SelectedItem.ToString().Substring(31))].type == equip[equipindex72].type) {
                                equipcb73.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb71.SelectedItem != null && equipcb71.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb71.SelectedItem.ToString().Substring(31))].type == equip[equipindex72].type) {
                                equipcb73.SelectedIndex = 0; return; }
                    }
                if (equipcb73.SelectedIndex > -1)
                    calcequip(7, equipindex72);
            }
            if (equipcb73.SelectedItem != null)
            {
                string equipselect73 = equipcb73.SelectedItem.ToString();
                int equipindex73 = getequipindex(equipselect73.Substring(31));
                if (equipindex73 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb72.SelectedItem != null && equipcb72.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb72.SelectedItem.ToString().Substring(31))].type == equip[equipindex73].type) {
                                equipcb73.SelectedIndex = 0; return; }
                    }

                if (equipcb73.SelectedIndex > -1)
                    calcequip(7, equipindex73);
                if (equip[equipindex73].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb73.Foreground = br;
                }
                else if (equip[equipindex73].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb73.Foreground = br;
                }
                else if (equip[equipindex73].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb73.Foreground = br;
                }
                else if (equip[equipindex73].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb73.Foreground = br;
                }
                setequiptooltips(73, equipindex73);
            }
            renewskill();
        }
        /// <summary>
        ///81装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb81_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo8.SelectedIndex;
            clearequip(8);
            if (equipcb81.SelectedItem != null)
            {
                string equipselect81 = equipcb81.SelectedItem.ToString();
                int equipindex81 = getequipindex(equipselect81.Substring(31));
                if (equipindex81 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb82.SelectedItem != null && equipcb82.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb82.SelectedItem.ToString().Substring(31))].type == equip[equipindex81].type) {
                                equipcb81.SelectedIndex = 0; return; }
                    }
                if (equipcb81.SelectedIndex > -1)
                    calcequip(8, equipindex81);
                if (equip[equipindex81].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb81.Foreground = br;
                }
                else if (equip[equipindex81].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb81.Foreground = br;
                }
                else if (equip[equipindex81].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb81.Foreground = br;
                }
                else if (equip[equipindex81].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb81.Foreground = br;
                }
                setequiptooltips(81, equipindex81);
            }
            if (equipcb82.SelectedItem != null)
            {
                string equipselect82 = equipcb82.SelectedItem.ToString();
                int equipindex82 = getequipindex(equipselect82.Substring(31));
                if (equipindex82 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb83.SelectedItem != null && equipcb83.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb83.SelectedItem.ToString().Substring(31))].type == equip[equipindex82].type) {
                                equipcb81.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb81.SelectedItem != null && equipcb81.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb81.SelectedItem.ToString().Substring(31))].type == equip[equipindex82].type) {
                                equipcb81.SelectedIndex = 0; return; }
                    }
                if (equipcb81.SelectedIndex > -1)
                    calcequip(8, equipindex82);
            }
            if (equipcb83.SelectedItem != null)
            {
                string equipselect83 = equipcb83.SelectedItem.ToString();
                int equipindex83 = getequipindex(equipselect83.Substring(31));
                if (equipindex83 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb82.SelectedItem != null && equipcb82.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb82.SelectedItem.ToString().Substring(31))].type == equip[equipindex83].type) {
                                equipcb81.SelectedIndex = 0; return; }
                    }

                if (equipcb81.SelectedIndex > -1)
                    calcequip(8, equipindex83);
            }
            renewskill();
        }
        /// <summary>
        ///82装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb82_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo8.SelectedIndex;
            clearequip(8);
            if (equipcb81.SelectedItem != null)
            {
                string equipselect81 = equipcb81.SelectedItem.ToString();
                int equipindex81 = getequipindex(equipselect81.Substring(31));
                if (equipindex81 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb82.SelectedItem != null && equipcb82.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb82.SelectedItem.ToString().Substring(31))].type == equip[equipindex81].type) {
                                equipcb82.SelectedIndex = 0; return; }
                    }
                if (equipcb82.SelectedIndex > -1)
                    calcequip(8, equipindex81);
            }
            if (equipcb82.SelectedItem != null)
            {
                string equipselect82 = equipcb82.SelectedItem.ToString();
                int equipindex82 = getequipindex(equipselect82.Substring(31));
                if (equipindex82 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb83.SelectedItem != null && equipcb83.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb83.SelectedItem.ToString().Substring(31))].type == equip[equipindex82].type) {
                                equipcb82.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb81.SelectedItem != null && equipcb81.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb81.SelectedItem.ToString().Substring(31))].type == equip[equipindex82].type) {
                                equipcb82.SelectedIndex = 0; return; }
                    }
                if (equipcb82.SelectedIndex > -1)
                    calcequip(8, equipindex82);
                if (equip[equipindex82].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb82.Foreground = br;
                }
                else if (equip[equipindex82].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb82.Foreground = br;
                }
                else if (equip[equipindex82].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb82.Foreground = br;
                }
                else if (equip[equipindex82].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb82.Foreground = br;
                }
                setequiptooltips(82, equipindex82);
            }
            if (equipcb83.SelectedItem != null)
            {
                string equipselect83 = equipcb83.SelectedItem.ToString();
                int equipindex83 = getequipindex(equipselect83.Substring(31));
                if (equipindex83 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb82.SelectedItem != null && equipcb82.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb82.SelectedItem.ToString().Substring(31))].type == equip[equipindex83].type) {
                                equipcb82.SelectedIndex = 0; return; }
                    }

                if (equipcb82.SelectedIndex > -1)
                    calcequip(8, equipindex83);
            }
            renewskill();
        }
        /// <summary>
        ///83装备格选项变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void equipcb83_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboindex = Combo8.SelectedIndex;
            clearequip(8);
            if (equipcb81.SelectedItem != null)
            {
                string equipselect81 = equipcb81.SelectedItem.ToString();
                int equipindex81 = getequipindex(equipselect81.Substring(31));
                if (equipindex81 != -1)
                    if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb82.SelectedItem != null && equipcb82.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb82.SelectedItem.ToString().Substring(31))].type == equip[equipindex81].type) {
                                equipcb83.SelectedIndex = 0; return; }
                    }
                if (equipcb83.SelectedIndex > -1)
                    calcequip(8, equipindex81);
            }
            if (equipcb82.SelectedItem != null)
            {
                string equipselect82 = equipcb82.SelectedItem.ToString();
                int equipindex82 = getequipindex(equipselect82.Substring(31));
                if (equipindex82 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb83.SelectedItem != null && equipcb83.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb83.SelectedItem.ToString().Substring(31))].type == equip[equipindex82].type) {
                                equipcb83.SelectedIndex = 0; return; }
                    }
                    else if (comboindex == 29 || comboindex == 34)
                    {
                        if (equipcb81.SelectedItem != null && equipcb81.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb81.SelectedItem.ToString().Substring(31))].type == equip[equipindex82].type) {
                                equipcb83.SelectedIndex = 0; return; }
                    }
                if (equipcb83.SelectedIndex > -1)
                    calcequip(8, equipindex82);
            }
            if (equipcb83.SelectedItem != null)
            {
                string equipselect83 = equipcb83.SelectedItem.ToString();
                int equipindex83 = getequipindex(equipselect83.Substring(31));
                if (equipindex83 != -1)
                    if (comboindex == 28)           //16
                    {
                        if (equipcb82.SelectedItem != null && equipcb82.SelectedItem.ToString().Substring(31)!= " ")
                            if (equip[getequipindex(equipcb82.SelectedItem.ToString().Substring(31))].type == equip[equipindex83].type) {
                                equipcb83.SelectedIndex = 0; return; }
                    }

                if (equipcb83.SelectedIndex > -1)
                    calcequip(8, equipindex83);
                if (equip[equipindex83].rank == 2)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                    equipcb83.Foreground = br;
                }
                else if (equip[equipindex83].rank == 3)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
                    equipcb83.Foreground = br;
                }
                else if (equip[equipindex83].rank == 4)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    equipcb83.Foreground = br;
                }
                else if (equip[equipindex83].rank == 5)
                {
                    Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"));
                    equipcb83.Foreground = br;
                }
                setequiptooltips(83, equipindex83);
            }
            renewskill();
        }
        /// <summary>
        /// 点击夜战checkbox事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbIsnight_Click(object sender, RoutedEventArgs e)
        {
            if (cbIsnight.IsChecked == true)
                innight = true;
            else
                innight = false;
            for (int i = 0; i < 9;i++ )
                renewindex(i);
        }
        /// <summary>
        /// 敌方护甲值变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enemyarmor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsNumber(enemyarmor.Text))
                enemyarmor.Text = "0";
            for (int i = 0; i < 9; i++)
            {
                renewindex(i);
            }
                renewskill();
        }
        /// <summary>
        /// 夏活相关按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xhbutton_Click(object sender, RoutedEventArgs e)
        {
            Aboutxh s = new Aboutxh();
            s.ShowDialog();
        }
        /// <summary>
        /// 练级计算器点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calclevelup_Click(object sender, RoutedEventArgs e)
        {
            calclevelup c = new calclevelup();
            c.Show();
        }
        /// <summary>
        /// 敌方伤害变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsNumber(enemydamage.Text))
                enemydamage.Text = "0";
            renewtank();
        }
        /// <summary>
        /// 点击左上格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry0_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[0] == 1.1)
            {
                merry[0] = 0.95;
                Merry0.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry0.Foreground = br;
            }
            else if(merry[0] == 0.95)
            {
                merry[0] = 1;
                Merry0.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry0.Foreground = br;
            }
            else if (merry[0] == 1)
            {
                merry[0] = 1.05;
                Merry0.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry0.Foreground = br;
            }
            else if(merry[0] == 1.05)
            {
                merry[0] = 1.1;
                Merry0.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry0.Foreground = br;
            }

            int select = Combo0.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level0.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel0.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击上格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[1] == 1.1)
            {
                merry[1] = 0.95;
                Merry1.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry1.Foreground = br;
            }
            else if (merry[1] == 0.95)
            {
                merry[1] = 1;
                Merry1.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry1.Foreground = br;
            }
            else if (merry[1] == 1)
            {
                merry[1] = 1.05;
                Merry1.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry1.Foreground = br;
            }
            else if (merry[1] == 1.05)
            {
                merry[1] = 1.1;
                Merry1.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry1.Foreground = br;
            }

            int select = Combo1.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level1.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel1.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击右上格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[2] == 1.1)
            {
                merry[2] = 0.95;
                Merry2.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry2.Foreground = br;
            }
            else if (merry[2] == 0.95)
            {
                merry[2] = 1;
                Merry2.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry2.Foreground = br;
            }
            else if (merry[2] == 1)
            {
                merry[2] = 1.05;
                Merry2.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry2.Foreground = br;
            }
            else if (merry[2] == 1.05)
            {
                merry[2] = 1.1;
                Merry2.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry2.Foreground = br;
            }

            int select = Combo2.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level2.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel2.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击左中格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[3] == 1.1)
            {
                merry[3] = 0.95;
                Merry3.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry3.Foreground = br;
            }
            else if (merry[3] == 0.95)
            {
                merry[3] = 1;
                Merry3.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry3.Foreground = br;
            }
            else if (merry[3] == 1)
            {
                merry[3] = 1.05;
                Merry3.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry3.Foreground = br;
            }
            else if (merry[3] == 1.05)
            {
                merry[3] = 1.1;
                Merry3.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry3.Foreground = br;
            }

            int select = Combo3.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level3.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel3.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击中格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[4] == 1.1)
            {
                merry[4] = 0.95;
                Merry4.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry4.Foreground = br;
            }
            else if (merry[4] == 0.95)
            {
                merry[4] = 1;
                Merry4.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry4.Foreground = br;
            }
            else if (merry[4] == 1)
            {
                merry[4] = 1.05;
                Merry4.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry4.Foreground = br;
            }
            else if (merry[4] == 1.05)
            {
                merry[4] = 1.1;
                Merry4.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry4.Foreground = br;
            }

            int select = Combo4.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level4.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel4.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击右中格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry5_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[5] == 1.1)
            {
                merry[5] = 0.95;
                Merry5.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry5.Foreground = br;
            }
            else if (merry[5] == 0.95)
            {
                merry[5] = 1;
                Merry5.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry5.Foreground = br;
            }
            else if (merry[5] == 1)
            {
                merry[5] = 1.05;
                Merry5.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry5.Foreground = br;
            }
            else if (merry[5] == 1.05)
            {
                merry[5] = 1.1;
                Merry5.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry5.Foreground = br;
            }

            int select = Combo5.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level5.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel5.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击左下格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry6_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[6] == 1.1)
            {
                merry[6] = 0.95;
                Merry6.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry6.Foreground = br;
            }
            else if (merry[6] == 0.95)
            {
                merry[6] = 1;
                Merry6.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry6.Foreground = br;
            }
            else if (merry[6] == 1)
            {
                merry[6] = 1.05;
                Merry6.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry6.Foreground = br;
            }
            else if (merry[6] == 1.05)
            {
                merry[6] = 1.1;
                Merry6.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry6.Foreground = br;
            }

            int select = Combo6.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level6.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel6.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击下格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry7_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[7] == 1.1)
            {
                merry[7] = 0.95;
                Merry7.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry7.Foreground = br;
            }
            else if (merry[7] == 0.95)
            {
                merry[7] = 1;
                Merry7.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry7.Foreground = br;
            }
            else if (merry[7] == 1)
            {
                merry[7] = 1.05;
                Merry7.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry7.Foreground = br;
            }
            else if (merry[7] == 1.05)
            {
                merry[7] = 1.1;
                Merry7.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry7.Foreground = br;
            }

            int select = Combo7.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level7.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel7.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }
        /// <summary>
        /// 点击右下格心事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Merry8_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (merry[8] == 1.1)
            {
                merry[8] = 0.95;
                Merry8.Content = "💔";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Merry8.Foreground = br;
            }
            else if (merry[8] == 0.95)
            {
                merry[8] = 1;
                Merry8.Content = "♡";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                Merry8.Foreground = br;
            }
            else if (merry[8] == 1)
            {
                merry[8] = 1.05;
                Merry8.Content = "❤";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
                Merry8.Foreground = br;
            }
            else if (merry[8] == 1.05)
            {
                merry[8] = 1.1;
                Merry8.Content = "💘";
                Brush br = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                Merry8.Foreground = br;
            }

            int select = Combo8.SelectedIndex;
            if (select == -1 || select == GUN_NUMBER)
                return;
            int levelselect = Level8.SelectedIndex;
            if (levelselect == -1 || levelselect == 100)
                return;
            int skillselect = SkillLevel8.SelectedIndex;
            if (skillselect == -1)
                return;
            renewskill();
        }

        private void showbuff(int combo,int index)
        {
            if (index == -1)
                return;
            string tbt = "";
            switch (gun[index].to)
            {
                case 1:
                    {
                        tbt += "对所有枪种：";
                        break;
                    }
                case 2:
                    {
                        tbt += "对突击步枪：";
                        break;
                    }
                case 3:
                    {
                        tbt += "对冲锋枪：";
                        break;
                    }
                case 4:
                    {
                        tbt += "对手枪：";
                        break;
                    }
                case 5:
                    {
                        tbt += "对狙击枪：";
                        break;
                    }
                case 6:
                    {
                        tbt += "对机枪：";
                        break;
                    }
                default: break;
            }
            if (gun[index].damageup != 0)
                tbt += "伤害+" + ((gun[index].damageup) * 100).ToString("0") + "% ";
            if (gun[index].shotspeedup != 0)
                tbt += "射速+" + ((gun[index].shotspeedup) * 100).ToString("0") + "% ";
            if (gun[index].hitup != 0)
                tbt += "命中+" + ((gun[index].hitup) * 100).ToString("0") + "% ";
            if (gun[index].dodgeup != 0)
                tbt += "回避+" + ((gun[index].dodgeup) * 100).ToString("0") + "% ";
            if (gun[index].critup != 0)
                tbt += "暴击率+" + ((gun[index].critup) * 100).ToString("0") + "% ";
            if (gun[index].rateup != 0)
                tbt += "发动率+" + (gun[index].rateup * 100).ToString("0") + "% ";
            if (tbt == "")
                tbt = "无";
            Brush br = new SolidColorBrush(Color.FromRgb(0,255,222));
            switch (combo)
            {
                case 0:
                    {
                        clearshowbuff(0);
                        buffGrids0.ToolTip = tbt;
                        switch(gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    gridlist[0][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    gridlist[0][gun[index].effect1 - 1].Background = br;
                                    gridlist[0][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    gridlist[0][gun[index].effect1 - 1].Background = br;
                                    gridlist[0][gun[index].effect2 - 1].Background = br;
                                    gridlist[0][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    gridlist[0][gun[index].effect1 - 1].Background = br;
                                    gridlist[0][gun[index].effect2 - 1].Background = br;
                                    gridlist[0][gun[index].effect3 - 1].Background = br;
                                    gridlist[0][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    gridlist[0][gun[index].effect1 - 1].Background = br;
                                    gridlist[0][gun[index].effect2 - 1].Background = br;
                                    gridlist[0][gun[index].effect3 - 1].Background = br;
                                    gridlist[0][gun[index].effect4 - 1].Background = br;
                                    gridlist[0][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    gridlist[0][gun[index].effect1 - 1].Background = br;
                                    gridlist[0][gun[index].effect2 - 1].Background = br;
                                    gridlist[0][gun[index].effect3 - 1].Background = br;
                                    gridlist[0][gun[index].effect4 - 1].Background = br;
                                    gridlist[0][gun[index].effect5 - 1].Background = br;
                                    gridlist[0][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[0][gun[index].effect0 - 1].Background = br;
                                    gridlist[0][gun[index].effect1 - 1].Background = br;
                                    gridlist[0][gun[index].effect2 - 1].Background = br;
                                    gridlist[0][gun[index].effect3 - 1].Background = br;
                                    gridlist[0][gun[index].effect4 - 1].Background = br;
                                    gridlist[0][gun[index].effect5 - 1].Background = br;
                                    gridlist[0][gun[index].effect6 - 1].Background = br;
                                    gridlist[0][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 1:
                    {
                        clearshowbuff(1);
                        buffGrids1.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    gridlist[1][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    gridlist[1][gun[index].effect1 - 1].Background = br;
                                    gridlist[1][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    gridlist[1][gun[index].effect1 - 1].Background = br;
                                    gridlist[1][gun[index].effect2 - 1].Background = br;
                                    gridlist[1][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    gridlist[1][gun[index].effect1 - 1].Background = br;
                                    gridlist[1][gun[index].effect2 - 1].Background = br;
                                    gridlist[1][gun[index].effect3 - 1].Background = br;
                                    gridlist[1][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    gridlist[1][gun[index].effect1 - 1].Background = br;
                                    gridlist[1][gun[index].effect2 - 1].Background = br;
                                    gridlist[1][gun[index].effect3 - 1].Background = br;
                                    gridlist[1][gun[index].effect4 - 1].Background = br;
                                    gridlist[1][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    gridlist[1][gun[index].effect1 - 1].Background = br;
                                    gridlist[1][gun[index].effect2 - 1].Background = br;
                                    gridlist[1][gun[index].effect3 - 1].Background = br;
                                    gridlist[1][gun[index].effect4 - 1].Background = br;
                                    gridlist[1][gun[index].effect5 - 1].Background = br;
                                    gridlist[1][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[1][gun[index].effect0 - 1].Background = br;
                                    gridlist[1][gun[index].effect1 - 1].Background = br;
                                    gridlist[1][gun[index].effect2 - 1].Background = br;
                                    gridlist[1][gun[index].effect3 - 1].Background = br;
                                    gridlist[1][gun[index].effect4 - 1].Background = br;
                                    gridlist[1][gun[index].effect5 - 1].Background = br;
                                    gridlist[1][gun[index].effect6 - 1].Background = br;
                                    gridlist[1][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 2:
                    {
                        clearshowbuff(2);
                        buffGrids2.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    gridlist[2][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    gridlist[2][gun[index].effect1 - 1].Background = br;
                                    gridlist[2][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    gridlist[2][gun[index].effect1 - 1].Background = br;
                                    gridlist[2][gun[index].effect2 - 1].Background = br;
                                    gridlist[2][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    gridlist[2][gun[index].effect1 - 1].Background = br;
                                    gridlist[2][gun[index].effect2 - 1].Background = br;
                                    gridlist[2][gun[index].effect3 - 1].Background = br;
                                    gridlist[2][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    gridlist[2][gun[index].effect1 - 1].Background = br;
                                    gridlist[2][gun[index].effect2 - 1].Background = br;
                                    gridlist[2][gun[index].effect3 - 1].Background = br;
                                    gridlist[2][gun[index].effect4 - 1].Background = br;
                                    gridlist[2][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    gridlist[2][gun[index].effect1 - 1].Background = br;
                                    gridlist[2][gun[index].effect2 - 1].Background = br;
                                    gridlist[2][gun[index].effect3 - 1].Background = br;
                                    gridlist[2][gun[index].effect4 - 1].Background = br;
                                    gridlist[2][gun[index].effect5 - 1].Background = br;
                                    gridlist[2][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[2][gun[index].effect0 - 1].Background = br;
                                    gridlist[2][gun[index].effect1 - 1].Background = br;
                                    gridlist[2][gun[index].effect2 - 1].Background = br;
                                    gridlist[2][gun[index].effect3 - 1].Background = br;
                                    gridlist[2][gun[index].effect4 - 1].Background = br;
                                    gridlist[2][gun[index].effect5 - 1].Background = br;
                                    gridlist[2][gun[index].effect6 - 1].Background = br;
                                    gridlist[2][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 3:
                    {
                        clearshowbuff(3);
                        buffGrids3.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    gridlist[3][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    gridlist[3][gun[index].effect1 - 1].Background = br;
                                    gridlist[3][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    gridlist[3][gun[index].effect1 - 1].Background = br;
                                    gridlist[3][gun[index].effect2 - 1].Background = br;
                                    gridlist[3][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    gridlist[3][gun[index].effect1 - 1].Background = br;
                                    gridlist[3][gun[index].effect2 - 1].Background = br;
                                    gridlist[3][gun[index].effect3 - 1].Background = br;
                                    gridlist[3][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    gridlist[3][gun[index].effect1 - 1].Background = br;
                                    gridlist[3][gun[index].effect2 - 1].Background = br;
                                    gridlist[3][gun[index].effect3 - 1].Background = br;
                                    gridlist[3][gun[index].effect4 - 1].Background = br;
                                    gridlist[3][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    gridlist[3][gun[index].effect1 - 1].Background = br;
                                    gridlist[3][gun[index].effect2 - 1].Background = br;
                                    gridlist[3][gun[index].effect3 - 1].Background = br;
                                    gridlist[3][gun[index].effect4 - 1].Background = br;
                                    gridlist[3][gun[index].effect5 - 1].Background = br;
                                    gridlist[3][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[3][gun[index].effect0 - 1].Background = br;
                                    gridlist[3][gun[index].effect1 - 1].Background = br;
                                    gridlist[3][gun[index].effect2 - 1].Background = br;
                                    gridlist[3][gun[index].effect3 - 1].Background = br;
                                    gridlist[3][gun[index].effect4 - 1].Background = br;
                                    gridlist[3][gun[index].effect5 - 1].Background = br;
                                    gridlist[3][gun[index].effect6 - 1].Background = br;
                                    gridlist[3][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 4:
                    {
                        clearshowbuff(4);
                        buffGrids4.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    gridlist[4][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    gridlist[4][gun[index].effect1 - 1].Background = br;
                                    gridlist[4][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    gridlist[4][gun[index].effect1 - 1].Background = br;
                                    gridlist[4][gun[index].effect2 - 1].Background = br;
                                    gridlist[4][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    gridlist[4][gun[index].effect1 - 1].Background = br;
                                    gridlist[4][gun[index].effect2 - 1].Background = br;
                                    gridlist[4][gun[index].effect3 - 1].Background = br;
                                    gridlist[4][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    gridlist[4][gun[index].effect1 - 1].Background = br;
                                    gridlist[4][gun[index].effect2 - 1].Background = br;
                                    gridlist[4][gun[index].effect3 - 1].Background = br;
                                    gridlist[4][gun[index].effect4 - 1].Background = br;
                                    gridlist[4][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    gridlist[4][gun[index].effect1 - 1].Background = br;
                                    gridlist[4][gun[index].effect2 - 1].Background = br;
                                    gridlist[4][gun[index].effect3 - 1].Background = br;
                                    gridlist[4][gun[index].effect4 - 1].Background = br;
                                    gridlist[4][gun[index].effect5 - 1].Background = br;
                                    gridlist[4][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[4][gun[index].effect0 - 1].Background = br;
                                    gridlist[4][gun[index].effect1 - 1].Background = br;
                                    gridlist[4][gun[index].effect2 - 1].Background = br;
                                    gridlist[4][gun[index].effect3 - 1].Background = br;
                                    gridlist[4][gun[index].effect4 - 1].Background = br;
                                    gridlist[4][gun[index].effect5 - 1].Background = br;
                                    gridlist[4][gun[index].effect6 - 1].Background = br;
                                    gridlist[4][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 5:
                    {
                        clearshowbuff(5);
                        buffGrids5.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    gridlist[5][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    gridlist[5][gun[index].effect1 - 1].Background = br;
                                    gridlist[5][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    gridlist[5][gun[index].effect1 - 1].Background = br;
                                    gridlist[5][gun[index].effect2 - 1].Background = br;
                                    gridlist[5][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    gridlist[5][gun[index].effect1 - 1].Background = br;
                                    gridlist[5][gun[index].effect2 - 1].Background = br;
                                    gridlist[5][gun[index].effect3 - 1].Background = br;
                                    gridlist[5][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    gridlist[5][gun[index].effect1 - 1].Background = br;
                                    gridlist[5][gun[index].effect2 - 1].Background = br;
                                    gridlist[5][gun[index].effect3 - 1].Background = br;
                                    gridlist[5][gun[index].effect4 - 1].Background = br;
                                    gridlist[5][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    gridlist[5][gun[index].effect1 - 1].Background = br;
                                    gridlist[5][gun[index].effect2 - 1].Background = br;
                                    gridlist[5][gun[index].effect3 - 1].Background = br;
                                    gridlist[5][gun[index].effect4 - 1].Background = br;
                                    gridlist[5][gun[index].effect5 - 1].Background = br;
                                    gridlist[5][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[5][gun[index].effect0 - 1].Background = br;
                                    gridlist[5][gun[index].effect1 - 1].Background = br;
                                    gridlist[5][gun[index].effect2 - 1].Background = br;
                                    gridlist[5][gun[index].effect3 - 1].Background = br;
                                    gridlist[5][gun[index].effect4 - 1].Background = br;
                                    gridlist[5][gun[index].effect5 - 1].Background = br;
                                    gridlist[5][gun[index].effect6 - 1].Background = br;
                                    gridlist[5][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 6:
                    {
                        clearshowbuff(6);
                        buffGrids6.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    gridlist[6][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    gridlist[6][gun[index].effect1 - 1].Background = br;
                                    gridlist[6][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    gridlist[6][gun[index].effect1 - 1].Background = br;
                                    gridlist[6][gun[index].effect2 - 1].Background = br;
                                    gridlist[6][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    gridlist[6][gun[index].effect1 - 1].Background = br;
                                    gridlist[6][gun[index].effect2 - 1].Background = br;
                                    gridlist[6][gun[index].effect3 - 1].Background = br;
                                    gridlist[6][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    gridlist[6][gun[index].effect1 - 1].Background = br;
                                    gridlist[6][gun[index].effect2 - 1].Background = br;
                                    gridlist[6][gun[index].effect3 - 1].Background = br;
                                    gridlist[6][gun[index].effect4 - 1].Background = br;
                                    gridlist[6][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    gridlist[6][gun[index].effect1 - 1].Background = br;
                                    gridlist[6][gun[index].effect2 - 1].Background = br;
                                    gridlist[6][gun[index].effect3 - 1].Background = br;
                                    gridlist[6][gun[index].effect4 - 1].Background = br;
                                    gridlist[6][gun[index].effect5 - 1].Background = br;
                                    gridlist[6][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[6][gun[index].effect0 - 1].Background = br;
                                    gridlist[6][gun[index].effect1 - 1].Background = br;
                                    gridlist[6][gun[index].effect2 - 1].Background = br;
                                    gridlist[6][gun[index].effect3 - 1].Background = br;
                                    gridlist[6][gun[index].effect4 - 1].Background = br;
                                    gridlist[6][gun[index].effect5 - 1].Background = br;
                                    gridlist[6][gun[index].effect6 - 1].Background = br;
                                    gridlist[6][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 7:
                    {
                        clearshowbuff(7);
                        buffGrids7.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    gridlist[7][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    gridlist[7][gun[index].effect1 - 1].Background = br;
                                    gridlist[7][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    gridlist[7][gun[index].effect1 - 1].Background = br;
                                    gridlist[7][gun[index].effect2 - 1].Background = br;
                                    gridlist[7][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    gridlist[7][gun[index].effect1 - 1].Background = br;
                                    gridlist[7][gun[index].effect2 - 1].Background = br;
                                    gridlist[7][gun[index].effect3 - 1].Background = br;
                                    gridlist[7][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    gridlist[7][gun[index].effect1 - 1].Background = br;
                                    gridlist[7][gun[index].effect2 - 1].Background = br;
                                    gridlist[7][gun[index].effect3 - 1].Background = br;
                                    gridlist[7][gun[index].effect4 - 1].Background = br;
                                    gridlist[7][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    gridlist[7][gun[index].effect1 - 1].Background = br;
                                    gridlist[7][gun[index].effect2 - 1].Background = br;
                                    gridlist[7][gun[index].effect3 - 1].Background = br;
                                    gridlist[7][gun[index].effect4 - 1].Background = br;
                                    gridlist[7][gun[index].effect5 - 1].Background = br;
                                    gridlist[7][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[7][gun[index].effect0 - 1].Background = br;
                                    gridlist[7][gun[index].effect1 - 1].Background = br;
                                    gridlist[7][gun[index].effect2 - 1].Background = br;
                                    gridlist[7][gun[index].effect3 - 1].Background = br;
                                    gridlist[7][gun[index].effect4 - 1].Background = br;
                                    gridlist[7][gun[index].effect5 - 1].Background = br;
                                    gridlist[7][gun[index].effect6 - 1].Background = br;
                                    gridlist[7][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 8:
                    {
                        clearshowbuff(8);
                        buffGrids8.ToolTip = tbt;
                        switch (gun[index].number)
                        {
                            case 1:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    break;
                                }
                            case 2:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    gridlist[8][gun[index].effect1 - 1].Background = br;
                                    break;
                                }
                            case 3:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    gridlist[8][gun[index].effect1 - 1].Background = br;
                                    gridlist[8][gun[index].effect2 - 1].Background = br;
                                    break;
                                }
                            case 4:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    gridlist[8][gun[index].effect1 - 1].Background = br;
                                    gridlist[8][gun[index].effect2 - 1].Background = br;
                                    gridlist[8][gun[index].effect3 - 1].Background = br;
                                    break;
                                }
                            case 5:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    gridlist[8][gun[index].effect1 - 1].Background = br;
                                    gridlist[8][gun[index].effect2 - 1].Background = br;
                                    gridlist[8][gun[index].effect3 - 1].Background = br;
                                    gridlist[8][gun[index].effect4 - 1].Background = br;
                                    break;
                                }
                            case 6:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    gridlist[8][gun[index].effect1 - 1].Background = br;
                                    gridlist[8][gun[index].effect2 - 1].Background = br;
                                    gridlist[8][gun[index].effect3 - 1].Background = br;
                                    gridlist[8][gun[index].effect4 - 1].Background = br;
                                    gridlist[8][gun[index].effect5 - 1].Background = br;
                                    break;
                                }
                            case 7:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    gridlist[8][gun[index].effect1 - 1].Background = br;
                                    gridlist[8][gun[index].effect2 - 1].Background = br;
                                    gridlist[8][gun[index].effect3 - 1].Background = br;
                                    gridlist[8][gun[index].effect4 - 1].Background = br;
                                    gridlist[8][gun[index].effect5 - 1].Background = br;
                                    gridlist[8][gun[index].effect6 - 1].Background = br;
                                    break;
                                }
                            case 8:
                                {
                                    gridlist[8][gun[index].effect0 - 1].Background = br;
                                    gridlist[8][gun[index].effect1 - 1].Background = br;
                                    gridlist[8][gun[index].effect2 - 1].Background = br;
                                    gridlist[8][gun[index].effect3 - 1].Background = br;
                                    gridlist[8][gun[index].effect4 - 1].Background = br;
                                    gridlist[8][gun[index].effect5 - 1].Background = br;
                                    gridlist[8][gun[index].effect6 - 1].Background = br;
                                    gridlist[8][gun[index].effect7 - 1].Background = br;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
            }
        }

        private void clearshowbuff(int combo)
        {
            Brush br = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Brush inner = new SolidColorBrush(Color.FromRgb(107, 105, 107));
            switch (combo)
            {
                case 0:
                    {
                        foreach(Border g in gridlist[0])
                        {
                            g.Background = br;
                        }
                        buffGrid05.Background = inner;
                        break;
                    }
                case 1:
                    {
                        foreach (Border g in gridlist[1])
                        {
                            g.Background = br;
                        }
                        buffGrid15.Background = inner;
                        break;
                    }
                case 2:
                    {
                        foreach (Border g in gridlist[2])
                        {
                            g.Background = br;
                        }
                        buffGrid25.Background = inner;
                        break;
                    }
                case 3:
                    {
                        foreach (Border g in gridlist[3])
                        {
                            g.Background = br;
                        }
                        buffGrid35.Background = inner;
                        break;
                    }
                case 4:
                    {
                        foreach (Border g in gridlist[4])
                        {
                            g.Background = br;
                        }
                        buffGrid45.Background = inner;
                        break;
                    }
                case 5:
                    {
                        foreach (Border g in gridlist[5])
                        {
                            g.Background = br;
                        }
                        buffGrid55.Background = inner;
                        break;
                    }
                case 6:
                    {
                        foreach (Border g in gridlist[6])
                        {
                            g.Background = br;
                        }
                        buffGrid65.Background = inner;
                        break;
                    }
                case 7:
                    {
                        foreach (Border g in gridlist[7])
                        {
                            g.Background = br;
                        }
                        buffGrid75.Background = inner;
                        break;
                    }
                case 8:
                    {
                        foreach (Border g in gridlist[8])
                        {
                            g.Background = br;
                        }
                        buffGrid85.Background = inner;
                        break;
                    }
            }
        }
    }
}
