﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class InputBase: DependencyObject, INotifyPropertyChanged
    {
        public InputBase()
        {
            Name = string.Format("{{{0}}}", GetType().Name.Replace("Input", "").ToUpper());
        }

        public int InputNumber { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlIgnore]
        public virtual int ID { get { return Name.GetHashCode() % GetType().GetHashCode(); } }
        [XmlIgnore]
        public virtual State ControlledState { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}