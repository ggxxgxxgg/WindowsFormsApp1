using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ginkgo;
using Newtonsoft.Json;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        ControlSPI.VSI_INIT_CONFIG pSPI_Config = new ControlSPI.VSI_INIT_CONFIG();
        Int32 DevIndex = -1;//设备下标
        Byte[] write_buffer = new Byte[10240];
        Byte[] read_buffer = new Byte[10240];
        Device device = new Device();
        Config config = new Config();//配置文件
        public Form1()
        {
            InitializeComponent();
            comboBox2.SelectedIndex = 2;
            comboBox3.SelectedIndex = 1;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            comboBox7.SelectedIndex = 0;
            comboBox8.SelectedIndex = 0;

            textBox1.Text = "32";
            textBoxReadRegAddress.Text = "00";
            init_cBox_config();
        }

        private void button_SearchDev_Click(object sender, EventArgs e)
        {
            int ret;
            //Scan connected device
            ret = ControlSPI.VSI_ScanDevice(1);
            if (ret <= 0)
            {
                MessageBox.Show("没有找到设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                string str = "找到" + ret + "个设备";
                MessageBox.Show(str, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                for (int i = 0; i < ret; i++)
                {
                    comboBox1.Items.Add(i);
                }
            }
        }

        private void button_OpenDev_Click(object sender, EventArgs e)
        {
            DevIndex = comboBox1.SelectedIndex;
            if (comboBox1.SelectedIndex != -1)
            {
                DevIndex = (int)comboBox1.SelectedItem;
            }
            else
            {
                MessageBox.Show("未选择设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int ret = ControlSPI.VSI_OpenDevice(ControlSPI.VSI_USBSPI, DevIndex, 0);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("打开设备失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                MessageBox.Show("打开设备成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button_InitSpi_Click(object sender, EventArgs e)
        {
            if (DevIndex == -1)
            {
                MessageBox.Show("未选择设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            pSPI_Config.ControlMode = ((byte)comboBox2.SelectedIndex);
            pSPI_Config.MasterMode = ((byte)comboBox3.SelectedIndex);
            pSPI_Config.ClockSpeed = Convert.ToUInt32(comboBox4.SelectedItem.ToString());//
            pSPI_Config.CPHA = ((byte)comboBox5.SelectedIndex);
            pSPI_Config.CPOL = ((byte)comboBox6.SelectedIndex);
            pSPI_Config.LSBFirst = ((byte)comboBox7.SelectedIndex);
            pSPI_Config.TranBits = Convert.ToByte(comboBox8.SelectedItem.ToString());//
            int ret = ControlSPI.VSI_InitSPI(ControlSPI.VSI_USBSPI, 0, ref pSPI_Config);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("初始化spi失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void button_ReadReg_ByLen_Click(object sender, EventArgs e)
        {
            if (DevIndex == -1)
            {
                MessageBox.Show("未选择设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            UInt16 ReadLen = Convert.ToUInt16(textBox1.Text);
            write_buffer[0] = 0x80;
            int ret = ControlSPI.VSI_WriteReadBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 1, read_buffer, ReadLen);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("SPI VSI_WriteReadBytes 错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                richTextBox1.AppendText($"从地址0x00读取{ReadLen}个数据\n");
                for (int i = 0; i < ReadLen; i++)
                {
                    richTextBox1.AppendText($"地址{i.ToString("X2")}:0x{read_buffer[i].ToString("X2")}");
                    if ((i + 1) % 4 == 0)
                    {
                        richTextBox1.AppendText("\n");
                    }
                    else
                    {
                        richTextBox1.AppendText("\t");
                    }
                }
            }
        }

        private void button_ReadReg_Selected_Click(object sender, EventArgs e)
        {
            if (DevIndex == -1)
            {
                MessageBox.Show("未选择设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            btn_OPT_NOW_Click(null, null);
            Byte AddresToRead = Convert.ToByte(textBoxReadRegAddress.Text,16);
            write_buffer[0] = AddresToRead;
            int ret = ControlSPI.VSI_WriteReadBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 1, read_buffer, 1);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("SPI写数据错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                richTextBox1.AppendText("\n地址0x" + textBoxReadRegAddress.Text + ":");
                richTextBox1.AppendText("0x" + read_buffer[0].ToString("X2") + "\n");
            }
        }

        private void button_ClearRichTextBox_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        /// <summary>
        /// 初始化参数配置区的数据
        /// </summary>
        void init_cBox_config()
        {
            //OSCF
            for (int i = 0; i <= 15; i++)
            {
                cBox_config_OSCF.Items.Add(i);
            }
            //PRTRM
            for (int i = 0; i <= 15; i++)
            {
                cBox_config_PRTRM.Items.Add(i);
            }
            //VTST
            for (int i = 0; i <= 63; i++)
            {
                cBox_config_VTST.Items.Add(i);
            }
            //BGST
            for (int i = 0; i <= 63; i++)
            {
                cBox_config_BGST.Items.Add(i);
            }
            //CBRG
            for (int i = 0; i <= 127; i++)
            {
                cBox_config_CBRG.Items.Add(i);
            }
            //CZRO
            for (int i = 0; i <= 511; i++)
            {
                cBox_config_CZRO.Items.Add(i);
            }
            //CVGA
            for (int i = 0; i <= 15; i++)
            {
                cBox_config_CVGA.Items.Add(i);
            }
            //FVGA
            for (int i = 0; i <= 511; i++)
            {
                cBox_config_FVGA.Items.Add(i);
            }
            //LFBW
            for (int i = 0; i <= 7; i++)
            {
                cBox_config_LFBW.Items.Add(i);
            }
            //TCMP
            for (int i = 0; i <= 255; i++)
            {
                cBox_config_TCMP.Items.Add(i);
            }
            //OPHS
            for (int i = 0; i <= 1; i++)
            {
                cBox_config_OPHS.Items.Add(i);
            }
            //SOEN
            for (int i = 0; i <= 1; i++)
            {
                cBox_config_SOEN.Items.Add(i);
            }
            //TICACEN
            for (int i = 0; i <= 1; i++)
            {
                cBox_config_TICACEN.Items.Add(i);
            }
            //TP2IEN
            for (int i = 0; i <= 1; i++)
            {
                cBox_config_TP2IEN.Items.Add(i);
            }
            //TC2VOEN
            for (int i = 0; i <= 1; i++)
            {
                cBox_config_TC2VOEN.Items.Add(i);
            }
            //TPGA
            for (int i = 0; i <= 7; i++)
            {
                cBox_config_TPGA.Items.Add(i);
            }
            //TLPFIEN
            for (int i = 0; i <= 1; i++)
            {
                cBox_config_TLPFIEN.Items.Add(i);
            }
            cBox_config_OSCF.SelectedIndex = 3;
            cBox_config_PRTRM.SelectedIndex = 7;
            cBox_config_VTST.SelectedIndex = 39;
            cBox_config_BGST.SelectedIndex = 50;
            cBox_config_CBRG.SelectedIndex = 64;
            cBox_config_CZRO.SelectedIndex = 0;
            cBox_config_CVGA.SelectedIndex = 8;
            cBox_config_FVGA.SelectedIndex = 276;
            cBox_config_LFBW.SelectedIndex = 4;
            cBox_config_TCMP.SelectedIndex = 50;
            cBox_config_OPHS.SelectedIndex = 0;
            cBox_config_SOEN.SelectedIndex = 1;

            cBox_config_TICACEN.SelectedIndex = 0;
            cBox_config_TP2IEN.SelectedIndex = 0;
            cBox_config_TC2VOEN.SelectedIndex = 0;
            cBox_config_TPGA.SelectedIndex = 0;
            cBox_config_TLPFIEN.SelectedIndex = 0;

            textBox_OrderS.Text = "ffffffff";
            textBox_OrderA.Text = "ffffffff";
            textBox_YMW.Text = "20240101";
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_softWrite_Click(object sender, EventArgs e)
        {
            SaveConfigValueToReg();
            write_buffer[0] = 0x00;//地址
            write_buffer[1] = 0x11;//数据
            for (int i = 0; i <= 0x1f; i++)
            {
                write_buffer[i + 2] = device.reg[i];
            }
            int ret = ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 0x1f + 2);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("软写 错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                richTextBox1.AppendText("\n软写\n");
                int readAddress = 0;
                for (int i = 0; i <= 0x1f; i++)
                {
                    richTextBox1.AppendText("地址" + readAddress.ToString("X2") + ":");
                    richTextBox1.AppendText("0x" + device.reg[i].ToString("X2"));
                    if (i % 2 == 0)
                    {
                        richTextBox1.AppendText("\n");
                    }
                    else
                    {
                        richTextBox1.AppendText("\t");
                    }
                    readAddress++;
                }
                ReadRegAll();
            }
        }
        private void btn_hardWrite_Click(object sender, EventArgs e)
        {
            SaveConfigValueToReg();
            write_buffer[0] = 0x00;
            write_buffer[1] = 0xaa;
            device.reg[0] = (Byte)((device.reg[0] & ~(7 << 5)) | (2 << 5));
            for (int i = 0; i < 0x1f; i++)
            {
                write_buffer[i + 2] = device.reg[i];
            }
            int ret = ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 0x1f + 2);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("硬写 错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                richTextBox1.AppendText("\n硬写\n");
                int readAddress = 0;
                for (int i = 0; i < 0x1f; i++)
                {
                    richTextBox1.AppendText("地址" + readAddress.ToString("X2") + ":");
                    richTextBox1.AppendText("0x" + device.reg[i].ToString("X2"));
                    if (i % 2 == 0)
                    {
                        richTextBox1.AppendText("\n");
                    }
                    else
                    {
                        richTextBox1.AppendText("\t");
                    }
                    readAddress++;
                }
                ReadRegAll();
            }
        }

        private void btn_startStart_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x25;
            ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
        }

        private void btn_stopStart_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x26;
            ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
        }

        private void btn_CBRG_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x21;
            ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
            ReadRegByAddress(4);
        }

        private void btn_CZRO_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x22;
            ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
            ReadRegByAddress(5);
            ReadRegByAddress(6);
        }

        private void btn_CBRG_enable_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x23;
            ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
        }

        private void btn_CZRO_enable_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x24;
            ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
        }

        private void btn_OPT_LSB_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x80;
            //write_buffer[2] = 0x80;
            int ret = ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
            //int ret = ControlSPI.VSI_WriteReadBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2,read_buffer,127);
            //int readAddress = 0;
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("读区域选择/OTP LSB(0x00~0x7F) 错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                device.Opt_b = (short)Device.OPT_BLOCK.B_00_7F;
                richTextBox1.AppendText("\n读区域选择/OTP LSB(0x00~0x7F)\n");
                //for (int i = 0; i < 127; i++)
                //{
                //    richTextBox1.AppendText("地址" + readAddress.ToString("X2") + ":");
                //    richTextBox1.AppendText("0x" + read_buffer[i].ToString("X2"));
                //    if (i % 2 == 0)
                //    {
                //        richTextBox1.AppendText("\n");
                //    }
                //    else
                //    {
                //        richTextBox1.AppendText("\t");
                //    }
                //    readAddress++;
                //}
            }
        }

        private void btn_OPT_MSB_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x81;
            //write_buffer[2] = 0x80;
            int ret = ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
            //int ret = ControlSPI.VSI_WriteReadBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2, read_buffer, 127);
            //int readAddress = 128;
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("读区域选择/OTP MSB(0x80~0xFF) 错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                device.Opt_b = (short)Device.OPT_BLOCK.B_80_FF;
                richTextBox1.AppendText("\n读区域选择/OTP MSB(0x80~0xFF)\n");
                //for (int i = 0; i < 127; i++)
                //{
                //    richTextBox1.AppendText("地址" + readAddress.ToString("X2") + ":");
                //    richTextBox1.AppendText("0x" + read_buffer[i].ToString("X2"));
                //    if (i % 2 == 0)
                //    {
                //        richTextBox1.AppendText("\n");
                //    }
                //    else
                //    {
                //        richTextBox1.AppendText("\t");
                //    }
                //    readAddress++;
                //}
            }
        }

        private void btn_OPT_NOW_Click(object sender, EventArgs e)
        {
            write_buffer[0] = 0x00;
            write_buffer[1] = 0x88;
            int ret = ControlSPI.VSI_WriteBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 2);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("读区域选择/当前寄存器 错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                device.Opt_b = (short)Device.OPT_BLOCK.B_NOW;
                richTextBox1.AppendText("\n读区域选择/当前寄存器\n");
                //ReadRegAll();
            }
        }

        private void textBoxReadRegAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 允许十六进制字符、退格键和删除键
            if (!Uri.IsHexDigit(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)127)
            {
                e.Handled = true; // 忽略非十六进制字符
            }
        }

        private void CheckAndLoadConfig()
        {
            string configFileName = "config.json";
            if (File.Exists(configFileName))
            {
                SaveConfigToFile(configFileName);
                // 如果文件存在，加载配置文件的逻辑
                // 这里可以读取和处理配置文件
                MessageBox.Show("保存参数成功");
            }
            else
            {
                DialogResult result = MessageBox.Show("找不到配置文件。是否要创建一个新的默认配置文件？", "提示", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // 创建默认配置文件
                    SaveConfigToFile(configFileName);
                }
                else
                {
                    // 用户选择不创建新文件，可以选择现有文件
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Title = "选择配置文件",
                        Filter = "JSON Files|*.json",
                        RestoreDirectory = true
                    };
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFile = openFileDialog.FileName;
                        SaveConfigToFile(selectedFile);
                        // 在这里处理用户选择的文件
                        // 可以将文件复制到当前目录或进行其他操作
                        MessageBox.Show($"已选择文件: {selectedFile}");
                    }
                }
            }
        }
        public void SaveConfigToFile(string filePath)
        {
            try
            {
                config.OSCF = (UInt16)cBox_config_OSCF.SelectedIndex;
                config.PRTRM = (UInt16)cBox_config_PRTRM.SelectedIndex;
                config.VTST = (UInt16)cBox_config_VTST.SelectedIndex;
                config.BGST = (UInt16)cBox_config_BGST.SelectedIndex;
                config.CBRG = (UInt16)cBox_config_CBRG.SelectedIndex;
                config.CZRO = (UInt16)cBox_config_CZRO.SelectedIndex;
                config.CVGA = (UInt16)cBox_config_CVGA.SelectedIndex;
                config.FVGA = (UInt16)cBox_config_FVGA.SelectedIndex;
                config.LFBW = (UInt16)cBox_config_LFBW.SelectedIndex;
                config.TCMP = (UInt16)cBox_config_TCMP.SelectedIndex;

                config.OPHS = (UInt16)cBox_config_OPHS.SelectedIndex;
                config.SOEN = (UInt16)cBox_config_SOEN.SelectedIndex;
                config.TICACEN = (UInt16)cBox_config_TICACEN.SelectedIndex;
                config.TP2IEN = (UInt16)cBox_config_TP2IEN.SelectedIndex;
                config.TC2VOEN = (UInt16)cBox_config_TC2VOEN.SelectedIndex;
                config.TPGA = (UInt16)cBox_config_TPGA.SelectedIndex;
                config.TLPFIEN = (UInt16)cBox_config_TLPFIEN.SelectedIndex;

                config.ORDER_S = textBox_OrderS.Text;
                config.ORDER_A = textBox_OrderA.Text;
                config.YMW = Convert.ToUInt32(textBox_YMW.Text);
            }
            catch
            {
                MessageBox.Show("保存参数失败");
                return;
            }
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        public void ImportConfigFile(string json)
        {
            config = JsonConvert.DeserializeObject<Config>(json);
            try
            {
                cBox_config_OSCF.SelectedIndex = config.OSCF;
                cBox_config_PRTRM.SelectedIndex = config.PRTRM;
                cBox_config_VTST.SelectedIndex = config.VTST;
                cBox_config_BGST.SelectedIndex = config.BGST;
                cBox_config_CBRG.SelectedIndex = config.CBRG;
                cBox_config_CZRO.SelectedIndex = config.CZRO;
                cBox_config_CVGA.SelectedIndex = config.CVGA;
                cBox_config_FVGA.SelectedIndex = config.FVGA;
                cBox_config_LFBW.SelectedIndex = config.LFBW;
                cBox_config_TCMP.SelectedIndex = config.TCMP;

                cBox_config_OPHS.SelectedIndex      = config.OPHS;
                cBox_config_SOEN.SelectedIndex      = config.SOEN;
                cBox_config_TICACEN.SelectedIndex   = config.TICACEN;
                cBox_config_TP2IEN.SelectedIndex    = config.TP2IEN;
                cBox_config_TC2VOEN.SelectedIndex   = config.TC2VOEN;
                cBox_config_TPGA.SelectedIndex      = config.TPGA;
                cBox_config_TLPFIEN.SelectedIndex   = config.TLPFIEN;

                textBox_OrderS.Text = config.ORDER_S;
                textBox_OrderA.Text = config.ORDER_A;
                textBox_YMW.Text = Convert.ToString(config.YMW);
            }
            catch
            {
                MessageBox.Show("导入参数失败");
                return;
            }
            MessageBox.Show("导入参数成功");
        }
        private void CheckAndSelectConfigFile()
        {
            string configFileName = "config.json";

            if (System.IO.File.Exists(configFileName))
            {
                string json = File.ReadAllText(configFileName);
                ImportConfigFile(json);
            }
            else
            {
                // 配置文件不存在，弹出文件选择框
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "选择配置文件",
                    Filter = "JSON Files|*.json",
                    InitialDirectory = Application.StartupPath // 设置初始目录为应用程序启动路径
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;
                    // 在这里处理用户选择的文件
                    // 你可以复制或读取文件内容等
                    MessageBox.Show($"已选择文件: {selectedFile}");
                    ImportConfigFile(File.ReadAllText(selectedFile));
                }
                else
                {
                    MessageBox.Show("没有选择配置文件。");
                }
            }
        }
        
        private void btn_saveValue_Click(object sender, EventArgs e)
        {
            CheckAndLoadConfig();
        }

        private void btn_importValue_Click(object sender, EventArgs e)
        {
            CheckAndSelectConfigFile();
        }

        private void SaveConfigValueToReg()
        {
            int num = ~(0x07 << 4);
            // 先清除下标为0的元素的[7:4]位，将其设为0
            device.reg[1] &= (Byte)num;
            // 然后将数字13左移4位（[7:4]位），并与元素[index]相或，以设置相应的位
            device.reg[1] |= (Byte)(cBox_config_OSCF.SelectedIndex << 4);
            
            num = ~(0x0F);
            device.reg[1] &= (Byte)num;
            device.reg[1] |= (Byte)cBox_config_PRTRM.SelectedIndex;

            num = ~(0x3F);
            device.reg[2] &= (Byte)num;
            device.reg[2] |= (Byte)cBox_config_VTST.SelectedIndex;

            device.reg[3] &= (Byte)num;
            device.reg[3] |= (Byte)cBox_config_BGST.SelectedIndex;

            num = ~(0x7F);
            device.reg[4] &= (Byte)num;
            device.reg[4] |= (Byte)cBox_config_CBRG.SelectedIndex;
            /////
            num = ~(0x01);//清除REG05的第0位，将其设为0 0x01 的二进制形式是 00000001
            device.reg[5] &= (Byte)num;
            num = ~(0xFF);//清除REG06的[7:0]位，将其设为0 0xFF 的二进制形式是 11111111
            device.reg[6] &= (Byte)num;
            num = cBox_config_CZRO.SelectedIndex;
            device.reg[5] = (Byte)((num & 0x100) >> 8);
            device.reg[6] = (Byte)(num & 0xFF);

            num = ~(0xF0);
            device.reg[7] &= (Byte)num;
            device.reg[7] |= (Byte)(cBox_config_CVGA.SelectedIndex << 4);
            num = ~(0x01);//清除REG05的第0位，将其设为0 0x01 的二进制形式是 00000001
            device.reg[7] &= (Byte)num;
            device.reg[7] |= (Byte)(cBox_config_FVGA.SelectedIndex >> 8);

            num = ~(0xFF);//清除REG06的[7:0]位，将其设为0 0xFF 的二进制形式是 11111111
            device.reg[8] &= (Byte)num;
            num = cBox_config_FVGA.SelectedIndex;
            device.reg[8] = (Byte)(num & 0xFF);

            num = ~(0x07);
            device.reg[9] &= (Byte)num;
            device.reg[9] |= (Byte)cBox_config_LFBW.SelectedIndex;

            device.reg[10] |= (Byte)cBox_config_TCMP.SelectedIndex;
            //0b
            num = ~(1 << 1);
            device.reg[11] &= (Byte)num;
            num = (Byte)((cBox_config_OPHS.SelectedIndex & 0x01) << 1);
            device.reg[11] |= (Byte)num;

            num = ~(1 << 0);
            device.reg[11] &= (Byte)num;
            num = (Byte)((cBox_config_SOEN.SelectedIndex & 0x01) << 0);
            device.reg[11] |= (Byte)num;
            //0c
            num = ~(1 << 6);
            device.reg[12] &= (Byte)num;
            num = (Byte)((cBox_config_TICACEN.SelectedIndex & 0x01) << 6);//TICACEN
            device.reg[12] |= (Byte)num;

            num = ~(1 << 5);
            device.reg[12] &= (Byte)num;
            num = (Byte)((cBox_config_TP2IEN.SelectedIndex & 0x01) << 5);
            device.reg[12] |= (Byte)num;

            num = ~(1 << 4);
            device.reg[12] &= (Byte)num;
            num = (Byte)((cBox_config_TC2VOEN.SelectedIndex & 0x01) << 4);
            device.reg[12] |= (Byte)num;

            num = ~(0x0E);
            device.reg[12] &= (Byte)num;
            num = (Byte)((cBox_config_TPGA.SelectedIndex & 0x07) << 1);
            device.reg[12] |= (Byte)num;

            num = ~(1 << 0);
            device.reg[12] &= (Byte)num;
            num = (Byte)((cBox_config_TLPFIEN.SelectedIndex & 0x01) << 0);
            device.reg[12] |= (Byte)num;

            device.reg[13] = 0xff;
            device.reg[14] = 0xff;
            device.reg[15] = 0xff;

            //10
            string year_s = textBox_YMW.Text.Substring(0, 4);
            int year_n = Convert.ToInt32(year_s);
            //Console.WriteLine($"{year_s}");
            num = ~(0xff);
            device.reg[16] &= (Byte)num;//清除
            num = (((year_n /100 / 10) << 4) & 0xF0);//放入年的2023的20
            device.reg[16] |= (Byte)num;
            num = (year_n / 100 % 10);//
            device.reg[16] |= (Byte)num;

            //Console.WriteLine(year_n % 100);
            num = ~(0xff);
            device.reg[17] &= (Byte)num;//清除
            num = (((year_n % 100 / 10) << 4) & 0xF0);//放入年的2023的23
            //Console.WriteLine($"num 1:{num}");
            device.reg[17] |= (Byte)num;
            num = (year_n % 100 % 10);//
            //Console.WriteLine($"num 2:{num}");
            device.reg[17] |= (Byte)num;
            //Console.WriteLine(device.reg[17]);
            //Console.WriteLine(device.reg[17].ToString("X2"));

            string month_s = textBox_YMW.Text.Substring(4, 2);
            int month_n = Convert.ToInt32(month_s);
            num = ~(0xff);
            device.reg[18] &= (Byte)num;//month
            num = (((month_n / 10) & 0x0F) << 4);
            device.reg[18] |= (Byte)num;
            num = (month_n % 10);
            device.reg[18] |= (Byte)num;

            string week_s = textBox_YMW.Text.Substring(textBox_YMW.Text.Length - 2);
            int week_n = Convert.ToInt32(week_s);
            num = ~(0xff);
            device.reg[19] &= (Byte)num;//week
            num = (((week_n / 10) & 0x0F) << 4);
            device.reg[19] |= (Byte)num;
            num = (week_n % 10);
            device.reg[19] |= (Byte)num;

            num = Convert.ToInt32(textBox_OrderS.Text, 16);
            device.reg[20] = (byte)(num >> 24); // 获取最高8位
            device.reg[21] = (byte)((num >> 16) & 0xFF); // 获取第二个8位
            device.reg[22] = (byte)((num >> 8) & 0xFF); // 获取第三个8位
            device.reg[23] = (byte)(num & 0xFF); // 获取最低8位

            num = Convert.ToInt32(textBox_OrderA.Text, 16);
            device.reg[24] = (byte)(num >> 24); // 获取最高8位
            device.reg[25] = (byte)((num >> 16) & 0xFF); // 获取第二个8位
            device.reg[26] = (byte)((num >> 8) & 0xFF); // 获取第三个8位
            device.reg[27] = (byte)(num & 0xFF); // 获取最低8位

            richTextBox1.AppendText("\n设置要写入寄存器的值\n");
            for (int i = 0; i <= 0x1f; i++)
            {
                richTextBox1.AppendText("地址" + i.ToString("X2") + ":");
                richTextBox1.AppendText("0x" + device.reg[i].ToString("X2"));
                if (i % 2 == 0)
                {
                    richTextBox1.AppendText("\n");
                }
                else
                {
                    richTextBox1.AppendText("\t");
                }
            }
        }

        private void ReadRegAll()
        {
            if (DevIndex == -1)
            {
                MessageBox.Show("未选择设备", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            btn_OPT_NOW_Click(null, null);
            write_buffer[0] = 0x80;
            int ret = ControlSPI.VSI_WriteReadBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 1, read_buffer, 0x1f);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("读所有寄存器", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                richTextBox1.AppendText("\n读所有寄存器\n");
                int readAddress = 0;
                for (int i = 0; i <= 0x1f; i++)
                {
                    richTextBox1.AppendText("地址" + readAddress.ToString("X2") + ":");
                    richTextBox1.AppendText("0x" + read_buffer[i].ToString("X2"));
                    if (i % 2 == 0)
                    {
                        richTextBox1.AppendText("\n");
                    }
                    else
                    {
                        richTextBox1.AppendText("\t");
                    }
                    readAddress++;
                }
            }
        }

        private void ReadRegByAddress(int address)
        {
            btn_OPT_NOW_Click(null, null);
            //int readAddress = address + 128;
            write_buffer[0] = (byte)(address);
            int ret = ControlSPI.VSI_WriteReadBytes(ControlSPI.VSI_USBSPI, DevIndex, 0, write_buffer, 1, read_buffer, 1);
            if (ret != ControlSPI.ERROR.SUCCESS)
            {
                MessageBox.Show("读单个寄存器", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                richTextBox1.AppendText("\n读单个寄存器\n");
                richTextBox1.AppendText("地址" + address.ToString("X2") + ":");
                richTextBox1.AppendText("0x" + read_buffer[0].ToString("X2"));
            }
        }

        private void textBox_OrderS_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 允许十六进制字符、退格键和删除键
            if (!Uri.IsHexDigit(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)127)
            {
                e.Handled = true; // 忽略非十六进制字符
            }
        }

        private void textBox_OrderA_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 允许十六进制字符、退格键和删除键
            if (!Uri.IsHexDigit(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)127)
            {
                e.Handled = true; // 忽略非十六进制字符
            }
        }

        private void ShowRegValue()
        {
            richTextBox1.AppendText("\n读所有寄存器\n");
            int readAddress = 0;
            for (int i = 0; i < 0x1f; i++)
            {
                richTextBox1.AppendText("地址" + i.ToString("X2") + ":");
                richTextBox1.AppendText("0x" + device.reg[i].ToString("X2"));
                if (i % 2 == 0)
                {
                    richTextBox1.AppendText("\n");
                }
                else
                {
                    richTextBox1.AppendText("\t");
                }
                readAddress++;
            }
        }
    }
}
