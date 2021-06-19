using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class DialogLoading : Window
    {
        //Label LblLoading, LblInfo;
        //Panel PanelBar;
        //Bar ProgressBar;
        //Progress LoadProgress;
        LoadingWidget Widget;
        Action Callback;

        public DialogLoading() : this(() => { }) { }
        public DialogLoading(Action callback) : this("Please wait...", callback) { }
        public DialogLoading(string text) : this(text, () => { }) { }
        public DialogLoading(string text, Action callback)
        {
            this.Title = text;
            this.Movable = false;
            this.AutoSize = true;

            this.Widget = new LoadingWidget(200) { Callback = callback };
            this.Client.Controls.Add(this.Widget);

            this.Location = this.CenterScreen;
            this.Anchor = Vector2.One * .5f;
        }



        public void Refresh(string text, float percentage)
        {
            this.Widget.Refresh(text, percentage);
        }
    }

    class LoadingTask
    {
        public string Name;
        public Action Task;
        public LoadingTask(string name, Action task)
        {
            this.Name = name;
            this.Task = task;
        }
    }

    /// <summary>
    /// TODO: add text to each task
    /// </summary>
    class LoadingOperation
    {
        string Name;
        DialogLoading LoadingWindow;
        List<LoadingTask> Tasks;
        Progress Progress;
        public LoadingOperation(string name, params LoadingTask[] tasks)
        {
            this.Name = name;
            this.Tasks = new List<LoadingTask>(tasks);
            this.Progress = new Progress(0, this.Tasks.Count, 0);
        }
        public LoadingOperation(string name, IEnumerable<LoadingTask> tasks)
        {
            this.LoadingWindow = new DialogLoading(name);
            this.Name = name;
            this.Tasks = new List<LoadingTask>(tasks);
            this.Progress = new Progress(0, this.Tasks.Count, 0);
        }
        public void Start()
        {
            Task.Factory.StartNew(this.Perform);
        }
        static public LoadingOperation StartNew(string name, IEnumerable<LoadingTask> tasks)
        {
            var obj = new LoadingOperation(name, tasks);
            obj.Start();
            return obj;
        }
        void Perform()
        {
            this.LoadingWindow.ShowDialog();
            int n = 0;
            foreach (var task in this.Tasks)
            {
                //this.LoadingWindow.Refresh(this.Name, n++ / (float)Tasks.Count);
                //task();
                this.LoadingWindow.Refresh(task.Name, n++ / (float)Tasks.Count);
                task.Task();  
            }
            this.LoadingWindow.Hide();
        }
    }
}
