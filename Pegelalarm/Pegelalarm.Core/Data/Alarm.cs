﻿using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Data
{
    [JsonObject(MemberSerialization=MemberSerialization.OptIn)]
    public class Alarm : Screen
    {
        private bool canSetAlarm;

        public bool CanSetAlarm
        {
            get { return canSetAlarm; }
            set { canSetAlarm = value; NotifyOfPropertyChange(); }
        }

        private double alarmValue;

        [JsonProperty]
        public double AlarmValue
        {
            get { return alarmValue; }
            set { alarmValue = value; NotifyOfPropertyChange(); }
        }

        private bool hasAlarm;

        public bool HasAlarm
        {
            get { return hasAlarm; }
            set { hasAlarm = value; NotifyOfPropertyChange(); }
        }

        private MetricKind metricKind;

        [JsonProperty]
        public MetricKind MetricKind
        {
            get { return metricKind; }
            set {
                metricKind = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MetricKindString));
            }
        }

        public string MetricKindString => MetricKind == MetricKind.Height ? "cm" : "m³/s";

        private WaterKind waterKind;
        
        [JsonProperty]
        public WaterKind WaterKind
        {
            get { return waterKind; }
            set
            {
                waterKind = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(WaterKindString));
                NotifyOfPropertyChange(nameof(WaterKindStringPlain));
            }
        }

        public string WaterKindString => $"({WaterKindStringPlain})";
        public string WaterKindStringPlain => WaterKind == WaterKind.Highwater ? "Hochwasser" : "Niedrigwasser";

        [JsonProperty]
        public string StationId { get; set; }
    }
}
