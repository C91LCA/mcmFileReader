using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace mcmFileReader
{
    public partial class Form1 : Form
    {
        public Bitmap[] charimgs;
        public Bitmap charsetImg;
        public bool loadedMCM = false;
        public String inpfname = "";

        public Bitmap charEditorTMP;
        public Color drawColor;

        public Color blackColor = Color.Black;
        public Color whiteColor = Color.White;
        public Color transColor = Color.Gray;

        int selChar = -1;

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                loadFile(files[0]);
            }
        }

        private int constrain(int x, int min, int max)
        {
            int result = 0;
            if (x > max) result = max;
            else if (x < min) result = min;
            else result = x;
            return result;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            button2.BackColor = blackColor;
            button3.BackColor = whiteColor;
            button4.BackColor = transColor;
            //////////
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                inpfname = args[1];
                loadFile(inpfname);
            }
        }

        private void mcmOpen(String fname)
        {
            //  MessageBox.Show(fname, "filen");
            StreamReader rd = new StreamReader(fname);
            String xchar = "", xtchar = "";
            String ln = "";
            int charn = 0, lin = 0;
            charimgs = new Bitmap[256];
            label1.Text = "Reading...";
            label1.Visible = true;
            loadedMCM = false;
            if (rd.ReadLine() == "MAX7456") // it's a real MAX7456 MCM file
            {
                while ((ln = rd.ReadLine()) != null)
                {
                    xchar += ln;
                    lin++;
                    if (lin == 64)
                    {
                        charimgs[charn] = new Bitmap(12, 18, PixelFormat.Format24bppRgb);
                        for (int i = 0; i < 512; i += 2)
                        {
                            String rd2 = xchar.Substring(i, 2);
                            if (rd2 == "00") xtchar += "0";
                            else if (rd2 == "01") xtchar += "2";
                            else if (rd2 == "10") xtchar += "1";
                            else if (rd2 == "11") xtchar += "2";
                        }
                        for (int iy = 0; iy < 18; iy++)
                            for (int ix = 0; ix < 12; ix++)
                            {
                                switch (xtchar.Substring(iy * 12 + ix, 1))
                                {
                                    case "0": charimgs[charn].SetPixel(ix, iy, blackColor); break;
                                    case "1": charimgs[charn].SetPixel(ix, iy, whiteColor); break;
                                    case "2": charimgs[charn].SetPixel(ix, iy, transColor); break;
                                    default: charimgs[charn].SetPixel(ix, iy, transColor); break;
                                }

                            }
                        xtchar = "";
                        xchar = "";
                        lin = 0;
                        charn++;
                    }
                }
                loadedMCM = true;
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
                toolStripButton4.Enabled = true;
                rd.Close();
            }
            else { rd.Close(); MessageBox.Show("File is not MCM file!", "Incorrect file!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            label1.Visible = false;
        }

        private void loadFile(String inpfname)
        {
            if (inpfname != "")
            {
                if (File.Exists(inpfname))
                {
                    mcmOpen(inpfname);
                    drawCharset();
                }
                else MessageBox.Show("File not exist!", "File not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void drawCharset()
        {
            if (loadedMCM)
            {
                label1.Text = "Drawing...";
                label1.Visible = true;
                charsetImg = new Bitmap(12 * 16, 18 * 16, PixelFormat.Format24bppRgb);
                for (int j = 0; j < 16; j++)
                    for (int i = 0; i < 16; i++)
                        for (int px = 0; px < 12; px++)
                            for (int py = 0; py < 18; py++)
                            {
                                charsetImg.SetPixel(i * 12 + px, j * 18 + py, charimgs[j * 16 + i].GetPixel(px, py));
                            }
                pictureBox1.Image = charsetImg;
                label1.Visible = false;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                loadFile(openFileDialog1.FileName);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (loadedMCM)
                {
                    String[] ext = saveFileDialog1.FileName.Split('.');
                    ImageFormat fmt;
                    switch (ext[ext.Length - 1])
                    {
                        case "png": fmt = ImageFormat.Png; break;
                        case "jpg": fmt = ImageFormat.Jpeg; break;
                        case "jpeg": fmt = ImageFormat.Jpeg; break;
                        case "bmp": fmt = ImageFormat.Bmp; break;
                        default: MessageBox.Show("Unknown image format!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); fmt = ImageFormat.Bmp; break;
                    }
                    charsetImg.Save(saveFileDialog1.FileName, fmt);
                }
                else MessageBox.Show("MCM File not loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveTiles(String fname)
        {
            label1.Text = "Saving...";
            label1.Visible = true;
            String[] ext = fname.Split('.');
            switch (ext[ext.Length - 1])
            {
                case "ico": saveBitmapsICO(fname, charimgs); break;
                default: MessageBox.Show("Unknown file ext! (*." + ext[ext.Length - 1] + " ???)", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
            }
            label1.Visible = false;
        }

        private void saveBitmapsICO(String fname, Bitmap[] sprt)
        {
            /*
             * that was a hard work.
             * trying to recognise how to store icon image
             * but it's just an DIB/BITMAPINFOHEADER here
             * ........
             */
            BinaryWriter icof = new BinaryWriter(new FileStream(fname, FileMode.Create, FileAccess.Write));
            icof.Write((byte)0); icof.Write((byte)0); // reserved
            icof.Write((byte)1); icof.Write((byte)0); // type
            icof.Write((byte)(sprt.Length & 255)); icof.Write((byte)(sprt.Length >> 8 & 255)); // count
            int offs = (16 * sprt.Length) + 6;
            int leng = 0;
            Color col;
            for (int i = 0; i < sprt.Length; i++)
            {
                icof.Write((byte)(sprt[i].Width)); icof.Write((byte)(sprt[i].Height)); //width&height
                icof.Write((byte)0); icof.Write((byte)255); //colors: FULL COLOR IMAGE!, 255 (because of .NET)
                icof.Write((byte)1); icof.Write((byte)0);   //PLANES: 1
                icof.Write((byte)32); icof.Write((byte)0);  //bpp: RGBA! 32pbs RRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA
                leng = (sprt[i].Width * sprt[i].Height * 4) + 40; //length of image & bmp info header
                icof.Write((byte)(leng & 255)); icof.Write((byte)(leng >> 8 & 255));         // for length
                icof.Write((byte)(leng >> 16 & 255)); icof.Write((byte)(leng >> 24 & 255));  // also for length
                icof.Write((byte)(offs & 255)); icof.Write((byte)(offs >> 8 & 255));         // for offset
                icof.Write((byte)(offs >> 16 & 255)); icof.Write((byte)(offs >> 24 & 255));  // also for offset
                /*MessageBox.Show(
                    "[" + (leng >> 24 & 255) + "," + (leng >> 16 & 255) + "," + (leng >> 8 & 255) + "," + (leng & 255) + "] = " + leng + "\n" +
                    "[" + (offs >> 24 & 255) + "," + (offs >> 16 & 255) + "," + (offs >> 8 & 255) + "," + (offs & 255) + "] = " + offs + "\n"
                );*/
                offs += leng;
            }
            for (int i = 0; i < sprt.Length; i++)
            {
                // that was a 'hard' work....
                icof.Write((byte)40); icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); //header length: 40 (BMPv3)
                //////////
                icof.Write((byte)(sprt[i].Width & 255)); icof.Write((byte)(sprt[i].Width >> 8 & 255));          // for width
                icof.Write((byte)(sprt[i].Width >> 16 & 255)); icof.Write((byte)(sprt[i].Width >> 24 & 255));   // also for width
                /////////
                icof.Write((byte)((sprt[i].Height * 2) & 255)); icof.Write((byte)((sprt[i].Height * 2) >> 8 & 255));    // for height
                icof.Write((byte)((sprt[i].Height * 2) >> 16 & 255)); icof.Write((byte)((sprt[i].Height * 2) >> 24 & 255)); //also for height
                ////////////
                icof.Write((byte)1); icof.Write((byte)0); // 1 plane
                //////////////////
                icof.Write((byte)32); icof.Write((byte)0); // 32 bps!
                ///////////
                icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); // ???
                ///////////
                leng = (sprt[i].Width * sprt[i].Height * 4);
                icof.Write((byte)(leng & 255)); icof.Write((byte)(leng >> 8 & 255));
                icof.Write((byte)(leng >> 16 & 255)); icof.Write((byte)(leng >> 24 & 255));
                /////////////////
                icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); // x/m, y/m = 0
                icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); // ???
                icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); // ???
                icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); icof.Write((byte)0); // ???
                /////////////////
                for (int y = 0; y < sprt[i].Height; y++)
                {
                    for (int x = 0; x < sprt[i].Width; x++)
                    {
                        col = sprt[i].GetPixel(x, (sprt[i].Height - 1) - y);
                        icof.Write((byte)col.R); icof.Write((byte)col.G); icof.Write((byte)col.B); icof.Write((byte)col.A);
                    }
                }


            }
            icof.Close();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                if (loadedMCM) saveTiles(saveFileDialog2.FileName);
                else MessageBox.Show("MCM File not loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (loadedMCM)
            {
                
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && loadedMCM)
            {
                selChar = ((e.X / 12) + (e.Y / 18 * 16));
                contextMenuStrip1.Show(pictureBox1, e.X, e.Y);
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "Save character": saveFileDialog3.ShowDialog(); break;
                case "Edit character":
                    if (selChar >= 0)
                    {
                        panel2.BringToFront();
                        charEditorTMP = charimgs[selChar];
                        drawCharEditor();
                        button1.Enabled = true;
                        button6.Enabled = true;
                        button5.Enabled = true;
                    }
                    break;
            }
        }

        private void drawCharEditor()
        {
            Bitmap tmp1 = new Bitmap(120, 180);
            for (int i = 0; i < 120; i++)
                for (int j = 0; j < 180; j++)
                    tmp1.SetPixel(i, j, charEditorTMP.GetPixel(i / 10, j / 10));

            for (int i = 0; i < 120; i += 10)
                for (int j = 0; j < 180; j++)
                    tmp1.SetPixel(i, j, Color.Black);

            for (int i = 0; i < 120; i++)
                for (int j = 0; j < 180; j += 10)
                    tmp1.SetPixel(i, j, Color.Black);

            pictureBox2.Image = tmp1;
        }

        private void saveFileDialog3_FileOk(object sender, CancelEventArgs e)
        {
            if (loadedMCM)
            {
                String[] ext = saveFileDialog3.FileName.Split('.');
                ImageFormat fmt;
                switch (ext[ext.Length - 1])
                {
                    case "png": fmt = ImageFormat.Png; break;
                    case "jpg": fmt = ImageFormat.Jpeg; break;
                    case "jpeg": fmt = ImageFormat.Jpeg; break;
                    case "bmp": fmt = ImageFormat.Bmp; break;
                    default: MessageBox.Show("Unknown image format!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); fmt = ImageFormat.Bmp; break;
                }
                charimgs[selChar].Save(saveFileDialog3.FileName, fmt);
            }
            else MessageBox.Show("MCM File not loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void saveMCM(String fname)
        {
            StreamWriter wr = new StreamWriter(fname);
            wr.WriteLine("MAX7456");
            String tstr = "";
            for (int d = 0; d < charimgs.Length; d++)
            {
                tstr = "";
                for (int j = 0; j < 18; j++)
                    for (int i = 0; i < 12; i++) {
                        Color gcol = charimgs[d].GetPixel(i, j);
                        if (gcol.R == blackColor.R && gcol.G == blackColor.G && gcol.B == blackColor.B)
                        {
                            tstr += "00";
                        }
                        else if (gcol.R == whiteColor.R && gcol.G == whiteColor.G && gcol.B == whiteColor.B)
                        {
                            tstr += "10";
                        }
                        else if (gcol.R == transColor.R && gcol.G == transColor.G && gcol.B == transColor.B)
                        {
                            tstr += "01";
                        }
                        else
                        {
                            tstr += "11";
                        }
                    }
                for (int g = 0; g < 54; g++)
                {
                    wr.WriteLine(tstr.Substring(g * 8, 8));
                }
                for (int f = 0; f < 10; f++) wr.WriteLine("01010101");
            }
            wr.Close();
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (loadedMCM && selChar >= 0 && e.Button == MouseButtons.Left) {
                charEditorTMP.SetPixel(e.X / 10, e.Y / 10, drawColor);
                drawCharEditor();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            drawColor = blackColor;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            drawColor = whiteColor;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            drawColor = transColor;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (loadedMCM && selChar >= 0 && e.Button == MouseButtons.Left)
            {
                charEditorTMP.SetPixel(constrain(e.X / 10, 0, 11), constrain(e.Y / 10, 0, 17), drawColor);
                drawCharEditor();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            charimgs[selChar] = charEditorTMP;
            drawCharset();
            panel2.SendToBack();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 18; j++)
                {
                    Color tmpc = charEditorTMP.GetPixel(i, j);
                    if (tmpc == blackColor) {
                        charEditorTMP.SetPixel(i, j, whiteColor); break;
                    } else if (tmpc == whiteColor) {
                        charEditorTMP.SetPixel(i, j, blackColor); break;
                    } else if (tmpc == transColor) {
                        charEditorTMP.SetPixel(i, j, transColor); break;
                    } else {
                        charEditorTMP.SetPixel(i, j, Color.Red); break;
                    }
                }
            drawCharEditor();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 18; j++)
                    charEditorTMP.SetPixel(i, j, drawColor);
            
            drawCharEditor();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (saveFileDialog4.ShowDialog() == DialogResult.OK)
            {
                saveMCM(saveFileDialog4.FileName);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
