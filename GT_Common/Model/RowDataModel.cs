using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class RowDataModel : INotifyPropertyChanged
    {
        private string col0;
        private string col1;
        private string col2;
        private string col3;

        public string Col0
        {
            get => col0;
            set
            {
                if (col0 != value)
                {
                    col0 = value;
                    OnPropertyChanged(nameof(Col0));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                }
            }
        }

        public string Col1
        {
            get => col1;
            set
            {
                if (col1 != value)
                {
                    col1 = value;
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                }
            }
        }

        public string Col2
        {
            get => col2;
            set
            {
                if (col2 != value)
                {
                    col2 = value;
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                }
            }
        }

        public string Col3
        {
            get => col3;
            set
            {
                if (col3 != value)
                {
                    col3 = value;
                    OnPropertyChanged(nameof(Col3));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RowDataModel2 : INotifyPropertyChanged
    {
        private string col0;
        private string col1;
        //private string col2;
        //private string col3;

        public string Col0
        {
            get => col0;
            set
            {
                if (col0 != value)
                {
                    col0 = value;
                    OnPropertyChanged(nameof(Col0));
                    OnPropertyChanged(nameof(Col1));
                }
            }
        }

        public string Col1
        {
            get => col1;
            set
            {
                if (col1 != value)
                {
                    col1 = value;
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col1));
                }
            }
        }

        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }



    public class TestItemMD : INotifyPropertyChanged
    {
        private string Test_item_name;
        private string Test_item_up;
        private string Test_item_value;
        private string Test_item_down;
        private string Test_item_result;

        public string Col0
        {
            get => Test_item_name;
            set
            {
                if (Test_item_name != value)
                {
                    Test_item_name = value;
                    OnPropertyChanged(nameof(Col0));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                    OnPropertyChanged(nameof(Col4));
                }
            }
        }

        public string Col1
        {
            get => Test_item_up;
            set
            {
                if (Test_item_up != value)
                {
                    Test_item_up = value;
                    OnPropertyChanged(nameof(Col0));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                    OnPropertyChanged(nameof(Col4));
                }
            }
        }

        public string Col2
        {
            get => Test_item_value;
            set
            {
                if (Test_item_value != value)
                {
                    Test_item_value = value;
                    OnPropertyChanged(nameof(Col0));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                    OnPropertyChanged(nameof(Col4));
                }
            }
        }

        public string Col3
        {
            get => Test_item_down;
            set
            {
                if (Test_item_down != value)
                {
                    Test_item_down = value;
                    OnPropertyChanged(nameof(Col0));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                    OnPropertyChanged(nameof(Col4));
                }
            }
        }

        public string Col4
        {
            get => Test_item_result;
            set
            {
                if (Test_item_result != value)
                {
                    Test_item_result = value;
                    OnPropertyChanged(nameof(Col0));
                    OnPropertyChanged(nameof(Col1));
                    OnPropertyChanged(nameof(Col2));
                    OnPropertyChanged(nameof(Col3));
                    OnPropertyChanged(nameof(Col4));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
