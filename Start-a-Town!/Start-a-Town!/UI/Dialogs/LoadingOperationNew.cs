using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Start_a_Town_.UI
{
    /// <summary>
    /// TODO: add text to each task
    /// </summary>
    class LoadingOperationNew
    {
        string Name;
        DialogLoading LoadingWindow;
        IEnumerator<Tuple<string, float>> Tasks;
        Progress Progress;
        Action OnFinish = () => { };
        //public LoadingOperationNew(string name, params LoadingTask[] tasks)
        //{
        //    this.Name = name;
        //    this.Tasks = new List<LoadingTask>(tasks);
        //    this.Progress = new Progress(0, this.Tasks.Count, 0);
        //}
        public LoadingOperationNew(string name, IEnumerable<Tuple<string, float>> tasks)
        {
            this.Tasks = tasks.GetEnumerator();

            this.LoadingWindow = new DialogLoading(name);
            this.LoadingWindow.OnUpdate = () =>
                {
                    this.Tasks.MoveNext();
                    this.LoadingWindow.Refresh(this.Tasks.Current.Item1, this.Tasks.Current.Item2);
                };
            this.LoadingWindow.ShowDialog();
            this.Name = name;
            this.Progress = new Progress(0, 100, 0);
        }


        internal static void Start(string name, IEnumerable<Tuple<string, float>> tasks)
        {
            var tasksEnum = tasks.GetEnumerator();

            //var dialog = new DialogLoading(name);
            //dialog.ShowDialog();
            //dialog.Hide();
            //return;

            tasksEnum.MoveNext();
            var dialog = new DialogLoading(name, tasksEnum.Current.Item1, tasksEnum.Current.Item2);
            dialog.ShowDialog();
            dialog.OnDrawAction = (sb, bounds) => Perform(tasksEnum, dialog);
        }
        //static int t = 0;
        private static void Perform(IEnumerator<Tuple<string, float>> tasksEnum, DialogLoading dialog)
        {
            //if (t++ < 10)
            //    return;
            //t = 0;
            //if (!dialog.IsValidated)
            //    return;
            if (!tasksEnum.MoveNext())
            {
                dialog.Hide();
                tasksEnum.Dispose();
            }
            dialog.Refresh(tasksEnum.Current.Item1, tasksEnum.Current.Item2);

            //dialog.Invalidate(true);
        }
        void Perform()
        {
            //this.LoadingWindow.ShowDialog();
            //int n = 0;
            //foreach (var task in this.Tasks)
            //{
            //    //this.LoadingWindow.Refresh(task.Name, n++ / (float)Tasks.Count);
            //    this.LoadingWindow.Refresh(task.Item1, task.Item2);
            //    //task.Task();
            //}
            this.Tasks.MoveNext();
            this.LoadingWindow.Refresh(this.Tasks.Current.Item1, this.Tasks.Current.Item2);

            //this.OnFinish();
            //this.LoadingWindow.Hide();
        }


    }
}
