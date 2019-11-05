using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Diagnostics;
using SkillShot.MouseManipulator;

namespace SkillShot
{
    /*
     * Matt Childers   1/10/19
     * 
     * This code is the very definition of messy purpose built spaghetti code and 
     * should not be viewed or referenced by anyone for anything.
     * 
     * Enjoy
     * */

    public partial class SkillShot : Form
    {
        double widthScale = 1;
        double heightScale = 1;
        int frameCount = 0;
        int waitCount = 0;
        private Dictionary<Rectangle, int> foundLocations = new Dictionary<Rectangle, int>() {};

        private Bitmap bmp = null;
        private string centerColor = "";
        private string previousMsg = "";
        private bool clicked = false;
        private bool running = false;
        private bool searchMode = false;
        private bool debugMode = false;

        private Rectangle offset;
        private Rectangle useOffset;
        private Rectangle scaledOffset;
        private Size screenSize;

        private Size refSize = new Size(2560, 1440);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
        }

        private float getScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        private Size GetDpiSafeResolution()
        {
            var factor = getScalingFactor();
            using (Graphics graphics = this.CreateGraphics())
            {
                var rect = getAtlasRectangle();
                return new Size((int)(rect.Width * graphics.DpiX * factor) / 96, 
                    (int)(rect.Height * graphics.DpiY * factor) / 96);
            }
        }

        private Rectangle getAtlasRectangle()
        {
            var result = new Rectangle(new Point(), refSize);

            var ps = Process.GetProcessesByName("AtlasGame");
            if (ps.Length == 0)
            {
                status.Text = "ATLAS ISNT OPEN YET";
                return result;
            }
            Process p = ps[0];
            GetWindowRect(p.MainWindowHandle, ref result);
            return result;
        }

        public SkillShot()
        {
            InitializeComponent();
            running = true;
            setupEnv();
        }


        private void reset(object sender, EventArgs e)
        {
            Properties.Settings.Default.hitRate = new Point();
            Properties.Settings.Default.Offset = new Rectangle();
            Properties.Settings.Default.Save();
            setupEnv();
        }

