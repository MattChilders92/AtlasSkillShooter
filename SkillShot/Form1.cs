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
        int foundCount = 0;
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
                if (running)
                {
                    button1_Click(null, null);
                }
                return result;
            }
            Process p = ps[0];
            GetWindowRect(p.MainWindowHandle, ref result);
            return result;
        }

        public SkillShot()
        {
            InitializeComponent();
            setupEnv();
            RefreshTimer.Enabled = true;
            updateStats();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            setupEnv();
            if (offset.Width == 0 || offset.Height == 0)
            {
                doSearch(true);
            } else
            {
                running = !running;
            }
            startButton.Text = running ? "Stop" : "Start";
            RefreshTimer.Enabled = true;
        }


        private void reset(object sender, EventArgs e)
        {
            Properties.Settings.Default.hitRate = new Point();
            Properties.Settings.Default.Offset = new Rectangle();
            Properties.Settings.Default.Save();
            doSearch(!searchMode);
            RefreshTimer.Enabled = true;
            running = true;
            setupEnv();
        }

        private void setupEnv(Rectangle overrideOffect = new Rectangle())
        {
            // setup offset from storage
            offset = overrideOffect.Width > 0 ? overrideOffect : Properties.Settings.Default.Offset;
            screenSize = GetDpiSafeResolution();
            widthScale = 1.0 * screenSize.Width / refSize.Width;
            heightScale = 1.0 * screenSize.Height / refSize.Height;
            debugButton.Text = debugMode ? "Stop Debugging" : "Debug";
            Height = debugMode ? 503 : 170;
            var screenCoords = getAtlasRectangle();
            scaledOffset = new Rectangle(screenCoords.X + (int)(widthScale * offset.X), screenCoords.Y + (int)(heightScale * offset.Y), (int)(widthScale * offset.Width), (int)(heightScale * offset.Height));
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                WindowScreenshot(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "window_screenshot.jpg", ImageFormat.Jpeg);
            } catch (Exception ex)
            {
                errorBox.Text = "ERROR: " + ex.Message + "/n Trace:" + ex.StackTrace;
                button1_Click(null, null);
            }
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
                if (running)
                {
                    RefreshTimer.Enabled = false;
                    running = false;
                    startButton.Text = running ? "Stop" : "Start";
                }
                return;
            }
            if ((!searchMode && (offset.Width == 0 | scaledOffset.Width == 0 | offset.Height == 0 | scaledOffset.Height == 0)))
            {
                RefreshTimer.Enabled = true;
                doSearch(true);
                return;
            }
            Process p = ps[0];
            Graphics g;
            if (searchMode)
            {
                if (useOffset.Width == 0 | useOffset.Height == 0)
                {
                    return;
                }
                bmp = new Bitmap(useOffset.Width, useOffset.Height);
                g = Graphics.FromImage(bmp);
                g.CopyFromScreen(useOffset.X, useOffset.Y, 0, 0, useOffset.Size, CopyPixelOperation.SourceCopy);
            } else
            {
                bmp = new Bitmap(scaledOffset.Width, scaledOffset.Height);
                g = Graphics.FromImage(bmp);
                g.CopyFromScreen(scaledOffset.Left, scaledOffset.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            }
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            g.Dispose();

            if (bmp != null)
            {
                if (searchMode)
                {
                    var doingSkillShot = isProgressPresent(bmp);
                    doingSkillShot.X += useOffset.X;
                    doingSkillShot.Y += useOffset.Y;
                    if (doingSkillShot.Height != 0 && doingSkillShot.Width != 0)
                    {
                        foundCount++;
                        if (foundLocations.ContainsKey(doingSkillShot))
                        {
                            foundLocations[doingSkillShot]++;
                        }
                        else
                        {
                            foundLocations[doingSkillShot] = 1;
                        }
                    }
                    else
                    {
                        foundCount++;
                    }
                    if (foundCount > 10 && foundLocations.Keys.Count > 0)
                    {
                        var topRect = new Rectangle();
                        var topCount = 0;
                        for (var i = 0; i < foundLocations.Keys.Count; i++)
                        {
                            var key = foundLocations.Keys.ToArray()[i];
                            var val = foundLocations[key];
                            if (val > topCount)
                            {
                                topCount = val;
                                topRect = key;
                            }
                        }
                        resultBox.Items.Add(topRect);

                        offset = topRect;
                        var unscaledOffset = new Rectangle(
                            (int)(topRect.X / widthScale),
                            (int)(topRect.Y / heightScale),
                            (int)(topRect.Width / widthScale),
                            (int)(topRect.Height / heightScale)
                        );

                        Properties.Settings.Default.Offset = unscaledOffset;
                        Properties.Settings.Default.Save();
                        doSearch(false);
                        running = true;
                        startButton.Text = running ? "Stop" : "Start";
                        RefreshTimer.Enabled = true;
                        foundCount = 0;
                        foundLocations = new Dictionary<Rectangle, int>() { };
                        setupEnv();
                        resultBox.Items.Add("Offset set to " + offset + ", (scaled: " + scaledOffset + ")");
                    }
                } else if (running) {
                    var doingSkillShot = isProgressPresent(bmp);
                    if (doingSkillShot.Height != 0)
                    {
                        var coords = getGoal(bmp, doingSkillShot);
                        var goal = coords[1];
                        var current = coords[0];
                        if (coords[2].Width > 200 * heightScale && goal.X > 5 && goal.Width > 5)
                        {
                            var pivotPoint = 150;
                            var wiggleMag = 70;
                            var wiggleRoom = (((pivotPoint * widthScale) - coords[1].Width) / (pivotPoint * widthScale)) * wiggleMag;
                            //wiggleRoom = 75 * widthScale;

                            var shouldClick = current.Right > (goal.Left - wiggleRoom) && current.Left < (goal.Right + wiggleRoom);
                            var goalStart = (int)(1000.0 * (coords[1].X - coords[2].X) / coords[2].Width) / 10.0;
                            var goalEnd = (int)((1000.0 * (coords[1].X + coords[1].Width) - coords[2].X) / coords[2].Width) / 10.0;
                            var currentPrecent = (int)((1000.0 * (current.X + (current.Width / 2.0)) - coords[2].X) / coords[2].Width) / 10.0;
                            var coordsString = 
                                System.DateTime.Now.ToLongTimeString() + ", " + widthScale + ": Current: " + current.Left + ", " +
                                "Goal: [" + coords[1].X + " - " + (coords[1].X + coords[1].Width) + "] +/- " + (int)(wiggleRoom) + " " +
                                "Total: [" + coords[2].X + " - " + (coords[2].X + coords[2].Width) + "]    Shot at (" + currentPrecent  + "%) going for (" + goalStart + "% - " + goalEnd + "%)";
                            if (shouldClick && !clicked)
                            {
                                status.Text = "Status: At (" + currentPrecent + "%) going for (" + goalStart + "% - " + goalEnd + "%)";
                                resultBox.Items.Add(coordsString);
                                previousMsg = coordsString;
                                VirtualMouse.LeftClick();
                                clicked = true;
                                waitCount = 0;
                            }
                            if (clicked && previousMsg != "" && (centerColor == "RED" || centerColor == "GREEN"))
                            {
                                if (waitCount == 10)
                                {
                                    var hitRate = Properties.Settings.Default.hitRate;
                                    hitRate.X += (centerColor == "RED") ? 0 : 1;
                                    hitRate.Y += (centerColor == "RED") ? 1 : 0;
                                    Properties.Settings.Default.hitRate = hitRate;
                                    Properties.Settings.Default.Save();
                                    updateStats();
                                    if (centerColor == "RED")
                                    {
                                        var savePath = basePath + "\\miss-" + System.DateTime.Now.ToFileTimeUtc() + ".png";

                                        var rate = (int)(hitRate.X * 1000.0 / (hitRate.X + hitRate.Y)) / 10.0;
                                        resultBox.Items.Add("MISSED, Saving SS (" + savePath  + ").");
                                        resultBox.Items.Add("Bot Status: Hits " + hitRate.X + "; Misses " + hitRate.Y + "; SuccessRate: (" + rate + "%)");
                                        bmp.Save(savePath, format);
                                        pictureBox1.ImageLocation = savePath;
                                    }
                                    frameCount = -100;
                                    resultBox.Items.Remove(previousMsg);
                                    resultBox.Items.Add(previousMsg + " Result: " + (centerColor == "RED" ? "Missed!" : "Nailed It!"));
                                    previousMsg = "";
                                    status.Text = "Status: See a SkillShot, Already Clicked (" + (centerColor == "RED" ? "Miss" : "Hit") + ")";
                                } else
                                {
                                    waitCount++;
                                }
                            }
                        }
                    } else
                    {
                        status.Text = "Status: Waiting for SkillShot";
                        clicked = false;
                        waitCount = 0;
                    }
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

        public Rectangle[] getGoal(Bitmap bmp, Rectangle coords) // [A....C....X..Y......Z]  returns [[Current], [X,Y], [A,Z]]
        {
            int searchY = 1;
            int greys = 0;
            int searchX = coords.Width / 2;
            Rectangle boxCoords = new Rectangle(0, 0, 0, 0);
            Rectangle goalCoords = new Rectangle(0, 0, 0, 0);
            Rectangle current = new Rectangle(0, 0, 0, 0);
            while (true && coords.Width > 0 && coords.Height > 0 && searchX < coords.Width && searchY < coords.Height)
            {
                Color searchClr = bmp.GetPixel(coords.X + searchX, coords.Y + searchY);
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

            for (searchX = 1; searchY > 0 && bmp.Width > 0 && searchX < bmp.Width && searchY > 0 && searchY < bmp.Height; searchX++)
            {
                Color searchClr = bmp.GetPixel(searchX, searchY);
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

            return new Rectangle[] { current, goalCoords, boxCoords };
        }

        private Rectangle isProgressPresent(Bitmap bmp)
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
                    result.Height = Math.Min(searchY - result.Y, result.Y + blackSize * 2 + whiteSize + 5);
                    // result.Height = Math.Min(searchY - result.Y, result.Y + blackSize * 2 + whiteSize + 5); // works at 3540
                }
            }
            found = greyWhite > 9 * heightScale && blackSection == 2;
            colors.Text = "Black Sections: " + blackSection + ", MidSection: " + greyWhite + " = " + found;
            if (found)
            {
                searchY = result.Y + result.Height / 2;
                var mid = 0;
                var black = 0;
                other = 0;
                var center = bmp.Width / 2;
                for (searchX = center; searchX > 0 && result.Width == 0; searchX--)
                {
                    Color searchClr = bmp.GetPixel(searchX, searchY);
                    string colorName = categorizeColor(searchClr);
                    if (colorName == "GREY" | colorName == "WHITE" | colorName == "GREEN" | colorName == "RED")
                    {
                        mid++;
                        black = (black <= 0 ? 0 : black - 1);
                    } else if (colorName == "BLACK")
                    {
                        black++;
                    }
                    if (mid > 15 * widthScale && black > 7 * heightScale)
                    {
                        other++;
                        if (other == 5)
                        {
                            // scale 1.5     + 12
                            // scale 4       + 4
                            // var diff = center - (searchX - 8 * widthScale); // works at 3540
                            // var normalizedDiff = (int)Math.Min(diff, result.Height * 7.5); // works at 3540
                            var diff = center - searchX + 4;
                            var normalizedDiff = (int)Math.Min(diff, result.Height * 7.5) + 3;
                            result.X = center - normalizedDiff;
                            result.Width = normalizedDiff * 2 + 5;
                        }
                    }
                }
                var ratio = result.Height * 1.0 / result.Width;
                if (ratio < 0.05 | ratio > 0.15)
                {
                    found = false;
                }
            }
            return found ? result : new Rectangle(0, 0, 0, 0);
        }

        private string categorizeColor(Color color)
        {
            if(color.R < 85 && color.G < 85 && color.B < 85)
            {
                return "BLACK";
            } else if (color.R > 215 && color.G > 215 && color.B > 215)
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
            else if (color.R < 185 && color.G < 185 && color.B < 185 && color.R > 115 && color.G > 115 && color.B > 115)
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
            if (mode)
            {
                var res = GetDpiSafeResolution();
                useOffset = new Rectangle(new Point(), res);
            }
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
