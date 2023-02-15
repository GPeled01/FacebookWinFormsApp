using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicFacebookFeatures
{
    public class ListControlAdapter
    {
        public ListControl ListControl { get; set; }

        public IList Items
        {
            get
            {
                IList items = null;

                if (ListControl is ListBox)
                {
                    items = (ListControl as ListBox).Items;
                }
                else if (ListControl is ComboBox)
                {
                    items = (ListControl as ComboBox).Items;
                }

                return items;
            }

            set
            {
                Items = value;
            }
        }

        public object SelectedItem
        {
            get
            {
                object selectedItem = null;

                if (ListControl is ListBox)
                {
                    selectedItem = (ListControl as ListBox).SelectedItem;
                }
                else if (ListControl is ComboBox)
                {
                    selectedItem = (ListControl as ComboBox).SelectedItem;
                }

                return selectedItem;
            }
        }

        public int ItemHeight
        {
            get
            {
                int itemHeight = 0;

                if (ListControl is ListBox)
                {
                    itemHeight = (ListControl as ListBox).ItemHeight;
                }
                else if (ListControl is ComboBox)
                {
                    itemHeight = (ListControl as ComboBox).ItemHeight;
                }

                return itemHeight;
            }

            set
            {
                if (ListControl is ListBox)
                {
                    (ListControl as ListBox).ItemHeight = value;
                }
                else if (ListControl is ComboBox)
                {
                    (ListControl as ComboBox).ItemHeight = value;
                }
            }
        }

        public void DisableSelection()
        {
            if (ListControl is ListBox)
            {
                (ListControl as ListBox).SelectionMode = SelectionMode.None;
            }
            else if (ListControl is ComboBox)
            {
                (ListControl as ComboBox).DroppedDown = false;
                (ListControl as ComboBox).Enabled = false;
                (ListControl as ComboBox).Text = "User have no pages";
            }
        }


    }
}
