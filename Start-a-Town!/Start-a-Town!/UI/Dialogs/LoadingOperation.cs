using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    /// <summary>
    /// TODO: add text to each task
    /// </summary>
    class LoadingOperation
    {
        string Name;
        DialogLoading LoadingWindow;
        List<LoadingTask> Tasks;
        Progress Progress;
        Action OnFinish = () => { };
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
        internal static LoadingOperation StartNew(string name, IEnumerable<LoadingTask> tasks, Action OnFinishSaving)
        {
            var obj = new LoadingOperation(name, tasks) { OnFinish = OnFinishSaving };
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
            this.OnFinish();
            this.LoadingWindow.Hide();
        }


    }
}
