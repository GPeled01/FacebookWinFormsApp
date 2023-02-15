using System.Windows.Forms;

namespace BasicFacebookFeatures
{
    public static class listControlFactory
    {
        public static ListControl CreateListControl(string i_ListControlName)
        {
            ListControl listControl = null;

            if (i_ListControlName == "ListBox")
            {
                listControl = new ListBox();
            }
            else if (i_ListControlName == "ComboBox")
            {
                listControl = new ComboBox();
                (listControl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            }

            return listControl;
        }
    }
}