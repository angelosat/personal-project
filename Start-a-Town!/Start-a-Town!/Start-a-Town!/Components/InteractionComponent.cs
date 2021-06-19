using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Tasks;
using Start_a_Town_.WorldEntities;
using Start_a_Town_.Interactions;

namespace Start_a_Town_.Components
{
    public class InteractionComponent : Component
    {
        List<int> Collection = new List<int>();

        public InteractionComponent(){}//IInteractable owner) { Owner = owner; }
        public InteractionComponent(int[] interactionIDarray)
        {
            AddRange(interactionIDarray);
        }

        public List<Task> GetTasks(GameObject actor, GameObject target)
        {
            List<Task> tasks = new List<Task>();
            foreach (int i in Collection)
            {
                //Interaction inter = InteractionManager.Instance.GetInteraction(i);
                //if (inter.IsAvailableTo(actor))
                    //tasks.Add(new Task(actor, target, inter));
                tasks.Add(new Task(actor, target, i));
            }
            return tasks;
        }

        public List<Interaction> GetInteractions()
        {
            List<Interaction> list = new List<Interaction>();
            foreach (int i in Collection)
                list.Add(InteractionManager.GetInteraction(i));
            return list;
        }

        public void Add(int interactionID)
        {
            Collection.Add(interactionID);
        }

        public void AddRange(int[] interactionIDarray)
        {
            Collection.AddRange(interactionIDarray);
        }

        public int Count
        { get { return Collection.Count; } }

        public List<int>.Enumerator GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        public int[] ToArray()
        {
            return Collection.ToArray();
        }

        public override string ToString()
        {
            return "Interactible";
        }

        public override object Clone()
        {
            InteractionComponent inter = new InteractionComponent();
            inter.Collection = new List<int>(Collection);
            return inter;
        }
    }
}
