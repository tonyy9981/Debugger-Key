using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HTTPDebuggerKeyGen
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, uint VolumeNameSize, ref uint VolumeSerialNumber, ref uint MaximumComponentLength, ref uint FileSystemFlags, StringBuilder FileSystemNameBuffer, uint FileSystemNameSize);

        public RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\\MadeForNet\\HTTPDebuggerPro");
        public RegistryKey openKey = Registry.CurrentUser.OpenSubKey(@"Software\\MadeForNet\\HTTPDebuggerPro");

        public static Random randomInt = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            if (openKey.GetValueNames().Contains("AppVer"))
            {
                string AppVer = (string)openKey.GetValue("AppVer");

                Text = AppVer;

                int appVerParsed = int.Parse(string.Join("", AppVer.ToCharArray().Where(char.IsDigit)));

                textBox1.Text = string.Format("SN{0:d}", appVerParsed ^ ((~GetVolumeSerial("C") >> 1) + 736) ^ 0x590D4);
            }
            else
            {
                MessageBox.Show("Please open HTTP Debugger to create the registry keys.");
                Environment.Exit(-1);
            }
        }

        public uint GetVolumeSerial(string strDriveLetter)
        {
            uint serNum = 0;
            uint maxCompLen = 0;
            StringBuilder VolLabel = new StringBuilder(256);
            uint VolFlags = new uint();
            StringBuilder FSName = new StringBuilder(256);
            strDriveLetter += ":\\";
            GetVolumeInformation(strDriveLetter, VolLabel, (uint)VolLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (uint)FSName.Capacity);

            return serNum;
        }

        public string CreateKey()
        {
            string key;

            do
            {
                int fInt = randomInt.Next(255);
                int sInt = randomInt.Next(255);
                int rInt = randomInt.Next();

                key = $"{fInt:X2}{sInt ^ 0x7C:X2}{(byte)~(byte)fInt:X2}7C{sInt:X2}{rInt % 255:X2}{((byte)(rInt % 255) ^ 7):X2}{(fInt ^ (byte)~(rInt % 255)):X2}";
            }

            while (key.Length != 16);

            return key;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string AppVer = (string)openKey.GetValue("AppVer");
            int appVerParsed = int.Parse(string.Join("", AppVer.ToCharArray().Where(char.IsDigit)));

            if (textBox2.Text != string.Empty)
            {
                Clipboard.SetText(textBox2.Text);

                key.SetValue(string.Format("SN{0:d}", appVerParsed ^ ((~GetVolumeSerial("C") >> 1) + 736) ^ 0x590D4), textBox2.Text);

                MessageBox.Show("Applied the key and copied it to your clipboard!");
            }
            else
            {
                MessageBox.Show("Generate a key first!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = CreateKey();   // Key: XXXXXX7CXXXXXXXX 16long
                                           // Value: SNXXXXXXXXXX SN+10long
        }
    }
}