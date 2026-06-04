using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        static readonly string[] SYMBOLS = { "★", "◆", "●", "♣", "♪", "♥" };
        static readonly Color[] SYM_COLORS = {
            Color.FromArgb(255, 215,  0),
            Color.FromArgb(255, 140,  0),
            Color.FromArgb( 70, 130, 180),
            Color.FromArgb( 50, 180,  80),
            Color.FromArgb(220,  80, 220),
            Color.FromArgb(220,  60,  60),
        };

        const int ROWS = 8;
        const int COLS = 4;
        const int CELL = 48;
        const int GAP = 6;
        const int FB = 14;
        const int PADL = 20;
        const int PADT = 60;

        int[] secret = new int[COLS];
        int[][] guesses = new int[ROWS][];
        int[][] feedbackArr = new int[ROWS][];
        int currentRow = 0;
        int[] currentGuess = new int[0];
        bool gameOver = false;

        Panel pnlBoard;
        Panel[] symButtons = new Panel[6];
        Label lblStatus;

        public Form1()
        {
            InitializeComponent();
            this.Text = "SKOCKO";
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.ClientSize = new Size(560, 640);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            BuildUI();
            NewGame();
        }

        void BuildUI()
        {
            Label lblTitle = new Label();
            lblTitle.Text = "S K O C K O";
            lblTitle.ForeColor = Color.FromArgb(245, 200, 66);
            lblTitle.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(130, 10);
            this.Controls.Add(lblTitle);

            pnlBoard = new Panel();
            pnlBoard.Location = new Point(10, 52);
            pnlBoard.Size = new Size(PADL + COLS * (CELL + GAP) + 2 * (FB + GAP) + 30, PADT + ROWS * (CELL + GAP) + CELL + 40);
            pnlBoard.BackColor = Color.Transparent;
            pnlBoard.Paint += new PaintEventHandler(pnlBoard_Paint);
            pnlBoard.MouseClick += new MouseEventHandler(pnlBoard_MouseClick);
            this.Controls.Add(pnlBoard);

            Label lbl = new Label();
            lbl.Text = "KLIKNI SIMBOL:";
            lbl.ForeColor = Color.FromArgb(160, 160, 160);
            lbl.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lbl.AutoSize = true;
            lbl.Location = new Point(326, 110);
            this.Controls.Add(lbl);

            for (int i = 0; i < SYMBOLS.Length; i++)
            {
                Panel p = new Panel();
                p.Size = new Size(CELL, CELL);
                p.Location = new Point(326 + (i % 3) * (CELL + GAP),
                                           130 + (i / 3) * (CELL + GAP));
                p.BackColor = Color.FromArgb(15, 15, 30);
                p.Cursor = Cursors.Hand;
                p.Tag = i;
                p.Paint += new PaintEventHandler(symBtn_Paint);
                p.MouseClick += new MouseEventHandler(symBtn_Click);
                p.MouseDoubleClick += new MouseEventHandler(symBtn_Click);
                p.MouseEnter += new EventHandler(symBtn_MouseEnter);
                p.MouseLeave += new EventHandler(symBtn_MouseLeave);
                symButtons[i] = p;
                this.Controls.Add(p);
            }

            lblStatus = new Label();
            lblStatus.ForeColor = Color.FromArgb(200, 200, 200);
            lblStatus.Font = new Font("Segoe UI", 9f);
            lblStatus.Size = new Size(200, 30);
            lblStatus.Location = new Point(326, 260);
            lblStatus.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(lblStatus);



            Button btnNew = new Button();
            btnNew.Text = "NOVA IGRA";
            btnNew.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnNew.Size = new Size(150, 34);
            btnNew.Location = new Point(326, 320);
            btnNew.BackColor = Color.FromArgb(200, 70, 70);
            btnNew.ForeColor = Color.White;
            btnNew.FlatStyle = FlatStyle.Flat;
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.Click += new EventHandler(btnNew_Click);
            this.Controls.Add(btnNew);

            Button btnr = new Button();
            btnr.Text = "RESTART";
            btnr.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnr.Size = new Size(150, 34);
            btnr.Location = new Point(326, 360);
            btnr.BackColor = Color.FromArgb(200, 70, 70);
            btnr.ForeColor = Color.White;
            btnr.FlatStyle = FlatStyle.Flat;
            btnr.FlatAppearance.BorderSize = 0;
            btnr.Click += new EventHandler(btnr_Click);
            this.Controls.Add(btnr);

            string[] leg = {
                "Crvena tacka = tacno mesto",
                "Zuta tacka  = tacan simbol,",
                "  ali pogresno mesto"
            };
            int ly = 400;
            foreach (string line in leg)
            {
                Label ll = new Label();
                ll.Text = line;
                ll.ForeColor = Color.FromArgb(110, 110, 130);
                ll.Font = new Font("Segoe UI", 8f);
                ll.AutoSize = true;
                ll.Location = new Point(326, ly);
                this.Controls.Add(ll);
                ly += 17;
            }
        }

        void btnNew_Click(object sender, EventArgs e)
        {
            NewGame();
        }
        void btnr_Click(object sender, EventArgs e)
        {
            RGame();
        }

        void RGame()
        {
            guesses = new int[ROWS][];
            feedbackArr = new int[ROWS][];
            currentRow = 0;
            currentGuess = new int[0];
            gameOver = false;

            SetStatus("Klikni simbol ->", Color.FromArgb(200, 200, 200));
            pnlBoard.Invalidate();
            RefreshSymButtons();
        }

        void NewGame()
        {
            Random rnd = new Random();
            secret = new int[COLS];
            for (int i = 0; i < COLS; i++)
                secret[i] = rnd.Next(SYMBOLS.Length);

            RGame();
        }

        void symBtn_Click(object sender, MouseEventArgs e)
        {
            if (gameOver) return;
            if (currentGuess.Length >= COLS) return;

            int idx = (int)((Panel)sender).Tag;
            int[] tmp = new int[currentGuess.Length + 1];
            currentGuess.CopyTo(tmp, 0);
            tmp[currentGuess.Length] = idx;
            currentGuess = tmp;

            pnlBoard.Invalidate();
            RefreshSymButtons();

            if (currentGuess.Length == COLS)
                CheckGuess();
        }

        void CheckGuess()
        {
            int blacks = 0, whites = 0;
            int[] sLeft = (int[])secret.Clone();
            int[] gLeft = (int[])currentGuess.Clone();

            for (int i = 0; i < COLS; i++)
                if (gLeft[i] == sLeft[i]) { blacks++; sLeft[i] = -1; gLeft[i] = -1; }

            for (int i = 0; i < COLS; i++)
            {
                if (gLeft[i] == -1) continue;
                for (int j = 0; j < COLS; j++)
                {
                    if (sLeft[j] == -1) continue;
                    if (gLeft[i] == sLeft[j]) { whites++; sLeft[j] = -1; break; }
                }
            }

            guesses[currentRow] = (int[])currentGuess.Clone();
            feedbackArr[currentRow] = new int[] { blacks, whites };
            currentRow++;
            currentGuess = new int[0];

            pnlBoard.Invalidate();
            RefreshSymButtons();

            if (blacks == COLS)
            {
                gameOver = true;
                SetStatus("BRAVO!\nPogodio si za " + currentRow + " pokusaja!", Color.FromArgb(245, 200, 66));
            }
            else if (currentRow >= ROWS)
            {
                gameOver = true;
                SetStatus("Kraj igre!", Color.FromArgb(220, 80, 80));

            }
            else
            {
                SetStatus("Tacno mesto: " + blacks + "\nTacan simbol: " + whites,
                          Color.FromArgb(200, 200, 200));
            }
        }

        void pnlBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver) return;
            int rowY = PADT + currentRow * (CELL + GAP);
            if (e.Y < rowY || e.Y > rowY + CELL) return;
            for (int c = 0; c < currentGuess.Length; c++)
            {
                int colX = PADL + c * (CELL + GAP);
                if (e.X >= colX && e.X <= colX + CELL)
                {
                    int[] tmp = new int[c];
                    Array.Copy(currentGuess, tmp, c);
                    currentGuess = tmp;
                    pnlBoard.Invalidate();
                    RefreshSymButtons();
                    return;
                }
            }
        }

        void pnlBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (int r = 0; r < ROWS; r++)
            {
                int rowY = PADT + r * (CELL + GAP);

                using (Brush b = new SolidBrush(Color.FromArgb(80, 80, 100)))
                using (Font f = new Font("Segoe UI", 8f))
                    g.DrawString((r + 1).ToString(), f, b, 2, rowY + CELL / 2 - 7);

                for (int c = 0; c < COLS; c++)
                {
                    int colX = PADL + c * (CELL + GAP);
                    bool isActive = (r == currentRow && !gameOver);
                    bool isFilled = isActive && c < currentGuess.Length;
                    bool isNextSlot = isActive && c == currentGuess.Length;

                    Color bgCol = isFilled ? Color.FromArgb(40, 40, 70)
                                           : Color.FromArgb(20, 20, 40);
                    using (Brush b = new SolidBrush(bgCol))
                        g.FillEllipse(b, colX, rowY, CELL, CELL);

                    Color borderCol = isNextSlot ? Color.FromArgb(245, 200, 66)
                                    : isFilled ? Color.FromArgb(100, 100, 140)
                                    : r < currentRow ? Color.FromArgb(60, 60, 80)
                                    : Color.FromArgb(40, 40, 60);
                    float bw = isNextSlot ? 2f : 1.5f;
                    using (Pen p = new Pen(borderCol, bw))
                        g.DrawEllipse(p, colX, rowY, CELL, CELL);

                    if (r < currentRow && guesses[r] != null)
                        DrawSymbol(g, guesses[r][c], colX, rowY, CELL);
                    else if (isActive && c < currentGuess.Length)
                        DrawSymbol(g, currentGuess[c], colX, rowY, CELL);
                }

                if (r < currentRow && feedbackArr[r] != null)
                {
                    int fbX = PADL + COLS * (CELL + GAP) + 4;
                    int blacks = feedbackArr[r][0];
                    int whites = feedbackArr[r][1];
                    int[] dots = new int[COLS];
                    for (int i = 0; i < blacks; i++) dots[i] = 2;
                    for (int i = blacks; i < blacks + whites; i++) dots[i] = 1;

                    for (int d = 0; d < COLS; d++)
                    {
                        int dx = fbX + (d % 2) * (FB + 3);
                        int dy = rowY + (d / 2) * (FB + 3) + (CELL - 2 * (FB + 3)) / 2;
                        Color dc = dots[d] == 2 ? Color.FromArgb(220, 60, 60)
                                 : dots[d] == 1 ? Color.FromArgb(255, 215, 0)
                                 : Color.FromArgb(50, 50, 70);
                        Color bc = dots[d] == 2 ? Color.FromArgb(150, 150, 150)
                                 : dots[d] == 1 ? Color.FromArgb(200, 200, 200)
                                 : Color.FromArgb(60, 60, 80);
                        using (Brush b = new SolidBrush(dc))
                            g.FillEllipse(b, dx, dy, FB, FB);
                        using (Pen p = new Pen(bc, 1f))
                            g.DrawEllipse(p, dx, dy, FB, FB);
                    }
                }
            }

            if (gameOver && currentRow >= ROWS)
                DrawSecret(g);
        }



        void DrawSecret(Graphics g)
        {
            int sepY = PADT + ROWS * (CELL + GAP) + 10;
            // separator line
            using (Pen sep = new Pen(Color.FromArgb(80, 80, 120), 1f))
                g.DrawLine(sep, PADL, sepY, PADL + COLS * (CELL + GAP) - GAP, sepY);

            // label
            using (Font lf = new Font("Segoe UI", 8f))
            using (Brush lb = new SolidBrush(Color.FromArgb(180, 80, 80)))
                g.DrawString("Resenje:", lf, lb, PADL, sepY + 4);

            // symbols
            int symY = sepY + 20;
            for (int i = 0; i < COLS; i++)
                DrawSymbol(g, secret[i], PADL + i * (CELL + GAP), symY, CELL);
        }

        void DrawSymbol(Graphics g, int idx, int x, int y, int size)
        {
            string sym = SYMBOLS[idx];
            Color col = SYM_COLORS[idx];
            float fs = size * 0.42f;
            using (Font f = new Font("Segoe UI Symbol", fs, FontStyle.Bold))
            using (Brush b = new SolidBrush(col))
            {
                SizeF sz = g.MeasureString(sym, f);
                g.DrawString(sym, f, b,
                    x + (size - sz.Width) / 2f,
                    y + (size - sz.Height) / 2f);
            }
        }

        void symBtn_Paint(object sender, PaintEventArgs e)
        {
            Panel p = (Panel)sender;
            int i = (int)p.Tag;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Brush b = new SolidBrush(p.BackColor))
                g.FillRectangle(b, 0, 0, p.Width, p.Height);
            using (Pen pen = new Pen(Color.FromArgb(60, 60, 90), 1.5f))
                g.DrawRectangle(pen, 1, 1, p.Width - 2, p.Height - 2);
            DrawSymbol(g, i, 0, 0, CELL);
        }

        void symBtn_MouseEnter(object sender, EventArgs e)
        {
            Panel p = (Panel)sender;
            if (!gameOver && currentGuess.Length < COLS)
                p.BackColor = Color.FromArgb(40, 40, 70);
        }

        void symBtn_MouseLeave(object sender, EventArgs e)
        {
            ((Panel)sender).BackColor = Color.FromArgb(15, 15, 30);
        }

        void RefreshSymButtons()
        {
            bool blocked = gameOver || currentGuess.Length >= COLS;
            foreach (Panel p in symButtons)
            {
                p.Cursor = blocked ? Cursors.Default : Cursors.Hand;
                p.BackColor = Color.FromArgb(15, 15, 30);
                p.Invalidate();
            }
        }

        void SetStatus(string msg, Color col)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = col;
        }
    }
}
