using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CubeTimer{
    public partial class Form1 : MetroFramework.Forms.MetroForm{
        bool enabled = false;
        Timer timer;
        Timer iTimer;
        Stopwatch watch;
        BindingList<double> history;
        List<String> turns = new List<String>{ "U", "D", "L", "R", "B", "F" };
        List<String> modifiers = new List<string> { "", "\'", "2" };
        List<int> inspections = new List<int> { 0, 3, 5, 10, 15 };
        int inspectionTime;

        public Form1(){
            InitializeComponent();
            watch = new Stopwatch();
            timer = new Timer();
            timer.Interval = 10;
            timer.Tick += new EventHandler(updateTime);
            //
            iTimer = new Timer();
            iTimer.Interval = 10;
            iTimer.Tick += new EventHandler(updateInspection);
            //
            history = new BindingList<double>();
            historyList.DataSource = history;
            scrambleLabel.Text = "Scramble: " + generateScramble();
            Console.Out.WriteLine("L2".First() != "L2".First());
            //Fill inspection times
            inspectionDropdown.DataSource = inspections;
            inspectionTime = 0;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e){
            if (e.KeyCode == Keys.Space){
                if (!enabled){
                    label1.ForeColor = Color.LightGreen;
                }else{
                    label1.ForeColor = Color.Black;
                    stopWatch();
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e){
            if(e.KeyCode == Keys.Space){
                if (!watch.IsRunning && !enabled) {
                    enabled = true;
                    if (inspectionDropdown.SelectedIndex == 0) {
                        startWatch();
                        label1.ForeColor = Color.Red;
                    }else{
                        startInspection(inspections[inspectionDropdown.SelectedIndex] * 1000);
                        label1.ForeColor = Color.Blue;
                    }
                    e.Handled = true;
                }else if (enabled){
                    enabled = false;
                }
            }else if(e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back){
                deleteSelectedTime();
            }
        }
        private void removeFromHistory_Click(object sender, EventArgs e){
            deleteSelectedTime();
        }
        //Functions
        public void startWatch(){
            watch.Restart();
            timer.Start();
        }
        public void stopWatch(){
            watch.Stop();
            timer.Stop();
            //Record results as long as the timer was not stopped during the inspection time period
            if (!iTimer.Enabled) {
                //Add time and refresh statistics
                history.Add(double.Parse(label1.Text));
                refreshStats();
            }
            //Generate scramble
            scrambleLabel.Text = "Scramble: " + generateScramble();
        }
        public async void startInspection(int time){
            inspectionTime = time;
            watch.Restart();
            iTimer.Start();
            await stopInspection();
        }
        public async Task stopInspection() {
            await Task.Delay(inspectionTime);
            label1.ForeColor = Color.Red;
            startWatch();
            iTimer.Stop();
        }
        public void updateTime(object sender, EventArgs e){
            label1.Text = "" + String.Format("{0:0.00}", watch.ElapsedMilliseconds / 1000.0);
            //Application.DoEvents();
        }
        public void updateInspection(object sender, EventArgs e) {
            label1.Text = "" + String.Format("{0:0.00}", (((inspectionTime) - watch.ElapsedMilliseconds) / 1000.0));
        }
        public void refreshStats(){
            if (history.Count == 0){
                bestLabel.Text = "Best: n/a";
                averageLabel.Text = "Average: n/a";
            }else{
                bestLabel.Text = "Best: " + history.Min();
                averageLabel.Text = "Average: " + String.Format("{0:0.00}", history.Average());
            }
        }
        public String generateScramble(){
            Random random = new Random();
            //25 random turns that do not cancel the previous one
            List<String> scramble = new List<String>();
            for (int i = 0; i < 25; i++){
                String move = turns.ElementAt(random.Next(0, turns.Count())) + modifiers.ElementAt(random.Next(0, modifiers.Count()));
                //Checks if there are at least 2 moves that have been alraedy generated. Makes sure two of the same move types will not coincide (Ex. F F or R2 R). Makes sure two moves do not cancel each other out (Ex. U D U' or L2 R L2)
                if (scramble.Count <= 1 || (scramble[scramble.Count - 1].First() != move.First() && !cancelOut(scramble[scramble.Count - 2], move))){
                    if(scramble.Count == 1 && scramble[scramble.Count - 1].First() == move.First()){
                        i--;
                        continue;
                    }

                    scramble.Add(move);
                }else{
                    i--;
                }
            }
            return String.Join("  ", scramble);
        }
        public bool cancelOut(String moveA, String moveB){
            //Does not cancel out a move made 2 turns ago. EX Moveset that would fall through the cracks) L F L'
            if (moveA.First() != moveB.First() || (moveA.Length == 0 && moveB.Length == 0))
                return false; //Different moves (L and R)
            if (moveA.Length == 1 && moveB.Length == 2 && moveB.Last() == '\'')
                return true; //F counters F'
            if (moveB.Length == 1 && moveA.Length == 1 && moveA.Last() == '\'')
                return true; //F' counters F
            if (moveA.Length == 2 && moveB.Length == 2 && moveA.Last() == '2' && moveB.Last() == '2')
                return true; //F2 counters F2
            return false;
        }
        public void deleteSelectedTime(){
            try{
                int index = historyList.SelectedIndex;
                history.RemoveAt(index);
            }catch{
                //
            }
            historyList.DataSource = null;
            historyList.DataSource = history;
            refreshStats();
        }
    }
}
