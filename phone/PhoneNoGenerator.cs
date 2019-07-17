using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace phone
{
    public partial class FORM1 : Form
    {
        private String oprName=null;
        private String province;
        private String city;
        private String count=null;
        private int iAmount;
        private int sqlAmount;
        private string[] provinceList= { "北京", "天津", "上海", "广东", "山东", "江苏", "浙江", "安徽", "四川", "陕西", "湖北", "广西", "河南", "甘肃", "吉林", "辽宁", "内蒙古", "新疆", "黑龙江", "福建", "河北", "重庆", "海南", "江西", "山西", "湖南", "青海", "贵州", "宁夏", "云南", "西藏" };
        private string whereProvince;
        private string whereNotCity;
        private string whereCity;
        SQLiteConnection conn = null;
        private int PER_PRE7_MAX = 10000;
        string dbPath = "Data Source =" + Environment.CurrentDirectory + "/phone.db";
        public FORM1()
        {
            InitializeComponent();

            oprBox_Load();

           //cityBox_Load();

            provinceBox_Load();

            listBoxLoad();

            conn = new SQLiteConnection(dbPath);//创建数据库实例，指定文件位置  
            conn.Open();//打开数据库，若文件不存在会自动创建 


        }

        private void oprBox_Load()
        {
            this.oprBox.Items.Add("Y");
            this.oprBox.Items.Add("L");
            this.oprBox.Items.Add("D");
        }

        private void provinceBox_Load()
        {
            for (int i=0; i<31; i++)
            {
                this.provinceBox.Items.Add(provinceList[i]);
            }
        }

        private void listBoxLoad()
        {
            for (int i = 0; i < 31; i++)
            {
                this.listBox1.Items.Add(provinceList[i]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*TODO:
             * √ 1、改为去掉哪些省市
             * √ 2、点击导出时要求输入秘钥
             * √ 3、多省市的混合输出
             * √ 4、生成的文件名称为 年月日时分秒.txt
             * */
            int sum = 0;
            int i = 0;
            string sqlDetail;
            int[] suffix4 = new int[PER_PRE7_MAX];
            int temp;
            int t;
            int indexPre7;
            string[] resultList = new string[440000];
            string sqlCount1;
            string sqlCount2;
            for ( i = 0; i < PER_PRE7_MAX; i++)
            {
                suffix4[i] = i;
            }

            //密码校验
            string passwd = Interaction.InputBox("请输入软件密码，否则无法继续进行", "操作验证", "****************", -1, -1);
            if (!"1Gl83gz".Equals(passwd))
            {
                this.Close();
            }

            count = textBox1.Text.ToString();
            int iCount = int.Parse(count);

            whereNotCity = "(";
            whereCity = "(";
            for (int x = 0; x < listBox2.Items.Count; x++)
            {
                if (listBox2.GetSelected(x)) {
                    whereNotCity += ("'" + listBox2.Items[x].ToString() + "',");
                } else {
                    whereCity+= ("'" + listBox2.Items[x].ToString() + "',");
                }
            }
            whereNotCity = whereNotCity.Substring(0, whereNotCity.Length - 1) + ")";
            whereCity = whereCity.Substring(0, whereCity.Length - 1) + ")";
            //查询数据,且给sqlAmount赋值
            if (null == oprName) {
                sqlCount1 = "select count(*) from phone where city in" + whereCity;
                sqlCount2 = "select count(*) from phone where province not in " + whereProvince;
            } else {
                sqlCount1 = "select count(*) from phone where isp='" + oprName + "' and city in" + whereCity;
                sqlCount2 = "select count(*) from phone where isp='" + oprName + "' and province not in " + whereProvince;
            }

            SQLiteCommand cmdC1 = new SQLiteCommand();
            cmdC1.CommandText = sqlCount1;
            cmdC1.Connection = conn;
            int sqlAmount = Convert.ToInt32(cmdC1.ExecuteScalar());

            SQLiteCommand cmdC2 = new SQLiteCommand();
            cmdC2.CommandText = sqlCount2;
            cmdC2.Connection = conn;
            sqlAmount += +Convert.ToInt32(cmdC2.ExecuteScalar());

            //需要生成iCount/10000+1次
            iAmount = iCount / 10001 + 1;
            //需要生成的号段数量和数据库中的号段数量对比
            if (iAmount > sqlAmount)
            {
                MessageBox.Show("选中地区数据量不足");
                return;
            }

            if (null == oprName) {
                sqlDetail = "select * from phone where city in" + whereCity
                    +"union select * from phone where province not in " + whereProvince;
            } else {
                sqlDetail = "select * from phone where isp='" + oprName + "' and city in" + whereCity
                    + "union select * from phone where province not in " + whereProvince;
            }
            SQLiteCommand cmdQ = new SQLiteCommand(sqlDetail, conn);
            SQLiteDataReader reader = cmdQ.ExecuteReader();

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
                       
            for (i=0; reader.Read(); i++)
            {
                resultList[i] = reader[1].ToString();
            }

            i = 0;
            Random pre7Radom = new Random();
            Random suf4Radom = new Random();
            while ( i < (iCount / 10001 + 1))
            {
                for (int j=0; j< PER_PRE7_MAX && sum < iCount; j++)
                {
                    t = suf4Radom.Next(PER_PRE7_MAX-j)+j;
                    temp = suffix4[j];
                    suffix4[j] = suffix4[t];
                    suffix4[t] = temp;

                    indexPre7 = pre7Radom.Next(sqlAmount);

                    sw.WriteLine(resultList[indexPre7] + String.Format("{0,-10:D4}", suffix4[j]));
                    sum++;
                }
                i++;
            }
            //while (reader.Read() && i < (iCount / 10001 + 1))
            //{
            //    //开始生成(前7位决定地区和运营商，只需按顺序生成后四位)
            //    for (int j = 0; j < (iCount<10000?iCount:10000); j++)
            //    {
            //        sw.WriteLine(reader[1].ToString() + String.Format("{0,-10:D4}", j));   //写入字符串
            //        sum++;
            //    }
            //    i++;
            //}
            
            MessageBox.Show("数据共计" + sum.ToString()+"！");
            sw.Close();
            conn.Close();
        }

        private void oprBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            oprName = this.oprBox.SelectedItem.ToString();
            if ("Y".Equals(oprName)) {
                oprName = "移动";
            } else if ("L".Equals(oprName)){
                oprName = "联通";
            }else {
                oprName = "电信";
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sqlDetail;
            whereProvince = "(";
            for (int x = 0; x < listBox1.Items.Count; x++)
            {
                if (listBox1.GetSelected(x))
                {
                    whereProvince += ("'" + listBox1.Items[x].ToString() + "',");
                }
            }
            whereProvince = whereProvince.Substring(0, whereProvince.Length - 1) + ")";
            this.listBox2.Items.Clear();

            if(null == oprName) {
                sqlDetail = "select distinct city from phone where province in" + whereProvince;
            } else {
                sqlDetail = "select distinct city from phone where isp='" + oprName + "' and province in" + whereProvince;
            }
            SQLiteCommand cmdCity = new SQLiteCommand(sqlDetail, conn);
            SQLiteDataReader cityReader = cmdCity.ExecuteReader();
            while (cityReader.Read())
            {
                this.listBox2.Items.Add(cityReader[0].ToString());
            }
        }


        private void provinceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            province = this.provinceBox.SelectedItem.ToString();
            this.cityBox.Items.Clear();

            //查询数据,且给sqlAmount赋值
            string sqlDetail = "select distinct city from phone where isp='" + oprName + "' and province =" + province;
            SQLiteCommand cmdCity = new SQLiteCommand(sqlDetail, conn);
            SQLiteDataReader cityReader = cmdCity.ExecuteReader();
            while (cityReader.Read())
            {
                this.cityBox.Items.Add(cityReader[0].ToString());
            }
            conn.Close();
        }

        private void cityBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            city = this.cityBox.SelectedItem.ToString();
        }

        //private void textBox1_TextChanged(object sender, EventArgs e)
        //{

        //}
    }
}
