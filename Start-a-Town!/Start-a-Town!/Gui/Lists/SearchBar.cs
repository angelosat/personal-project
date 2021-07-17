using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class SearchBar<TObject> : TextBox where TObject : class, ILabeled
    {
        IconButton IconClear = new IconButton(Icon.Cross) { BackgroundTexture = UIManager.Icon16Background };
        IListSearchable<TObject> List;
        public SearchBar(int width) : base(width)
        {
            this.IconClear.LocationFunc = () => new Vector2(this.Width, this.Height / 2);
            this.IconClear.Anchor = new Vector2(1,.5f);
            this.IconClear.LeftClickAction = () =>
              {
                  this.Text = "";
              };
        }
        public SearchBar<TObject> BindTo(IListSearchable<TObject> list)
        {
            this.List = list;
            return this;
        }
        protected override void OnTextChanged()
        {
            if (this.Text?.Length > 0 && !this.Controls.Contains(this.IconClear))
                this.AddControls(this.IconClear);
            else if (this.Text?.Length == 0 && this.Controls.Contains(this.IconClear))
                this.RemoveControls(this.IconClear);
            this.List?.Filter(o => o.Label.ToLower().Contains(this.Text.ToLower()));
        }
    }
}