        private void setupEnv()
        {
            var screenCoords = getAtlasRectangle();
            if (Properties.Settings.Default.Offset.Width > 0)
            {
                offset = Properties.Settings.Default.Offset;
            } else
            {
                offset = screenCoords;
                useOffset = getAtlasRectangle();
                searchMode = true;
            }
            screenSize = GetDpiSafeResolution();
            widthScale = 1.0 * screenSize.Width / refSize.Width;
            heightScale = 1.0 * screenSize.Height / refSize.Height;
            Height = debugMode ? 503 : 170;
            scaledOffset = new Rectangle(screenCoords.X + (int)(widthScale * offset.X), screenCoords.Y + (int)(heightScale * offset.Y), (int)(widthScale * offset.Width), (int)(heightScale * offset.Height));
            errorBox.Text = "ScreenSize = (" + screenSize + ")    AtlasCoords = (" + screenCoords + ")  scaledOffset = (" + scaledOffset + ")  offset = (" + offset + ")";

            updateStats();
            debugButton.Text = debugMode ? "Stop Debugging" : "Debug";
            RefreshTimer.Enabled = searchMode || running;
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                WindowScreenshot(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "window_screenshot.jpg", ImageFormat.Jpeg);
            } catch (Exception ex)
            {
                errorBox.Text = "ERROR: " + ex.Message + "/n Trace:" + ex.StackTrace;
            }
        }

        private bool isSetup()
        {
            return !(offset.Width <= 0 | scaledOffset.Width <= 0 | offset.Height == 0 | scaledOffset.Height == 0);
        }

        private void WindowScreenshot(String filepath, String filename, ImageFormat format)
        {
            if (bmp != null)
            {
                bmp.Dispose();
                bmp = null;
            }
            string basePath = filepath + "\\temp\\";
            bool exists = System.IO.Directory.Exists(basePath);

            if (!exists)
                System.IO.Directory.CreateDirectory(basePath);

            
            string fullpath = basePath + filename;
            string fullpath2 = basePath + "scaled_" + filename;
            string testPath = basePath + "test1.jpg";
            string tempPath = filepath + "\\temp\\temp.jpg";
            string success = basePath + "succeed";
            string[] result = new string[50];
            var ps = Process.GetProcessesByName("AtlasGame");
            if (ps.Length == 0)
            {
                 colors.Text = "ATLAS ISNT OPEN YET";
                return;
            }
            Process p = ps[0];
            if (!isSetup())
            {
                colors.Text = "Need to find the skillshot region of screen!";
                setupEnv();
                doSearch(true);
            }
            Graphics g;
            Rectangle screenshotBounds = searchMode ? useOffset : offset;
            if (screenshotBounds.Width <= 0 || screenshotBounds.Height <= 0)
            {
                return;
            }
            bmp = new Bitmap(screenshotBounds.Width, screenshotBounds.Height);
            g = Graphics.FromImage(bmp);
            g.CopyFromScreen(screenshotBounds.X, screenshotBounds.Y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            g.Dispose();

            if (bmp != null)
            {
                var doingSkillShot = IsProgressPresent(bmp);
                Rectangle[] coords = new Rectangle[] { };
                if (doingSkillShot.Height != 0 && doingSkillShot.X >= 0)
                {
                    Rectangle rounded = new Rectangle(RoundOff(doingSkillShot.X), RoundOff(doingSkillShot.Y), RoundOff(doingSkillShot.Width), RoundOff(doingSkillShot.Height));
                    var ratio = RoundOff(doingSkillShot.Width / doingSkillShot.Height, 1);
                    var screenRatio = RoundOff(screenshotBounds.Height / doingSkillShot.Height, 1);
                    Point skillPos = new Point((int)(100.0 * doingSkillShot.X / screenshotBounds.Width), (int)(100.0 * doingSkillShot.Y / screenshotBounds.Height));
                    var seemsRight = rounded.Height != 0 && rounded.Width != 0 && ratio >= 10 && ratio <= 14 && (!searchMode || (searchMode && skillPos.X > 25 && skillPos.X < 40 && skillPos.Y > 70 && skillPos.Y < 85));
                    doingSkillshotCheck.Checked = seemsRight;
                    if (seemsRight)
                    {
                        coords = getGoal(bmp, doingSkillShot);
                        if (searchMode && coords.Length == 3 && coords[1].Width > 0)
                        {
                            seeGoalCheck.Checked = true;
                            resultBox.Items.Insert(0, "MaybeOffset (" + rounded + ") ratio = " + ratio + ", positioning = " + (skillPos) + "%");
                            resultBox.Items.Insert(0, "Seems right, lets go with it.");
                            var unscaledOffset = new Rectangle(
                                (int)(doingSkillShot.X / widthScale),
                                (int)(doingSkillShot.Y / heightScale),
                                (int)(doingSkillShot.Width / widthScale),
                                (int)(doingSkillShot.Height / heightScale)
                            );
                            offset = doingSkillShot;

                            Properties.Settings.Default.Offset = offset;
                            Properties.Settings.Default.Save();
                            doSearch(false);
                            running = true;
                            RefreshTimer.Enabled = true;
                            setupEnv();
                            resultBox.Items.Insert(0, "Offset set to " + offset + ", (scaled: " + scaledOffset + ")");
                        }
                        if (coords.Length == 3 && coords[2].Width > 200 * heightScale && coords[1].X > 5 && coords[1].Width > 5)
                        {
                            seeGoalCheck.Checked = true;
                            var goal = coords[1];
                            var current = coords[0];
                            var wiggleRoom = (coords[2].Width / goal.Width - 5) * 3;
                            var currentMid = current.Left + current.Width / 2;

                            var shouldClick = currentMid > (goal.Left - wiggleRoom) && currentMid < (goal.Right + wiggleRoom);
                            var goalStart = (int)(1000.0 * (coords[1].X - coords[2].X) / coords[2].Width) / 10.0;
                            var goalEnd = (int)((1000.0 * (coords[1].X + coords[1].Width) - coords[2].X) / coords[2].Width) / 10.0;
                            var currentPrecent = (int)((1000.0 * (current.X + (current.Width / 2.0)) - coords[2].X) / coords[2].Width) / 10.0;
                            var coordsString =
                                System.DateTime.Now.ToLongTimeString() + ", " + widthScale + ": Current: " + current.Left + ", " +
                                "Goal: [" + coords[1].X + " - " + (coords[1].X + coords[1].Width) + "] +/- " + (int)(wiggleRoom) + " " +
                                "Total: [" + coords[2].X + " - " + (coords[2].X + coords[2].Width) + "]    Shot at (" + currentPrecent + "%) going for (" + goalStart + "% - " + goalEnd + "%)";
                            if (shouldClick && !clicked)
                            {
                                status.Text = "Status: At (" + currentPrecent + "%) going for (" + goalStart + "% - " + goalEnd + "%)";
                                previousMsg = coordsString;
                                VirtualMouse.LeftClick();
                                clicked = true;
                                waitCount = 0;
                            }
                            if (clicked && previousMsg != "" && (centerColor == "RED" || centerColor == "GREEN"))
                            {
                                if (waitCount == 5)
                                {
                                    var hitRate = Properties.Settings.Default.hitRate;
                                    hitRate.X += (centerColor == "RED") ? 0 : 1;
                                    hitRate.Y += (centerColor == "RED") ? 1 : 0;
                                    Properties.Settings.Default.hitRate = hitRate;
                                    Properties.Settings.Default.Save();
                                    updateStats();
                                    if (centerColor == "RED")
                                    {
                                        var savePath = basePath + "miss-" + System.DateTime.Now.ToFileTimeUtc() + ".png";

                                        var rate = (int)(hitRate.X * 1000.0 / (hitRate.X + hitRate.Y)) / 10.0;
                                        resultBox.Items.Insert(0, "MISSED, Saving SS (" + savePath + ").");
                                        resultBox.Items.Insert(0, "Bot Status: Hits " + hitRate.X + "; Misses " + hitRate.Y + "; SuccessRate: (" + rate + "%)");
                                        bmp.Save(savePath, format);
                                        pictureBox1.ImageLocation = savePath;
                                    }
                                    frameCount = -50;
                                    resultBox.Items.Remove(previousMsg);
                                    resultBox.Items.Insert(0, previousMsg + " Result: " + (centerColor == "RED" ? "Missed!" : "Nailed It!"));
                                    previousMsg = "";
                                    status.Text = "Status: See a SkillShot, Already Clicked (" + (centerColor == "RED" ? "Miss" : "Hit") + ")";
                                    doSearch(false);
                                }
                                else
                                {
                                    waitCount++;
                                }
                            }
                        } else
                        {
                            seeGoalCheck.Checked = false;
                        }
                    }
                } else
                {
                    status.Text = "Status: Waiting for SkillShot";
                    clicked = false;
                    seeGoalCheck.Checked = false;
                    doingSkillshotCheck.Checked = false;
                    waitCount = 0;
                }

                frameCount++;
                if (frameCount == 2 && waitCount != 10)
                {
                    bmp.Save(tempPath, format);
                    pictureBox1.ImageLocation = tempPath;
                    frameCount = 0;
                }
            }
            bmp.Dispose();
        }

        private void updateStats()
        {
            var hits = Properties.Settings.Default.hitRate;
            if (hits.X + hits.Y == 0)
            {
                botStats.Text = "Bot Stats: No stats yet!";
            } else
            {
                var rate = (int)(hits.X * 1000.0 / (hits.X + hits.Y)) / 10.0;
                botStats.Text = "Bot Status: Hits " + hits.X + "; Misses " + hits.Y + "; SuccessRate: (" + rate  + "%)";
            }
        }
        public int RoundOff(int number, int interval = 10)
        {
            int remainder = number % interval;
            number += (remainder < interval / 2) ? -remainder : (interval - remainder);
            return number;
        }
        public Rectangle[] getGoal(Bitmap bmp, Rectangle coords) // [A....C....X..Y......Z]  returns [[Current], [X,Y], [A,Z]]
        {
            int searchY = 0;
            int greys = 0;
            int searchX = coords.X + coords.Width / 2;
            Rectangle boxCoords = new Rectangle(0, 0, 0, 0);
            Rectangle goalCoords = new Rectangle(0, 0, 0, 0);
            Rectangle current = new Rectangle(0, 0, 0, 0);
            while (coords.Width > 0 && coords.Height > 0 && searchY < coords.Height)
            {
                Color searchClr = bmp.GetPixel(searchX, coords.Y + searchY);
                string colorName = categorizeColor(searchClr);
                if (colorName == "GREY" || colorName == "WHITE")
                {
                    greys++;
                } else
                {
                    if (greys < 5 * heightScale)
                    {
                        greys = 0;
                    } else
                    {
                        int startGrey = searchY - greys;
                        boxCoords.Y = startGrey;
                        boxCoords.Height = greys;
                        goalCoords.Y = startGrey;
                        goalCoords.Height = greys;
                        current.Y = startGrey;
                        current.Height = greys;
                        searchY -= greys / 2;
                        break;
                    }
                }
                searchY++;
            }
            greys = 0;
            var whites = 0;
            var foundStart = false;
            var end = 0;

            for (searchX = 1; searchY > 0 && coords.X + searchX < bmp.Width && searchX < coords.Width && searchY > 0 && coords.Y + searchY < bmp.Height; searchX++)
            {
                if (searchX < 0 || searchY < 0 || coords.X + searchX >= bmp.Width || coords.Y + searchY >= bmp.Height)
                {
                    continue;
                }

                Color searchClr = bmp.GetPixel(coords.X + searchX, coords.Y + searchY);
                string colorName = categorizeColor(searchClr);

                if (colorName == "BLACK" && greys < 5 * heightScale)
                {
                    greys = 0; // still in beginning
                }

                if (foundStart && (colorName == "RED" || colorName == "GREEN") && current.X == 0)
                {
                    current.X = searchX;
                    centerColor = colorName;
                } else if (foundStart && searchX > current.X && current.X > 0 && current.Width == 0)
                {
                    current.Width = searchX - current.X;
                }

                if (colorName == "GREY" || colorName == "WHITE" || colorName == "RED" || colorName == "GREEN")
                {
                    greys++;
                    if (!foundStart && greys > 10 * heightScale)
                    {
                        foundStart = true;
                        boxCoords.X = searchX;
                    }
                    if (foundStart && (colorName == "WHITE" || colorName == "RED" || colorName == "GREEN"))
                    {
                        if (colorName == "WHITE")
                        {
                            whites++;
                        }
                        if (whites > 10 * heightScale && goalCoords.X == 0 && (colorName != "RED" || colorName != "GREEN"))
                        {
                            goalCoords.X = searchX - whites;
                        }
                    } else
                    {
                        if (goalCoords.X > 0 && whites > 0 && goalCoords.Width == 0)
                        {
                            goalCoords.Width = searchX - goalCoords.X;
                        }
                        whites = 0;
                    }
                } else if (foundStart && boxCoords.Width == 0)
                {
                    if (end > 5 * heightScale)
                    {
                        boxCoords.Width = searchX - boxCoords.X - end;
                    } else
                    {
                        end++;
                    }
                }
            }
            return new Rectangle[] { current, goalCoords, coords };
        }

        private Rectangle IsProgressPresent(Bitmap bmp)
        {
            int searchX = bmp.Width / 2;
            int blackInRow = 0;
            int greyWhite = 0;
            int blackSection = 0;
            Rectangle result = new Rectangle();
            string colorCheck = "Color: ";
            var found = false;
            var blackSize = 0;
            var whiteSize = 0;
            int searchY = 1;
            var other = 0;
            var hasGoal = false;
            for (searchY = 1; bmp.Width > 0 && searchY < bmp.Height && !found; searchY++)
            {
                found = greyWhite > 11 * heightScale && blackSection == 2;
                Color searchClr = bmp.GetPixel(searchX, searchY);
                string colorName = categorizeColor(searchClr);
                colorCheck += ", " + categorizeColor(searchClr);
                if (colorName == "BLACK")
                {
                    blackInRow++;
                } else
                {
                    if (blackInRow > 7 * heightScale)
                    {
                        blackSection++; //searchY - blackInRow - 1;
                        if (blackSection == 1)
                        {
                            blackSize = (int)Math.Max(20 * heightScale, blackInRow - 1);
                            //blackSize = (int)Math.Max(20 * heightScale, blackInRow - 1); // works at 3540
                            result.Y = searchY - blackSize;
                        } else if (blackSection == 2)
                        {
                            result.Height = Math.Min(searchY - result.Y, result.Y + blackSize * 2 + whiteSize + 5);
                        }
                        blackInRow = 0;
                    } else if (blackInRow > 0 && !(colorName == "GREY" || colorName == "WHITE"))
                    {
                        blackInRow -= 1;
                    }
                    if (blackSection == 1 && (colorName == "GREY" || colorName == "WHITE"))
                    {
                        greyWhite++;
                        other = 0;
                        if (greyWhite > 9 * heightScale)
                        {
                            hasGoal = true;
                        }
                    } else if (blackSection >= 1)
                    {
                        greyWhite--;
                        other++;
                        if (other == 5)
                        {
                            blackSection = 0;
                            greyWhite = 0;
                            blackInRow = 0;
                            other = 0;
                        }
                    }
                }
            }
            if (blackInRow > 7 * heightScale)
            {
                blackSection++;
                if (blackSection == 2)
                {
                    result.Height = Math.Min(searchY - result.Y, result.Y + blackSize * 2 + whiteSize + 4);
                }
            }
            found = hasGoal && blackSection == 2;
            colors.Text = "Black Sections: " + blackSection + ", MidSection: " + greyWhite + " = " + found + ", SearchMode = " + searchMode;
            result.Width = (int)(result.Height * 11.5);
            result.X = (bmp.Width - result.Width) / 2;
            if (searchMode)
            {
                result.X += useOffset.X;
                result.Y += useOffset.Y;
            }
            return found ? result : new Rectangle(0, 0, 0, 0);
        }

        private string categorizeColor(Color color)
        {
            if(color.R < 45 && color.G < 45 && color.B < 45 && (Math.Abs(color.R - color.G) <= 15 && Math.Abs(color.R - color.B) <= 15 && Math.Abs(color.B - color.G) <= 2015))
            {
                return "BLACK";
            } else if (color.R > 230 && color.G > 230 && color.B > 230)
            {
                return "WHITE";
            }
            else if (color.R < 50 && color.G > 115 && color.B < 50)
            {
                return "GREEN";
            }
            else if (color.R > 115 && color.G < 49 && color.B < 49)
            {
                return "RED";
            }
            else if ((color.R < 185 && color.G < 185 && color.B < 185 && color.R > 115 && color.G > 115 && color.B > 115) ||
                (Math.Abs(color.R - color.G) <= 20 && Math.Abs(color.R - color.B) <= 20 && Math.Abs(color.B - color.G) <= 20))
            {
                return "GREY";
            }
            return "NONE (" + color.R + ", " + color.G + ", " + color.B + ")";
        }

        private void clear_Click(object sender, EventArgs e)
        {
            resultBox.Items.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            debugMode = !debugMode;
            setupEnv();
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle rect);

        private void doSearch(bool mode)
        {
            searchMode = mode;
            useOffset = getAtlasRectangle();
        }
    }

    namespace MouseManipulator
    {
        public static class VirtualMouse
        {
            [DllImport("user32.dll")]
            static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
            private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
            private const int MOUSEEVENTF_LEFTUP = 0x0004;
            public static void LeftClick()
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
            }
        }
    }
}
