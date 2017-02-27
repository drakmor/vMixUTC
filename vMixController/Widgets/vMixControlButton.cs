﻿using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.Extensions;
using vMixController.PropertiesControls;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlButton : vMixControl
    {
        [NonSerialized]
        DispatcherTimer _timer;
        int _pointer;
        int _waitBeforeUpdate = -1;
        DateTime _lastShadowUpdate = DateTime.Now;

        /// <summary>
        /// The <see cref="HasScriptErrors" /> property's name.
        /// </summary>
        public const string HasScriptErrorsPropertyName = "HasScriptErrors";

        private bool _hasScriptErrors = false;

        /// <summary>
        /// Sets and gets the HasScriptErrors property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool HasScriptErrors
        {
            get
            {
                return _hasScriptErrors;
            }

            set
            {
                if (_hasScriptErrors == value)
                {
                    return;
                }

                _hasScriptErrors = value;
                RaisePropertyChanged(HasScriptErrorsPropertyName);
            }
        }

        [XmlIgnore]
        public override State State
        {
            get
            {
                return base.State;
            }

            set
            {
                if (_internalState != null)
                {
                    _internalState.OnStateUpdated -= State_OnStateUpdated;
                    //_internalState.OnFunctionSend += State_OnFunctionSend;
                }

                base.State = value;

                if (_internalState != null)
                {
                    _internalState.OnStateUpdated += State_OnStateUpdated;
                    //_internalState.OnFunctionSend += State_OnFunctionSend;
                }
            }
        }

        public override void ShadowUpdate()
        {

            if (IsStateDependent && _internalState != null && (DateTime.Now - _lastShadowUpdate).TotalSeconds > 5)
            {
                _internalState.UpdateAsync();
                _lastShadowUpdate = DateTime.Now;
            }
            base.ShadowUpdate();
        }

        /*private void State_OnFunctionSend(object sender, FunctionSendArgs e)
        {
            if (e.Function != "") ;
                //UpdateActiveProperty();
            //throw new NotImplementedException();
        }*/

        private void State_OnStateUpdated(object sender, StateUpdatedEventArgs e)
        {
            if (e.Successfully)
                RealUpdateActiveProperty();
        }

        private void RealUpdateActiveProperty(bool skipStateDependency = false, vMixAPI.State stateToCheck = null)
        {
            if (stateToCheck == null) stateToCheck = _internalState;
            if ((!IsStateDependent && skipStateDependency) || stateToCheck == null) return;
            var result = true;
            HasScriptErrors = false;
            foreach (var item in _commands)
            {
                var input = State.Inputs.Where(x => x.Key == item.InputKey).FirstOrDefault()?.Number;
                if (string.IsNullOrWhiteSpace(item.Action.ActiveStatePath)) continue;
                var path = string.Format(item.Action.ActiveStatePath, item.InputKey, item.Parameter, item.StringParameter, item.Parameter - 1, input.HasValue ? input.Value : -1);
                var nval = GetValueByPath(stateToCheck, path);
                var val = nval == null ? "" : nval.ToString();
                HasScriptErrors = HasScriptErrors || nval == null;
                var aval = string.Format(item.Action.ActiveStateValue, GetInputNumber(item.InputKey), item.Parameter, item.StringParameter, item.Parameter - 1, input.HasValue ? input.Value : -1);
                var realval = aval;
                aval = aval.TrimStart('!');
                bool mult = (aval == "-" && ((val is string && string.IsNullOrWhiteSpace((string)val)) || (val == null) /*|| (val is bool && (bool)val == false)*/)) ||
                    (aval == "*") ||
                    (val != null && !(val is string) && aval == val.ToString()) ||
                    (val is string && (string)val == aval);
                if (!string.IsNullOrWhiteSpace(aval) && aval[0] == '!')
                    mult = !mult;
                result = result && mult;

            }
            Active = result;
        }

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Button");
            }
        }

        /// <summary>
        /// The <see cref="Commands" /> property's name.
        /// </summary>
        public const string CommandsPropertyName = "Commands";

        private ObservableCollection<vMixControlButtonCommand> _commands = new ObservableCollection<vMixControlButtonCommand>();

        /// <summary>
        /// Sets and gets the Actions property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixControlButtonCommand> Commands
        {
            get
            {
                return _commands;
            }

            set
            {
                if (_commands == value)
                {
                    return;
                }

                _commands = value;
                RaisePropertyChanged(CommandsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Enabled" /> property's name.
        /// </summary>
        public const string EnabledPropertyName = "Enabled";

        private bool _enabled = true;

        /// <summary>
        /// Sets and gets the Enabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                RaisePropertyChanged(EnabledPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Active" /> property's name.
        /// </summary>
        public const string ActivePropertyName = "Active";

        private bool _active = false;

        /// <summary>
        /// Sets and gets the Active property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                if (_active == value)
                {
                    return;
                }

                _active = value;
                RaisePropertyChanged(ActivePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsStateDependent" /> property's name.
        /// </summary>
        public const string IsStateDependentPropertyName = "IsStateDependent";

        private bool _isStateDependent = false;

        /// <summary>
        /// Sets and gets the IsStateDependent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsStateDependent
        {
            get
            {
                return _isStateDependent;
            }

            set
            {
                if (_isStateDependent == value)
                {
                    return;
                }

                _isStateDependent = value;
                RaisePropertyChanged(IsStateDependentPropertyName);
            }
        }


        [NonSerialized]
        private RelayCommand _executeScriptCommand;

        /// <summary>
        /// Gets the ExecuteScriptCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand ExecuteScriptCommand
        {
            get
            {
                return _executeScriptCommand
                    ?? (_executeScriptCommand = new RelayCommand(
                    () =>
                    {
                        Enabled = false;
                        _pointer = 0;
                        _timer.Interval = TimeSpan.FromMilliseconds(0);
                        _timer.Start();
                    }));
            }
        }

        [NonSerialized]
        private RelayCommand _stopScriptCommand;

        /// <summary>
        /// Gets the StopScriptCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand StopScriptCommand
        {
            get
            {
                return _stopScriptCommand
                    ?? (_stopScriptCommand = new RelayCommand(
                    () =>
                    {
                        _timer.Stop();
                        Enabled = true;
                    }));
            }
        }


        private void UpdateActiveProperty()
        {
            if (!IsStateDependent) return;
            State.UpdateAsync();

        }

        public vMixControlButton()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            _pointer = 0;
            Enabled = true;
            RealUpdateActiveProperty();
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { new Classes.Hotkey() { Name = "Execute" }, new Classes.Hotkey() { Name = "Reset" } };
        }

        private string GetInputNumber(int input)
        {
            try
            {
                return State.Inputs[input].Number.ToString();
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        private string GetInputNumber(string key)
        {
            try
            {
                return State.Inputs.Where(x => x.Key == key).FirstOrDefault().Number.ToString();
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_pointer >= _commands.Count)
            {
                _timer.Stop();
                Enabled = true;
                //_waitBeforeUpdate++;
                //Active = !Active;
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    Thread.Sleep(_waitBeforeUpdate);
                    _waitBeforeUpdate = -1;
                    Dispatcher.Invoke(() => _internalState.UpdateAsync());

                });
                return;
            }
            _timer.Interval = TimeSpan.FromMilliseconds(0);
            var cmd = _commands[_pointer];
            if (cmd.Action.Native)
                switch (cmd.Action.Function)
                {
                    case "Timer":
                        _timer.Interval = TimeSpan.FromMilliseconds(cmd.Parameter);
                        break;
                    case "UpdateState":
                        if (State != null)
                            State.Update();
                        break;
                    case "GoTo":
                        _pointer = cmd.Parameter - 1;
                        break;
                    case "ExecLink":
                        Messenger.Default.Send<string>(cmd.StringParameter);
                        break;
                }
            else if (State != null)
            {
                var input = State.Inputs.Where(x => x.Key == cmd.InputKey).FirstOrDefault()?.Number;

                if (!cmd.Action.StateDirect)
                {

                    State.SendFunction(string.Format(cmd.Action.FormatString, cmd.InputKey, cmd.Parameter, cmd.StringParameter, cmd.Parameter - 1, input.HasValue ? input.Value : 0));
                }
                else
                {
                    var path = string.Format(cmd.Action.ActiveStatePath, cmd.InputKey, cmd.Parameter, cmd.StringParameter, cmd.Parameter - 1, input.HasValue ? input.Value : 0);
                    SetValueByPath(State, path, cmd.Action.StateValue == "Input" ? (object)cmd.InputKey : (cmd.Action.StateValue == "String" ? (object)cmd.StringParameter : (object)cmd.Parameter));
                }
                _waitBeforeUpdate = Math.Max(_internalState.Transitions[cmd.Action.TransitionNumber].Duration, _waitBeforeUpdate);
            }
            _pointer++;

        }

        public override void ExecuteHotkey(int index)
        {
            base.ExecuteHotkey(index);
            switch (index)
            {
                case 0:
                    ExecuteScriptCommand.Execute(null);
                    break;
                case 1:
                    StopScriptCommand.Execute(null);
                    break;
            }
        }

        public override UserControl[] GetPropertiesControls()
        {
            //!!!!!
            BoolControl boolctrl = new BoolControl() { Title = LocalizationManager.Get("State Dependent"), Value = IsStateDependent, Visibility = System.Windows.Visibility.Visible };
            ScriptControl control = GetPropertyControl<ScriptControl>();
            control.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            control.Commands.Clear();
            foreach (var item in Commands)
            {
                control.Commands.Add(new vMixControlButtonCommand() { Action = item.Action, Input = item.Input, InputKey = item.InputKey, Parameter = item.Parameter, StringParameter = item.StringParameter });
            }
            return base.GetPropertiesControls().Concat(new UserControl[] { boolctrl, control }).ToArray();
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);
        }

        public override void SetProperties(UserControl[] _controls)
        {
            Commands.Clear();
            foreach (var item in (_controls.OfType<ScriptControl>().First()).Commands)
                Commands.Add(new vMixControlButtonCommand() { Action = item.Action, Input = item.Input, InputKey = item.InputKey, Parameter = item.Parameter, StringParameter = item.StringParameter });

            IsStateDependent = _controls.OfType<BoolControl>().First().Value;

            RealUpdateActiveProperty(true, State);
            base.SetProperties(_controls);
        }

        public override void Dispose()
        {
            if (_internalState != null)
                _internalState.OnStateUpdated -= State_OnStateUpdated;
            _timer.Stop();
            base.Dispose();
        }
    }
}
