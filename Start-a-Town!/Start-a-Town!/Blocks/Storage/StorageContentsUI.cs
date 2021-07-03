using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class StorageContentsUI : GroupBox
    {
        TableScrollableCompact<GameObject> Table;
        BlockStorage.BlockStorageEntity Container;
        public StorageContentsUI()
        {
            this.Table = new TableScrollableCompact<GameObject>(10, BackgroundStyle.Tooltip)
                .AddColumn("name", "Name", 150, o => new Label() { TextFunc = o.GetName });
            this.AddControls(this.Table);
        }
        public void Refresh(BlockStorage.BlockStorageEntity container)
        {
            this.Container = container;
            this.Table.Build(this.Container.Contents, false);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.ContentsChanged:
                    if (e.Parameters[0] == this.Container)
                        this.Refresh(this.Container);
                    break;

                default:
                    break;
            }
        }
    }
}
