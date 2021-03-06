﻿using Newtonsoft.Json;
using Pegelalarm.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pegelalarm.Core.Persistance
{
    public class GlobalSettings
    {

        private Dictionary<string, object> _defaultValues;

        private Mutex stationMutex = new Mutex();

        private static GlobalSettings _instance;

        public static GlobalSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalSettings();
                }
                return _instance;
            }
        }

        private const string LocationRangeString = "location";

        public LocationRange LocationRange
        {
            get
            {
                return Get<LocationRange>(LocationRangeString);
            }
            set
            {
                Save(LocationRangeString, value);
            }
        }

        private const string AlarmRangeNotificationsOnString = "alarm";

        public bool AlarmRangeNotificationsOn
        {
            get
            {
                return Get<bool>(AlarmRangeNotificationsOnString);
            }
            set
            {
                Save(AlarmRangeNotificationsOnString, value);
            }
        }

        private GlobalSettings()
        {
            SetDefaultValues();

            ApplyDefaultValues();
        }

        public object this[string name]
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values[name];
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[name] = value;
            }
        }

        /// <summary>
        /// Sets the default values for any settings
        /// </summary>
        private void SetDefaultValues()
        {
            _defaultValues = new Dictionary<string, object>();
            _defaultValues[LocationRangeString] = new LocationRange { Latitude = 0, Longitude = 0, AlarmRadius = 10, DisplayRadius = 30 };
            _defaultValues[LocationRangeString] = new LocationRange { Latitude = 0, Longitude = 0, AlarmRadius = 10, DisplayRadius = 30 };
            _defaultValues[AlarmRangeNotificationsOnString] = true;
        }

        #region Storage Methods

        /// <summary>
        /// Stores the default value in the local settings
        /// </summary>
        /// <returns></returns>
        public void ApplyDefaultValues()
        {
            foreach (var item in _defaultValues)
            {
                if (this[item.Key] == null)
                {
                    Save(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Saves a setting in the local settings file of the app
        /// </summary>
        /// <param name="name">name of the setting</param>
        /// <param name="data">data to store</param>
        public void Save(string name, object data)
        {
            if (data == null) return;
            var json = JsonConvert.SerializeObject(data);
            this[name] = json;
        }

        /// <summary>
        /// Gets a setting form the local settings file of the app
        /// </summary>
        /// <typeparam name="T">Type if the settings</typeparam>
        /// <param name="name">Name of the setting</param>
        /// <returns>The value of the setting</returns>
        public T Get<T>(string name)
        {
            var json = (string)this[name];
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Saves settings that are to big for the settings file to an own file - async
        /// Currently used for the hydration file
        /// </summary>
        /// <param name="name">Name of the setting/file</param>
        /// <param name="data">The data to store</param>
        /// <returns></returns>
        public async Task SaveToFile(string name, object data)
        {
            if (data == null) return;
            var json = JsonConvert.SerializeObject(data);

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            var s = await file.OpenAsync(FileAccessMode.ReadWrite);
            var writer = new StreamWriter(s.AsStreamForWrite());
            writer.Write(json);
            await writer.FlushAsync();
            writer.Dispose();
        }


        /// <summary>
        /// Gets the content of the file as an object of the given type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">name of the file/setting</param>
        /// <returns>object value</returns>
        public async Task<T> GetFromFile<T>(string name)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            var s = await file.OpenAsync(FileAccessMode.Read);
            var reader = new StreamReader(s.AsStreamForRead());
            var json = reader.ReadToEnd();
            reader.Dispose();
            return JsonConvert.DeserializeObject<T>(json);
        }

        #endregion

        #region Setting Properties

        public async Task<List<Alarm>> GetMonitoredStations()
        {
            stationMutex.WaitOne();
            var st = await GetFromFile<List<Alarm>>("m_stations");
            var a = st ?? new List<Alarm>();
            stationMutex.ReleaseMutex();
            return a;
        }

        public async Task SaveMonitoredStations(List<Alarm> stationIds)
        {
            stationMutex.WaitOne();
            await SaveToFile("m_stations", stationIds);
            stationMutex.ReleaseMutex();
        }
        
        #endregion


    }
}
