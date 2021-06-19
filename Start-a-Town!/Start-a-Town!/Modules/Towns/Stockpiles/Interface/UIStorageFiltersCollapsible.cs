using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    class UIStorageFiltersCollapsible : GroupBox
    {
        public UIStorageFiltersCollapsible(IStorage storage)
        {
            var settings = storage.Settings;
            var categories = Def.Database.Values.OfType<ItemCategory>();
            var listtest =
                new ListBoxCollapsible<StorageFilter, CheckBoxNew>(150, 200);

            foreach (var cat in categories.Where(c => c.Filters.Any()))
            {
                var inode = new ListBoxCollapsibleNode<StorageFilter, CheckBoxNew>(cat.Label);
                inode.OnNodeControlInit = (catTickBox) =>
                {
                    catTickBox.LeftClickAction = () =>
                    {
                        var enabled = cat.Filters.Where(c => settings.ActiveFilters.Contains(c)).ToArray();
                        var disabled = cat.Filters.Except(enabled).ToArray();
                        var disabledCount = disabled.Length;
                        var enabledCount = enabled.Length;
                        if (enabledCount == 0 || disabledCount == 0)
                            PacketStorageFilters.Send(storage, cat.Filters.ToArray());
                        else
                        {
                            var toSend = disabledCount >= enabledCount ? enabled : disabled;
                            PacketStorageFilters.Send(storage, toSend.ToArray());
                        }
                    };
                    catTickBox.TickedFunc = () => inode.CustomLeafs.All(b => settings.ActiveFilters.Contains(b));
                };
                foreach (var f in cat.Filters)
                {
                    inode.AddLeaf(f);
                }
                listtest.AddNode(inode);
            }
            listtest.Build((i, itemTickBox) =>
            {
                itemTickBox.TickedFunc = () =>
                {
                    return settings.ActiveFilters.Contains(i);
                };
            });
            listtest.CallBack = (e) => PacketStorageFilters.Send(storage, e);

            this.Controls.Add(listtest);
        }

        //public UIStorageFiltersCollapsible(IStorage storage)
        //{
        //    var settings = storage.Settings;
        //    var categories = Def.Database.Values.OfType<StorageCategory>();
        //    var listtest =
        //        new ListBoxCollapsible(200, 200);

        //    foreach (var cat in categories.Where(c => c.Filters.Any()))
        //    {
        //        var inode = new ListBoxCollapsibleNode(cat.Label, ()=>
        //        {
        //            var catTickBox = new CheckBoxNew();
        //            catTickBox.LeftClickAction = () =>
        //            {
        //                var enabled = cat.Filters.Where(c => settings.ActiveFilters.Contains(c)).ToArray();
        //                var disabled = cat.Filters.Except(enabled).ToArray();
        //                var disabledCount = disabled.Length;
        //                var enabledCount = enabled.Length;
        //                if (enabledCount == 0 || disabledCount == 0)
        //                    PacketStorageFilters.Send(storage, cat.Filters.ToArray());
        //                else
        //                {
        //                    var toSend = disabledCount >= enabledCount ? enabled : disabled;
        //                    PacketStorageFilters.Send(storage, toSend.ToArray());
        //                }
        //            };
        //            //catTickBox.TickedFunc = () => inode.CustomLeafs.All(b => settings.ActiveFilters.Contains(b));
        //            return catTickBox;
        //        });
        //        foreach (var f in cat.Filters)
        //        {
        //            inode.AddLeaf(f);
        //        }
        //        listtest.AddNode(inode);
        //    }
        //    //listtest.Build((i, itemTickBox) =>
        //    //{
        //    //    itemTickBox.TickedFunc = () =>
        //    //    {
        //    //        return settings.ActiveFilters.Contains(i);
        //    //    };
        //    //});
        //    //listtest.CallBack = (e) => PacketStorageFilters.Send(storage, e);
        //    listtest.Build();
        //    this.Controls.Add(listtest);
        //}

        private void ToggleCategory(IStorage storage, IGrouping<ItemCategory, ItemDef> itype)
        {
            //throw new NotImplementedException();
        }
    }
}
