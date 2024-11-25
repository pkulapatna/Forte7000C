using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte7000E.Services
{
   public class DataOutput : BindableBase
   {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StrValue { get; set; }
        public string FieldType { get; set; }
        public string FieldFormat { get; set; }

        public DataOutput(int _id, string _name, string _ftype, string _ffmt)
        {
            Id = _id;
            Name = _name;
            FieldType = _ftype;
            FieldFormat = _ffmt;
        }
   }

    public class CheckedListItem : BindableBase
    {

        public int Id { get; set; }

        private bool _ischeck;
        public bool IsChecked
        {
            get { return _ischeck; }
            set { SetProperty(ref _ischeck, value); }
        }

        public string Name { get; set; }
        public string FieldType { get; set; }

        public CheckedListItem(int _id, string _name, bool _check, string _ftype)
        {
            Id = _id;
            IsChecked = _check;
            Name = _name;
            FieldType = _ftype;
        }
        public void CheckedListItemChanged()
        {
            CheckedListItem MyList = this;

            MyList.Id = Id;
            MyList.IsChecked = IsChecked;
            MyList.Name = Name;
        }
    }

    public class SerialDataOutput : BindableBase
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Format { get; set; }

        public SerialDataOutput(string _name, string _val, string _ffmt)
        {
            Name = _name;
            Value = _val;
            Format = _ffmt;
        }
    }
}
